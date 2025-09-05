using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Tests.Infrastructure;

/// <summary>
/// Production infrastructure test environment with zero test doubles.
/// Business value: Authentic validation using real GitHub API, file system, and environment.
/// Provides safe sandbox for production infrastructure testing with automatic cleanup.
/// </summary>
public sealed class ProductionInfrastructureTestEnvironment : IDisposable
{
    private readonly string _testInstallationRoot;
    private readonly Dictionary<string, string?> _originalEnvironmentVariables;
    private readonly List<string> _createdDirectories;
    private readonly List<string> _modifiedFiles;
    private readonly HttpClient _gitHubClient;
    private bool _disposed;

    /// <summary>
    /// Test installation directory using real file system.
    /// </summary>
    public string TestInstallationRoot => _testInstallationRoot;

    /// <summary>
    /// Real GitHub API client for production testing.
    /// </summary>
    public HttpClient GitHubClient => _gitHubClient;

    public ProductionInfrastructureTestEnvironment()
    {
        // Create isolated test environment using real infrastructure
        _testInstallationRoot = Path.Combine(
            Path.GetTempPath(),
            $"constraint-server-e2e-{Guid.NewGuid():N}");

        _originalEnvironmentVariables = new Dictionary<string, string?>();
        _createdDirectories = new List<string>();
        _modifiedFiles = new List<string>();

        // Real GitHub API client - no mocking
        _gitHubClient = new HttpClient();
        _gitHubClient.DefaultRequestHeaders.Add("User-Agent",
            "ConstraintMcpServer-E2E-Tests/1.0");

        _disposed = false;
    }

    /// <summary>
    /// Validates real network connectivity to GitHub API.
    /// Business value: Ensures tests can download actual releases.
    /// </summary>
    public async Task<bool> ValidateNetworkConnectivity()
    {
        try
        {
            var response = await _gitHubClient.GetAsync("https://api.github.com");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates real installation directory on file system.
    /// Business value: Tests actual directory creation process users will experience.
    /// </summary>
    public async Task<bool> CreateRealInstallationDirectory()
    {
        try
        {
            Directory.CreateDirectory(_testInstallationRoot);
            Directory.CreateDirectory(Path.Combine(_testInstallationRoot, "bin"));
            Directory.CreateDirectory(Path.Combine(_testInstallationRoot, "config"));

            _createdDirectories.Add(_testInstallationRoot);
            return await Task.FromResult(true);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Modifies real environment PATH variable for testing.
    /// Business value: Tests actual PATH modification users will experience.
    /// </summary>
    public bool ModifyRealEnvironmentPath()
    {
        try
        {
            // Backup original PATH
            var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            _originalEnvironmentVariables["PATH"] = currentPath;

            // Add test installation to PATH
            var newPath = $"{_testInstallationRoot}{Path.PathSeparator}{currentPath}";
            Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Process);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Downloads real binary from GitHub releases.
    /// Business value: Tests actual download process and network performance.
    /// </summary>
    public async Task<bool> DownloadRealBinaryFromGitHub(string url, string destinationPath)
    {
        try
        {
            using var response = await _gitHubClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(destinationPath, content);

            _modifiedFiles.Add(destinationPath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Executes real process for binary testing.
    /// Business value: Validates actual binary execution and functionality.
    /// </summary>
    public async Task<ProcessResult> ExecuteRealProcess(string fileName, string arguments = "")
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName, arguments)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = _testInstallationRoot
                }
            };

            var stopwatch = Stopwatch.StartNew();
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();
            stopwatch.Stop();

            var output = await outputTask;
            var error = await errorTask;

            return new ProcessResult(
                process.ExitCode,
                output,
                error,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            return new ProcessResult(-1, "", ex.Message, 0);
        }
    }

    /// <summary>
    /// Validates real file system state after operations.
    /// Business value: Confirms actual file and directory creation.
    /// </summary>
    public bool ValidateRealFileSystemState()
    {
        try
        {
            // Validate installation directory exists
            if (!Directory.Exists(_testInstallationRoot))
            {
                return false;
            }

            // Validate subdirectories exist
            var binDir = Path.Combine(_testInstallationRoot, "bin");
            var configDir = Path.Combine(_testInstallationRoot, "config");

            return Directory.Exists(binDir) && Directory.Exists(configDir);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates real environment PATH modification.
    /// Business value: Confirms actual PATH configuration for users.
    /// </summary>
    public bool ValidateRealEnvironmentPath()
    {
        try
        {
            var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            return currentPath?.Contains(_testInstallationRoot) == true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            // Restore original environment variables
            foreach (var kvp in _originalEnvironmentVariables)
            {
                Environment.SetEnvironmentVariable(kvp.Key, kvp.Value, EnvironmentVariableTarget.Process);
            }

            // Clean up created directories
            foreach (var directory in _createdDirectories)
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }

            // Clean up modified files
            foreach (var file in _modifiedFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }

            _gitHubClient?.Dispose();
        }
        catch (Exception ex)
        {
            // Log cleanup errors but don't throw
            System.Console.WriteLine($"Warning: Test cleanup encountered error: {ex.Message}");
        }
        finally
        {
            _disposed = true;
        }
    }
}

/// <summary>
/// Result of real process execution.
/// Business value: Provides authentic process execution metrics and output.
/// </summary>
public sealed record ProcessResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    long ElapsedMilliseconds)
{
    /// <summary>
    /// Whether the process executed successfully.
    /// </summary>
    public bool IsSuccess => ExitCode == 0;

    /// <summary>
    /// Whether the process completed within performance requirements.
    /// </summary>
    public bool MeetsPerformanceRequirement(long maxMilliseconds) =>
        ElapsedMilliseconds <= maxMilliseconds;
}
