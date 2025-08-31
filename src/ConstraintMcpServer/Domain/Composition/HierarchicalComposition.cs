namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Hierarchical composition strategy for architectural pattern workflows.
/// Enforces strict hierarchy-based ordering: Architecture → Implementation → Testing.
/// Within same hierarchy level, orders by priority (highest first).
/// </summary>
public sealed class HierarchicalComposition
{
    /// <summary>
    /// Gets constraints ordered by hierarchy level, then by priority within each level.
    /// </summary>
    /// <param name="constraints">The constraints to order hierarchically</param>
    /// <returns>Constraints ordered by hierarchy level (ascending), then priority (descending)</returns>
    public IEnumerable<HierarchicalConstraintInfo> GetConstraintsByHierarchy(
        IEnumerable<HierarchicalConstraintInfo> constraints)
    {
        if (constraints == null)
        {
            throw new ArgumentNullException(nameof(constraints));
        }

        return constraints
            .OrderBy(c => c.HierarchyLevel)     // Primary sort: hierarchy level (0 = highest priority)
            .ThenByDescending(c => c.Priority)  // Secondary sort: priority within level (highest first)
            .ToList();
    }
}
