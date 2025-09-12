namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Service for managing user sessions with state persistence across updates.
/// Business value: Ensures workflow continuity during system updates and restarts.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Creates a new user session with specified context information.
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="sessionName">Human-readable session name</param>
    /// <param name="context">Session context description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Unique session identifier</returns>
    Task<string> CreateSessionAsync(string userId, string sessionName, string context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves session by identifier.
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Session information or null if not found</returns>
    Task<SessionInfo?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all currently active sessions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active sessions</returns>
    Task<IEnumerable<SessionInfo>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses session to preserve state during system maintenance.
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if session was paused successfully</returns>
    Task<bool> PauseSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes paused session after system maintenance.
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if session was resumed successfully</returns>
    Task<bool> ResumeSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminates session and cleans up associated resources.
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if session was terminated successfully</returns>
    Task<bool> TerminateSessionAsync(string sessionId, CancellationToken cancellationToken = default);
}
