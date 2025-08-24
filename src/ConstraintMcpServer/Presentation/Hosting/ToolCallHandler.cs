using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ConstraintMcpServer.Application.Injection;
using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Application.Scheduling;
using ConstraintMcpServer.Domain;

namespace ConstraintMcpServer.Presentation.Hosting;

/// <summary>
/// Handles MCP tools/call requests with constraint injection based on scheduler decisions.
/// Integrates the deterministic scheduler, constraint selection, and injection into the MCP request pipeline.
/// </summary>
public sealed class ToolCallHandler : IMcpCommandHandler
{
    private readonly Scheduler _scheduler;
    private readonly ConstraintSelector _selector;
    private readonly Injector _injector;
    private readonly IReadOnlyList<Constraint> _constraints;
    private int _currentInteractionNumber = 0; // Session-scoped interaction counter
    private static readonly Phase WalkingSkeletonPhase = new("red"); // Red phase for TDD focus

    /// <summary>
    /// Initializes a new ToolCallHandler with the specified scheduler.
    /// </summary>
    /// <param name="scheduler">The scheduler to determine when to inject constraints.</param>
    /// <exception cref="ArgumentNullException">Thrown when scheduler is null.</exception>
    public ToolCallHandler(Scheduler scheduler)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _selector = new ConstraintSelector();
        _injector = new Injector();

        // Load constraints for walking skeleton using domain factory
        _constraints = ConstraintFactory.CreateWalkingSkeletonConstraints();
    }

    /// <summary>
    /// Handles a tools/call MCP request with scheduler-driven constraint injection.
    /// 
    /// Logic:
    /// 1. Increment interaction count
    /// 2. Ask scheduler if constraints should be injected
    /// 3. Return response with or without constraints based on scheduler decision
    /// 4. Include "CONSTRAINT" marker in response when injecting for E2E test detection
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

        // Ask scheduler if we should inject constraints for this interaction
        bool shouldInject = _scheduler.ShouldInject(_currentInteractionNumber);

        // Create response based on scheduler decision
        return shouldInject
            ? CreateConstraintResponse(requestId)
            : CreateStandardResponse(requestId);
    }

    /// <summary>
    /// Creates a JSON-RPC response with constraint injection.
    /// Uses constraint selection and injection for prioritized constraint delivery.
    /// </summary>
    /// <param name="requestId">The JSON-RPC request ID.</param>
    /// <returns>JSON-RPC response with constraint message.</returns>
    private object CreateConstraintResponse(int requestId)
    {
        // Select top-K constraints by priority for current phase
        IReadOnlyList<Constraint> selectedConstraints = _selector.SelectConstraints(_constraints, WalkingSkeletonPhase, InjectionConfiguration.MaxConstraintsPerInjection);

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
