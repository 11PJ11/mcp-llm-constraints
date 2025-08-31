using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Domain;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Validates composite constraint construction and business rules.
/// Extracted from CompositeConstraint to follow Single Responsibility Principle.
/// Level 3 Refactoring: Separates validation concerns from constraint definition.
/// </summary>
internal static class CompositeConstraintValidator
{
    /// <summary>
    /// Validates all business rules for composite constraint creation.
    /// </summary>
    /// <param name="title">The constraint title</param>
    /// <param name="priority">The constraint priority</param>
    /// <param name="components">The atomic constraint components (if any)</param>
    /// <param name="componentReferences">The constraint references (if any)</param>
    /// <param name="reminders">The reminder messages (if any)</param>
    /// <param name="compositionType">The composition type</param>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    public static void ValidateCompositeConstraint(
        string title,
        double priority,
        IEnumerable<AtomicConstraint>? components,
        IEnumerable<ConstraintReference>? componentReferences,
        IEnumerable<string>? reminders,
        CompositionType compositionType)
    {
        ConstraintValidationRules.ValidateTitle(title);
        ConstraintValidationRules.ValidatePriority(priority);

        // Validate either components or component references are provided
        if (components != null)
        {
            ValidateComponents(components);
            if (reminders != null)
            {
                ConstraintValidationRules.ValidateReminders(reminders);
            }
            ValidateCompositionType(compositionType, components);
        }
        else if (componentReferences != null)
        {
            ValidateComponentReferences(componentReferences);
        }
        else
        {
            throw new ValidationException("Composite constraint must have either components or component references");
        }
    }


    /// <summary>
    /// Validates that components collection is not null or empty.
    /// </summary>
    public static void ValidateComponents(IEnumerable<AtomicConstraint> components)
    {
        ArgumentNullException.ThrowIfNull(components);

        var componentList = components.ToList();
        if (componentList.Count == 0)
        {
            throw new ValidationException("Composite constraint must have at least one component");
        }
    }

    /// <summary>
    /// Validates that component references collection is not null or empty.
    /// </summary>
    public static void ValidateComponentReferences(IEnumerable<ConstraintReference> componentReferences)
    {
        ArgumentNullException.ThrowIfNull(componentReferences);

        var referenceList = componentReferences.ToList();
        if (referenceList.Count == 0)
        {
            throw new ValidationException("Composite constraint must have at least one component reference");
        }
    }


    /// <summary>
    /// Validates composition type specific rules against components.
    /// </summary>
    public static void ValidateCompositionType(CompositionType compositionType, IEnumerable<AtomicConstraint> components)
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

    /// <summary>
    /// Validates sequential composition requires unique sequence orders.
    /// </summary>
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

    /// <summary>
    /// Validates hierarchical composition requires valid hierarchy levels.
    /// </summary>
    private static void ValidateHierarchicalComposition(List<AtomicConstraint> components)
    {
        var hierarchyLevels = components
            .Where(c => c.HierarchyLevel.HasValue)
            .Select(c => c.HierarchyLevel!.Value)
            .ToList();

        if (hierarchyLevels.Any(level => level < 0))
        {
            throw new ValidationException("Hierarchical composition requires non-negative hierarchy levels");
        }
    }
}
