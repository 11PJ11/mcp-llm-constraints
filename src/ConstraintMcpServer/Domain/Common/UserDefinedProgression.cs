using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Represents a user-defined progressive workflow configuration.
/// This class allows users to define their own progressive stages with custom descriptions,
/// barrier support, and progression rules for any methodology or practice.
/// </summary>
public sealed class UserDefinedProgression : IEquatable<UserDefinedProgression>
{
    /// <summary>
    /// Gets the user-defined name of this progression.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the user-defined description of this progression.
    /// </summary>
    public string? Description { get; }
    
    /// <summary>
    /// Gets the user-defined stages in this progression.
    /// Key is the stage number, Value is the stage definition.
    /// </summary>
    public IReadOnlyDictionary<int, ProgressiveStageDefinition> Stages { get; }
    
    /// <summary>
    /// Gets whether stage skipping is allowed in this user-defined progression.
    /// </summary>
    public bool AllowStageSkipping { get; }
    
    /// <summary>
    /// Gets additional user-defined metadata for this progression.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Initializes a new instance of UserDefinedProgression.
    /// </summary>
    /// <param name="name">The user-defined progression name.</param>
    /// <param name="stages">The user-defined stages configuration.</param>
    /// <param name="description">Optional user-defined description.</param>
    /// <param name="allowStageSkipping">Whether to allow skipping stages.</param>
    /// <param name="metadata">Optional user-defined metadata.</param>
    /// <exception cref="ArgumentException">Thrown when name is invalid or stages are empty.</exception>
    public UserDefinedProgression(
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
        Description = description?.Trim();
        Stages = stages;
        AllowStageSkipping = allowStageSkipping;
        Metadata = metadata ?? new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Initializes a new instance of UserDefinedProgression using parameter object.
    /// </summary>
    /// <param name="parameters">The progression parameters.</param>
    /// <exception cref="ArgumentNullException">Thrown when parameters is null.</exception>
    public UserDefinedProgression(ProgressionParameters parameters)
    {
        ValidationHelpers.RequireNotNull(parameters, nameof(parameters), "Progression parameters");
        
        Name = parameters.Name;
        Description = parameters.Description;
        Stages = parameters.Stages;
        AllowStageSkipping = parameters.AllowStageSkipping;
        Metadata = parameters.Metadata;
    }
    
    /// <summary>
    /// Creates a UserDefinedProgression from user configuration input.
    /// </summary>
    /// <param name="name">The user-defined progression name.</param>
    /// <param name="stageDefinitions">User-defined stage definitions.</param>
    /// <param name="allowStageSkipping">Whether to allow stage skipping.</param>
    /// <returns>A new UserDefinedProgression instance.</returns>
    public static UserDefinedProgression FromUserConfiguration(
        string name,
        IEnumerable<(int stageNumber, string constraintId, string description)> stageDefinitions,
        bool allowStageSkipping = false)
    {
        var stages = new Dictionary<int, ProgressiveStageDefinition>();
        
        foreach (var (stageNumber, constraintId, description) in stageDefinitions)
        {
            stages[stageNumber] = new ProgressiveStageDefinition(
                constraintId, 
                description, 
                isBarrierStage: false, 
                barrierGuidance: new List<string>());
        }
        
        return new UserDefinedProgression(name, stages, allowStageSkipping: allowStageSkipping);
    }
    
    /// <summary>
    /// Creates a UserDefinedProgression with barrier stage support.
    /// </summary>
    /// <param name="name">The user-defined progression name.</param>
    /// <param name="stageDefinitions">User-defined stage definitions with barrier support.</param>
    /// <param name="allowStageSkipping">Whether to allow stage skipping.</param>
    /// <returns>A new UserDefinedProgression instance with barrier support.</returns>
    public static UserDefinedProgression WithBarrierSupport(
        string name,
        IEnumerable<(int stageNumber, string constraintId, string description, bool isBarrier, IEnumerable<string> guidance)> stageDefinitions,
        bool allowStageSkipping = false)
    {
        var stages = new Dictionary<int, ProgressiveStageDefinition>();
        
        foreach (var (stageNumber, constraintId, description, isBarrier, guidance) in stageDefinitions)
        {
            stages[stageNumber] = new ProgressiveStageDefinition(
                constraintId, 
                description, 
                isBarrier, 
                guidance.ToList());
        }
        
        return new UserDefinedProgression(name, stages, allowStageSkipping: allowStageSkipping);
    }
    
    /// <summary>
    /// Gets the first stage number in this progression.
    /// </summary>
    /// <returns>The lowest stage number.</returns>
    public int GetFirstStage()
    {
        return Stages.Keys.Min();
    }
    
    /// <summary>
    /// Gets the last stage number in this progression.
    /// </summary>
    /// <returns>The highest stage number.</returns>
    public int GetLastStage()
    {
        return Stages.Keys.Max();
    }
    
    /// <summary>
    /// Checks if the specified stage exists in this progression.
    /// </summary>
    /// <param name="stageNumber">The stage number to check.</param>
    /// <returns>True if the stage exists in this progression.</returns>
    public bool HasStage(int stageNumber)
    {
        return Stages.ContainsKey(stageNumber);
    }
    
    /// <inheritdoc />
    public bool Equals(UserDefinedProgression? other)
    {
        return other is not null &&
               string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is UserDefinedProgression other && Equals(other);
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        var stageCount = Stages.Count;
        var description = Description is not null ? $" ({Description})" : "";
        return $"{Name}: {stageCount} stages{description}";
    }
    
    /// <summary>
    /// Equality operator for UserDefinedProgression.
    /// </summary>
    public static bool operator ==(UserDefinedProgression? left, UserDefinedProgression? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    /// <summary>
    /// Inequality operator for UserDefinedProgression.
    /// </summary>
    public static bool operator !=(UserDefinedProgression? left, UserDefinedProgression? right)
    {
        return !(left == right);
    }
}