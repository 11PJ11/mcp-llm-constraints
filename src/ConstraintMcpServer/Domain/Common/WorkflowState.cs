using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Represents a user-defined workflow state in any methodology.
/// This class is completely methodology-agnostic and allows users to define
/// any workflow states they need (e.g., "planning", "coding", "reviewing").
/// </summary>
public sealed class WorkflowState : IEquatable<WorkflowState>
{
    /// <summary>
    /// Gets the user-defined name of the workflow state.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the optional user-defined description of the workflow state.
    /// </summary>
    public string? Description { get; }
    
    /// <summary>
    /// Gets additional user-defined properties for the workflow state.
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties { get; }
    
    /// <summary>
    /// Initializes a new instance of WorkflowState.
    /// </summary>
    /// <param name="name">The user-defined name of the workflow state.</param>
    /// <param name="description">Optional user-defined description.</param>
    /// <param name="properties">Optional user-defined properties.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    public WorkflowState(string name, string? description = null, IReadOnlyDictionary<string, object>? properties = null)
    {
        ValidationHelpers.RequireNonEmptyString(name, nameof(name), "Workflow state name");
        
        Name = name.Trim();
        Description = description?.Trim();
        Properties = properties ?? new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Creates a WorkflowState from user input, performing validation.
    /// </summary>
    /// <param name="name">The user-defined workflow state name.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>A new WorkflowState instance.</returns>
    public static WorkflowState FromUserInput(string name, string? description = null)
    {
        return new WorkflowState(name, description);
    }
    
    /// <summary>
    /// Attempts to create a WorkflowState from user input.
    /// </summary>
    /// <param name="name">The user-defined workflow state name.</param>
    /// <param name="workflowState">The created WorkflowState if successful.</param>
    /// <returns>True if creation was successful, false otherwise.</returns>
    public static bool TryFromUserInput(string name, out WorkflowState? workflowState)
    {
        try
        {
            workflowState = FromUserInput(name);
            return true;
        }
        catch (ArgumentException)
        {
            workflowState = null;
            return false;
        }
    }
    
    /// <inheritdoc />
    public bool Equals(WorkflowState? other)
    {
        return other is not null && 
               string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is WorkflowState other && Equals(other);
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        return Description is not null ? $"{Name} ({Description})" : Name;
    }
    
    /// <summary>
    /// Equality operator for WorkflowState.
    /// </summary>
    public static bool operator ==(WorkflowState? left, WorkflowState? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    /// <summary>
    /// Inequality operator for WorkflowState.
    /// </summary>
    public static bool operator !=(WorkflowState? left, WorkflowState? right)
    {
        return !(left == right);
    }
}