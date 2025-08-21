namespace ConstraintMcpServer.Domain.Learning;

/// <summary>
/// Domain interface for exporting learned weights and patterns.
/// Part of the Learning bounded context for ML model training.
/// </summary>
internal interface IWeightsExporter
{
    /// <summary>
    /// Exports constraint effectiveness weights based on historical data.
    /// </summary>
    /// <param name="analysis">Analysis results from log data</param>
    /// <param name="outputPath">Path to export weights file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExportConstraintWeightsAsync(ConstraintAnalysis analysis, string outputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports phase detection patterns and signals.
    /// </summary>
    /// <param name="analysis">Analysis results from log data</param>
    /// <param name="outputPath">Path to export patterns file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExportPhaseDetectionPatternsAsync(PhaseAnalysis analysis, string outputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports drift detection signatures and thresholds.
    /// </summary>
    /// <param name="analysis">Analysis results from log data</param>
    /// <param name="outputPath">Path to export signatures file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExportDriftDetectionSignaturesAsync(DriftAnalysis analysis, string outputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports consolidated model for improved constraint enforcement.
    /// </summary>
    /// <param name="consolidatedAnalysis">Combined analysis from all domains</param>
    /// <param name="outputPath">Path to export model file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExportConsolidatedModelAsync(ConsolidatedAnalysis consolidatedAnalysis, string outputPath, CancellationToken cancellationToken = default);
}