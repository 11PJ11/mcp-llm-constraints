using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Represents a library-based constraint system that manages atomic and composite constraints.
/// Refactored to use Single Responsibility Principle with extracted helper classes.
/// </summary>
public sealed class ConstraintLibrary
{
    private readonly ConstraintLibraryStorage _storage = new();

    /// <summary>
    /// Gets the version of the constraint library.
    /// </summary>
    public string? Version { get; }

    /// <summary>
    /// Gets the description of the constraint library.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the atomic constraints in this library.
    /// </summary>
    public IReadOnlyList<AtomicConstraint> AtomicConstraints => _storage.AtomicConstraints;

    /// <summary>
    /// Gets the composite constraints in this library.
    /// </summary>
    public IReadOnlyList<CompositeConstraint> CompositeConstraints => _storage.CompositeConstraints;

    /// <summary>
    /// Gets the total number of constraints in this library.
    /// </summary>
    public int TotalConstraints => _storage.TotalConstraints;

    /// <summary>
    /// Initializes a new instance of ConstraintLibrary.
    /// </summary>
    /// <param name="version">Optional version identifier</param>
    /// <param name="description">Optional library description</param>
    public ConstraintLibrary(string? version = null, string? description = null)
    {
        Version = version;
        Description = description;
    }

    /// <summary>
    /// Adds an atomic constraint to the library.
    /// </summary>
    /// <param name="constraint">The atomic constraint to add</param>
    /// <exception cref="ArgumentNullException">Thrown when constraint is null</exception>
    /// <exception cref="DuplicateConstraintIdException">Thrown when constraint ID already exists</exception>
    public void AddAtomicConstraint(AtomicConstraint constraint)
    {
        _storage.AddAtomicConstraint(constraint);
    }

    /// <summary>
    /// Adds a composite constraint to the library.
    /// </summary>
    /// <param name="constraint">The composite constraint to add</param>
    /// <exception cref="ArgumentNullException">Thrown when constraint is null</exception>
    /// <exception cref="DuplicateConstraintIdException">Thrown when constraint ID already exists</exception>
    /// <exception cref="ConstraintReferenceValidationException">Thrown when constraint references are invalid</exception>
    public void AddCompositeConstraint(CompositeConstraint constraint)
    {
        // Validate that all component references exist in the library
        ConstraintLibraryValidator.ValidateConstraintReferences(_storage, constraint);

        _storage.AddCompositeConstraint(constraint);
    }

    /// <summary>
    /// Checks if a constraint with the given ID exists in the library.
    /// </summary>
    /// <param name="id">The constraint ID to check</param>
    /// <returns>True if the constraint exists</returns>
    public bool ContainsConstraint(ConstraintId id)
    {
        return _storage.ContainsConstraint(id);
    }

    /// <summary>
    /// Gets a constraint by its ID.
    /// </summary>
    /// <param name="id">The constraint ID</param>
    /// <returns>The constraint</returns>
    /// <exception cref="ConstraintNotFoundException">Thrown when constraint is not found</exception>
    public IConstraint GetConstraint(ConstraintId id)
    {
        return ConstraintLibraryQueryEngine.GetConstraint(_storage, id);
    }

    /// <summary>
    /// Tries to get a constraint by its ID.
    /// </summary>
    /// <param name="id">The constraint ID</param>
    /// <param name="constraint">The found constraint, or null if not found</param>
    /// <returns>True if constraint was found</returns>
    public bool TryGetConstraint(ConstraintId id, out IConstraint? constraint)
    {
        return _storage.TryGetConstraint(id, out constraint);
    }

    /// <summary>
    /// Gets constraints within a priority range.
    /// </summary>
    /// <param name="minPriority">Minimum priority (inclusive)</param>
    /// <param name="maxPriority">Maximum priority (inclusive)</param>
    /// <returns>Constraints within the priority range</returns>
    public IEnumerable<IConstraint> GetConstraintsByPriority(double minPriority, double maxPriority)
    {
        return ConstraintLibraryQueryEngine.GetConstraintsByPriority(_storage, minPriority, maxPriority);
    }

    /// <summary>
    /// Gets constraints that contain a specific keyword in their triggers.
    /// </summary>
    /// <param name="keyword">The keyword to search for</param>
    /// <returns>Constraints matching the keyword</returns>
    public IEnumerable<IConstraint> GetConstraintsByKeyword(string keyword)
    {
        return ConstraintLibraryQueryEngine.GetConstraintsByKeyword(_storage, keyword);
    }

