namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Represents constraint information for hierarchical composition.
/// Contains hierarchy level and priority for hierarchical ordering.
/// </summary>
public sealed record HierarchicalConstraintInfo
{
    /// <summary>
    /// Gets the unique constraint identifier.
    /// </summary>
    public string ConstraintId { get; init; }

    /// <summary>
    /// Gets the hierarchy level (0 = highest priority, increasing numbers = lower priority).
    /// Architecture constraints typically use level 0, implementation level 1, testing level 2.
    /// </summary>
    public int HierarchyLevel { get; init; }

    /// <summary>
    /// Gets the priority within the same hierarchy level (0.0 to 1.0, higher is more important).
    /// Used for ordering constraints within the same hierarchy level.
    /// </summary>
    public double Priority { get; init; }

    /// <summary>
    /// Initializes a new instance of HierarchicalConstraintInfo.
    /// </summary>
    /// <param name="constraintId">The unique constraint identifier</param>
    /// <param name="hierarchyLevel">The hierarchy level (0 = highest)</param>
    /// <param name="priority">The priority within hierarchy level (0.0 to 1.0)</param>
    public HierarchicalConstraintInfo(string constraintId, int hierarchyLevel, double priority)
    {
        ConstraintId = constraintId ?? throw new ArgumentNullException(nameof(constraintId));
        HierarchyLevel = hierarchyLevel;
        Priority = priority;

        if (hierarchyLevel < 0)
        {
            throw new ArgumentException("Hierarchy level must be non-negative", nameof(hierarchyLevel));
        }

        if (priority < 0.0 || priority > 1.0)
        {
            throw new ArgumentException("Priority must be between 0.0 and 1.0", nameof(priority));
        }
    }
}
