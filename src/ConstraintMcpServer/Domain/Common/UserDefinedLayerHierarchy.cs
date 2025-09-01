using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Represents a user-defined layer hierarchy for layered architectural patterns.
/// This class allows users to define their own layer structures, dependency rules,
/// and namespace mappings for any layered architecture methodology.
/// </summary>
public sealed class UserDefinedLayerHierarchy : IEquatable<UserDefinedLayerHierarchy>
{
    /// <summary>
    /// Gets the user-defined name of this layer hierarchy.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the user-defined description of this layer hierarchy.
    /// </summary>
    public string? Description { get; }
    
    /// <summary>
    /// Gets the user-defined layers in this hierarchy.
    /// </summary>
    public IReadOnlyList<UserDefinedLayerInfo> Layers { get; }
    
    /// <summary>
    /// Gets the user-defined namespace patterns for layer detection.
    /// Key is the layer level, Value is the list of namespace patterns.
    /// </summary>
    public IReadOnlyDictionary<int, IReadOnlyList<string>> NamespacePatterns { get; }
    
    /// <summary>
    /// Gets the user-defined dependency rules between layers.
    /// Key is the source layer, Value is the list of allowed target layers.
    /// </summary>
    public IReadOnlyDictionary<int, IReadOnlyList<int>> AllowedDependencies { get; }
    
