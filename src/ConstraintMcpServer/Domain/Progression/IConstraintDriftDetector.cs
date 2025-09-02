using System.Threading.Tasks;

namespace ConstraintMcpServer.Domain.Progression;

/// <summary>
/// Port for detecting constraint compliance drift and triggering proactive interventions.
/// Analyzes agent behavior patterns to identify deteriorating compliance before breakdown.
/// Performance requirement: Sub-25ms drift detection latency.
/// </summary>
public interface IConstraintDriftDetector
{
    /// <summary>
    /// Analyzes agent compliance patterns to detect drift.
    /// Performance requirement: Sub-25ms p95 latency.
    /// </summary>
    /// <param name="complianceHistory">Agent's historical compliance data</param>
    /// <returns>Drift analysis with severity assessment</returns>
    Task<ComplianceAnalysisResult> DetectComplianceDriftAsync(AgentConstraintAdherence complianceHistory);

    /// <summary>
    /// Assesses the severity of detected compliance drift.
    /// Determines urgency of intervention required.
    /// </summary>
    /// <param name="driftAnalysis">Results of drift detection analysis</param>
    /// <returns>Severity classification and intervention recommendations</returns>
    Task<ViolationSeverity> AssessDriftSeverityAsync(ComplianceAnalysisResult driftAnalysis);

    /// <summary>
    /// Triggers proactive intervention when drift is detected.
    /// Prevents critical compliance breakdown through early intervention.
    /// </summary>
    /// <param name="severity">Severity level of detected drift</param>
    /// <param name="agentId">Identifier for the affected agent</param>
    /// <returns>Intervention strategy and expected outcomes</returns>
    Task<ConstraintRefinementSuggestion> TriggerProactiveInterventionAsync(ViolationSeverity severity, string agentId);
}
