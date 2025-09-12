namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Information about a user session including state and activity tracking.
/// Business value: Enables session continuity and workflow preservation across updates.
/// </summary>
public sealed record SessionInfo
{
    /// <summary>
    /// Unique session identifier.
    /// </summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>
    /// User identifier who owns this session.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable session name.
    /// </summary>
    public string SessionName { get; init; } = string.Empty;

    /// <summary>
    /// Session context description.
    /// </summary>
    public string Context { get; init; } = string.Empty;

    /// <summary>
    /// Whether the session is currently active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Whether the session is paused (for maintenance or updates).
    /// </summary>
    public bool IsPaused { get; init; }

    /// <summary>
    /// When the session was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the session was last active.
    /// </summary>
    public DateTime LastActivity { get; init; }

    /// <summary>
    /// When the session was paused (if applicable).
    /// </summary>
    public DateTime? PausedAt { get; init; }

    /// <summary>
    /// When the session was resumed (if applicable).
    /// </summary>
    public DateTime? ResumedAt { get; init; }

    /// <summary>
    /// Creates active session info.
    /// </summary>
    public static SessionInfo CreateActive(string sessionId, string userId, string sessionName, string context, DateTime createdAt) =>
        new()
        {
            SessionId = sessionId,
            UserId = userId,
            SessionName = sessionName,
            Context = context,
            IsActive = true,
            IsPaused = false,
            CreatedAt = createdAt,
            LastActivity = createdAt
        };

    /// <summary>
    /// Creates paused copy of this session.
    /// </summary>
    public SessionInfo AsPaused(DateTime pausedAt) =>
        this with
        {
            IsActive = false,
            IsPaused = true,
            PausedAt = pausedAt,
            LastActivity = pausedAt
        };

    /// <summary>
    /// Creates resumed copy of this session.
    /// </summary>
    public SessionInfo AsResumed(DateTime resumedAt) =>
        this with
        {
            IsActive = true,
            IsPaused = false,
            ResumedAt = resumedAt,
            LastActivity = resumedAt
        };
}