    /// <summary>
    /// Gets additional user-defined metadata for this hierarchy.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Initializes a new instance of UserDefinedLayerHierarchy.
    /// </summary>
    /// <param name="name">The user-defined hierarchy name.</param>
    /// <param name="layers">The user-defined layers.</param>
    /// <param name="namespacePatterns">User-defined namespace patterns for layer detection.</param>
    /// <param name="allowedDependencies">User-defined dependency rules.</param>
    /// <param name="description">Optional user-defined description.</param>
    /// <param name="metadata">Optional user-defined metadata.</param>
    /// <exception cref="ArgumentException">Thrown when name is invalid or layers are empty.</exception>
    public UserDefinedLayerHierarchy(
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
        Description = description?.Trim();
        Layers = layers;
        NamespacePatterns = namespacePatterns ?? new Dictionary<int, IReadOnlyList<string>>();
        AllowedDependencies = allowedDependencies ?? new Dictionary<int, IReadOnlyList<int>>();
        Metadata = metadata ?? new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Initializes a new instance of UserDefinedLayerHierarchy using parameter object.
    /// </summary>
    /// <param name="parameters">The layer hierarchy parameters.</param>
    /// <exception cref="ArgumentNullException">Thrown when parameters is null.</exception>
    public UserDefinedLayerHierarchy(LayerHierarchyParameters parameters)
    {
        ValidationHelpers.RequireNotNull(parameters, nameof(parameters), "Layer hierarchy parameters");
        
        Name = parameters.Name;
        Description = parameters.Description;
        Layers = parameters.Layers;
        NamespacePatterns = parameters.NamespacePatterns;
        AllowedDependencies = parameters.AllowedDependencies;
        Metadata = parameters.Metadata;
    }
    
    /// <summary>
    /// Creates a UserDefinedLayerHierarchy from simple user configuration.
    /// </summary>
    /// <param name="name">The user-defined hierarchy name.</param>
    /// <param name="layerDefinitions">User-defined layer definitions.</param>
    /// <returns>A new UserDefinedLayerHierarchy instance.</returns>
    public static UserDefinedLayerHierarchy FromSimpleConfiguration(
        string name,
        IEnumerable<(int level, string layerName, string description, string[] namespacePatterns)> layerDefinitions)
    {
        var layers = new List<UserDefinedLayerInfo>();
        var namespacePatterns = new Dictionary<int, IReadOnlyList<string>>();
        var allowedDependencies = new Dictionary<int, IReadOnlyList<int>>();
        
        var orderedDefinitions = layerDefinitions.OrderBy(def => def.level).ToList();
        
        foreach (var (level, layerName, description, patterns) in orderedDefinitions)
        {
            layers.Add(new UserDefinedLayerInfo(
                $"layer.{layerName.ToLower()}",
                level,
                layerName,
                description));
                
            namespacePatterns[level] = patterns.ToList();
            
            // By default, inner layers can depend on outer layers (lower level -> higher levels)
            allowedDependencies[level] = orderedDefinitions
                .Where(def => def.level > level)
                .Select(def => def.level)
                .ToList();
        }
        
        return new UserDefinedLayerHierarchy(name, layers, namespacePatterns, allowedDependencies);
    }
    
    /// <summary>
    /// Determines the layer level for a given namespace based on user-defined patterns.
    /// </summary>
    /// <param name="namespaceName">The namespace to analyze.</param>
    /// <returns>The layer level, or the outermost layer if no pattern matches.</returns>
    public int DetermineLayerFromNamespace(string namespaceName)
    {
        var lowerNamespace = namespaceName.ToLowerInvariant();
        
        foreach (var (layerLevel, patterns) in NamespacePatterns.OrderBy(kvp => kvp.Key))
        {
            if (patterns.Any(pattern => lowerNamespace.Contains(pattern.ToLowerInvariant())))
            {
                return layerLevel;
            }
        }
        
        // Default to outermost layer if no pattern matches
        return GetOuterMostLayer();
    }
    
    /// <summary>
    /// Checks if a dependency between two layers violates user-defined rules.
    /// </summary>
    /// <param name="sourceLayer">The source layer level.</param>
    /// <param name="targetLayer">The target layer level.</param>
    /// <returns>True if this dependency is a violation according to user configuration.</returns>
    public bool IsViolation(int sourceLayer, int targetLayer)
    {
        if (!AllowedDependencies.ContainsKey(sourceLayer))
        {
            // If no rules defined for source layer, assume any dependency is allowed
            return false;
        }
        
        return !AllowedDependencies[sourceLayer].Contains(targetLayer);
    }
    
    /// <summary>
    /// Gets the allowed dependencies for a given layer.
    /// </summary>
    /// <param name="layerLevel">The layer level to check.</param>
    /// <returns>List of layer levels that this layer is allowed to depend on.</returns>
    public IReadOnlyList<int> GetAllowedDependencies(int layerLevel)
    {
        return AllowedDependencies.ContainsKey(layerLevel) 
            ? AllowedDependencies[layerLevel] 
            : new List<int>();
    }
    
    /// <summary>
    /// Gets the user-defined name for a layer level.
    /// </summary>
    /// <param name="layerLevel">The layer level.</param>
    /// <returns>The user-defined layer name.</returns>
    public string GetLayerName(int layerLevel)
    {
        var layer = Layers.FirstOrDefault(l => l.LayerLevel == layerLevel);
        return layer?.LayerName ?? $"Layer {layerLevel}";
    }
    
    /// <summary>
    /// Gets the innermost (lowest level) layer in this hierarchy.
    /// </summary>
    /// <returns>The innermost layer level.</returns>
    public int GetInnerMostLayer()
    {
        return Layers.Min(l => l.LayerLevel);
    }
    
    /// <summary>
    /// Gets the outermost (highest level) layer in this hierarchy.
    /// </summary>
    /// <returns>The outermost layer level.</returns>
    public int GetOuterMostLayer()
    {
        return Layers.Max(l => l.LayerLevel);
    }
    
    /// <summary>
    /// Gets the next layer in the hierarchy after the specified layer.
    /// </summary>
    /// <param name="currentLayer">The current layer level.</param>
    /// <returns>The next layer level, or current if it's the outermost.</returns>
    public int GetNextLayer(int currentLayer)
    {
        var orderedLayers = Layers.OrderBy(l => l.LayerLevel).ToList();
        var currentIndex = orderedLayers.FindIndex(l => l.LayerLevel == currentLayer);
        
        if (currentIndex >= 0 && currentIndex < orderedLayers.Count - 1)
        {
            return orderedLayers[currentIndex + 1].LayerLevel;
        }
        
        return currentLayer; // Return current if no next layer
    }
    
    /// <summary>
    /// Checks if the specified layer level exists in this hierarchy.
    /// </summary>
    /// <param name="layerLevel">The layer level to check.</param>
    /// <returns>True if the layer exists in this hierarchy.</returns>
    public bool HasLayer(int layerLevel)
    {
        return Layers.Any(l => l.LayerLevel == layerLevel);
    }
    
    /// <inheritdoc />
    public bool Equals(UserDefinedLayerHierarchy? other)
    {
        return other is not null &&
               string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is UserDefinedLayerHierarchy other && Equals(other);
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        var layerCount = Layers.Count;
        var description = Description is not null ? $" ({Description})" : "";
        return $"{Name}: {layerCount} layers{description}";
    }
    
    /// <summary>
    /// Equality operator for UserDefinedLayerHierarchy.
    /// </summary>
    public static bool operator ==(UserDefinedLayerHierarchy? left, UserDefinedLayerHierarchy? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    /// <summary>
    /// Inequality operator for UserDefinedLayerHierarchy.
    /// </summary>
    public static bool operator !=(UserDefinedLayerHierarchy? left, UserDefinedLayerHierarchy? right)
    {
        return !(left == right);
    }
}