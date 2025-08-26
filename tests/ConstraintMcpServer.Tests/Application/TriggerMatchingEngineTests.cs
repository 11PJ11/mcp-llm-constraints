using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Context;
using ConstraintMcpServer.Domain.Constraints;

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
        _engine = new TriggerMatchingEngine();
    }

    /// <summary>
    /// E2E Acceptance Test: Context-Aware Constraint Activation
    /// This test validates the entire Step A2 implementation
    /// </summary>
    [Test]
    public async Task TriggerMatchingEngine_Should_Activate_Constraints_Based_On_User_Context()
    {
        // Business Scenario: Developer types "implement feature test" in development context
        // Expected: System activates TDD constraints with >80% confidence

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
        Assert.That(tddConstraint!.ConfidenceScore, Is.GreaterThan(0.8), "high confidence for clear TDD indicators");
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
