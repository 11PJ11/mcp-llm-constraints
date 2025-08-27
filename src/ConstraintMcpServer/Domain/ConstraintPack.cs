using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain;

/// <summary>
/// Aggregate root representing a collection of constraints loaded from configuration.
/// Provides ordering and validation for constraint sets.
/// </summary>
public sealed class ConstraintPack
{
    /// <summary>
    /// Gets the version of the constraint pack format.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the constraints in the pack, ordered by priority (highest first).
    /// </summary>
    public IReadOnlyList<Constraint> Constraints { get; }

    /// <summary>
    /// Initializes a new instance of ConstraintPack.
    /// </summary>
    /// <param name="version">The constraint pack format version.</param>
    /// <param name="constraints">The constraints to include in the pack.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ValidationException">Thrown when validation rules are violated.</exception>
    public ConstraintPack(string version, IEnumerable<Constraint> constraints)
    {
        Version = version ?? throw new ArgumentNullException(nameof(version));

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ValidationException("Constraint pack version cannot be empty or whitespace");
        }

        List<Constraint> constraintList = constraints?.ToList() ?? throw new ArgumentNullException(nameof(constraints));

        // Validate no duplicate constraint IDs
        HashSet<ConstraintId> seenIds = new();
        foreach (Constraint constraint in constraintList)
        {
            if (!seenIds.Add(constraint.Id))
            {
                throw new ValidationException($"Duplicate constraint ID found: {constraint.Id}");
            }
        }

        // Order by priority (highest first) for efficient top-K selection
        Constraints = constraintList
            .OrderByDescending(c => c.Priority.Value)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the top-K constraints with highest priority.
    /// </summary>
    /// <param name="count">The number of constraints to return.</param>
    /// <returns>The top-K constraints by priority.</returns>
    public IEnumerable<Constraint> GetTopByPriority(int count)
    {
        if (count <= 0)
        {
            throw new ArgumentException("Count must be positive", nameof(count));
        }

        return Constraints.Take(count);
    }

    /// <summary>
    /// Gets constraints that apply to the specified phase, ordered by priority.
    /// </summary>
    /// <param name="phase">The phase to filter by.</param>
    /// <returns>Constraints applicable to the phase, ordered by priority.</returns>
    public IEnumerable<Constraint> GetByPhase(Phase phase)
    {
        ArgumentNullException.ThrowIfNull(phase);

        return Constraints.Where(c => c.AppliesTo(phase));
    }

    /// <summary>
    /// Gets the top-K constraints for a specific phase.
    /// </summary>
    /// <param name="phase">The phase to filter by.</param>
    /// <param name="count">The number of constraints to return.</param>
    /// <returns>The top-K constraints for the phase.</returns>
    public IEnumerable<Constraint> GetTopByPhase(Phase phase, int count)
    {
        if (count <= 0)
        {
            throw new ArgumentException("Count must be positive", nameof(count));
        }

        return GetByPhase(phase).Take(count);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ConstraintPack v{Version} ({Constraints.Count} constraints)";
    }
}
