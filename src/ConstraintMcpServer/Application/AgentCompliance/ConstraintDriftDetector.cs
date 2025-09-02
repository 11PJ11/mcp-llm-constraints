using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain.Progression;

namespace ConstraintMcpServer.Application.AgentCompliance;

/// <summary>
/// Application service implementing constraint compliance drift detection and proactive intervention.
/// Analyzes agent behavior patterns to identify deteriorating compliance before breakdown.
/// Performance requirement: Sub-25ms drift detection latency.
/// </summary>
public sealed class ConstraintDriftDetector : IConstraintDriftDetector
{
    private readonly Dictionary<string, List<ComplianceAnalysisResult>> _driftHistory = new();

    /// <summary>
    /// Analyzes agent compliance patterns to detect drift.
    /// Performance requirement: Sub-25ms p95 latency.
    /// </summary>
    public async Task<ComplianceAnalysisResult> DetectComplianceDriftAsync(AgentConstraintAdherence complianceHistory)
    {
        var startTime = DateTime.UtcNow;

        if (!complianceHistory.IsValid)
        {
            throw new ArgumentException("Invalid compliance history", nameof(complianceHistory));
        }

        // Perform drift analysis
        var driftDetected = AnalyzeDriftPatterns(complianceHistory);
        var driftSeverity = CalculateDriftSeverity(complianceHistory);
        var confidence = CalculateDriftConfidence(complianceHistory);

        // Generate findings
        var findings = GenerateDriftFindings(complianceHistory, driftDetected);
        var riskFactors = IdentifyDriftRiskFactors(complianceHistory);
        var recommendations = GenerateDriftRecommendations(complianceHistory, driftDetected);

        var analysisResult = new ComplianceAnalysisResult(
            AnalysisId: Guid.NewGuid(),
            AgentId: complianceHistory.AgentId,
            AnalysisType: "DriftDetection",
            AnalyzedAt: DateTime.UtcNow,
            ComplianceScore: complianceHistory.ComplianceRate,
            TrendDirection: complianceHistory.ComplianceTrend,
            AnalysisConfidence: confidence,
            KeyFindings: findings,
            RiskFactors: riskFactors,
            Recommendations: recommendations,
            MetricsAnalyzed: complianceHistory.TotalInteractions
        );

        // Store analysis for trend tracking
        if (!_driftHistory.ContainsKey(complianceHistory.AgentId))
        {
            _driftHistory[complianceHistory.AgentId] = new List<ComplianceAnalysisResult>();
        }
        _driftHistory[complianceHistory.AgentId].Add(analysisResult);

        // Ensure sub-25ms performance
        var processingTime = DateTime.UtcNow - startTime;
        if (processingTime.TotalMilliseconds > 25)
        {
            Console.WriteLine($"Warning: Drift detection took {processingTime.TotalMilliseconds}ms");
        }

        return await Task.FromResult(analysisResult);
    }

    /// <summary>
    /// Assesses the severity of detected compliance drift.
    /// Determines urgency of intervention required.
    /// </summary>
    public async Task<ViolationSeverity> AssessDriftSeverityAsync(ComplianceAnalysisResult driftAnalysis)
    {
        if (!driftAnalysis.IsValid)
        {
            throw new ArgumentException("Invalid drift analysis", nameof(driftAnalysis));
        }

        var severity = DetermineDriftSeverity(driftAnalysis);

        return await Task.FromResult(severity);
    }

    /// <summary>
    /// Triggers proactive intervention when drift is detected.
    /// Prevents critical compliance breakdown through early intervention.
    /// </summary>
    public async Task<ConstraintRefinementSuggestion> TriggerProactiveInterventionAsync(ViolationSeverity severity, string agentId)
    {
        if (string.IsNullOrWhiteSpace(agentId))
        {
            throw new ArgumentException("Agent ID cannot be null or empty", nameof(agentId));
        }

        var intervention = GenerateInterventionSuggestion(severity, agentId);

        return await Task.FromResult(intervention);
    }

    private static bool AnalyzeDriftPatterns(AgentConstraintAdherence complianceHistory)
    {
        // Check if compliance is deteriorating
        if (complianceHistory.ComplianceTrend == ComplianceTrend.Declining)
        {
            return true;
        }

        // Check for recent critical violations
        if (complianceHistory.CriticalViolationCount > 0)
        {
            return true;
        }

        // Check for declining compliance rate
        if (complianceHistory.ComplianceRate < 0.7)
        {
            return true;
        }

        // Analyze recent assessment trends
        if (complianceHistory.ComplianceHistory.Count >= 5)
        {
            var recentAssessments = complianceHistory.ComplianceHistory.TakeLast(5);
            var avgRecent = recentAssessments.Average(a => (int)a.ComplianceLevel);
            var overallAvg = complianceHistory.ComplianceHistory.Average(a => (int)a.ComplianceLevel);

            // Drift detected if recent performance is significantly worse
            return avgRecent < overallAvg - 0.5;
        }

        return false;
    }

    private static double CalculateDriftSeverity(AgentConstraintAdherence complianceHistory)
    {
        var baseScore = 1.0 - complianceHistory.ComplianceRate;

        // Increase severity for declining trend
        if (complianceHistory.ComplianceTrend == ComplianceTrend.Declining)
        {
            baseScore += 0.3;
        }

        // Increase severity for critical violations
        baseScore += complianceHistory.CriticalViolationCount * 0.2;

        // Increase severity for high violation rate
        if (complianceHistory.ViolationRate > 0.4)
        {
            baseScore += 0.2;
        }

        return Math.Min(baseScore, 1.0);
    }

