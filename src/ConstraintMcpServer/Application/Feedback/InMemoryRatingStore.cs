using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Domain.Feedback;

namespace ConstraintMcpServer.Application.Feedback;

/// <summary>
/// In-memory implementation of rating storage.
/// Provides fast access for development and testing scenarios.
/// </summary>
internal sealed class InMemoryRatingStore : IRatingStore
{
    private readonly Dictionary<string, List<SimpleUserRating>> _ratingsByConstraint;

    public InMemoryRatingStore()
    {
        _ratingsByConstraint = new Dictionary<string, List<SimpleUserRating>>();
    }

    public IReadOnlyList<SimpleUserRating> GetRatingsForConstraint(string constraintId)
    {
        return _ratingsByConstraint.TryGetValue(constraintId, out var ratings) 
            ? ratings.AsReadOnly() 
            : new List<SimpleUserRating>().AsReadOnly();
    }

    public IReadOnlyList<string> GetAllConstraintIds()
    {
        return _ratingsByConstraint.Keys.ToList().AsReadOnly();
    }

    public void AddRating(SimpleUserRating rating)
    {
        if (!_ratingsByConstraint.TryGetValue(rating.ConstraintId, out var ratings))
        {
            ratings = new List<SimpleUserRating>();
            _ratingsByConstraint[rating.ConstraintId] = ratings;
        }

        ratings.Add(rating);
    }

    public void Clear()
    {
        _ratingsByConstraint.Clear();
    }
}