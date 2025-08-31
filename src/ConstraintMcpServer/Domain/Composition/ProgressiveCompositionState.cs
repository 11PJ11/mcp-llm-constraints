namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Represents the current state of progressive composition workflow.
/// Tracks current refactoring level, completed levels, and context information.
/// </summary>
/// <param name="CurrentLevel">The current refactoring level (1-6)</param>
/// <param name="CompletedLevels">Set of refactoring levels that have been completed</param>
/// <param name="RefactoringContext">The current refactoring context identifier</param>
/// <param name="BarrierDetectionEnabled">Whether barrier detection and support is enabled</param>
/// <param name="TestsPassing">Whether tests are currently passing</param>
/// <param name="ReadyForRefactoring">Whether the code is ready for refactoring</param>
public sealed record ProgressiveCompositionState
{
    public int CurrentLevel { get; init; } = 1;
    public HashSet<int> CompletedLevels { get; init; } = new();
    public string RefactoringContext { get; init; } = string.Empty;
    public bool BarrierDetectionEnabled { get; init; } = true;
    public bool TestsPassing { get; init; }
    public bool ReadyForRefactoring { get; init; }

    /// <summary>
    /// Validates that the state is consistent and within expected bounds.
    /// </summary>
    /// <returns>True if state is valid, false otherwise</returns>
    public bool IsValid()
    {
        return CurrentLevel >= 1 && CurrentLevel <= 6 &&
               CompletedLevels.All(level => level >= 1 && level <= 6) &&
               !string.IsNullOrEmpty(RefactoringContext);
    }

    /// <summary>
    /// Gets the progress percentage through all refactoring levels.
    /// </summary>
    /// <returns>Progress percentage from 0.0 to 1.0</returns>
    public double GetProgressPercentage()
    {
        return CompletedLevels.Count / 6.0;
    }

    /// <summary>
    /// Checks if the specified level has been completed.
    /// </summary>
    /// <param name="level">The refactoring level to check</param>
    /// <returns>True if level is completed, false otherwise</returns>
    public bool IsLevelCompleted(int level)
    {
        return CompletedLevels.Contains(level);
    }
}
