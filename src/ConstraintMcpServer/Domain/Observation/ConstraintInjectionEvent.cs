namespace ConstraintMcpServer.Domain.Observation;

using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Domain.Enforcement;

/// <summary>
/// Domain event representing a constraint injection occurrence.
/// Contains details about what constraints were injected and why.
/// </summary>
internal sealed record ConstraintInjectionEvent
{
    /// <summary>
    /// Unique identifier for this injection event.
    /// </summary>
    public required string EventId { get; init; }

    /// <summary>
    /// Timestamp when the injection occurred.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Interaction context where injection occurred.
    /// </summary>
    public required InteractionContext Context { get; init; }

    /// <summary>
    /// Constraints that were injected.
    /// </summary>
    public required IReadOnlyList<Constraint> InjectedConstraints { get; init; }

    /// <summary>
    /// Reason for the injection (cadence, phase transition, etc.).
    /// </summary>
    public required string InjectionReason { get; init; }

    /// <summary>
    /// Processing latency in milliseconds.
    /// </summary>
    public required double LatencyMs { get; init; }

    /// <summary>
    /// Size of the modified request in bytes.
    /// </summary>
    public int? ModifiedRequestSize { get; init; }

    /// <summary>
    /// Whether drift detection was triggered.
    /// </summary>
    public bool DriftDetected { get; init; }
}