using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Represents a composite constraint that orchestrates multiple atomic constraints.
/// Enables complete methodology workflows like Outside-In Development, Clean Architecture, etc.
/// </summary>
public sealed class CompositeConstraint : IConstraint, IEquatable<CompositeConstraint>
{
    /// <summary>
    /// Gets the unique identifier for this composite constraint.
    /// </summary>
    public ConstraintId Id { get; }

    /// <summary>
    /// Gets the human-readable title of the composite constraint.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the priority for constraint selection (0.0 to 1.0, higher is more important).
    /// </summary>
    public double Priority { get; }

    /// <summary>
    /// Gets the trigger configuration for context-aware activation.
    /// </summary>
    public TriggerConfiguration Triggers { get; }

    /// <summary>
    /// Gets the composition type that determines how components are coordinated.
    /// </summary>
    public CompositionType CompositionType { get; }

    /// <summary>
    /// Gets the atomic constraint components that make up this composite.
    /// </summary>
    public IReadOnlyList<AtomicConstraint> Components { get; }

    /// <summary>
    /// Gets the constraint references for library-based composition.
    /// Used in the new library-based constraint system.
    /// </summary>
    public IReadOnlyList<ConstraintReference> ComponentReferences { get; }

    /// <summary>
    /// Gets the reminder messages to inject when this composite constraint is selected.
    /// </summary>
    public IReadOnlyList<string> Reminders { get; }

    /// <summary>
    /// Gets the composition rules that govern component coordination.
    /// </summary>
    public IReadOnlyList<string> CompositionRules { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets optional metadata for this composite constraint.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; }

    /// <summary>
    /// Initializes a new instance of CompositeConstraint.
    /// </summary>
    /// <param name="id">The unique constraint identifier</param>
    /// <param name="title">The human-readable title</param>
    /// <param name="priority">The selection priority (0.0 to 1.0)</param>
    /// <param name="triggers">The trigger configuration for activation</param>
    /// <param name="compositionType">The composition strategy</param>
    /// <param name="components">The atomic constraint components</param>
    /// <param name="reminders">The reminder messages for injection</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
    /// <exception cref="ValidationException">Thrown when validation rules are violated</exception>
    public CompositeConstraint(
        ConstraintId id,
        string title,
        double priority,
        TriggerConfiguration triggers,
        CompositionType compositionType,
        IEnumerable<AtomicConstraint> components,
        IEnumerable<string> reminders)
    {
        // Validate required parameters
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Triggers = triggers ?? throw new ArgumentNullException(nameof(triggers));

        // Validate business rules
        ValidateTitle(title);
        ValidatePriority(priority);
        ValidateComponents(components);
        ValidateReminders(reminders);
        ValidateCompositionType(compositionType, components);

        Priority = priority;
        CompositionType = compositionType;
        Components = components.ToList().AsReadOnly();
        ComponentReferences = new List<ConstraintReference>().AsReadOnly(); // Empty for legacy constructor
        Reminders = reminders.ToList().AsReadOnly();
        Metadata = new Dictionary<string, object>().AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of CompositeConstraint with constraint references.
    /// Library-based constructor for the new constraint system.
    /// </summary>
    /// <param name="id">The unique constraint identifier</param>
    /// <param name="title">The human-readable title</param>
    /// <param name="priority">The selection priority (0.0 to 1.0)</param>
    /// <param name="compositionType">The composition strategy</param>
    /// <param name="componentReferences">The constraint references</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
    /// <exception cref="ValidationException">Thrown when validation rules are violated</exception>
    public CompositeConstraint(
        ConstraintId id,
        string title,
        double priority,
        CompositionType compositionType,
        IEnumerable<ConstraintReference> componentReferences)
    {
        // Validate required parameters
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Title = title ?? throw new ArgumentNullException(nameof(title));

        // Use default trigger configuration for library-based constructor
        Triggers = new TriggerConfiguration();

        // Validate business rules
        ValidateTitle(title);
        ValidatePriority(priority);
        ValidateComponentReferences(componentReferences);

        Priority = priority;
        CompositionType = compositionType;
        Components = new List<AtomicConstraint>().AsReadOnly(); // Empty for library-based constructor
        ComponentReferences = componentReferences.ToList().AsReadOnly();
        Reminders = new List<string>().AsReadOnly(); // Default empty reminders
        Metadata = new Dictionary<string, object>().AsReadOnly();
    }

    /// <summary>
    /// Checks if this composite constraint matches the given trigger context.
    /// </summary>
    /// <param name="context">The trigger context to evaluate</param>
    /// <returns>True if the constraint should activate for this context</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    public bool MatchesTriggerContext(TriggerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Delegate to trigger configuration like atomic constraints
        return context.CalculateRelevanceScore(Triggers) >= Triggers.ConfidenceThreshold &&
               !context.HasAnyAntiPattern(Triggers.AntiPatterns);
    }