    private static double CalculateDriftConfidence(AgentConstraintAdherence complianceHistory)
    {
        var baseConfidence = 0.6;

        // Higher confidence with more data
        if (complianceHistory.HasSufficientHistory)
        {
            baseConfidence += 0.2;
        }

        // Higher confidence with clear trend
        if (complianceHistory.ComplianceTrend != ComplianceTrend.Unknown)
        {
            baseConfidence += 0.1;
        }

        // Higher confidence with recent violations
        if (complianceHistory.CriticalViolationCount > 0)
        {
            baseConfidence += 0.1;
        }

        return Math.Min(baseConfidence, 1.0);
    }

    private static IReadOnlyList<string> GenerateDriftFindings(AgentConstraintAdherence complianceHistory, bool driftDetected)
    {
        var findings = new List<string>();

        if (driftDetected)
        {
            findings.Add("Compliance drift detected");

            if (complianceHistory.ComplianceTrend == ComplianceTrend.Declining)
            {
                findings.Add("Declining compliance trend observed");
            }

            if (complianceHistory.CriticalViolationCount > 0)
            {
                findings.Add($"{complianceHistory.CriticalViolationCount} critical violations in recent history");
            }

            findings.Add($"Current compliance rate: {complianceHistory.ComplianceRate:P1}");
        }
        else
        {
            findings.Add("No significant compliance drift detected");
            findings.Add($"Compliance rate within acceptable range: {complianceHistory.ComplianceRate:P1}");
        }

        return findings.AsReadOnly();
    }

    private static IReadOnlyList<string> IdentifyDriftRiskFactors(AgentConstraintAdherence complianceHistory)
    {
        var riskFactors = new List<string>();

        if (complianceHistory.ComplianceTrend == ComplianceTrend.Declining)
        {
            riskFactors.Add("Sustained decline in compliance performance");
        }

        if (complianceHistory.ViolationRate > 0.3)
        {
            riskFactors.Add("High violation rate indicates systemic issues");
        }

        if (!complianceHistory.HasSufficientHistory)
        {
            riskFactors.Add("Limited historical data may affect drift detection accuracy");
        }

        if (complianceHistory.CriticalViolationCount > 0)
        {
            riskFactors.Add("Recent critical violations suggest urgent attention needed");
        }

        return riskFactors.AsReadOnly();
    }

    private static IReadOnlyList<string> GenerateDriftRecommendations(AgentConstraintAdherence complianceHistory, bool driftDetected)
    {
        var recommendations = new List<string>();

        if (driftDetected)
        {
            recommendations.Add("Implement proactive intervention strategy");

            if (complianceHistory.ComplianceTrend == ComplianceTrend.Declining)
            {
                recommendations.Add("Investigate root causes of performance decline");
            }

            if (complianceHistory.CriticalViolationCount > 0)
            {
                recommendations.Add("Address critical violations with immediate corrective action");
            }

            recommendations.Add("Increase monitoring frequency for early detection");
            recommendations.Add("Consider constraint refinement based on violation patterns");
        }
        else
        {
            recommendations.Add("Continue current monitoring approach");
            recommendations.Add("Maintain regular compliance assessment schedule");
        }

        return recommendations.AsReadOnly();
    }

    private static ViolationSeverity DetermineDriftSeverity(ComplianceAnalysisResult driftAnalysis)
    {
        var score = 1.0 - driftAnalysis.ComplianceScore;

        // Adjust for trend
        if (driftAnalysis.TrendDirection == ComplianceTrend.Declining)
        {
            score += 0.2;
        }

        return score switch
        {
            >= 0.8 => ViolationSeverity.Critical,
            >= 0.6 => ViolationSeverity.Major,
            >= 0.4 => ViolationSeverity.Moderate,
            _ => ViolationSeverity.Minor
        };
    }

    private static ConstraintRefinementSuggestion GenerateInterventionSuggestion(ViolationSeverity severity, string agentId)
    {
        var (refinementType, suggestion, rationale, confidence, impact) = severity switch
        {
            ViolationSeverity.Critical => (
                "Emergency Intervention",
                "Immediate constraint reinforcement with mandatory compliance validation",
                "Critical drift detected requiring urgent intervention to prevent complete breakdown",
                0.95,
                0.8
            ),
            ViolationSeverity.Major => (
                "Proactive Intervention",
                "Enhanced constraint guidance with increased monitoring frequency",
                "Major drift trend requires proactive intervention to prevent escalation",
                0.85,
                0.6
            ),
            ViolationSeverity.Moderate => (
                "Preventive Guidance",
                "Targeted constraint reminders and additional context",
                "Moderate drift indicates need for preventive guidance",
                0.75,
                0.4
            ),
            _ => (
                "Monitoring Enhancement",
                "Increased observation frequency with trend analysis",
                "Minor drift detected, enhanced monitoring recommended",
                0.65,
                0.2
            )
        };

        return new ConstraintRefinementSuggestion(
            SuggestionId: Guid.NewGuid(),
            ConstraintId: "drift-intervention",
            RefinementType: refinementType,
            Suggestion: suggestion,
            Rationale: rationale,
            ConfidenceLevel: confidence,
            ExpectedImpact: impact,
            GeneratedAt: DateTime.UtcNow,
            BasedOnInteractions: 1
        );
    }
}
