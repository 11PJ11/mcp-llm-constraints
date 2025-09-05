using ConstraintMcpServer.Domain.Distribution;

namespace ConstraintMcpServer.Application.Services;

/// <summary>
/// Application service for professional installation workflows.
/// Business value: Orchestrates installation, update, and maintenance operations.
/// </summary>
public interface IInstallationService
{
    /// <summary>
    /// Installs the constraint system with professional configuration.
    /// Business value: One-command installation with immediate usability.
    /// </summary>
    Task<InstallationResult> InstallAsync(InstallationOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the system while preserving user configurations.
    /// Business value: Seamless updates maintain user personalization.
    /// </summary>
    Task<UpdateResult> UpdateAsync(UpdateOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uninstalls the system with configuration preservation options.
    /// Business value: Clean removal with optional configuration preservation.
    /// </summary>
    Task<UninstallResult> UninstallAsync(UninstallOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates system health and generates diagnostic report.
    /// Business value: Quick troubleshooting with actionable guidance.
    /// </summary>
    Task<HealthCheckResult> PerformHealthCheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks for available updates from GitHub releases.
    /// Business value: Proactive update notifications keep users current.
    /// </summary>
    Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the system is properly installed and functional.
    /// Business value: Installation verification with immediate feedback.
    /// </summary>
    Task<bool> ValidateInstallationAsync(CancellationToken cancellationToken = default);
}