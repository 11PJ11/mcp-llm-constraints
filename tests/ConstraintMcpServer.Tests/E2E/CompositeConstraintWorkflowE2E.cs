using ConstraintMcpServer.Tests.Infrastructure;
using ConstraintMcpServer.Tests.Steps;
using ConstraintMcpServer.Tests.Framework;
using NUnit.Framework;
using static ConstraintMcpServer.Tests.Framework.ScenarioBuilder;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// End-to-end tests for composite constraint workflow orchestration.
/// Tests complex methodology workflows: Sequential (TDD), Hierarchical (Outside-In), Progressive (Refactoring Levels).
/// Following ATDD/BDD Outside-In approach - these tests drive domain implementation.
/// </summary>
[TestFixture]
[Category("E2E")]
public class CompositeConstraintWorkflowE2E
{
    private CompositeConstraintSteps _steps = null!;

    [SetUp]
    public void SetUp()
    {
        _steps = new CompositeConstraintSteps();
    }

    [TearDown]
    public void TearDown()
    {
        _steps?.Dispose();
    }

    /// <summary>
    /// Business Scenario: Developer starts TDD workflow for new feature development
    /// Expected Behavior: System orchestrates RED → GREEN → REFACTOR sequence with proper phase transitions
    /// 
    /// This E2E test will FAIL initially and drive the entire Sequential Composition implementation.
    /// </summary>
    [Test]
    public async Task Should_Orchestrate_TDD_Workflow_With_Sequential_Composition()
    {
        // ATDD/BDD: This test defines "done" for Sequential Composition
        // Will stay RED until proper domain implementation through inner TDD loops
        await Given(() => _steps.TddWorkflowConstraintExists())
            .And(() => _steps.UserStartsFeatureDevelopment())
            .When(() => _steps.SequentialCompositionActivates())
            .Then(() => _steps.FailingTestConstraintActivatesFirst())
            .And(() => _steps.SimplestCodeConstraintActivatesAfterRedPhase())
            .And(() => _steps.RefactoringConstraintActivatesAfterGreenPhase())
            .And(() => _steps.WorkflowProgressesInCorrectSequence())
            .ExecuteAsync();
    }

    // Additional E2E tests will be implemented one at a time following Outside-In TDD
    // Each test will drive specific composition strategy implementation
}
