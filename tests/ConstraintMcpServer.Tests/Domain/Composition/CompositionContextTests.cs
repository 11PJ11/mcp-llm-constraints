using NUnit.Framework;
using ConstraintMcpServer.Domain.Composition;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Tests.Domain.Composition;

/// <summary>
/// Unit tests for CompositionStrategyContext - drives implementation through TDD.
/// Tests context information needed for composition strategy decision-making.
/// </summary>
[TestFixture]
[Category("Unit")]
public class CompositionContextTests
{
    /// <summary>
    /// RED: This test will fail and drive CompositionContext implementation
    /// Business requirement: Track workflow state for sequential composition decisions
    /// </summary>
    [Test]
    public void CompositionContext_Should_Track_Workflow_State_For_Sequential_Composition()
    {
        // Arrange
        var workflowState = new WorkflowState("red", "TDD Red phase");
        var evaluationStatus = new UserDefinedEvaluationStatus("failing", "testing", false);
        var context = new CompositionStrategyContext
        {
            CurrentWorkflowState = workflowState,
            EvaluationStatus = evaluationStatus,
            DevelopmentContext = "feature_development"
        };

        // Act & Assert
        Assert.That(context.CurrentWorkflowState.Name, Is.EqualTo("red"));
        Assert.That(context.EvaluationStatus.Name, Is.EqualTo("failing"));
        Assert.That(context.DevelopmentContext, Is.EqualTo("feature_development"));
    }

    /// <summary>
    /// RED: Test will drive WorkflowState implementation
    /// Business requirement: Support user-defined workflow state transitions
    /// </summary>
    [Test]
    public void CompositionContext_Should_Support_User_Defined_Workflow_States()
    {
        // Arrange & Act - Create user-defined workflow states
        var redState = new WorkflowState("red", "TDD Red phase");
        var greenState = new WorkflowState("green", "TDD Green phase");
        var refactorState = new WorkflowState("refactor", "TDD Refactor phase");

        // Assert
        Assert.That(redState.Name, Is.Not.EqualTo(greenState.Name));
        Assert.That(greenState.Name, Is.Not.EqualTo(refactorState.Name));
        Assert.That(refactorState.Name, Is.Not.EqualTo(redState.Name));
    }

    /// <summary>
    /// RED: Test will drive UserDefinedEvaluationStatus implementation
    /// Business requirement: Track user-defined evaluation states for workflow transitions
    /// </summary>
    [Test]
    public void CompositionContext_Should_Track_User_Defined_Evaluation_Status()
    {
        // Arrange & Act - Create user-defined evaluation statuses
        var failingStatus = new UserDefinedEvaluationStatus("failing", "testing", false);
        var passingStatus = new UserDefinedEvaluationStatus("passing", "testing", true);
        var notRunStatus = new UserDefinedEvaluationStatus("not-run", "testing", false);

        // Assert
        Assert.That(failingStatus.Name, Is.Not.EqualTo(passingStatus.Name));
        Assert.That(passingStatus.Name, Is.Not.EqualTo(notRunStatus.Name));
        Assert.That(notRunStatus.Name, Is.Not.EqualTo(failingStatus.Name));
    }

    /// <summary>
    /// Business requirement: Context must be immutable for functional composition updates
    /// </summary>
    [Test]
    public void CompositionContext_Should_Support_Immutable_Updates()
    {
        // Arrange
        var redState = new WorkflowState("red", "TDD Red phase");
        var greenState = new WorkflowState("green", "TDD Green phase");
        var failingStatus = new UserDefinedEvaluationStatus("failing", "testing", false);
        var passingStatus = new UserDefinedEvaluationStatus("passing", "testing", true);

        var originalContext = new CompositionStrategyContext
        {
            CurrentWorkflowState = redState,
            EvaluationStatus = failingStatus
        };

        // Act - Should create new instance rather than mutating
        var updatedContext = originalContext with
        {
            CurrentWorkflowState = greenState,
            EvaluationStatus = passingStatus
        };

        // Assert - Original unchanged, new instance created
        Assert.That(originalContext.CurrentWorkflowState.Name, Is.EqualTo("red"));
        Assert.That(originalContext.EvaluationStatus.Name, Is.EqualTo("failing"));
        Assert.That(updatedContext.CurrentWorkflowState.Name, Is.EqualTo("green"));
        Assert.That(updatedContext.EvaluationStatus.Name, Is.EqualTo("passing"));
    }
}
