namespace ConstraintMcpServer.Domain.Learning;

/// <summary>
/// Domain value object representing analysis results for phase detection patterns.
/// Contains insights from historical phase transition data.
/// </summary>
internal sealed record PhaseAnalysis
{
    /// <summary>
    /// Time period analyzed.
    /// </summary>
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; init; }

    /// <summary>
    /// Total number of phase transitions analyzed.
    /// </summary>
    public required int TotalTransitions { get; init; }

    /// <summary>
    /// Phase detection patterns and their accuracy.
    /// </summary>
    public required IReadOnlyDictionary<string, PhaseDetectionPattern> DetectionPatterns { get; init; }

    /// <summary>
    /// Common phase transition sequences.
    /// </summary>
    public required IReadOnlyList<PhaseSequence> CommonSequences { get; init; }

    /// <summary>
    /// Signal effectiveness for phase detection.
    /// </summary>
    public required IReadOnlyDictionary<string, SignalEffectiveness> SignalMetrics { get; init; }

    /// <summary>
    /// Recommendations for improving phase detection.
    /// </summary>
    public IReadOnlyList<PhaseDetectionRecommendation>? Recommendations { get; init; }
}

/// <summary>
/// Pattern for detecting specific development phases.
/// </summary>
internal sealed record PhaseDetectionPattern
{
    public required string Phase { get; init; }
    public required IReadOnlyList<string> KeywordSignals { get; init; }
    public required IReadOnlyList<string> RegexPatterns { get; init; }
    public required double AccuracyRate { get; init; }
    public required double FalsePositiveRate { get; init; }
    public required double ConfidenceThreshold { get; init; }
}

/// <summary>
/// Common sequence of phase transitions.
/// </summary>
internal sealed record PhaseSequence
{
    public required IReadOnlyList<string> Phases { get; init; }
    public required int Frequency { get; init; }
    public required double AverageDurationMinutes { get; init; }
    public required double SuccessRate { get; init; }
}

/// <summary>
/// Effectiveness of signals for phase detection.
/// </summary>
internal sealed record SignalEffectiveness
{
    public required string Signal { get; init; }
    public required string SignalType { get; init; } // keyword, regex, behavioral
    public required double Precision { get; init; }
    public required double Recall { get; init; }
    public required double F1Score { get; init; }
}

/// <summary>
/// Recommendation for improving phase detection.
/// </summary>
internal sealed record PhaseDetectionRecommendation
{
    public required string Phase { get; init; }
    public required string RecommendationType { get; init; } // add_signal, remove_signal, adjust_threshold
    public required string Details { get; init; }
    public required double ExpectedImprovement { get; init; }
}