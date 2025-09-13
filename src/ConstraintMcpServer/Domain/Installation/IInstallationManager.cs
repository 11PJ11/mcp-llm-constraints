using System;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Domain.Installation;

/// <summary>
/// Installation manager responsible for constraint server installation workflow.
/// Following Hexagonal Architecture: Port interface for installation operations.
/// Domain responsibility: Installation business logic and workflow orchestration.
/// </summary>
public interface IInstallationManager
{
    /// <summary>
    /// Perform basic installation of constraint server for specified platform.
    /// Business operation: Complete installation workflow including directory creation,
    /// binary deployment, and environment configuration.
    /// </summary>
    /// <param name="options">Platform-specific installation configuration options.</param>
    /// <returns>Installation result with success status and installation details.</returns>
    Task<InstallationResult> InstallAsync(InstallationOptions options);

    /// <summary>
    /// Validate installation was completed successfully.
    /// Business operation: Comprehensive post-installation validation including
    /// file system state, environment configuration, and operational readiness.
    /// </summary>
    /// <param name="installationPath">Path where installation was performed.</param>
    /// <returns>Validation result indicating installation health and any issues.</returns>
    Task<InstallationValidationResult> ValidateInstallationAsync(string installationPath);

    /// <summary>
    /// Perform system health validation after installation completion.
    /// Business operation: End-to-end health check ensuring all components
    /// are properly installed, configured, and operationally ready.
    /// </summary>
    /// <param name="installationPath">Path of completed installation to validate.</param>
    /// <returns>System health result with component status and readiness indicators.</returns>
    Task<SystemHealthResult> ValidateSystemHealthAsync(string installationPath);
}
