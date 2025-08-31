using NUnit.Framework;
using ConstraintMcpServer.Domain.Composition;

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
    /// Business requirement: Track TDD phase for sequential composition decisions
    /// </summary>
    [Test]
    public void CompositionContext_Should_Track_TDD_Phase_For_Sequential_Composition()
    {
        // Arrange
        var context = new CompositionStrategyContext
        {
            CurrentPhase = TddPhase.Red,
            TestStatus = TestStatus.Failing,
            DevelopmentContext = "feature_development"
        };

        // Act & Assert
        Assert.That(context.CurrentPhase, Is.EqualTo(TddPhase.Red));
        Assert.That(context.TestStatus, Is.EqualTo(TestStatus.Failing));
        Assert.That(context.DevelopmentContext, Is.EqualTo("feature_development"));
    }

    /// <summary>
    /// RED: Test will drive TddPhase enum implementation
    /// Business requirement: Support TDD workflow phase transitions
    /// </summary>
    [Test]
    public void CompositionContext_Should_Support_All_TDD_Phases()
    {
        // Arrange & Act - These will fail until TddPhase enum exists
        var redPhase = TddPhase.Red;
        var greenPhase = TddPhase.Green;
        var refactorPhase = TddPhase.Refactor;

        // Assert
        Assert.That(redPhase, Is.Not.EqualTo(greenPhase));
        Assert.That(greenPhase, Is.Not.EqualTo(refactorPhase));
        Assert.That(refactorPhase, Is.Not.EqualTo(redPhase));
    }

    /// <summary>
    /// RED: Test will drive TestStatus enum implementation
    /// Business requirement: Track test execution state for phase transitions
    /// </summary>
    [Test]
    public void CompositionContext_Should_Track_Test_Execution_Status()
    {
        // Arrange & Act - These will fail until TestStatus enum exists
        var failingStatus = TestStatus.Failing;
        var passingStatus = TestStatus.Passing;
        var notRunStatus = TestStatus.NotRun;

        // Assert
        Assert.That(failingStatus, Is.Not.EqualTo(passingStatus));
        Assert.That(passingStatus, Is.Not.EqualTo(notRunStatus));
        Assert.That(notRunStatus, Is.Not.EqualTo(failingStatus));
    }

    /// <summary>
    /// Business requirement: Context must be immutable for functional composition updates
    /// </summary>
    [Test]
    public void CompositionContext_Should_Support_Immutable_Updates()
    {
        // Arrange
        var originalContext = new CompositionStrategyContext
        {
            CurrentPhase = TddPhase.Red,
            TestStatus = TestStatus.Failing
        };

        // Act - Should create new instance rather than mutating
        var updatedContext = originalContext with
        {
            CurrentPhase = TddPhase.Green,
            TestStatus = TestStatus.Passing
        };

        // Assert - Original unchanged, new instance created
        Assert.That(originalContext.CurrentPhase, Is.EqualTo(TddPhase.Red));
        Assert.That(originalContext.TestStatus, Is.EqualTo(TestStatus.Failing));
        Assert.That(updatedContext.CurrentPhase, Is.EqualTo(TddPhase.Green));
        Assert.That(updatedContext.TestStatus, Is.EqualTo(TestStatus.Passing));
    }
}
