using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Shared validation utilities for argument validation across domain classes.
/// Provides consistent error messages and validation patterns.
/// </summary>
public static class ArgumentValidation
{
    /// <summary>
    /// Validates that a string argument is not null or empty.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="parameterName">The parameter name for error reporting</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when value is empty</exception>
    public static void RequireNotNullOrEmpty(string value, string parameterName)
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"Parameter '{parameterName}' cannot be empty", parameterName);
        }
    }

    /// <summary>
    /// Validates that a string argument is not null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="parameterName">The parameter name for error reporting</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when value is empty or whitespace</exception>
    public static void RequireNotNullOrWhiteSpace(string value, string parameterName)
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Parameter '{parameterName}' cannot be empty or whitespace", parameterName);
        }
    }

    /// <summary>
    /// Validates that an object argument is not null.
    /// </summary>
    /// <typeparam name="T">The type of the object</typeparam>
    /// <param name="value">The value to validate</param>
    /// <param name="parameterName">The parameter name for error reporting</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static void RequireNotNull<T>(T value, string parameterName) where T : class
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }

    /// <summary>
    /// Validates that a collection is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of collection elements</typeparam>
    /// <param name="value">The collection to validate</param>
    /// <param name="parameterName">The parameter name for error reporting</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when collection is empty</exception>
    public static void RequireNotNullOrEmpty<T>(IEnumerable<T> value, string parameterName)
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (!value.Any())
        {
            throw new ArgumentException($"Parameter '{parameterName}' cannot be empty", parameterName);
        }
    }

    /// <summary>
    /// Validates that a numeric value is within a specified range.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="parameterName">The parameter name for error reporting</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside the range</exception>
    public static void RequireInRange(double value, double min, double max, string parameterName)
    {
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(parameterName, value,
                $"Parameter '{parameterName}' must be between {min} and {max}, but was {value}");
        }
    }

    /// <summary>
    /// Validates that a priority value is within the valid range (0.0 to 1.0).
    /// </summary>
    /// <param name="priority">The priority value to validate</param>
    /// <param name="parameterName">The parameter name for error reporting</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when priority is outside valid range</exception>
    public static void RequireValidPriority(double priority, string parameterName = "priority")
    {
        RequireInRange(priority, 0.0, 1.0, parameterName);
    }

    /// <summary>
    /// Validates that a confidence score is within the valid range (0.0 to 1.0).
    /// </summary>
    /// <param name="confidence">The confidence value to validate</param>
    /// <param name="parameterName">The parameter name for error reporting</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when confidence is outside valid range</exception>
    public static void RequireValidConfidence(double confidence, string parameterName = "confidence")
    {
        RequireInRange(confidence, 0.0, 1.0, parameterName);
    }
}
