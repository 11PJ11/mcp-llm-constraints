namespace ConstraintMcpServer.Domain.Constraints;

// COMMENTED OUT: Domain layer created without failing tests (TDD violation)
// TODO: Uncomment when driven by failing acceptance tests for constraint enforcement

/// <summary>
/// Domain value object representing a single constraint rule for LLM coding agents.
/// Immutable constraint definition with priority, phases, and reminder content.
/// </summary>
/*
internal sealed record Constraint
{
    /// <summary>
    /// Unique identifier for this constraint (e.g., "tdd.test-first").
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Human-readable title describing the constraint.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Priority for constraint selection (0.0 to 1.0, higher = more important).
    /// </summary>
    public required double Priority { get; init; }

    /// <summary>
    /// Development phases where this constraint applies (e.g., kickoff, red, commit).
    /// </summary>
    public required IReadOnlyList<string> Phases { get; init; }

    /// <summary>
    /// List of reminder messages to inject at tool boundaries.
    /// </summary>
    public required IReadOnlyList<string> Reminders { get; init; }

    /// <summary>
    /// Optional keywords for drift detection and matching.
    /// </summary>
    public IReadOnlyList<string>? Keywords { get; init; }

    /// <summary>
    /// Optional regex patterns for advanced drift detection.
    /// </summary>
    public IReadOnlyList<string>? RegexPatterns { get; init; }
}
*/