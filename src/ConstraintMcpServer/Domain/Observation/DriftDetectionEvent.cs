namespace ConstraintMcpServer.Domain.Observation;

using ConstraintMcpServer.Domain.Enforcement;

/// <summary>
/// Domain event representing model drift detection.
/// Contains details about detected drift patterns and severity.
/// </summary>
internal sealed record DriftDetectionEvent
{
    /// <summary>
    /// Unique identifier for this drift detection event.
    /// </summary>
    public required string EventId { get; init; }

    /// <summary>
    /// Timestamp when drift was detected.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Interaction context where drift was detected.
    /// </summary>
    public required InteractionContext Context { get; init; }

    /// <summary>
    /// Type of drift detected (keyword, pattern, behavioral).
    /// </summary>
    public required string DriftType { get; init; }

    /// <summary>
    /// Severity of the detected drift (0.0 to 1.0).
    /// </summary>
    public required double Severity { get; init; }

    /// <summary>
    /// Specific patterns or keywords that triggered detection.
    /// </summary>
    public required IReadOnlyList<string> TriggerPatterns { get; init; }

    /// <summary>
    /// Constraints that would be relevant for this drift.
    /// </summary>
    public IReadOnlyList<string>? RelevantConstraintIds { get; init; }

    /// <summary>
    /// Processing latency in milliseconds.
    /// </summary>
    public required double LatencyMs { get; init; }
}