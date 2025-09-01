using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Hierarchical composition strategy for user-defined hierarchical workflows.
/// Enforces strict hierarchy-based ordering based on user-defined hierarchy levels.
/// Within same hierarchy level, orders by user-defined priority (highest first).
/// This strategy is methodology-agnostic and works with any user-defined hierarchical structure.
/// </summary>
public sealed class HierarchicalComposition
{
    /// <summary>
    /// Gets constraints ordered by user-defined hierarchy level, then by priority within each level.
    /// </summary>
    /// <param name="constraints">The constraints to order hierarchically</param>
    /// <param name="userDefinedHierarchy">User-defined hierarchy configuration</param>
    /// <returns>Constraints ordered by user-defined hierarchy level (ascending), then priority (descending)</returns>
    public IEnumerable<UserDefinedHierarchicalConstraintInfo> GetConstraintsByHierarchy(
        IEnumerable<UserDefinedHierarchicalConstraintInfo> constraints,
        UserDefinedHierarchy userDefinedHierarchy)
    {
        ArgumentNullException.ThrowIfNull(constraints);
        ArgumentNullException.ThrowIfNull(userDefinedHierarchy);

        // Validate that all constraints belong to defined hierarchy levels
        foreach (var constraint in constraints)
        {
            if (!userDefinedHierarchy.HasLevel(constraint.HierarchyLevel))
            {
                throw new InvalidOperationException(
                    $"Constraint hierarchy level {constraint.HierarchyLevel} is not defined in the user hierarchy configuration");
            }
        }

        return constraints
            .OrderBy(c => c.HierarchyLevel)     // Primary sort: user-defined hierarchy level (lower = higher priority)
            .ThenByDescending(c => c.Priority)  // Secondary sort: user-defined priority within level (highest first)
            .ToList();
    }

    /// <summary>
    /// Gets constraints for a specific user-defined hierarchy level.
    /// </summary>
    /// <param name="constraints">The constraints to filter.</param>
    /// <param name="hierarchyLevel">The user-defined hierarchy level to filter by.</param>
    /// <param name="userDefinedHierarchy">User-defined hierarchy configuration.</param>
    /// <returns>Constraints for the specified hierarchy level ordered by priority.</returns>
    public IEnumerable<UserDefinedHierarchicalConstraintInfo> GetConstraintsForLevel(
        IEnumerable<UserDefinedHierarchicalConstraintInfo> constraints,
        int hierarchyLevel,
        UserDefinedHierarchy userDefinedHierarchy)
    {
        ArgumentNullException.ThrowIfNull(constraints);
        ArgumentNullException.ThrowIfNull(userDefinedHierarchy);

        if (!userDefinedHierarchy.HasLevel(hierarchyLevel))
        {
            throw new ArgumentException($"Hierarchy level {hierarchyLevel} is not defined in user configuration", nameof(hierarchyLevel));
        }

        return constraints
            .Where(c => c.HierarchyLevel == hierarchyLevel)
            .OrderByDescending(c => c.Priority)
            .ToList();
    }

    /// <summary>
    /// Gets the next hierarchy level that should be activated based on user-defined progression rules.
    /// </summary>
    /// <param name="currentLevel">The current hierarchy level.</param>
    /// <param name="completedLevels">Set of completed hierarchy levels.</param>
    /// <param name="userDefinedHierarchy">User-defined hierarchy configuration.</param>
    /// <returns>The next hierarchy level to activate, or null if hierarchy is complete.</returns>
    public int? GetNextHierarchyLevel(
        int? currentLevel,
        IReadOnlySet<int> completedLevels,
        UserDefinedHierarchy userDefinedHierarchy)
    {
        ArgumentNullException.ThrowIfNull(completedLevels);
        ArgumentNullException.ThrowIfNull(userDefinedHierarchy);

        var availableLevels = userDefinedHierarchy.GetOrderedLevels()
            .Where(level => !completedLevels.Contains(level))
            .ToList();

        if (!availableLevels.Any())
        {
            return null; // All levels completed
        }

        // Return the first (lowest priority number) available level
        return availableLevels.First();
    }
}
