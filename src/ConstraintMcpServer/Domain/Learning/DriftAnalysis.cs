namespace ConstraintMcpServer.Domain.Learning;

/// <summary>
/// Domain value object representing analysis results for drift detection patterns.
/// Contains insights from historical model drift detection data.
/// </summary>
internal sealed record DriftAnalysis
{
    /// <summary>
    /// Time period analyzed.
    /// </summary>
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; init; }

    /// <summary>
    /// Total number of drift events analyzed.
    /// </summary>
    public required int TotalDriftEvents { get; init; }

    /// <summary>
    /// Drift detection signatures and their effectiveness.
    /// </summary>
    public required IReadOnlyDictionary<string, DriftSignature> DriftSignatures { get; init; }

    /// <summary>
    /// Severity distribution of detected drift.
    /// </summary>
    public required IReadOnlyDictionary<string, int> SeverityDistribution { get; init; }

    /// <summary>
    /// Common drift patterns by client or context.
    /// </summary>
    public required IReadOnlyList<DriftPattern> CommonPatterns { get; init; }

    /// <summary>
    /// Recommended thresholds for drift detection.
    /// </summary>
    public IReadOnlyDictionary<string, double>? RecommendedThresholds { get; init; }

    /// <summary>
    /// Effectiveness of current drift detection methods.
    /// </summary>
    public required DriftDetectionEffectiveness DetectionEffectiveness { get; init; }
}

/// <summary>
/// Signature for detecting specific types of model drift.
/// </summary>
internal sealed record DriftSignature
{
    public required string DriftType { get; init; }
    public required IReadOnlyList<string> KeywordTriggers { get; init; }
    public required IReadOnlyList<string> PatternTriggers { get; init; }
    public required double SeverityThreshold { get; init; }
    public required double DetectionAccuracy { get; init; }
    public required int FrequencyCount { get; init; }
}

/// <summary>
/// Common pattern of model drift occurrence.
/// </summary>
internal sealed record DriftPattern
{
    public required string PatternName { get; init; }
    public required IReadOnlyList<string> TriggerSequence { get; init; }
    public required double AverageSeverity { get; init; }
    public required int Frequency { get; init; }
    public required IReadOnlyList<string> RelevantConstraints { get; init; }
    public required double PreventionSuccessRate { get; init; }
}

/// <summary>
/// Overall effectiveness of drift detection system.
/// </summary>
internal sealed record DriftDetectionEffectiveness
{
    public required double OverallAccuracy { get; init; }
    public required double FalsePositiveRate { get; init; }
    public required double FalseNegativeRate { get; init; }
    public required double AverageDetectionLatencyMs { get; init; }
    public required double TruePositiveRate { get; init; }
    public required double TrueNegativeRate { get; init; }
}