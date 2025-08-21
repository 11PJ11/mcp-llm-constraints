namespace ConstraintMcpServer.Domain.Enforcement;

/// <summary>
/// Domain value object representing scheduler configuration for constraint injection.
/// Defines cadence, phase overrides, and injection policies.
/// </summary>
internal sealed record ScheduleConfiguration
{
    /// <summary>
    /// Base injection cadence (inject every N interactions).
    /// </summary>
    public required int BaseCadence { get; init; }

    /// <summary>
    /// Phase-specific injection overrides.
    /// </summary>
    public IReadOnlyDictionary<string, int>? PhaseOverrides { get; init; }

    /// <summary>
    /// Whether to inject constraints on the first interaction of a session.
    /// </summary>
    public bool InjectOnFirstInteraction { get; init; } = true;

    /// <summary>
    /// Whether to inject constraints after phase transitions.
    /// </summary>
    public bool InjectOnPhaseTransition { get; init; } = true;

    /// <summary>
    /// Maximum number of constraints to inject at once.
    /// </summary>
    public int MaxConstraintsPerInjection { get; init; } = 5;

    /// <summary>
    /// Minimum priority threshold for constraint selection.
    /// </summary>
    public double MinimumPriority { get; init; } = 0.0;

    /// <summary>
    /// Whether to enable deterministic constraint selection.
    /// </summary>
    public bool DeterministicSelection { get; init; } = true;
}