namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Progressive composition strategy for systematic refactoring level progression.
/// Enforces level-by-level progression through 6 refactoring levels with barrier detection.
/// Prevents level skipping and provides additional support at major barrier points.
/// </summary>
public sealed class ProgressiveComposition
{
    /// <summary>
    /// Gets the currently active constraint based on progression state.
    /// </summary>
    /// <param name="state">The current progressive composition state</param>
    /// <returns>The active refactoring constraint for current level</returns>
    public ProgressiveConstraintInfo GetActiveConstraint(ProgressiveCompositionState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return state.CurrentLevel switch
        {
            1 => new ProgressiveConstraintInfo("refactor.level1.readability", 1, "Level 1: Focus on readability improvements"),
            2 => new ProgressiveConstraintInfo("refactor.level2.complexity", 2, "Level 2: Reduce complexity and duplication"),
            3 => new ProgressiveConstraintInfo("refactor.level3.responsibilities", 3, "Level 3: Reorganize responsibilities"),
            4 => new ProgressiveConstraintInfo("refactor.level4.abstractions", 4, "Level 4: Refine abstractions"),
            5 => new ProgressiveConstraintInfo("refactor.level5.patterns", 5, "Level 5: Apply design patterns"),
            6 => new ProgressiveConstraintInfo("refactor.level6.solid", 6, "Level 6: Apply SOLID principles"),
            _ => throw new InvalidOperationException($"Invalid refactoring level: {state.CurrentLevel}")
        };
    }

    /// <summary>
    /// Gets constraints for the current refactoring level.
    /// </summary>
    /// <param name="state">The current progressive composition state</param>
    /// <returns>Collection of constraints applicable to current level</returns>
    public IEnumerable<ProgressiveConstraintInfo> GetCurrentLevelConstraints(ProgressiveCompositionState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        yield return GetActiveConstraint(state);
    }

    /// <summary>
    /// Completes the specified refactoring level and advances to next level.
    /// </summary>
    /// <param name="state">The current progressive composition state</param>
    /// <param name="completedLevel">The level that was completed</param>
    /// <returns>Updated state with level marked as completed</returns>
    public ProgressiveCompositionState CompleteLevel(ProgressiveCompositionState state, int completedLevel)
    {
        ArgumentNullException.ThrowIfNull(state);

        const int MinRefactoringLevel = 1;
        const int MaxRefactoringLevel = 6;

        if (completedLevel < MinRefactoringLevel || completedLevel > MaxRefactoringLevel)
        {
            throw new ArgumentOutOfRangeException(nameof(completedLevel), "Refactoring level must be between 1 and 6");
        }

        var updatedCompletedLevels = new HashSet<int>(state.CompletedLevels) { completedLevel };
        var nextLevel = Math.Min(completedLevel + 1, MaxRefactoringLevel);

        return state with
        {
            CurrentLevel = nextLevel,
            CompletedLevels = updatedCompletedLevels
        };
    }

    /// <summary>
    /// Gets barrier support information for the specified level.
    /// </summary>
    /// <param name="state">The current progressive composition state</param>
    /// <param name="level">The refactoring level to check for barrier support</param>
    /// <returns>Barrier support information with additional guidance if needed</returns>
    public BarrierSupportInfo GetBarrierSupport(ProgressiveCompositionState state, int level)
    {
        ArgumentNullException.ThrowIfNull(state);

        const int ResponsibilitiesBarrierLevel = 3;
        const int PatternsBarrierLevel = 5;

        var isBarrierLevel = level == ResponsibilitiesBarrierLevel || level == PatternsBarrierLevel;

        if (isBarrierLevel)
        {
            var guidance = level switch
            {
                ResponsibilitiesBarrierLevel => new List<string>
                {
                    "Level 3 is a common drop-off point - take your time with class responsibilities",
                    "Focus on Single Responsibility Principle and reducing coupling",
                    "Consider pair programming or code review for this level"
                },
                PatternsBarrierLevel => new List<string>
                {
                    "Level 5 patterns require deeper architectural thinking",
                    "Start with simple patterns like Strategy or Command",
                    "Don't force patterns where they don't naturally fit"
                },
                _ => new List<string>()
            };

            return new BarrierSupportInfo(true, guidance);
        }

        return new BarrierSupportInfo(false, new List<string>());
    }

    /// <summary>
    /// Attempts to skip to the specified level, validating prerequisites.
    /// </summary>
    /// <param name="state">The current progressive composition state</param>
    /// <param name="targetLevel">The level to skip to</param>
    /// <returns>Result indicating success or failure with error message</returns>
    public ProgressiveSkipResult TrySkipToLevel(ProgressiveCompositionState state, int targetLevel)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (targetLevel <= state.CurrentLevel)
        {
            return ProgressiveSkipResult.Failure("Cannot skip to a level that is current or previous");
        }

        if (HasMissingPrerequisites(state, targetLevel))
        {
            var missingLevel = FindFirstMissingPrerequisite(state, targetLevel);
            return ProgressiveSkipResult.Failure($"Cannot skip to level {targetLevel}: prerequisite levels {missingLevel} not completed");
        }

        if (RequiresSystematicProgression(state, targetLevel))
        {
            return ProgressiveSkipResult.Failure($"Level skipping not allowed: must complete prerequisite levels systematically");
        }

        return ProgressiveSkipResult.Success(targetLevel);
    }

    private static bool HasMissingPrerequisites(ProgressiveCompositionState state, int targetLevel)
    {
        for (int level = 1; level < targetLevel; level++)
        {
            if (!state.CompletedLevels.Contains(level) && level != state.CurrentLevel)
            {
                return true;
            }
        }
        return false;
    }

    private static int FindFirstMissingPrerequisite(ProgressiveCompositionState state, int targetLevel)
    {
        for (int level = 1; level < targetLevel; level++)
        {
            if (!state.CompletedLevels.Contains(level) && level != state.CurrentLevel)
            {
                return level;
            }
        }
        return targetLevel;
    }

    private static bool RequiresSystematicProgression(ProgressiveCompositionState state, int targetLevel)
    {
        return targetLevel > state.CurrentLevel + 1;
    }

    /// <summary>
    /// Gets the complete progression path through all refactoring levels.
    /// </summary>
    /// <param name="state">The current progressive composition state</param>
    /// <returns>Progression path information with ordered levels</returns>
    public ProgressionPathInfo GetProgressionPath(ProgressiveCompositionState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var levels = CreateRefactoringLevels();
        var levelDescriptions = CreateLevelDescriptions();

        return new ProgressionPathInfo(levels, levelDescriptions);
    }

    private static List<int> CreateRefactoringLevels()
    {
        return new List<int> { 1, 2, 3, 4, 5, 6 };
    }

    private static Dictionary<int, string> CreateLevelDescriptions()
    {
        return new Dictionary<int, string>
        {
            { 1, "Readability: Comments, dead code, naming, magic strings/numbers" },
            { 2, "Complexity: Method extraction, duplication elimination" },
            { 3, "Responsibilities: Class responsibilities, coupling reduction" },
            { 4, "Abstractions: Parameter objects, value objects, abstractions" },
            { 5, "Patterns: Strategy, State, Command patterns" },
            { 6, "SOLID: Advanced architectural principles" }
        };
    }
}
