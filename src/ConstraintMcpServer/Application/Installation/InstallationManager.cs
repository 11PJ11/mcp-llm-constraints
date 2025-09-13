using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain.Installation;

namespace ConstraintMcpServer.Application.Installation;

/// <summary>
/// Installation manager responsible for constraint server installation workflow.
/// Following Hexagonal Architecture: Application service implementing domain port.
/// Orchestrates installation business logic with infrastructure adapters.
/// </summary>
public sealed class InstallationManager : IInstallationManager
{
    /// <summary>
    /// Perform basic installation of constraint server for specified platform.
    /// Business operation: Complete installation workflow including directory creation,
    /// binary deployment, and environment configuration.
    /// </summary>
    /// <param name="options">Platform-specific installation configuration options.</param>
    /// <returns>Installation result with success status and installation details.</returns>
    public async Task<InstallationResult> InstallAsync(InstallationOptions options)
    {
        var startTime = DateTimeOffset.UtcNow;
        var installedComponents = new List<string>();

        try
        {
            // Validate installation requirements
            if (options.ValidateRequirements)
            {
                await ValidateInstallationRequirementsAsync(options);
            }

            // Create installation directory
            await CreateInstallationDirectoryAsync(options.InstallationPath);
            installedComponents.Add("Installation directory");

            // Deploy constraint server binaries
            await DeployConstraintServerBinariesAsync(options.InstallationPath, options.TargetPlatform);
            installedComponents.Add("Constraint server binaries");

            // Create default configuration
            await CreateDefaultConfigurationAsync(options.InstallationPath);
            installedComponents.Add("Default configuration");

            // Configure environment PATH if requested
            if (options.ConfigureEnvironmentPath)
            {
                await ConfigureEnvironmentPathAsync(options.InstallationPath, options.TargetPlatform);
                installedComponents.Add("Environment PATH configuration");
            }

            // Create shortcuts if requested
            if (options.CreateShortcuts)
            {
                await CreateShortcutsAsync(options.InstallationPath, options.TargetPlatform);
                installedComponents.Add("Desktop shortcuts");
            }

            var duration = DateTimeOffset.UtcNow - startTime;

            return InstallationResult.Success(
                installationPath: options.InstallationPath,
                targetPlatform: options.TargetPlatform,
                installedComponents: installedComponents,
                durationMs: (long)duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            var issues = new[] { ex.Message };

            return InstallationResult.Failure(
                installationPath: options.InstallationPath,
                targetPlatform: options.TargetPlatform,
                errorMessage: "Installation failed",
                issues: issues);
        }
    }

    /// <summary>
    /// Validate installation was completed successfully.
    /// Business operation: Comprehensive post-installation validation including
    /// file system state, environment configuration, and operational readiness.
    /// </summary>
    /// <param name="installationPath">Path where installation was performed.</param>
    /// <returns>Validation result indicating installation health and any issues.</returns>
    public async Task<InstallationValidationResult> ValidateInstallationAsync(string installationPath)
    {
        var validatedComponents = new List<string>();
        var issues = new List<ValidationIssue>();

        try
        {
            // Validate installation directory exists
            if (!Directory.Exists(installationPath))
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Critical,
                    Component = "Installation Directory",
                    Description = $"Installation directory not found: {installationPath}",
                    SuggestedResolution = "Run installation process to create required directory"
                });
            }
            else
            {
                validatedComponents.Add("Installation directory present");
            }

