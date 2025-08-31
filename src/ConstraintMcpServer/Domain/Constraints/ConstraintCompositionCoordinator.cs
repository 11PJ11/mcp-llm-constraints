using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Coordinates composition strategies for composite constraints.
/// Extracted from CompositeConstraint to follow Single Responsibility Principle.
/// Level 3 Refactoring: Separates composition orchestration from constraint definition.
/// </summary>
internal static class ConstraintCompositionCoordinator
{
    // Level 1 Refactoring: Named constants for composition weights
    private const double TriggerWeight = 0.7;
    private const double ComponentWeight = 0.3;

    /// <summary>
    /// Gets the active components for a composite constraint based on composition type and context.
    /// </summary>
    /// <param name="compositionType">The composition strategy to use</param>
    /// <param name="components">The available atomic constraint components</param>
    /// <param name="context">The composition context</param>
    /// <returns>The atomic constraints that should be active</returns>
    /// <exception cref="ArgumentNullException">Thrown when context or components are null</exception>
    public static IEnumerable<AtomicConstraint> GetActiveComponents(
        CompositionType compositionType,
        IReadOnlyList<AtomicConstraint> components,
        CompositionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(components);

        return compositionType switch
        {
            CompositionType.Sequential => GetSequentialComponents(components, context),
            CompositionType.Parallel => GetParallelComponents(components),
            CompositionType.Hierarchical => GetHierarchicalComponents(components, context),
            CompositionType.Progressive => GetProgressiveComponents(components, context),
            CompositionType.Layered => GetLayeredComponents(components, context),
            _ => throw new ArgumentException($"Unsupported composition type: {compositionType}", nameof(compositionType))
        };
    }

    /// <summary>
    /// Calculates composite relevance score combining trigger match and component relevance.
    /// </summary>
    /// <param name="triggerScore">The trigger match score (0.0 to 1.0)</param>
    /// <param name="components">The atomic constraint components</param>
    /// <param name="context">The trigger context for component evaluation</param>
    /// <returns>Combined relevance score (0.0 to 1.0)</returns>
    public static double CalculateCompositeRelevanceScore(
        double triggerScore,
        IReadOnlyList<AtomicConstraint> components,
        TriggerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(components);

        if (triggerScore <= 0.0)
        {
            return 0.0;
        }

        if (components.Count == 0)
        {
            return triggerScore; // Only trigger score if no components
        }

        // Calculate average component relevance
        double componentScore = components.Average(c => c.CalculateRelevanceScore(context));

        // Apply weighted combination: 70% trigger match, 30% component relevance
        return (triggerScore * TriggerWeight) + (componentScore * ComponentWeight);
    }

    /// <summary>
    /// Advances composition context to next state based on composition type.
    /// </summary>
    /// <param name="compositionType">The composition strategy</param>
    /// <param name="context">Current composition context</param>
    /// <param name="components">Available components for advancement logic</param>
    /// <returns>Advanced composition context</returns>
    public static CompositionContext AdvanceComposition(
        CompositionType compositionType,
        CompositionContext context,
        IReadOnlyList<AtomicConstraint> components)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(components);

