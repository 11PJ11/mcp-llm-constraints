namespace ConstraintMcpServer.Domain.Learning;

/// <summary>
/// Domain value object representing consolidated analysis across all learning domains.
/// Combines constraint, phase, and drift analysis for holistic model improvement.
/// </summary>
internal sealed record ConsolidatedAnalysis
{
    /// <summary>
    /// Time period analyzed.
    /// </summary>
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; init; }

    /// <summary>
    /// Constraint effectiveness analysis results.
    /// </summary>
    public required ConstraintAnalysis ConstraintAnalysis { get; init; }

    /// <summary>
    /// Phase detection analysis results.
    /// </summary>
    public required PhaseAnalysis PhaseAnalysis { get; init; }

    /// <summary>
    /// Drift detection analysis results.
    /// </summary>
    public required DriftAnalysis DriftAnalysis { get; init; }

    /// <summary>
    /// Cross-domain insights and correlations.
    /// </summary>
    public required IReadOnlyList<CrossDomainInsight> CrossDomainInsights { get; init; }

    /// <summary>
    /// Overall system performance metrics.
    /// </summary>
    public required SystemPerformanceMetrics SystemMetrics { get; init; }

    /// <summary>
    /// Recommended actions for system improvement.
    /// </summary>
    public required IReadOnlyList<SystemRecommendation> Recommendations { get; init; }

    /// <summary>
    /// Model confidence and reliability scores.
    /// </summary>
    public required ModelConfidenceScores ConfidenceScores { get; init; }
}

/// <summary>
/// Insight that spans multiple analysis domains.
/// </summary>
internal sealed record CrossDomainInsight
{
    public required string InsightType { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyList<string> AffectedDomains { get; init; }
    public required double Confidence { get; init; }
    public required double ImpactScore { get; init; }
    public IReadOnlyDictionary<string, object>? SupportingData { get; init; }
}

/// <summary>
/// Overall system performance metrics.
/// </summary>
internal sealed record SystemPerformanceMetrics
{
    public required double AverageLatencyMs { get; init; }
    public required double P95LatencyMs { get; init; }
    public required double P99LatencyMs { get; init; }
    public required double MemoryEfficiencyScore { get; init; }
    public required double ConstraintEffectivenessScore { get; init; }
    public required double DriftPreventionScore { get; init; }
    public required double OverallSystemScore { get; init; }
}

/// <summary>
/// High-level recommendation for system improvement.
/// </summary>
internal sealed record SystemRecommendation
{
    public required string RecommendationType { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required double ExpectedImprovement { get; init; }
    public required string Priority { get; init; } // high, medium, low
    public required IReadOnlyList<string> ImplementationSteps { get; init; }
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Model confidence and reliability scores.
/// </summary>
internal sealed record ModelConfidenceScores
{
    public required double ConstraintSelectionConfidence { get; init; }
    public required double PhaseDetectionConfidence { get; init; }
    public required double DriftDetectionConfidence { get; init; }
    public required double OverallModelConfidence { get; init; }
    public required double DataQualityScore { get; init; }
    public required double PredictionReliability { get; init; }
}