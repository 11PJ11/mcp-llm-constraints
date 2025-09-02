using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Domain.Progression;

/// <summary>
/// Port for adaptive constraint optimization based on agent behavior patterns.
/// Analyzes compliance patterns and generates data-driven constraint refinement suggestions.
/// Performance requirement: Sub-200ms constraint adaptation latency.
/// </summary>
public interface IConstraintAdaptationEngine
{
    /// <summary>
    /// Analyzes constraint effectiveness across multiple agents.
    /// Identifies patterns in compliance success and failure.
    /// Performance requirement: Sub-200ms p95 latency.
    /// </summary>
    /// <param name="multiAgentHistory">Compliance history from multiple agents</param>
    /// <returns>Effectiveness analysis with confidence metrics</returns>
    Task<ComplianceAnalysisResult> AnalyzeConstraintEffectivenessAsync(IEnumerable<AgentConstraintAdherence> multiAgentHistory);

    /// <summary>
    /// Generates constraint refinement suggestions based on agent behavior patterns.
    /// Uses data-driven analysis to improve constraint effectiveness.
    /// </summary>
    /// <param name="effectivenessAnalysis">Results of constraint effectiveness analysis</param>
    /// <returns>Ranked refinement suggestions with confidence levels</returns>
    Task<IEnumerable<ConstraintRefinementSuggestion>> GenerateRefinementSuggestionsAsync(ComplianceAnalysisResult effectivenessAnalysis);

    /// <summary>
    /// Ranks refinement suggestions by confidence level and expected impact.
    /// Prioritizes suggestions most likely to improve compliance.
    /// </summary>
    /// <param name="suggestions">Collection of refinement suggestions</param>
    /// <returns>Suggestions ranked by confidence and impact</returns>
    Task<IEnumerable<ConstraintRefinementSuggestion>> RankSuggestionsByConfidenceAsync(IEnumerable<ConstraintRefinementSuggestion> suggestions);
}
