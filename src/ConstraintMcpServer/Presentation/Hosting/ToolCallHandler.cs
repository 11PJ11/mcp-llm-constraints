using System;
using System.Text.Json;
using System.Threading.Tasks;
using ConstraintMcpServer.Application.Scheduling;

namespace ConstraintMcpServer.Presentation.Hosting;

/// <summary>
/// Handles MCP tools/call requests with constraint injection based on scheduler decisions.
/// Integrates the deterministic scheduler into the MCP request pipeline.
/// </summary>
public sealed class ToolCallHandler : IMcpCommandHandler
{
    private readonly Scheduler _scheduler;
    private int _currentInteractionNumber = 0; // Session-scoped interaction counter

    /// <summary>
    /// Initializes a new ToolCallHandler with the specified scheduler.
    /// </summary>
    /// <param name="scheduler">The scheduler to determine when to inject constraints.</param>
    /// <exception cref="ArgumentNullException">Thrown when scheduler is null.</exception>
    public ToolCallHandler(Scheduler scheduler)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
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
        if (shouldInject)
        {
            // Response with constraint injection (includes "CONSTRAINT" marker for E2E test detection)
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
                            text = $"Tool call {_currentInteractionNumber} processed. CONSTRAINT: Remember to follow TDD - write failing tests first!"
                        }
                    }
                }
            };
        }
        else
        {
            // Standard response without constraint injection
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
                            text = $"Tool call {_currentInteractionNumber} processed."
                        }
                    }
                }
            };
        }
    }
}
