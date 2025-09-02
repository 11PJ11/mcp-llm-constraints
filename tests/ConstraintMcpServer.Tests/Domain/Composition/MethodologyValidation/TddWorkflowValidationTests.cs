using System.Collections.Generic;
using NUnit.Framework;
using ConstraintMcpServer.Domain.Composition;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Tests.Domain.Composition.MethodologyValidation;

/// <summary>
/// Validation tests proving that our generic system can correctly express TDD methodology.
/// These tests demonstrate that the methodology-agnostic architecture can support
/// the original TDD workflows through user-defined configuration.
/// </summary>
[TestFixture]
[Category("MethodologyValidation")]
[Category("TDD")]
public class TddWorkflowValidationTests
{
    public SequentialComposition _sequentialComposition = null!;
    public IReadOnlyList<string> _tddSequence = null!;
    public UserDefinedContext _redPhaseContext = null!;
    public UserDefinedContext _greenPhaseContext = null!;
    public UserDefinedContext _refactorPhaseContext = null!;

    [SetUp]
    public void SetUp()
    {
        _sequentialComposition = new SequentialComposition();

        // Configure TDD workflow through generic system
        _tddSequence = new List<string>
        {
            "tdd.write-failing-test",    // RED phase
            "tdd.write-simplest-code",   // GREEN phase  
            "tdd.refactor-code"          // REFACTOR phase
        };

        // Define TDD contexts using generic UserDefinedContext
        _redPhaseContext = new UserDefinedContext("workflow", "red", priority: 0.9);
        _greenPhaseContext = new UserDefinedContext("workflow", "green", priority: 0.8);
        _refactorPhaseContext = new UserDefinedContext("workflow", "refactor", priority: 0.7);
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system can express TDD RED phase activation
    /// Original requirement: "Activate RED phase constraint when starting TDD"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Express_TDD_RED_Phase_Through_User_Configuration()
    {
        // Arrange - Using generic system configured for TDD
        var completedConstraints = new HashSet<string>(); // Empty - starting TDD cycle

        // Act - Generic system with TDD configuration
        var result = _sequentialComposition.GetNextConstraintId(
            _tddSequence,
            _redPhaseContext,
            completedConstraints);

        // Assert - Validates TDD workflow is correctly expressed
        Assert.That(result.IsSuccess, Is.True, "Generic system should successfully handle TDD configuration");
        Assert.That(result.Value, Is.EqualTo("tdd.write-failing-test"),
            "Should activate RED phase constraint first in TDD sequence");
        Assert.That(result.Reason, Does.Contain("red"),
            "Should provide context-aware guidance for RED phase");
        Assert.That(result.Reason, Does.Contain("Step 1 of 3"),
            "Should indicate position in TDD sequence");
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system can express TDD GREEN phase transition
    /// Original requirement: "Transition from RED to GREEN when test fails"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Express_TDD_GREEN_Phase_Transition_Through_Configuration()
    {
        // Arrange - RED phase completed, moving to GREEN
        var completedConstraints = new HashSet<string> { "tdd.write-failing-test" };

        // Act - Generic system handles TDD phase progression
        var result = _sequentialComposition.GetNextConstraintId(
            _tddSequence,
            _greenPhaseContext,
            completedConstraints);

        // Assert - Validates TDD progression logic
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("tdd.write-simplest-code"),
            "Should progress to GREEN phase after RED completion");
        Assert.That(result.Reason, Does.Contain("green"),
            "Should provide GREEN phase context guidance");
        Assert.That(result.Reason, Does.Contain("Step 2 of 3"),
            "Should show progression through TDD cycle");
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system can express TDD REFACTOR phase activation
    /// Original requirement: "Transition to REFACTOR when tests pass"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Express_TDD_REFACTOR_Phase_Through_Configuration()
    {
        // Arrange - RED and GREEN phases completed
        var completedConstraints = new HashSet<string>
        {
            "tdd.write-failing-test",
            "tdd.write-simplest-code"
        };

        // Act - Generic system handles final TDD phase
        var result = _sequentialComposition.GetNextConstraintId(
            _tddSequence,
            _refactorPhaseContext,
            completedConstraints);

        // Assert - Validates complete TDD cycle support
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("tdd.refactor-code"),
            "Should activate REFACTOR phase after GREEN completion");
        Assert.That(result.Reason, Does.Contain("refactor"),
            "Should provide REFACTOR phase guidance");
        Assert.That(result.Reason, Does.Contain("Step 3 of 3"),
            "Should indicate final step in TDD cycle");
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system can detect TDD cycle completion
    /// Original requirement: "Complete TDD cycle and restart"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Detect_Complete_TDD_Cycle_Through_Configuration()
    {
        // Arrange - All TDD phases completed
        var completedConstraints = new HashSet<string>
        {
            "tdd.write-failing-test",
            "tdd.write-simplest-code",
            "tdd.refactor-code"
        };

        // Act - Check cycle completion
        var isComplete = _sequentialComposition.IsSequenceComplete(_tddSequence, completedConstraints);
        var progress = _sequentialComposition.GetSequenceProgress(_tddSequence, completedConstraints);

        // Assert - Validates TDD cycle completion detection
        Assert.That(isComplete, Is.True, "Should detect complete TDD cycle");
        Assert.That(progress.ProgressPercentage, Is.EqualTo(1.0), "Should show 100% TDD cycle completion");
        Assert.That(progress.CompletedCount, Is.EqualTo(3), "Should count all three TDD phases as complete");
        Assert.That(progress.TotalCount, Is.EqualTo(3), "Should recognize full TDD cycle length");
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system supports TDD with different evaluation criteria
    /// Shows how different TDD contexts (unit tests, integration tests, etc.) can be expressed
    /// </summary>
    [Test]
    public void GenericSystem_Should_Support_Different_TDD_Evaluation_Contexts()
    {
        // Arrange - Different TDD evaluation contexts
        var unitTestContext = new UserDefinedContext("test-type", "unit", priority: 0.9);
        var integrationTestContext = new UserDefinedContext("test-type", "integration", priority: 0.8);

        var unitTestSequence = new List<string> { "tdd.unit-test-first", "tdd.unit-implementation" };
        var integrationSequence = new List<string> { "tdd.integration-test-first", "tdd.integration-implementation" };

        // Act - Generic system with different TDD contexts
        var unitResult = _sequentialComposition.GetNextConstraintId(
            unitTestSequence, unitTestContext, new HashSet<string>());
        var integrationResult = _sequentialComposition.GetNextConstraintId(
            integrationSequence, integrationTestContext, new HashSet<string>());

        // Assert - Validates flexible TDD expression
        Assert.That(unitResult.Value, Is.EqualTo("tdd.unit-test-first"));
        Assert.That(integrationResult.Value, Is.EqualTo("tdd.integration-test-first"));
        Assert.That(unitResult.Reason, Does.Contain("unit"));
        Assert.That(integrationResult.Reason, Does.Contain("integration"));
    }
}
