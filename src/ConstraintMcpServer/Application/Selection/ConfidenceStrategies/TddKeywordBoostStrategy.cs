using System;
using ConstraintMcpServer.Domain.Context;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Application.Selection.ConfidenceStrategies;

/// <summary>
/// Confidence boost strategy for TDD constraints with strong keyword indicators.
/// </summary>
public sealed class TddKeywordBoostStrategy : IConfidenceBoostStrategy
{
    private const double BoostFactor = 1.1;
    private const double MaxScore = 1.0;
    private static readonly string[] TddStrongIndicators = { "implement", "feature", "test" };

    public bool AppliesTo(AtomicConstraint constraint, TriggerContext context)
    {
        return constraint.Id.Value == "tdd.test-first" &&
               context.ContainsAnyKeyword(TddStrongIndicators);
    }

    public double ApplyBoost(double baseScore)
    {
        return Math.Min(MaxScore, baseScore * BoostFactor);
    }
}
