using System.Collections.Generic;
using NUnit.Framework;
using ConstraintMcpServer.Domain.Composition;

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
        var context = new CompositionStrategyContext
        {
            CurrentPhase = TddPhase.Red,
            TestStatus = TestStatus.NotRun
        };

        // Act
        var result = composition.GetNextConstraintId(context);

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
        var context = new CompositionStrategyContext
        {
            CurrentPhase = TddPhase.Red,
            TestStatus = TestStatus.Failing
        };

        // Act
        var result = composition.GetNextConstraintId(context);

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
        var context = new CompositionStrategyContext
        {
            CurrentPhase = TddPhase.Green,
            TestStatus = TestStatus.Passing
        };

        // Act
        var result = composition.GetNextConstraintId(context);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("tdd.refactor-code"));
        Assert.That(result.Reason, Contains.Substring("REFACTOR phase"));
    }

}
