using System;
using System.Threading.Tasks;
using ConstraintMcpServer.Tests.Infrastructure;
using NUnit.Framework;
using ConstraintMcpServer.Tests.Steps;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// E2E tests for Step C2: Basic Feedback Collection system.
/// Tests user feedback workflow with local storage and effectiveness tracking.
/// Follows Outside-In TDD methodology with business scenario focus.
/// </summary>
[TestFixture]
public class BasicFeedbackCollectionE2E
{
    private BasicFeedbackCollectionSteps? _steps;

    [SetUp]
    public void Setup()
    {
        _steps = new BasicFeedbackCollectionSteps();
    }

    [TearDown]
    public void TearDown()
    {
        _steps?.Dispose();
    }

    [Test]
    public async Task Should_Allow_User_To_Rate_Constraint_Effectiveness_With_Simple_Thumbs_System()
    {
        // Business Scenario: User provides simple thumbs up/down feedback on constraint helpfulness
        // Expected: System records feedback locally and updates effectiveness scores

        await Given(_steps!.UserHasConfiguredConstraints)
            .And(_steps.ConstraintHasBeenActivatedDuringSession)
            .When(_steps.UserProvidesThumbsUpFeedbackOnConstraint)
            .Then(_steps.FeedbackIsRecordedWithTimestamp)
            .And(_steps.EffectivenessScoreIsUpdatedBasedOnFeedback)
            .And(_steps.FeedbackIsStoredLocallyWithPrivacyProtection)
            .And(_steps.FeedbackRecordingCompletesWithinPerformanceBudget)
            .ExecuteAsync();
    }

    [Test]
    [Category("MacOSPerformance")]
    public async Task Should_Track_Basic_Effectiveness_Metrics_Without_Complex_Algorithms()
    {
        // Business Scenario: System provides simple effectiveness metrics for constraint performance
        // Expected: Basic scoring system without complex machine learning or algorithms

        await Given(_steps!.MultipleConstraintsHaveReceivedUserFeedback)
            .And(_steps.ConstraintActivationHistoryExists)
            .When(_steps.UserRequestsEffectivenessReport)
            .Then(_steps.SimpleEffectivenessScoresAreCalculated)
            .And(_steps.TopPerformingConstraintsAreIdentified)
            .And(_steps.UsageStatisticsAreProvidedWithoutComplexity)
            .And(_steps.EffectivenessCalculationCompletesWithinPerformanceBudget)
            .ExecuteAsync();
    }

    [Test]
    [Category("MacOSPerformance")]
    public async Task Should_Provide_Usage_Analytics_With_Constraint_Activation_Frequency()
    {
        // Business Scenario: User wants to understand which constraints are most/least used
        // Expected: Simple usage analytics showing activation frequency and patterns

        await Given(_steps!.ConstraintActivationHistorySpansMultipleSessions)
            .And(_steps.VariousConstraintsHaveBeenActivatedWithDifferentFrequencies)
            .When(_steps.UserRequestsUsageAnalyticsReport)
            .Then(_steps.ConstraintActivationFrequenciesAreDisplayed)
            .And(_steps.SessionBasedUsagePatternsAreShown)
            .And(_steps.MostAndLeastUsedConstraintsAreHighlighted)
            .And(_steps.UsageAnalyticsGenerationCompletesWithinPerformanceBudget)
            .ExecuteAsync();
    }

    [Test]
    [Category("MacOSPerformance")]
    public async Task Should_Store_Feedback_Data_Locally_With_User_Privacy_Protection()
    {
        // Business Scenario: User feedback is collected with strong privacy protection
        // Expected: All data stored locally, no external transmission, user control over data

        await Given(_steps!.UserProvidesMultipleFeedbackRatingsAcrossSessions)
            .When(_steps.SystemStoresFeedbackDataLocally)
            .Then(_steps.FeedbackDataIsStoredInLocalSQLiteDatabase)
            .And(_steps.NoDataIsTransmittedExternally)
            .And(_steps.UserHasControlOverDataRetentionAndCleanup)
            .And(_steps.FeedbackStorageOperationsCompleteWithinPerformanceBudget)
            .ExecuteAsync();
    }

    [Test]
    [Category("MacOSPerformance")]
    public async Task Should_Integrate_Feedback_Display_With_Enhanced_Visualization_System()
    {
        // Business Scenario: Feedback indicators are integrated with existing tree visualization
        // Expected: Visual feedback indicators overlay existing constraint tree display

        await Given(_steps!.UserHasConfiguredConstraints)
            .And(_steps.MultipleConstraintsHaveReceivedUserFeedback)
            .And(_steps.EnhancedVisualizationSystemIsActive)
            .And(_steps.ConstraintsHaveAccumulatedFeedbackRatings)
            .When(_steps.UserViewsConstraintTreeVisualization)
            .Then(_steps.FeedbackIndicatorsAreDisplayedInTreeView)
            .And(_steps.EffectivenessScoresAreVisuallyRepresented)
            .And(_steps.VisualizationWithFeedbackRendersWithinPerformanceBudget)
            .And(_steps.FeedbackDisplayIntegratesSeamlesslyWithExistingVisualization)
            .ExecuteAsync();
    }

    private static ScenarioBuilder Given(Func<Task> step) => ScenarioBuilder.Given(step);
}
