namespace ConstraintMcpServer.Domain.Enforcement;

/// <summary>
/// Domain value object representing a development phase transition.
/// Captures the history of phase changes for analysis and debugging.
/// </summary>
internal sealed record PhaseTransition
{
    /// <summary>
    /// Timestamp when the phase transition occurred.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Previous development phase.
    /// </summary>
    public required string FromPhase { get; init; }

    /// <summary>
    /// New development phase.
    /// </summary>
    public required string ToPhase { get; init; }

    /// <summary>
    /// Interaction that triggered the phase transition.
    /// </summary>
    public required int InteractionNumber { get; init; }

    /// <summary>
    /// Reason for the phase transition (e.g., "test failure detected", "tests passing").
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Confidence level of the phase detection (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; init; } = 1.0;
}