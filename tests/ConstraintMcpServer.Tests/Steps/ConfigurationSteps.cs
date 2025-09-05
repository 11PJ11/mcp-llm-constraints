using System;
using System.IO;

namespace ConstraintMcpServer.Tests.Steps;

/// <summary>
/// Level 3 Refactoring: Extract configuration management logic.
/// Focused responsibility for configuration loading, validation, and testing scenarios.
/// Follows CUPID properties: Composable, Unix Philosophy, Predictable, Idiomatic, Domain-based.
/// </summary>
internal sealed class ConfigurationSteps
{
    private string? _configPath;
    private string? _lastErrorOutput;

    /// <summary>
    /// Business-focused step: Set configuration path for server testing
    /// </summary>
    public void SetConfigurationPath(string configPath)
    {
        _configPath = configPath;
    }

    /// <summary>
    /// Get current configuration path
    /// </summary>
    public string? GetConfigurationPath() => _configPath;

    /// <summary>
    /// Business-focused step: Validate that constraint configuration exists and is well-formed
    /// </summary>
    public void ValidateConstraintConfigurationExists()
    {
        string projectRoot = GetProjectRoot();
        string defaultConfigPath = Path.Combine(projectRoot, "config", "constraints.yaml");

        if (!File.Exists(defaultConfigPath))
        {
            throw new InvalidOperationException($"Default constraint configuration not found: {defaultConfigPath}");
        }

        // Basic validation - file should contain expected structure
        string content = File.ReadAllText(defaultConfigPath);
        if (!content.Contains("version:") || !content.Contains("constraints:"))
        {
            throw new InvalidOperationException("Configuration file does not contain expected structure (version, constraints)");
        }
    }

    /// <summary>
    /// Business-focused step: Create invalid configuration for negative testing
    /// </summary>
    public void CreateInvalidConstraintConfiguration()
    {
        string projectRoot = GetProjectRoot();
        string tempConfigPath = Path.Combine(projectRoot, "config", "invalid-test.yaml");

        // Create intentionally invalid configuration
        string invalidContent = @"# Invalid configuration for testing
invalid_root_property: true
constraints:
  - id: invalid.constraint
    # Missing required properties like title, priority
    reminders:
      - ""Invalid reminder without proper structure""
";

        Directory.CreateDirectory(Path.GetDirectoryName(tempConfigPath)!);
        File.WriteAllText(tempConfigPath, invalidContent);
        SetConfigurationPath(tempConfigPath);
    }

    /// <summary>
    /// Business-focused step: Validate server loads configuration successfully
    /// </summary>
    public void ValidateServerLoadsConfiguration()
    {
        // This step verifies that the server process started successfully with configuration
        // The actual validation happens during server startup in ProcessManagementSteps
        // This is a business validation that depends on server startup success

        if (_configPath != null && !File.Exists(_configPath))
        {
            throw new InvalidOperationException($"Configuration file not found: {_configPath}");
        }
    }

    /// <summary>
    /// Business-focused step: Validate server rejects invalid configuration with clear error
    /// </summary>
    public void ValidateServerRejectsConfiguration()
    {
        // Server should have failed to start due to invalid configuration
        // This validation is performed in conjunction with ProcessManagementSteps
        if (string.IsNullOrWhiteSpace(_lastErrorOutput))
        {
            throw new InvalidOperationException("Expected configuration error but no error output found");
        }
    }

    /// <summary>
    /// Business-focused step: Validate error message indicates validation failure
    /// </summary>
    public void ValidateErrorIndicatesValidationFailure()
    {
        if (_lastErrorOutput == null)
        {
            throw new InvalidOperationException("No error output available for validation");
        }

        // Error should indicate configuration validation failure
        bool hasValidationError = _lastErrorOutput.Contains("validation", StringComparison.OrdinalIgnoreCase) ||
                                _lastErrorOutput.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
                                _lastErrorOutput.Contains("configuration", StringComparison.OrdinalIgnoreCase) ||
                                _lastErrorOutput.Contains("constraint", StringComparison.OrdinalIgnoreCase);

        if (!hasValidationError)
        {
            throw new InvalidOperationException($"Error message does not indicate configuration validation failure: {_lastErrorOutput}");
        }
    }

    /// <summary>
    /// Business-focused step: Create constraint pack with multiple priorities for testing
    /// </summary>
    public void CreateConstraintPackWithMultiplePriorities()
    {
        string projectRoot = GetProjectRoot();
        string testConfigPath = Path.Combine(projectRoot, "config", "test-priorities.yaml");

        string configContent = @"version: ""0.1.0""
constraints:
  - id: ""high.priority.constraint""
    title: ""High Priority Test Constraint""
    priority: 0.95
    phases: [""kickoff"", ""red"", ""commit""]
    reminders:
      - ""This is a high priority constraint for testing""
      
  - id: ""medium.priority.constraint""
    title: ""Medium Priority Test Constraint""
    priority: 0.65
    phases: [""green"", ""refactor""]
    reminders:
      - ""This is a medium priority constraint for testing""
      
  - id: ""low.priority.constraint""
    title: ""Low Priority Test Constraint""
    priority: 0.25
    phases: [""commit""]
    reminders:
      - ""This is a low priority constraint for testing""
";

        Directory.CreateDirectory(Path.GetDirectoryName(testConfigPath)!);
        File.WriteAllText(testConfigPath, configContent);
        SetConfigurationPath(testConfigPath);
    }

    /// <summary>
    /// Business-focused step: Clean up test configuration files
    /// </summary>
    public void CleanupTestConfigurations()
    {
        string projectRoot = GetProjectRoot();
        string configDir = Path.Combine(projectRoot, "config");

        // Clean up test configuration files
        var testFiles = new[]
        {
            Path.Combine(configDir, "invalid-test.yaml"),
            Path.Combine(configDir, "test-priorities.yaml")
        };

        foreach (string file in testFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

    /// <summary>
    /// Set last error output for validation
    /// </summary>
    public void SetLastErrorOutput(string? errorOutput)
    {
        _lastErrorOutput = errorOutput;
    }

    /// <summary>
    /// Get last error output for diagnostics
    /// </summary>
    public string? GetLastErrorOutput() => _lastErrorOutput;

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
}
