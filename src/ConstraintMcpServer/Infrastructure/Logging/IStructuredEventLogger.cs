namespace ConstraintMcpServer.Infrastructure.Logging;

/// <summary>
/// Interface for structured event logging operations.
/// Defines contract for emitting NDJSON events for constraint enforcement analysis.
/// </summary>
public interface IStructuredEventLogger
{
    /// <summary>
    /// Logs a constraint injection event with structured data.
    /// </summary>
    /// <param name="interactionNumber">Current interaction number.</param>
    /// <param name="phase">TDD phase context.</param>
    /// <param name="constraintIds">Selected constraint IDs.</param>
    /// <param name="reason">Reason for injection.</param>
    void LogConstraintInjection(int interactionNumber, string phase, string[] constraintIds, string reason);

    /// <summary>
    /// Logs a pass-through event with structured data.
    /// </summary>
    /// <param name="interactionNumber">Current interaction number.</param>
    /// <param name="reason">Reason for pass-through.</param>
    void LogPassThrough(int interactionNumber, string reason);

    /// <summary>
    /// Logs an error event with structured data.
    /// </summary>
    /// <param name="interactionNumber">Current interaction number.</param>
    /// <param name="errorMessage">Error message.</param>
    void LogError(int interactionNumber, string errorMessage);
}
