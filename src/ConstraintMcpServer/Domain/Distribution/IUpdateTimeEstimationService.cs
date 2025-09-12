namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Service for estimating and validating update completion times based on real measurements.
/// Business value: Provides reliable time estimates that help users plan their work effectively.
/// </summary>
public interface IUpdateTimeEstimationService
{
    /// <summary>
    /// Estimates the time required to complete an update based on historical data and current system state.
    /// </summary>
    /// <param name="updateOptions">Configuration options for the update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Time estimation result with confidence intervals</returns>
    Task<UpdateTimeEstimation> EstimateUpdateTimeAsync(UpdateOptions updateOptions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates estimation accuracy by comparing predicted vs actual completion times.
    /// </summary>
    /// <param name="updateId">Unique identifier for the update to validate</param>
    /// <param name="validationDuration">How long to run accuracy validation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Accuracy validation result with measurement-based analysis</returns>
    Task<EstimationAccuracyResult> ValidateEstimationAccuracyAsync(string updateId, TimeSpan validationDuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records actual update completion time to improve future estimates.
    /// </summary>
    /// <param name="updateOptions">Update configuration that was used</param>
    /// <param name="actualDuration">Measured completion time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if measurement was successfully recorded</returns>
    Task<bool> RecordActualUpdateTimeAsync(UpdateOptions updateOptions, TimeSpan actualDuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets historical estimation accuracy metrics for continuous improvement.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Historical accuracy analysis</returns>
    Task<HistoricalAccuracyMetrics> GetHistoricalAccuracyAsync(CancellationToken cancellationToken = default);
}
