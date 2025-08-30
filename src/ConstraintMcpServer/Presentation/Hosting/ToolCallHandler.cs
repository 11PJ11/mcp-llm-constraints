using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ConstraintMcpServer.Application.Injection;
using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Context;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Infrastructure.Logging;

namespace ConstraintMcpServer.Presentation.Hosting;

/// <summary>
/// Handles MCP tools/call requests with context-aware constraint injection.
/// Integrates context analysis, constraint selection, and injection into the MCP request pipeline.
/// </summary>
public sealed class ToolCallHandler : IMcpCommandHandler
{
    private const string ErrorMessagePrefix = "Error during constraint injection: ";

    private readonly ConstraintSelector _selector;
    private readonly Injector _injector;
    private readonly IStructuredEventLogger _logger;
    private readonly IReadOnlyList<Constraint> _constraints;
    private readonly ContextAnalyzer _contextAnalyzer;
    private readonly TriggerMatchingEngine _triggerMatchingEngine;
    private int _currentInteractionNumber = 0;

    /// <summary>
    /// Initializes a new ToolCallHandler with context analysis capabilities.
    /// </summary>
    /// <param name="contextAnalyzer">The context analyzer to extract development context from MCP calls.</param>
    /// <param name="triggerMatchingEngine">The trigger matching engine for intelligent constraint activation.</param>
    /// <param name="logger">The structured event logger for NDJSON event emission.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public ToolCallHandler(ContextAnalyzer contextAnalyzer, TriggerMatchingEngine triggerMatchingEngine, IStructuredEventLogger logger)
    {
        _contextAnalyzer = contextAnalyzer ?? throw new ArgumentNullException(nameof(contextAnalyzer));
        _triggerMatchingEngine = triggerMatchingEngine ?? throw new ArgumentNullException(nameof(triggerMatchingEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _selector = new ConstraintSelector();
        _injector = new Injector();

        // Load constraints for walking skeleton using domain factory
        _constraints = ConstraintFactory.CreateWalkingSkeletonConstraints();
    }

    /// <summary>
    /// Handles a tools/call MCP request with context-aware constraint injection.
    /// 
    /// Logic:
    /// 1. Increment interaction count
    /// 2. Analyze context from the MCP request
    /// 3. Return response with or without constraints based on context analysis
    /// 4. Emit structured logging events for offline analysis
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
            return await CreateContextAwareResponseAsync(requestId, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(_currentInteractionNumber, $"{ErrorMessagePrefix}{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Determines response type based on context analysis and handles logging.
    /// </summary>
    /// <param name="requestId">The JSON-RPC request ID.</param>
    /// <param name="request">The JSON-RPC request element for context analysis.</param>
    /// <returns>JSON-RPC response based on context analysis.</returns>
    private async Task<object> CreateContextAwareResponseAsync(int requestId, JsonElement request)
    {
        // Use context analysis for intelligent constraint activation
        var activatedConstraints = await GetActivatedConstraintsAsync(request);

        if (activatedConstraints.Count > 0)
        {
            return CreateContextAwareConstraintResponse(requestId, activatedConstraints);
        }
        else
        {
            _logger.LogPassThrough(_currentInteractionNumber, "no_context_match");
            return CreateStandardResponse(requestId, hasContextAnalysis: true);
        }
    }

    /// <summary>
    /// Gets activated constraints based on MCP context analysis.
    /// </summary>
    /// <param name="request">The JSON-RPC request element for context analysis.</param>
    /// <returns>List of activated constraints from context analysis.</returns>
    private async Task<IReadOnlyList<ConstraintActivation>> GetActivatedConstraintsAsync(JsonElement request)
    {
        try
        {
            var (methodName, parameters) = ExtractMcpToolCallParameters(request);
            var context = _contextAnalyzer.AnalyzeToolCallContext(methodName, parameters, "session");
            return await _triggerMatchingEngine.EvaluateConstraints(context);
        }
        catch
        {
            return Array.Empty<ConstraintActivation>();
        }
    }

    /// <summary>
    /// Extracts method name and parameters from MCP tool call request.
    /// Supports both standard MCP format and E2E test format.
    /// </summary>
    /// <param name="request">The JSON-RPC request element.</param>
    /// <returns>Tuple containing method name and parameters array.</returns>
    private static (string methodName, object[] parameters) ExtractMcpToolCallParameters(JsonElement request)
    {
        if (!request.TryGetProperty("params", out JsonElement paramsElement))
        {
            return ("", Array.Empty<object>());
        }

        // Check if this is standard MCP format (has 'name' and 'arguments' properties)
        if (paramsElement.TryGetProperty("name", out JsonElement nameElement))
        {
            return ExtractStandardMcpFormat(paramsElement, nameElement);
        }

        // Handle E2E test format (direct properties in params)
        return ExtractE2ETestFormat(paramsElement);
    }

    /// <summary>
    /// Extracts parameters from standard MCP tools/call format.
    /// </summary>
    private static (string methodName, object[] parameters) ExtractStandardMcpFormat(JsonElement paramsElement, JsonElement nameElement)
    {
        string methodName = nameElement.GetString() ?? "";

        var parameters = Array.Empty<object>();
        if (paramsElement.TryGetProperty("arguments", out JsonElement argsElement))
        {
            var argsList = new List<object>();
            if (argsElement.TryGetProperty("file_path", out JsonElement filePathElement))
            {
                argsList.Add(filePathElement.GetString() ?? "");
            }
            if (argsElement.TryGetProperty("content", out JsonElement contentElement))
            {
                argsList.Add(contentElement.GetString() ?? "");
            }
            if (argsElement.TryGetProperty("test_type", out JsonElement testTypeElement))
            {
                argsList.Add(testTypeElement.GetString() ?? "");
            }
            parameters = argsList.ToArray();
        }

        return (methodName, parameters);
    }

    /// <summary>
    /// Extracts parameters from E2E test format (direct properties).
    /// </summary>
    private static (string methodName, object[] parameters) ExtractE2ETestFormat(JsonElement paramsElement)
    {
        string methodName = "tools/call";
        var argsList = new List<object>();

        // Extract filePath
        if (paramsElement.TryGetProperty("filePath", out JsonElement filePathElement))
        {
            argsList.Add(filePathElement.GetString() ?? "");
        }

        // Extract context as content
        if (paramsElement.TryGetProperty("context", out JsonElement contextElement))
        {
            argsList.Add(contextElement.GetString() ?? "");
        }

        // Extract sessionId (though it will be filtered out by ContextAnalyzer)
        if (paramsElement.TryGetProperty("sessionId", out JsonElement sessionIdElement))
        {
            argsList.Add(sessionIdElement.GetString() ?? "");
        }

        return (methodName, argsList.ToArray());
    }


    /// <summary>
    /// Creates a JSON-RPC response with context-aware constraint activation.
    /// Uses activated constraints from TriggerMatchingEngine instead of walking skeleton constraints.
    /// </summary>
    /// <param name="requestId">The JSON-RPC request ID.</param>
    /// <param name="activatedConstraints">Constraints activated by context analysis.</param>
    /// <returns>JSON-RPC response with context-aware constraint message.</returns>
    private object CreateContextAwareConstraintResponse(int requestId, IReadOnlyList<ConstraintActivation> activatedConstraints)
    {
        // Extract constraint IDs for logging
        string[] constraintIds = activatedConstraints.Select(a => a.ConstraintId).ToArray();

        _logger.LogConstraintInjection(_currentInteractionNumber, "context_aware", constraintIds, "context_analysis");

        // Create message using activated constraints
        string constraintMessage = $"🎯 CONSTRAINT GUIDANCE (Interaction {_currentInteractionNumber}) - Context-Aware Activation\n\n";

        foreach (var activation in activatedConstraints)
        {
            constraintMessage += $"• {activation.ConstraintId} (Confidence: {activation.ConfidenceScore:F2})\n";
            constraintMessage += $"  Reason: {activation.Reason}\n\n";
        }

        constraintMessage += "💡 These constraints were selected based on your current development context.";

        return CreateJsonRpcResponse(requestId, constraintMessage, hasContextAnalysis: true, hasActivation: true, constraintCount: activatedConstraints.Count);
    }

    /// <summary>
    /// Creates a standard JSON-RPC response without constraint injection.
    /// </summary>
    /// <param name="requestId">The JSON-RPC request ID.</param>
    /// <param name="hasContextAnalysis">Whether context analysis is enabled.</param>
    /// <returns>Standard JSON-RPC response.</returns>
    private object CreateStandardResponse(int requestId, bool hasContextAnalysis = false)
    {
        string standardMessage = $"Tool call {_currentInteractionNumber} processed.";
        return CreateJsonRpcResponse(requestId, standardMessage, hasContextAnalysis, hasActivation: false, constraintCount: 0);
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
    /// Creates a JSON-RPC response with context analysis information.
    /// </summary>
    /// <param name="requestId">The JSON-RPC request ID.</param>
    /// <param name="message">The content message to include.</param>
    /// <param name="hasContextAnalysis">Whether context analysis is enabled.</param>
    /// <param name="hasActivation">Whether constraints were activated.</param>
    /// <param name="constraintCount">The number of constraints activated (default 0).</param>
    /// <returns>Formatted JSON-RPC response object with context analysis.</returns>
    private static object CreateJsonRpcResponse(int requestId, string message, bool hasContextAnalysis, bool hasActivation, int constraintCount = 0)
    {
        var result = new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = message
                }
            }
        };

        if (hasContextAnalysis)
        {
            return new
            {
                jsonrpc = "2.0",
                id = requestId,
                result = new
                {
                    content = result.content,
                    context_analysis = new
                    {
                        has_activation = hasActivation,
                        constraint_count = constraintCount
                    }
                }
            };
        }

        return new
        {
            jsonrpc = "2.0",
            id = requestId,
            result = result
        };
    }

}
