using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Builds composite constraint instances with proper validation and initialization.
/// Extracted from CompositeConstraint to follow Single Responsibility Principle.
/// Level 3 Refactoring: Separates construction logic from constraint definition.
/// </summary>
public static class CompositeConstraintBuilder
{
    /// <summary>
    /// Creates a new composite constraint with atomic constraint components.
    /// </summary>
    /// <param name="id">The unique constraint identifier</param>
    /// <param name="title">The human-readable title</param>
    /// <param name="priority">The selection priority (0.0 to 1.0)</param>
    /// <param name="triggers">The trigger configuration for activation</param>
    /// <param name="compositionType">The composition strategy</param>
    /// <param name="components">The atomic constraint components</param>
    /// <param name="reminders">The reminder messages for injection</param>
    /// <returns>A new composite constraint instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
    /// <exception cref="ValidationException">Thrown when validation rules are violated</exception>
    public static CompositeConstraint CreateWithComponents(
        ConstraintId id,
        string title,
        double priority,
        TriggerConfiguration triggers,
        CompositionType compositionType,
        IEnumerable<AtomicConstraint> components,
        IEnumerable<string> reminders)
    {
        // Validate required parameters
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        ArgumentNullException.ThrowIfNull(title, nameof(title));
        ArgumentNullException.ThrowIfNull(triggers, nameof(triggers));

        // Validate business rules using extracted validator
        CompositeConstraintValidator.ValidateCompositeConstraint(
            title, priority, components, null, reminders, compositionType);

        // Build the constraint with validated parameters using internal constructor
        return new CompositeConstraint(
            id: id,
            title: title,
            priority: priority,
            triggers: triggers,
            compositionType: compositionType,
            components: components.ToList().AsReadOnly(),
            componentReferences: new List<ConstraintReference>().AsReadOnly(),
            reminders: reminders.ToList().AsReadOnly(),
            metadata: new Dictionary<string, object>().AsReadOnly());
    }

    /// <summary>
    /// Creates a new composite constraint with constraint references (library-based).
    /// </summary>
    /// <param name="id">The unique constraint identifier</param>
    /// <param name="title">The human-readable title</param>
    /// <param name="priority">The selection priority (0.0 to 1.0)</param>
    /// <param name="compositionType">The composition strategy</param>
    /// <param name="componentReferences">The constraint references</param>
    /// <returns>A new composite constraint instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
    /// <exception cref="ValidationException">Thrown when validation rules are violated</exception>
    public static CompositeConstraint CreateWithReferences(
        ConstraintId id,
        string title,
        double priority,
        CompositionType compositionType,
        IEnumerable<ConstraintReference> componentReferences)
    {
        // Validate required parameters
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        ArgumentNullException.ThrowIfNull(title, nameof(title));

        // Validate business rules using extracted validator
        CompositeConstraintValidator.ValidateCompositeConstraint(
            title, priority, null, componentReferences, null, compositionType);

        // Use default trigger configuration for library-based constructor
        var defaultTriggers = new TriggerConfiguration();

        // Build the constraint with validated parameters using internal constructor
        return new CompositeConstraint(
            id: id,
            title: title,
            priority: priority,
            triggers: defaultTriggers,
            compositionType: compositionType,
            components: new List<AtomicConstraint>().AsReadOnly(),
            componentReferences: componentReferences.ToList().AsReadOnly(),
            reminders: new List<string>().AsReadOnly(),
            metadata: new Dictionary<string, object>().AsReadOnly());
    }

    /// <summary>
    /// Creates a new composite constraint with full metadata and composition rules.
    /// Advanced builder method for complete constraint specification.
    /// </summary>
    /// <param name="id">The unique constraint identifier</param>
    /// <param name="title">The human-readable title</param>
    /// <param name="priority">The selection priority (0.0 to 1.0)</param>
    /// <param name="triggers">The trigger configuration for activation</param>
    /// <param name="compositionType">The composition strategy</param>
    /// <param name="components">The atomic constraint components</param>
    /// <param name="componentReferences">The constraint references (for library-based)</param>
    /// <param name="reminders">The reminder messages for injection</param>
    /// <param name="compositionRules">The composition rules governing component coordination</param>
    /// <param name="metadata">Optional metadata for the constraint</param>
    /// <returns>A new composite constraint instance</returns>
    public static CompositeConstraint CreateComplete(
        ConstraintId id,
        string title,
        double priority,
        TriggerConfiguration triggers,
        CompositionType compositionType,
        IEnumerable<AtomicConstraint>? components = null,
        IEnumerable<ConstraintReference>? componentReferences = null,
        IEnumerable<string>? reminders = null,
        IEnumerable<string>? compositionRules = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        // Validate required parameters
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        ArgumentNullException.ThrowIfNull(title, nameof(title));
        ArgumentNullException.ThrowIfNull(triggers, nameof(triggers));

        // Validate business rules using extracted validator
        CompositeConstraintValidator.ValidateCompositeConstraint(
            title, priority, components, componentReferences, reminders, compositionType);

        // Build the constraint with all parameters
        return new CompositeConstraint(
            id: id,
            title: title,
            priority: priority,
            triggers: triggers,
            compositionType: compositionType,
            components: (components ?? Enumerable.Empty<AtomicConstraint>()).ToList().AsReadOnly(),
            componentReferences: (componentReferences ?? Enumerable.Empty<ConstraintReference>()).ToList().AsReadOnly(),
            reminders: (reminders ?? Enumerable.Empty<string>()).ToList().AsReadOnly(),
            metadata: metadata ?? new Dictionary<string, object>().AsReadOnly())
        {
            CompositionRules = (compositionRules ?? Enumerable.Empty<string>()).ToList().AsReadOnly()
        };
    }
}
