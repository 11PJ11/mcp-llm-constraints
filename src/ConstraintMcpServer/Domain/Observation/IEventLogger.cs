namespace ConstraintMcpServer.Domain.Observation;

using ConstraintMcpServer.Domain.Enforcement;

/// <summary>
/// Domain interface for logging constraint enforcement events.
/// Part of the Observation bounded context.
/// </summary>
internal interface IEventLogger
{
    /// <summary>
    /// Logs a constraint injection event.
    /// </summary>
    /// <param name="event">Constraint injection event details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task LogConstraintInjectionAsync(ConstraintInjectionEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a phase transition event.
    /// </summary>
    /// <param name="event">Phase transition event details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task LogPhaseTransitionAsync(PhaseTransitionEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a drift detection event.
    /// </summary>
    /// <param name="event">Drift detection event details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task LogDriftDetectionAsync(DriftDetectionEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a performance measurement event.
    /// </summary>
    /// <param name="event">Performance measurement details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task LogPerformanceAsync(PerformanceEvent @event, CancellationToken cancellationToken = default);
}