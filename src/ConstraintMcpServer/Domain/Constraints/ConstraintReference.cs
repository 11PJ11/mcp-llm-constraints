using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Represents a reference to a constraint within a library-based constraint system.
/// Enables composition metadata and ordering information for composite constraints.
/// </summary>
public sealed class ConstraintReference : IEquatable<ConstraintReference>
{
    /// <summary>
    /// Gets the ID of the referenced constraint.
    /// </summary>
    public ConstraintId ConstraintId { get; }

    /// <summary>
    /// Gets the sequence order for sequential composition (nullable).
    /// </summary>
    public int? SequenceOrder { get; }

    /// <summary>
    /// Gets the hierarchy level for hierarchical composition (nullable).
    /// </summary>
    public int? HierarchyLevel { get; }

    /// <summary>
    /// Gets the composition metadata for this reference.
    /// </summary>
    public IReadOnlyDictionary<string, object> CompositionMetadata { get; }

    /// <summary>
    /// Gets whether this reference has a sequence order.
    /// </summary>
    public bool HasSequenceOrder => SequenceOrder.HasValue;

    /// <summary>
    /// Gets whether this reference has a hierarchy level.
    /// </summary>
    public bool HasHierarchyLevel => HierarchyLevel.HasValue;

    /// <summary>
    /// Initializes a new instance of ConstraintReference.
    /// </summary>
    /// <param name="constraintId">The constraint ID to reference</param>
    /// <param name="sequenceOrder">Optional sequence order for sequential composition</param>
    /// <param name="hierarchyLevel">Optional hierarchy level for hierarchical composition</param>
    /// <param name="metadata">Optional composition metadata</param>
    /// <exception cref="ArgumentNullException">Thrown when constraintId is null</exception>
    /// <exception cref="ValidationException">Thrown when validation rules are violated</exception>
    public ConstraintReference(
        ConstraintId constraintId,
        int? sequenceOrder = null,
        int? hierarchyLevel = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ConstraintId = constraintId ?? throw new ArgumentNullException(nameof(constraintId));

        // Validate sequence order
        if (sequenceOrder.HasValue && sequenceOrder.Value <= 0)
        {
            throw new ValidationException("Sequence order must be positive");
        }

        // Validate hierarchy level
        if (hierarchyLevel.HasValue && hierarchyLevel.Value < 0)
        {
            throw new ValidationException("Hierarchy level must be non-negative");
        }

        SequenceOrder = sequenceOrder;
        HierarchyLevel = hierarchyLevel;
        CompositionMetadata = metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).AsReadOnly()
                             ?? new Dictionary<string, object>().AsReadOnly();
    }

    /// <summary>
    /// Creates a new ConstraintReference with updated composition metadata.
    /// </summary>
    /// <param name="metadata">The new composition metadata</param>
    /// <returns>A new ConstraintReference with the updated metadata</returns>
    /// <exception cref="ArgumentNullException">Thrown when metadata is null</exception>
    public ConstraintReference WithCompositionMetadata(IReadOnlyDictionary<string, object> metadata)
    {
        if (metadata == null)
        {
            throw new ArgumentNullException(nameof(metadata));
        }

        return new ConstraintReference(ConstraintId, SequenceOrder, HierarchyLevel, metadata);
    }

    /// <summary>
    /// Creates a new ConstraintReference with updated sequence order.
    /// </summary>
    /// <param name="sequenceOrder">The new sequence order</param>
    /// <returns>A new ConstraintReference with the updated sequence order</returns>
    /// <exception cref="ValidationException">Thrown when sequence order is invalid</exception>
    public ConstraintReference WithSequenceOrder(int sequenceOrder)
    {
        if (sequenceOrder <= 0)
        {
            throw new ValidationException("Sequence order must be positive");
        }

        return new ConstraintReference(ConstraintId, sequenceOrder, HierarchyLevel, CompositionMetadata);
    }

    /// <summary>
    /// Gets the reference context for error reporting.
    /// </summary>
    /// <returns>A ReferenceContext with constraint information</returns>
    public ReferenceContext GetReferenceContext()
    {
        return new ReferenceContext
        {
            ConstraintId = ConstraintId,
            Source = "ConstraintReference",
            Step = SequenceOrder?.ToString(),
            Position = HierarchyLevel?.ToString()
        };
    }

    /// <summary>
    /// Determines equality with another ConstraintReference.
    /// </summary>
    /// <param name="other">The other ConstraintReference to compare</param>
    /// <returns>True if references are equal</returns>
    public bool Equals(ConstraintReference? other)
    {
        return other is not null && ConstraintId.Equals(other.ConstraintId);
    }

    /// <summary>
    /// Determines equality with another object.
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns>True if objects are equal</returns>
    public override bool Equals(object? obj)
    {
        return obj is ConstraintReference other && Equals(other);
    }

    /// <summary>
    /// Gets the hash code for this reference.
    /// </summary>
    /// <returns>Hash code based on ConstraintId</returns>
    public override int GetHashCode()
    {
        return ConstraintId.GetHashCode();
    }

    /// <summary>
    /// Returns a string representation of this constraint reference.
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        var parts = new List<string> { ConstraintId.ToString() };

        if (HasSequenceOrder)
        {
            parts.Add($"Sequence: {SequenceOrder}");
        }

        if (HasHierarchyLevel)
        {
            parts.Add($"Level: {HierarchyLevel}");
        }

        if (CompositionMetadata.Count > 0)
        {
            parts.Add($"Metadata: {CompositionMetadata.Count} items");
        }

        return $"ConstraintReference({string.Join(", ", parts)})";
    }
}

/// <summary>
/// Context information for constraint reference error reporting.
/// </summary>
public sealed class ReferenceContext
{
    /// <summary>
    /// Gets or sets the constraint ID for this context.
    /// </summary>
    public required ConstraintId ConstraintId { get; init; }

    /// <summary>
    /// Gets or sets the source of this reference.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Gets or sets the step information.
    /// </summary>
    public string? Step { get; init; }

    /// <summary>
    /// Gets or sets the position information.
    /// </summary>
    public string? Position { get; init; }

    /// <summary>
    /// Returns a string representation of this reference context.
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        var parts = new List<string> { ConstraintId.ToString() };

        if (!string.IsNullOrWhiteSpace(Source))
        {
            parts.Add($"Source: {Source}");
        }

        if (!string.IsNullOrWhiteSpace(Step))
        {
            parts.Add($"Step: {Step}");
        }

        if (!string.IsNullOrWhiteSpace(Position))
        {
            parts.Add($"Position: {Position}");
        }

        return $"ReferenceContext({string.Join(", ", parts)})";
    }
}
