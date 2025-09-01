using System.Collections.Generic;
using NUnit.Framework;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Common;
using ConstraintMcpServer.Domain.Composition;
using ConstraintMcpServer.Application.Selection;

namespace ConstraintMcpServer.Tests.Integration;

/// <summary>
/// Integration tests validating that the core refactored functionality works correctly
/// with the new methodology-agnostic types after removing hardcoded TDD/Phase dependencies.
/// </summary>
[TestFixture]
[Category("Integration")]
[Category("CoreValidation")]
public class CoreFunctionalityValidationTests
{
    /// <summary>
    /// Validates that constraints can be created with UserDefinedContext instead of Phase.
    /// </summary>
    [Test]
    public void Constraint_Should_Work_With_UserDefinedContext_Instead_Of_Phase()
    {
        // Arrange - Create constraint using new methodology-agnostic types
        var workflowContexts = new[]
        {
            new UserDefinedContext("workflow", "red", 0.9),
            new UserDefinedContext("workflow", "green", 0.8),
            new UserDefinedContext("methodology", "tdd", 0.7)
        };

        // Act - Create constraint with new types
        var constraint = new Constraint(
            new ConstraintId("test.constraint"),
            "Test constraint with new types",
            new Priority(0.85),
            workflowContexts,
            new[] { "Test reminder" }
        );

        // Assert - Verify constraint created successfully with new types
        Assert.That(constraint.Id.Value, Is.EqualTo("test.constraint"));
        Assert.That(constraint.Title, Is.EqualTo("Test constraint with new types"));
        Assert.That(constraint.Priority.Value, Is.EqualTo(0.85));
        Assert.That(constraint.WorkflowContexts.Count, Is.EqualTo(3));
        Assert.That(constraint.Reminders.Count, Is.EqualTo(1));

        // Verify context filtering works
        var redContext = new UserDefinedContext("workflow", "red", 0.9);
        Assert.That(constraint.AppliesTo(redContext), Is.True);
        
        var blueContext = new UserDefinedContext("workflow", "blue", 0.9);
        Assert.That(constraint.AppliesTo(blueContext), Is.False);
    }

    /// <summary>
    /// Validates that ConstraintSelector works with UserDefinedContext filtering.
    /// </summary>
    [Test]
    public void ConstraintSelector_Should_Filter_By_UserDefinedContext()
    {
        // Arrange - Create constraints with different contexts
        var constraints = new[]
        {
            new Constraint(
                new ConstraintId("red.constraint"),
                "RED phase constraint",
                new Priority(0.9),
                new[] { new UserDefinedContext("workflow", "red", 0.9) },
                new[] { "RED reminder" }
            ),
            new Constraint(
                new ConstraintId("green.constraint"),
                "GREEN phase constraint", 
                new Priority(0.8),
                new[] { new UserDefinedContext("workflow", "green", 0.8) },
                new[] { "GREEN reminder" }
            ),
            new Constraint(
                new ConstraintId("universal.constraint"),
                "Universal constraint",
                new Priority(0.95),
                new[] { 
                    new UserDefinedContext("workflow", "red", 0.9),
                    new UserDefinedContext("workflow", "green", 0.8)
                },
                new[] { "Universal reminder" }
            )
        };

        var redContext = new UserDefinedContext("workflow", "red", 0.9);

        // Act - Select constraints for RED context
        var selected = ConstraintSelector.SelectConstraints(constraints, redContext, topK: 10);

        // Assert - Should get RED and Universal constraints, sorted by priority
        Assert.That(selected.Count, Is.EqualTo(2));
        Assert.That(selected[0].Id.Value, Is.EqualTo("universal.constraint"), "Highest priority constraint should be first");
        Assert.That(selected[1].Id.Value, Is.EqualTo("red.constraint"), "RED constraint should be second");
    }

