using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain.Progression;

namespace ConstraintMcpServer.Application.AgentCompliance;

/// <summary>
/// Application service implementing adaptive constraint optimization based on agent behavior patterns.
/// Analyzes compliance patterns and generates data-driven constraint refinement suggestions.
/// Performance requirement: Sub-200ms constraint adaptation latency.
/// </summary>
public sealed class ConstraintAdaptationEngine : IConstraintAdaptationEngine
{
    private readonly Dictionary<string, List<ConstraintRefinementSuggestion>> _refinementHistory = new();

    /// <summary>
    /// Analyzes constraint effectiveness across multiple agents.
    /// Identifies patterns in compliance success and failure.
    /// Performance requirement: Sub-200ms p95 latency.
    /// </summary>
    public async Task<ComplianceAnalysisResult> AnalyzeConstraintEffectivenessAsync(IEnumerable<AgentConstraintAdherence> multiAgentHistory)
    {
        var startTime = DateTime.UtcNow;
        var agents = multiAgentHistory.ToArray();

        if (agents.Length == 0)
        {
            throw new ArgumentException("No agent history provided", nameof(multiAgentHistory));
        }

        // Analyze overall effectiveness
        var overallCompliance = CalculateOverallCompliance(agents);
        var systemTrend = AnalyzeSystemTrend(agents);
        var effectivenessScore = CalculateEffectivenessScore(agents);

        // Generate insights
        var findings = GenerateEffectivenessFindings(agents, effectivenessScore);
        var riskFactors = IdentifySystemRiskFactors(agents);
        var recommendations = GenerateEffectivenessRecommendations(agents, effectivenessScore);

        var analysisResult = new ComplianceAnalysisResult(
            AnalysisId: Guid.NewGuid(),
            AgentId: "system-wide",
            AnalysisType: "EffectivenessAnalysis",
            AnalyzedAt: DateTime.UtcNow,
            ComplianceScore: overallCompliance,
            TrendDirection: systemTrend,
            AnalysisConfidence: CalculateAnalysisConfidence(agents),
            KeyFindings: findings,
            RiskFactors: riskFactors,
            Recommendations: recommendations,
            MetricsAnalyzed: agents.Sum(a => a.TotalInteractions)
        );

        // Ensure sub-200ms performance
        var processingTime = DateTime.UtcNow - startTime;
        if (processingTime.TotalMilliseconds > 200)
        {
            Console.WriteLine($"Warning: Effectiveness analysis took {processingTime.TotalMilliseconds}ms");
        }

        return await Task.FromResult(analysisResult);
    }

    /// <summary>
    /// Generates constraint refinement suggestions based on agent behavior patterns.
    /// Uses data-driven analysis to improve constraint effectiveness.
    /// </summary>
    public async Task<IEnumerable<ConstraintRefinementSuggestion>> GenerateRefinementSuggestionsAsync(ComplianceAnalysisResult effectivenessAnalysis)
    {
        if (!effectivenessAnalysis.IsValid)
        {
            throw new ArgumentException("Invalid effectiveness analysis", nameof(effectivenessAnalysis));
        }

        var suggestions = new List<ConstraintRefinementSuggestion>();

        // Generate suggestions based on analysis findings
        suggestions.AddRange(GenerateComplianceBasedSuggestions(effectivenessAnalysis));
        suggestions.AddRange(GenerateTrendBasedSuggestions(effectivenessAnalysis));
        suggestions.AddRange(GenerateRiskBasedSuggestions(effectivenessAnalysis));

        // Store suggestions for tracking
        if (!_refinementHistory.ContainsKey(effectivenessAnalysis.AgentId))
        {
            _refinementHistory[effectivenessAnalysis.AgentId] = new List<ConstraintRefinementSuggestion>();
        }
        _refinementHistory[effectivenessAnalysis.AgentId].AddRange(suggestions);

        return await Task.FromResult(suggestions);
    }

    /// <summary>
    /// Ranks refinement suggestions by confidence level and expected impact.
    /// Prioritizes suggestions most likely to improve compliance.
    /// </summary>
    public async Task<IEnumerable<ConstraintRefinementSuggestion>> RankSuggestionsByConfidenceAsync(IEnumerable<ConstraintRefinementSuggestion> suggestions)
    {
        var suggestionArray = suggestions.ToArray();

        // Validate all suggestions
        var validSuggestions = suggestionArray.Where(s => s.IsValid).ToArray();
        if (validSuggestions.Length != suggestionArray.Length)
        {
            Console.WriteLine($"Warning: {suggestionArray.Length - validSuggestions.Length} invalid suggestions filtered out");
        }

        // Sort by priority score (combination of confidence and impact)
        var rankedSuggestions = validSuggestions
            .OrderByDescending(s => s.PriorityScore)
            .ThenByDescending(s => s.ConfidenceLevel)
            .ThenByDescending(s => s.ExpectedImpact)
            .ToArray();

        return await Task.FromResult(rankedSuggestions);
    }

