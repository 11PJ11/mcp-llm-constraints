using ConstraintMcpServer.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Sequential composition strategy for user-defined linear workflows.
/// Enforces strict ordering and transitions based on user-defined contexts and evaluation criteria.
/// This strategy is methodology-agnostic and works with any user-defined sequential workflow.
/// </summary>
public sealed class SequentialComposition
{
    /// <summary>
    /// Gets the next constraint to activate based on user-defined sequence and current context.
    /// </summary>
    /// <param name="userDefinedSequence">The user-defined sequence of constraint references.</param>
    /// <param name="currentContext">The current user-defined context.</param>
    /// <param name="completedConstraints">Set of constraint IDs already completed in this sequence.</param>
    /// <returns>Result containing next constraint ID and user-defined activation guidance.</returns>
    public SequentialCompositionResult GetNextConstraintId(
        IReadOnlyList<string> userDefinedSequence,
        UserDefinedContext currentContext,
        IReadOnlySet<string> completedConstraints)
    {
        if (userDefinedSequence == null)
        {
            return SequentialCompositionResult.Failure("User-defined sequence cannot be null");
        }

        if (currentContext == null)
        {
            return SequentialCompositionResult.Failure("User-defined context cannot be null");
        }

        if (completedConstraints == null)
        {
            return SequentialCompositionResult.Failure("Completed constraints set cannot be null");
        }

        // Find the next constraint in user-defined sequence that hasn't been completed
        var nextConstraint = userDefinedSequence.FirstOrDefault(constraintId =>
            !completedConstraints.Contains(constraintId));

        if (nextConstraint == null)
        {
            return SequentialCompositionResult.Success(
                string.Empty,
                "Sequential workflow complete - all user-defined constraints have been activated");
        }

        // Calculate position in sequence for context-aware guidance
        var completedCount = userDefinedSequence.Count(constraintId =>
            completedConstraints.Contains(constraintId));
        var totalCount = userDefinedSequence.Count;
        var position = completedCount + 1;

        var guidance = GenerateUserContextGuidance(nextConstraint, currentContext, position, totalCount);

        return SequentialCompositionResult.Success(nextConstraint, guidance);
    }

    /// <summary>
    /// Checks if a user-defined sequential workflow is complete.
    /// </summary>
    /// <param name="userDefinedSequence">The user-defined sequence of constraints.</param>
    /// <param name="completedConstraints">Set of completed constraint IDs.</param>
    /// <returns>True if all constraints in the sequence have been completed.</returns>
    public bool IsSequenceComplete(
        IReadOnlyList<string> userDefinedSequence,
        IReadOnlySet<string> completedConstraints)
    {
        return userDefinedSequence?.All(constraintId =>
            completedConstraints?.Contains(constraintId) == true) == true;
    }

    /// <summary>
    /// Gets the progress of a user-defined sequential workflow.
    /// </summary>
    /// <param name="userDefinedSequence">The user-defined sequence of constraints.</param>
    /// <param name="completedConstraints">Set of completed constraint IDs.</param>
    /// <returns>Progress information for the sequence.</returns>
    public SequentialProgressInfo GetSequenceProgress(
        IReadOnlyList<string> userDefinedSequence,
        IReadOnlySet<string> completedConstraints)
    {
        if (userDefinedSequence == null || completedConstraints == null)
        {
            return new SequentialProgressInfo(0, 0, 0.0);
        }

        var completedCount = userDefinedSequence.Count(constraintId =>
            completedConstraints.Contains(constraintId));
        var totalCount = userDefinedSequence.Count;
        var progressPercentage = totalCount > 0 ? (double)completedCount / totalCount : 0.0;

        return new SequentialProgressInfo(completedCount, totalCount, progressPercentage);
    }

    private static string GenerateUserContextGuidance(
        string nextConstraintId,
        UserDefinedContext currentContext,
        int position,
        int totalCount)
    {
        return $"Next in sequence: {nextConstraintId} " +
               $"(Step {position} of {totalCount} in user-defined workflow, " +
               $"Context: {currentContext.Category}={currentContext.Value})";
    }
}

/// <summary>
/// Represents progress information for a user-defined sequential workflow.
/// </summary>
/// <param name="CompletedCount">Number of constraints completed in the sequence.</param>
/// <param name="TotalCount">Total number of constraints in the sequence.</param>
/// <param name="ProgressPercentage">Progress as a percentage (0.0 to 1.0).</param>
public record SequentialProgressInfo(int CompletedCount, int TotalCount, double ProgressPercentage);
