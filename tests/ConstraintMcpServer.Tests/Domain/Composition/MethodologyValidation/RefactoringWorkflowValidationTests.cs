using System.Collections.Generic;
using NUnit.Framework;
using ConstraintMcpServer.Domain.Composition;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Tests.Domain.Composition.MethodologyValidation;

/// <summary>
/// Validation tests proving that our generic system can correctly express systematic refactoring methodology.
/// These tests demonstrate that the methodology-agnostic ProgressiveComposition can support
/// the original 6-level refactoring hierarchy through user-defined configuration.
/// </summary>
[TestFixture]
[Category("MethodologyValidation")]
[Category("Refactoring")]
public class RefactoringWorkflowValidationTests
{
    private required ProgressiveComposition _progressiveComposition;
    private required UserDefinedProgression _refactoringProgression;
    private required ProgressiveCompositionState _initialState;

    [SetUp]
    public void SetUp()
    {
        _progressiveComposition = new ProgressiveComposition();

        // Configure 6-level refactoring hierarchy through generic system
        var refactoringStages = new Dictionary<int, ProgressiveStageDefinition>
        {
            { 1, new ProgressiveStageDefinition("refactor.level1.readability", "Level 1: Focus on readability improvements") },
            { 2, new ProgressiveStageDefinition("refactor.level2.complexity", "Level 2: Reduce complexity and duplication") },
            { 3, ProgressiveStageDefinition.WithBarrierGuidance(
                "refactor.level3.responsibilities",
                "Level 3: Reorganize responsibilities",
                new[] { "Level 3 is a common drop-off point - take your time", "Focus on Single Responsibility Principle" }) },
            { 4, new ProgressiveStageDefinition("refactor.level4.abstractions", "Level 4: Refine abstractions") },
            { 5, ProgressiveStageDefinition.WithBarrierGuidance(
                "refactor.level5.patterns",
                "Level 5: Apply design patterns",
                new[] { "Level 5 patterns require deeper architectural thinking", "Start with simple patterns" }) },
            { 6, new ProgressiveStageDefinition("refactor.level6.solid", "Level 6: Apply SOLID principles") }
        };

        _refactoringProgression = new UserDefinedProgression(
            "systematic-refactoring",
            refactoringStages,
            "Six-level systematic refactoring approach",
            allowStageSkipping: false); // Enforce systematic progression

        _initialState = new ProgressiveCompositionState(
            currentLevel: 1,
            completedLevels: new HashSet<int>());
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system can express Level 1 (Readability) refactoring
    /// Original requirement: "Start with readability improvements - comments, naming, dead code"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Express_Refactoring_Level1_Readability_Through_Configuration()
    {
        // Act - Generic system configured for refactoring methodology
        var activeConstraint = _progressiveComposition.GetActiveConstraint(_initialState, _refactoringProgression);
        var levelConstraints = _progressiveComposition.GetCurrentStageConstraints(_initialState, _refactoringProgression);

        // Assert - Validates Level 1 refactoring support
        Assert.That(activeConstraint.ConstraintId, Is.EqualTo("refactor.level1.readability"));
        Assert.That(activeConstraint.Level, Is.EqualTo(1));
        Assert.That(activeConstraint.Description, Does.Contain("readability"));
        Assert.That(levelConstraints, Contains.Item(activeConstraint));
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system enforces systematic progression (Level 1 → 2)
    /// Original requirement: "Progress through levels systematically, no level skipping"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Enforce_Systematic_Refactoring_Progression()
    {
        // Arrange - Complete Level 1
        var level1CompletedState = _progressiveComposition.CompleteStage(_initialState, 1, _refactoringProgression);

        // Act - Generic system handles level progression
        var nextConstraint = _progressiveComposition.GetActiveConstraint(level1CompletedState, _refactoringProgression);

        // Assert - Validates systematic progression
        Assert.That(level1CompletedState.CurrentLevel, Is.EqualTo(2), "Should progress to Level 2 after Level 1 completion");
        Assert.That(level1CompletedState.CompletedLevels, Contains.Item(1), "Should track Level 1 as completed");
        Assert.That(nextConstraint.ConstraintId, Is.EqualTo("refactor.level2.complexity"));
        Assert.That(nextConstraint.Description, Does.Contain("complexity"));
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system prevents level skipping
    /// Original requirement: "Cannot skip levels in systematic refactoring"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Prevent_Refactoring_Level_Skipping_Through_Configuration()
    {
        // Act - Attempt to skip from Level 1 to Level 3
        var skipResult = _progressiveComposition.TrySkipToStage(_initialState, 3, _refactoringProgression);

        // Assert - Validates level skipping prevention
        Assert.That(skipResult.IsSuccess, Is.False, "Should prevent level skipping in systematic refactoring");
        Assert.That(skipResult.ErrorMessage, Does.Contain("systematic"),
            "Should explain systematic progression requirement");
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system supports barrier stages (Level 3)
    /// Original requirement: "Level 3 (Responsibilities) is a common barrier - provide extra support"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Provide_Barrier_Support_For_Refactoring_Level3()
    {
        // Arrange - Progress to Level 3 (Responsibilities barrier)
        var level3State = new ProgressiveCompositionState(
            currentLevel: 3,
            completedLevels: new HashSet<int> { 1, 2 });

        // Act - Check barrier support
        var barrierSupport = _progressiveComposition.GetBarrierSupport(level3State, 3, _refactoringProgression);

        // Assert - Validates barrier support for Level 3
        Assert.That(barrierSupport.IsBarrierStage, Is.True, "Level 3 should be recognized as barrier stage");
        Assert.That(barrierSupport.Guidance, Is.Not.Empty, "Should provide guidance for barrier stage");
        Assert.That(barrierSupport.Guidance.First(), Does.Contain("drop-off point"),
            "Should include specific Level 3 barrier guidance");
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system supports Level 5 pattern barrier
    /// Original requirement: "Level 5 (Patterns) requires deeper architectural thinking"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Provide_Barrier_Support_For_Refactoring_Level5_Patterns()
    {
        // Arrange - Progress to Level 5 (Patterns barrier)
        var level5State = new ProgressiveCompositionState(
            currentLevel: 5,
            completedLevels: new HashSet<int> { 1, 2, 3, 4 });

        // Act - Check Level 5 barrier support
        var barrierSupport = _progressiveComposition.GetBarrierSupport(level5State, 5, _refactoringProgression);

        // Assert - Validates Level 5 barrier support
        Assert.That(barrierSupport.IsBarrierStage, Is.True, "Level 5 should be recognized as barrier stage");
        Assert.That(barrierSupport.Guidance.Any(g => g.Contains("architectural thinking")), Is.True,
            "Should provide Level 5 specific guidance about architectural thinking");
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system supports complete refactoring progression path
    /// Original requirement: "Provide complete progression through all 6 refactoring levels"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Support_Complete_Refactoring_Progression_Path()
    {
        // Act - Get complete progression path
        var progressionPath = _progressiveComposition.GetProgressionPath(_initialState, _refactoringProgression);

        // Assert - Validates complete refactoring support
        Assert.That(progressionPath.Levels, Has.Count.EqualTo(6), "Should support all 6 refactoring levels");
        Assert.That(progressionPath.Levels, Is.EquivalentTo(new[] { 1, 2, 3, 4, 5, 6 }));

        // Verify specific level descriptions match original refactoring methodology
        Assert.That(progressionPath.LevelDescriptions[1], Does.Contain("readability"));
        Assert.That(progressionPath.LevelDescriptions[2], Does.Contain("complexity"));
        Assert.That(progressionPath.LevelDescriptions[3], Does.Contain("responsibilities"));
        Assert.That(progressionPath.LevelDescriptions[4], Does.Contain("abstractions"));
        Assert.That(progressionPath.LevelDescriptions[5], Does.Contain("patterns"));
        Assert.That(progressionPath.LevelDescriptions[6], Does.Contain("SOLID"));
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system can be configured for different refactoring approaches
    /// Shows how the same progressive system can support various refactoring methodologies
    /// </summary>
    [Test]
    public void GenericSystem_Should_Support_Alternative_Refactoring_Methodologies()
    {
        // Arrange - Configure different refactoring approach (3-level simple)
        var simpleRefactoringStages = new Dictionary<int, ProgressiveStageDefinition>
        {
            { 1, new ProgressiveStageDefinition("simple.clean", "Clean up code") },
            { 2, new ProgressiveStageDefinition("simple.structure", "Improve structure") },
            { 3, new ProgressiveStageDefinition("simple.optimize", "Optimize performance") }
        };

        var simpleProgression = new UserDefinedProgression(
            "simple-refactoring",
            simpleRefactoringStages,
            "Simple 3-level refactoring",
            allowStageSkipping: true); // Different approach allows skipping

        // Act - Test alternative methodology
        var simpleConstraint = _progressiveComposition.GetActiveConstraint(_initialState, simpleProgression);
        var skipResult = _progressiveComposition.TrySkipToStage(_initialState, 3, simpleProgression);

        // Assert - Validates flexible refactoring methodology support
        Assert.That(simpleConstraint.ConstraintId, Is.EqualTo("simple.clean"));
        Assert.That(skipResult.IsSuccess, Is.True, "Alternative methodology should allow stage skipping");
    }
}
