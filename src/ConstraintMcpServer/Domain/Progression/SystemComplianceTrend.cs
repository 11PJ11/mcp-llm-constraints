using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Progression;

/// <summary>
/// Domain entity representing system-wide compliance trends and analysis.
/// Immutable record aggregating compliance data across multiple agents for system optimization.
/// </summary>
/// <param name="TrendId">Unique identifier for this trend analysis</param>
/// <param name="AnalyzedAt">When this trend analysis was performed</param>
/// <param name="AgentCount">Number of agents included in the analysis</param>
/// <param name="OverallComplianceRate">System-wide average compliance rate</param>
/// <param name="SystemTrend">Overall direction of system compliance</param>
/// <param name="HighPerformingAgents">Agents with exceptional compliance</param>
/// <param name="UnderperformingAgents">Agents needing improvement</param>
/// <param name="TopViolatedConstraints">Most frequently violated constraints</param>
/// <param name="EffectiveConstraints">Constraints with high compliance rates</param>
/// <param name="SystemHealthScore">Overall system health rating (0.0-100.0)</param>
/// <param name="OptimizationOpportunities">Identified areas for system improvement</param>
/// <param name="ResourceAllocationRecommendations">Recommendations for resource allocation</param>
public sealed record SystemComplianceTrend(
    Guid TrendId,
    DateTime AnalyzedAt,
    int AgentCount,
    double OverallComplianceRate,
    ComplianceTrend SystemTrend,
    IReadOnlyList<AgentComplianceMetrics> HighPerformingAgents,
    IReadOnlyList<AgentComplianceMetrics> UnderperformingAgents,
    IReadOnlyList<ConstraintViolationMetrics> TopViolatedConstraints,
    IReadOnlyList<ConstraintEffectivenessMetrics> EffectiveConstraints,
    double SystemHealthScore,
    IReadOnlyList<string> OptimizationOpportunities,
    IReadOnlyList<string> ResourceAllocationRecommendations
)
{
    /// <summary>
    /// Validates that the system trend has required fields and valid ranges.
    /// </summary>
    public bool IsValid =>
        TrendId != Guid.Empty &&
        AnalyzedAt != default &&
        AgentCount >= 0 &&
        OverallComplianceRate is >= 0.0 and <= 1.0 &&
        HighPerformingAgents != null &&
        UnderperformingAgents != null &&
        TopViolatedConstraints != null &&
        EffectiveConstraints != null &&
        SystemHealthScore is >= 0.0 and <= 100.0 &&
        OptimizationOpportunities != null &&
        ResourceAllocationRecommendations != null;

    /// <summary>
    /// Determines if the system has healthy compliance (>= 0.8).
    /// </summary>
    public bool HasHealthyCompliance => OverallComplianceRate >= 0.8;

    /// <summary>
    /// Determines if the system health score indicates good health (>= 80.0).
    /// </summary>
    public bool HasGoodSystemHealth => SystemHealthScore >= 80.0;

    /// <summary>
    /// Calculates the percentage of underperforming agents.
    /// </summary>
    public double UnderperformanceRate =>
        AgentCount > 0 ? (double)UnderperformingAgents.Count / AgentCount : 0.0;

    /// <summary>
    /// Gets the most problematic constraint (highest violation rate).
    /// </summary>
    public ConstraintViolationMetrics? MostProblematicConstraint =>
        TopViolatedConstraints.MaxBy(c => c.ViolationRate);

    /// <summary>
    /// Calculates priority level for system intervention.
    /// </summary>
    public string InterventionPriority =>
        (OverallComplianceRate, SystemTrend, SystemHealthScore) switch
        {
            ( < 0.6, ComplianceTrend.Declining, _) => "Critical",
            ( < 0.7, ComplianceTrend.Declining, _) => "High",
            ( < 0.8, _, < 70.0) => "High",
            (_, ComplianceTrend.Declining, < 80.0) => "Medium",
            ( < 0.9, _, _) => "Medium",
            _ => "Low"
        };
}

/// <summary>
/// Metrics for individual agent compliance performance.
/// </summary>
/// <param name="AgentId">Agent identifier</param>
/// <param name="ComplianceRate">Agent's compliance rate</param>
/// <param name="TrendDirection">Agent's compliance trend</param>
/// <param name="InteractionCount">Number of interactions analyzed</param>
/// <param name="AverageRemediationTime">Average time to resolve violations</param>
public sealed record AgentComplianceMetrics(
    string AgentId,
    double ComplianceRate,
    ComplianceTrend TrendDirection,
    int InteractionCount,
    TimeSpan? AverageRemediationTime
)
{
    /// <summary>
    /// Calculates performance score combining compliance rate and trend.
    /// </summary>
    public double PerformanceScore =>
        TrendDirection switch
        {
            ComplianceTrend.Improving => ComplianceRate * 1.1,
            ComplianceTrend.Declining => ComplianceRate * 0.9,
            _ => ComplianceRate
        };
}

/// <summary>
/// Metrics for constraint violation patterns.
/// </summary>
/// <param name="ConstraintId">Constraint identifier</param>
/// <param name="ViolationCount">Number of violations</param>
/// <param name="ViolationRate">Violation rate across agents</param>
/// <param name="AverageSeverity">Average severity of violations</param>
/// <param name="AffectedAgentCount">Number of agents with violations</param>
public sealed record ConstraintViolationMetrics(
    string ConstraintId,
    int ViolationCount,
    double ViolationRate,
    double AverageSeverity,
    int AffectedAgentCount
)
{
    /// <summary>
    /// Calculates impact score based on violation metrics.
    /// </summary>
    public double ImpactScore =>
        ViolationRate * AverageSeverity * (AffectedAgentCount / 10.0);
}

/// <summary>
/// Metrics for constraint effectiveness analysis.
/// </summary>
/// <param name="ConstraintId">Constraint identifier</param>
/// <param name="ComplianceRate">Compliance rate for this constraint</param>
/// <param name="EffectivenessScore">Overall effectiveness rating</param>
/// <param name="AgentSuccessCount">Number of agents successfully following constraint</param>
/// <param name="ImprovementTrend">Trend in constraint effectiveness</param>
public sealed record ConstraintEffectivenessMetrics(
    string ConstraintId,
    double ComplianceRate,
    double EffectivenessScore,
    int AgentSuccessCount,
    ComplianceTrend ImprovementTrend
)
{
    /// <summary>
    /// Determines if this constraint is highly effective (>= 0.9 compliance).
    /// </summary>
    public bool IsHighlyEffective => ComplianceRate >= 0.9;
}