    /// <summary>
    /// Validates that Sequential Composition works with new methodology-agnostic types.
    /// </summary>
    [Test]
    public void SequentialComposition_Should_Work_With_UserDefinedContext()
    {
        // Arrange - Create sequential composition and TDD-like sequence using new types
        var composition = new SequentialComposition();
        var tddSequence = new List<string> { "tdd.write-test", "tdd.write-code", "tdd.refactor" };
        var redContext = new UserDefinedContext("workflow", "red", 0.9);
        var completedConstraints = new HashSet<string>();

        // Act - Get next constraint for RED phase
        var result = composition.GetNextConstraintId(tddSequence, redContext, completedConstraints);

        // Assert - Should return first constraint in sequence
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("tdd.write-test"));
        Assert.That(result.Reason, Contains.Substring("tdd.write-test"));
    }

    /// <summary>
    /// Validates that CompositionContext works with WorkflowState and UserDefinedEvaluationStatus.
    /// </summary>
    [Test]
    public void CompositionContext_Should_Work_With_New_Types()
    {
        // Arrange - Create context using new methodology-agnostic types
        var workflowState = new WorkflowState("red", "RED phase - writing failing tests");
        var evaluationStatus = new UserDefinedEvaluationStatus("failing", "Tests are currently failing", false);
        
        // Act - Create composition context with new types
        var context = new CompositionContext()
            .WithWorkflowState(workflowState)
            .WithEvaluationStatus(evaluationStatus)
            .WithDevelopmentContext("TDD workflow validation");

        // Assert - Verify context created successfully with new types
        Assert.That(context.CurrentWorkflowState.Name, Is.EqualTo("red"));
        Assert.That(context.CurrentWorkflowState.Description, Is.EqualTo("RED phase - writing failing tests"));
        Assert.That(context.EvaluationStatus.Status, Is.EqualTo("failing"));
        Assert.That(context.EvaluationStatus.IsSuccessful, Is.False);
        Assert.That(context.DevelopmentContext, Is.EqualTo("TDD workflow validation"));
    }

    /// <summary>
    /// End-to-end integration test simulating TDD workflow with new methodology-agnostic types.
    /// </summary>
    [Test]
    public void EndToEnd_TDD_Workflow_Should_Work_With_New_Types()
    {
        // Arrange - Set up TDD workflow using new methodology-agnostic types
        var tddConstraints = new[]
        {
            new Constraint(
                new ConstraintId("tdd.write-failing-test"),
                "Write a failing test first",
                new Priority(0.95),
                new[] { new UserDefinedContext("workflow", "red", 0.9) },
                new[] { "Start with a failing test (RED) before implementation" }
            ),
            new Constraint(
                new ConstraintId("tdd.write-simplest-code"),
                "Write the simplest code to make test pass",
                new Priority(0.90),
                new[] { new UserDefinedContext("workflow", "green", 0.8) },
                new[] { "Write minimal code to make the test pass (GREEN)" }
            ),
            new Constraint(
                new ConstraintId("tdd.refactor-improve"),
                "Refactor to improve design",
                new Priority(0.85),
                new[] { new UserDefinedContext("workflow", "refactor", 0.7) },
                new[] { "Refactor the code while keeping tests green (REFACTOR)" }
            )
        };

        var composition = new SequentialComposition();
        var tddSequence = new List<string> { "tdd.write-failing-test", "tdd.write-simplest-code", "tdd.refactor-improve" };

        // Act & Assert - Simulate TDD RED → GREEN → REFACTOR cycle

        // 1. RED Phase - Should get "write failing test" constraint
        var redContext = new UserDefinedContext("workflow", "red", 0.9);
        var redSelected = ConstraintSelector.SelectConstraints(tddConstraints, redContext, topK: 1);
        var redResult = composition.GetNextConstraintId(tddSequence, redContext, new HashSet<string>());

        Assert.That(redSelected.Count, Is.EqualTo(1));
        Assert.That(redSelected[0].Id.Value, Is.EqualTo("tdd.write-failing-test"));
        Assert.That(redResult.IsSuccess, Is.True);
        Assert.That(redResult.Value, Is.EqualTo("tdd.write-failing-test"));

        // 2. GREEN Phase - Should get "write simplest code" constraint
        var greenContext = new UserDefinedContext("workflow", "green", 0.8);
        var greenSelected = ConstraintSelector.SelectConstraints(tddConstraints, greenContext, topK: 1);
        var greenCompleted = new HashSet<string> { "tdd.write-failing-test" };
        var greenResult = composition.GetNextConstraintId(tddSequence, greenContext, greenCompleted);

        Assert.That(greenSelected.Count, Is.EqualTo(1));
        Assert.That(greenSelected[0].Id.Value, Is.EqualTo("tdd.write-simplest-code"));
        Assert.That(greenResult.IsSuccess, Is.True);
        Assert.That(greenResult.Value, Is.EqualTo("tdd.write-simplest-code"));

        // 3. REFACTOR Phase - Should get "refactor improve" constraint
        var refactorContext = new UserDefinedContext("workflow", "refactor", 0.7);
        var refactorSelected = ConstraintSelector.SelectConstraints(tddConstraints, refactorContext, topK: 1);
        var refactorCompleted = new HashSet<string> { "tdd.write-failing-test", "tdd.write-simplest-code" };
        var refactorResult = composition.GetNextConstraintId(tddSequence, refactorContext, refactorCompleted);

        Assert.That(refactorSelected.Count, Is.EqualTo(1));
        Assert.That(refactorSelected[0].Id.Value, Is.EqualTo("tdd.refactor-improve"));
        Assert.That(refactorResult.IsSuccess, Is.True);
        Assert.That(refactorResult.Value, Is.EqualTo("tdd.refactor-improve"));

        // 4. Workflow Complete - All constraints completed
        var allCompleted = new HashSet<string> { "tdd.write-failing-test", "tdd.write-simplest-code", "tdd.refactor-improve" };
        var completeResult = composition.GetNextConstraintId(tddSequence, redContext, allCompleted);

        Assert.That(completeResult.IsSuccess, Is.True);
        Assert.That(completeResult.Value, Is.EqualTo(string.Empty), "Should return empty string when workflow complete");
        Assert.That(completeResult.Reason, Contains.Substring("complete"));
    }
}