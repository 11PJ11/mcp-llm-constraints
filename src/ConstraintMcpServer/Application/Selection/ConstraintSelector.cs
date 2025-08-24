using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Domain;

namespace ConstraintMcpServer.Application.Selection;

/// <summary>
/// Selects constraints based on priority and phase filtering.
/// Core logic for intelligent constraint selection during tool calls.
/// </summary>
public sealed class ConstraintSelector
{
    /// <summary>
    /// Selects the top-K constraints by priority, filtered by the specified phase.
    /// </summary>
    /// <param name="constraints">Available constraints to select from.</param>
    /// <param name="phase">Current development phase for filtering.</param>
    /// <param name="topK">Maximum number of constraints to return.</param>
    /// <returns>Selected constraints sorted by priority (highest first).</returns>
    public IReadOnlyList<Constraint> SelectConstraints(
        IEnumerable<Constraint> constraints,
        Phase phase,
        int topK)
    {
        return constraints
            .Where(constraint => constraint.AppliesTo(phase))
            .OrderByDescending(constraint => constraint.Priority.Value)
            .Take(topK)
            .ToList()
            .AsReadOnly();
    }
}
