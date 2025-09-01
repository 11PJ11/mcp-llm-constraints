using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Progressive composition strategy for user-defined systematic progression.
/// Enforces step-by-step progression through user-defined stages with configurable barrier support.
/// This strategy is methodology-agnostic and works with any user-defined progressive workflow.
/// </summary>
public sealed class ProgressiveComposition
{
    /// <summary>
    /// Gets the currently active constraint based on user-defined progression state.
    /// </summary>
    /// <param name="state">The current progressive composition state</param>
    /// <param name="userDefinedProgression">User-defined progression configuration</param>
    /// <returns>The active constraint for current stage in user-defined progression</returns>
    public ProgressiveConstraintInfo GetActiveConstraint(
        ProgressiveCompositionState state, 
        UserDefinedProgression userDefinedProgression)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(userDefinedProgression);

        if (!userDefinedProgression.Stages.ContainsKey(state.CurrentLevel))
        {
            throw new InvalidOperationException($"Invalid progression stage: {state.CurrentLevel}");
        }

        var currentStage = userDefinedProgression.Stages[state.CurrentLevel];
        return new ProgressiveConstraintInfo(
            currentStage.ConstraintId, 
            state.CurrentLevel, 
            currentStage.Description);
    }

    /// <summary>
    /// Gets constraints for the current user-defined progression stage.
    /// </summary>
    /// <param name="state">The current progressive composition state</param>
    /// <param name="userDefinedProgression">User-defined progression configuration</param>
    /// <returns>Collection of constraints applicable to current stage</returns>
    public IEnumerable<ProgressiveConstraintInfo> GetCurrentStageConstraints(
        ProgressiveCompositionState state,
        UserDefinedProgression userDefinedProgression)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(userDefinedProgression);

        yield return GetActiveConstraint(state, userDefinedProgression);
    }

    /// <summary>
    /// Completes the specified user-defined stage and advances to next stage.
    /// </summary>
    /// <param name="state">The current progressive composition state</param>
    /// <param name="completedStage">The stage that was completed</param>
    /// <param name="userDefinedProgression">User-defined progression configuration</param>
    /// <returns>Updated state with stage marked as completed</returns>
    public ProgressiveCompositionState CompleteStage(
        ProgressiveCompositionState state, 
        int completedStage,
        UserDefinedProgression userDefinedProgression)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(userDefinedProgression);

        var minStage = userDefinedProgression.Stages.Keys.Min();
        var maxStage = userDefinedProgression.Stages.Keys.Max();

        if (completedStage < minStage || completedStage > maxStage)
        {
            throw new ArgumentOutOfRangeException(nameof(completedStage), 
                $"Stage must be between {minStage} and {maxStage} for this user-defined progression");
        }

        var updatedCompletedStages = new HashSet<int>(state.CompletedLevels) { completedStage };
        var nextStage = Math.Min(completedStage + 1, maxStage);

        return state with
        {
            CurrentLevel = nextStage,
            CompletedLevels = updatedCompletedStages
        };
    }

    /// <summary>
    /// Gets user-defined barrier support information for the specified stage.
    /// </summary>
    /// <param name="state">The current progressive composition state</param>
    /// <param name="stage">The user-defined stage to check for barrier support</param>
    /// <param name="userDefinedProgression">User-defined progression configuration</param>
    /// <returns>Barrier support information with user-defined guidance if configured</returns>
    public BarrierSupportInfo GetBarrierSupport(
        ProgressiveCompositionState state, 
        int stage,
        UserDefinedProgression userDefinedProgression)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(userDefinedProgression);

        if (!userDefinedProgression.Stages.ContainsKey(stage))
        {
            return new BarrierSupportInfo(false, new List<string>());
        }

        var stageDefinition = userDefinedProgression.Stages[stage];
        
        if (stageDefinition.IsBarrierStage)
        {
            return new BarrierSupportInfo(true, stageDefinition.BarrierGuidance);
        }

        return new BarrierSupportInfo(false, new List<string>());
    }

    /// <summary>
    /// Attempts to skip to the specified user-defined stage, validating prerequisites.
    /// </summary>
    /// <param name="state">The current progressive composition state</param>
    /// <param name="targetStage">The stage to skip to</param>
    /// <param name="userDefinedProgression">User-defined progression configuration</param>
    /// <returns>Result indicating success or failure with error message</returns>
    public ProgressiveSkipResult TrySkipToStage(
        ProgressiveCompositionState state, 
        int targetStage,
        UserDefinedProgression userDefinedProgression)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(userDefinedProgression);

        if (targetStage <= state.CurrentLevel)
        {
            return ProgressiveSkipResult.Failure("Cannot skip to a stage that is current or previous");
        }

        if (HasMissingPrerequisites(state, targetStage, userDefinedProgression))
        {
            var missingStage = FindFirstMissingPrerequisite(state, targetStage, userDefinedProgression);
            return ProgressiveSkipResult.Failure($"Cannot skip to stage {targetStage}: prerequisite stage {missingStage} not completed");
        }

        if (RequiresSystematicProgression(state, targetStage, userDefinedProgression))
        {
            return ProgressiveSkipResult.Failure($"Stage skipping not allowed per user configuration: must complete prerequisite stages systematically");
        }

        return ProgressiveSkipResult.Success(targetStage);
    }

    private static bool HasMissingPrerequisites(
        ProgressiveCompositionState state, 
        int targetStage,
        UserDefinedProgression userDefinedProgression)
    {
        var orderedStages = userDefinedProgression.Stages.Keys.OrderBy(x => x);
        
        foreach (var stage in orderedStages.Where(s => s < targetStage))
        {
            if (!state.CompletedLevels.Contains(stage) && stage != state.CurrentLevel)
            {
                return true;
            }
        }
        return false;
    }

    private static int FindFirstMissingPrerequisite(
        ProgressiveCompositionState state, 
        int targetStage,
        UserDefinedProgression userDefinedProgression)
    {
        var orderedStages = userDefinedProgression.Stages.Keys.OrderBy(x => x);
        
        foreach (var stage in orderedStages.Where(s => s < targetStage))
        {
            if (!state.CompletedLevels.Contains(stage) && stage != state.CurrentLevel)
            {
                return stage;
            }
        }
        return targetStage;
    }

    private static bool RequiresSystematicProgression(
        ProgressiveCompositionState state, 
        int targetStage,
        UserDefinedProgression userDefinedProgression)
    {
        if (!userDefinedProgression.AllowStageSkipping)
        {
            return targetStage > state.CurrentLevel + 1;
        }
        
        return false;
    }

    /// <summary>
    /// Gets the complete user-defined progression path through all stages.
    /// </summary>
    /// <param name="state">The current progressive composition state</param>
    /// <param name="userDefinedProgression">User-defined progression configuration</param>
    /// <returns>Progression path information with user-defined ordered stages</returns>
    public ProgressionPathInfo GetProgressionPath(
        ProgressiveCompositionState state,
        UserDefinedProgression userDefinedProgression)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(userDefinedProgression);

        var stages = userDefinedProgression.Stages.Keys.OrderBy(x => x).ToList();
        var stageDescriptions = userDefinedProgression.Stages.ToDictionary(
            kvp => kvp.Key, 
            kvp => kvp.Value.Description);

        return new ProgressionPathInfo(stages, stageDescriptions);
    }
}
