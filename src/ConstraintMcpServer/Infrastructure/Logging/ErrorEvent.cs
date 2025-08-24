using System;

namespace ConstraintMcpServer.Infrastructure.Logging;

/// <summary>
/// Structured logging event for error conditions.
/// Emitted as NDJSON for offline analysis and debugging.
/// </summary>
public sealed class ErrorEvent
{
    /// <summary>
    /// Event type identifier for structured log processing.
    /// </summary>
    public string EventType { get; set; } = "error";

    /// <summary>
    /// Current interaction number in the session.
    /// </summary>
    public int InteractionNumber { get; set; }

    /// <summary>
    /// Error message describing the issue.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp of the event.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
