namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Represents real-time progress information for system updates.
/// Business value: Provides transparent progress feedback for professional user experience.
/// </summary>
public sealed record UpdateProgress
{
    /// <summary>
    /// Unique identifier for the update operation.
    /// </summary>
    public string UpdateId { get; init; } = string.Empty;

    /// <summary>
    /// Current progress percentage (0.0 to 100.0).
    /// </summary>
    public double PercentageComplete { get; init; }

    /// <summary>
    /// Current status message describing the update step.
    /// </summary>
    public string CurrentStatus { get; init; } = string.Empty;

    /// <summary>
    /// Estimated time remaining for completion.
    /// </summary>
    public TimeSpan EstimatedTimeRemaining { get; init; }

    /// <summary>
    /// Whether the update is currently active and progressing.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Whether the update has completed successfully.
    /// </summary>
    public bool IsCompleted { get; init; }

    /// <summary>
    /// Timestamp when progress was last updated.
    /// </summary>
    public DateTime LastUpdated { get; init; }

    /// <summary>
    /// Creates progress information for an active update.
    /// </summary>
    public static UpdateProgress CreateActive(string updateId, double percentage, string status, TimeSpan estimatedRemaining) =>
        new()
        {
            UpdateId = updateId,
            PercentageComplete = percentage,
            CurrentStatus = status,
            EstimatedTimeRemaining = estimatedRemaining,
            IsActive = true,
            IsCompleted = false,
            LastUpdated = DateTime.UtcNow
        };

    /// <summary>
    /// Creates progress information for a completed update.
    /// </summary>
    public static UpdateProgress CreateCompleted(string updateId) =>
        new()
        {
            UpdateId = updateId,
            PercentageComplete = 100.0,
            CurrentStatus = "Update completed successfully",
            EstimatedTimeRemaining = TimeSpan.Zero,
            IsActive = false,
            IsCompleted = true,
            LastUpdated = DateTime.UtcNow
        };

    /// <summary>
    /// Creates progress information for an inactive/not found update.
    /// </summary>
    public static UpdateProgress CreateInactive(string updateId) =>
        new()
        {
            UpdateId = updateId,
            PercentageComplete = 0.0,
            CurrentStatus = "Update not active",
            EstimatedTimeRemaining = TimeSpan.Zero,
            IsActive = false,
            IsCompleted = false,
            LastUpdated = DateTime.UtcNow
        };
}
