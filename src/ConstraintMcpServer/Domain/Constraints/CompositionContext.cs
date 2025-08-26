using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Represents the current state and context for composite constraint evaluation.
/// Tracks progression through methodology workflows and component activation.
/// </summary>
public sealed class CompositionContext
{
    /// <summary>
    /// Gets the trigger context that initiated this composition.
    /// </summary>
    public TriggerContext TriggerContext { get; init; } = null!;

    /// <summary>
    /// Gets the current state of the composition.
    /// </summary>
    public CompositionState CurrentState { get; init; } = CompositionState.Inactive;

    /// <summary>
    /// Gets the current sequence step for sequential compositions.
    /// </summary>
    public int SequenceStep { get; init; } = 1;

    /// <summary>
    /// Gets the current hierarchy level for hierarchical compositions.
    /// </summary>
    public int HierarchyLevel { get; init; } = 1;

    /// <summary>
    /// Gets the current progression level for progressive compositions.
    /// </summary>
    public int ProgressionLevel { get; init; } = 1;

    /// <summary>
    /// Gets the set of completed component IDs.
    /// </summary>
    public IReadOnlySet<string> CompletedComponents { get; init; } = new HashSet<string>();

    /// <summary>
    /// Gets additional metadata for this composition context.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } =
        new Dictionary<string, object>().AsReadOnly();

    /// <summary>
    /// Gets the timestamp when this context was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates a new composition context.
    /// </summary>
    public CompositionContext()
    {
        // Default constructor for property initialization
    }

    /// <summary>
    /// Creates a new composition context with the specified trigger context.
    /// </summary>
    /// <param name="triggerContext">The trigger context that initiated this composition</param>
    public CompositionContext(TriggerContext triggerContext)
    {
        TriggerContext = triggerContext ?? throw new ArgumentNullException(nameof(triggerContext));
    }

    /// <summary>
    /// Creates a copy of this context with updated sequence step.
    /// </summary>
    /// <param name="newStep">The new sequence step</param>
    /// <returns>New composition context with updated step</returns>
    public CompositionContext WithSequenceStep(int newStep)
    {
        return new CompositionContext
        {
            TriggerContext = TriggerContext,
            CurrentState = CurrentState,
            SequenceStep = newStep,
            HierarchyLevel = HierarchyLevel,
            ProgressionLevel = ProgressionLevel,
            CompletedComponents = CompletedComponents,
            Metadata = Metadata,
            Timestamp = Timestamp
        };
    }

    /// <summary>
    /// Creates a copy of this context with updated hierarchy level.
    /// </summary>
    /// <param name="newLevel">The new hierarchy level</param>
    /// <returns>New composition context with updated level</returns>
    public CompositionContext WithHierarchyLevel(int newLevel)
    {
        return new CompositionContext
        {
            TriggerContext = TriggerContext,
            CurrentState = CurrentState,
            SequenceStep = SequenceStep,
            HierarchyLevel = newLevel,
            ProgressionLevel = ProgressionLevel,
            CompletedComponents = CompletedComponents,
            Metadata = Metadata,
            Timestamp = Timestamp
        };
    }

    /// <summary>
    /// Creates a copy of this context with updated progression level.
    /// </summary>
    /// <param name="newLevel">The new progression level</param>
    /// <returns>New composition context with updated progression level</returns>
    public CompositionContext WithProgressionLevel(int newLevel)
    {
        return new CompositionContext
        {
            TriggerContext = TriggerContext,
            CurrentState = CurrentState,
            SequenceStep = SequenceStep,
            HierarchyLevel = HierarchyLevel,
            ProgressionLevel = newLevel,
            CompletedComponents = CompletedComponents,
            Metadata = Metadata,
            Timestamp = Timestamp
        };
    }

    /// <summary>
    /// Creates a copy of this context with updated state.
    /// </summary>
    /// <param name="newState">The new composition state</param>
    /// <returns>New composition context with updated state</returns>
    public CompositionContext WithState(CompositionState newState)
    {
        return new CompositionContext
        {
            TriggerContext = TriggerContext,
            CurrentState = newState,
            SequenceStep = SequenceStep,
            HierarchyLevel = HierarchyLevel,
            ProgressionLevel = ProgressionLevel,
            CompletedComponents = CompletedComponents,
            Metadata = Metadata,
            Timestamp = Timestamp
        };
    }

    /// <summary>
    /// Creates a copy of this context with an additional completed component.
    /// </summary>
    /// <param name="componentId">The ID of the completed component</param>
    /// <returns>New composition context with updated completed components</returns>
    public CompositionContext WithCompletedComponent(string componentId)
    {
        var newCompleted = new HashSet<string>(CompletedComponents) { componentId };

        return new CompositionContext
        {
            TriggerContext = TriggerContext,
            CurrentState = CurrentState,
            SequenceStep = SequenceStep,
            HierarchyLevel = HierarchyLevel,
            ProgressionLevel = ProgressionLevel,
            CompletedComponents = newCompleted,
            Metadata = Metadata,
            Timestamp = Timestamp
        };
    }

    /// <summary>
    /// Checks if a component has been completed.
    /// </summary>
    /// <param name="componentId">The component ID to check</param>
    /// <returns>True if the component has been completed</returns>
    public bool IsComponentCompleted(string componentId)
    {
        return CompletedComponents.Contains(componentId);
    }

    /// <summary>
    /// Returns a string representation of the composition context.
    /// </summary>
    public override string ToString()
    {
        var parts = new List<string>
        {
            $"State: {CurrentState}",
            $"Step: {SequenceStep}",
            $"Level: {HierarchyLevel}",
            $"Progression: {ProgressionLevel}",
            $"Completed: {CompletedComponents.Count}"
        };

        return $"CompositionContext({string.Join(", ", parts)})";
    }
}

/// <summary>
/// Represents the possible states of a composite constraint composition.
/// </summary>
public enum CompositionState
{
    /// <summary>
    /// Composition is not active.
    /// </summary>
    Inactive,

    /// <summary>
    /// Composition is actively running.
    /// </summary>
    Active,

    /// <summary>
    /// Composition is progressing through components.
    /// </summary>
    Progressing,

    /// <summary>
    /// Composition has completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Composition has been paused or suspended.
    /// </summary>
    Suspended,

    /// <summary>
    /// Composition has failed or encountered an error.
    /// </summary>
    Failed
}
