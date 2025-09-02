using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain.Progression;

namespace ConstraintMcpServer.Application.AgentCompliance;

/// <summary>
/// Application service implementing real-time agent constraint compliance tracking.
/// Provides sub-50ms violation detection and compliance metrics updates.
/// Core component of the Agent Constraint Adherence Intelligence system.
/// </summary>
public sealed class AgentComplianceTracker : IAgentComplianceTracker
{
    private readonly Dictionary<string, AgentConstraintAdherence> _agentProfiles = new();
    private readonly List<ConstraintViolation> _recentViolations = new();

    /// <summary>
    /// Tracks agent compliance for a constraint interaction.
    /// Performance requirement: Sub-50ms p95 latency.
    /// </summary>
    public async Task<ConstraintComplianceAssessment> TrackComplianceAsync(ConstraintInteraction interaction)
    {
        var startTime = DateTime.UtcNow;

        // Validate interaction
        if (!interaction.IsValid)
        {
            throw new ArgumentException("Invalid constraint interaction", nameof(interaction));
        }

        // Get or create agent profile
        if (!_agentProfiles.TryGetValue(interaction.AgentId, out var agentProfile))
        {
            agentProfile = CreateInitialAgentProfile(interaction.AgentId);
            _agentProfiles[interaction.AgentId] = agentProfile;
        }

        // Analyze compliance
        var complianceLevel = AnalyzeCompliance(interaction, agentProfile);
        var confidence = CalculateAssessmentConfidence(interaction, complianceLevel);

        // Create assessment
        var assessment = new ConstraintComplianceAssessment(
            AssessmentId: Guid.NewGuid(),
            InteractionId: interaction.InteractionId,
            AgentId: interaction.AgentId,
            ConstraintId: interaction.ConstraintId,
            ComplianceLevel: complianceLevel,
            AssessmentConfidence: confidence,
            AssessedAt: DateTime.UtcNow,
            AssessmentNotes: GenerateAssessmentNotes(interaction, complianceLevel),
            RecommendedAction: DetermineRecommendedAction(complianceLevel)
        );

        // Update agent profile
        UpdateAgentProfile(interaction.AgentId, assessment);

        // Ensure sub-50ms performance
        var processingTime = DateTime.UtcNow - startTime;
        if (processingTime.TotalMilliseconds > 50)
        {
            // Log performance warning but don't fail
            Console.WriteLine($"Warning: Compliance tracking took {processingTime.TotalMilliseconds}ms");
        }

        return await Task.FromResult(assessment);
    }

    /// <summary>
    /// Records a constraint violation with full context.
    /// Updates compliance metrics in real-time.
    /// </summary>
    public async Task<ComplianceAnalysisResult> RecordViolationAsync(ConstraintViolation violation)
    {
        if (!violation.IsValid)
        {
            throw new ArgumentException("Invalid constraint violation", nameof(violation));
        }

        // Store violation
        _recentViolations.Add(violation);

        // Maintain recent violations list (keep last 100)
        if (_recentViolations.Count > 100)
        {
            _recentViolations.RemoveAt(0);
        }

        // Update agent compliance metrics
        if (_agentProfiles.TryGetValue(violation.AgentId, out var agentProfile))
        {
            var updatedProfile = UpdateProfileWithViolation(agentProfile, violation);
            _agentProfiles[violation.AgentId] = updatedProfile;

            // Create analysis result
            var analysisResult = new ComplianceAnalysisResult(
                AnalysisId: Guid.NewGuid(),
                AgentId: violation.AgentId,
                AnalysisType: "ViolationRecording",
                AnalyzedAt: DateTime.UtcNow,
                ComplianceScore: updatedProfile.ComplianceRate,
                TrendDirection: updatedProfile.ComplianceTrend,
                AnalysisConfidence: 0.9,
                KeyFindings: new[] { $"Violation recorded: {violation.Severity}" },
                RiskFactors: DetermineRiskFactors(violation, updatedProfile),
                Recommendations: GenerateViolationRecommendations(violation, updatedProfile),
                MetricsAnalyzed: updatedProfile.TotalInteractions
            );

            return await Task.FromResult(analysisResult);
        }

        throw new InvalidOperationException($"Agent profile not found: {violation.AgentId}");
    }

