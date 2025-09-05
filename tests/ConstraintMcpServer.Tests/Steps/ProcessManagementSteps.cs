using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Tests.Steps;

/// <summary>
/// Level 3 Refactoring: Extract process management logic.
/// Focused responsibility for server process lifecycle, cleanup, and timeout handling.
/// Follows CUPID properties: Composable, Unix Philosophy, Predictable, Idiomatic, Domain-based.
/// </summary>
internal sealed class ProcessManagementSteps : IDisposable
{
    private Process? _serverProcess;
    private StreamWriter? _serverInput;
    private StreamReader? _serverOutput;
    private StreamReader? _serverError;
    private string? _lastErrorOutput;

    // Process management constants (extracted from main class)
    private const int PROCESS_KILL_TIMEOUT_MS = 2000;
    private const int PROCESS_CLEANUP_TIMEOUT_MS = 5000;
    private static readonly TimeSpan DefaultTestTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Business-focused step: Start server process if not already running
    /// </summary>
    public void StartServerIfNeeded()
    {
        if (_serverProcess?.HasExited != false)
        {
            StartServerProcess();
        }
    }

    /// <summary>
    /// Business-focused step: Start server process with configuration
    /// </summary>
    public void StartServerWithConfiguration(string? configPath = null)
    {
        StartServerProcess(configPath);
    }

    /// <summary>
    /// Business-focused step: Gracefully stop the server process
    /// </summary>
    public void StopServer()
    {
        try
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                _serverInput?.Close();
                if (!_serverProcess.WaitForExit(PROCESS_KILL_TIMEOUT_MS))
                {
                    _serverProcess.Kill();
                    _serverProcess.WaitForExit(PROCESS_CLEANUP_TIMEOUT_MS);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Server shutdown failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Business-focused step: Force cleanup of hanging server process
    /// </summary>
    public void ForceProcessCleanup()
    {
        try
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                _serverInput?.Close();
                if (!_serverProcess.WaitForExit(PROCESS_KILL_TIMEOUT_MS))
                {
                    _serverProcess.Kill();
                    _serverProcess.WaitForExit(PROCESS_CLEANUP_TIMEOUT_MS);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Emergency process cleanup failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Business-focused step: Verify clean session termination
    /// </summary>
    public void VerifyCleanSessionTermination()
    {
        if (_serverProcess?.HasExited != true)
        {
            throw new InvalidOperationException("Server process should have terminated cleanly");
        }

        // Check if termination was clean (exit code 0)
        if (_serverProcess.ExitCode != 0)
        {
            string errorDetails = _lastErrorOutput ?? "No error details available";
            throw new InvalidOperationException($"Server terminated with non-zero exit code: {_serverProcess.ExitCode}. Error: {errorDetails}");
        }
    }

    /// <summary>
    /// Business-focused step: Verify predictable process behavior
    /// </summary>
    public void VerifyPredictableBehavior()
    {
        if (_serverProcess == null)
        {
            throw new InvalidOperationException("No server process to verify");
        }

        // Server should be responsive and not consuming excessive resources
        if (_serverProcess.HasExited)
        {
            throw new InvalidOperationException("Server process exited unexpectedly");
        }

        // Basic health check - server should be running
        if (_serverInput == null || _serverOutput == null)
        {
            throw new InvalidOperationException("Server I/O streams not properly initialized");
        }
    }

    /// <summary>
    /// Get server input stream for JSON-RPC communication
    /// </summary>
    public StreamWriter? GetServerInput() => _serverInput;

    /// <summary>
    /// Get server output stream for reading responses
    /// </summary>
    public StreamReader? GetServerOutput() => _serverOutput;

    /// <summary>
    /// Get server error stream for diagnostics
    /// </summary>
    public StreamReader? GetServerError() => _serverError;

    /// <summary>
    /// Get last error output for diagnostics
    /// </summary>
    public string? GetLastErrorOutput() => _lastErrorOutput;

    /// <summary>
    /// Check if server process is running
    /// </summary>
    public bool IsServerRunning() => _serverProcess?.HasExited == false;

    /// <summary>
    /// Wrapper method to ensure all async operations have a timeout
    /// </summary>
    public async Task<T> WithTimeoutAsync<T>(Task<T> task, TimeSpan? timeout = null)
    {
        timeout ??= DefaultTestTimeout;

        using var cts = new CancellationTokenSource(timeout.Value);
        var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);

        var completedTask = await Task.WhenAny(task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            // Force cleanup of hanging server process
            ForceProcessCleanup();
            throw new TimeoutException($"Operation timed out after {timeout.Value.TotalSeconds} seconds");
        }

        return await task;
    }

    private void StartServerProcess(string? configPath = null)
    {
        // Clean up any existing process first
        StopServer();

        string projectRoot = GetProjectRoot();
        string serverExecutable = Path.Combine(projectRoot,
            "src", "ConstraintMcpServer", "bin", "Release", "net8.0", "ConstraintMcpServer.dll");

        if (!File.Exists(serverExecutable))
        {
            throw new InvalidOperationException($"Server executable not found: {serverExecutable}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = configPath != null ? $"\"{serverExecutable}\" --config \"{configPath}\"" : $"\"{serverExecutable}\"",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = projectRoot
        };

        _serverProcess = Process.Start(startInfo);
        if (_serverProcess == null)
        {
            throw new InvalidOperationException("Failed to start server process");
        }

        _serverInput = _serverProcess.StandardInput;
        _serverOutput = _serverProcess.StandardOutput;
        _serverError = _serverProcess.StandardError;

        // Give server a moment to initialize
        Thread.Sleep(500);

        // Check if process started successfully
        if (_serverProcess.HasExited)
        {
            _lastErrorOutput = _serverError?.ReadToEnd();
            throw new InvalidOperationException($"Server process exited immediately. Error: {_lastErrorOutput}");
        }
    }

    private static string GetProjectRoot()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(currentDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "mcp-llm-constraints.sln")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find project root directory");
    }

    public void Dispose()
    {
        StopServer();
        _serverInput?.Dispose();
        _serverOutput?.Dispose();
        _serverError?.Dispose();
        _serverProcess?.Dispose();
    }
}