    /// <summary>
    /// Gets the active components for the given composition context.
    /// Different composition types activate components differently.
    /// </summary>
    /// <param name="context">The composition context</param>
    /// <returns>The atomic constraints that should be active</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    public IEnumerable<AtomicConstraint> GetActiveComponents(CompositionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return CompositionType switch
        {
            CompositionType.Sequential => GetSequentialComponents(context),
            CompositionType.Parallel => GetParallelComponents(),
            CompositionType.Hierarchical => GetHierarchicalComponents(context),
            CompositionType.Progressive => GetProgressiveComponents(context),
            CompositionType.Layered => GetLayeredComponents(context),
            _ => throw new InvalidOperationException($"Unsupported composition type: {CompositionType}")
        };
    }

    /// <summary>
    /// Advances the composition to the next state based on current context.
    /// </summary>
    /// <param name="context">The current composition context</param>
    /// <returns>The next composition context</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    public CompositionContext AdvanceComposition(CompositionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return CompositionType switch
        {
            CompositionType.Sequential => AdvanceSequential(context),
            CompositionType.Hierarchical => AdvanceHierarchical(context),
            CompositionType.Progressive => AdvanceProgressive(context),
            CompositionType.Parallel => context.WithState(CompositionState.Completed),
            CompositionType.Layered => AdvanceLayered(context),
            _ => throw new InvalidOperationException($"Unsupported composition type: {CompositionType}")
        };
    }

    /// <summary>
    /// Calculates the overall relevance score for this composite constraint.
    /// </summary>
    /// <param name="context">The trigger context to evaluate against</param>
    /// <returns>Relevance score between 0.0 and 1.0</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    public double CalculateRelevanceScore(TriggerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Composite relevance is combination of trigger match + component relevance
        double triggerScore = context.CalculateRelevanceScore(Triggers);

        if (triggerScore == 0.0)
        {
            return 0.0;
        }

        // Factor in component relevance (components must be relevant too)
        double componentScore = Components.Average(c => c.CalculateRelevanceScore(context));

        // Weight: 70% trigger match, 30% component relevance
        const double TriggerWeight = 0.7;
        const double ComponentWeight = 0.3;

        return (triggerScore * TriggerWeight) + (componentScore * ComponentWeight);
    }

    #region Composition Strategy Implementations

    private IEnumerable<AtomicConstraint> GetSequentialComponents(CompositionContext context)
    {
        // Sequential: Only the current step component is active
        return Components
            .Where(c => c.SequenceOrder == context.SequenceStep)
            .OrderBy(c => c.SequenceOrder);
    }

    private IEnumerable<AtomicConstraint> GetParallelComponents()
    {
        // Parallel: All components are active simultaneously
        return Components;
    }

    private IEnumerable<AtomicConstraint> GetHierarchicalComponents(CompositionContext context)
    {
        // Hierarchical: Components at current hierarchy level are active
        return Components
            .Where(c => c.HierarchyLevel == context.HierarchyLevel)
            .OrderBy(c => c.HierarchyLevel);
    }

    private IEnumerable<AtomicConstraint> GetProgressiveComponents(CompositionContext context)
    {
        // Progressive: Components up to current progression level are active
        return Components
            .Where(c => c.HierarchyLevel.GetValueOrDefault(1) <= context.ProgressionLevel)
            .OrderBy(c => c.HierarchyLevel.GetValueOrDefault(1));
    }

    private IEnumerable<AtomicConstraint> GetLayeredComponents(CompositionContext context)
    {
        // Layered: Similar to hierarchical but with dependency validation
        var currentLevelComponents = Components
            .Where(c => c.HierarchyLevel == context.HierarchyLevel)
            .ToList();

        // Check if lower layers are completed (simplified logic)
        bool lowerLayersCompleted = Components
            .Where(c => c.HierarchyLevel < context.HierarchyLevel)
            .All(c => context.IsComponentCompleted(c.Id.ToString()));

        return lowerLayersCompleted ? currentLevelComponents : Array.Empty<AtomicConstraint>();
    }

    private CompositionContext AdvanceSequential(CompositionContext context)
    {
        int nextStep = context.SequenceStep + 1;
        int maxStep = Components.Max(c => c.SequenceOrder.GetValueOrDefault(1));

        if (nextStep > maxStep)
        {
            return context.WithState(CompositionState.Completed);
        }

        return context.WithSequenceStep(nextStep);
    }

    private CompositionContext AdvanceHierarchical(CompositionContext context)
    {
        int nextLevel = context.HierarchyLevel + 1;
        int maxLevel = Components.Max(c => c.HierarchyLevel.GetValueOrDefault(1));

        if (nextLevel > maxLevel)
        {
            return context.WithState(CompositionState.Completed);
        }

        return context.WithHierarchyLevel(nextLevel);
    }

    private CompositionContext AdvanceProgressive(CompositionContext context)
    {
        int nextLevel = context.ProgressionLevel + 1;
        int maxLevel = Components.Max(c => c.HierarchyLevel.GetValueOrDefault(1));

        if (nextLevel > maxLevel)
        {
            return context.WithState(CompositionState.Completed);
        }

        return context.WithProgressionLevel(nextLevel);
    }

    private CompositionContext AdvanceLayered(CompositionContext context)
    {
        // Similar to hierarchical but with stricter dependency checking
        return AdvanceHierarchical(context);
    }

    #endregion

    #region Equality and ToString

    /// <inheritdoc />
    public bool Equals(CompositeConstraint? other)
    {
        return other is not null && Id == other.Id;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is CompositeConstraint other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Id} (Priority: {Priority:F2}, Composite, {CompositionType}, {Components.Count} components)";
    }

    #endregion

    #region Private Validation Methods

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException("Constraint title cannot be empty or whitespace");
        }
    }

    private static void ValidatePriority(double priority)
    {
        if (priority < 0.0 || priority > 1.0)
        {
            throw new ValidationException("Constraint priority must be between 0.0 and 1.0");
        }
    }

    private static void ValidateComponents(IEnumerable<AtomicConstraint> components)
    {
        ArgumentNullException.ThrowIfNull(components);

        var componentList = components.ToList();
        if (componentList.Count == 0)
        {
            throw new ValidationException("Composite constraint must have at least one component");
        }
    }

    private static void ValidateComponentReferences(IEnumerable<ConstraintReference> componentReferences)
    {
        ArgumentNullException.ThrowIfNull(componentReferences);

        var referenceList = componentReferences.ToList();
        if (referenceList.Count == 0)
        {
            throw new ValidationException("Composite constraint must have at least one component reference");
        }
    }

    private static void ValidateReminders(IEnumerable<string> reminders)
    {
        ArgumentNullException.ThrowIfNull(reminders);

        var reminderList = reminders.ToList();
        if (reminderList.Count == 0)
        {
            throw new ValidationException("Composite constraint must have at least one reminder");
        }

        if (reminderList.Any(string.IsNullOrWhiteSpace))
        {
            throw new ValidationException("All reminders must be non-empty and not whitespace");
        }
    }

    private static void ValidateCompositionType(CompositionType compositionType, IEnumerable<AtomicConstraint> components)
    {
        var componentList = components.ToList();

        switch (compositionType)
        {
            case CompositionType.Sequential:
                ValidateSequentialComposition(componentList);
                break;
            case CompositionType.Hierarchical:
            case CompositionType.Progressive:
            case CompositionType.Layered:
                ValidateHierarchicalComposition(componentList);
                break;
            case CompositionType.Parallel:
                // No additional validation needed for parallel
                break;
            default:
                throw new ValidationException($"Unsupported composition type: {compositionType}");
        }
    }

    private static void ValidateSequentialComposition(List<AtomicConstraint> components)
    {
        var sequenceOrders = components
            .Where(c => c.SequenceOrder.HasValue)
            .Select(c => c.SequenceOrder!.Value)
            .ToList();

        if (sequenceOrders.Count != sequenceOrders.Distinct().Count())
        {
            throw new ValidationException("Sequential composition requires unique sequence orders for all components");
        }
    }

    private static void ValidateHierarchicalComposition(List<AtomicConstraint> components)
    {
        var hierarchyLevels = components
            .Where(c => c.HierarchyLevel.HasValue)
            .Select(c => c.HierarchyLevel!.Value)
            .ToList();

        if (hierarchyLevels.Any(level => level < 0))
        {
            throw new ValidationException("Hierarchical composition requires all hierarchy levels to be non-negative");
        }
    }

    #endregion
}
