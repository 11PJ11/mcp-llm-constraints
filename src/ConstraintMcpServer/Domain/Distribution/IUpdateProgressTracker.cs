namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Service for tracking real-time update progress with professional user experience.
/// Business value: Provides transparency and user confidence during system updates.
/// </summary>
public interface IUpdateProgressTracker
{
    /// <summary>
    /// Gets current update progress with percentage and status information.
    /// </summary>
    /// <param name="updateId">Unique identifier for the update operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current update progress information</returns>
    Task<UpdateProgress> GetCurrentProgressAsync(string updateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts tracking progress for an update operation.
    /// </summary>
    /// <param name="updateId">Unique identifier for the update operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if tracking started successfully</returns>
    Task<bool> StartTrackingAsync(string updateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops tracking progress for an update operation.
    /// </summary>
    /// <param name="updateId">Unique identifier for the update operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if tracking stopped successfully</returns>
    Task<bool> StopTrackingAsync(string updateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all currently tracked update operations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of update IDs currently being tracked</returns>
    Task<IEnumerable<string>> GetActiveUpdateIdsAsync(CancellationToken cancellationToken = default);
}
