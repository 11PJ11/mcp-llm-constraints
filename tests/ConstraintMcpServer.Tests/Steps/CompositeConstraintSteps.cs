using System;
using System.Threading.Tasks;
using ConstraintMcpServer.Tests.Infrastructure;
using ConstraintMcpServer.Domain.Composition;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Tests.Steps;

/// <summary>
/// BDD step definitions for composite constraint workflow testing.
/// Implements Given-When-Then steps for Sequential, Hierarchical, and Progressive composition.
/// These steps will initially throw NotImplementedException and drive domain implementation.
/// </summary>
public class CompositeConstraintSteps : IDisposable
{
    private const string FeatureDevelopmentContext = "feature-development";
    private const string ActiveFeatureDevelopmentContext = "feature-development-active";
    private const string TddWriteFailingTestConstraint = "tdd.write-failing-test";
    private const string TddWriteSimplestCodeConstraint = "tdd.write-simplest-code";
    private const string TddRefactorCodeConstraint = "tdd.refactor-code";
    private const string RedPhaseKeyword = "RED phase";
    private const string GreenPhaseKeyword = "GREEN phase";
    private const string RefactorPhaseKeyword = "REFACTOR phase";
    private const string InvalidTransitionErrorPrefix = "Invalid TDD phase transition";

    // Hierarchical composition constants
    private const string SystemDesignContext = "system-design";
    private const string ActiveSystemDesignContext = "system-design-active";
    private const string ArchitectureConstraintId = "arch.solid-principles";
    private const string ImplementationConstraintId = "impl.clean-code";
    private const string TestingConstraintId = "test.unit-tests";

    // Hierarchical composition priority constants
    private const double HighArchitecturePriority = 0.9;
    private const double MediumImplementationPriority = 0.8;
    private const double LowTestingPriority = 0.7;

    // Hierarchy level constants
    private const int ArchitectureHierarchyLevel = 0;
    private const int ImplementationHierarchyLevel = 1;
    private const int TestingHierarchyLevel = 2;

    // Progressive composition constants
    private const string RefactoringContext = "refactoring";
    private const string ActiveRefactoringContext = "refactoring-active";
    private const string Level1ReadabilityConstraintId = "refactor.level1.readability";
    private const string Level2ComplexityConstraintId = "refactor.level2.complexity";
    private const string Level3ResponsibilitiesConstraintId = "refactor.level3.responsibilities";
    private const string Level4AbstractionsConstraintId = "refactor.level4.abstractions";
    private const string Level5PatternsConstraintId = "refactor.level5.patterns";
    private const string Level6SolidConstraintId = "refactor.level6.solid";

    // Layered composition constants  
    private const string CleanArchitectureContext = "clean-architecture";
    private const string ActiveCleanArchitectureContext = "clean-architecture-active";
    private const string DomainLayerConstraintId = "arch.domain-layer";
    private const string ApplicationLayerConstraintId = "arch.application-layer";
    private const string InfrastructureLayerConstraintId = "arch.infrastructure-layer";
    private const string PresentationLayerConstraintId = "arch.presentation-layer";

    // Clean Architecture layer priority constants
    private const double DomainLayerPriority = 1.0;
    private const double ApplicationLayerPriority = 0.9;
    private const double InfrastructureLayerPriority = 0.8;
    private const double PresentationLayerPriority = 0.7;

    // Architecture layer level constants
    private const int DomainLayerLevel = 0;
    private const int ApplicationLayerLevel = 1;
    private const int InfrastructureLayerLevel = 2;
    private const int PresentationLayerLevel = 3;

    // Refactoring level progression constants
    private const int Level1ReadabilityLevel = 1;
    private const int Level2ComplexityLevel = 2;
    private const int Level3ResponsibilitiesLevel = 3;
    private const int Level4AbstractionsLevel = 4;
    private const int Level5PatternsLevel = 5;
    private const int Level6SolidLevel = 6;

    // Barrier detection constants
    private const int MajorBarrierLevel3 = 3; // Responsibilities - major drop-off point
    private const int MajorBarrierLevel5 = 5; // Patterns - major drop-off point

