using System;

namespace ConstraintMcpServer.Domain.Feedback;

/// <summary>
/// Immutable representation of basic effectiveness score for a constraint.
/// Provides simple effectiveness metrics without complex algorithms.
/// Follows CUPID principles with domain-based language and predictable behavior.
/// </summary>
public sealed record BasicEffectivenessScore
{
    private const double NeutralEffectivenessScore = 0.5;
    private const double NeutralRatingWeight = 0.5;
    private const int EffectivenessScorePrecision = 3;
    /// <summary>
    /// Gets the constraint this score relates to.
    /// </summary>
    public string ConstraintId { get; }

    /// <summary>
    /// Gets the effectiveness score (0.0 to 1.0).
    /// </summary>
    public double EffectivenessScore { get; }

    /// <summary>
    /// Gets the total number of ratings received.
    /// </summary>
    public int TotalRatings { get; }

    /// <summary>
    /// Gets the number of positive ratings.
    /// </summary>
    public int PositiveRatings { get; }

    /// <summary>
    /// Gets the number of negative ratings.
    /// </summary>
    public int NegativeRatings { get; }

    /// <summary>
    /// Gets when this score was last updated.
    /// </summary>
    public DateTimeOffset LastUpdated { get; }

    /// <summary>
    /// Creates a new effectiveness score.
    /// </summary>
    /// <param name="constraintId">The constraint this score relates to</param>
    /// <param name="effectivenessScore">The effectiveness score (0.0 to 1.0)</param>
    /// <param name="totalRatings">Total number of ratings</param>
    /// <param name="positiveRatings">Number of positive ratings</param>
    /// <param name="negativeRatings">Number of negative ratings</param>
    /// <param name="lastUpdated">When this score was last updated</param>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
    public BasicEffectivenessScore(
        string constraintId,
        double effectivenessScore,
        int totalRatings,
        int positiveRatings,
        int negativeRatings,
        DateTimeOffset lastUpdated)
    {
        if (string.IsNullOrWhiteSpace(constraintId))
        {
            throw new ArgumentException("Constraint ID cannot be null or empty", nameof(constraintId));
        }

        if (effectivenessScore < 0.0 || effectivenessScore > 1.0)
        {
            throw new ArgumentException("Effectiveness score must be between 0.0 and 1.0", nameof(effectivenessScore));
        }

        if (totalRatings < 0)
        {
            throw new ArgumentException("Total ratings cannot be negative", nameof(totalRatings));
        }

        if (positiveRatings < 0)
        {
            throw new ArgumentException("Positive ratings cannot be negative", nameof(positiveRatings));
        }

        if (negativeRatings < 0)
        {
            throw new ArgumentException("Negative ratings cannot be negative", nameof(negativeRatings));
        }

        if (positiveRatings + negativeRatings > totalRatings)
        {
            throw new ArgumentException("Positive and negative ratings cannot exceed total ratings");
        }

        ConstraintId = constraintId;
        EffectivenessScore = effectivenessScore;
        TotalRatings = totalRatings;
        PositiveRatings = positiveRatings;
        NegativeRatings = negativeRatings;
        LastUpdated = lastUpdated;
    }

    /// <summary>
    /// Creates an effectiveness score from rating counts.
    /// </summary>
    /// <param name="constraintId">The constraint being scored</param>
    /// <param name="positiveCount">Number of positive ratings</param>
    /// <param name="negativeCount">Number of negative ratings</param>
    /// <param name="neutralCount">Number of neutral ratings</param>
    /// <returns>New effectiveness score based on rating counts</returns>
    public static BasicEffectivenessScore FromRatingCounts(
        string constraintId,
        int positiveCount,
        int negativeCount,
        int neutralCount = 0)
    {
        var totalRatings = positiveCount + negativeCount + neutralCount;

        var effectivenessScore = totalRatings == 0
            ? NeutralEffectivenessScore
            : (positiveCount + (neutralCount * NeutralRatingWeight)) / totalRatings;

        return new BasicEffectivenessScore(
            constraintId,
            Math.Round(effectivenessScore, EffectivenessScorePrecision),
            totalRatings,
            positiveCount,
            negativeCount,
            DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Creates a zero-ratings effectiveness score.
    /// </summary>
    /// <param name="constraintId">The constraint being scored</param>
    /// <returns>Effectiveness score with no ratings</returns>
    public static BasicEffectivenessScore NoRatings(string constraintId)
    {
        return new BasicEffectivenessScore(
            constraintId,
            NeutralEffectivenessScore,
            0,
            0,
            0,
            DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Gets the neutral rating count (calculated).
    /// </summary>
    public int NeutralRatings => TotalRatings - PositiveRatings - NegativeRatings;

    /// <summary>
    /// Gets the positive rating percentage.
    /// </summary>
    public double PositivePercentage => TotalRatings == 0 ? 0.0 : (double)PositiveRatings / TotalRatings;

    /// <summary>
    /// Gets the negative rating percentage.
    /// </summary>
    public double NegativePercentage => TotalRatings == 0 ? 0.0 : (double)NegativeRatings / TotalRatings;

    /// <summary>
    /// Gets whether this constraint is considered highly effective (>= 0.8).
    /// </summary>
    public bool IsHighlyEffective => EffectivenessScore >= 0.8;

    /// <summary>
    /// Gets whether this constraint is considered moderately effective (0.4 to 0.8).
    /// </summary>
    public bool IsModeratelyEffective => EffectivenessScore >= 0.4 && EffectivenessScore < 0.8;

    /// <summary>
    /// Gets whether this constraint is considered poorly effective (< 0.4).
    /// </summary>
    public bool IsPoorlyEffective => EffectivenessScore < 0.4;

    /// <summary>
    /// Gets whether this score is based on sufficient data (>= 5 ratings).
    /// </summary>
    public bool HasSufficientData => TotalRatings >= 5;

    /// <summary>
    /// Updates this score with a new rating.
    /// </summary>
    /// <param name="rating">The new rating to incorporate</param>
    /// <returns>New effectiveness score with the rating incorporated</returns>
    public BasicEffectivenessScore WithNewRating(SimpleUserRating rating)
    {
        if (rating.ConstraintId != ConstraintId)
        {
            throw new ArgumentException("Rating constraint ID must match score constraint ID");
        }

        var newPositive = PositiveRatings + (rating.IsPositive ? 1 : 0);
        var newNegative = NegativeRatings + (rating.IsNegative ? 1 : 0);
        var newNeutral = NeutralRatings + (rating.IsNeutral ? 1 : 0);

        return FromRatingCounts(ConstraintId, newPositive, newNegative, newNeutral);
    }
}
