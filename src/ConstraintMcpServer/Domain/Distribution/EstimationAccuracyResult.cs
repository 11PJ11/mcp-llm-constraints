namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Represents the accuracy of time estimation compared to actual measured completion time.
/// Business value: Validates estimation quality and enables continuous improvement of time predictions.
/// </summary>
public sealed record EstimationAccuracyResult
{
    /// <summary>
    /// Whether the estimation was considered accurate within acceptable thresholds.
    /// </summary>
    public bool IsAccurate { get; init; }

    /// <summary>
    /// Overall accuracy percentage (0.0 to 100.0).
    /// </summary>
    public double AccuracyPercentage { get; init; }

    /// <summary>
    /// Initial time estimation that was made.
    /// </summary>
    public TimeSpan InitialEstimate { get; init; }

    /// <summary>
    /// Actual measured completion time.
    /// </summary>
    public TimeSpan FinalActualTime { get; init; }

    /// <summary>
    /// Percentage error between estimated and actual time (can be positive or negative).
    /// </summary>
    public double EstimationErrorPercentage { get; init; }

    /// <summary>
    /// Absolute difference between estimated and actual time.
    /// </summary>
    public TimeSpan AbsoluteDifference { get; init; }

    /// <summary>
    /// Number of measurements used during accuracy validation.
    /// </summary>
    public int MeasurementCount { get; init; }

    /// <summary>
    /// Validation method used for accuracy assessment.
    /// </summary>
    public string ValidationMethod { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp when accuracy validation was performed.
    /// </summary>
    public DateTime ValidatedAt { get; init; }

    /// <summary>
    /// Duration of the accuracy validation process.
    /// </summary>
    public TimeSpan ValidationDuration { get; init; }

    /// <summary>
    /// Additional details about the accuracy assessment.
    /// </summary>
    public string Details { get; init; } = string.Empty;

    /// <summary>
    /// Creates an accurate estimation result.
    /// </summary>
    public static EstimationAccuracyResult Accurate(
        double accuracyPercentage,
        TimeSpan initialEstimate,
        TimeSpan finalActualTime,
        int measurementCount,
        TimeSpan validationDuration,
        string validationMethod = "Real-time comparison") =>
        new()
        {
            IsAccurate = true,
            AccuracyPercentage = accuracyPercentage,
            InitialEstimate = initialEstimate,
            FinalActualTime = finalActualTime,
            EstimationErrorPercentage = CalculateErrorPercentage(initialEstimate, finalActualTime),
            AbsoluteDifference = TimeSpan.FromMilliseconds(Math.Abs((initialEstimate - finalActualTime).TotalMilliseconds)),
            MeasurementCount = measurementCount,
            ValidationMethod = validationMethod,
            ValidatedAt = DateTime.UtcNow,
            ValidationDuration = validationDuration,
            Details = $"Estimation accuracy validated with {measurementCount} measurements over {validationDuration.TotalSeconds:F1} seconds"
        };

    /// <summary>
    /// Creates an inaccurate estimation result.
    /// </summary>
    public static EstimationAccuracyResult Inaccurate(
        double accuracyPercentage,
        TimeSpan initialEstimate,
        TimeSpan finalActualTime,
        int measurementCount,
        TimeSpan validationDuration,
        string reason,
        string validationMethod = "Real-time comparison") =>
        new()
        {
            IsAccurate = false,
            AccuracyPercentage = accuracyPercentage,
            InitialEstimate = initialEstimate,
            FinalActualTime = finalActualTime,
            EstimationErrorPercentage = CalculateErrorPercentage(initialEstimate, finalActualTime),
            AbsoluteDifference = TimeSpan.FromMilliseconds(Math.Abs((initialEstimate - finalActualTime).TotalMilliseconds)),
            MeasurementCount = measurementCount,
            ValidationMethod = validationMethod,
            ValidatedAt = DateTime.UtcNow,
            ValidationDuration = validationDuration,
            Details = $"Estimation inaccurate: {reason}"
        };

    /// <summary>
    /// Calculates error percentage between estimated and actual time.
    /// </summary>
    private static double CalculateErrorPercentage(TimeSpan estimated, TimeSpan actual)
    {
        if (actual.TotalMilliseconds == 0)
        {
            return 0.0;
        }

        return ((estimated.TotalMilliseconds - actual.TotalMilliseconds) / actual.TotalMilliseconds) * 100.0;
    }
}
