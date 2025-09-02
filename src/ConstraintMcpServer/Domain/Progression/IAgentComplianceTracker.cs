using System.Threading.Tasks;

namespace ConstraintMcpServer.Domain.Progression;

/// <summary>
/// Port for tracking agent constraint compliance in real-time.
/// Provides sub-50ms violation detection and compliance metrics updates.
/// Core component of the Agent Constraint Adherence Intelligence system.
/// </summary>
public interface IAgentComplianceTracker
{
    /// <summary>
    /// Tracks agent compliance for a constraint interaction.
    /// Performance requirement: Sub-50ms p95 latency.
    /// </summary>
    /// <param name="interaction">The constraint interaction to evaluate</param>
    /// <returns>Compliance assessment with violation detection</returns>
    Task<ConstraintComplianceAssessment> TrackComplianceAsync(ConstraintInteraction interaction);

    /// <summary>
    /// Records a constraint violation with full context.
    /// Updates compliance metrics in real-time.
    /// </summary>
    /// <param name="violation">The constraint violation to record</param>
    /// <returns>Updated compliance metrics</returns>
    Task<ComplianceAnalysisResult> RecordViolationAsync(ConstraintViolation violation);

    /// <summary>
    /// Retrieves historical compliance data for an agent.
    /// Used for trend analysis and drift detection.
    /// </summary>
    /// <param name="agentId">Identifier for the agent</param>
    /// <returns>Agent's historical constraint adherence data</returns>
    Task<AgentConstraintAdherence> GetComplianceHistoryAsync(string agentId);
}