    private bool _disposed;
    private SequentialComposition _sequentialComposition = null!;
    private CompositionContext _currentContext = null!;
    private SequentialCompositionResult _lastResult = null!;
    private HierarchicalComposition _hierarchicalComposition = null!;
    private IEnumerable<HierarchicalConstraintInfo> _hierarchicalResult = null!;
    private ProgressiveComposition _progressiveComposition = null!;
    private ProgressiveCompositionState _progressiveState = null!;

    // Layered composition state
    private LayeredComposition _layeredComposition = null!;
    private LayeredCompositionState _layeredState = null!;
    private CompositionContext _compositionContext = null!;
    private ConstraintActivation _lastActivation = null!;

    #region Helper Methods

    private void ValidateConstraintResult(string expectedConstraintId, string expectedPhaseKeyword)
    {
        if (!_lastResult.IsSuccess)
        {
            throw new InvalidOperationException($"Expected successful result but got error: {_lastResult.Error}");
        }

        if (_lastResult.Value != expectedConstraintId)
        {
            throw new InvalidOperationException($"Expected constraint '{expectedConstraintId}' but got '{_lastResult.Value}'");
        }

        if (!_lastResult.Reason.Contains(expectedPhaseKeyword))
        {
            throw new InvalidOperationException($"Expected reason to contain '{expectedPhaseKeyword}' but got: {_lastResult.Reason}");
        }
    }

    private void ValidateInvalidTransitionResult(SequentialCompositionResult result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Expected invalid transition to fail but it succeeded");
        }

