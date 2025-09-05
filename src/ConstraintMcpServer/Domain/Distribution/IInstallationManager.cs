namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Domain interface for professional installation management.
/// Business value: Handles cross-platform installation logic and configuration preservation.
/// </summary>
public interface IInstallationManager
{
    /// <summary>
    /// Performs system installation for the specified platform.
    /// Business value: Provides professional installation experience across platforms.
    /// </summary>
    Task<InstallationResult> InstallSystemAsync(InstallationOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the system is properly installed and functional.
    /// Business value: Ensures installation actually works and provides value.
    /// </summary>
    Task<HealthCheckResult> ValidateSystemHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates existing installation to new version.
    /// Business value: Seamless updates with configuration preservation.
    /// </summary>
    Task<UpdateResult> UpdateSystemAsync(UpdateOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes system installation with optional configuration preservation.
    /// Business value: Clean removal with option to preserve user configurations.
    /// </summary>
    Task<UninstallResult> UninstallSystemAsync(UninstallOptions options, CancellationToken cancellationToken = default);
}