    /// <summary>
    /// Retrieves historical compliance data for an agent.
    /// Used for trend analysis and drift detection.
    /// </summary>
    public async Task<AgentConstraintAdherence> GetComplianceHistoryAsync(string agentId)
    {
        if (string.IsNullOrWhiteSpace(agentId))
        {
            throw new ArgumentException("Agent ID cannot be null or empty", nameof(agentId));
        }

        if (_agentProfiles.TryGetValue(agentId, out var profile))
        {
            return await Task.FromResult(profile);
        }

        // Return empty profile for unknown agents
        return await Task.FromResult(CreateInitialAgentProfile(agentId));
    }

    private static AgentConstraintAdherence CreateInitialAgentProfile(string agentId)
    {
        return new AgentConstraintAdherence(
            AgentId: agentId,
            ComplianceLevel: ConstraintComplianceLevel.AdequatelyCompliant,
            ComplianceTrend: ComplianceTrend.Unknown,
            TotalInteractions: 0,
            SuccessfulCompliances: 0,
            ViolationCount: 0,
            LastInteractionAt: DateTime.UtcNow,
            ComplianceHistory: Array.Empty<ConstraintComplianceAssessment>(),
            RecentViolations: Array.Empty<ConstraintViolation>()
        );
    }

    private static ConstraintComplianceLevel AnalyzeCompliance(ConstraintInteraction interaction, AgentConstraintAdherence profile)
    {
        // Simple heuristic based on agent response and history
        if (string.IsNullOrWhiteSpace(interaction.AgentResponse))
        {
            return ConstraintComplianceLevel.NonCompliant;
        }

        // Check for compliance keywords in response
        var response = interaction.AgentResponse.ToLowerInvariant();
        var complianceKeywords = new[] { "test", "tdd", "solid", "clean", "refactor", "validate" };
        var keywordMatches = complianceKeywords.Count(keyword => response.Contains(keyword));

        return keywordMatches switch
        {
            >= 3 => ConstraintComplianceLevel.ExceptionallyCompliant,
            2 => ConstraintComplianceLevel.HighlyCompliant,
            1 => ConstraintComplianceLevel.AdequatelyCompliant,
            _ => profile.ComplianceRate > 0.8 ? ConstraintComplianceLevel.AdequatelyCompliant : ConstraintComplianceLevel.PartiallyCompliant
        };
    }

    private static double CalculateAssessmentConfidence(ConstraintInteraction interaction, ConstraintComplianceLevel complianceLevel)
    {
        var baseConfidence = 0.7;

        // Higher confidence if we have agent response
        if (!string.IsNullOrWhiteSpace(interaction.AgentResponse))
        {
            baseConfidence += 0.2;
        }

        // Adjust based on compliance level certainty
        if (complianceLevel is ConstraintComplianceLevel.ExceptionallyCompliant or ConstraintComplianceLevel.NonCompliant)
        {
            baseConfidence += 0.1;
        }

        return Math.Min(baseConfidence, 1.0);
    }

    private static string GenerateAssessmentNotes(ConstraintInteraction interaction, ConstraintComplianceLevel complianceLevel)
    {
        return $"Agent compliance assessed as {complianceLevel} based on interaction context: {interaction.Context}";
    }

    private static string DetermineRecommendedAction(ConstraintComplianceLevel complianceLevel)
    {
        return complianceLevel switch
        {
            ConstraintComplianceLevel.NonCompliant => "Immediate intervention required",
            ConstraintComplianceLevel.PartiallyCompliant => "Provide additional guidance",
            ConstraintComplianceLevel.AdequatelyCompliant => "Continue monitoring",
            ConstraintComplianceLevel.HighlyCompliant => "Reinforce positive behavior",
            ConstraintComplianceLevel.ExceptionallyCompliant => "Use as positive example",
            _ => "Continue monitoring"
        };
    }

