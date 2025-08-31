using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Validates constraint operations for constraint libraries.
/// Extracted from ConstraintLibrary to follow Single Responsibility Principle.
/// </summary>
internal static class ConstraintLibraryValidator
{
    /// <summary>
    /// Validates that all component references exist in the library.
    /// </summary>
    /// <param name="storage">The constraint storage to validate against</param>
    /// <param name="constraint">The composite constraint to validate</param>
    /// <exception cref="ConstraintReferenceValidationException">Thrown when constraint references are invalid</exception>
    public static void ValidateConstraintReferences(
        ConstraintLibraryStorage storage,
        CompositeConstraint constraint)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(constraint);

        var missingReferences = new List<ConstraintId>();

        foreach (ConstraintReference reference in constraint.ComponentReferences)
        {
            if (!storage.ContainsConstraint(reference.ConstraintId))
            {
                missingReferences.Add(reference.ConstraintId);
            }
        }

        if (missingReferences.Count > 0)
        {
            throw new ConstraintReferenceValidationException(missingReferences);
        }
    }

    /// <summary>
    /// Validates that a constraint can be removed safely.
    /// </summary>
    /// <param name="storage">The constraint storage to validate against</param>
    /// <param name="id">The constraint ID to check for removal</param>
    /// <exception cref="ConstraintInUseException">Thrown when constraint is referenced by other constraints</exception>
    public static void ValidateConstraintRemoval(ConstraintLibraryStorage storage, ConstraintId id)
    {
        ArgumentNullException.ThrowIfNull(storage);

        var referencingConstraints = ConstraintLibraryQueryEngine
            .GetReferencesToConstraint(storage, id)
            .ToList();

        if (referencingConstraints.Count > 0)
        {
            throw new ConstraintInUseException(id, referencingConstraints);
        }
    }
}
