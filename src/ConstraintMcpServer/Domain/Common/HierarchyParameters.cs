using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Parameter object for creating UserDefinedHierarchy instances.
/// Encapsulates all the required parameters to improve readability and maintainability.
/// </summary>
public sealed class HierarchyParameters : IEquatable<HierarchyParameters>
{
    /// <summary>
    /// Gets the user-defined name of this hierarchy.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the user-defined hierarchy levels with their descriptions.
    /// </summary>
    public IReadOnlyDictionary<int, string> Levels { get; }
    
    /// <summary>
    /// Gets the user-defined description of this hierarchy.
    /// </summary>
    public string? Description { get; }
    
    /// <summary>
    /// Gets additional user-defined metadata for this hierarchy.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Initializes a new instance of HierarchyParameters.
    /// </summary>
    /// <param name="name">The user-defined hierarchy name.</param>
    /// <param name="levels">The user-defined hierarchy levels.</param>
    /// <param name="description">Optional user-defined description.</param>
    /// <param name="metadata">Optional user-defined metadata.</param>
    public HierarchyParameters(
        string name,
        IReadOnlyDictionary<int, string> levels,
        string? description = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ValidationHelpers.ValidateAll(
            () => ValidationHelpers.RequireNonEmptyString(name, nameof(name), "Hierarchy name"),
            () => ValidationHelpers.RequireNonEmptyCollection(levels, nameof(levels), "Levels collection")
        );
        
        Name = name.Trim();
        Levels = levels;
        Description = description?.Trim();
        Metadata = metadata ?? new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Creates a parameter object with metadata.
    /// </summary>
    /// <param name="name">The user-defined hierarchy name.</param>
    /// <param name="levels">The user-defined hierarchy levels.</param>
    /// <param name="description">Optional user-defined description.</param>
    /// <param name="metadata">User-defined metadata.</param>
    /// <returns>A new HierarchyParameters instance with metadata.</returns>
    public static HierarchyParameters WithMetadata(
        string name,
        IReadOnlyDictionary<int, string> levels,
        string? description,
        IReadOnlyDictionary<string, object> metadata)
    {
        return new HierarchyParameters(name, levels, description, metadata);
    }
    
    /// <inheritdoc />
    public bool Equals(HierarchyParameters? other)
    {
        return other is not null &&
               string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(Description, other.Description, StringComparison.Ordinal);
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is HierarchyParameters other && Equals(other);
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(Name),
            Description != null ? StringComparer.Ordinal.GetHashCode(Description) : 0);
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        var levelCount = Levels.Count;
        var description = Description is not null ? $" ({Description})" : "";
        return $"{Name}: {levelCount} levels{description}";
    }
    
    /// <summary>
    /// Equality operator for HierarchyParameters.
    /// </summary>
    public static bool operator ==(HierarchyParameters? left, HierarchyParameters? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    /// <summary>
    /// Inequality operator for HierarchyParameters.
    /// </summary>
    public static bool operator !=(HierarchyParameters? left, HierarchyParameters? right)
    {
        return !(left == right);
    }
}