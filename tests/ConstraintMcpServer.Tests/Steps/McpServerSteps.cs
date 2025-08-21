using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Tests.Steps;

/// <summary>
/// Business-focused steps for MCP server interactions.
/// Encapsulates implementation details away from test scenarios.
/// </summary>
public class McpServerSteps : IDisposable
{
    private Process? _serverProcess;
    private StreamWriter? _serverInput;
    private StreamReader? _serverOutput;
    private StreamReader? _serverError;
    private string? _lastResponse;
    private JsonDocument? _lastJsonResponse;

    // Business-focused step: The repository builds successfully
    public void RepositoryBuildsSuccessfully()
    {
        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "build --configuration Release",
                WorkingDirectory = GetProjectRoot(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        buildProcess.Start();
        buildProcess.WaitForExit(30000); // 30 second timeout

        if (buildProcess.ExitCode != 0)
        {
            string error = buildProcess.StandardError.ReadToEnd();
            throw new InvalidOperationException($"Build failed: {error}");
        }
    }

    // Business-focused step: Request help from the server
    public async Task RequestHelpFromServer()
    {
        StartServerIfNeeded();

        var request = new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "server.help",
            @params = new
            {
                clientInfo = new
                {
                    name = "test-client",
                    version = "0.1.0"
                }
            }
        };

        await SendJsonRpcRequest(request);
        await ReadJsonRpcResponse();
    }

    // Business-focused step: Receive product description
    public void ReceiveConciseProductDescription()
    {
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        JsonElement root = _lastJsonResponse.RootElement;

        // Check for JSON-RPC structure
        if (!root.TryGetProperty("result", out JsonElement result))
        {
            throw new InvalidOperationException("Response does not contain a 'result' property");
        }

        // Verify product information is present
        if (!result.TryGetProperty("product", out JsonElement product))
        {
            throw new InvalidOperationException("Response does not contain product information");
        }

        string? productName = product.GetString();
        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new InvalidOperationException("Product name is missing or empty");
        }

        // Verify description is present and meaningful
        if (!result.TryGetProperty("description", out JsonElement description))
        {
            throw new InvalidOperationException("Response does not contain a description");
        }

