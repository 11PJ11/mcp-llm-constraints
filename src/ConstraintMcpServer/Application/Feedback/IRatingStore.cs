using System.Collections.Generic;
using ConstraintMcpServer.Domain.Feedback;

namespace ConstraintMcpServer.Application.Feedback;

/// <summary>
/// Abstracts rating storage operations for effectiveness tracking.
/// Follows Single Responsibility Principle by focusing only on data storage.
/// </summary>
public interface IRatingStore
{
    /// <summary>
    /// Retrieves all ratings for a specific constraint.
    /// </summary>
    /// <param name="constraintId">The constraint identifier</param>
    /// <returns>Collection of ratings for the constraint</returns>
    IReadOnlyList<SimpleUserRating> GetRatingsForConstraint(string constraintId);

    /// <summary>
    /// Retrieves all constraint identifiers that have ratings.
    /// </summary>
    /// <returns>Collection of constraint identifiers</returns>
    IReadOnlyList<string> GetAllConstraintIds();

    /// <summary>
    /// Adds a new rating to the store.
    /// </summary>
    /// <param name="rating">The rating to add</param>
    void AddRating(SimpleUserRating rating);

    /// <summary>
    /// Clears all stored ratings.
    /// </summary>
    void Clear();
}
