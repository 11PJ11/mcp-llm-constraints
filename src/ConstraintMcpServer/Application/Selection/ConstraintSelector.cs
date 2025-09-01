using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Application.Selection;

/// <summary>
/// Selects constraints based on priority and user-defined context filtering.
/// Core logic for methodology-agnostic intelligent constraint selection during tool calls.
/// </summary>
public sealed class ConstraintSelector
{
    /// <summary>
    /// Selects the top-K constraints by priority, filtered by the specified user-defined context.
    /// </summary>
    /// <param name="constraints">Available constraints to select from.</param>
    /// <param name="context">Current workflow context for filtering.</param>
    /// <param name="topK">Maximum number of constraints to return.</param>
    /// <returns>Selected constraints sorted by priority (highest first).</returns>
    public static IReadOnlyList<Constraint> SelectConstraints(
        IEnumerable<Constraint> constraints,
        UserDefinedContext context,
        int topK)
    {
        if (constraints == null)
        {
            throw new ArgumentNullException(nameof(constraints));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (topK <= 0)
        {
            throw new ArgumentException("TopK must be positive", nameof(topK));
        }

        return constraints
            .Where(constraint => constraint.AppliesTo(context))
            .OrderByDescending(constraint => constraint.Priority.Value)
            .Take(topK)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Selects the top-K constraints by priority, filtered by the specified context category.
    /// </summary>
    /// <param name="constraints">Available constraints to select from.</param>
    /// <param name="category">Current workflow category for filtering.</param>
    /// <param name="topK">Maximum number of constraints to return.</param>
    /// <returns>Selected constraints sorted by priority (highest first).</returns>
    public static IReadOnlyList<Constraint> SelectConstraintsByCategory(
        IEnumerable<Constraint> constraints,
        string category,
        int topK)
    {
        if (constraints == null)
        {
            throw new ArgumentNullException(nameof(constraints));
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category cannot be null or empty", nameof(category));
        }

        if (topK <= 0)
        {
            throw new ArgumentException("TopK must be positive", nameof(topK));
        }

        return constraints
            .Where(constraint => constraint.AppliesToCategory(category))
            .OrderByDescending(constraint => constraint.Priority.Value)
            .Take(topK)
            .ToList()
            .AsReadOnly();
    }
}