        string? descriptionText = description.GetString();
        if (string.IsNullOrWhiteSpace(descriptionText) || descriptionText.Length < 20)
        {
            throw new InvalidOperationException("Description is missing or too brief");
        }
    }

    // Business-focused step: Receive main commands
    public void ReceiveMainCommands()
    {
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        JsonElement root = _lastJsonResponse.RootElement;
        JsonElement result = root.GetProperty("result");

        // Verify commands are present
        if (!result.TryGetProperty("commands", out JsonElement commands))
        {
            throw new InvalidOperationException("Response does not contain available commands");
        }

        // Check that it's an array with at least the essential commands
        if (commands.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Commands should be provided as an array");
        }

        int commandCount = commands.GetArrayLength();
        if (commandCount < 3) // Expecting at least: server.help, initialize, shutdown
        {
            throw new InvalidOperationException($"Expected at least 3 commands, but got {commandCount}");
        }

        // Verify essential commands are present
        bool hasHelp = false;
        bool hasInitialize = false;
        bool hasShutdown = false;

        foreach (JsonElement cmd in commands.EnumerateArray())
        {
            string? cmdName = cmd.GetString();
            if (cmdName == "server.help")
            {
                hasHelp = true;
            }

            if (cmdName == "initialize")
            {
                hasInitialize = true;
            }

            if (cmdName == "shutdown")
            {
                hasShutdown = true;
            }
        }

        if (!hasHelp || !hasInitialize || !hasShutdown)
        {
            throw new InvalidOperationException("Essential commands (server.help, initialize, shutdown) are missing");
        }
    }

    // Business-focused step: Process behaves predictably
    public void ProcessBehavesPredictably()
    {
        if (_serverProcess == null)
        {
            throw new InvalidOperationException("Server process is not running");
        }

        // The server should still be running (long-running process model)
        if (_serverProcess.HasExited)
        {
            throw new InvalidOperationException("Server process exited unexpectedly");
        }

        // Check that we got a valid JSON-RPC response with the same ID we sent
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No valid JSON response received");
        }

        JsonElement root = _lastJsonResponse.RootElement;
        if (!root.TryGetProperty("id", out JsonElement id) || id.GetInt32() != 1)
        {
            throw new InvalidOperationException("Response ID does not match request ID");
        }
    }

    // Helper methods
    private void StartServerIfNeeded()
    {
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            return;
        }

        string projectPath = Path.Combine(GetProjectRoot(), "src", "ConstraintMcpServer", "ConstraintMcpServer.csproj");

        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectPath}\" --no-build --configuration {GetCurrentConfiguration()}",
                WorkingDirectory = GetProjectRoot(),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _serverProcess.Start();
        _serverInput = _serverProcess.StandardInput;
        _serverOutput = _serverProcess.StandardOutput;
        _serverError = _serverProcess.StandardError;

        // Give server time to start - increased for Release mode reliability
        Thread.Sleep(1000);
    }

    private async Task SendJsonRpcRequest(object request)
    {
        if (_serverInput == null)
        {
            throw new InvalidOperationException("Server input stream is not available");
        }

        if (_serverProcess?.HasExited == true)
        {
            string errorOutput = "";
            try
            {
                if (_serverError != null)
                {
                    errorOutput = await _serverError.ReadToEndAsync();
                }
            }
            catch
            {
                // Ignore error reading stderr
            }
            throw new InvalidOperationException($"Server process has exited with code {_serverProcess.ExitCode}. Error output: {errorOutput}");
        }

        string json = JsonSerializer.Serialize(request);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        // MCP uses HTTP-style headers for framing
        await _serverInput.WriteLineAsync($"Content-Length: {bytes.Length}");
        await _serverInput.WriteLineAsync();
        await _serverInput.WriteAsync(json);
        await _serverInput.FlushAsync();
    }

    private async Task ReadJsonRpcResponse()
    {
        if (_serverOutput == null)
        {
            throw new InvalidOperationException("Server output stream is not available");
        }

        // Read Content-Length header
        string? headerLine = await _serverOutput.ReadLineAsync();
        if (headerLine == null || !headerLine.StartsWith("Content-Length:"))
        {
            throw new InvalidOperationException($"Expected Content-Length header, got: {headerLine}");
        }

        string lengthStr = headerLine.Substring("Content-Length:".Length).Trim();
        if (!int.TryParse(lengthStr, out int contentLength))
        {
            throw new InvalidOperationException($"Invalid Content-Length: {lengthStr}");
        }

        // Read blank line
        await _serverOutput.ReadLineAsync();

        // Read JSON content
        char[] buffer = new char[contentLength];
        int totalRead = 0;
        while (totalRead < contentLength)
        {
            int read = await _serverOutput.ReadAsync(buffer, totalRead, contentLength - totalRead);
            if (read == 0)
            {
                throw new InvalidOperationException("Unexpected end of stream");
            }

            totalRead += read;
        }

        _lastResponse = new string(buffer);
        _lastJsonResponse = JsonDocument.Parse(_lastResponse);
    }

    private string GetProjectRoot()
    {
        string? currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null && !File.Exists(Path.Combine(currentDir, "ConstraintMcpServer.sln")))
        {
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        if (currentDir == null)
        {
            throw new InvalidOperationException("Could not find project root");
        }

        return currentDir;
    }

    private static string GetCurrentConfiguration()
    {
        // Detect if we're running in Debug or Release mode by checking the assembly location
#if DEBUG
        return "Debug";
#else
        return "Release";
#endif
    }

    public void Dispose()
    {
        _lastJsonResponse?.Dispose();
        _serverInput?.Dispose();
        _serverOutput?.Dispose();
        _serverError?.Dispose();

        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            _serverProcess.Kill();
            _serverProcess.WaitForExit(5000);
            _serverProcess.Dispose();
        }
    }
}
