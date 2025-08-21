namespace ConstraintMcpServer.Domain.Learning;

/// <summary>
/// Domain value object representing analysis results for constraint effectiveness.
/// Contains insights from historical constraint injection data.
/// </summary>
internal sealed record ConstraintAnalysis
{
    /// <summary>
    /// Time period analyzed.
    /// </summary>
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; init; }

    /// <summary>
    /// Total number of interactions analyzed.
    /// </summary>
    public required int TotalInteractions { get; init; }

    /// <summary>
    /// Constraint effectiveness metrics by constraint ID.
    /// </summary>
    public required IReadOnlyDictionary<string, ConstraintEffectiveness> ConstraintMetrics { get; init; }

    /// <summary>
    /// Phase-specific constraint performance.
    /// </summary>
    public IReadOnlyDictionary<string, PhaseConstraintMetrics>? PhaseMetrics { get; init; }

    /// <summary>
    /// Client-specific patterns and preferences.
    /// </summary>
    public IReadOnlyDictionary<string, ClientMetrics>? ClientMetrics { get; init; }

    /// <summary>
    /// Recommendations for constraint weight adjustments.
    /// </summary>
    public IReadOnlyList<WeightRecommendation>? Recommendations { get; init; }
}

/// <summary>
/// Metrics for individual constraint effectiveness.
/// </summary>
internal sealed record ConstraintEffectiveness
{
    public required string ConstraintId { get; init; }
    public required int TimesInjected { get; init; }
    public required double AverageLatencyMs { get; init; }
    public required double DriftPreventionRate { get; init; }
    public required double UserSatisfactionScore { get; init; }
    public double RecommendedWeight { get; init; }
}

/// <summary>
/// Phase-specific constraint metrics.
/// </summary>
internal sealed record PhaseConstraintMetrics
{
    public required string Phase { get; init; }
    public required IReadOnlyDictionary<string, double> ConstraintEffectiveness { get; init; }
    public required double PhaseDetectionAccuracy { get; init; }
}

/// <summary>
/// Client-specific usage patterns.
/// </summary>
internal sealed record ClientMetrics
{
    public required string ClientName { get; init; }
    public required IReadOnlyDictionary<string, double> PreferredConstraints { get; init; }
    public required double AverageSessionLength { get; init; }
}

/// <summary>
/// Recommendation for weight adjustments.
/// </summary>
internal sealed record WeightRecommendation
{
    public required string ConstraintId { get; init; }
    public required double CurrentWeight { get; init; }
    public required double RecommendedWeight { get; init; }
    public required string Reason { get; init; }
    public required double Confidence { get; init; }
}