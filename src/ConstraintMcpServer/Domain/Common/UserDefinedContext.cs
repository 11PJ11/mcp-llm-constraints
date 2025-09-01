using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Represents a user-defined context for constraint activation.
/// This class is methodology-agnostic and allows users to define
/// any context categories and values they need for their chosen practices.
/// </summary>
public sealed class UserDefinedContext : IEquatable<UserDefinedContext>
{
    /// <summary>
    /// Gets the user-defined category of this context.
    /// Examples: "phase", "activity", "stage", "priority", "focus-area"
    /// </summary>
    public string Category { get; }
    
    /// <summary>
    /// Gets the user-defined value for this context category.
    /// Examples: "planning", "implementation", "review", "high-priority"
    /// </summary>
    public string Value { get; }
    
    /// <summary>
    /// Gets the user-defined priority for this context (0.0 to 1.0).
    /// Higher values indicate higher priority contexts.
    /// </summary>
    public double Priority { get; }
    
    /// <summary>
    /// Gets additional user-defined metadata for this context.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Initializes a new instance of UserDefinedContext.
    /// </summary>
    /// <param name="category">The user-defined context category.</param>
    /// <param name="value">The user-defined context value.</param>
    /// <param name="priority">The user-defined priority (0.0 to 1.0).</param>
    /// <param name="metadata">Optional user-defined metadata.</param>
    /// <exception cref="ArgumentException">Thrown when category or value is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when priority is out of range.</exception>
    public UserDefinedContext(string category, string value, double priority = 0.5, IReadOnlyDictionary<string, object>? metadata = null)
    {
        ValidationHelpers.ValidateAll(
            () => ValidationHelpers.RequireNonEmptyString(category, nameof(category), "Context category"),
            () => ValidationHelpers.RequireNonEmptyString(value, nameof(value), "Context value"),
            () => ValidationHelpers.RequireValidPriority(priority, nameof(priority))
        );
        
        Category = category.Trim();
        Value = value.Trim();
        Priority = priority;
        Metadata = metadata ?? new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Creates a UserDefinedContext from user configuration input.
    /// </summary>
    /// <param name="category">The user-defined category.</param>
    /// <param name="value">The user-defined value.</param>
    /// <param name="priority">Optional priority (defaults to 0.5).</param>
    /// <returns>A new UserDefinedContext instance.</returns>
    public static UserDefinedContext FromUserConfiguration(string category, string value, double priority = 0.5)
    {
        return new UserDefinedContext(category, value, priority);
    }
    
    /// <summary>
    /// Creates multiple contexts from a user-defined context collection.
    /// </summary>
    /// <param name="contextDefinitions">User-defined context definitions.</param>
    /// <returns>Collection of UserDefinedContext instances.</returns>
    public static IEnumerable<UserDefinedContext> FromUserDefinitions(
        IEnumerable<(string category, string value, double priority)> contextDefinitions)
    {
        return contextDefinitions?.Select(def => new UserDefinedContext(def.category, def.value, def.priority))
               ?? Enumerable.Empty<UserDefinedContext>();
    }
    
    /// <summary>
    /// Checks if this context matches a user-defined pattern.
    /// </summary>
    /// <param name="categoryPattern">User-defined category pattern to match.</param>
    /// <param name="valuePattern">Optional user-defined value pattern to match.</param>
    /// <returns>True if context matches the user-defined patterns.</returns>
    public bool MatchesUserPattern(string categoryPattern, string? valuePattern = null)
    {
        var categoryMatches = string.Equals(Category, categoryPattern, StringComparison.OrdinalIgnoreCase);
        
        if (valuePattern is null)
            return categoryMatches;
        
        var valueMatches = string.Equals(Value, valuePattern, StringComparison.OrdinalIgnoreCase);
        return categoryMatches && valueMatches;
    }
    
    /// <inheritdoc />
    public bool Equals(UserDefinedContext? other)
    {
        return other is not null &&
               string.Equals(Category, other.Category, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is UserDefinedContext other && Equals(other);
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(Category),
            StringComparer.OrdinalIgnoreCase.GetHashCode(Value));
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Category}: {Value} (Priority: {Priority:F2})";
    }
    
    /// <summary>
    /// Equality operator for UserDefinedContext.
    /// </summary>
    public static bool operator ==(UserDefinedContext? left, UserDefinedContext? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    /// <summary>
    /// Inequality operator for UserDefinedContext.
    /// </summary>
    public static bool operator !=(UserDefinedContext? left, UserDefinedContext? right)
    {
        return !(left == right);
    }
}