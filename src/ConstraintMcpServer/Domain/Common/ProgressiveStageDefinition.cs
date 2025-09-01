using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Represents a user-defined stage definition in a progressive workflow.
/// This class allows users to define individual stages with their own constraints,
/// descriptions, and barrier support for any methodology or practice.
/// </summary>
public sealed class ProgressiveStageDefinition : IEquatable<ProgressiveStageDefinition>
{
    /// <summary>
    /// Gets the user-defined constraint ID for this stage.
    /// </summary>
    public string ConstraintId { get; }
    
    /// <summary>
    /// Gets the user-defined description of this stage.
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// Gets whether this stage is configured as a barrier stage by the user.
    /// Barrier stages typically require additional support or guidance.
    /// </summary>
    public bool IsBarrierStage { get; }
    
    /// <summary>
    /// Gets the user-defined guidance for this stage when it's a barrier.
    /// </summary>
    public IReadOnlyList<string> BarrierGuidance { get; }
    
    /// <summary>
    /// Gets additional user-defined metadata for this stage.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Initializes a new instance of ProgressiveStageDefinition.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="description">The user-defined stage description.</param>
    /// <param name="isBarrierStage">Whether this is a user-defined barrier stage.</param>
    /// <param name="barrierGuidance">User-defined barrier guidance.</param>
    /// <param name="metadata">Optional user-defined metadata.</param>
    /// <exception cref="ArgumentException">Thrown when constraintId or description is invalid.</exception>
    public ProgressiveStageDefinition(
        string constraintId,
        string description,
        bool isBarrierStage = false,
        IReadOnlyList<string>? barrierGuidance = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(constraintId))
            throw new ArgumentException("Constraint ID cannot be null or empty", nameof(constraintId));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Stage description cannot be null or empty", nameof(description));
        
        ConstraintId = constraintId.Trim();
        Description = description.Trim();
        IsBarrierStage = isBarrierStage;
        BarrierGuidance = barrierGuidance ?? new List<string>();
        Metadata = metadata ?? new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Creates a simple ProgressiveStageDefinition from user input.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="description">The user-defined description.</param>
    /// <returns>A new ProgressiveStageDefinition instance.</returns>
    public static ProgressiveStageDefinition FromUserInput(string constraintId, string description)
    {
        return new ProgressiveStageDefinition(constraintId, description);
    }
    
    /// <summary>
    /// Creates a barrier ProgressiveStageDefinition with user-defined guidance.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="description">The user-defined description.</param>
    /// <param name="barrierGuidance">User-defined barrier guidance.</param>
    /// <returns>A new ProgressiveStageDefinition instance configured as a barrier stage.</returns>
    public static ProgressiveStageDefinition WithBarrierGuidance(
        string constraintId, 
        string description, 
        IEnumerable<string> barrierGuidance)
    {
        return new ProgressiveStageDefinition(
            constraintId, 
            description, 
            isBarrierStage: true, 
            barrierGuidance: barrierGuidance.ToList());
    }
    
    /// <summary>
    /// Creates a ProgressiveStageDefinition with additional user-defined metadata.
    /// </summary>
    /// <param name="constraintId">The user-defined constraint ID.</param>
    /// <param name="description">The user-defined description.</param>
    /// <param name="metadata">User-defined metadata.</param>
    /// <returns>A new ProgressiveStageDefinition instance with metadata.</returns>
    public static ProgressiveStageDefinition WithMetadata(
        string constraintId,
        string description,
        IReadOnlyDictionary<string, object> metadata)
    {
        return new ProgressiveStageDefinition(constraintId, description, metadata: metadata);
    }
    
    /// <inheritdoc />
    public bool Equals(ProgressiveStageDefinition? other)
    {
        return other is not null &&
               string.Equals(ConstraintId, other.ConstraintId, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is ProgressiveStageDefinition other && Equals(other);
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(ConstraintId),
            StringComparer.OrdinalIgnoreCase.GetHashCode(Description));
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        var barrierIndicator = IsBarrierStage ? " [Barrier]" : "";
        return $"{ConstraintId}: {Description}{barrierIndicator}";
    }
    
    /// <summary>
    /// Equality operator for ProgressiveStageDefinition.
    /// </summary>
    public static bool operator ==(ProgressiveStageDefinition? left, ProgressiveStageDefinition? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    /// <summary>
    /// Inequality operator for ProgressiveStageDefinition.
    /// </summary>
    public static bool operator !=(ProgressiveStageDefinition? left, ProgressiveStageDefinition? right)
    {
        return !(left == right);
    }
}