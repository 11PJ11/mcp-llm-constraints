using NUnit.Framework;
using ConstraintMcpServer.Tests.Steps;
using static ConstraintMcpServer.Tests.Framework.ScenarioBuilder;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// E2E tests for Step B2: Agent Constraint Adherence Intelligence.
/// These tests drive implementation through Outside-In TDD methodology.
/// Business scenarios validate agent compliance monitoring and constraint optimization.
/// </summary>
[TestFixture]
[Category("E2E")]
[Category("AgentConstraintAdherence")]
public class AgentConstraintAdherenceE2E
{
    private AgentConstraintAdherenceSteps? _steps;

    [SetUp]
    public void Setup()
    {
        _steps = new AgentConstraintAdherenceSteps();
    }

    [TearDown]
    public void TearDown()
    {
        _steps?.Dispose();
    }

    /// <summary>
    /// E2E Test 1: Agent compliance tracking with real-time violation detection.
    /// Business Scenario: System tracks agent constraint compliance in real-time,
    /// detecting violations and updating compliance metrics with sub-50ms latency.
    /// 
    /// This test will FAIL initially and drive IAgentComplianceTracker implementation.
    /// </summary>
    [Test]
    public async Task Should_Track_Agent_Compliance_With_Realtime_Violation_Detection()
    {
        await Given(() => _steps!.AgentComplianceTrackingIsEnabled())
            .And(() => _steps!.AgentHasHistoricalComplianceData())
            .And(() => _steps!.ConstraintInteractionOccurs())
            .When(() => _steps!.AgentViolatesConstraint())
            .Then(() => _steps!.ViolationIsDetectedWithinLatencyBudget())
            .And(() => _steps!.ComplianceMetricsAreUpdatedInRealtime())
            .And(() => _steps!.ViolationIsRecordedWithContext())
            .ExecuteAsync();
    }

    /// <summary>
    /// E2E Test 2: Constraint drift detection and proactive intervention.
    /// Business Scenario: System detects when agent compliance is deteriorating
    /// and triggers proactive interventions before critical compliance breakdown.
    /// 
    /// This test drives IConstraintDriftDetector implementation.
    /// </summary>
    [Test]
    public async Task Should_Detect_Constraint_Drift_And_Trigger_Intervention()
    {
        await Given(() => _steps!.AgentHasDecliningCompliancePattern())
            .And(() => _steps!.ConstraintDriftDetectionIsActive())
            .And(() => _steps!.DriftSeverityThresholdIsConfigured())
            .When(() => _steps!.DriftDetectionAnalysisIsPerformed())
            .Then(() => _steps!.ComplianceDriftIsIdentified())
            .And(() => _steps!.DriftSeverityIsAssessed())
            .And(() => _steps!.ProactiveInterventionIsTriggered())
            .ExecuteAsync();
    }

    /// <summary>
    /// E2E Test 3: Adaptive constraint optimization based on agent behavior patterns.
    /// Business Scenario: System analyzes agent compliance patterns and generates
    /// data-driven constraint refinement suggestions to improve effectiveness.
    /// 
    /// This drives IConstraintAdaptationEngine implementation.
    /// </summary>
    [Test]
    public async Task Should_Generate_Constraint_Refinements_Based_On_Agent_Patterns()
    {
        await Given(() => _steps!.MultipleAgentsHaveComplianceHistory())
            .And(() => _steps!.ConstraintEffectivenessAnalysisIsEnabled())
            .And(() => _steps!.RefinementSuggestionGenerationIsActive())
            .When(() => _steps!.ComplianceAnalysisIsPerformed())
            .Then(() => _steps!.ConstraintEffectivenessIsAssessed())
            .And(() => _steps!.RefinementSuggestionsAreGenerated())
            .And(() => _steps!.SuggestionsAreRankedByConfidenceLevel())
            .ExecuteAsync();
    }

    /// <summary>
    /// E2E Test 4: System-wide compliance trend analysis and optimization recommendations.
    /// Business Scenario: System provides comprehensive analysis of constraint compliance
    /// across all agents with strategic optimization recommendations.
    /// 
    /// This ensures integration of all compliance intelligence components.
    /// </summary>
    [Test]
    public async Task Should_Provide_Systemwide_Compliance_Analysis_And_Optimization()
    {
        await Given(() => _steps!.SystemHasMultipleAgentsWithVaryingCompliance())
            .And(() => _steps!.SystemComplianceAnalysisIsEnabled())
            .And(() => _steps!.OptimizationRecommendationEngineIsActive())
            .When(() => _steps!.SystemWideAnalysisIsRequested())
            .Then(() => _steps!.SystemComplianceTrendsAreAnalyzed())
            .And(() => _steps!.OptimizationOpportunitiesAreIdentified())
            .And(() => _steps!.ResourceAllocationRecommendationsAreProvided())
            .And(() => _steps!.SystemHealthScoreIsCalculated())
            .ExecuteAsync();
    }

    /// <summary>
    /// E2E Test 5: Performance validation for agent constraint adherence intelligence.
    /// Business Scenario: Agent compliance intelligence operates within strict performance
    /// budgets to enable real-time constraint enforcement.
    /// </summary>
    [Test]
    public async Task Should_Maintain_Performance_Budget_With_Compliance_Intelligence()
    {
        await Given(() => _steps!.AgentConstraintAdherenceSystemIsActive())
            .And(() => _steps!.ComplexAgentComplianceHistoryExists())
            .And(() => _steps!.PerformanceBudgetsAreConfigured())
            .When(() => _steps!.ComplianceAnalysisIsPerformed())
            .Then(() => _steps!.ComplianceTrackingOperatesUnder50Milliseconds())
            .And(() => _steps!.DriftDetectionCompletesUnder25Milliseconds())
            .And(() => _steps!.ConstraintAdaptationCompletesUnder200Milliseconds())
            .ExecuteAsync();
    }
}
