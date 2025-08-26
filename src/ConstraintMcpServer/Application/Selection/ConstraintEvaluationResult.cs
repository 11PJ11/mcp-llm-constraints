using System;
using ConstraintMcpServer.Domain.Context;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Application.Selection;

/// <summary>
/// Value object representing the result of constraint evaluation.
/// Encapsulates the relevance score and activation decision.
/// </summary>
public sealed class ConstraintEvaluationResult : IEquatable<ConstraintEvaluationResult>
{
    public double RelevanceScore { get; }
    public bool ShouldActivate { get; }
    public ActivationReason Reason { get; }
    public IConstraint Constraint { get; }
    public TriggerContext Context { get; }

    public ConstraintEvaluationResult(
        double relevanceScore,
        bool shouldActivate,
        ActivationReason reason,
        IConstraint constraint,
        TriggerContext context)
    {
        RelevanceScore = relevanceScore;
        ShouldActivate = shouldActivate;
        Reason = reason;
        Constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public ConstraintActivation? ToActivation(string constraintId)
    {
        if (!ShouldActivate)
        {
            return null;
        }

        return new ConstraintActivation(
            constraintId: constraintId,
            confidenceScore: RelevanceScore,
            reason: Reason,
            triggerContext: Context,
            constraint: Constraint
        );
    }

    public bool Equals(ConstraintEvaluationResult? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Math.Abs(RelevanceScore - other.RelevanceScore) < 0.0001 &&
               ShouldActivate == other.ShouldActivate &&
               Reason == other.Reason;
    }

    public override bool Equals(object? obj) => Equals(obj as ConstraintEvaluationResult);

    public override int GetHashCode() => HashCode.Combine(RelevanceScore, ShouldActivate, Reason);
}
