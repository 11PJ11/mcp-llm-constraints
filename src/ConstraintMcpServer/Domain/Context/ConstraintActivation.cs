using System;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Domain.Context;

/// <summary>
/// Represents an activated constraint with confidence and activation metadata.
/// Value object containing business information about constraint selection.
/// </summary>
public sealed class ConstraintActivation : IEquatable<ConstraintActivation>
{
    /// <summary>
    /// Unique identifier for the activated constraint.
    /// </summary>
    public string ConstraintId { get; init; } = string.Empty;

    /// <summary>
    /// Confidence score for this activation between 0.0 and 1.0.
    /// Higher scores indicate stronger relevance to the current context.
    /// </summary>
    public double ConfidenceScore { get; init; }

    /// <summary>
    /// Reason for constraint activation with business context.
    /// </summary>
    public ActivationReason Reason { get; init; } = ActivationReason.Unknown;

    /// <summary>
    /// Timestamp when this constraint was activated.
    /// </summary>
    public DateTimeOffset ActivatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Context that triggered this constraint activation.
    /// Provides audit trail for activation decisions.
    /// </summary>
    public TriggerContext TriggerContext { get; init; } = null!;

    /// <summary>
    /// Reference to the activated constraint for accessing reminders and configuration.
    /// </summary>
    public IConstraint Constraint { get; init; } = null!;

    /// <summary>
    /// Creates a new constraint activation.
    /// </summary>
    /// <param name="constraintId">Constraint identifier</param>
    /// <param name="confidenceScore">Confidence score (0.0 to 1.0)</param>
    /// <param name="reason">Activation reason</param>
    /// <param name="triggerContext">Context that triggered activation</param>
    /// <param name="constraint">Reference to the constraint</param>
    public ConstraintActivation(
        string constraintId,
        double confidenceScore,
        ActivationReason reason,
        TriggerContext triggerContext,
        IConstraint constraint)
    {
        if (string.IsNullOrWhiteSpace(constraintId))
        {
            throw new ArgumentException("Constraint ID cannot be null or empty", nameof(constraintId));
        }

        if (confidenceScore < 0.0 || confidenceScore > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(confidenceScore),
                "Confidence score must be between 0.0 and 1.0");
        }

        ConstraintId = constraintId;
        ConfidenceScore = confidenceScore;
        Reason = reason;
        TriggerContext = triggerContext ?? throw new ArgumentNullException(nameof(triggerContext));
        Constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
    }

    /// <summary>
    /// Determines if this constraint activation meets the specified confidence threshold.
    /// </summary>
    /// <param name="threshold">Minimum confidence threshold</param>
    /// <returns>True if confidence score meets or exceeds threshold</returns>
    public bool MeetsConfidenceThreshold(double threshold)
    {
        return ConfidenceScore >= threshold;
    }

    /// <summary>
    /// Gets a human-readable description of this constraint activation.
    /// </summary>
    /// <returns>Formatted activation description</returns>
    public string GetActivationDescription()
    {
        return $"Constraint '{ConstraintId}' activated with {ConfidenceScore:P1} confidence due to {Reason}";
    }

    #region Equality Implementation

    public bool Equals(ConstraintActivation? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ConstraintId == other.ConstraintId &&
               Math.Abs(ConfidenceScore - other.ConfidenceScore) < 0.001 &&
               Reason == other.Reason;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is ConstraintActivation other && Equals(other));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ConstraintId, ConfidenceScore, Reason);
    }

    public static bool operator ==(ConstraintActivation? left, ConstraintActivation? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ConstraintActivation? left, ConstraintActivation? right)
    {
        return !Equals(left, right);
    }

    #endregion

    public override string ToString()
    {
        return $"ConstraintActivation(Id: {ConstraintId}, Confidence: {ConfidenceScore:F2}, Reason: {Reason})";
    }
}
