using System;
using System.Linq;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Context;
using ConstraintMcpServer.Domain.Constraints;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Unit.Domain;

/// <summary>
/// Unit tests for SessionContext aggregate root.
/// Tests session-based pattern tracking and constraint activation history.
/// Uses outside-in TDD approach with business scenario focus.
/// </summary>
[TestFixture]
public class SessionContextTests
{
    private SessionContext _sessionContext = null!;
    private const string TestSessionId = "test-session-123";
    private static readonly string[] keywords = new[] { "implement", "test" };

    [SetUp]
    public void SetUp()
    {
        _sessionContext = new SessionContext(TestSessionId);
    }

    /// <summary>
    /// E2E Acceptance Test: Session-based Pattern Recognition
    /// This test validates SessionContext aggregate root implementation
    /// </summary>
    [Test]
    public void SessionContext_Should_Track_Development_Patterns_Across_Tool_Calls()
    {
        // Business Scenario: Developer performs TDD workflow across multiple tool calls
        // Expected: System recognizes TDD pattern and provides session-based insights

        // Arrange - Simulate TDD workflow with test-first development
        var tddConstraint = CreateTestConstraint("tdd.test-first");
        var triggerContext = new TriggerContext(
            keywords: keywords,
            filePath: "/src/features/NewFeature.cs",
            contextType: "testing"
        );

        // Act - Record multiple activations simulating TDD workflow
        for (int i = 0; i < 3; i++)
        {
            var activation = new ConstraintActivation(
                constraintId: tddConstraint.Id.Value,
                confidenceScore: 0.85,
                reason: ActivationReason.ContextPatternMatch,
                triggerContext: triggerContext,
                constraint: tddConstraint
            );

            _sessionContext.RecordActivation(activation);
            _sessionContext.RecordToolCall();
        }

        Assert.Multiple(() =>
        {
            // Assert - Business validation
            Assert.That(_sessionContext.ActivationHistory, Has.Count.EqualTo(3), "should track all activations in session");
            Assert.That(_sessionContext.TotalToolCalls, Is.EqualTo(3), "should track tool call count");
            Assert.That(_sessionContext.DetectedActivityPattern, Is.EqualTo("test-driven"), "should detect TDD pattern from activations");
            Assert.That(_sessionContext.DominantContextType, Is.EqualTo("testing"), "should identify dominant context type");
        });

        var relevanceAdjustment = _sessionContext.GetSessionRelevanceAdjustment("tdd.test-first");
        Assert.That(relevanceAdjustment, Is.GreaterThan(1.0), "should boost relevance for successful constraints in session");
    }

