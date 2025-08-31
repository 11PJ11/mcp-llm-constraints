using System;
using System.Threading.Tasks;
using ConstraintMcpServer.Tests.Infrastructure;
using ConstraintMcpServer.Domain.Composition;

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

    private bool _disposed;
    private SequentialComposition _sequentialComposition = null!;
    private CompositionStrategyContext _currentContext = null!;
    private SequentialCompositionResult _lastResult = null!;
    private HierarchicalComposition _hierarchicalComposition = null!;
    private IEnumerable<HierarchicalConstraintInfo> _hierarchicalResult = null!;

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

    private CompositionStrategyContext TransitionToPhase(TddPhase phase, TestStatus testStatus)
    {
        return _currentContext with
        {
            CurrentPhase = phase,
            TestStatus = testStatus
        };
    }

    private CompositionStrategyContext CreateInvalidTransitionContext()
    {
        return new CompositionStrategyContext
        {
            CurrentPhase = TddPhase.Green,
            TestStatus = TestStatus.NotRun // Invalid: GREEN phase with NotRun status
        };
    }

    private void ExecuteSequentialComposition()
    {
        _lastResult = _sequentialComposition.GetNextConstraintId(_currentContext);
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
        _currentContext = new CompositionStrategyContext
        {
            CurrentPhase = TddPhase.Red,
            TestStatus = TestStatus.NotRun,
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
        _currentContext = TransitionToPhase(TddPhase.Red, TestStatus.Failing);
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
        _currentContext = TransitionToPhase(TddPhase.Green, TestStatus.Passing);
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
        var invalidContext = CreateInvalidTransitionContext();
        var invalidResult = _sequentialComposition.GetNextConstraintId(invalidContext);

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
            new HierarchicalConstraintInfo(ArchitectureConstraintId, ArchitectureHierarchyLevel, HighArchitecturePriority),
            new HierarchicalConstraintInfo(ImplementationConstraintId, ImplementationHierarchyLevel, MediumImplementationPriority),
            new HierarchicalConstraintInfo(TestingConstraintId, TestingHierarchyLevel, LowTestingPriority)
        };
        
        _hierarchicalResult = _hierarchicalComposition.GetConstraintsByHierarchy(constraints);
        return this;
    }

    /// <summary>
    /// AND: User starts system design context
    /// Simulates architect beginning system design work
    /// </summary>
    public CompositeConstraintSteps UserStartsSystemDesign()
    {
        // Setup system design context for hierarchical composition
        _currentContext = new CompositionStrategyContext
        {
            CurrentPhase = TddPhase.Red, // Starting design phase
            TestStatus = TestStatus.NotRun,
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
            throw new InvalidOperationException($"Expected architecture constraint '{ArchitectureConstraintId}' first but got '{first.ConstraintId}'");
        
        if (first.HierarchyLevel != ArchitectureHierarchyLevel)
            throw new InvalidOperationException($"Expected architecture hierarchy level {ArchitectureHierarchyLevel} first but got level {first.HierarchyLevel}");
        
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
            throw new InvalidOperationException($"Expected implementation constraint '{ImplementationConstraintId}' second but got '{second.ConstraintId}'");
        
        if (second.HierarchyLevel != ImplementationHierarchyLevel)
            throw new InvalidOperationException($"Expected implementation hierarchy level {ImplementationHierarchyLevel} second but got level {second.HierarchyLevel}");
        
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
            throw new InvalidOperationException($"Expected testing constraint '{TestingConstraintId}' last but got '{last.ConstraintId}'");
        
        if (last.HierarchyLevel != TestingHierarchyLevel)
            throw new InvalidOperationException($"Expected testing hierarchy level {TestingHierarchyLevel} last but got level {last.HierarchyLevel}");
        
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
