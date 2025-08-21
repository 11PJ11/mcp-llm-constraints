namespace ConstraintMcpServer.Domain.Observation;

using ConstraintMcpServer.Domain.Enforcement;

/// <summary>
/// Domain event representing a development phase transition.
/// Contains details about phase changes for analysis and debugging.
/// </summary>
internal sealed record PhaseTransitionEvent
{
    /// <summary>
    /// Unique identifier for this phase transition event.
    /// </summary>
    public required string EventId { get; init; }

    /// <summary>
    /// Timestamp when the phase transition occurred.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Interaction context where transition occurred.
    /// </summary>
    public required InteractionContext Context { get; init; }

    /// <summary>
    /// Phase transition details.
    /// </summary>
    public required PhaseTransition Transition { get; init; }

    /// <summary>
    /// Processing latency in milliseconds.
    /// </summary>
    public required double LatencyMs { get; init; }

    /// <summary>
    /// Input signals that triggered the phase detection.
    /// </summary>
    public IReadOnlyList<string>? DetectionSignals { get; init; }
}