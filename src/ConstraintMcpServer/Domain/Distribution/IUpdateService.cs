namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Service for managing system updates with minimal service interruption.
/// Business value: Professional update experience with seamless service continuity.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Updates the system to specified version with configurable service interruption.
    /// Provides professional-grade update experience with minimal downtime.
    /// </summary>
    /// <param name="options">Update configuration options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update result with service restart status</returns>
    Task<UpdateResult> UpdateSystemAsync(UpdateOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if updates are available from the configured update source.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updates are available</returns>
    Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current system version.
    /// </summary>
    /// <returns>Current system version string</returns>
    Task<string> GetCurrentVersionAsync();

    /// <summary>
    /// Gets the latest available version from update source.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Latest available version string</returns>
    Task<string?> GetLatestVersionAsync(CancellationToken cancellationToken = default);
}
