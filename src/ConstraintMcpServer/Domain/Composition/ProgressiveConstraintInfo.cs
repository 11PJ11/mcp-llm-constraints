namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Represents information about a progressive refactoring constraint.
/// Contains constraint identification, refactoring level, and description.
/// </summary>
/// <param name="ConstraintId">Unique identifier for the constraint</param>
/// <param name="RefactoringLevel">The refactoring level this constraint belongs to (1-6)</param>
/// <param name="Description">Human-readable description of the constraint</param>
public sealed record ProgressiveConstraintInfo(
    string ConstraintId,
    int RefactoringLevel,
    string Description)
{
    /// <summary>
    /// Validates that the constraint information is complete and consistent.
    /// </summary>
    /// <returns>True if constraint information is valid, false otherwise</returns>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(ConstraintId) &&
               RefactoringLevel >= 1 && RefactoringLevel <= 6 &&
               !string.IsNullOrEmpty(Description);
    }

    /// <summary>
    /// Gets the category name for the refactoring level.
    /// </summary>
    /// <returns>Category name corresponding to the refactoring level</returns>
    public string GetLevelCategory()
    {
        return RefactoringLevel switch
        {
            1 => "Readability",
            2 => "Complexity",
            3 => "Responsibilities",
            4 => "Abstractions",
            5 => "Patterns",
            6 => "SOLID",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets the priority level for this refactoring level.
    /// Lower levels have higher priority and should be completed first.
    /// </summary>
    /// <returns>Priority value (1 = highest priority, 6 = lowest priority)</returns>
    public int GetPriority()
    {
        return RefactoringLevel; // Level 1 has priority 1 (highest), Level 6 has priority 6 (lowest)
    }
}
