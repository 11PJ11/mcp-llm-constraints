using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Manages storage and retrieval of constraints in a library.
/// Extracted from ConstraintLibrary to follow Single Responsibility Principle.
/// </summary>
internal sealed class ConstraintLibraryStorage
{
    private readonly Dictionary<ConstraintId, AtomicConstraint> _atomicConstraints = new();
    private readonly Dictionary<ConstraintId, CompositeConstraint> _compositeConstraints = new();

    /// <summary>
    /// Gets the atomic constraints in the storage.
    /// </summary>
    public IReadOnlyList<AtomicConstraint> AtomicConstraints => _atomicConstraints.Values.ToList().AsReadOnly();

    /// <summary>
    /// Gets the composite constraints in the storage.
    /// </summary>
    public IReadOnlyList<CompositeConstraint> CompositeConstraints => _compositeConstraints.Values.ToList().AsReadOnly();

    /// <summary>
    /// Gets the total number of constraints in storage.
    /// </summary>
    public int TotalConstraints => _atomicConstraints.Count + _compositeConstraints.Count;

    /// <summary>
    /// Adds an atomic constraint to storage.
    /// </summary>
    /// <param name="constraint">The atomic constraint to add</param>
    /// <exception cref="ArgumentNullException">Thrown when constraint is null</exception>
    /// <exception cref="DuplicateConstraintIdException">Thrown when constraint ID already exists</exception>
    public void AddAtomicConstraint(AtomicConstraint constraint)
    {
        ArgumentNullException.ThrowIfNull(constraint);

        if (ContainsConstraint(constraint.Id))
        {
            throw new DuplicateConstraintIdException(constraint.Id);
        }

        _atomicConstraints[constraint.Id] = constraint;
    }

    /// <summary>
    /// Adds a composite constraint to storage.
    /// </summary>
    /// <param name="constraint">The composite constraint to add</param>
    /// <exception cref="ArgumentNullException">Thrown when constraint is null</exception>
    /// <exception cref="DuplicateConstraintIdException">Thrown when constraint ID already exists</exception>
    public void AddCompositeConstraint(CompositeConstraint constraint)
    {
        ArgumentNullException.ThrowIfNull(constraint);

        if (ContainsConstraint(constraint.Id))
        {
            throw new DuplicateConstraintIdException(constraint.Id);
        }

        _compositeConstraints[constraint.Id] = constraint;
    }

    /// <summary>
    /// Checks if a constraint with the given ID exists in storage.
    /// </summary>
    /// <param name="id">The constraint ID to check</param>
    /// <returns>True if the constraint exists</returns>
    public bool ContainsConstraint(ConstraintId id)
    {
        return _atomicConstraints.ContainsKey(id) || _compositeConstraints.ContainsKey(id);
    }

    /// <summary>
    /// Tries to get a constraint by its ID.
    /// </summary>
    /// <param name="id">The constraint ID</param>
    /// <param name="constraint">The found constraint, or null if not found</param>
    /// <returns>True if constraint was found</returns>
    public bool TryGetConstraint(ConstraintId id, out IConstraint? constraint)
    {
        if (_atomicConstraints.TryGetValue(id, out AtomicConstraint? atomicConstraint))
        {
            constraint = atomicConstraint;
            return true;
        }

        if (_compositeConstraints.TryGetValue(id, out CompositeConstraint? compositeConstraint))
        {
            constraint = compositeConstraint;
            return true;
        }

        constraint = null;
        return false;
    }

    /// <summary>
    /// Removes a constraint from storage.
    /// </summary>
    /// <param name="id">The constraint ID to remove</param>
    /// <returns>True if constraint was removed</returns>
    public bool RemoveConstraint(ConstraintId id)
    {
        return _atomicConstraints.Remove(id) || _compositeConstraints.Remove(id);
    }

    /// <summary>
    /// Clears all constraints from storage.
    /// </summary>
    public void Clear()
    {
        _atomicConstraints.Clear();
        _compositeConstraints.Clear();
    }
}
