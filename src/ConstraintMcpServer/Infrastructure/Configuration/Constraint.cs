using System.Collections.Generic;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Represents a single constraint that can be enforced during LLM code generation.
/// Each constraint has an identifier, priority, applicable phases, and reminder messages.
/// </summary>
internal sealed record Constraint
{
    /// <summary>
    /// Unique identifier for the constraint (e.g., "tdd.test-first").
    /// </summary>
    public required ConstraintId Id { get; init; }

    /// <summary>
    /// Human-readable title describing the constraint.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Priority score between 0.0 and 1.0 for constraint selection.
    /// Higher values indicate higher priority.
    /// </summary>
    public required Priority Priority { get; init; }

    /// <summary>
    /// The development phases where this constraint applies.
    /// </summary>
    public required IReadOnlyList<Phase> Phases { get; init; }

    /// <summary>
    /// The reminder messages to inject when this constraint is selected.
    /// </summary>
    public required IReadOnlyList<string> Reminders { get; init; }
}
