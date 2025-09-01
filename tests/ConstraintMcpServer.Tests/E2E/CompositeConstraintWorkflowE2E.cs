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

    /// <summary>
    /// E2E Test 2: Hierarchical Composition for Architectural Patterns
    /// Tests constraint prioritization based on hierarchy levels (Architecture → Implementation → Testing)
    /// This test will drive HierarchicalComposition implementation through inner TDD loops
    /// </summary>
    [Test]
    public async Task Should_Orchestrate_Architectural_Patterns_With_Hierarchical_Composition()
    {
        await Given(() => _steps.ArchitecturalPatternsConstraintExists())
            .And(() => _steps.UserStartsSystemDesign())
            .When(() => _steps.HierarchicalCompositionActivates())
            .Then(() => _steps.HighLevelArchitectureConstraintsActivateFirst())
            .And(() => _steps.ImplementationConstraintsActivateSecond())
            .And(() => _steps.TestingConstraintsActivateLast())
            .And(() => _steps.HierarchyLevelsRespectPriorityOrder())
            .ExecuteAsync();
    }

    /// <summary>
    /// E2E Test 3: Progressive Composition for Systematic Refactoring Levels
    /// Tests systematic progression through refactoring levels 1-6 with barrier detection
    /// This test will drive ProgressiveComposition implementation through inner TDD loops
    /// </summary>
    [Test]
    public async Task Should_Guide_Through_Refactoring_Levels_With_Progressive_Composition()
    {
        // Business Scenario: Developer enters GREEN phase and needs systematic refactoring guidance
        // Expected: System guides through levels 1-6 without skipping, with barrier support

        // This E2E test will FAIL initially and drive the entire Progressive Composition implementation
        await Given(() => _steps.RefactoringCycleConstraintExists())
            .And(() => _steps.UserEntersGreenPhaseWithPassingTests())
            .When(() => _steps.ProgressiveCompositionActivates())
            .Then(() => _steps.Level1ReadabilityConstraintActivatesFirst())
            .And(() => _steps.Level2ComplexityConstraintActivatesAfterLevel1())
            .And(() => _steps.Level3ResponsibilitiesConstraintActivatesAfterLevel2())
            .And(() => _steps.BarrierDetectionProvidesExtraSupportAtLevel3())
            .And(() => _steps.LevelSkippingIsPreventedByComposition())
            .And(() => _steps.RefactoringLevelsProgressSystematically())
            .ExecuteAsync();
    }

    /// <summary>
    /// E2E Test 4: Layered Composition for Clean Architecture Enforcement
    /// Tests architectural layer dependency validation and constraint enforcement
    /// This test will drive LayeredComposition implementation through inner TDD loops
    /// </summary>
    [Test]
    public async Task Should_Enforce_Clean_Architecture_With_Layered_Composition()
    {
        // Business Scenario: Developer works on Clean Architecture project and needs layer dependency enforcement
        // Expected: System prevents architectural violations and enforces proper layer dependencies

        // This E2E test will FAIL initially and drive the entire Layered Composition implementation
        await Given(() => _steps.CleanArchitectureConstraintExists())
            .And(() => _steps.UserWorksOnMultiLayeredProject())
            .When(() => _steps.LayeredCompositionActivates())
            .Then(() => _steps.DomainLayerConstraintsActivateFirst())
            .And(() => _steps.ApplicationLayerConstraintsActivateSecond())
            .And(() => _steps.InfrastructureLayerConstraintsActivateThird())
            .And(() => _steps.PresentationLayerConstraintsActivateLast())
            .And(() => _steps.LayerDependencyViolationsAreDetected())
            .And(() => _steps.ArchitecturalViolationsArePreventedByComposition())
            .ExecuteAsync();
    }

    // Additional E2E tests will be implemented one at a time following Outside-In TDD
    // Each test will drive specific composition strategy implementation
}
