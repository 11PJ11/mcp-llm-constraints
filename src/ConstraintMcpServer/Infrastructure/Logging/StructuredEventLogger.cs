using System;
using System.Linq;
using System.Text.Json;
using Serilog;

namespace ConstraintMcpServer.Infrastructure.Logging;

/// <summary>
/// Structured event logger for constraint enforcement operations.
/// Emits NDJSON events for offline analysis and debugging.
/// </summary>
public sealed class StructuredEventLogger : IStructuredEventLogger
{
    private static readonly ILogger Logger = LoggingConfiguration.CreateLogger();

    /// <summary>
    /// Logs a constraint injection event with structured data.
    /// </summary>
    /// <param name="interactionNumber">Current interaction number.</param>
    /// <param name="phase">TDD phase context.</param>
    /// <param name="constraintIds">Selected constraint IDs.</param>
    /// <param name="reason">Reason for injection.</param>
    public void LogConstraintInjection(int interactionNumber, string phase, string[] constraintIds, string reason)
    {
        var constraintEvent = new ConstraintInjectionEvent
        {
            InteractionNumber = interactionNumber,
            Phase = phase,
            SelectedConstraintIds = constraintIds,
            Reason = reason
        };

        string json = JsonSerializer.Serialize(constraintEvent);
        Logger.Information("Constraint injection: {@ConstraintEvent}", json);
    }

    /// <summary>
    /// Logs a pass-through event with structured data.
    /// </summary>
    /// <param name="interactionNumber">Current interaction number.</param>
    /// <param name="reason">Reason for pass-through.</param>
    public void LogPassThrough(int interactionNumber, string reason)
    {
        var passThroughEvent = new PassThroughEvent
        {
            InteractionNumber = interactionNumber,
            Reason = reason
        };

        string json = JsonSerializer.Serialize(passThroughEvent);
        Logger.Information("Pass through: {@PassThroughEvent}", json);
    }

    /// <summary>
    /// Logs an error event with structured data.
    /// </summary>
    /// <param name="interactionNumber">Current interaction number.</param>
    /// <param name="errorMessage">Error message.</param>
    public void LogError(int interactionNumber, string errorMessage)
    {
        var errorEvent = new ErrorEvent
        {
            InteractionNumber = interactionNumber,
            ErrorMessage = errorMessage
        };

        string json = JsonSerializer.Serialize(errorEvent);
        Logger.Error("Constraint error: {@ErrorEvent}", json);
    }
}
