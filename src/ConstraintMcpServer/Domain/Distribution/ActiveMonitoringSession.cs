namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Represents an active monitoring session for an update process.
/// Business value: Tracks user monitoring activity and provides session management capabilities.
/// </summary>
public sealed record ActiveMonitoringSession
{
    /// <summary>
    /// Unique identifier for the monitored update.
    /// </summary>
    public string UpdateId { get; init; } = string.Empty;

    /// <summary>
    /// Unique identifier for the monitoring session.
    /// </summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>
    /// Whether the monitoring session is currently active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Timestamp when monitoring started.
    /// </summary>
    public DateTime StartedAt { get; init; }

    /// <summary>
    /// Current status of the monitored update.
    /// </summary>
    public string CurrentStatus { get; init; } = string.Empty;

    /// <summary>
    /// Number of status updates received so far.
    /// </summary>
    public int StatusUpdatesReceived { get; init; }

    /// <summary>
    /// Number of progress snapshots captured so far.
    /// </summary>
    public int ProgressSnapshotCount { get; init; }

    /// <summary>
    /// Duration of the monitoring session so far.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Creates an active monitoring session.
    /// </summary>
    public static ActiveMonitoringSession CreateActive(string updateId, string sessionId) =>
        new()
        {
            UpdateId = updateId,
            SessionId = sessionId,
            IsActive = true,
            StartedAt = DateTime.UtcNow,
            CurrentStatus = "Monitoring started",
            StatusUpdatesReceived = 0,
            ProgressSnapshotCount = 0,
            Duration = TimeSpan.Zero
        };

    /// <summary>
    /// Creates an updated monitoring session with new progress information.
    /// </summary>
    public ActiveMonitoringSession WithProgress(string currentStatus, int statusUpdatesReceived, int progressSnapshotCount) =>
        this with
        {
            CurrentStatus = currentStatus,
            StatusUpdatesReceived = statusUpdatesReceived,
            ProgressSnapshotCount = progressSnapshotCount,
            Duration = DateTime.UtcNow - StartedAt
        };

    /// <summary>
    /// Creates a completed monitoring session.
    /// </summary>
    public ActiveMonitoringSession AsCompleted(string finalStatus) =>
        this with
        {
            IsActive = false,
            CurrentStatus = finalStatus,
            Duration = DateTime.UtcNow - StartedAt
        };
}
