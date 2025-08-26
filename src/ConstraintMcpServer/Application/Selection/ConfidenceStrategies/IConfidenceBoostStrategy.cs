using ConstraintMcpServer.Domain.Context;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Application.Selection.ConfidenceStrategies;

/// <summary>
/// Strategy interface for applying confidence score boosts.
/// Allows different boosting algorithms based on context.
/// </summary>
public interface IConfidenceBoostStrategy
{
    /// <summary>
    /// Determines if this strategy applies to the given constraint and context.
    /// </summary>
    bool AppliesTo(AtomicConstraint constraint, TriggerContext context);

    /// <summary>
    /// Calculates the boosted confidence score.
    /// </summary>
    double ApplyBoost(double baseScore);
}
