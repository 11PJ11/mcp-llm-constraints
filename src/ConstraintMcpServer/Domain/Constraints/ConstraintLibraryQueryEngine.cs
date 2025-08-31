using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Provides query operations for constraint libraries.
/// Extracted from ConstraintLibrary to follow Single Responsibility Principle.
/// </summary>
internal static class ConstraintLibraryQueryEngine
{
    /// <summary>
    /// Gets constraints within a priority range.
    /// </summary>
    /// <param name="storage">The constraint storage to query</param>
    /// <param name="minPriority">Minimum priority (inclusive)</param>
    /// <param name="maxPriority">Maximum priority (inclusive)</param>
    /// <returns>Constraints within the priority range</returns>
    public static IEnumerable<IConstraint> GetConstraintsByPriority(
        ConstraintLibraryStorage storage,
        double minPriority,
        double maxPriority)
    {
        ArgumentNullException.ThrowIfNull(storage);

        return storage.AtomicConstraints
            .Where(c => c.Priority >= minPriority && c.Priority <= maxPriority)
            .Cast<IConstraint>()
            .Concat(storage.CompositeConstraints
                .Where(c => c.Priority >= minPriority && c.Priority <= maxPriority)
                .Cast<IConstraint>())
            .OrderByDescending(c => c.Priority);
    }

    /// <summary>
    /// Gets constraints that contain a specific keyword in their triggers.
    /// </summary>
    /// <param name="storage">The constraint storage to query</param>
    /// <param name="keyword">The keyword to search for</param>
    /// <returns>Constraints matching the keyword</returns>
    public static IEnumerable<IConstraint> GetConstraintsByKeyword(
        ConstraintLibraryStorage storage,
        string keyword)
    {
        ArgumentNullException.ThrowIfNull(storage);

        if (string.IsNullOrWhiteSpace(keyword))
        {
            yield break;
        }

        string keywordLower = keyword.ToLowerInvariant();

        foreach (AtomicConstraint constraint in storage.AtomicConstraints)
        {
            if (constraint.Triggers.Keywords.Any(k => k.Contains(keywordLower, StringComparison.InvariantCultureIgnoreCase)))
            {
                yield return constraint;
            }
        }

        foreach (CompositeConstraint constraint in storage.CompositeConstraints)
        {
            if (constraint.Triggers.Keywords.Any(k => k.Contains(keywordLower, StringComparison.InvariantCultureIgnoreCase)))
            {
                yield return constraint;
            }
        }
    }

    /// <summary>
    /// Gets a constraint by its ID.
    /// </summary>
    /// <param name="storage">The constraint storage to query</param>
    /// <param name="id">The constraint ID</param>
    /// <returns>The constraint</returns>
    /// <exception cref="ConstraintNotFoundException">Thrown when constraint is not found</exception>
    public static IConstraint GetConstraint(ConstraintLibraryStorage storage, ConstraintId id)
    {
        ArgumentNullException.ThrowIfNull(storage);

        if (storage.TryGetConstraint(id, out IConstraint? constraint))
        {
            return constraint!; // Non-null because TryGetConstraint returned true
        }

        throw new ConstraintNotFoundException(id);
    }

    /// <summary>
    /// Gets all constraints that reference the given constraint ID.
    /// </summary>
    /// <param name="storage">The constraint storage to query</param>
    /// <param name="id">The constraint ID being referenced</param>
    /// <returns>IDs of constraints that reference the given constraint</returns>
    public static IEnumerable<ConstraintId> GetReferencesToConstraint(
        ConstraintLibraryStorage storage,
        ConstraintId id)
    {
        ArgumentNullException.ThrowIfNull(storage);

        foreach (CompositeConstraint composite in storage.CompositeConstraints)
        {
            if (composite.ComponentReferences.Any(r => r.ConstraintId.Equals(id)))
            {
                yield return composite.Id;
            }
        }
    }
}
