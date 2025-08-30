using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Domain.Context;

namespace ConstraintMcpServer.Tests.Application.Selection;

/// <summary>
/// Unit tests for TriggerMatchingEngine driven by E2E test requirements.
/// These tests define the constraint activation behavior needed for context-aware activation.
/// </summary>
[TestFixture]
public sealed class TriggerMatchingEngineTests
{
    private TriggerMatchingEngine _engine = null!;

    [SetUp]
    public void SetUp()
    {
        _engine = new TriggerMatchingEngine();
    }

    [Test]
    public async Task EvaluateConstraints_WithTddContext_ActivatesTestingConstraints()
    {
        // Arrange - TDD context from E2E test 
        var tddContext = new TriggerContext(
            keywords: new[] { "unit", "tests", "test", "authentication" },
            filePath: "/src/auth/UserAuthTests.cs",
            contextType: "testing"
        );

        // Act
        var activations = await _engine.EvaluateConstraints(tddContext);

        // Assert - Should activate TDD-related constraints
        Assert.That(activations, Is.Not.Empty);
        // At least one constraint should be activated for TDD context
        Assert.That(activations.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task EvaluateConstraints_WithRefactoringContext_ActivatesRefactoringConstraints()
    {
        // Arrange - Refactoring context from E2E test
        var refactoringContext = new TriggerContext(
            keywords: new[] { "refactoring", "legacy", "improve", "maintainability" },
            filePath: "/src/legacy/OldModule.cs",
            contextType: "refactoring"
        );

        // Act
        var activations = await _engine.EvaluateConstraints(refactoringContext);

        // Assert - Should activate refactoring-related constraints
        Assert.That(activations, Is.Not.Empty);
        // At least one constraint should be activated for refactoring context
        Assert.That(activations.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task EvaluateConstraints_WithUnclearContext_ActivatesNoConstraints()
    {
        // Arrange - Unclear context from E2E test
        var unclearContext = new TriggerContext(
            keywords: new[] { "working", "general", "tasks" },
            filePath: "/src/utils/Helper.cs",
            contextType: "unknown"
        );

        // Act
        var activations = await _engine.EvaluateConstraints(unclearContext);

        // Assert - Should activate no constraints for unclear context
        Assert.That(activations, Is.Empty);
    }

    [Test]
    public async Task GetRelevantConstraints_WithHighConfidenceThreshold_FiltersLowConfidenceConstraints()
    {
        // Arrange
        var context = new TriggerContext(
            keywords: new[] { "unit", "tests" },
            filePath: "/src/tests/SomeTest.cs",
            contextType: "testing"
        );

        // Act
        var highConfidenceConstraints = await _engine.GetRelevantConstraints(context, minConfidence: 0.8);
        var lowConfidenceConstraints = await _engine.GetRelevantConstraints(context, minConfidence: 0.1);

        // Assert - High confidence threshold should return fewer or equal constraints
        Assert.That(highConfidenceConstraints.Count, Is.LessThanOrEqualTo(lowConfidenceConstraints.Count));
    }
}
