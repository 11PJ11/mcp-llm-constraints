using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Represents information about a user-defined layer in a layered architecture.
/// This class allows users to define individual layers with their own constraints,
/// descriptions, and properties for any layered architecture methodology.
/// </summary>
public sealed class UserDefinedLayerInfo : IEquatable<UserDefinedLayerInfo>
{
    /// <summary>
    /// Gets the user-defined constraint ID for this layer.
    /// </summary>
    public string ConstraintId { get; }
    
    /// <summary>
    /// Gets the user-defined layer level (priority in hierarchy).
    /// Lower numbers typically represent inner layers.
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
    /// Initializes a new instance of UserDefinedLayerInfo.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="layerLevel">The user-defined layer level.</param>
    /// <param name="layerName">The user-defined layer name.</param>
    /// <param name="description">The user-defined layer description.</param>
    /// <param name="metadata">Optional user-defined metadata.</param>
    /// <exception cref="ArgumentException">Thrown when required parameters are invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when layerLevel is negative.</exception>
    public UserDefinedLayerInfo(
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
    /// Initializes a new instance of UserDefinedLayerInfo using parameter object.
    /// </summary>
    /// <param name="parameters">The layer info parameters.</param>
    /// <exception cref="ArgumentNullException">Thrown when parameters is null.</exception>
    public UserDefinedLayerInfo(LayerInfoParameters parameters)
    {
        ValidationHelpers.RequireNotNull(parameters, nameof(parameters), "Layer info parameters");
        
        ConstraintId = parameters.ConstraintId;
        LayerLevel = parameters.LayerLevel;
        LayerName = parameters.LayerName;
        Description = parameters.Description;
        Metadata = parameters.Metadata;
    }
    
    /// <summary>
    /// Creates a UserDefinedLayerInfo from user input with validation.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="layerLevel">The user-defined layer level.</param>
    /// <param name="layerName">The user-defined layer name.</param>
    /// <param name="description">The user-defined layer description.</param>
    /// <returns>A new UserDefinedLayerInfo instance.</returns>
    public static UserDefinedLayerInfo Create(string constraintId, int layerLevel, string layerName, string description)
    {
        return new UserDefinedLayerInfo(constraintId, layerLevel, layerName, description);
    }
    
    /// <summary>
    /// Creates a UserDefinedLayerInfo from parameter object.
    /// </summary>
    /// <param name="parameters">The layer info parameters.</param>
    /// <returns>A new UserDefinedLayerInfo instance.</returns>
    public static UserDefinedLayerInfo Create(LayerInfoParameters parameters)
    {
        return new UserDefinedLayerInfo(parameters);
    }
    
    /// <summary>
    /// Creates a UserDefinedLayerInfo with additional user-defined metadata.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="layerLevel">The user-defined layer level.</param>
    /// <param name="layerName">The user-defined layer name.</param>
    /// <param name="description">The user-defined layer description.</param>
    /// <param name="metadata">User-defined metadata.</param>
    /// <returns>A new UserDefinedLayerInfo instance with metadata.</returns>
    public static UserDefinedLayerInfo WithMetadata(
        string constraintId,
        int layerLevel,
        string layerName,
        string description,
        IReadOnlyDictionary<string, object> metadata)
    {
        return new UserDefinedLayerInfo(constraintId, layerLevel, layerName, description, metadata);
    }
    
    /// <summary>
    /// Attempts to create a UserDefinedLayerInfo from user input.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="layerLevel">The user-defined layer level.</param>
    /// <param name="layerName">The user-defined layer name.</param>
    /// <param name="description">The user-defined layer description.</param>
    /// <param name="layerInfo">The created UserDefinedLayerInfo if successful.</param>
    /// <returns>True if creation was successful, false otherwise.</returns>
    public static bool TryCreate(
        string constraintId,
        int layerLevel,
        string layerName,
        string description,
        out UserDefinedLayerInfo? layerInfo)
    {
        try
        {
            layerInfo = Create(constraintId, layerLevel, layerName, description);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            layerInfo = null;
            return false;
        }
        catch (ArgumentException)
        {
            layerInfo = null;
            return false;
        }
    }
    
    /// <inheritdoc />
    public bool Equals(UserDefinedLayerInfo? other)
    {
        return other is not null &&
               string.Equals(ConstraintId, other.ConstraintId, StringComparison.OrdinalIgnoreCase) &&
               LayerLevel == other.LayerLevel &&
               string.Equals(LayerName, other.LayerName, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is UserDefinedLayerInfo other && Equals(other);
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(ConstraintId),
            LayerLevel,
            StringComparer.OrdinalIgnoreCase.GetHashCode(LayerName));
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        return $"Layer {LayerLevel}: {LayerName} ({ConstraintId})";
    }
    
    /// <summary>
    /// Equality operator for UserDefinedLayerInfo.
    /// </summary>
    public static bool operator ==(UserDefinedLayerInfo? left, UserDefinedLayerInfo? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    /// <summary>
    /// Inequality operator for UserDefinedLayerInfo.
    /// </summary>
    public static bool operator !=(UserDefinedLayerInfo? left, UserDefinedLayerInfo? right)
    {
        return !(left == right);
    }
}