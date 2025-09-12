namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Represents a time estimation for an update operation with confidence intervals.
/// Business value: Provides users with reliable time estimates to plan their work effectively.
/// </summary>
public sealed record UpdateTimeEstimation
{
    /// <summary>
    /// Whether the estimation was calculated successfully.
    /// </summary>
    public bool IsSuccessful { get; init; }

    /// <summary>
    /// Primary estimated completion time.
    /// </summary>
    public TimeSpan EstimatedDuration { get; init; }

    /// <summary>
    /// Minimum expected completion time (optimistic scenario).
    /// </summary>
    public TimeSpan MinimumDuration { get; init; }

    /// <summary>
    /// Maximum expected completion time (pessimistic scenario).
    /// </summary>
    public TimeSpan MaximumDuration { get; init; }

    /// <summary>
    /// Confidence level of the estimation (0.0 to 1.0).
    /// </summary>
    public double ConfidenceLevel { get; init; }

    /// <summary>
    /// Number of historical measurements used for this estimation.
    /// </summary>
    public int BasedOnMeasurements { get; init; }

    /// <summary>
    /// Factors that influence the estimation time.
    /// </summary>
    public IReadOnlyList<string> InfluencingFactors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Timestamp when the estimation was calculated.
    /// </summary>
    public DateTime CalculatedAt { get; init; }

    /// <summary>
    /// Error message if estimation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful time estimation.
    /// </summary>
    public static UpdateTimeEstimation Success(
        TimeSpan estimatedDuration,
        TimeSpan minimumDuration,
        TimeSpan maximumDuration,
        double confidenceLevel,
        int basedOnMeasurements,
        IReadOnlyList<string>? influencingFactors = null) =>
        new()
        {
            IsSuccessful = true,
            EstimatedDuration = estimatedDuration,
            MinimumDuration = minimumDuration,
            MaximumDuration = maximumDuration,
            ConfidenceLevel = confidenceLevel,
            BasedOnMeasurements = basedOnMeasurements,
            InfluencingFactors = influencingFactors ?? Array.Empty<string>(),
            CalculatedAt = DateTime.UtcNow
        };

    /// <summary>
    /// Creates a failed time estimation.
    /// </summary>
    public static UpdateTimeEstimation Failure(string errorMessage) =>
        new()
        {
            IsSuccessful = false,
            EstimatedDuration = TimeSpan.Zero,
            MinimumDuration = TimeSpan.Zero,
            MaximumDuration = TimeSpan.Zero,
            ConfidenceLevel = 0.0,
            BasedOnMeasurements = 0,
            ErrorMessage = errorMessage,
            CalculatedAt = DateTime.UtcNow
        };
}