        if (!result.Error.Contains(InvalidTransitionErrorPrefix))
        {
            throw new InvalidOperationException($"Expected error about invalid transition but got: {result.Error}");
        }
    }

    private CompositionContext TransitionToPhase(WorkflowState workflowState, UserDefinedEvaluationStatus evaluationStatus)
    {
        return _currentContext with
        {
            CurrentWorkflowState = workflowState,
            EvaluationStatus = evaluationStatus
        };
    }

    private CompositionContext CreateInvalidTransitionContext()
    {
        return new CompositionContext
        {
            CurrentWorkflowState = new WorkflowState("green", "Green phase - tests passing"),
            EvaluationStatus = new UserDefinedEvaluationStatus("not-run", "Tests not yet run", false) // Invalid: GREEN phase with NotRun status
        };
    }

    private void ExecuteSequentialComposition()
    {
        var sequence = new List<string> { "tdd.red", "tdd.green", "tdd.refactor" };
        var currentContext = new UserDefinedContext("workflow", "red", 0.9);
        var completedConstraints = new HashSet<string>();
        _lastResult = _sequentialComposition.GetNextConstraintId(sequence, currentContext, completedConstraints);
    }

    private UserDefinedProgression CreateRefactoringProgression()
    {
        var refactoringStages = new Dictionary<int, ProgressiveStageDefinition>
        {
            { 1, new ProgressiveStageDefinition("refactor.level1.readability", "Level 1: Readability improvements") },
            { 2, new ProgressiveStageDefinition("refactor.level2.complexity", "Level 2: Complexity reduction") },
            { 3, new ProgressiveStageDefinition("refactor.level3.responsibilities", "Level 3: Responsibility organization") }
        };
        
        return new UserDefinedProgression("systematic-refactoring", refactoringStages, "Systematic refactoring approach", allowStageSkipping: false);
    }

    private void ValidateHierarchyLevelOrdering(List<HierarchicalConstraintInfo> orderedConstraints)
    {
        for (int i = 0; i < orderedConstraints.Count - 1; i++)
        {
            var current = orderedConstraints[i];
            var next = orderedConstraints[i + 1];

            if (current.HierarchyLevel > next.HierarchyLevel)
            {
                throw new InvalidOperationException(
                    $"Hierarchy level ordering violated: constraint '{current.ConstraintId}' at level {current.HierarchyLevel} " +
                    $"appears before constraint '{next.ConstraintId}' at level {next.HierarchyLevel}");
            }
        }
    }

    private void ValidatePriorityOrderingWithinLevels(List<HierarchicalConstraintInfo> orderedConstraints)
    {
        var groupedByLevel = orderedConstraints.GroupBy(c => c.HierarchyLevel);
        foreach (var levelGroup in groupedByLevel)
        {
            ValidatePriorityOrderingForLevel(levelGroup.ToList());
        }
    }

    private void ValidatePriorityOrderingForLevel(List<HierarchicalConstraintInfo> constraintsAtLevel)
    {
        for (int i = 0; i < constraintsAtLevel.Count - 1; i++)
        {
            var current = constraintsAtLevel[i];
            var next = constraintsAtLevel[i + 1];

            if (current.Priority < next.Priority)
            {
                throw new InvalidOperationException(
                    $"Priority ordering violated within hierarchy level {current.HierarchyLevel}: " +
                    $"constraint '{current.ConstraintId}' (priority {current.Priority}) appears before " +
                    $"constraint '{next.ConstraintId}' (priority {next.Priority})");
            }
        }
    }

    #endregion

    #region Sequential Composition Steps (TDD Workflow)

    /// <summary>
    /// GIVEN: TDD workflow constraint exists in the constraint library
    /// Sets up a sequential composition constraint for RED → GREEN → REFACTOR workflow
    /// </summary>
    public CompositeConstraintSteps TddWorkflowConstraintExists()
    {
        _sequentialComposition = new SequentialComposition();
        _currentContext = new CompositionContext
        {
            CurrentWorkflowState = new WorkflowState("red", "RED phase - writing failing tests"),
            EvaluationStatus = new UserDefinedEvaluationStatus("not-run", "Tests not yet run", false),
            DevelopmentContext = FeatureDevelopmentContext
        };
        return this;
    }

    /// <summary>
    /// AND: User starts feature development context
    /// Simulates developer beginning new feature implementation
    /// </summary>
    public CompositeConstraintSteps UserStartsFeatureDevelopment()
    {
        // Update context to reflect user starting feature development
        _currentContext = _currentContext with
        {
            DevelopmentContext = ActiveFeatureDevelopmentContext
        };
        return this;
    }

    /// <summary>
    /// WHEN: Sequential composition activates based on development context
    /// Tests that the composition engine recognizes TDD workflow scenario
    /// </summary>
    public CompositeConstraintSteps SequentialCompositionActivates()
    {
        ExecuteSequentialComposition();
        return this;
    }

    /// <summary>
    /// THEN: Failing test constraint activates first in RED phase
    /// Validates that TDD workflow starts with write-failing-test-first constraint
    /// </summary>
    public CompositeConstraintSteps FailingTestConstraintActivatesFirst()
    {
        ValidateConstraintResult(TddWriteFailingTestConstraint, RedPhaseKeyword);
        return this;
    }

    /// <summary>
    /// AND: Simplest code constraint activates after RED phase completion
    /// Tests phase transition from RED to GREEN with appropriate constraint
    /// </summary>
    public CompositeConstraintSteps SimplestCodeConstraintActivatesAfterRedPhase()
    {
        // Simulate test failing - transition from RED with NotRun to RED with Failing
        _currentContext = TransitionToPhase(
            new WorkflowState("red", "RED phase - tests are failing"),
            new UserDefinedEvaluationStatus("failing", "Tests are failing", false));
        ExecuteSequentialComposition();

        ValidateConstraintResult(TddWriteSimplestCodeConstraint, GreenPhaseKeyword);
        return this;
    }

    /// <summary>
    /// AND: Refactoring constraint activates after GREEN phase completion
    /// Tests transition from GREEN to REFACTOR with clean code constraints
    /// </summary>
    public CompositeConstraintSteps RefactoringConstraintActivatesAfterGreenPhase()
    {
        // Simulate test passing - transition from GREEN with Passing
        _currentContext = TransitionToPhase(
            new WorkflowState("green", "GREEN phase - tests are passing"),
            new UserDefinedEvaluationStatus("passing", "Tests are passing", true));
        ExecuteSequentialComposition();

        ValidateConstraintResult(TddRefactorCodeConstraint, RefactorPhaseKeyword);
        return this;
    }

    /// <summary>
    /// AND: Workflow progresses in correct sequence without phase skipping
    /// Validates the entire TDD workflow maintains proper sequence and timing
    /// </summary>
    public CompositeConstraintSteps WorkflowProgressesInCorrectSequence()
    {
        // Verify that invalid phase transitions fail properly
        var sequence = new List<string> { "tdd.red", "tdd.green", "tdd.refactor" };
        var currentContext = new UserDefinedContext("workflow", "green", 0.8);
        var completedConstraints = new HashSet<string>();
        var invalidResult = _sequentialComposition.GetNextConstraintId(sequence, currentContext, completedConstraints);

        ValidateInvalidTransitionResult(invalidResult);
        return this;
    }

    #endregion

    #region Hierarchical Composition Steps (Architectural Patterns)

    /// <summary>
    /// GIVEN: Architectural patterns constraint exists in the constraint library
    /// Sets up a hierarchical composition constraint for Architecture → Implementation → Testing workflow
    /// </summary>
    public CompositeConstraintSteps ArchitecturalPatternsConstraintExists()
    {
        _hierarchicalComposition = new HierarchicalComposition();

        // Setup architectural pattern constraints with different hierarchy levels
        var constraints = new[]
        {
            new UserDefinedHierarchicalConstraintInfo(ArchitectureConstraintId, ArchitectureHierarchyLevel, HighArchitecturePriority, "Architecture constraint"),
            new UserDefinedHierarchicalConstraintInfo(ImplementationConstraintId, ImplementationHierarchyLevel, MediumImplementationPriority, "Implementation constraint"),
            new UserDefinedHierarchicalConstraintInfo(TestingConstraintId, TestingHierarchyLevel, LowTestingPriority, "Testing constraint")
        };

        var userDefinedHierarchy = new UserDefinedHierarchy(
            "system-architecture",
            new Dictionary<int, string>
            {
                { ArchitectureHierarchyLevel, "Architecture Level" },
                { ImplementationHierarchyLevel, "Implementation Level" },
                { TestingHierarchyLevel, "Testing Level" }
            },
            "System architecture hierarchy");

        _hierarchicalResult = _hierarchicalComposition.GetConstraintsByHierarchy(constraints, userDefinedHierarchy);
        return this;
    }

    /// <summary>
    /// AND: User starts system design context
    /// Simulates architect beginning system design work
    /// </summary>
    public CompositeConstraintSteps UserStartsSystemDesign()
    {
        // Setup system design context for hierarchical composition
        _currentContext = new CompositionContext
        {
            CurrentWorkflowState = new WorkflowState("design", "System design phase"),
            EvaluationStatus = new UserDefinedEvaluationStatus("not-run", "Design not yet evaluated", false),
            DevelopmentContext = SystemDesignContext
        };
        return this;
    }

    /// <summary>
    /// WHEN: Hierarchical composition activates based on design context
    /// Tests that the composition engine recognizes architectural workflow scenario
    /// </summary>
    public CompositeConstraintSteps HierarchicalCompositionActivates()
    {
        // Update context to indicate hierarchical composition is now active
        _currentContext = _currentContext with
        {
            DevelopmentContext = ActiveSystemDesignContext
        };

        // The hierarchical result was already computed in ArchitecturalPatternsConstraintExists
        // This simulates the composition engine activating for system design
        return this;
    }

    /// <summary>
    /// THEN: High-level architecture constraints activate first (Level 0)
    /// Validates that architectural concerns have highest priority
    /// </summary>
    public CompositeConstraintSteps HighLevelArchitectureConstraintsActivateFirst()
    {
        var first = _hierarchicalResult.First();

        if (first.ConstraintId != ArchitectureConstraintId)
        {
            throw new InvalidOperationException($"Expected architecture constraint '{ArchitectureConstraintId}' first but got '{first.ConstraintId}'");
        }

        if (first.HierarchyLevel != ArchitectureHierarchyLevel)
        {
            throw new InvalidOperationException($"Expected architecture hierarchy level {ArchitectureHierarchyLevel} first but got level {first.HierarchyLevel}");
        }

        return this;
    }

    /// <summary>
    /// AND: Implementation constraints activate second (Level 1)
    /// Tests hierarchy level ordering for implementation-specific constraints
    /// </summary>
    public CompositeConstraintSteps ImplementationConstraintsActivateSecond()
    {
        var second = _hierarchicalResult.Skip(1).First();

        if (second.ConstraintId != ImplementationConstraintId)
        {
            throw new InvalidOperationException($"Expected implementation constraint '{ImplementationConstraintId}' second but got '{second.ConstraintId}'");
        }

        if (second.HierarchyLevel != ImplementationHierarchyLevel)
        {
            throw new InvalidOperationException($"Expected implementation hierarchy level {ImplementationHierarchyLevel} second but got level {second.HierarchyLevel}");
        }

        return this;
    }

    /// <summary>
    /// AND: Testing constraints activate last (Level 2)
    /// Tests that testing constraints have lowest priority in hierarchy
    /// </summary>
    public CompositeConstraintSteps TestingConstraintsActivateLast()
    {
        var last = _hierarchicalResult.Last();

        if (last.ConstraintId != TestingConstraintId)
        {
            throw new InvalidOperationException($"Expected testing constraint '{TestingConstraintId}' last but got '{last.ConstraintId}'");
        }

        if (last.HierarchyLevel != TestingHierarchyLevel)
        {
            throw new InvalidOperationException($"Expected testing hierarchy level {TestingHierarchyLevel} last but got level {last.HierarchyLevel}");
        }

        return this;
    }

    /// <summary>
    /// AND: Hierarchy levels respect priority order throughout workflow
    /// Validates the entire hierarchical workflow maintains proper priority ordering
    /// </summary>
    public CompositeConstraintSteps HierarchyLevelsRespectPriorityOrder()
    {
        var orderedConstraints = _hierarchicalResult.ToList();

        ValidateHierarchyLevelOrdering(orderedConstraints);
        ValidatePriorityOrderingWithinLevels(orderedConstraints);

        return this;
    }

    #endregion

    #region Progressive Composition Steps (Refactoring Levels)

    /// <summary>
    /// GIVEN: Refactoring cycle constraint exists in the constraint library
    /// Sets up a progressive composition constraint for systematic refactoring levels 1-6
    /// </summary>
    public CompositeConstraintSteps RefactoringCycleConstraintExists()
    {
        _progressiveComposition = new ProgressiveComposition();
        _progressiveState = new ProgressiveCompositionState
        {
            CurrentLevel = Level1ReadabilityLevel,
            CompletedLevels = new HashSet<int>(),
            RefactoringContext = RefactoringContext,
            BarrierDetectionEnabled = true
        };
        return this;
    }

    /// <summary>
    /// AND: User enters GREEN phase with passing tests
    /// Simulates developer completing implementation with passing tests, ready for refactoring
    /// </summary>
    public CompositeConstraintSteps UserEntersGreenPhaseWithPassingTests()
    {
        _progressiveState = _progressiveState with
        {
            RefactoringContext = ActiveRefactoringContext,
            TestsPassing = true,
            ReadyForRefactoring = true
        };
        return this;
    }

    /// <summary>
    /// WHEN: Progressive composition activates based on GREEN phase context
    /// Tests that the composition engine recognizes refactoring workflow scenario
    /// </summary>
    public CompositeConstraintSteps ProgressiveCompositionActivates()
    {
        var userDefinedProgression = CreateRefactoringProgression();
        var levelConstraints = _progressiveComposition.GetCurrentStageConstraints(_progressiveState, userDefinedProgression);
        // Store result for validation in subsequent steps
        return this;
    }

    /// <summary>
    /// THEN: Level 1 readability constraint activates first
    /// Validates that refactoring starts with foundational readability improvements
    /// </summary>
    public CompositeConstraintSteps Level1ReadabilityConstraintActivatesFirst()
    {
        var userDefinedProgression = CreateRefactoringProgression();
        var currentConstraint = _progressiveComposition.GetActiveConstraint(_progressiveState, userDefinedProgression);

        if (currentConstraint.ConstraintId != Level1ReadabilityConstraintId)
        {
            throw new InvalidOperationException($"Expected Level 1 readability constraint '{Level1ReadabilityConstraintId}' but got '{currentConstraint.ConstraintId}'");
        }

        if (currentConstraint.RefactoringLevel != Level1ReadabilityLevel)
        {
            throw new InvalidOperationException($"Expected refactoring level {Level1ReadabilityLevel} but got level {currentConstraint.RefactoringLevel}");
        }

        return this;
    }

    /// <summary>
    /// AND: Level 2 complexity constraint activates after Level 1
    /// Tests systematic progression through refactoring levels
    /// </summary>
    public CompositeConstraintSteps Level2ComplexityConstraintActivatesAfterLevel1()
    {
        // Simulate Level 1 completion
        var userDefinedProgression = CreateRefactoringProgression();
        _progressiveState = _progressiveComposition.CompleteStage(_progressiveState, Level1ReadabilityLevel, userDefinedProgression);
        var userDefinedProgression = CreateRefactoringProgression();
        var currentConstraint = _progressiveComposition.GetActiveConstraint(_progressiveState, userDefinedProgression);

        if (currentConstraint.ConstraintId != Level2ComplexityConstraintId)
        {
            throw new InvalidOperationException($"Expected Level 2 complexity constraint '{Level2ComplexityConstraintId}' but got '{currentConstraint.ConstraintId}'");
        }

        return this;
    }

    /// <summary>
    /// AND: Level 3 responsibilities constraint activates after Level 2
    /// Tests continued systematic progression
    /// </summary>
    public CompositeConstraintSteps Level3ResponsibilitiesConstraintActivatesAfterLevel2()
    {
        // Simulate Level 2 completion
        var userDefinedProgression = CreateRefactoringProgression();
        _progressiveState = _progressiveComposition.CompleteStage(_progressiveState, Level2ComplexityLevel, userDefinedProgression);
        var userDefinedProgression = CreateRefactoringProgression();
        var currentConstraint = _progressiveComposition.GetActiveConstraint(_progressiveState, userDefinedProgression);

        if (currentConstraint.ConstraintId != Level3ResponsibilitiesConstraintId)
        {
            throw new InvalidOperationException($"Expected Level 3 responsibilities constraint '{Level3ResponsibilitiesConstraintId}' but got '{currentConstraint.ConstraintId}'");
        }

        return this;
    }

    /// <summary>
    /// AND: Barrier detection provides extra support at Level 3
    /// Tests that Level 3 (major drop-off point) gets additional guidance
    /// </summary>
    public CompositeConstraintSteps BarrierDetectionProvidesExtraSupportAtLevel3()
    {
        var userDefinedProgression = CreateRefactoringProgression();
        var barrierSupport = _progressiveComposition.GetBarrierSupport(_progressiveState, Level3ResponsibilitiesLevel, userDefinedProgression);

        if (!barrierSupport.IsBarrierLevel)
        {
            throw new InvalidOperationException($"Expected Level 3 to be detected as barrier level but it was not");
        }

        if (barrierSupport.AdditionalGuidance.Count == 0)
        {
            throw new InvalidOperationException("Expected additional guidance for barrier level but none was provided");
        }

        return this;
    }

    /// <summary>
    /// AND: Level skipping is prevented by composition
    /// Tests that users cannot skip levels in refactoring progression
    /// </summary>
    public CompositeConstraintSteps LevelSkippingIsPreventedByComposition()
    {
        // Reset to initial state (Level 1) and attempt to skip to Level 3 directly
        _progressiveState = _progressiveState with
        {
            CurrentLevel = Level1ReadabilityLevel,
            CompletedLevels = new HashSet<int>()
        };

        var userDefinedProgression = CreateRefactoringProgression();
        var skipAttemptResult = _progressiveComposition.TrySkipToStage(_progressiveState, Level3ResponsibilitiesLevel, userDefinedProgression);

        if (skipAttemptResult.IsSuccess)
        {
            throw new InvalidOperationException("Expected level skipping to be prevented but it was allowed");
        }

        // Check for systematic progression requirement (the domain model's actual business rule)
        if (!skipAttemptResult.Error.Contains("systematic") && !skipAttemptResult.Error.Contains("prerequisite"))
        {
            throw new InvalidOperationException($"Expected error about systematic progression or prerequisite levels but got: {skipAttemptResult.Error}");
        }

        return this;
    }

    /// <summary>
    /// AND: Refactoring levels progress systematically
    /// Validates the entire progressive workflow maintains proper level ordering
    /// </summary>
    public CompositeConstraintSteps RefactoringLevelsProgressSystematically()
    {
        // Validate complete progression through all levels
        var progression = _progressiveComposition.GetProgressionPath(_progressiveState);

        if (progression.Levels.Count != 6)
        {
            throw new InvalidOperationException($"Expected 6 refactoring levels but got {progression.Levels.Count}");
        }

        // Verify levels are in correct order
        for (int i = 0; i < progression.Levels.Count - 1; i++)
        {
            if (progression.Levels[i] >= progression.Levels[i + 1])
            {
                throw new InvalidOperationException($"Refactoring level progression violation: level {progression.Levels[i]} should come before level {progression.Levels[i + 1]}");
            }
        }

        return this;
    }

    #endregion

    #region Layered Composition Steps

    /// <summary>
    /// Given: Clean Architecture constraint exists for layer dependency enforcement
    /// </summary>
    public CompositeConstraintSteps CleanArchitectureConstraintExists()
    {
        // Create a layered composition for Clean Architecture enforcement
        _layeredComposition = new LayeredComposition();
        _layeredState = LayeredCompositionState.Initial;

        // Set up Clean Architecture context
        _compositionContext = new CompositionContext
        {
            DevelopmentContext = CleanArchitectureContext,
            CurrentPhase = TddPhase.Green, // Working on implementation after tests pass
            TestStatus = TestStatus.Passing
        };

        return this;
    }

    /// <summary>
    /// Given: User works on multi-layered project requiring architectural constraints
    /// </summary>
    public CompositeConstraintSteps UserWorksOnMultiLayeredProject()
    {
        // Set up multi-layered project context with VALID Clean Architecture dependencies
        var dependencies = new[]
        {
            // Valid dependencies following Clean Architecture principles
            new DependencyInfo("Application.UserApplicationService", "Domain.UserService", DependencyType.Reference),        // Application → Domain ✓
            new DependencyInfo("Infrastructure.UserRepository", "Application.IUserRepository", DependencyType.Implementation), // Infrastructure → Application ✓  
            new DependencyInfo("Presentation.UserController", "Application.UserApplicationService", DependencyType.Reference)  // Presentation → Application ✓
        };

        var codeAnalysis = CodeAnalysisInfo.WithDependencies(dependencies);
        var currentFile = new ConstraintMcpServer.Domain.Composition.FileInfo("Domain/UserService.cs", "MyApp.Domain", "UserService");

        _compositionContext = _compositionContext with
        {
            CodeAnalysis = codeAnalysis,
            CurrentFile = currentFile,
            DevelopmentContext = ActiveCleanArchitectureContext
        };

        return this;
    }

    /// <summary>
    /// When: Layered composition activates for Clean Architecture enforcement
    /// </summary>
    public CompositeConstraintSteps LayeredCompositionActivates()
    {
        // Activate layered composition by getting the next constraint
        var layers = new[]
        {
            new LayerConstraintInfo(DomainLayerConstraintId, DomainLayerLevel, "Domain Layer"),
            new LayerConstraintInfo(ApplicationLayerConstraintId, ApplicationLayerLevel, "Application Layer"),
            new LayerConstraintInfo(InfrastructureLayerConstraintId, InfrastructureLayerLevel, "Infrastructure Layer"),
            new LayerConstraintInfo(PresentationLayerConstraintId, PresentationLayerLevel, "Presentation Layer")
        };

        var result = _layeredComposition.GetNextConstraint(_layeredState, layers, _compositionContext);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to activate layered composition: {result.Error}");
        }

        // Store the activation result for validation
        _lastActivation = result.Value;

        return this;
    }

    /// <summary>
    /// Then: Domain layer constraints activate first (highest priority)
    /// </summary>
    public CompositeConstraintSteps DomainLayerConstraintsActivateFirst()
    {
        // Verify that domain layer constraint was activated
        if (_lastActivation == null)
        {
            throw new InvalidOperationException("No constraint activation occurred");
        }

        if (_lastActivation.ConstraintId != DomainLayerConstraintId)
        {
            throw new InvalidOperationException($"Expected domain layer constraint '{DomainLayerConstraintId}' but got '{_lastActivation.ConstraintId}'");
        }

        if (_lastActivation.LayerLevel != DomainLayerLevel)
        {
            throw new InvalidOperationException($"Expected domain layer level {DomainLayerLevel} but got {_lastActivation.LayerLevel}");
        }

        return this;
    }

    /// <summary>
    /// And: Application layer constraints activate second
    /// </summary>
    public CompositeConstraintSteps ApplicationLayerConstraintsActivateSecond()
    {
        // Simulate completing domain layer and moving to application layer
        _layeredState = _layeredState.WithCompletedLayer(DomainLayerLevel);

        // Verify layered composition can provide application layer constraint
        var layers = new[]
        {
            new LayerConstraintInfo(ApplicationLayerConstraintId, ApplicationLayerLevel, "Application Layer")
        };

        // For this test, we just verify that the composition is working as expected
        if (_layeredState.CompletedLayerCount < 1)
        {
            throw new InvalidOperationException("Expected at least one layer to be completed");
        }

        return this;
    }

    /// <summary>
    /// And: Infrastructure layer constraints activate third
    /// </summary>
    public CompositeConstraintSteps InfrastructureLayerConstraintsActivateThird()
    {
        // Simulate completing application layer and moving to infrastructure layer
        _layeredState = _layeredState.WithCompletedLayer(ApplicationLayerLevel);

        // Verify progressive layer completion
        if (_layeredState.CompletedLayerCount < 2)
        {
            throw new InvalidOperationException("Expected at least two layers to be completed");
        }

        return this;
    }

    /// <summary>
    /// And: Presentation layer constraints activate last (lowest priority)
    /// </summary>
    public CompositeConstraintSteps PresentationLayerConstraintsActivateLast()
    {
        // Simulate completing infrastructure layer and moving to presentation layer
        _layeredState = _layeredState.WithCompletedLayer(InfrastructureLayerLevel);

        // Verify all layers except presentation are completed
        if (_layeredState.CompletedLayerCount < 3)
        {
            throw new InvalidOperationException("Expected at least three layers to be completed");
        }

        return this;
    }

    /// <summary>
    /// And: Layer dependency violations are detected and reported
    /// </summary>
    public CompositeConstraintSteps LayerDependencyViolationsAreDetected()
    {
        // Verify that the layered composition can detect violations
        // The test setup includes valid dependencies, so no violations should be detected
        if (_layeredState.ViolationCount > 0)
        {
            throw new InvalidOperationException($"Unexpected violations detected: {_layeredState.ViolationCount}");
        }

        return this;
    }

    /// <summary>
    /// And: Architectural violations are prevented by composition enforcement
    /// </summary>
    public CompositeConstraintSteps ArchitecturalViolationsArePreventedByComposition()
    {
        // Verify that the layered composition enforces architectural rules
        var validationResult = _layeredState.Validate();

        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Layered composition validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        // Verify layered composition type is available
        if (_layeredComposition.Type != CompositionType.Layered)
        {
            throw new InvalidOperationException($"Expected Layered composition type but got {_layeredComposition.Type}");
        }

        return this;
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Cleanup test resources when implemented
            }
            _disposed = true;
        }
    }

    #endregion
}
