using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain.Feedback;
using ConstraintMcpServer.Domain.Common;
using ConstraintMcpServer.Infrastructure.Storage;

namespace ConstraintMcpServer.Application.Feedback;

/// <summary>
/// Application service for tracking constraint effectiveness through user ratings.
/// Focuses on effectiveness calculation and coordination with storage.
/// Follows CUPID principles with domain-focused language and predictable behavior.
/// </summary>
public sealed class BasicEffectivenessTracker : IDisposable
{
    private readonly IRatingStore _ratingStore;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the BasicEffectivenessTracker.
    /// </summary>
    /// <param name="ratingStore">The rating storage implementation</param>
    public BasicEffectivenessTracker(IRatingStore? ratingStore = null)
    {
        _ratingStore = ratingStore ?? new SqliteRatingStore();
    }

    /// <summary>
    /// Calculates the effectiveness score for a given constraint.
    /// </summary>
    /// <param name="constraintId">The constraint to calculate effectiveness for</param>
    /// <returns>Result containing the effectiveness score or error</returns>
    public async Task<Result<BasicEffectivenessScore, string>> CalculateEffectiveness(string constraintId)
    {
        var validationResult = ValidateTrackerState();
        if (validationResult.IsError)
        {
            return Result<BasicEffectivenessScore, string>.Failure(validationResult.Error);
        }

        if (string.IsNullOrWhiteSpace(constraintId))
        {
            return Result<BasicEffectivenessScore, string>.Failure("Constraint ID cannot be null or empty");
        }

        await Task.CompletedTask;

        var ratings = _ratingStore.GetRatingsForConstraint(constraintId);
        if (ratings.Count == 0)
        {
            return Result<BasicEffectivenessScore, string>.Success(
                BasicEffectivenessScore.NoRatings(constraintId));
        }

        var effectivenessScore = CreateEffectivenessScore(constraintId, ratings);
        return Result<BasicEffectivenessScore, string>.Success(effectivenessScore);
    }

    /// <summary>
    /// Updates effectiveness tracking with new feedback.
    /// </summary>
    /// <param name="rating">The user rating to incorporate</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result<Unit, string>> UpdateWithFeedback(SimpleUserRating rating)
    {
        var validationResult = ValidateTrackerState();
        if (validationResult.IsError)
        {
            return Result<Unit, string>.Failure(validationResult.Error);
        }

        if (rating == null)
        {
            return Result<Unit, string>.Failure("Rating cannot be null");
        }

        await Task.CompletedTask;

        _ratingStore.AddRating(rating);
        return Result<Unit, string>.Success(Unit.Value);
    }

    /// <summary>
    /// Gets the top constraints ordered by effectiveness score.
    /// </summary>
    /// <param name="count">Maximum number of constraints to return</param>
    /// <returns>Result containing list of top effectiveness scores</returns>
    public async Task<Result<IReadOnlyList<BasicEffectivenessScore>, string>> GetTopConstraints(int count)
    {
        var validationResult = ValidateTrackerState();
        if (validationResult.IsError)
        {
            return Result<IReadOnlyList<BasicEffectivenessScore>, string>.Failure(validationResult.Error);
        }

        if (count <= 0)
        {
            return Result<IReadOnlyList<BasicEffectivenessScore>, string>.Failure("Count must be greater than zero");
        }

        await Task.CompletedTask;

        var effectivenessScores = new List<BasicEffectivenessScore>();
        var constraintIds = _ratingStore.GetAllConstraintIds();

        foreach (var constraintId in constraintIds)
        {
            var ratings = _ratingStore.GetRatingsForConstraint(constraintId);
            var score = CreateEffectivenessScore(constraintId, ratings);
            effectivenessScores.Add(score);
        }

        var topScores = effectivenessScores
            .OrderByDescending(s => s.EffectivenessScore)
            .Take(count)
            .ToList();

        return Result<IReadOnlyList<BasicEffectivenessScore>, string>.Success(topScores);
    }

    /// <summary>
    /// Disposes of the tracker and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _ratingStore.Clear();
            _disposed = true;
        }
    }

    private Result<Unit, string> ValidateTrackerState()
    {
        return _disposed
            ? Result<Unit, string>.Failure("Tracker has been disposed")
            : Result<Unit, string>.Success(Unit.Value);
    }

    private static BasicEffectivenessScore CreateEffectivenessScore(string constraintId, IEnumerable<SimpleUserRating> ratings)
    {
        var positiveCount = ratings.Count(r => r.IsPositive);
        var negativeCount = ratings.Count(r => r.IsNegative);
        var neutralCount = ratings.Count(r => r.IsNeutral);

        return BasicEffectivenessScore.FromRatingCounts(
            constraintId,
            positiveCount,
            negativeCount,
            neutralCount);
    }
}
