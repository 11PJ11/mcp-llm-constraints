using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ConstraintMcpServer.Application.AgentCompliance;
using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Domain.Context;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Domain.Progression;
using ConstraintMcpServer.Infrastructure.Logging;

namespace ConstraintMcpServer.Presentation.Hosting;

/// <summary>
/// Agent Compliance-Enhanced MCP tool call handler that combines traditional constraint enforcement
/// with real-time agent compliance intelligence, drift detection, and adaptive optimization.
/// Implements sub-50ms compliance tracking with proactive intervention capabilities.
/// </summary>
public sealed class AgentComplianceEnhancedToolCallHandler : IMcpCommandHandler
{
    private const string ErrorMessagePrefix = "Error during agent compliance-enhanced constraint injection: ";

    private readonly ToolCallHandler _baseHandler;
    private readonly AgentComplianceTracker _complianceTracker;
    private readonly ConstraintDriftDetector _driftDetector;
    private readonly ConstraintAdaptationEngine _adaptationEngine;
    private readonly IStructuredEventLogger _logger;
    private int _currentInteractionNumber = 0;

    /// <summary>
    /// Initializes the agent compliance-enhanced tool call handler.
    /// Combines traditional constraint enforcement with intelligent agent compliance tracking.
    /// </summary>
    /// <param name="contextAnalyzer">Context analyzer for extracting development context</param>
    /// <param name="triggerMatchingEngine">Engine for intelligent constraint activation</param>
    /// <param name="logger">Structured event logger for compliance monitoring</param>
    /// <param name="complianceTracker">Real-time agent compliance tracker</param>
    /// <param name="driftDetector">Proactive drift detection system</param>
    /// <param name="adaptationEngine">Adaptive constraint optimization engine</param>
    public AgentComplianceEnhancedToolCallHandler(
        ContextAnalyzer contextAnalyzer,
        TriggerMatchingEngine triggerMatchingEngine,
        IStructuredEventLogger logger,
        AgentComplianceTracker complianceTracker,
        ConstraintDriftDetector driftDetector,
        ConstraintAdaptationEngine adaptationEngine)
    {
        _baseHandler = new ToolCallHandler(contextAnalyzer, triggerMatchingEngine, logger);
        _complianceTracker = complianceTracker ?? throw new ArgumentNullException(nameof(complianceTracker));
        _driftDetector = driftDetector ?? throw new ArgumentNullException(nameof(driftDetector));
        _adaptationEngine = adaptationEngine ?? throw new ArgumentNullException(nameof(adaptationEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles MCP tool call request with enhanced agent compliance intelligence.
    /// Integrates traditional constraint enforcement with real-time compliance tracking,
    /// drift detection, and adaptive optimization.
    /// </summary>
    /// <param name="requestId">JSON-RPC request ID</param>
    /// <param name="request">JSON-RPC request element</param>
    /// <returns>Enhanced JSON-RPC response with agent compliance intelligence</returns>
    public async Task<object> HandleAsync(int requestId, JsonElement request)
    {
        _currentInteractionNumber++;

        try
        {
            return await ProcessWithAgentComplianceIntelligence(requestId, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(_currentInteractionNumber, $"{ErrorMessagePrefix}{ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Processes MCP request with full agent compliance intelligence pipeline.
    /// Combines constraint enforcement with compliance tracking, drift detection, and adaptation.
    /// </summary>
    /// <param name="requestId">JSON-RPC request ID</param>
    /// <param name="request">JSON-RPC request element</param>
    /// <returns>Enhanced response with agent compliance insights</returns>
    private async Task<object> ProcessWithAgentComplianceIntelligence(int requestId, JsonElement request)
    {
        var startTime = DateTime.UtcNow;

        // Step 1: Get traditional constraint enforcement response
        var baseResponse = await _baseHandler.HandleAsync(requestId, request);

        // Step 2: Extract constraint interaction data for compliance analysis
        var interaction = ExtractConstraintInteraction(requestId, request, baseResponse);

        // Step 3: Track real-time agent compliance (sub-50ms requirement)
        var complianceAssessment = await _complianceTracker.TrackComplianceAsync(interaction);

        // Step 4: Detect potential drift patterns (sub-25ms requirement)
        var agentHistory = await _complianceTracker.GetComplianceHistoryAsync(interaction.AgentId ?? "default_agent");
        var driftAnalysis = await _driftDetector.DetectComplianceDriftAsync(agentHistory);
        var driftSeverity = await _driftDetector.AssessDriftSeverityAsync(driftAnalysis);

        // Step 5: Apply adaptive constraint optimization if needed (sub-200ms requirement)
        ConstraintRefinementSuggestion? adaptationSuggestion = null;
        if (driftSeverity >= ViolationSeverity.Major)
        {
            adaptationSuggestion = await _driftDetector.TriggerProactiveInterventionAsync(driftSeverity, interaction.AgentId ?? "default_agent");
        }

        // Step 6: Log comprehensive agent compliance event
        LogAgentComplianceEvent(complianceAssessment, driftAnalysis, driftSeverity, adaptationSuggestion);

        // Step 7: Enhance response with agent compliance intelligence
        var enhancedResponse = EnhanceResponseWithComplianceIntelligence(
            baseResponse,
            complianceAssessment,
            driftAnalysis,
            driftSeverity,
            adaptationSuggestion);

        // Step 8: Validate performance requirements
        var totalProcessingTime = DateTime.UtcNow - startTime;
        if (totalProcessingTime.TotalMilliseconds > 300) // Total budget: base + compliance overhead
        {
            _logger.LogError(_currentInteractionNumber,
                $"Agent compliance processing exceeded budget: {totalProcessingTime.TotalMilliseconds}ms");
        }

        return enhancedResponse;
    }

    /// <summary>
    /// Extracts constraint interaction data from MCP request and response.
    /// Creates domain model for agent compliance analysis.
    /// </summary>
    /// <param name="request">Original MCP request</param>
    /// <param name="response">Base constraint enforcement response</param>
    /// <returns>Constraint interaction for compliance analysis</returns>
    private ConstraintInteraction ExtractConstraintInteraction(int requestId, JsonElement request, object response)
    {
        // Extract basic interaction metadata
        var interactionId = $"interaction_{_currentInteractionNumber}";
        var timestamp = DateTime.UtcNow;
        var sessionId = $"session_{requestId}_{timestamp.Ticks}"; // Generate unique session ID based on request

        // Extract method and parameters
        string methodName = "tools/call"; // Default
        string context = "Agent compliance tracking interaction"; // Context
        string agentResponse = ""; // Agent response (extracted from response)
        string constraintId = "agent_compliance_constraint"; // Default constraint

        if (request.TryGetProperty("method", out var methodElement))
        {
            methodName = methodElement.GetString() ?? methodName;
        }

        // Extract agent response from MCP response
        var responseJson = JsonSerializer.Serialize(response);
        if (responseJson.Contains("CONSTRAINT GUIDANCE"))
        {
            agentResponse = "Constraint guidance provided";
        }
        else
        {
            agentResponse = "Standard tool call response";
        }

        return new ConstraintInteraction(
            InteractionId: Guid.NewGuid(),
            AgentId: sessionId, // Using session ID as agent ID
            ConstraintId: constraintId,
            Timestamp: timestamp,
            Context: context,
            AgentResponse: agentResponse);
    }

    /// <summary>
    /// Logs comprehensive agent compliance event with structured data.
    /// Captures compliance metrics, drift analysis, and adaptation suggestions.
    /// </summary>
    /// <param name="assessment">Real-time compliance assessment</param>
    /// <param name="driftAnalysis">Drift pattern analysis results</param>
    /// <param name="driftSeverity">Severity level of detected drift</param>
    /// <param name="adaptationSuggestion">Adaptive optimization suggestion (if any)</param>
    private void LogAgentComplianceEvent(
        ConstraintComplianceAssessment assessment,
        ComplianceAnalysisResult driftAnalysis,
        ViolationSeverity driftSeverity,
        ConstraintRefinementSuggestion? adaptationSuggestion)
    {
        var eventData = new
        {
            interaction_number = _currentInteractionNumber,
            event_type = "agent_compliance_enhanced",
            compliance_level = assessment.ComplianceLevel.ToString().ToLowerInvariant(),
            assessment_confidence = assessment.AssessmentConfidence,
            drift_severity = driftSeverity.ToString().ToLowerInvariant(),
            requires_intervention = driftSeverity >= ViolationSeverity.Major,
            adaptation_suggested = adaptationSuggestion != null,
            processing_timestamp = DateTime.UtcNow.ToString("O")
        };

        var eventJson = JsonSerializer.Serialize(eventData);
        Console.WriteLine(eventJson); // NDJSON structured logging

        // Also use structured logger for consistency
        _logger.LogConstraintInjection(
            _currentInteractionNumber,
            "agent_compliance_enhanced",
            new[] { assessment.InteractionId.ToString() },
            "agent_compliance_intelligence");
    }

    /// <summary>
    /// Enhances base constraint response with agent compliance intelligence.
    /// Adds compliance metrics, drift warnings, and adaptation suggestions to response.
    /// </summary>
    /// <param name="baseResponse">Original constraint enforcement response</param>
    /// <param name="assessment">Compliance assessment results</param>
    /// <param name="driftAnalysis">Drift analysis results</param>
    /// <param name="driftSeverity">Severity level of detected drift</param>
    /// <param name="adaptationSuggestion">Adaptation suggestion (if any)</param>
    /// <returns>Enhanced response with agent compliance intelligence</returns>
    private object EnhanceResponseWithComplianceIntelligence(
        object baseResponse,
        ConstraintComplianceAssessment assessment,
        ComplianceAnalysisResult driftAnalysis,
        ViolationSeverity driftSeverity,
        ConstraintRefinementSuggestion? adaptationSuggestion)
    {
        // Serialize base response to modify it
        var baseJson = JsonSerializer.Serialize(baseResponse);
        using var document = JsonDocument.Parse(baseJson);
        var root = document.RootElement;

        // Extract original result content
        string originalMessage = "";
        if (root.TryGetProperty("result", out var resultElement) &&
            resultElement.TryGetProperty("content", out var contentElement) &&
            contentElement.GetArrayLength() > 0)
        {
            var firstContent = contentElement[0];
            if (firstContent.TryGetProperty("text", out var textElement))
            {
                originalMessage = textElement.GetString() ?? "";
            }
        }

        // Create enhanced message with agent compliance intelligence
        var enhancedMessage = CreateEnhancedComplianceMessage(
            originalMessage,
            assessment,
            driftAnalysis,
            driftSeverity,
            adaptationSuggestion);

        // Build enhanced response structure
        var enhancedResponse = new
        {
            jsonrpc = "2.0",
            id = root.TryGetProperty("id", out var idElement) ? idElement.GetInt32() : 0,
            result = new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = enhancedMessage
                    }
                },
                agent_compliance_intelligence = new
                {
                    compliance_level = assessment.ComplianceLevel.ToString().ToLowerInvariant(),
                    assessment_confidence = assessment.AssessmentConfidence,
                    drift_severity = driftSeverity.ToString().ToLowerInvariant(),
                    requires_intervention = driftSeverity >= ViolationSeverity.Major,
                    adaptation_available = adaptationSuggestion != null,
                    processing_interaction = _currentInteractionNumber
                }
            }
        };

        return enhancedResponse;
    }

    /// <summary>
    /// Creates enhanced constraint message with agent compliance intelligence insights.
    /// Combines traditional constraints with real-time compliance feedback.
    /// </summary>
    /// <param name="originalMessage">Original constraint enforcement message</param>
    /// <param name="assessment">Compliance assessment</param>
    /// <param name="driftAnalysis">Drift analysis</param>
    /// <param name="driftSeverity">Severity level of detected drift</param>
    /// <param name="adaptationSuggestion">Adaptation suggestion</param>
    /// <returns>Enhanced message with agent compliance intelligence</returns>
    private string CreateEnhancedComplianceMessage(
        string originalMessage,
        ConstraintComplianceAssessment assessment,
        ComplianceAnalysisResult driftAnalysis,
        ViolationSeverity driftSeverity,
        ConstraintRefinementSuggestion? adaptationSuggestion)
    {
        var enhancedMessage = originalMessage;

        // Add agent compliance intelligence section
        enhancedMessage += $"\n\n🤖 AGENT COMPLIANCE INTELLIGENCE (Interaction {_currentInteractionNumber})\n";
        enhancedMessage += $"• Compliance Level: {assessment.ComplianceLevel} (Confidence: {assessment.AssessmentConfidence:F2})\n";

        if (assessment.RecommendedAction != null)
        {
            enhancedMessage += $"• Recommended Action: {assessment.RecommendedAction}\n";
        }

        // Add drift analysis insights
        if (driftSeverity > ViolationSeverity.Minor)
        {
            enhancedMessage += $"• Behavior Drift Detected: {driftSeverity} severity\n";

            if (driftSeverity >= ViolationSeverity.Major)
            {
                enhancedMessage += "⚠️ Proactive intervention recommended to maintain constraint adherence\n";
            }
        }

        // Add adaptation suggestions
        if (adaptationSuggestion != null)
        {
            enhancedMessage += $"• Adaptive Optimization Available\n";
            enhancedMessage += $"  Rationale: {adaptationSuggestion.Rationale ?? "Data-driven refinement suggested"}\n";
        }

        enhancedMessage += "\n💡 This enhanced guidance uses real-time agent behavior analysis for improved constraint adherence.";

        return enhancedMessage;
    }
}
