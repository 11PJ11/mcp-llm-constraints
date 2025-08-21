namespace ConstraintMcpServer.Domain.Learning;

using ConstraintMcpServer.Domain.Observation;

/// <summary>
/// Domain interface for reading structured constraint enforcement logs.
/// Part of the Learning bounded context for offline analysis.
/// </summary>
internal interface ILogReader
{
    /// <summary>
    /// Reads constraint injection events from logs within a time range.
    /// </summary>
    /// <param name="startTime">Start of time range</param>
    /// <param name="endTime">End of time range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sequence of constraint injection events</returns>
    IAsyncEnumerable<ConstraintInjectionEvent> ReadInjectionEventsAsync(
        DateTimeOffset startTime, 
        DateTimeOffset endTime, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads phase transition events from logs within a time range.
    /// </summary>
    /// <param name="startTime">Start of time range</param>
    /// <param name="endTime">End of time range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sequence of phase transition events</returns>
    IAsyncEnumerable<PhaseTransitionEvent> ReadPhaseTransitionEventsAsync(
        DateTimeOffset startTime, 
        DateTimeOffset endTime, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads drift detection events from logs within a time range.
    /// </summary>
    /// <param name="startTime">Start of time range</param>
    /// <param name="endTime">End of time range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sequence of drift detection events</returns>
    IAsyncEnumerable<DriftDetectionEvent> ReadDriftDetectionEventsAsync(
        DateTimeOffset startTime, 
        DateTimeOffset endTime, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads performance events from logs within a time range.
    /// </summary>
    /// <param name="startTime">Start of time range</param>
    /// <param name="endTime">End of time range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sequence of performance events</returns>
    IAsyncEnumerable<PerformanceEvent> ReadPerformanceEventsAsync(
        DateTimeOffset startTime, 
        DateTimeOffset endTime, 
        CancellationToken cancellationToken = default);
}