    [Test]
    public void Constructor_Should_Initialize_Session_With_Valid_ID()
    {
        // Arrange & Act
        var session = new SessionContext("valid-session-123");

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(session.SessionId, Is.EqualTo("valid-session-123"));
            Assert.That(session.ActivationHistory, Is.Empty);
            Assert.That(session.TotalToolCalls, Is.Zero);
            Assert.That(session.DetectedActivityPattern, Is.EqualTo("unknown"));
            Assert.That(session.DominantContextType, Is.EqualTo("unknown"));
        });
    }

    [Test]
    public void Constructor_Should_Throw_For_Invalid_Session_ID()
    {
        // Test various invalid session IDs
        Assert.Throws<ArgumentException>(() => new SessionContext(null!));
        Assert.Throws<ArgumentException>(() => new SessionContext(""));
        Assert.Throws<ArgumentException>(() => new SessionContext("   "));
    }

    [Test]
    public void RecordActivation_Should_Update_Session_History()
    {
        // Arrange
        var constraint = CreateTestConstraint("test.constraint");
        var triggerContext = new TriggerContext(
            keywords: new[] { "test" },
            filePath: "/test.cs",
            contextType: "testing"
        );
        var activation = new ConstraintActivation(
            constraintId: constraint.Id.Value,
            confidenceScore: 0.8,
            reason: ActivationReason.ContextPatternMatch,
            triggerContext: triggerContext,
            constraint: constraint
        );

        // Act
        _sessionContext.RecordActivation(activation);

        // Assert
        Assert.That(_sessionContext.ActivationHistory, Has.Count.EqualTo(1));
        Assert.That(_sessionContext.ActivationHistory[0], Is.EqualTo(activation));
    }

    [Test]
    public void RecordActivation_Should_Update_Activity_Pattern()
    {
        // Arrange - Multiple testing activations
        var constraint = CreateTestConstraint("test.constraint");
        var testingContext = new TriggerContext(
            keywords: new[] { "test" },
            filePath: "/test.cs",
            contextType: "testing"
        );

        // Act - Record multiple testing activations
        for (int i = 0; i < 3; i++)
        {
            var activation = new ConstraintActivation(
                constraintId: constraint.Id.Value,
                confidenceScore: 0.8,
                reason: ActivationReason.ContextPatternMatch,
                triggerContext: testingContext,
                constraint: constraint
            );
            _sessionContext.RecordActivation(activation);
        }

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(_sessionContext.DetectedActivityPattern, Is.EqualTo("test-driven"));
            Assert.That(_sessionContext.DominantContextType, Is.EqualTo("testing"));
        });
    }

    [Test]
    public void GetSessionRelevanceAdjustment_Should_Boost_Successful_Constraints()
    {
        // Arrange
        var constraint = CreateTestConstraint("successful.constraint");
        var triggerContext = new TriggerContext(
            keywords: new[] { "test" },
            filePath: "/test.cs",
            contextType: "testing"
        );
        var activation = new ConstraintActivation(
            constraintId: constraint.Id.Value,
            confidenceScore: 0.8,
            reason: ActivationReason.ContextPatternMatch,
            triggerContext: triggerContext,
            constraint: constraint
        );

        // Act
        _sessionContext.RecordActivation(activation);
        var adjustment = _sessionContext.GetSessionRelevanceAdjustment("successful.constraint");

        // Assert
        Assert.That(adjustment, Is.GreaterThan(1.0), "should boost constraints that have been activated in session");
    }

    [Test]
    public void GetSessionRelevanceAdjustment_Should_Handle_Unknown_Constraints()
    {
        // Act
        var adjustment = _sessionContext.GetSessionRelevanceAdjustment("unknown.constraint");

        // Assert
        Assert.That(adjustment, Is.EqualTo(1.0), "should return neutral adjustment for unknown constraints");
    }

    [Test]
    public void GetSessionAnalytics_Should_Provide_Session_Insights()
    {
        // Arrange
        var constraint = CreateTestConstraint("test.constraint");
        var triggerContext = new TriggerContext(
            keywords: new[] { "test" },
            filePath: "/test.cs",
            contextType: "testing"
        );
        var activation = new ConstraintActivation(
            constraintId: constraint.Id.Value,
            confidenceScore: 0.8,
            reason: ActivationReason.ContextPatternMatch,
            triggerContext: triggerContext,
            constraint: constraint
        );

        _sessionContext.RecordActivation(activation);
        _sessionContext.RecordToolCall();

        // Act
        var analytics = _sessionContext.GetSessionAnalytics();

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(analytics.SessionId, Is.EqualTo(TestSessionId));
            Assert.That(analytics.TotalToolCalls, Is.EqualTo(1));
            Assert.That(analytics.TotalActivations, Is.EqualTo(1));
            Assert.That(analytics.MostActivatedConstraint, Is.EqualTo("test.constraint"));
            Assert.That(analytics.ContextTypeBreakdown, Contains.Key("testing"));
        });
    }

    [Test]
    public void Reset_Should_Clear_Session_State()
    {
        // Arrange
        var constraint = CreateTestConstraint("test.constraint");
        var triggerContext = new TriggerContext(
            keywords: new[] { "test" },
            filePath: "/test.cs",
            contextType: "testing"
        );
        var activation = new ConstraintActivation(
            constraintId: constraint.Id.Value,
            confidenceScore: 0.8,
            reason: ActivationReason.ContextPatternMatch,
            triggerContext: triggerContext,
            constraint: constraint
        );

        _sessionContext.RecordActivation(activation);
        _sessionContext.RecordToolCall();

        // Act
        _sessionContext.Reset();

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(_sessionContext.ActivationHistory, Is.Empty);
            Assert.That(_sessionContext.TotalToolCalls, Is.Zero);
            Assert.That(_sessionContext.DetectedActivityPattern, Is.EqualTo("unknown"));
            Assert.That(_sessionContext.DominantContextType, Is.EqualTo("unknown"));
            Assert.That(_sessionContext.SessionId, Is.EqualTo(TestSessionId), "session ID should remain unchanged");
        });
    }

    [Test]
    public void SessionContext_Should_Handle_Mixed_Development_Pattern()
    {
        // Arrange - Mixed development activities
        var testingContext = new TriggerContext(new[] { "test" }, "/test.cs", "testing");
        var featureContext = new TriggerContext(new[] { "implement" }, "/feature.cs", "feature_development");
        var refactorContext = new TriggerContext(new[] { "refactor" }, "/service.cs", "refactoring");

        var constraint = CreateTestConstraint("mixed.constraint");

        // Act - Record varied activations
        RecordActivation(constraint, testingContext);
        RecordActivation(constraint, featureContext);
        RecordActivation(constraint, refactorContext);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(_sessionContext.DetectedActivityPattern, Is.EqualTo("mixed-development"));
            Assert.That(_sessionContext.DominantContextType, Is.EqualTo("mixed"));
        });
    }

    private void RecordActivation(AtomicConstraint constraint, TriggerContext context)
    {
        var activation = new ConstraintActivation(
            constraintId: constraint.Id.Value,
            confidenceScore: 0.8,
            reason: ActivationReason.ContextPatternMatch,
            triggerContext: context,
            constraint: constraint
        );
        _sessionContext.RecordActivation(activation);
    }

    private static AtomicConstraint CreateTestConstraint(string id)
    {
        var triggers = new TriggerConfiguration(
            keywords: new[] { "test" },
            filePatterns: Array.Empty<string>(),
            contextPatterns: new[] { "testing" },
            antiPatterns: Array.Empty<string>(),
            confidenceThreshold: 0.8
        );

        return new AtomicConstraint(
            id: new ConstraintId(id),
            title: "Test Constraint",
            priority: 0.8,
            triggers: triggers,
            reminders: new[] { "Test reminder" }
        );
    }
}
