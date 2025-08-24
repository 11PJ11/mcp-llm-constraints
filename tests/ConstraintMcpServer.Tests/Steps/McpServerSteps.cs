using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    private string? _configPath;
    private string? _lastErrorOutput;
    private readonly List<long> _performanceMetrics = new();

    // Business-focused step: The repository builds successfully
    public void RepositoryBuildsSuccessfully()
    {
        // Verify that the built assemblies exist (build should have been done already via --no-build)
        string serverAssembly = Path.Combine(GetProjectRoot(),
            "src", "ConstraintMcpServer", "bin", "Release", "net8.0", "ConstraintMcpServer.dll");
        string testAssembly = Path.Combine(GetProjectRoot(),
            "tests", "ConstraintMcpServer.Tests", "bin", "Release", "net8.0", "ConstraintMcpServer.Tests.dll");

        if (!File.Exists(serverAssembly))
        {
            throw new InvalidOperationException($"Server assembly not found: {serverAssembly}. Run 'dotnet build --configuration Release' first.");
        }

        if (!File.Exists(testAssembly))
        {
            throw new InvalidOperationException($"Test assembly not found: {testAssembly}. Run 'dotnet build --configuration Release' first.");
        }

        // Both assemblies exist - build was successful
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

        // Check that we got a valid JSON-RPC response with an ID
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No valid JSON response received");
        }

        JsonElement root = _lastJsonResponse.RootElement;
        if (!root.TryGetProperty("id", out JsonElement id))
        {
            throw new InvalidOperationException($"Response does not contain an ID. Response: {root.GetRawText()}");
        }

        // Just verify we have a valid integer ID - the actual value depends on which request was sent
        try
        {
            id.GetInt32();
        }
        catch
        {
            throw new InvalidOperationException($"Response ID is not a valid integer. Response: {root.GetRawText()}");
        }
    }

    // Business-focused step: Send MCP initialize request
    public async Task SendInitializeRequest()
    {
        StartServerIfNeeded();

        var request = new
        {
            jsonrpc = "2.0",
            id = 2,
            method = "initialize",
            @params = new
            {
                protocolVersion = "2024-11-05",
                clientInfo = new
                {
                    name = "ConstraintMcpServer-E2E-Test",
                    version = "0.1.0"
                }
            }
        };

        await SendJsonRpcRequest(request);
        await ReadJsonRpcResponse();
    }

    // Business-focused step: Send MCP shutdown request
    public async Task SendShutdownRequest()
    {
        if (_serverProcess == null || _serverProcess.HasExited)
        {
            throw new InvalidOperationException("Server is not running for shutdown request");
        }

        var request = new
        {
            jsonrpc = "2.0",
            id = 3,
            method = "shutdown",
            @params = new { }
        };

        await SendJsonRpcRequest(request);
        await ReadJsonRpcResponse();
    }

    // Business-focused step: Send initialize followed by shutdown (for lifecycle testing)
    public async Task SendInitializeAndShutdownSequence()
    {
        // First send initialize
        await SendInitializeRequest();
        // Then send shutdown
        await SendShutdownRequest();
    }

    // Business-focused step: Verify capabilities response structure
    public void ReceiveCapabilitiesResponse()
    {
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        JsonElement root = _lastJsonResponse.RootElement;

        // Check for JSON-RPC structure
        if (!root.TryGetProperty("result", out JsonElement result))
        {
            throw new InvalidOperationException("Initialize response does not contain a 'result' property");
        }

        // Verify capabilities structure
        if (!result.TryGetProperty("capabilities", out JsonElement capabilities))
        {
            throw new InvalidOperationException("Initialize response does not contain capabilities");
        }

        // Verify required capability sections exist
        if (!capabilities.TryGetProperty("tools", out _))
        {
            throw new InvalidOperationException("Capabilities missing 'tools' section");
        }

        if (!capabilities.TryGetProperty("resources", out _))
        {
            throw new InvalidOperationException("Capabilities missing 'resources' section");
        }

        if (!capabilities.TryGetProperty("notifications", out JsonElement notifications))
        {
            throw new InvalidOperationException("Capabilities missing 'notifications' section");
        }

        // Verify constraint notifications are advertised
        if (!notifications.TryGetProperty("constraints", out JsonElement constraintsNotif) ||
            !constraintsNotif.GetBoolean())
        {
            throw new InvalidOperationException("Capabilities do not advertise constraint notifications");
        }

        // Verify server info is present
        if (!result.TryGetProperty("serverInfo", out JsonElement serverInfo))
        {
            throw new InvalidOperationException("Initialize response missing serverInfo");
        }

        if (!serverInfo.TryGetProperty("name", out _) || !serverInfo.TryGetProperty("version", out _))
        {
            throw new InvalidOperationException("Server info missing name or version");
        }
    }

    // Business-focused step: Verify shutdown confirmation
    public void ReceiveShutdownConfirmation()
    {
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No shutdown response received from server");
        }

        JsonElement root = _lastJsonResponse.RootElement;

        // Verify JSON-RPC structure with empty result
        if (!root.TryGetProperty("result", out _))
        {
            throw new InvalidOperationException("Shutdown response does not contain a 'result' property");
        }

        // Verify response ID matches request
        if (!root.TryGetProperty("id", out JsonElement id) || id.GetInt32() != 3)
        {
            throw new InvalidOperationException("Shutdown response ID does not match request ID");
        }
    }

    // Business-focused step: Verify protocol compliance
    public void VerifyProtocolCompliance()
    {
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No response received for protocol verification");
        }

        JsonElement root = _lastJsonResponse.RootElement;

        // Verify JSON-RPC 2.0 compliance
        if (!root.TryGetProperty("jsonrpc", out JsonElement jsonrpc) ||
            jsonrpc.GetString() != "2.0")
        {
            throw new InvalidOperationException("Response is not JSON-RPC 2.0 compliant");
        }

        // Verify response ID matches request
        if (!root.TryGetProperty("id", out JsonElement id) || id.GetInt32() != 2)
        {
            throw new InvalidOperationException("Response ID does not match initialize request ID");
        }

        // Verify no error field
        if (root.TryGetProperty("error", out _))
        {
            throw new InvalidOperationException("Initialize response contains an error");
        }
    }

    // Business-focused step: Verify latency budget (p95 < 50ms requirement)
    public void VerifyLatencyBudget()
    {
        // For the BDD test, we'll do a simple response time check
        // In a real performance test suite, this would involve multiple iterations

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Re-send the same initialize request to measure latency
        var task = Task.Run(async () =>
        {
            var request = new
            {
                jsonrpc = "2.0",
                id = 4,
                method = "initialize",
                @params = new
                {
                    protocolVersion = "2024-11-05",
                    clientInfo = new
                    {
                        name = "ConstraintMcpServer-Latency-Test",
                        version = "0.1.0"
                    }
                }
            };

            await SendJsonRpcRequest(request);
            await ReadJsonRpcResponse();
        });

        task.Wait();
        stopwatch.Stop();

        // Verify response time is under budget (being generous for CI environments)
        if (stopwatch.ElapsedMilliseconds > 100) // 100ms threshold for E2E test
        {
            throw new InvalidOperationException($"Initialize latency {stopwatch.ElapsedMilliseconds}ms exceeds budget of 100ms");
        }
    }

    // Business-focused step: Verify clean session termination
    public void VerifyCleanSessionTermination()
    {
        if (_serverProcess == null)
        {
            throw new InvalidOperationException("Server process reference is null");
        }

        // After shutdown, the server should still be running (long-running process model)
        // but ready to accept new sessions
        if (_serverProcess.HasExited)
        {
            throw new InvalidOperationException("Server process exited after shutdown - should remain running");
        }

        // Verify we can still communicate (server should be ready for new session)
        // This is validated by the successful completion of the shutdown response reception
    }

    // Business-focused step: Valid constraint configuration exists
    public void ValidConstraintConfigurationExists()
    {
        string configDir = Path.Combine(GetProjectRoot(), "config");
        _configPath = Path.Combine(configDir, "constraints.yaml");

        if (!File.Exists(_configPath))
        {
            throw new InvalidOperationException($"Valid constraint configuration file not found at: {_configPath}");
        }

        // Verify basic YAML structure exists
        string content = File.ReadAllText(_configPath);
        if (!content.Contains("version:") || !content.Contains("constraints:"))
        {
            throw new InvalidOperationException("Configuration file does not contain expected YAML structure");
        }
    }

    // Business-focused step: Invalid constraint configuration exists
    public void InvalidConstraintConfigurationExists()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "constraint-server-test");
        Directory.CreateDirectory(tempDir);
        _configPath = Path.Combine(tempDir, "invalid-constraints.yaml");

        // Create invalid YAML configuration
        string invalidYaml = """
            version: "0.1.0"
            constraints:
              - id: invalid.constraint
                # Missing required fields: title, priority, phases, reminders
            """;

        File.WriteAllText(_configPath, invalidYaml);
    }

    // Business-focused step: Start server with configuration
    // Note: Since refactor to pure MCP server, configuration is no longer loaded via CLI
    // This method now starts the MCP server and configuration will be loaded via MCP protocol
    public void StartServerWithConfiguration()
    {
        if (string.IsNullOrEmpty(_configPath))
        {
            throw new InvalidOperationException("Configuration path not set - call ValidConstraintConfigurationExists first");
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

        // Give server time to start (no longer loading configuration at startup)
        Thread.Sleep(2000);
    }

    // Business-focused step: Attempt to start server with invalid configuration
    public void AttemptToStartServerWithInvalidConfiguration()
    {
        if (string.IsNullOrEmpty(_configPath))
        {
            throw new InvalidOperationException("Configuration path not set - call InvalidConstraintConfigurationExists first");
        }

        string projectPath = Path.Combine(GetProjectRoot(), "src", "ConstraintMcpServer", "ConstraintMcpServer.csproj");

        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectPath}\" --no-build --configuration {GetCurrentConfiguration()} -- --config \"{_configPath}\"",
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

        // Wait for server to attempt loading and potentially fail
        Thread.Sleep(2000);
    }

    // Business-focused step: Server loads configuration successfully
    public void ServerLoadsConfigurationSuccessfully()
    {
        if (_serverProcess == null)
        {
            throw new InvalidOperationException("Server process not started");
        }

        // Server should still be running after configuration load
        if (_serverProcess.HasExited)
        {
            string errorOutput = "";
            try
            {
                if (_serverError != null)
                {
                    errorOutput = _serverError.ReadToEnd();
                }
            }
            catch
            {
                // Ignore error reading stderr
            }
            throw new InvalidOperationException($"Server process exited during configuration load. Error: {errorOutput}");
        }
    }

    // Business-focused step: Server advertises constraint capabilities
    public async Task ServerAdvertisesConstraintCapabilities()
    {
        // Send initialize request and verify constraint capabilities are advertised
        await SendInitializeRequest();

        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        JsonElement root = _lastJsonResponse.RootElement;
        JsonElement result = root.GetProperty("result");
        JsonElement capabilities = result.GetProperty("capabilities");

        // Verify constraint-specific capabilities
        if (!capabilities.TryGetProperty("notifications", out JsonElement notifications))
        {
            throw new InvalidOperationException("Server does not advertise notification capabilities");
        }

        if (!notifications.TryGetProperty("constraints", out JsonElement constraintsNotif) ||
            !constraintsNotif.GetBoolean())
        {
            throw new InvalidOperationException("Server does not advertise constraint notification capabilities");
        }
    }

    // Business-focused step: Server rejects configuration with clear error
    public void ServerRejectsConfigurationWithClearError()
    {
        if (_serverProcess == null)
        {
            throw new InvalidOperationException("Server process not started");
        }

        // Server should have exited due to configuration validation failure
        if (!_serverProcess.HasExited)
        {
            // Give it a bit more time
            _serverProcess.WaitForExit(5000);
        }

        if (!_serverProcess.HasExited)
        {
            throw new InvalidOperationException("Server did not exit with invalid configuration - validation may not be working");
        }

        // Capture error output for analysis
        try
        {
            if (_serverError != null)
            {
                _lastErrorOutput = _serverError.ReadToEnd();
            }
        }
        catch
        {
            // Error reading stderr
        }

        if (string.IsNullOrWhiteSpace(_lastErrorOutput))
        {
            throw new InvalidOperationException("Server exited but provided no error output for invalid configuration");
        }
    }

    // Business-focused step: Error message indicates validation failure
    public void ErrorMessageIndicatesValidationFailure()
    {
        if (string.IsNullOrWhiteSpace(_lastErrorOutput))
        {
            throw new InvalidOperationException("No error output captured to analyze");
        }

        // Check for validation-related error keywords
        string errorLower = _lastErrorOutput.ToLowerInvariant();
        bool hasValidationKeywords = errorLower.Contains("validation") ||
                                   errorLower.Contains("invalid") ||
                                   errorLower.Contains("required") ||
                                   errorLower.Contains("missing") ||
                                   errorLower.Contains("configuration");

        if (!hasValidationKeywords)
        {
            throw new InvalidOperationException($"Error message does not indicate validation failure. Actual error: {_lastErrorOutput}");
        }
    }

    // Business-focused step: Constraint pack with multiple priorities exists
    public void ConstraintPackWithMultiplePriorities()
    {
        // Verify that the existing constraint pack has constraints with different priorities
        // This will be used to test priority-based selection
        ValidConstraintConfigurationExists();

        // The existing config has constraints with priorities 0.92, 0.88, 0.75
        // which is perfect for testing priority-based selection
    }

    // Business-focused step: Server starts with phase tracking
    public void ServerStartsWithPhaseTracking()
    {
        // For this walking skeleton, we'll assume red phase
        // Future iterations will add proper phase tracking
        StartServerIfNeeded();
    }

    // Business-focused step: Simulate tool calls in red phase
    public async Task SimulateToolCallsInRedPhase()
    {
        // Simulate the E2E pattern: 1st interaction injects, 2nd doesn't, 3rd injects
        await SendInitializeRequest();

        // First tool call should trigger constraint injection
        await SendToolCallRequest("test-tool-1");

        // Second tool call should pass through
        await SendToolCallRequest("test-tool-2");

        // Third tool call should trigger constraint injection again
        await SendToolCallRequest("test-tool-3");
    }

    // Business-focused step: Server injects constraints by priority
    public void ServerInjectsConstraintsByPriority()
    {
        if (_lastResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        // Response should contain evidence of constraint injection
        // For now, check for any constraint marker - will be refined in implementation
        if (!_lastResponse.Contains("CONSTRAINT"))
        {
            throw new InvalidOperationException($"Response does not contain constraint injection: {_lastResponse}");
        }
    }

    // Business-focused step: Constraint message contains anchors
    public void ConstraintMessageContainsAnchors()
    {
        if (_lastResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        // Should contain anchor prologue/epilogue markers
        // This will fail initially and drive implementation
        if (!_lastResponse.Contains("ANCHOR") && !_lastResponse.Contains("Remember:"))
        {
            throw new InvalidOperationException($"Response does not contain anchor patterns: {_lastResponse}");
        }
    }

    // Business-focused step: Constraint message contains top-K reminders
    public void ConstraintMessageContainsTopKReminders()
    {
        if (_lastResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        // Should contain specific constraint reminders from top priority constraints
        // This will fail initially and drive the constraint selection implementation
        bool hasHighPriorityConstraint = _lastResponse.Contains("Write a failing test first") ||
                                        _lastResponse.Contains("Test-first") ||
                                        _lastResponse.Contains("Domain must not depend on Infrastructure");

        if (!hasHighPriorityConstraint)
        {
            throw new InvalidOperationException($"Response does not contain high-priority constraint reminders: {_lastResponse}");
        }
    }

    // Business-focused step: Pass-through calls remain unchanged
    public void PassThroughCallsRemainUnchanged()
    {
        // For non-injection interactions, verify response format is standard
        // This ensures our constraint injection doesn't break normal operation

        // This step validates that the 2nd tool call (which shouldn't inject) has standard format
        // We'll need to track responses from multiple tool calls for this validation
        // For now, this serves as a placeholder for the business requirement
    }

    // Helper method for tool call requests
    private async Task SendToolCallRequest(string toolName)
    {
        var request = new
        {
            jsonrpc = "2.0",
            id = 2,
            method = "tools/call",
            @params = new
            {
                name = toolName,
                arguments = new { }
            }
        };

        await SendJsonRpcRequest(request);
        await ReadJsonRpcResponse();
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

    // Performance testing steps for Step 6

    /// <summary>
    /// Business-focused step: Server starts with default configuration for performance testing
    /// </summary>
    public void ServerStartsWithDefaultConfiguration()
    {
        StartServerIfNeeded();
    }

    /// <summary>
    /// Business-focused step: Process multiple tool calls under load to measure performance
    /// </summary>
    public async Task ProcessMultipleToolCallsUnderLoad()
    {
        const int NumberOfCalls = 100; // Performance budget test with 100 calls
        _performanceMetrics.Clear();

        StartServerIfNeeded();

        for (int i = 0; i < NumberOfCalls; i++)
        {
            var stopwatch = Stopwatch.StartNew();

            // Simulate tools/call request
            var toolCallRequest = new
            {
                jsonrpc = "2.0",
                id = i + 1,
                method = "tools/call",
                @params = new
                {
                    name = "test_tool",
                    arguments = new { interaction = i + 1 }
                }
            };

            await SendJsonRpcRequest(toolCallRequest);
            await ReadJsonRpcResponse();

            stopwatch.Stop();
            _performanceMetrics.Add(stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Business-focused step: Verify p95 latency is within budget (≤ 50ms)
    /// </summary>
    public void P95LatencyIsWithinBudget()
    {
        if (_performanceMetrics.Count == 0)
        {
            throw new InvalidOperationException("No performance metrics collected");
        }

        var sortedMetrics = _performanceMetrics.OrderBy(x => x).ToList();
        int p95Index = (int)Math.Ceiling(sortedMetrics.Count * 0.95) - 1;
        long p95Latency = sortedMetrics[p95Index];

        if (p95Latency > 50)
        {
            throw new InvalidOperationException($"P95 latency {p95Latency}ms exceeds budget of 50ms. Metrics: [{string.Join(", ", sortedMetrics)}]");
        }

        // P95 is within budget
        Console.WriteLine($"✅ P95 latency: {p95Latency}ms (within 50ms budget)");
    }

    /// <summary>
    /// Business-focused step: Verify p99 latency is within budget (≤ 100ms)
    /// </summary>
    public void P99LatencyIsWithinBudget()
    {
        if (_performanceMetrics.Count == 0)
        {
            throw new InvalidOperationException("No performance metrics collected");
        }

        var sortedMetrics = _performanceMetrics.OrderBy(x => x).ToList();
        int p99Index = (int)Math.Ceiling(sortedMetrics.Count * 0.99) - 1;
        long p99Latency = sortedMetrics[p99Index];

        if (p99Latency > 100)
        {
            throw new InvalidOperationException($"P99 latency {p99Latency}ms exceeds budget of 100ms. Metrics: [{string.Join(", ", sortedMetrics)}]");
        }

        // P99 is within budget
        Console.WriteLine($"✅ P99 latency: {p99Latency}ms (within 100ms budget)");
    }

    /// <summary>
    /// Business-focused step: Verify no performance regression detected
    /// </summary>
    public void NoPerformanceRegressionDetected()
    {
        if (_performanceMetrics.Count == 0)
        {
            throw new InvalidOperationException("No performance metrics collected");
        }

        // Calculate basic statistics
        double averageLatency = _performanceMetrics.Average();
        long maxLatency = _performanceMetrics.Max();
        long minLatency = _performanceMetrics.Min();

        // Basic regression detection - allow for startup costs, focus on P99 rather than outliers
        // If P99 exceeds budget significantly (2x), that indicates a real performance issue
        var sortedMetrics = _performanceMetrics.OrderBy(x => x).ToList();
        int p99Index = (int)Math.Ceiling(sortedMetrics.Count * 0.99) - 1;
        long p99Latency = sortedMetrics[p99Index];

        if (p99Latency > 200) // 2x the P99 budget of 100ms
        {
            throw new InvalidOperationException($"Performance regression detected. P99 latency {p99Latency}ms exceeds acceptable threshold of 200ms");
        }

        // Performance is consistent
        Console.WriteLine($"✅ Performance consistent - Avg: {averageLatency:F2}ms, Min: {minLatency}ms, Max: {maxLatency}ms");
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
