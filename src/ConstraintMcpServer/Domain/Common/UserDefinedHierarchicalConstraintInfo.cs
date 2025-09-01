using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Represents information about a user-defined hierarchical constraint.
/// This class allows users to define constraints with hierarchy levels and priorities
/// for any hierarchical methodology or practice.
/// </summary>
public sealed class UserDefinedHierarchicalConstraintInfo : IEquatable<UserDefinedHierarchicalConstraintInfo>
{
    /// <summary>
    /// Gets the user-defined constraint ID.
    /// </summary>
    public string ConstraintId { get; }
    
    /// <summary>
    /// Gets the user-defined hierarchy level for this constraint.
    /// Lower numbers typically represent higher priority levels.
    /// </summary>
    public int HierarchyLevel { get; }
    
    /// <summary>
    /// Gets the user-defined priority within the hierarchy level.
    /// Higher numbers represent higher priority within the same level.
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
    /// Initializes a new instance of UserDefinedHierarchicalConstraintInfo.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="hierarchyLevel">The user-defined hierarchy level.</param>
    /// <param name="priority">The user-defined priority within the hierarchy level.</param>
    /// <param name="description">The user-defined constraint description.</param>
    /// <param name="metadata">Optional user-defined metadata.</param>
    /// <exception cref="ArgumentException">Thrown when required parameters are invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when priority is out of valid range.</exception>
    public UserDefinedHierarchicalConstraintInfo(
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
    /// Initializes a new instance of UserDefinedHierarchicalConstraintInfo using parameter object.
    /// </summary>
    /// <param name="parameters">The hierarchical constraint parameters.</param>
    /// <exception cref="ArgumentNullException">Thrown when parameters is null.</exception>
    public UserDefinedHierarchicalConstraintInfo(HierarchicalConstraintParameters parameters)
    {
        ValidationHelpers.RequireNotNull(parameters, nameof(parameters), "Hierarchical constraint parameters");
        
        ConstraintId = parameters.ConstraintId;
        HierarchyLevel = parameters.HierarchyLevel;
        Priority = parameters.Priority;
        Description = parameters.Description;
        Metadata = parameters.Metadata;
    }
    
    /// <summary>
    /// Creates a UserDefinedHierarchicalConstraintInfo from user input with validation.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="hierarchyLevel">The user-defined hierarchy level.</param>
    /// <param name="priority">The user-defined priority.</param>
    /// <param name="description">The user-defined constraint description.</param>
    /// <returns>A new UserDefinedHierarchicalConstraintInfo instance.</returns>
    public static UserDefinedHierarchicalConstraintInfo Create(
        string constraintId,
        int hierarchyLevel,
        double priority,
        string description)
    {
        return new UserDefinedHierarchicalConstraintInfo(constraintId, hierarchyLevel, priority, description);
    }
    
    /// <summary>
    /// Creates a UserDefinedHierarchicalConstraintInfo from parameter object.
    /// </summary>
    /// <param name="parameters">The hierarchical constraint parameters.</param>
    /// <returns>A new UserDefinedHierarchicalConstraintInfo instance.</returns>
    public static UserDefinedHierarchicalConstraintInfo Create(HierarchicalConstraintParameters parameters)
    {
        return new UserDefinedHierarchicalConstraintInfo(parameters);
    }
    
    /// <summary>
    /// Creates a UserDefinedHierarchicalConstraintInfo with default priority (0.5).
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="hierarchyLevel">The user-defined hierarchy level.</param>
    /// <param name="description">The user-defined constraint description.</param>
    /// <returns>A new UserDefinedHierarchicalConstraintInfo instance with default priority.</returns>
    public static UserDefinedHierarchicalConstraintInfo WithDefaultPriority(
        string constraintId,
        int hierarchyLevel,
        string description)
    {
        return new UserDefinedHierarchicalConstraintInfo(constraintId, hierarchyLevel, 0.5, description);
    }
    
    /// <summary>
    /// Creates a UserDefinedHierarchicalConstraintInfo with additional user-defined metadata.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="hierarchyLevel">The user-defined hierarchy level.</param>
    /// <param name="priority">The user-defined priority.</param>
    /// <param name="description">The user-defined constraint description.</param>
    /// <param name="metadata">User-defined metadata.</param>
    /// <returns>A new UserDefinedHierarchicalConstraintInfo instance with metadata.</returns>
    public static UserDefinedHierarchicalConstraintInfo WithMetadata(
        string constraintId,
        int hierarchyLevel,
        double priority,
        string description,
        IReadOnlyDictionary<string, object> metadata)
    {
        return new UserDefinedHierarchicalConstraintInfo(constraintId, hierarchyLevel, priority, description, metadata);
    }
    
    /// <summary>
    /// Attempts to create a UserDefinedHierarchicalConstraintInfo from user input.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="hierarchyLevel">The user-defined hierarchy level.</param>
    /// <param name="priority">The user-defined priority.</param>
    /// <param name="description">The user-defined constraint description.</param>
    /// <param name="constraintInfo">The created UserDefinedHierarchicalConstraintInfo if successful.</param>
    /// <returns>True if creation was successful, false otherwise.</returns>
    public static bool TryCreate(
        string constraintId,
        int hierarchyLevel,
        double priority,
        string description,
        out UserDefinedHierarchicalConstraintInfo? constraintInfo)
    {
        try
        {
            constraintInfo = Create(constraintId, hierarchyLevel, priority, description);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            constraintInfo = null;
            return false;
        }
        catch (ArgumentException)
        {
            constraintInfo = null;
            return false;
        }
    }
    
    /// <summary>
    /// Compares this constraint to another for hierarchical ordering.
    /// </summary>
    /// <param name="other">The other constraint to compare to.</param>
    /// <returns>Comparison result for hierarchical ordering (level first, then priority).</returns>
    public int CompareHierarchically(UserDefinedHierarchicalConstraintInfo other)
    {
        if (other == null)
            return 1;
        
        // Primary comparison: hierarchy level (ascending - lower levels have higher priority)
        var levelComparison = HierarchyLevel.CompareTo(other.HierarchyLevel);
        if (levelComparison != 0)
            return levelComparison;
        
        // Secondary comparison: priority within level (descending - higher values have higher priority)
        return other.Priority.CompareTo(Priority);
    }
    
    /// <inheritdoc />
    public bool Equals(UserDefinedHierarchicalConstraintInfo? other)
    {
        return other is not null &&
               string.Equals(ConstraintId, other.ConstraintId, StringComparison.OrdinalIgnoreCase) &&
               HierarchyLevel == other.HierarchyLevel &&
               Math.Abs(Priority - other.Priority) < 0.001; // Handle floating point comparison
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is UserDefinedHierarchicalConstraintInfo other && Equals(other);
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(ConstraintId),
            HierarchyLevel,
            Priority);
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        return $"Level {HierarchyLevel}: {ConstraintId} (Priority: {Priority:F2}) - {Description}";
    }
    
    /// <summary>
    /// Equality operator for UserDefinedHierarchicalConstraintInfo.
    /// </summary>
    public static bool operator ==(UserDefinedHierarchicalConstraintInfo? left, UserDefinedHierarchicalConstraintInfo? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    /// <summary>
    /// Inequality operator for UserDefinedHierarchicalConstraintInfo.
    /// </summary>
    public static bool operator !=(UserDefinedHierarchicalConstraintInfo? left, UserDefinedHierarchicalConstraintInfo? right)
    {
        return !(left == right);
    }
}