        return compositionType switch
        {
            CompositionType.Sequential => AdvanceSequential(context, components),
            CompositionType.Hierarchical => AdvanceHierarchical(context, components),
            CompositionType.Progressive => AdvanceProgressive(context, components),
            CompositionType.Layered => AdvanceLayered(context, components),
            CompositionType.Parallel => context, // No advancement needed for parallel
            _ => throw new ArgumentException($"Unsupported composition type: {compositionType}", nameof(compositionType))
        };
    }

    #region Composition Strategy Implementations

    /// <summary>
    /// Sequential composition: Only current step component is active.
    /// </summary>
    private static IEnumerable<AtomicConstraint> GetSequentialComponents(
        IReadOnlyList<AtomicConstraint> components,
        CompositionContext context)
    {
        return components
            .Where(c => c.SequenceOrder == context.SequenceStep)
            .OrderBy(c => c.SequenceOrder);
    }

    /// <summary>
    /// Parallel composition: All components are active simultaneously.
    /// </summary>
    private static IEnumerable<AtomicConstraint> GetParallelComponents(IReadOnlyList<AtomicConstraint> components)
    {
        return components;
    }

    /// <summary>
    /// Hierarchical composition: Components at current hierarchy level are active.
    /// </summary>
    private static IEnumerable<AtomicConstraint> GetHierarchicalComponents(
        IReadOnlyList<AtomicConstraint> components,
        CompositionContext context)
    {
        return components
            .Where(c => c.HierarchyLevel == context.HierarchyLevel)
            .OrderBy(c => c.HierarchyLevel);
    }

    /// <summary>
    /// Progressive composition: Components up to current progression level are active.
    /// </summary>
    private static IEnumerable<AtomicConstraint> GetProgressiveComponents(
        IReadOnlyList<AtomicConstraint> components,
        CompositionContext context)
    {
        return components
            .Where(c => c.HierarchyLevel.GetValueOrDefault(1) <= context.ProgressionLevel)
            .OrderBy(c => c.HierarchyLevel.GetValueOrDefault(1));
    }

    /// <summary>
    /// Layered composition: Components with dependency validation.
    /// </summary>
    private static IEnumerable<AtomicConstraint> GetLayeredComponents(
        IReadOnlyList<AtomicConstraint> components,
        CompositionContext context)
    {
        var currentLevelComponents = components
            .Where(c => c.HierarchyLevel == context.HierarchyLevel)
            .ToList();

        // Check if lower layers are completed (simplified logic)
        bool lowerLayersCompleted = components
            .Where(c => c.HierarchyLevel < context.HierarchyLevel)
            .All(c => context.IsComponentCompleted(c.Id.ToString()));

        return lowerLayersCompleted ? currentLevelComponents : Array.Empty<AtomicConstraint>();
    }

    #endregion

    #region Context Advancement Logic

    /// <summary>
    /// Advances sequential composition to next step.
    /// </summary>
    private static CompositionContext AdvanceSequential(CompositionContext context, IReadOnlyList<AtomicConstraint> components)
    {
        int nextStep = context.SequenceStep + 1;
        int maxStep = components.Max(c => c.SequenceOrder.GetValueOrDefault(1));

        if (nextStep > maxStep)
        {
            return context.WithState(CompositionState.Completed);
        }

        return context.WithSequenceStep(nextStep);
    }

    /// <summary>
    /// Advances hierarchical composition to next level.
    /// </summary>
    private static CompositionContext AdvanceHierarchical(CompositionContext context, IReadOnlyList<AtomicConstraint> components)
    {
        int nextLevel = context.HierarchyLevel + 1;
        int maxLevel = components.Max(c => c.HierarchyLevel.GetValueOrDefault(1));

        if (nextLevel > maxLevel)
        {
            return context.WithState(CompositionState.Completed);
        }

        return context.WithHierarchyLevel(nextLevel);
    }

    /// <summary>
    /// Advances progressive composition to next progression level.
    /// </summary>
    private static CompositionContext AdvanceProgressive(CompositionContext context, IReadOnlyList<AtomicConstraint> components)
    {
        int nextLevel = context.ProgressionLevel + 1;
        int maxLevel = components.Max(c => c.HierarchyLevel.GetValueOrDefault(1));

        if (nextLevel > maxLevel)
        {
            return context.WithState(CompositionState.Completed);
        }

        return context.WithProgressionLevel(nextLevel);
    }

    /// <summary>
    /// Advances layered composition with dependency checking.
    /// </summary>
    private static CompositionContext AdvanceLayered(CompositionContext context, IReadOnlyList<AtomicConstraint> components)
    {
        // Similar to hierarchical but with additional dependency validation
        return AdvanceHierarchical(context, components);
    }

    #endregion
}