    private static double CalculateOverallCompliance(AgentConstraintAdherence[] agents)
    {
        if (agents.Length == 0)
        {
            return 0.0;
        }

        var totalInteractions = agents.Sum(a => a.TotalInteractions);
        var totalSuccesses = agents.Sum(a => a.SuccessfulCompliances);

        return totalInteractions > 0 ? (double)totalSuccesses / totalInteractions : 0.0;
    }

    private static ComplianceTrend AnalyzeSystemTrend(AgentConstraintAdherence[] agents)
    {
        if (agents.Length == 0)
        {
            return ComplianceTrend.Unknown;
        }

        var trendCounts = new Dictionary<ComplianceTrend, int>
        {
            { ComplianceTrend.Improving, 0 },
            { ComplianceTrend.Stable, 0 },
            { ComplianceTrend.Declining, 0 },
            { ComplianceTrend.Unknown, 0 }
        };

        foreach (var agent in agents)
        {
            trendCounts[agent.ComplianceTrend]++;
        }

        // Return the dominant trend
        return trendCounts.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    private static double CalculateEffectivenessScore(AgentConstraintAdherence[] agents)
    {
        if (agents.Length == 0)
        {
            return 0.0;
        }

        var complianceScore = CalculateOverallCompliance(agents);
        var trendBonus = AnalyzeSystemTrend(agents) switch
        {
            ComplianceTrend.Improving => 0.1,
            ComplianceTrend.Declining => -0.1,
            _ => 0.0
        };

        // Factor in consistency across agents
        var complianceRates = agents.Select(a => a.ComplianceRate).ToArray();
        var consistency = complianceRates.Length > 1
            ? 1.0 - CalculateStandardDeviation(complianceRates)
            : 1.0;

        var effectivenessScore = (complianceScore * 0.7) + (consistency * 0.2) + (0.1);
        effectivenessScore += trendBonus;

        return Math.Max(0.0, Math.Min(1.0, effectivenessScore));
    }

    private static double CalculateStandardDeviation(double[] values)
    {
        if (values.Length <= 1)
        {
            return 0.0;
        }

        var mean = values.Average();
        var sumOfSquares = values.Sum(x => Math.Pow(x - mean, 2));

        return Math.Sqrt(sumOfSquares / values.Length);
    }

    private static double CalculateAnalysisConfidence(AgentConstraintAdherence[] agents)
    {
        var baseConfidence = 0.6;

        // Higher confidence with more agents
        baseConfidence += Math.Min(agents.Length * 0.05, 0.2);

        // Higher confidence with sufficient data
        var totalInteractions = agents.Sum(a => a.TotalInteractions);
        if (totalInteractions > 100)
        {
            baseConfidence += 0.1;
        }

        // Higher confidence if agents have sufficient individual history
        var agentsWithSufficientHistory = agents.Count(a => a.HasSufficientHistory);
        if (agentsWithSufficientHistory > agents.Length / 2)
        {
            baseConfidence += 0.1;
        }

        return Math.Min(baseConfidence, 1.0);
    }

    private static IReadOnlyList<string> GenerateEffectivenessFindings(AgentConstraintAdherence[] agents, double effectivenessScore)
    {
        var findings = new List<string>();

        findings.Add($"System effectiveness score: {effectivenessScore:P1}");
        findings.Add($"Analyzed {agents.Length} agents with {agents.Sum(a => a.TotalInteractions)} total interactions");

        var overallCompliance = CalculateOverallCompliance(agents);
        findings.Add($"Overall compliance rate: {overallCompliance:P1}");

        var systemTrend = AnalyzeSystemTrend(agents);
        findings.Add($"System trend: {systemTrend}");

        // Identify high and low performers
        var highPerformers = agents.Where(a => a.ComplianceRate > 0.9).Count();
        var lowPerformers = agents.Where(a => a.ComplianceRate < 0.6).Count();

        if (highPerformers > 0)
        {
            findings.Add($"{highPerformers} agents demonstrating exceptional compliance (>90%)");
        }

        if (lowPerformers > 0)
        {
            findings.Add($"{lowPerformers} agents requiring improvement (<60% compliance)");
        }

        return findings.AsReadOnly();
    }

    private static IReadOnlyList<string> IdentifySystemRiskFactors(AgentConstraintAdherence[] agents)
    {
        var riskFactors = new List<string>();

        var decliningAgents = agents.Count(a => a.ComplianceTrend == ComplianceTrend.Declining);
        if (decliningAgents > agents.Length / 3)
        {
            riskFactors.Add($"High number of agents with declining compliance: {decliningAgents}/{agents.Length}");
        }

        var criticalViolations = agents.Sum(a => a.CriticalViolationCount);
        if (criticalViolations > 0)
        {
            riskFactors.Add($"System has {criticalViolations} critical violations requiring attention");
        }

        var overallCompliance = CalculateOverallCompliance(agents);
        if (overallCompliance < 0.8)
        {
            riskFactors.Add("System compliance below recommended threshold (80%)");
        }

        var inconsistentPerformance = agents.Select(a => a.ComplianceRate).ToArray();
        var stdDev = CalculateStandardDeviation(inconsistentPerformance);
        if (stdDev > 0.2)
        {
            riskFactors.Add("High variance in agent performance indicates inconsistent constraint effectiveness");
        }

        return riskFactors.AsReadOnly();
    }

    private static IReadOnlyList<string> GenerateEffectivenessRecommendations(AgentConstraintAdherence[] agents, double effectivenessScore)
    {
        var recommendations = new List<string>();

        if (effectivenessScore < 0.7)
        {
            recommendations.Add("Consider comprehensive constraint review and refinement");
        }

        var lowPerformers = agents.Where(a => a.ComplianceRate < 0.6).ToArray();
        if (lowPerformers.Length > 0)
        {
            recommendations.Add($"Focus improvement efforts on {lowPerformers.Length} underperforming agents");
        }

        var decliningTrend = agents.Count(a => a.ComplianceTrend == ComplianceTrend.Declining) > agents.Length / 2;
        if (decliningTrend)
        {
            recommendations.Add("Investigate system-wide factors contributing to declining compliance");
        }

        recommendations.Add("Implement regular effectiveness monitoring and adaptive constraint refinement");

        var highPerformers = agents.Where(a => a.ComplianceRate > 0.9).ToArray();
        if (highPerformers.Length > 0)
        {
            recommendations.Add($"Study success patterns from {highPerformers.Length} high-performing agents");
        }

        return recommendations.AsReadOnly();
    }

    private static IEnumerable<ConstraintRefinementSuggestion> GenerateComplianceBasedSuggestions(ComplianceAnalysisResult analysis)
    {
        var suggestions = new List<ConstraintRefinementSuggestion>();

        if (analysis.ComplianceScore < 0.8)
        {
            suggestions.Add(new ConstraintRefinementSuggestion(
                SuggestionId: Guid.NewGuid(),
                ConstraintId: "system-wide",
                RefinementType: "Compliance Enhancement",
                Suggestion: "Strengthen constraint guidance with more specific examples and clearer expectations",
                Rationale: $"Low system compliance ({analysis.ComplianceScore:P1}) indicates need for clearer guidance",
                ConfidenceLevel: 0.8,
                ExpectedImpact: 0.3,
                GeneratedAt: DateTime.UtcNow,
                BasedOnInteractions: analysis.MetricsAnalyzed
            ));
        }

        return suggestions;
    }

    private static IEnumerable<ConstraintRefinementSuggestion> GenerateTrendBasedSuggestions(ComplianceAnalysisResult analysis)
    {
        var suggestions = new List<ConstraintRefinementSuggestion>();

        if (analysis.TrendDirection == ComplianceTrend.Declining)
        {
            suggestions.Add(new ConstraintRefinementSuggestion(
                SuggestionId: Guid.NewGuid(),
                ConstraintId: "system-wide",
                RefinementType: "Trend Correction",
                Suggestion: "Implement immediate corrective measures to reverse declining compliance trend",
                Rationale: "Declining system trend requires urgent intervention to prevent further deterioration",
                ConfidenceLevel: 0.85,
                ExpectedImpact: 0.5,
                GeneratedAt: DateTime.UtcNow,
                BasedOnInteractions: analysis.MetricsAnalyzed
            ));
        }

        return suggestions;
    }

    private static IEnumerable<ConstraintRefinementSuggestion> GenerateRiskBasedSuggestions(ComplianceAnalysisResult analysis)
    {
        var suggestions = new List<ConstraintRefinementSuggestion>();

        if (analysis.RiskFactors.Count > 2)
        {
            suggestions.Add(new ConstraintRefinementSuggestion(
                SuggestionId: Guid.NewGuid(),
                ConstraintId: "system-wide",
                RefinementType: "Risk Mitigation",
                Suggestion: "Address identified risk factors through targeted constraint modifications",
                Rationale: $"Multiple risk factors identified: {string.Join(", ", analysis.RiskFactors.Take(2))}",
                ConfidenceLevel: 0.75,
                ExpectedImpact: 0.4,
                GeneratedAt: DateTime.UtcNow,
                BasedOnInteractions: analysis.MetricsAnalyzed
            ));
        }

        return suggestions;
    }
}
