using System;

namespace ConstraintMcpServer.Infrastructure.Logging;

/// <summary>
/// Structured logging event for pass-through operations (no constraint injection).
/// Emitted as NDJSON for offline analysis and debugging.
/// </summary>
public sealed class PassThroughEvent
{
    /// <summary>
    /// Event type identifier for structured log processing.
    /// </summary>
    public string EventType { get; set; } = "pass";

    /// <summary>
    /// Current interaction number in the session.
    /// </summary>
    public int InteractionNumber { get; set; }

    /// <summary>
    /// Reason for pass-through (not_scheduled, phase_mismatch, etc.).
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp of the event.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
