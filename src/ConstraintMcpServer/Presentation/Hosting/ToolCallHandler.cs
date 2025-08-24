using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ConstraintMcpServer.Application.Injection;
using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Application.Scheduling;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Infrastructure.Logging;

namespace ConstraintMcpServer.Presentation.Hosting;

/// <summary>
/// Handles MCP tools/call requests with constraint injection based on scheduler decisions.
/// Integrates the deterministic scheduler, constraint selection, and injection into the MCP request pipeline.
/// </summary>
public sealed class ToolCallHandler : IMcpCommandHandler
{
    private const string ScheduledInjectionReason = "scheduled";
    private const string NotScheduledReason = "not_scheduled";
    private const string ErrorMessagePrefix = "Error during constraint injection: ";

    private readonly Scheduler _scheduler;
    private readonly ConstraintSelector _selector;
    private readonly Injector _injector;
    private readonly IStructuredEventLogger _logger;
    private readonly IReadOnlyList<Constraint> _constraints;
    private int _currentInteractionNumber = 0;
    private static readonly Phase WalkingSkeletonPhase = new("red");

    /// <summary>
    /// Initializes a new ToolCallHandler with the specified scheduler and logger.
    /// </summary>
    /// <param name="scheduler">The scheduler to determine when to inject constraints.</param>
    /// <param name="logger">The structured event logger for NDJSON event emission.</param>
    /// <exception cref="ArgumentNullException">Thrown when scheduler or logger is null.</exception>
    public ToolCallHandler(Scheduler scheduler, IStructuredEventLogger logger)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _selector = new ConstraintSelector();
        _injector = new Injector();

        // Load constraints for walking skeleton using domain factory
        _constraints = ConstraintFactory.CreateWalkingSkeletonConstraints();
    }

    /// <summary>
    /// Initializes a new ToolCallHandler with the specified scheduler (backwards compatibility).
    /// Uses default StructuredEventLogger for logging.
    /// </summary>
    /// <param name="scheduler">The scheduler to determine when to inject constraints.</param>
    /// <exception cref="ArgumentNullException">Thrown when scheduler is null.</exception>
    public ToolCallHandler(Scheduler scheduler) : this(scheduler, new StructuredEventLogger())
    {
    }

    /// <summary>
    /// Handles a tools/call MCP request with scheduler-driven constraint injection.
    /// 
    /// Logic:
    /// 1. Increment interaction count
    /// 2. Ask scheduler if constraints should be injected
    /// 3. Return response with or without constraints based on scheduler decision
    /// 4. Emit structured logging events for offline analysis
    /// 5. Include "CONSTRAINT" marker in response when injecting for E2E test detection
    /// </summary>
    /// <param name="requestId">The JSON-RPC request ID.</param>
    /// <param name="request">The JSON-RPC request element.</param>
    /// <returns>JSON-RPC response object with optional constraint injection.</returns>
    public async Task<object> HandleAsync(int requestId, JsonElement request)
    {
        // Increment interaction count for walking skeleton
        _currentInteractionNumber++;

        // Satisfy async requirement (for future async operations)
        await Task.CompletedTask;

        try
        {
            return CreateResponseBasedOnSchedule(requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(_currentInteractionNumber, $"{ErrorMessagePrefix}{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Determines response type based on scheduler decision and handles logging.
    /// </summary>
    /// <param name="requestId">The JSON-RPC request ID.</param>
    /// <returns>JSON-RPC response based on scheduler decision.</returns>
    private object CreateResponseBasedOnSchedule(int requestId)
    {
        bool shouldInject = _scheduler.ShouldInject(_currentInteractionNumber);

        if (shouldInject)
        {
            return CreateConstraintResponseWithLogging(requestId);
        }
        else
        {
            _logger.LogPassThrough(_currentInteractionNumber, NotScheduledReason);
            return CreateStandardResponse(requestId);
        }
    }

    /// <summary>
    /// Creates a JSON-RPC response with constraint injection and logs the event.
    /// Uses constraint selection and injection for prioritized constraint delivery.
    /// </summary>
    /// <param name="requestId">The JSON-RPC request ID.</param>
    /// <returns>JSON-RPC response with constraint message.</returns>
    private object CreateConstraintResponseWithLogging(int requestId)
    {
        // Select top-K constraints by priority for current phase
        IReadOnlyList<Constraint> selectedConstraints = _selector.SelectConstraints(_constraints, WalkingSkeletonPhase, InjectionConfiguration.MaxConstraintsPerInjection);

        // Extract constraint IDs for logging
        string[] constraintIds = selectedConstraints.Select(c => c.Id.Value).ToArray();

        _logger.LogConstraintInjection(_currentInteractionNumber, WalkingSkeletonPhase.Value, constraintIds, ScheduledInjectionReason);

        // Format constraint message with anchors and reminders
        string constraintMessage = _injector.FormatConstraintMessage(selectedConstraints, _currentInteractionNumber);

        return CreateJsonRpcResponse(requestId, constraintMessage);
    }

    /// <summary>
    /// Creates a standard JSON-RPC response without constraint injection.
    /// </summary>
    /// <param name="requestId">The JSON-RPC request ID.</param>
    /// <returns>Standard JSON-RPC response.</returns>
    private object CreateStandardResponse(int requestId)
    {
        string standardMessage = $"Tool call {_currentInteractionNumber} processed.";
        return CreateJsonRpcResponse(requestId, standardMessage);
    }

    /// <summary>
    /// Creates a JSON-RPC response with the specified content message.
    /// Eliminates response structure duplication.
    /// </summary>
    /// <param name="requestId">The JSON-RPC request ID.</param>
    /// <param name="message">The content message to include.</param>
    /// <returns>Formatted JSON-RPC response object.</returns>
    private static object CreateJsonRpcResponse(int requestId, string message)
    {
        return new
        {
            jsonrpc = "2.0",
            id = requestId,
            result = new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = message
                    }
                }
            }
        };
    }

}
