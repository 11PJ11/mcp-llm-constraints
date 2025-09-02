using System;

namespace ConstraintMcpServer.Domain.Progression;

/// <summary>
/// Value object representing a single interaction between an agent and the constraint system.
/// Immutable record capturing the context and details of constraint enforcement.
/// </summary>
/// <param name="InteractionId">Unique identifier for this constraint interaction</param>
/// <param name="AgentId">Identifier of the agent involved in the interaction</param>
/// <param name="ConstraintId">Identifier of the constraint being enforced</param>
/// <param name="Timestamp">When the interaction occurred</param>
/// <param name="Context">Contextual information about the interaction</param>
/// <param name="AgentResponse">How the agent responded to the constraint</param>
public sealed record ConstraintInteraction(
    Guid InteractionId,
    string AgentId,
    string ConstraintId,
    DateTime Timestamp,
    string Context,
    string? AgentResponse
)
{
    /// <summary>
    /// Validates that the constraint interaction has required fields.
    /// </summary>
    public bool IsValid =>
        InteractionId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(AgentId) &&
        !string.IsNullOrWhiteSpace(ConstraintId) &&
        Timestamp != default;

    /// <summary>
    /// Calculates the age of this interaction from the current time.
    /// </summary>
    public TimeSpan Age => DateTime.UtcNow - Timestamp;
}
