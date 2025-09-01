using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Parameter object for creating UserDefinedLayerHierarchy instances.
/// Encapsulates all the required parameters to improve readability and maintainability.
/// </summary>
public sealed class LayerHierarchyParameters : IEquatable<LayerHierarchyParameters>
{
    /// <summary>
    /// Gets the user-defined name of this layer hierarchy.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the user-defined layers in this hierarchy.
    /// </summary>
    public IReadOnlyList<UserDefinedLayerInfo> Layers { get; }

    /// <summary>
    /// Gets the user-defined namespace patterns for layer detection.
    /// </summary>
    public IReadOnlyDictionary<int, IReadOnlyList<string>> NamespacePatterns { get; }

    /// <summary>
    /// Gets the user-defined dependency rules between layers.
    /// </summary>
    public IReadOnlyDictionary<int, IReadOnlyList<int>> AllowedDependencies { get; }

    /// <summary>
    /// Gets the user-defined description of this layer hierarchy.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets additional user-defined metadata for this hierarchy.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Initializes a new instance of LayerHierarchyParameters.
    /// </summary>
    /// <param name="name">The user-defined hierarchy name.</param>
    /// <param name="layers">The user-defined layers.</param>
    /// <param name="namespacePatterns">User-defined namespace patterns for layer detection.</param>
    /// <param name="allowedDependencies">User-defined dependency rules.</param>
    /// <param name="description">Optional user-defined description.</param>
    /// <param name="metadata">Optional user-defined metadata.</param>
    public LayerHierarchyParameters(
        string name,
        IReadOnlyList<UserDefinedLayerInfo> layers,
        IReadOnlyDictionary<int, IReadOnlyList<string>> namespacePatterns,
        IReadOnlyDictionary<int, IReadOnlyList<int>> allowedDependencies,
        string? description = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ValidationHelpers.ValidateAll(
            () => ValidationHelpers.RequireNonEmptyString(name, nameof(name), "Layer hierarchy name"),
            () => ValidationHelpers.RequireNonEmptyCollection(layers, nameof(layers), "Layers collection")
        );

        Name = name.Trim();
        Layers = layers;
        NamespacePatterns = namespacePatterns ?? new Dictionary<int, IReadOnlyList<string>>();
        AllowedDependencies = allowedDependencies ?? new Dictionary<int, IReadOnlyList<int>>();
        Description = description?.Trim();
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a parameter object with metadata.
    /// </summary>
    /// <param name="name">The user-defined hierarchy name.</param>
    /// <param name="layers">The user-defined layers.</param>
    /// <param name="namespacePatterns">User-defined namespace patterns.</param>
    /// <param name="allowedDependencies">User-defined dependency rules.</param>
    /// <param name="description">Optional user-defined description.</param>
    /// <param name="metadata">User-defined metadata.</param>
    /// <returns>A new LayerHierarchyParameters instance with metadata.</returns>
    public static LayerHierarchyParameters WithMetadata(
        string name,
        IReadOnlyList<UserDefinedLayerInfo> layers,
        IReadOnlyDictionary<int, IReadOnlyList<string>> namespacePatterns,
        IReadOnlyDictionary<int, IReadOnlyList<int>> allowedDependencies,
        string? description,
        IReadOnlyDictionary<string, object> metadata)
    {
        return new LayerHierarchyParameters(name, layers, namespacePatterns, allowedDependencies, description, metadata);
    }

    /// <summary>
    /// Creates a parameter object with basic configuration (no namespace patterns or dependencies).
    /// </summary>
    /// <param name="name">The user-defined hierarchy name.</param>
    /// <param name="layers">The user-defined layers.</param>
    /// <param name="description">Optional user-defined description.</param>
    /// <returns>A new LayerHierarchyParameters instance with basic configuration.</returns>
    public static LayerHierarchyParameters WithBasicConfiguration(
        string name,
        IReadOnlyList<UserDefinedLayerInfo> layers,
        string? description = null)
    {
        return new LayerHierarchyParameters(
            name,
            layers,
            new Dictionary<int, IReadOnlyList<string>>(),
            new Dictionary<int, IReadOnlyList<int>>(),
            description);
    }

    /// <inheritdoc />
    public bool Equals(LayerHierarchyParameters? other)
    {
        return other is not null &&
               string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(Description, other.Description, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is LayerHierarchyParameters other && Equals(other);
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
        var layerCount = Layers.Count;
        var description = Description is not null ? $" ({Description})" : "";
        return $"{Name}: {layerCount} layers{description}";
    }

    /// <summary>
    /// Equality operator for LayerHierarchyParameters.
    /// </summary>
    public static bool operator ==(LayerHierarchyParameters? left, LayerHierarchyParameters? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    /// <summary>
    /// Inequality operator for LayerHierarchyParameters.
    /// </summary>
    public static bool operator !=(LayerHierarchyParameters? left, LayerHierarchyParameters? right)
    {
        return !(left == right);
    }
}
