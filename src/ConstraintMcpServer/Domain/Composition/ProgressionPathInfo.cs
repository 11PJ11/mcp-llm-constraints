namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Represents the complete progression path through refactoring levels.
/// Contains ordered levels and their descriptions for systematic refactoring guidance.
/// </summary>
/// <param name="Levels">Ordered list of refactoring levels (1-6)</param>
/// <param name="LevelDescriptions">Dictionary mapping levels to their descriptions</param>
public sealed record ProgressionPathInfo(
    IReadOnlyList<int> Levels,
    IReadOnlyDictionary<int, string> LevelDescriptions)
{
    /// <summary>
    /// Validates that the progression path is complete and consistent.
    /// </summary>
    /// <returns>True if progression path is valid, false otherwise</returns>
    public bool IsValid()
    {
        return Levels.Count == 6 &&
               Levels.All(level => level >= 1 && level <= 6) &&
               LevelDescriptions.Count == 6 &&
               LevelDescriptions.Keys.All(level => level >= 1 && level <= 6) &&
               Levels.OrderBy(x => x).SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 });
    }

    /// <summary>
    /// Gets the description for the specified refactoring level.
    /// </summary>
    /// <param name="level">The refactoring level to get description for</param>
    /// <returns>Description for the level, or empty string if not found</returns>
    public string GetLevelDescription(int level)
    {
        return LevelDescriptions.TryGetValue(level, out var description) ? description : string.Empty;
    }

    /// <summary>
    /// Gets the next level after the specified current level.
    /// </summary>
    /// <param name="currentLevel">The current refactoring level</param>
    /// <returns>Next level, or null if already at the highest level</returns>
    public int? GetNextLevel(int currentLevel)
    {
        for (int i = 0; i < Levels.Count; i++)
        {
            if (Levels[i] == currentLevel && i < Levels.Count - 1)
            {
                return Levels[i + 1];
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the previous level before the specified current level.
    /// </summary>
    /// <param name="currentLevel">The current refactoring level</param>
    /// <returns>Previous level, or null if already at the lowest level</returns>
    public int? GetPreviousLevel(int currentLevel)
    {
        for (int i = 0; i < Levels.Count; i++)
        {
            if (Levels[i] == currentLevel && i > 0)
            {
                return Levels[i - 1];
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the completion percentage for the specified level within the progression.
    /// </summary>
    /// <param name="currentLevel">The current refactoring level</param>
    /// <returns>Completion percentage from 0.0 to 1.0</returns>
    public double GetCompletionPercentage(int currentLevel)
    {
        for (int i = 0; i < Levels.Count; i++)
        {
            if (Levels[i] == currentLevel)
            {
                return (i + 1) / (double)Levels.Count;
            }
        }
        return 0.0;
    }
}