    private void UpdateAgentProfile(string agentId, ConstraintComplianceAssessment assessment)
    {
        if (!_agentProfiles.TryGetValue(agentId, out var profile))
        {
            return;
        }

        var isCompliant = !assessment.IndicatesViolation;
        var newHistory = profile.ComplianceHistory.Append(assessment).ToArray();

        // Calculate new metrics
        var totalInteractions = profile.TotalInteractions + 1;
        var successfulCompliances = profile.SuccessfulCompliances + (isCompliant ? 1 : 0);
        var violationCount = profile.ViolationCount + (isCompliant ? 0 : 1);

        // Determine trend
        var trend = CalculateComplianceTrend(newHistory);
        var overallLevel = DetermineOverallComplianceLevel(successfulCompliances, totalInteractions);

        var updatedProfile = new AgentConstraintAdherence(
            AgentId: agentId,
            ComplianceLevel: overallLevel,
            ComplianceTrend: trend,
            TotalInteractions: totalInteractions,
            SuccessfulCompliances: successfulCompliances,
            ViolationCount: violationCount,
            LastInteractionAt: DateTime.UtcNow,
            ComplianceHistory: newHistory,
            RecentViolations: profile.RecentViolations
        );

        _agentProfiles[agentId] = updatedProfile;
    }

    private static ComplianceTrend CalculateComplianceTrend(IEnumerable<ConstraintComplianceAssessment> history)
    {
        var recentAssessments = history.TakeLast(5).ToArray();
        if (recentAssessments.Length < 3)
        {
            return ComplianceTrend.Unknown;
        }

        var firstHalf = recentAssessments.Take(recentAssessments.Length / 2);
        var secondHalf = recentAssessments.Skip(recentAssessments.Length / 2);

        var firstAvg = firstHalf.Average(a => (int)a.ComplianceLevel);
        var secondAvg = secondHalf.Average(a => (int)a.ComplianceLevel);

        var difference = secondAvg - firstAvg;

        return difference switch
        {
            > 0.5 => ComplianceTrend.Improving,
            < -0.5 => ComplianceTrend.Declining,
            _ => ComplianceTrend.Stable
        };
    }

    private static ConstraintComplianceLevel DetermineOverallComplianceLevel(int successfulCompliances, int totalInteractions)
    {
        if (totalInteractions == 0)
        {
            return ConstraintComplianceLevel.AdequatelyCompliant;
        }

        var rate = (double)successfulCompliances / totalInteractions;

        return rate switch
        {
            >= 0.95 => ConstraintComplianceLevel.ExceptionallyCompliant,
            >= 0.85 => ConstraintComplianceLevel.HighlyCompliant,
            >= 0.70 => ConstraintComplianceLevel.AdequatelyCompliant,
            >= 0.50 => ConstraintComplianceLevel.PartiallyCompliant,
            _ => ConstraintComplianceLevel.NonCompliant
        };
    }

    private static AgentConstraintAdherence UpdateProfileWithViolation(AgentConstraintAdherence profile, ConstraintViolation violation)
    {
        var newViolations = profile.RecentViolations.Append(violation).TakeLast(20).ToArray();
        var newViolationCount = profile.ViolationCount + 1;
        var newTrend = newViolationCount > profile.ViolationCount && profile.ComplianceTrend != ComplianceTrend.Declining
            ? ComplianceTrend.Declining
            : profile.ComplianceTrend;

        return profile with
        {
            ViolationCount = newViolationCount,
            ComplianceTrend = newTrend,
            RecentViolations = newViolations,
            LastInteractionAt = violation.DetectedAt
        };
    }

    private static IReadOnlyList<string> DetermineRiskFactors(ConstraintViolation violation, AgentConstraintAdherence profile)
    {
        var riskFactors = new List<string>();

        if (violation.Severity >= ViolationSeverity.Major)
        {
            riskFactors.Add("High severity violation detected");
        }

        if (profile.ComplianceTrend == ComplianceTrend.Declining)
        {
            riskFactors.Add("Declining compliance trend");
        }

        if (profile.ViolationRate > 0.3)
        {
            riskFactors.Add("High violation rate");
        }

        return riskFactors.AsReadOnly();
    }

    private static IReadOnlyList<string> GenerateViolationRecommendations(ConstraintViolation violation, AgentConstraintAdherence profile)
    {
        var recommendations = new List<string>();

        if (violation.RequiresImmediateAttention)
        {
            recommendations.Add("Schedule immediate intervention session");
        }

        if (profile.ComplianceTrend == ComplianceTrend.Declining)
        {
            recommendations.Add("Investigate root cause of declining compliance");
        }

        recommendations.Add($"Review constraint guidance for: {violation.ConstraintId}");

        return recommendations.AsReadOnly();
    }
}
