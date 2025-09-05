using System;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Utility class providing fluent builders and factory methods for Result creation.
/// Eliminates duplication in Result&lt;T, E&gt; instantiation patterns across the domain.
/// Implements CUPID properties: Composable, Predictable, Idiomatic.
/// </summary>
public static class Results
{
    /// <summary>
    /// Creates a successful result with the given value.
    /// Provides generic type inference to reduce verbosity.
    /// </summary>
    /// <typeparam name="TSuccess">The success value type</typeparam>
    /// <typeparam name="TError">The error type</typeparam>
    /// <param name="value">The success value</param>
    /// <returns>A successful result</returns>
    public static Result<TSuccess, TError> Ok<TSuccess, TError>(TSuccess value) =>
        Result<TSuccess, TError>.Success(value);

    /// <summary>
    /// Creates a failed result with the given error.
    /// Provides generic type inference to reduce verbosity.
    /// </summary>
    /// <typeparam name="TSuccess">The success value type</typeparam>
    /// <typeparam name="TError">The error type</typeparam>
    /// <param name="error">The error value</param>
    /// <returns>A failed result</returns>
    public static Result<TSuccess, TError> Error<TSuccess, TError>(TError error) =>
        Result<TSuccess, TError>.Failure(error);
}

/// <summary>
/// Specialized result builders for common domain error types.
/// Provides strongly-typed factory methods with consistent naming.
/// </summary>
public static class DomainResults
{
    /// <summary>
    /// Creates a successful result with domain error type.
    /// </summary>
    /// <typeparam name="T">The success value type</typeparam>
    /// <param name="value">The success value</param>
    /// <returns>A successful domain result</returns>
    public static Result<T, DomainError> Ok<T>(T value) =>
        Result<T, DomainError>.Success(value);

    /// <summary>
    /// Creates a failed result with domain error.
    /// </summary>
    /// <typeparam name="T">The success value type</typeparam>
    /// <param name="error">The domain error</param>
    /// <returns>A failed domain result</returns>
    public static Result<T, DomainError> Error<T>(DomainError error) =>
        Result<T, DomainError>.Failure(error);

    /// <summary>
    /// Creates a failed result with validation error.
    /// </summary>
    /// <typeparam name="T">The success value type</typeparam>
    /// <param name="fieldName">Name of the field that failed validation</param>
    /// <param name="message">Validation error message</param>
    /// <param name="attemptedValue">The value that failed validation</param>
    /// <returns>A failed domain result with validation error</returns>
    public static Result<T, DomainError> ValidationError<T>(string fieldName, string message, object? attemptedValue = null) =>
        Result<T, DomainError>.Failure(
            Common.ValidationError.ForField(fieldName, message, attemptedValue));

    /// <summary>
    /// Creates a failed result with business error.
    /// </summary>
    /// <typeparam name="T">The success value type</typeparam>
    /// <param name="errorCode">The business error code</param>
    /// <param name="message">The error message</param>
    /// <returns>A failed domain result with business error</returns>
    public static Result<T, DomainError> BusinessError<T>(string errorCode, string message) =>
        Result<T, DomainError>.Failure(new BusinessError(errorCode, message));

    /// <summary>
    /// Creates a failed result with parse error.
    /// </summary>
    /// <typeparam name="T">The success value type</typeparam>
    /// <param name="message">The parse error message</param>
    /// <returns>A failed domain result with parse error</returns>
    public static Result<T, DomainError> ParseError<T>(string message) =>
        Result<T, DomainError>.Failure(new ParseError(message));
}

/// <summary>
/// Specialized result builders for validation error types.
/// Provides strongly-typed factory methods for validation scenarios.
/// </summary>
public static class ValidationResults
{
    /// <summary>
    /// Creates a successful result with validation error type.
    /// </summary>
    /// <typeparam name="T">The success value type</typeparam>
    /// <param name="value">The success value</param>
    /// <returns>A successful validation result</returns>
    public static Result<T, ValidationError> Ok<T>(T value) =>
        Result<T, ValidationError>.Success(value);

    /// <summary>
    /// Creates a failed result with validation error.
    /// </summary>
    /// <typeparam name="T">The success value type</typeparam>
    /// <param name="fieldName">Name of the field that failed validation</param>
    /// <param name="message">Validation error message</param>
    /// <param name="attemptedValue">The value that failed validation</param>
    /// <returns>A failed validation result</returns>
    public static Result<T, ValidationError> Error<T>(string fieldName, string message, object? attemptedValue = null) =>
        Result<T, ValidationError>.Failure(
            ValidationError.ForField(fieldName, message, attemptedValue));

    /// <summary>
    /// Creates a failed result for missing required field.
    /// </summary>
    /// <typeparam name="T">The success value type</typeparam>
    /// <param name="fieldName">Name of the missing field</param>
    /// <returns>A failed validation result</returns>
    public static Result<T, ValidationError> MissingField<T>(string fieldName) =>
        Result<T, ValidationError>.Failure(ValidationError.MissingRequiredField(fieldName));

    /// <summary>
    /// Creates a failed result for invalid format.
    /// </summary>
    /// <typeparam name="T">The success value type</typeparam>
    /// <param name="fieldName">Name of the field with invalid format</param>
    /// <param name="expectedFormat">Description of expected format</param>
    /// <param name="attemptedValue">The value that failed validation</param>
    /// <returns>A failed validation result</returns>
    public static Result<T, ValidationError> InvalidFormat<T>(string fieldName, string expectedFormat, object? attemptedValue) =>
        Result<T, ValidationError>.Failure(
            ValidationError.InvalidFormat(fieldName, expectedFormat, attemptedValue));

    /// <summary>
    /// Creates a failed result for out of range values.
    /// </summary>
    /// <typeparam name="T">The success value type</typeparam>
    /// <param name="fieldName">Name of the field with out of range value</param>
    /// <param name="minValue">Minimum allowed value</param>
    /// <param name="maxValue">Maximum allowed value</param>
    /// <param name="attemptedValue">The value that failed validation</param>
    /// <returns>A failed validation result</returns>
    public static Result<T, ValidationError> OutOfRange<T>(string fieldName, object minValue, object maxValue, object? attemptedValue) =>
        Result<T, ValidationError>.Failure(
            ValidationError.OutOfRange(fieldName, minValue, maxValue, attemptedValue));
}
