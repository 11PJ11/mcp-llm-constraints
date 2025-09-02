using System;

namespace ConstraintMcpServer.Domain.Progression;

/// <summary>
/// Domain entity representing a constraint violation detected in agent behavior.
/// Immutable record capturing violation details, severity, and context for analysis.
/// </summary>
/// <param name="ViolationId">Unique identifier for this violation</param>
/// <param name="InteractionId">The constraint interaction that resulted in this violation</param>
/// <param name="AgentId">Identifier of the agent that committed the violation</param>
/// <param name="ConstraintId">Identifier of the violated constraint</param>
/// <param name="Severity">Severity level of the violation</param>
/// <param name="DetectedAt">When the violation was detected</param>
/// <param name="ViolationContext">Detailed context about the violation</param>
/// <param name="ExpectedBehavior">What the agent should have done</param>
/// <param name="ActualBehavior">What the agent actually did</param>
public sealed record ConstraintViolation(
    Guid ViolationId,
    Guid InteractionId,
    string AgentId,
    string ConstraintId,
    ViolationSeverity Severity,
    DateTime DetectedAt,
    string ViolationContext,
    string ExpectedBehavior,
    string ActualBehavior
)
{
    /// <summary>
    /// Validates that the violation has all required fields.
    /// </summary>
    public bool IsValid =>
        ViolationId != Guid.Empty &&
        InteractionId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(AgentId) &&
        !string.IsNullOrWhiteSpace(ConstraintId) &&
        DetectedAt != default &&
        !string.IsNullOrWhiteSpace(ViolationContext) &&
        !string.IsNullOrWhiteSpace(ExpectedBehavior) &&
        !string.IsNullOrWhiteSpace(ActualBehavior);

    /// <summary>
    /// Determines if this violation requires immediate attention based on severity.
    /// </summary>
    public bool RequiresImmediateAttention =>
        Severity >= ViolationSeverity.Major;

    /// <summary>
    /// Calculates how long ago this violation was detected.
    /// </summary>
    public TimeSpan TimeSinceDetection => DateTime.UtcNow - DetectedAt;
}
