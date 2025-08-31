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
    /// Use CompositeConstraintBuilder for construction with validation.
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
    [Obsolete("Use CompositeConstraintBuilder.CreateWithComponents instead", false)]
    public CompositeConstraint(
        ConstraintId id,
        string title,
        double priority,
        TriggerConfiguration triggers,
        CompositionType compositionType,
        IEnumerable<AtomicConstraint> components,
        IEnumerable<string> reminders)
    {
        // Delegate to builder for proper validation and construction
        var built = CompositeConstraintBuilder.CreateWithComponents(
            id, title, priority, triggers, compositionType, components, reminders);

        // Copy properties from built instance
        Id = built.Id;
        Title = built.Title;
        Priority = built.Priority;
        Triggers = built.Triggers;
        CompositionType = built.CompositionType;
        Components = built.Components;
        ComponentReferences = built.ComponentReferences;
        Reminders = built.Reminders;
        Metadata = built.Metadata;
        CompositionRules = built.CompositionRules;
    }

    /// <summary>
    /// Initializes a new instance of CompositeConstraint with constraint references.
    /// Library-based constructor for the new constraint system.
    /// Use CompositeConstraintBuilder for construction with validation.
    /// </summary>
    /// <param name="id">The unique constraint identifier</param>
    /// <param name="title">The human-readable title</param>
    /// <param name="priority">The selection priority (0.0 to 1.0)</param>
    /// <param name="compositionType">The composition strategy</param>
    /// <param name="componentReferences">The constraint references</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
    /// <exception cref="ValidationException">Thrown when validation rules are violated</exception>
    [Obsolete("Use CompositeConstraintBuilder.CreateWithReferences instead", false)]
    public CompositeConstraint(
        ConstraintId id,
        string title,
        double priority,
        CompositionType compositionType,
        IEnumerable<ConstraintReference> componentReferences)
    {
        // Delegate to builder for proper validation and construction
        var built = CompositeConstraintBuilder.CreateWithReferences(
            id, title, priority, compositionType, componentReferences);

        // Copy properties from built instance
        Id = built.Id;
        Title = built.Title;
        Priority = built.Priority;
        Triggers = built.Triggers;
        CompositionType = built.CompositionType;
        Components = built.Components;
        ComponentReferences = built.ComponentReferences;
        Reminders = built.Reminders;
        Metadata = built.Metadata;
        CompositionRules = built.CompositionRules;
    }

    /// <summary>
    /// Internal constructor for use by CompositeConstraintBuilder.
    /// All validation should be done by the builder before calling this constructor.
    /// </summary>
    internal CompositeConstraint(
        ConstraintId id,
        string title,
        double priority,
        TriggerConfiguration triggers,
        CompositionType compositionType,
        IReadOnlyList<AtomicConstraint> components,
        IReadOnlyList<ConstraintReference> componentReferences,
        IReadOnlyList<string> reminders,
        IReadOnlyDictionary<string, object> metadata)
    {
        Id = id;
        Title = title;
        Priority = priority;
        Triggers = triggers;
        CompositionType = compositionType;
        Components = components;
        ComponentReferences = componentReferences;
        Reminders = reminders;
        Metadata = metadata;
        CompositionRules = Array.Empty<string>();
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
        // Delegate to extracted composition coordinator
        return ConstraintCompositionCoordinator.GetActiveComponents(CompositionType, Components, context);
    }

    /// <summary>
    /// Advances the composition to the next state based on current context.
    /// </summary>
    /// <param name="context">The current composition context</param>
    /// <returns>The next composition context</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    public CompositionContext AdvanceComposition(CompositionContext context)
    {
        // Delegate to extracted composition coordinator
        return ConstraintCompositionCoordinator.AdvanceComposition(CompositionType, context, Components);
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

        // Delegate to extracted composition coordinator
        double triggerScore = context.CalculateRelevanceScore(Triggers);
        return ConstraintCompositionCoordinator.CalculateCompositeRelevanceScore(triggerScore, Components, context);
    }


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

}
