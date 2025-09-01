using System;
using System.Linq;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Common validation helpers for domain objects to eliminate code duplication.
/// Provides consistent validation logic across all user-defined domain types.
/// </summary>
public static class ValidationHelpers
{
    /// <summary>
    /// Validates that a string parameter is not null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <param name="friendlyName">Optional friendly name for the parameter in error messages.</param>
    /// <exception cref="ArgumentException">Thrown when the value is null, empty, or whitespace.</exception>
    public static void RequireNonEmptyString(string? value, string paramName, string? friendlyName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            var displayName = friendlyName ?? paramName;
            throw new ArgumentException($"{displayName} cannot be null or empty", paramName);
        }
    }
    
    /// <summary>
    /// Validates that a priority value is within the valid range (0.0 to 1.0).
    /// </summary>
    /// <param name="priority">The priority value to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when priority is outside valid range.</exception>
    public static void RequireValidPriority(double priority, string paramName)
    {
        if (priority is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(paramName, priority, "Priority must be between 0.0 and 1.0");
    }
    
    /// <summary>
    /// Validates that a hierarchy level is non-negative.
    /// </summary>
    /// <param name="level">The hierarchy level to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when level is negative.</exception>
    public static void RequireNonNegativeLevel(int level, string paramName)
    {
        if (level < 0)
            throw new ArgumentOutOfRangeException(paramName, level, "Level must be non-negative");
    }
    
    /// <summary>
    /// Validates that an object reference is not null.
    /// </summary>
    /// <param name="value">The object reference to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <param name="friendlyName">Optional friendly name for the parameter in error messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public static void RequireNotNull(object? value, string paramName, string? friendlyName = null)
    {
        if (value is null)
        {
            var displayName = friendlyName ?? paramName;
            throw new ArgumentNullException(paramName, $"{displayName} cannot be null");
        }
    }
    
    /// <summary>
    /// Validates that a collection has at least one element.
    /// </summary>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <param name="friendlyName">Optional friendly name for the parameter in error messages.</param>
    /// <exception cref="ArgumentException">Thrown when the collection is empty.</exception>
    public static void RequireNonEmptyCollection<T>(System.Collections.Generic.IEnumerable<T>? collection, string paramName, string? friendlyName = null)
    {
        if (collection is null)
        {
            RequireNotNull(collection, paramName, friendlyName);
            return;
        }
        
        if (!collection.Any())
        {
            var displayName = friendlyName ?? paramName;
            throw new ArgumentException($"{displayName} must contain at least one element", paramName);
        }
    }
    
    /// <summary>
    /// Validates multiple conditions and throws the first validation error encountered.
    /// Useful for complex validation scenarios with multiple requirements.
    /// </summary>
    /// <param name="validations">Array of validation actions to perform.</param>
    public static void ValidateAll(params Action[] validations)
    {
        foreach (var validation in validations)
        {
            validation();
        }
    }
}