using System;

namespace ConstraintMcpServer.Infrastructure.Logging;

/// <summary>
/// Structured logging event for constraint injection operations.
/// Emitted as NDJSON for offline analysis and debugging.
/// </summary>
public sealed class ConstraintInjectionEvent
{
    /// <summary>
    /// Event type identifier for structured log processing.
    /// </summary>
    public string EventType { get; set; } = "inject";

    /// <summary>
    /// Current interaction number in the session.
    /// </summary>
    public int InteractionNumber { get; set; }

    /// <summary>
    /// TDD phase context (red, green, refactor, etc.).
    /// </summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>
    /// IDs of constraints selected for injection.
    /// </summary>
    public string[] SelectedConstraintIds { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Reason for constraint injection (scheduled_injection, phase_override, etc.).
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp of the event.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
