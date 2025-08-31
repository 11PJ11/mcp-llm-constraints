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

    private bool _disposed;
    private SequentialComposition _sequentialComposition = null!;
    private CompositionStrategyContext _currentContext = null!;
    private SequentialCompositionResult _lastResult = null!;

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
