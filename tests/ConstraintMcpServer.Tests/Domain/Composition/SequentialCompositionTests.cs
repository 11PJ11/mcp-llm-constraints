using System.Collections.Generic;
using NUnit.Framework;
using ConstraintMcpServer.Domain.Composition;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Tests.Domain.Composition;

/// <summary>
/// Unit tests for SequentialComposition strategy - drives implementation through TDD.
/// Tests TDD workflow orchestration: RED → GREEN → REFACTOR sequence.
/// </summary>
[TestFixture]
[Category("Unit")]
public class SequentialCompositionTests
{
    /// <summary>
    /// RED: This test will fail and drive SequentialComposition implementation
    /// Business requirement: Orchestrate TDD workflow with proper phase transitions
    /// </summary>
    [Test]
    public void SequentialComposition_Should_Activate_RED_Phase_Constraint_When_Starting_TDD()
    {
        // Arrange
        var composition = new SequentialComposition();
        var userDefinedSequence = new[] { "tdd.write-failing-test", "tdd.write-simplest-code", "tdd.refactor-code" };
        var currentContext = new UserDefinedContext("tdd-phase", "red", 0.9);
        var completedConstraints = new HashSet<string>();

        // Act
        var result = composition.GetNextConstraintId(userDefinedSequence, currentContext, completedConstraints);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("tdd.write-failing-test"));
        Assert.That(result.Reason, Contains.Substring("RED phase"));
    }

    /// <summary>
    /// RED: Test will drive phase transition logic implementation
    /// Business requirement: Transition from RED to GREEN when test fails
    /// </summary>
    [Test]
    public void SequentialComposition_Should_Transition_To_GREEN_Phase_When_Test_Fails()
    {
        // Arrange
        var composition = new SequentialComposition();
        var userDefinedSequence = new[] { "tdd.write-failing-test", "tdd.write-simplest-code", "tdd.refactor-code" };
        var currentContext = new UserDefinedContext("tdd-phase", "green", 0.8);
        var completedConstraints = new HashSet<string> { "tdd.write-failing-test" };

        // Act
        var result = composition.GetNextConstraintId(userDefinedSequence, currentContext, completedConstraints);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("tdd.write-simplest-code"));
        Assert.That(result.Reason, Contains.Substring("GREEN phase"));
    }

    /// <summary>
    /// RED: Test will drive REFACTOR phase activation
    /// Business requirement: Transition to REFACTOR when tests pass
    /// </summary>
    [Test]
    public void SequentialComposition_Should_Transition_To_REFACTOR_Phase_When_Tests_Pass()
    {
        // Arrange
        var composition = new SequentialComposition();
        var userDefinedSequence = new[] { "tdd.write-failing-test", "tdd.write-simplest-code", "tdd.refactor-code" };
        var currentContext = new UserDefinedContext("tdd-phase", "refactor", 0.7);
        var completedConstraints = new HashSet<string> { "tdd.write-failing-test", "tdd.write-simplest-code" };

        // Act
        var result = composition.GetNextConstraintId(userDefinedSequence, currentContext, completedConstraints);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("tdd.refactor-code"));
        Assert.That(result.Reason, Contains.Substring("REFACTOR phase"));
    }

}
