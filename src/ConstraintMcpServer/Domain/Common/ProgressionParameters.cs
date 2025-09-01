using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Parameter object for creating UserDefinedProgression instances.
/// Encapsulates all the required parameters to improve readability and maintainability.
/// </summary>
public sealed class ProgressionParameters : IEquatable<ProgressionParameters>
{
    /// <summary>
    /// Gets the user-defined name of this progression.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the user-defined stages in this progression.
    /// </summary>
    public IReadOnlyDictionary<int, ProgressiveStageDefinition> Stages { get; }

    /// <summary>
    /// Gets the user-defined description of this progression.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets whether stage skipping is allowed in this user-defined progression.
    /// </summary>
    public bool AllowStageSkipping { get; }

    /// <summary>
    /// Gets additional user-defined metadata for this progression.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Initializes a new instance of ProgressionParameters.
    /// </summary>
    /// <param name="name">The user-defined progression name.</param>
    /// <param name="stages">The user-defined stages configuration.</param>
    /// <param name="description">Optional user-defined description.</param>
    /// <param name="allowStageSkipping">Whether to allow skipping stages.</param>
    /// <param name="metadata">Optional user-defined metadata.</param>
    public ProgressionParameters(
        string name,
        IReadOnlyDictionary<int, ProgressiveStageDefinition> stages,
        string? description = null,
        bool allowStageSkipping = false,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ValidationHelpers.ValidateAll(
            () => ValidationHelpers.RequireNonEmptyString(name, nameof(name), "Progression name"),
            () => ValidationHelpers.RequireNonEmptyCollection(stages, nameof(stages), "Stages collection")
        );

        Name = name.Trim();
        Stages = stages;
        Description = description?.Trim();
        AllowStageSkipping = allowStageSkipping;
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a parameter object with default stage skipping disabled.
    /// </summary>
    /// <param name="name">The user-defined progression name.</param>
    /// <param name="stages">The user-defined stages configuration.</param>
    /// <param name="description">Optional user-defined description.</param>
    /// <returns>A new ProgressionParameters instance with stage skipping disabled.</returns>
    public static ProgressionParameters WithoutStageSkipping(
        string name,
        IReadOnlyDictionary<int, ProgressiveStageDefinition> stages,
        string? description = null)
    {
        return new ProgressionParameters(name, stages, description, allowStageSkipping: false);
    }

    /// <summary>
    /// Creates a parameter object with stage skipping enabled.
    /// </summary>
    /// <param name="name">The user-defined progression name.</param>
    /// <param name="stages">The user-defined stages configuration.</param>
    /// <param name="description">Optional user-defined description.</param>
    /// <returns>A new ProgressionParameters instance with stage skipping enabled.</returns>
    public static ProgressionParameters WithStageSkipping(
        string name,
        IReadOnlyDictionary<int, ProgressiveStageDefinition> stages,
        string? description = null)
    {
        return new ProgressionParameters(name, stages, description, allowStageSkipping: true);
    }

    /// <inheritdoc />
    public bool Equals(ProgressionParameters? other)
    {
        return other is not null &&
               string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
               AllowStageSkipping == other.AllowStageSkipping &&
               string.Equals(Description, other.Description, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is ProgressionParameters other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(Name),
            AllowStageSkipping,
            Description != null ? StringComparer.Ordinal.GetHashCode(Description) : 0);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var stageCount = Stages.Count;
        var description = Description is not null ? $" ({Description})" : "";
        var skipping = AllowStageSkipping ? " [Skipping Allowed]" : "";
        return $"{Name}: {stageCount} stages{description}{skipping}";
    }

    /// <summary>
    /// Equality operator for ProgressionParameters.
    /// </summary>
    public static bool operator ==(ProgressionParameters? left, ProgressionParameters? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    /// <summary>
    /// Inequality operator for ProgressionParameters.
    /// </summary>
    public static bool operator !=(ProgressionParameters? left, ProgressionParameters? right)
    {
        return !(left == right);
    }
}
