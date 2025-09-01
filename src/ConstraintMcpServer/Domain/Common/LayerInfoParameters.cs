using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Parameter object for creating UserDefinedLayerInfo instances.
/// Encapsulates all the required parameters to improve readability and maintainability.
/// </summary>
public sealed class LayerInfoParameters : IEquatable<LayerInfoParameters>
{
    /// <summary>
    /// Gets the user-defined constraint ID for this layer.
    /// </summary>
    public string ConstraintId { get; }
    
    /// <summary>
    /// Gets the user-defined layer level (priority in hierarchy).
    /// </summary>
    public int LayerLevel { get; }
    
    /// <summary>
    /// Gets the user-defined layer name.
    /// </summary>
    public string LayerName { get; }
    
    /// <summary>
    /// Gets the user-defined description of this layer's purpose.
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// Gets additional user-defined metadata for this layer.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Initializes a new instance of LayerInfoParameters.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="layerLevel">The user-defined layer level.</param>
    /// <param name="layerName">The user-defined layer name.</param>
    /// <param name="description">The user-defined layer description.</param>
    /// <param name="metadata">Optional user-defined metadata.</param>
    public LayerInfoParameters(
        string constraintId,
        int layerLevel,
        string layerName,
        string description,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ValidationHelpers.ValidateAll(
            () => ValidationHelpers.RequireNonEmptyString(constraintId, nameof(constraintId), "Constraint ID"),
            () => ValidationHelpers.RequireNonNegativeLevel(layerLevel, nameof(layerLevel)),
            () => ValidationHelpers.RequireNonEmptyString(layerName, nameof(layerName), "Layer name"),
            () => ValidationHelpers.RequireNonEmptyString(description, nameof(description), "Layer description")
        );
        
        ConstraintId = constraintId.Trim();
        LayerLevel = layerLevel;
        LayerName = layerName.Trim();
        Description = description.Trim();
        Metadata = metadata ?? new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Creates a parameter object with metadata.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="layerLevel">The user-defined layer level.</param>
    /// <param name="layerName">The user-defined layer name.</param>
    /// <param name="description">The user-defined layer description.</param>
    /// <param name="metadata">User-defined metadata.</param>
    /// <returns>A new LayerInfoParameters instance with metadata.</returns>
    public static LayerInfoParameters WithMetadata(
        string constraintId,
        int layerLevel,
        string layerName,
        string description,
        IReadOnlyDictionary<string, object> metadata)
    {
        return new LayerInfoParameters(constraintId, layerLevel, layerName, description, metadata);
    }
    
    /// <inheritdoc />
    public bool Equals(LayerInfoParameters? other)
    {
        return other is not null &&
               string.Equals(ConstraintId, other.ConstraintId, StringComparison.OrdinalIgnoreCase) &&
               LayerLevel == other.LayerLevel &&
               string.Equals(LayerName, other.LayerName, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(Description, other.Description, StringComparison.Ordinal);
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is LayerInfoParameters other && Equals(other);
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(ConstraintId),
            LayerLevel,
            StringComparer.OrdinalIgnoreCase.GetHashCode(LayerName),
            StringComparer.Ordinal.GetHashCode(Description));
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        return $"Layer {LayerLevel}: {LayerName} ({ConstraintId})";
    }
    
    /// <summary>
    /// Equality operator for LayerInfoParameters.
    /// </summary>
    public static bool operator ==(LayerInfoParameters? left, LayerInfoParameters? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    /// <summary>
    /// Inequality operator for LayerInfoParameters.
    /// </summary>
    public static bool operator !=(LayerInfoParameters? left, LayerInfoParameters? right)
    {
        return !(left == right);
    }
}