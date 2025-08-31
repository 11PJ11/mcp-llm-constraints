using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Represents an atomic constraint - a single, focused constraint rule that can be composed.
/// Atomic constraints are building blocks for composite methodologies and can activate independently.
/// </summary>
public sealed class AtomicConstraint : IConstraint, IEquatable<AtomicConstraint>
{
    /// <summary>
    /// Gets the unique identifier for this constraint.
    /// </summary>
    public ConstraintId Id { get; }

    /// <summary>
    /// Gets the human-readable title of the constraint.
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
    /// Gets the reminder messages to inject when this constraint is selected.
    /// </summary>
    public IReadOnlyList<string> Reminders { get; }

    /// <summary>
    /// Gets the sequence order for sequential composition (nullable).
    /// Only relevant when used as component in sequential composite constraint.
    /// </summary>
    public int? SequenceOrder { get; init; }

    /// <summary>
    /// Gets the hierarchy level for hierarchical composition (nullable).
    /// Only relevant when used as component in hierarchical composite constraint.
    /// </summary>
    public int? HierarchyLevel { get; init; }

    /// <summary>
    /// Gets optional metadata for this constraint.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; }

    /// <summary>
    /// Initializes a new instance of AtomicConstraint.
    /// </summary>
    /// <param name="id">The unique constraint identifier</param>
    /// <param name="title">The human-readable title</param>
    /// <param name="priority">The selection priority (0.0 to 1.0)</param>
    /// <param name="triggers">The trigger configuration for activation</param>
    /// <param name="reminders">The reminder messages for injection</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
    /// <exception cref="ValidationException">Thrown when validation rules are violated</exception>
    public AtomicConstraint(
        ConstraintId id,
        string title,
        double priority,
        TriggerConfiguration triggers,
        IEnumerable<string> reminders)
    {
        // Validate required parameters
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Triggers = triggers ?? throw new ArgumentNullException(nameof(triggers));

        // Validate business rules
        ConstraintValidationRules.ValidateTitle(title);
        ConstraintValidationRules.ValidatePriority(priority);
        ConstraintValidationRules.ValidateReminders(reminders);

        Priority = priority;
        Reminders = reminders.ToList().AsReadOnly();
        Metadata = new Dictionary<string, object>().AsReadOnly();
    }

    /// <summary>
    /// Checks if this constraint matches the given trigger context.
    /// </summary>
    /// <param name="context">The trigger context to evaluate</param>
    /// <returns>True if the constraint should activate for this context</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    public bool MatchesTriggerContext(TriggerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Check for anti-patterns first - they override everything
        if (Triggers.AntiPatterns.Count > 0 && context.HasAnyAntiPattern(Triggers.AntiPatterns))
        {
            return false;
        }

        // Must have activation criteria to match
        if (!Triggers.HasActivationCriteria)
        {
            return false;
        }

        double relevanceScore = CalculateRelevanceScore(context);
        return relevanceScore >= Triggers.ConfidenceThreshold;
    }

    /// <summary>
    /// Calculates the relevance score for this constraint given the trigger context.
    /// </summary>
    /// <param name="context">The trigger context to evaluate against</param>
    /// <returns>Relevance score between 0.0 and 1.0</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    public double CalculateRelevanceScore(TriggerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.CalculateRelevanceScore(Triggers);
    }

    /// <summary>
    /// Creates a copy of this constraint with updated sequence order for composition.
    /// </summary>
    /// <param name="sequenceOrder">The sequence order for sequential composition</param>
    /// <returns>New atomic constraint with updated sequence order</returns>
    public AtomicConstraint WithSequenceOrder(int sequenceOrder)
    {
        return new AtomicConstraint(Id, Title, Priority, Triggers, Reminders)
        {
            SequenceOrder = sequenceOrder,
            HierarchyLevel = HierarchyLevel,
            Metadata = Metadata
        };
    }

    /// <summary>
    /// Creates a copy of this constraint with updated hierarchy level for composition.
    /// </summary>
    /// <param name="hierarchyLevel">The hierarchy level for hierarchical composition</param>
    /// <returns>New atomic constraint with updated hierarchy level</returns>
    public AtomicConstraint WithHierarchyLevel(int hierarchyLevel)
    {
        if (hierarchyLevel < 0)
        {
            throw new ValidationException("Hierarchy level must be non-negative");
        }

        return new AtomicConstraint(Id, Title, Priority, Triggers, Reminders)
        {
            SequenceOrder = SequenceOrder,
            HierarchyLevel = hierarchyLevel,
            Metadata = Metadata
        };
    }

    /// <summary>
    /// Creates a copy of this constraint with additional metadata.
    /// </summary>
    /// <param name="metadata">Additional metadata to include</param>
    /// <returns>New atomic constraint with updated metadata</returns>
    public AtomicConstraint WithMetadata(IReadOnlyDictionary<string, object> metadata)
    {
        return new AtomicConstraint(Id, Title, Priority, Triggers, Reminders)
        {
            SequenceOrder = SequenceOrder,
            HierarchyLevel = HierarchyLevel,
            Metadata = metadata ?? new Dictionary<string, object>().AsReadOnly()
        };
    }

    #region Equality and ToString

    /// <inheritdoc />
    public bool Equals(AtomicConstraint? other)
    {
        return other is not null && Id == other.Id;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is AtomicConstraint other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var components = new List<string>
        {
            $"Priority: {Priority:F2}",
            "Atomic"
        };

        if (SequenceOrder.HasValue)
        {
            components.Add($"Sequence: {SequenceOrder.Value}");
        }

        if (HierarchyLevel.HasValue)
        {
            components.Add($"Level: {HierarchyLevel.Value}");
        }

        return $"{Id} ({string.Join(", ", components)})";
    }

    /// <summary>
    /// Equality operator for AtomicConstraint.
    /// </summary>
    public static bool operator ==(AtomicConstraint? left, AtomicConstraint? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    /// <summary>
    /// Inequality operator for AtomicConstraint.
    /// </summary>
    public static bool operator !=(AtomicConstraint? left, AtomicConstraint? right)
    {
        return !(left == right);
    }

    #endregion

}
