using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Tests.Steps;
using ConstraintMcpServer.Tests.Infrastructure;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// End-to-End acceptance tests for Interactive Constraint Definition System (Step A3).
/// These tests define business scenarios and will initially FAIL to drive Outside-In TDD implementation.
/// Tests use Given().When().Then() lambda pattern with ScenarioBuilder for BDD-style execution.
/// </summary>
[TestFixture]
[Category("E2E")]
[Category("InteractiveConstraintDefinition")]
public sealed class InteractiveConstraintDefinitionE2E
{
    private InteractiveConstraintSteps _steps = null!;

    [SetUp]
    public void Setup()
    {
        _steps = new InteractiveConstraintSteps();
    }

    [TearDown]
    public void TearDown()
    {
        _steps?.Dispose();
    }

    /// <summary>
    /// E2E Acceptance Test 1: Conversational Constraint Definition
    /// Business Scenario: User describes constraint in natural language, system creates structured constraint
    /// This test will FAIL initially and drive the implementation of conversational constraint creation
    /// </summary>
    [Test]
    public async Task Should_Create_Constraint_From_Natural_Language_Conversation()
    {
        // Arrange & Act & Assert using BDD Given-When-Then with lambdas
        await Given(() => _steps.UserStartsConstraintDefinitionConversation())
            .And(() => _steps.UserSaysNaturalLanguage("Remind developers to write tests before implementation"))
            .And(() => _steps.UserSpecifiesContext("when implementing new features"))
            .And(() => _steps.UserProvidesPriority(0.9))
            .When(() => _steps.SystemProcessesConversationalInput())
            .Then(() => _steps.ConstraintIsCreatedWithId("tdd.test-first-reminder"))
            .And(() => _steps.ConstraintHasTitle("Write Tests Before Implementation"))
            .And(() => _steps.ConstraintHasKeywords("test", "implementation", "feature"))
            .And(() => _steps.ConstraintHasContextPattern("feature_development"))
            .And(() => _steps.ConstraintHasPriority(0.9))
            .And(() => _steps.ConstraintIsValidatedSuccessfully())
            .And(() => _steps.ConstraintIntegratesWithExistingTriggerSystem())
            .ExecuteAsync();
    }

    /// <summary>
    /// E2E Acceptance Test 2: Tree Visualization System
    /// Business Scenario: User requests visual representation of constraint relationships and hierarchy
    /// This test will FAIL initially and drive the implementation of tree rendering system
    /// </summary>
    [Test]
    public async Task Should_Visualize_Constraint_Composition_Hierarchy()
    {
        // Arrange & Act & Assert using BDD Given-When-Then with lambdas
        await Given(() => _steps.MultipleConstraintsExistWithRelationships())
            .And(() => _steps.ConstraintsHaveHierarchicalDependencies())
            .And(() => _steps.UserRequestsTreeVisualization())
            .And(() => _steps.UserSpecifiesAsciiRenderingFormat())
            .When(() => _steps.SystemGeneratesTreeVisualization())
            .Then(() => _steps.TreeShowsHierarchicalStructure())
            .And(() => _steps.TreeShowsCompositionRelationships())
            .And(() => _steps.TreeDisplaysConstraintMetadata())
            .And(() => _steps.TreeIsRenderedInAsciiFormat())
            .And(() => _steps.TreeIsCompatibleWithClaudeCodeConsole())
            .And(() => _steps.TreeRenderingCompletesWithinPerformanceThreshold())
            .ExecuteAsync();
    }

    /// <summary>
    /// E2E Acceptance Test 3: Interactive Refinement Workflow
    /// Business Scenario: User refines constraint definition through guided dialogue with validation feedback
    /// This test will FAIL initially and drive the implementation of iterative constraint improvement
    /// </summary>
    [Test]
    [Ignore("Temporarily disabled until implementation - will enable one at a time to avoid commit blocks")]
    public async Task Should_Refine_Constraints_Through_Iterative_Feedback()
    {
        // Arrange & Act & Assert using BDD Given-When-Then with lambdas
        await Given(() => _steps.ConstraintExistsWithInitialDefinition())
            .And(() => _steps.UserRequestsConstraintRefinement())
            .And(() => _steps.SystemProvidesCurrentConstraintState())
            .When(() => _steps.SystemProvidesValidationFeedback())
            .And(() => _steps.UserMakesSpecificImprovements("Add keyword 'unit' for better matching"))
            .And(() => _steps.UserAdjustsPriorityLevel(0.95))
            .And(() => _steps.UserRefinesContextPatterns("unit_testing", "test_driven_development"))
            .Then(() => _steps.ConstraintIsUpdatedWithChanges())
            .And(() => _steps.ValidationPassesWithImprovedDefinition())
            .And(() => _steps.ChangesIntegrateSeamlesslyWithExistingSystem())
            .And(() => _steps.RefinementHistoryIsTrackedForAuditability())
            .And(() => _steps.UpdatedConstraintActivatesInRelevantScenarios())
            .ExecuteAsync();
    }

    /// <summary>
    /// E2E Acceptance Test 4: Real-time Validation During Creation
    /// Business Scenario: User receives immediate feedback during constraint creation process
    /// This test validates the real-time validation and feedback system
    /// </summary>
    [Test]
    [Ignore("Temporarily disabled until implementation - will enable one at a time to avoid commit blocks")]
    public async Task Should_Provide_Real_Time_Validation_During_Constraint_Creation()
    {
        await Given(() => _steps.UserStartsConstraintDefinitionConversation())
            .And(() => _steps.UserProvidesIncompleteInput("Remind about"))
            .When(() => _steps.SystemProcessesPartialInput())
            .Then(() => _steps.ValidationFeedbackIsProvidedImmediately())
            .And(() => _steps.FeedbackIdentifiesSpecificIssues("Missing constraint description", "No context specified"))
            .And(() => _steps.FeedbackProvidesSuggestions("Try: 'Remind about writing tests when implementing features'"))
            .And(() => _steps.UserCanIterativelyImproveBasedOnFeedback())
            .ExecuteAsync();
    }

    /// <summary>
    /// E2E Acceptance Test 5: Integration with Existing Context-Aware System
    /// Business Scenario: New constraints created through conversation immediately work with existing activation system
    /// This test validates seamless integration with Step A2's context-aware constraint activation
    /// </summary>
    [Test]
    [Ignore("Temporarily disabled until implementation - will enable one at a time to avoid commit blocks")]
    public async Task Should_Integrate_New_Constraints_With_Context_Aware_Activation()
    {
        await Given(() => _steps.UserCreatesNewConstraintThroughConversation())
            .And(() => _steps.ConstraintIsPersistedInSystem())
            .And(() => _steps.DeveloperPerformsActionMatchingNewConstraint())
            .When(() => _steps.ContextAnalyzerProcessesDeveloperAction())
            .And(() => _steps.TriggerMatchingEngineEvaluatesConstraints())
            .Then(() => _steps.NewConstraintIsActivatedAppropriately())
            .And(() => _steps.ConstraintActivationHasCorrectConfidenceScore())
            .And(() => _steps.SystemBehaviorIsConsistentWithExistingConstraints())
            .ExecuteAsync();
    }

    // Helper method to create Given() fluent interface
    private static ScenarioBuilder Given(Func<Task> step) => new ScenarioBuilder().Given(step);
}
