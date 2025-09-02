using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Application.Feedback;
using ConstraintMcpServer.Domain.Feedback;

namespace ConstraintMcpServer.Tests.TestDoubles;

/// <summary>
/// Test double implementation of rating storage for unit tests.
/// Provides predictable behavior for testing scenarios.
/// </summary>
internal sealed class InMemoryRatingStore : IRatingStore
{
    private readonly Dictionary<string, List<SimpleUserRating>> _ratingsByConstraint;

    public InMemoryRatingStore()
    {
        _ratingsByConstraint = new Dictionary<string, List<SimpleUserRating>>();
    }

    /// <summary>
    /// Retrieves all ratings for a specific constraint.
    /// </summary>
    /// <param name="constraintId">The constraint identifier</param>
    /// <returns>Collection of ratings for the constraint</returns>
    public IReadOnlyList<SimpleUserRating> GetRatingsForConstraint(string constraintId)
    {
        if (string.IsNullOrWhiteSpace(constraintId))
        {
            return Array.Empty<SimpleUserRating>();
        }

        return _ratingsByConstraint.TryGetValue(constraintId, out var ratings)
            ? ratings.OrderByDescending(r => r.Timestamp).ToList().AsReadOnly()
            : Array.Empty<SimpleUserRating>();
    }

    /// <summary>
    /// Retrieves all constraint identifiers that have ratings.
    /// </summary>
    /// <returns>Collection of constraint identifiers</returns>
    public IReadOnlyList<string> GetAllConstraintIds()
    {
        return _ratingsByConstraint.Keys.OrderBy(k => k).ToList().AsReadOnly();
    }

    /// <summary>
    /// Adds a new rating to the store.
    /// </summary>
    /// <param name="rating">The rating to add</param>
    public void AddRating(SimpleUserRating rating)
    {
        if (rating == null)
        {
            throw new ArgumentNullException(nameof(rating));
        }

        if (!_ratingsByConstraint.TryGetValue(rating.ConstraintId, out var ratings))
        {
            ratings = new List<SimpleUserRating>();
            _ratingsByConstraint[rating.ConstraintId] = ratings;
        }

        ratings.Add(rating);
    }

    /// <summary>
    /// Clears all stored ratings.
    /// </summary>
    public void Clear()
    {
        _ratingsByConstraint.Clear();
    }
}
