using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Progression;

/// <summary>
/// Domain entity representing the results of compliance analysis operations.
/// Immutable record containing analysis findings, insights, and recommendations.
/// </summary>
/// <param name="AnalysisId">Unique identifier for this analysis</param>
/// <param name="AgentId">Identifier of the analyzed agent</param>
/// <param name="AnalysisType">Type of analysis performed</param>
/// <param name="AnalyzedAt">When the analysis was performed</param>
/// <param name="ComplianceScore">Overall compliance score (0.0-1.0)</param>
/// <param name="TrendDirection">Direction of compliance trend</param>
/// <param name="AnalysisConfidence">Confidence in analysis results (0.0-1.0)</param>
/// <param name="KeyFindings">Primary findings from the analysis</param>
/// <param name="RiskFactors">Identified risk factors for compliance</param>
/// <param name="Recommendations">Actionable recommendations based on analysis</param>
/// <param name="MetricsAnalyzed">Number of data points analyzed</param>
public sealed record ComplianceAnalysisResult(
    Guid AnalysisId,
    string AgentId,
    string AnalysisType,
    DateTime AnalyzedAt,
    double ComplianceScore,
    ComplianceTrend TrendDirection,
    double AnalysisConfidence,
    IReadOnlyList<string> KeyFindings,
    IReadOnlyList<string> RiskFactors,
    IReadOnlyList<string> Recommendations,
    int MetricsAnalyzed
)
{
    /// <summary>
    /// Validates that the analysis result has required fields and valid ranges.
    /// </summary>
    public bool IsValid =>
        AnalysisId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(AgentId) &&
        !string.IsNullOrWhiteSpace(AnalysisType) &&
        AnalyzedAt != default &&
        ComplianceScore is >= 0.0 and <= 1.0 &&
        AnalysisConfidence is >= 0.0 and <= 1.0 &&
        KeyFindings != null &&
        RiskFactors != null &&
        Recommendations != null &&
        MetricsAnalyzed >= 0;

    /// <summary>
    /// Determines if the compliance score indicates good performance (>= 0.8).
    /// </summary>
    public bool IndicatesGoodCompliance => ComplianceScore >= 0.8;

    /// <summary>
    /// Determines if the analysis has high confidence (>= 0.8).
    /// </summary>
    public bool HasHighConfidence => AnalysisConfidence >= 0.8;

    /// <summary>
    /// Determines if the analysis indicates declining compliance.
    /// </summary>
    public bool IndicatesDecliningCompliance =>
        TrendDirection == ComplianceTrend.Declining || ComplianceScore < 0.6;

    /// <summary>
    /// Calculates priority level based on compliance score and trend.
    /// </summary>
    public string PriorityLevel =>
        (ComplianceScore, TrendDirection) switch
        {
            ( < 0.5, ComplianceTrend.Declining) => "Critical",
            ( < 0.7, ComplianceTrend.Declining) => "High",
            ( < 0.8, _) => "Medium",
            (_, ComplianceTrend.Declining) => "Medium",
            _ => "Low"
        };
}
