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
    private const int MaxConstraintsPerInjection = 2; // Optimal attention retention
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
        
        // Load constraints for walking skeleton - will be refactored in future iterations
        _constraints = LoadConstraintsForWalkingSkeleton();
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
        var selectedConstraints = _selector.SelectConstraints(_constraints, WalkingSkeletonPhase, MaxConstraintsPerInjection);
        
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

    /// <summary>
    /// Loads constraints for the walking skeleton implementation.
    /// This will be replaced with proper configuration loading in future iterations.
    /// </summary>
    /// <returns>List of sample constraints for testing.</returns>
    private static IReadOnlyList<Constraint> LoadConstraintsForWalkingSkeleton()
    {
        return new List<Constraint>
        {
            CreateTddConstraint(),
            CreateArchitectureConstraint(),
            CreateYagniConstraint()
        }.AsReadOnly();
    }

    private static Constraint CreateTddConstraint()
    {
        return new Constraint(
            new ConstraintId("tdd.test-first"),
            "Write a failing test first",
            new Priority(0.92),
            new[] { new Phase("kickoff"), new Phase("red"), new Phase("commit") },
            new[] { "Start with a failing test (RED) before implementation.", "Let the test drive the API design and behavior." }
        );
    }

    private static Constraint CreateArchitectureConstraint()
    {
        return new Constraint(
            new ConstraintId("arch.hex.domain-pure"),
            "Domain must not depend on Infrastructure",
            new Priority(0.88),
            new[] { new Phase("red"), new Phase("green"), new Phase("refactor") },
            new[] { "Domain layer: pure business logic, no framework dependencies.", "Use ports (interfaces) to define infrastructure contracts." }
        );
    }

    private static Constraint CreateYagniConstraint()
    {
        return new Constraint(
            new ConstraintId("quality.yagni"),
            "You Aren't Gonna Need It",
            new Priority(0.75),
            new[] { new Phase("green"), new Phase("refactor") },
            new[] { "Implement only what's needed right now.", "Avoid speculative generality and over-engineering." }
        );
    }
}
