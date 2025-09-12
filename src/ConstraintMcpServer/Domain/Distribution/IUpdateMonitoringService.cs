namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Service for monitoring update processes and providing real-time status visibility.
/// Business value: Empowers users with complete visibility and control over system updates.
/// </summary>
public interface IUpdateMonitoringService
{
    /// <summary>
    /// Monitors an update process continuously and provides comprehensive status visibility.
    /// </summary>
    /// <param name="updateId">Unique identifier for the update to monitor</param>
    /// <param name="monitoringDuration">How long to monitor the update process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive monitoring result with status updates and progress snapshots</returns>
    Task<UpdateMonitoringResult> MonitorUpdateProcessAsync(string updateId, TimeSpan monitoringDuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current monitoring status for all active update processes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active monitoring sessions</returns>
    Task<IEnumerable<ActiveMonitoringSession>> GetActiveMonitoringSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts monitoring an update process and returns immediately.
    /// </summary>
    /// <param name="updateId">Unique identifier for the update to monitor</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if monitoring started successfully</returns>
    Task<bool> StartMonitoringAsync(string updateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops monitoring an update process.
    /// </summary>
    /// <param name="updateId">Unique identifier for the update to stop monitoring</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Final monitoring result</returns>
    Task<UpdateMonitoringResult> StopMonitoringAsync(string updateId, CancellationToken cancellationToken = default);
}
