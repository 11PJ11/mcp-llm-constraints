using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Progression;

/// <summary>
/// Domain entity representing an agent's constraint adherence profile and compliance history.
/// Immutable record serving as aggregate root for agent constraint compliance data.
/// </summary>
/// <param name="AgentId">Unique identifier for the agent</param>
/// <param name="ComplianceLevel">Current overall compliance level</param>
/// <param name="ComplianceTrend">Direction of compliance change over time</param>
/// <param name="TotalInteractions">Total number of constraint interactions</param>
/// <param name="SuccessfulCompliances">Number of successful constraint adherences</param>
/// <param name="ViolationCount">Total number of constraint violations</param>
/// <param name="LastInteractionAt">Timestamp of most recent constraint interaction</param>
/// <param name="ComplianceHistory">Historical compliance assessments</param>
/// <param name="RecentViolations">Recent constraint violations for trend analysis</param>
public sealed record AgentConstraintAdherence(
    string AgentId,
    ConstraintComplianceLevel ComplianceLevel,
    ComplianceTrend ComplianceTrend,
    int TotalInteractions,
    int SuccessfulCompliances,
    int ViolationCount,
    DateTime LastInteractionAt,
    IReadOnlyList<ConstraintComplianceAssessment> ComplianceHistory,
    IReadOnlyList<ConstraintViolation> RecentViolations
)
{
    /// <summary>
    /// Calculates current compliance rate as a percentage.
    /// </summary>
    public double ComplianceRate =>
        TotalInteractions > 0 ? (double)SuccessfulCompliances / TotalInteractions : 0.0;

    /// <summary>
    /// Calculates violation rate as a percentage.
    /// </summary>
    public double ViolationRate =>
        TotalInteractions > 0 ? (double)ViolationCount / TotalInteractions : 0.0;

    /// <summary>
    /// Determines if the agent has sufficient interaction history for analysis.
    /// </summary>
    public bool HasSufficientHistory => TotalInteractions >= 10;

    /// <summary>
    /// Gets the most recent compliance assessment.
    /// </summary>
    public ConstraintComplianceAssessment? MostRecentAssessment =>
        ComplianceHistory.MaxBy(h => h.AssessedAt);

    /// <summary>
    /// Counts critical violations in recent history.
    /// </summary>
    public int CriticalViolationCount =>
        RecentViolations.Count(v => v.Severity == ViolationSeverity.Critical);

    /// <summary>
    /// Determines if compliance is deteriorating based on recent trend.
    /// </summary>
    public bool IsComplianceDeteriorating =>
        ComplianceTrend == ComplianceTrend.Declining ||
        CriticalViolationCount > 0;

    /// <summary>
    /// Validates that the agent adherence data is consistent and valid.
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(AgentId) &&
        TotalInteractions >= 0 &&
        SuccessfulCompliances >= 0 &&
        ViolationCount >= 0 &&
        (SuccessfulCompliances + ViolationCount) <= TotalInteractions &&
        LastInteractionAt != default &&
        ComplianceHistory != null &&
        RecentViolations != null;
}