    /// <summary>
    /// Removes a constraint from the library.
    /// </summary>
    /// <param name="id">The constraint ID to remove</param>
    /// <returns>True if constraint was removed</returns>
    /// <exception cref="ConstraintInUseException">Thrown when constraint is referenced by other constraints</exception>
    public bool RemoveConstraint(ConstraintId id)
    {
        // Validate constraint can be removed safely
        ConstraintLibraryValidator.ValidateConstraintRemoval(_storage, id);

        return _storage.RemoveConstraint(id);
    }

    /// <summary>
    /// Gets all constraints that reference the given constraint ID.
    /// </summary>
    /// <param name="id">The constraint ID being referenced</param>
    /// <returns>IDs of constraints that reference the given constraint</returns>
    public IEnumerable<ConstraintId> GetReferencesToConstraint(ConstraintId id)
    {
        return ConstraintLibraryQueryEngine.GetReferencesToConstraint(_storage, id);
    }

    /// <summary>
    /// Gets statistics about this library.
    /// </summary>
    /// <returns>Library statistics</returns>
    public LibraryStatistics GetLibraryStatistics()
    {
        return ConstraintLibraryStatisticsCalculator.CalculateStatistics(_storage);
    }

    /// <summary>
    /// Merges another library with this one.
    /// </summary>
    /// <param name="other">The library to merge</param>
    /// <returns>A new library containing constraints from both libraries</returns>
    /// <exception cref="ArgumentNullException">Thrown when other library is null</exception>
    /// <exception cref="DuplicateConstraintIdException">Thrown when duplicate IDs are found</exception>
    public ConstraintLibrary MergeWith(ConstraintLibrary other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var merged = new ConstraintLibrary(Version, Description);

        // Add atomic constraints from this library
        foreach (AtomicConstraint constraint in AtomicConstraints)
        {
            merged.AddAtomicConstraint(constraint);
        }

        // Add atomic constraints from other library
        foreach (AtomicConstraint constraint in other.AtomicConstraints)
        {
            merged.AddAtomicConstraint(constraint);
        }

        // Add composite constraints from this library
        foreach (CompositeConstraint constraint in CompositeConstraints)
        {
            merged.AddCompositeConstraint(constraint);
        }

        // Add composite constraints from other library
        foreach (CompositeConstraint constraint in other.CompositeConstraints)
        {
            merged.AddCompositeConstraint(constraint);
        }

        return merged;
    }

    /// <summary>
    /// Creates a deep copy of this library.
    /// </summary>
    /// <returns>A cloned library</returns>
    public ConstraintLibrary Clone()
    {
        var cloned = new ConstraintLibrary(Version, Description);

        // Add atomic constraints
        foreach (AtomicConstraint constraint in AtomicConstraints)
        {
            cloned.AddAtomicConstraint(constraint);
        }

        // Add composite constraints
        foreach (CompositeConstraint constraint in CompositeConstraints)
        {
            cloned.AddCompositeConstraint(constraint);
        }

        return cloned;
    }

}

/// <summary>
/// Statistics about a constraint library.
/// </summary>
public sealed class LibraryStatistics
{
    /// <summary>
    /// Gets the total number of constraints.
    /// </summary>
    public int TotalConstraints { get; init; }

    /// <summary>
    /// Gets the number of atomic constraints.
    /// </summary>
    public int AtomicConstraintCount { get; init; }

    /// <summary>
    /// Gets the number of composite constraints.
    /// </summary>
    public int CompositeConstraintCount { get; init; }

    /// <summary>
    /// Gets the average priority of all constraints.
    /// </summary>
    public double AveragePriority { get; init; }

    /// <summary>
    /// Gets the total number of constraint references.
    /// </summary>
    public int ReferenceCount { get; init; }
}

/// <summary>
/// Exception thrown when attempting to add a constraint with a duplicate ID.
/// </summary>
public class DuplicateConstraintIdException : Exception
{
    public ConstraintId ConstraintId { get; }

    public DuplicateConstraintIdException(ConstraintId constraintId)
        : base($"Constraint with ID '{constraintId}' already exists in library")
    {
        ConstraintId = constraintId;
    }
}
