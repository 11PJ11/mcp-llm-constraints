using System;

namespace ConstraintMcpServer.Domain.Progression;

/// <summary>
/// Domain entity representing a compliance assessment for a specific constraint interaction.
/// Immutable record capturing the evaluation of agent adherence to constraints.
/// </summary>
/// <param name="AssessmentId">Unique identifier for this compliance assessment</param>
/// <param name="InteractionId">The constraint interaction being assessed</param>
/// <param name="AgentId">Identifier of the agent being assessed</param>
/// <param name="ConstraintId">Identifier of the constraint being evaluated</param>
/// <param name="ComplianceLevel">Level of compliance demonstrated</param>
/// <param name="AssessmentConfidence">Confidence in the assessment accuracy (0.0-1.0)</param>
/// <param name="AssessedAt">When this assessment was performed</param>
/// <param name="AssessmentNotes">Detailed notes about the compliance evaluation</param>
/// <param name="RecommendedAction">Suggested action based on assessment</param>
public sealed record ConstraintComplianceAssessment(
    Guid AssessmentId,
    Guid InteractionId,
    string AgentId,
    string ConstraintId,
    ConstraintComplianceLevel ComplianceLevel,
    double AssessmentConfidence,
    DateTime AssessedAt,
    string? AssessmentNotes,
    string? RecommendedAction
)
{
    /// <summary>
    /// Validates that the assessment has required fields and valid confidence range.
    /// </summary>
    public bool IsValid =>
        AssessmentId != Guid.Empty &&
        InteractionId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(AgentId) &&
        !string.IsNullOrWhiteSpace(ConstraintId) &&
        AssessmentConfidence is >= 0.0 and <= 1.0 &&
        AssessedAt != default;

    /// <summary>
    /// Determines if this assessment indicates a compliance violation.
    /// </summary>
    public bool IndicatesViolation =>
        ComplianceLevel <= ConstraintComplianceLevel.PartiallyCompliant;

    /// <summary>
    /// Determines if this assessment has high confidence (>= 0.8).
    /// </summary>
    public bool HasHighConfidence => AssessmentConfidence >= 0.8;

    /// <summary>
    /// Calculates how long ago this assessment was performed.
    /// </summary>
    public TimeSpan TimeSinceAssessment => DateTime.UtcNow - AssessedAt;
}
