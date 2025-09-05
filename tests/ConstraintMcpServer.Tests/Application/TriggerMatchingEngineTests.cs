using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Context;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Presentation.Hosting;

namespace ConstraintMcpServer.Tests.Application;

/// <summary>
/// Unit tests for TriggerMatchingEngine with keyword/context detection.
/// Tests business logic for context-aware constraint activation.
/// Uses outside-in TDD approach with business scenario focus.
/// </summary>
[TestFixture]
public class TriggerMatchingEngineTests
{
    private ITriggerMatchingEngine _engine = null!;

    [SetUp]
    public void SetUp()
    {
        // Use relaxed threshold configuration to match realistic TDD constraint scoring
        var configuration = new TriggerMatchingConfiguration(defaultConfidenceThreshold: TriggerMatchingConfiguration.RelaxedConfidenceThreshold);
        _engine = new TriggerMatchingEngine(configuration);
    }

    /// <summary>
    /// E2E Acceptance Test: Context-Aware Constraint Activation
    /// This test validates the entire Step A2 implementation
    /// </summary>
    [Test]
    public async Task TriggerMatchingEngine_Should_Activate_Constraints_Based_On_User_Context()
    {
        // Business Scenario: Developer types "implement feature test" in development context
        // Expected: System activates TDD constraints with >60% confidence

        // Arrange - Development context indicating TDD workflow
        var context = new TriggerContext(
            keywords: new[] { "implement", "feature", "test" },
            filePath: "/src/features/NewFeature.cs",
            contextType: "feature_development"
        );

        // Act - Core implementation under test
        var activatedConstraints = await _engine.EvaluateConstraints(context);

        // Assert - Business validation
        Assert.That(activatedConstraints, Is.Not.Empty, "context indicates test-driven development");

        var tddConstraint = activatedConstraints.FirstOrDefault(c => c.ConstraintId == "tdd.test-first");
        Assert.That(tddConstraint, Is.Not.Null, "TDD context should activate test-first constraint");
        Assert.That(tddConstraint!.ConfidenceScore, Is.GreaterThan(TriggerMatchingConfiguration.RelaxedConfidenceThreshold), "sufficient confidence for clear TDD indicators");
    }

    /// <summary>
    /// Integration Test: MCP Pipeline Integration with Context-Aware Constraint Activation
    /// This test validates Day 3 milestone - MCP tool calls triggering context-aware constraints
    /// </summary>
    [Test]
    public async Task MCP_Pipeline_Should_Extract_Context_And_Activate_Relevant_Constraints()
    {
        // Business Scenario: User calls MCP tool "tools/list" in TDD development context
        // Expected: System extracts context and activates TDD constraints through MCP pipeline

        // Arrange - Create enhanced ToolCallHandler with context analysis
        var contextAnalyzer = new ContextAnalyzer();
        var configuration = new TriggerMatchingConfiguration(defaultConfidenceThreshold: TriggerMatchingConfiguration.RelaxedConfidenceThreshold);
        var triggerMatchingEngine = new TriggerMatchingEngine(configuration);

        var enhancedHandler = new EnhancedToolCallHandler(
            contextAnalyzer,
            triggerMatchingEngine
        );

        // Simulate MCP request parameters that indicate TDD context
        var mcpRequest = new
        {
            method = "tools/list",
            @params = new
            {
                context = "implementing feature with test-first approach",
                filePath = "/src/features/UserAuthentication.test.ts"
            }
        };

        // Act - Process MCP request with context-aware constraint activation
        var result = await enhancedHandler.HandleWithContextAnalysis(
            requestId: 1,
            mcpRequest,
            sessionId: "test-session-123"
        );

        // Assert - Verify context-aware constraint activation
        Assert.That(result, Is.Not.Null, "should return MCP response");
        Assert.That(result.HasConstraintActivation, Is.EqualTo(true), "should activate constraints based on context");

        var activatedConstraints = result.ActivatedConstraints;
        Assert.That(activatedConstraints, Is.Not.Empty, "TDD context should activate constraints");

        // Verify at least one constraint was activated with sufficient confidence
        var highConfidenceConstraint = activatedConstraints.FirstOrDefault(c =>
            c.ConfidenceScore > TriggerMatchingConfiguration.RelaxedConfidenceThreshold);
        Assert.That(highConfidenceConstraint, Is.Not.Null, "should activate at least one constraint with sufficient confidence");
    }

    [Test]
    public void Should_Create_TriggerMatchingEngine_Successfully()
    {
        // Simple test to verify engine creates successfully

        // Arrange & Act 
        var engine = new TriggerMatchingEngine();

        // Assert
        Assert.That(engine, Is.Not.Null, "engine should be created");
        Assert.That(engine.Configuration, Is.Not.Null, "engine should have configuration");
        Assert.That(engine.Configuration.DefaultConfidenceThreshold, Is.EqualTo(0.7), "should have default threshold");
    }
}