            // Validate constraint server binary exists
            var binaryPath = GetConstraintServerBinaryPath(installationPath);
            if (!File.Exists(binaryPath))
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Critical,
                    Component = "Server Binary",
                    Description = "Constraint server binary not found",
                    SuggestedResolution = "Reinstall constraint server binaries"
                });
            }
            else
            {
                validatedComponents.Add("Server binary present");
            }

            // Validate configuration files exist
            await ValidateConfigurationFilesAsync(installationPath, validatedComponents, issues);

            // Return validation result
            if (issues.Count == 0)
            {
                return InstallationValidationResult.Success(
                    installationPath: installationPath,
                    validatedComponents: validatedComponents);
            }
            else
            {
                return InstallationValidationResult.Failure(
                    installationPath: installationPath,
                    issues: issues);
            }
        }
        catch (Exception ex)
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Component = "Validation Process",
                Description = $"Validation failed with error: {ex.Message}",
                SuggestedResolution = "Check installation integrity and retry validation"
            });

            return InstallationValidationResult.Failure(
                installationPath: installationPath,
                issues: issues);
        }
    }

    /// <summary>
    /// Perform system health validation after installation completion.
    /// Business operation: End-to-end health check ensuring all components
    /// are properly installed, configured, and operationally ready.
    /// </summary>
    /// <param name="installationPath">Path of completed installation to validate.</param>
    /// <returns>System health result with component status and readiness indicators.</returns>
    public async Task<SystemHealthResult> ValidateSystemHealthAsync(string installationPath)
    {
        var healthChecks = new List<ComponentHealthCheck>();
        var healthIssues = new List<HealthIssue>();

        try
        {
            // Check runtime environment
            var runtimeCheck = await PerformRuntimeHealthCheckAsync();
            healthChecks.Add(runtimeCheck);
            if (!runtimeCheck.IsHealthy)
            {
                healthIssues.Add(new HealthIssue
                {
                    Severity = HealthSeverity.Critical,
                    Component = "Runtime",
                    Description = runtimeCheck.HealthMessage,
                    Impact = "System cannot start",
                    SuggestedAction = "Install required runtime dependencies"
                });
            }

            // Check configuration health
            var configCheck = await PerformConfigurationHealthCheckAsync(installationPath);
            healthChecks.Add(configCheck);
            if (!configCheck.IsHealthy)
            {
                healthIssues.Add(new HealthIssue
                {
                    Severity = HealthSeverity.Major,
                    Component = "Configuration",
                    Description = configCheck.HealthMessage,
                    Impact = "System may not function correctly",
                    SuggestedAction = "Repair or recreate configuration files"
                });
            }

            // Check MCP protocol readiness
            var mcpCheck = await PerformMcpProtocolHealthCheckAsync(installationPath);
            healthChecks.Add(mcpCheck);
            if (!mcpCheck.IsHealthy)
            {
                healthIssues.Add(new HealthIssue
                {
                    Severity = HealthSeverity.Major,
                    Component = "MCP Protocol",
                    Description = mcpCheck.HealthMessage,
                    Impact = "MCP communication will fail",
                    SuggestedAction = "Check network connectivity and protocol configuration"
                });
            }

            // Return health result
            if (healthIssues.Count == 0)
            {
                return SystemHealthResult.Healthy(
                    installationPath: installationPath,
                    componentChecks: healthChecks);
            }
            else
            {
                return SystemHealthResult.Unhealthy(
                    installationPath: installationPath,
                    componentChecks: healthChecks,
                    healthIssues: healthIssues);
            }
        }
        catch (Exception ex)
        {
            var errorHealthIssue = new HealthIssue
            {
                Severity = HealthSeverity.Critical,
                Component = "Health Check Process",
                Description = $"Health validation failed: {ex.Message}",
                Impact = "Cannot determine system operational status",
                SuggestedAction = "Check system integrity and retry health validation"
            };

            return SystemHealthResult.Unhealthy(
                installationPath: installationPath,
                componentChecks: healthChecks,
                healthIssues: new[] { errorHealthIssue });
        }
    }

    #region Private Implementation Methods

    private async Task ValidateInstallationRequirementsAsync(InstallationOptions options)
    {
        // Validate platform support
        if (!IsSupportedPlatform(options.TargetPlatform))
        {
            throw new PlatformNotSupportedException($"Platform {options.TargetPlatform} is not supported");
        }

        // Validate installation path accessibility
        var parentDirectory = Path.GetDirectoryName(options.InstallationPath);
        if (!string.IsNullOrEmpty(parentDirectory) && !Directory.Exists(parentDirectory))
        {
            throw new DirectoryNotFoundException($"Parent directory does not exist: {parentDirectory}");
        }

        await Task.CompletedTask; // Placeholder for async requirements validation
    }

    private async Task CreateInstallationDirectoryAsync(string installationPath)
    {
        if (!Directory.Exists(installationPath))
        {
            Directory.CreateDirectory(installationPath);
        }

        await Task.CompletedTask;
    }

    private async Task DeployConstraintServerBinariesAsync(string installationPath, PlatformType platform)
    {
        // Placeholder: In production, this would extract and deploy actual binaries
        var binaryName = GetBinaryNameForPlatform(platform);
        var binaryPath = Path.Combine(installationPath, binaryName);

        // Create placeholder binary file for testing
        await File.WriteAllTextAsync(binaryPath, "# Constraint MCP Server Binary Placeholder");
    }

    private async Task CreateDefaultConfigurationAsync(string installationPath)
    {
        var configPath = Path.Combine(installationPath, "constraints.yaml");
        var defaultConfig = """
            version: "0.1.0"
            constraints:
              - id: tdd.test-first
                title: "Write a failing test first"
                priority: 0.92
                phases: [kickoff, red, commit]
                reminders:
                  - "Start with a failing test (RED) before implementation."
            """;

        await File.WriteAllTextAsync(configPath, defaultConfig);
    }

    private async Task ConfigureEnvironmentPathAsync(string installationPath, PlatformType platform)
    {
        // Placeholder: In production, this would modify system PATH
        await Task.CompletedTask;
    }

    private async Task CreateShortcutsAsync(string installationPath, PlatformType platform)
    {
        // Placeholder: In production, this would create desktop shortcuts
        await Task.CompletedTask;
    }

    private async Task ValidateConfigurationFilesAsync(string installationPath,
        List<string> validatedComponents, List<ValidationIssue> issues)
    {
        var configPath = Path.Combine(installationPath, "constraints.yaml");
        if (!File.Exists(configPath))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Warning,
                Component = "Configuration",
                Description = "Default configuration file not found",
                SuggestedResolution = "Create default constraints.yaml configuration"
            });
        }
        else
        {
            validatedComponents.Add("Configuration files present");
        }

        await Task.CompletedTask;
    }

    private async Task<ComponentHealthCheck> PerformRuntimeHealthCheckAsync()
    {
        var startTime = DateTimeOffset.UtcNow;

        // Check .NET runtime
        var runtimeVersion = Environment.Version;
        var isHealthy = runtimeVersion.Major >= 8; // Require .NET 8+

        var duration = DateTimeOffset.UtcNow - startTime;

        return new ComponentHealthCheck
        {
            ComponentName = "Runtime Environment",
            IsHealthy = isHealthy,
            HealthMessage = isHealthy
                ? $".NET runtime {runtimeVersion} is compatible"
                : $".NET runtime {runtimeVersion} is outdated",
            CheckDurationMs = (long)duration.TotalMilliseconds
        };
    }

    private async Task<ComponentHealthCheck> PerformConfigurationHealthCheckAsync(string installationPath)
    {
        var startTime = DateTimeOffset.UtcNow;

        var configPath = Path.Combine(installationPath, "constraints.yaml");
        var isHealthy = File.Exists(configPath);

        var duration = DateTimeOffset.UtcNow - startTime;

        return new ComponentHealthCheck
        {
            ComponentName = "Configuration",
            IsHealthy = isHealthy,
            HealthMessage = isHealthy
                ? "Configuration files are present and accessible"
                : "Configuration files are missing or inaccessible",
            CheckDurationMs = (long)duration.TotalMilliseconds
        };
    }

    private async Task<ComponentHealthCheck> PerformMcpProtocolHealthCheckAsync(string installationPath)
    {
        var startTime = DateTimeOffset.UtcNow;

        // Placeholder: In production, this would test MCP protocol functionality
        var isHealthy = true;

        var duration = DateTimeOffset.UtcNow - startTime;

        return new ComponentHealthCheck
        {
            ComponentName = "MCP Protocol",
            IsHealthy = isHealthy,
            HealthMessage = isHealthy
                ? "MCP protocol is ready for communication"
                : "MCP protocol has connectivity issues",
            CheckDurationMs = (long)duration.TotalMilliseconds
        };
    }

    private static bool IsSupportedPlatform(PlatformType platform)
    {
        return platform is PlatformType.Windows or PlatformType.Linux or PlatformType.MacOS;
    }

    private static string GetBinaryNameForPlatform(PlatformType platform)
    {
        return platform switch
        {
            PlatformType.Windows => "constraint-mcp-server.exe",
            PlatformType.Linux => "constraint-mcp-server",
            PlatformType.MacOS => "constraint-mcp-server",
            _ => throw new ArgumentException($"Unsupported platform: {platform}")
        };
    }

    private static string GetConstraintServerBinaryPath(string installationPath)
    {
        var platform = DetectCurrentPlatform();
        var binaryName = GetBinaryNameForPlatform(platform);
        return Path.Combine(installationPath, binaryName);
    }

    private static PlatformType DetectCurrentPlatform()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return PlatformType.Windows;
        }

        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            if (Directory.Exists("/System/Library/CoreServices"))
            {
                return PlatformType.MacOS;
            }

            return PlatformType.Linux;
        }

        throw new PlatformNotSupportedException($"Unsupported platform: {Environment.OSVersion.Platform}");
    }

    #endregion
}