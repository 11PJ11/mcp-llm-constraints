namespace ConstraintMcpServer.Domain.Observation;

using ConstraintMcpServer.Domain.Enforcement;

/// <summary>
/// Domain event representing performance measurements.
/// Contains detailed performance data for monitoring and optimization.
/// </summary>
internal sealed record PerformanceEvent
{
    /// <summary>
    /// Unique identifier for this performance event.
    /// </summary>
    public required string EventId { get; init; }

    /// <summary>
    /// Timestamp when the measurement was taken.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Interaction context for this measurement.
    /// </summary>
    public required InteractionContext Context { get; init; }

    /// <summary>
    /// Operation that was measured.
    /// </summary>
    public required string Operation { get; init; }

    /// <summary>
    /// Total operation latency in milliseconds.
    /// </summary>
    public required double TotalLatencyMs { get; init; }

    /// <summary>
    /// Breakdown of latency by component (scheduling, selection, injection, etc.).
    /// </summary>
    public IReadOnlyDictionary<string, double>? LatencyBreakdown { get; init; }

    /// <summary>
    /// Memory usage in bytes during the operation.
    /// </summary>
    public long? MemoryUsageBytes { get; init; }

    /// <summary>
    /// CPU usage percentage during the operation.
    /// </summary>
    public double? CpuUsagePercent { get; init; }

    /// <summary>
    /// Size of input data in bytes.
    /// </summary>
    public int? InputSizeBytes { get; init; }

    /// <summary>
    /// Size of output data in bytes.
    /// </summary>
    public int? OutputSizeBytes { get; init; }
}