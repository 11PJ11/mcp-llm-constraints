namespace ConstraintMcpServer.Domain.Observation;

/// <summary>
/// Domain interface for capturing constraint enforcement metrics.
/// Part of the Observation bounded context.
/// </summary>
internal interface IMetrics
{
    /// <summary>
    /// Records the latency of a constraint enforcement operation.
    /// </summary>
    /// <param name="operation">Operation name</param>
    /// <param name="latencyMs">Latency in milliseconds</param>
    void RecordLatency(string operation, double latencyMs);

    /// <summary>
    /// Increments a counter metric.
    /// </summary>
    /// <param name="metric">Metric name</param>
    /// <param name="tags">Optional tags for metric dimensions</param>
    void IncrementCounter(string metric, IDictionary<string, string>? tags = null);

    /// <summary>
    /// Records a gauge value.
    /// </summary>
    /// <param name="metric">Metric name</param>
    /// <param name="value">Gauge value</param>
    /// <param name="tags">Optional tags for metric dimensions</param>
    void RecordGauge(string metric, double value, IDictionary<string, string>? tags = null);

    /// <summary>
    /// Records a histogram value.
    /// </summary>
    /// <param name="metric">Metric name</param>
    /// <param name="value">Histogram value</param>
    /// <param name="tags">Optional tags for metric dimensions</param>
    void RecordHistogram(string metric, double value, IDictionary<string, string>? tags = null);

    /// <summary>
    /// Creates a timer for measuring operation duration.
    /// </summary>
    /// <param name="operation">Operation name</param>
    /// <param name="tags">Optional tags for metric dimensions</param>
    /// <returns>Disposable timer that records duration on disposal</returns>
    IDisposable StartTimer(string operation, IDictionary<string, string>? tags = null);
}