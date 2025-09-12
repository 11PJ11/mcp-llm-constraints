namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Represents historical accuracy metrics for time estimations over multiple updates.
/// Business value: Enables continuous improvement of time estimation accuracy.
/// </summary>
public sealed record HistoricalAccuracyMetrics
{
    /// <summary>
    /// Overall accuracy percentage across all historical estimations.
    /// </summary>
    public double OverallAccuracyPercentage { get; init; }

    /// <summary>
    /// Total number of estimations in the historical dataset.
    /// </summary>
    public int TotalEstimations { get; init; }

    /// <summary>
    /// Number of estimations that were considered accurate.
    /// </summary>
    public int AccurateEstimations { get; init; }

    /// <summary>
    /// Average absolute error across all estimations.
    /// </summary>
    public TimeSpan AverageAbsoluteError { get; init; }

    /// <summary>
    /// Standard deviation of estimation errors.
    /// </summary>
    public double StandardDeviationSeconds { get; init; }

    /// <summary>
    /// Best (most accurate) estimation error percentage.
    /// </summary>
    public double BestAccuracyPercentage { get; init; }

    /// <summary>
    /// Worst (least accurate) estimation error percentage.
    /// </summary>
    public double WorstAccuracyPercentage { get; init; }

    /// <summary>
    /// Trend in accuracy over time (improving, stable, declining).
    /// </summary>
    public string AccuracyTrend { get; init; } = string.Empty;

    /// <summary>
    /// Date range covered by the historical data.
    /// </summary>
    public DateTimeOffset AnalysisStartDate { get; init; }

    /// <summary>
    /// End date of the historical analysis.
    /// </summary>
    public DateTimeOffset AnalysisEndDate { get; init; }

    /// <summary>
    /// Timestamp when this analysis was generated.
    /// </summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>
    /// Creates historical accuracy metrics.
    /// </summary>
    public static HistoricalAccuracyMetrics Create(
        double overallAccuracyPercentage,
        int totalEstimations,
        int accurateEstimations,
        TimeSpan averageAbsoluteError,
        double standardDeviationSeconds,
        double bestAccuracy,
        double worstAccuracy,
        string accuracyTrend,
        DateTimeOffset startDate,
        DateTimeOffset endDate) =>
        new()
        {
            OverallAccuracyPercentage = overallAccuracyPercentage,
            TotalEstimations = totalEstimations,
            AccurateEstimations = accurateEstimations,
            AverageAbsoluteError = averageAbsoluteError,
            StandardDeviationSeconds = standardDeviationSeconds,
            BestAccuracyPercentage = bestAccuracy,
            WorstAccuracyPercentage = worstAccuracy,
            AccuracyTrend = accuracyTrend,
            AnalysisStartDate = startDate,
            AnalysisEndDate = endDate,
            GeneratedAt = DateTime.UtcNow
        };
}
