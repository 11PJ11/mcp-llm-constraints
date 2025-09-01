using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Parameter object for creating UserDefinedHierarchicalConstraintInfo instances.
/// Encapsulates all the required parameters to improve readability and maintainability.
/// </summary>
public sealed class HierarchicalConstraintParameters : IEquatable<HierarchicalConstraintParameters>
{
    /// <summary>
    /// Gets the user-defined constraint ID.
    /// </summary>
    public string ConstraintId { get; }
    
    /// <summary>
    /// Gets the user-defined hierarchy level for this constraint.
    /// </summary>
    public int HierarchyLevel { get; }
    
    /// <summary>
    /// Gets the user-defined priority within the hierarchy level.
    /// </summary>
    public double Priority { get; }
    
    /// <summary>
    /// Gets the user-defined description of this constraint.
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// Gets additional user-defined metadata for this constraint.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Initializes a new instance of HierarchicalConstraintParameters.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="hierarchyLevel">The user-defined hierarchy level.</param>
    /// <param name="priority">The user-defined priority within the hierarchy level.</param>
    /// <param name="description">The user-defined constraint description.</param>
    /// <param name="metadata">Optional user-defined metadata.</param>
    public HierarchicalConstraintParameters(
        string constraintId,
        int hierarchyLevel,
        double priority,
        string description,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ValidationHelpers.ValidateAll(
            () => ValidationHelpers.RequireNonEmptyString(constraintId, nameof(constraintId), "Constraint ID"),
            () => ValidationHelpers.RequireValidPriority(priority, nameof(priority)),
            () => ValidationHelpers.RequireNonEmptyString(description, nameof(description), "Constraint description")
        );
        
        ConstraintId = constraintId.Trim();
        HierarchyLevel = hierarchyLevel;
        Priority = priority;
        Description = description.Trim();
        Metadata = metadata ?? new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Creates a parameter object with default priority (0.5).
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="hierarchyLevel">The user-defined hierarchy level.</param>
    /// <param name="description">The user-defined constraint description.</param>
    /// <returns>A new HierarchicalConstraintParameters instance with default priority.</returns>
    public static HierarchicalConstraintParameters WithDefaultPriority(
        string constraintId,
        int hierarchyLevel,
        string description)
    {
        return new HierarchicalConstraintParameters(constraintId, hierarchyLevel, 0.5, description);
    }
    
    /// <summary>
    /// Creates a parameter object with metadata.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="hierarchyLevel">The user-defined hierarchy level.</param>
    /// <param name="priority">The user-defined priority within the hierarchy level.</param>
    /// <param name="description">The user-defined constraint description.</param>
    /// <param name="metadata">User-defined metadata.</param>
    /// <returns>A new HierarchicalConstraintParameters instance with metadata.</returns>
    public static HierarchicalConstraintParameters WithMetadata(
        string constraintId,
        int hierarchyLevel,
        double priority,
        string description,
        IReadOnlyDictionary<string, object> metadata)
    {
        return new HierarchicalConstraintParameters(constraintId, hierarchyLevel, priority, description, metadata);
    }
    
    /// <inheritdoc />
    public bool Equals(HierarchicalConstraintParameters? other)
    {
        return other is not null &&
               string.Equals(ConstraintId, other.ConstraintId, StringComparison.OrdinalIgnoreCase) &&
               HierarchyLevel == other.HierarchyLevel &&
               Math.Abs(Priority - other.Priority) < 0.001 &&
               string.Equals(Description, other.Description, StringComparison.Ordinal);
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is HierarchicalConstraintParameters other && Equals(other);
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(ConstraintId),
            HierarchyLevel,
            Priority,
            StringComparer.Ordinal.GetHashCode(Description));
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        return $"Level {HierarchyLevel}: {ConstraintId} (Priority: {Priority:F2}) - {Description}";
    }
    
    /// <summary>
    /// Equality operator for HierarchicalConstraintParameters.
    /// </summary>
    public static bool operator ==(HierarchicalConstraintParameters? left, HierarchicalConstraintParameters? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    /// <summary>
    /// Inequality operator for HierarchicalConstraintParameters.
    /// </summary>
    public static bool operator !=(HierarchicalConstraintParameters? left, HierarchicalConstraintParameters? right)
    {
        return !(left == right);
    }
}