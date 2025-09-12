namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Represents the result of monitoring an update process with comprehensive status information.
/// Business value: Provides users with complete visibility into update process execution.
/// </summary>
public sealed record UpdateMonitoringResult
{
    /// <summary>
    /// Whether the monitoring process completed successfully.
    /// </summary>
    public bool IsSuccessful { get; init; }

    /// <summary>
    /// Unique identifier for the monitored update.
    /// </summary>
    public string UpdateId { get; init; } = string.Empty;

    /// <summary>
    /// Total number of status updates received during monitoring.
    /// </summary>
    public int StatusUpdatesReceived { get; init; }

    /// <summary>
    /// Number of progress snapshots captured during monitoring.
    /// </summary>
    public int ProgressSnapshotCount { get; init; }

    /// <summary>
    /// Final status message when monitoring completed.
    /// </summary>
    public string FinalStatus { get; init; } = string.Empty;

    /// <summary>
    /// Duration of the monitoring session.
    /// </summary>
    public TimeSpan MonitoringDuration { get; init; }

    /// <summary>
    /// Collection of all status updates received during monitoring.
    /// </summary>
    public IReadOnlyList<string> StatusUpdateHistory { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Collection of progress snapshots taken during monitoring.
    /// </summary>
    public IReadOnlyList<UpdateProgress> ProgressSnapshots { get; init; } = Array.Empty<UpdateProgress>();

    /// <summary>
    /// Timestamp when monitoring started.
    /// </summary>
    public DateTime StartedAt { get; init; }

    /// <summary>
    /// Timestamp when monitoring completed.
    /// </summary>
    public DateTime CompletedAt { get; init; }

    /// <summary>
    /// Creates a successful monitoring result.
    /// </summary>
    public static UpdateMonitoringResult Success(
        string updateId,
        int statusUpdatesReceived,
        int progressSnapshotCount,
        string finalStatus,
        TimeSpan monitoringDuration,
        IReadOnlyList<string> statusHistory,
        IReadOnlyList<UpdateProgress> progressSnapshots) =>
        new()
        {
            IsSuccessful = true,
            UpdateId = updateId,
            StatusUpdatesReceived = statusUpdatesReceived,
            ProgressSnapshotCount = progressSnapshotCount,
            FinalStatus = finalStatus,
            MonitoringDuration = monitoringDuration,
            StatusUpdateHistory = statusHistory,
            ProgressSnapshots = progressSnapshots,
            StartedAt = DateTime.UtcNow.Subtract(monitoringDuration),
            CompletedAt = DateTime.UtcNow
        };

    /// <summary>
    /// Creates a failed monitoring result.
    /// </summary>
    public static UpdateMonitoringResult Failure(string updateId, string reason, TimeSpan monitoringDuration) =>
        new()
        {
            IsSuccessful = false,
            UpdateId = updateId,
            StatusUpdatesReceived = 0,
            ProgressSnapshotCount = 0,
            FinalStatus = $"Monitoring failed: {reason}",
            MonitoringDuration = monitoringDuration,
            StartedAt = DateTime.UtcNow.Subtract(monitoringDuration),
            CompletedAt = DateTime.UtcNow
        };
}
