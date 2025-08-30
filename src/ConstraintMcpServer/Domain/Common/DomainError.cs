using System;
using System.Collections.Immutable;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Base class for all domain errors in the constraint system.
/// Provides immutable error representation with context and categorization.
/// Implements CUPID properties: Predictable, Domain-based.
/// </summary>
public abstract record DomainError
{
    /// <summary>
    /// Gets a unique identifier for this type of error.
    /// </summary>
    public abstract string ErrorCode { get; }

    /// <summary>
    /// Gets a human-readable description of the error.
    /// </summary>
    public abstract string Message { get; }

    /// <summary>
    /// Gets optional context information about the error.
    /// </summary>
    public virtual ImmutableDictionary<string, object> Context { get; init; } =
        ImmutableDictionary<string, object>.Empty;

    /// <summary>
    /// Gets the severity level of this error.
    /// </summary>
    public virtual ErrorSeverity Severity => ErrorSeverity.Error;

    /// <summary>
    /// Creates a new domain error with additional context.
    /// </summary>
    /// <param name="key">Context key</param>
    /// <param name="value">Context value</param>
    /// <returns>A new error with additional context</returns>
    public DomainError WithContext(string key, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        return this with { Context = Context.Add(key, value) };
    }

    /// <summary>
    /// Creates a new domain error with multiple context entries.
    /// </summary>
    /// <param name="contextEntries">Context entries to add</param>
    /// <returns>A new error with additional context</returns>
    public DomainError WithContext(params (string Key, object Value)[] contextEntries)
    {
        ArgumentNullException.ThrowIfNull(contextEntries);

        var newContext = Context;
        foreach (var (key, value) in contextEntries)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(value);
            newContext = newContext.Add(key, value);
        }

        return this with { Context = newContext };
    }

    public override string ToString() =>
        $"{ErrorCode}: {Message}" +
        (Context.IsEmpty ? string.Empty : $" (Context: {string.Join(", ", Context.Select(kv => $"{kv.Key}={kv.Value}"))})");
}

/// <summary>
/// Represents validation errors in domain objects.
/// </summary>
public sealed record ValidationError : DomainError
{
    public override string ErrorCode { get; }
    public override string Message { get; }

    /// <summary>
    /// Gets the field or property that failed validation.
    /// </summary>
    public string? FieldName { get; init; }

    /// <summary>
    /// Gets the value that failed validation.
    /// </summary>
    public object? AttemptedValue { get; init; }

    public ValidationError(string errorCode, string message)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// Creates a validation error for a specific field.
    /// </summary>
    /// <param name="fieldName">Name of the field that failed validation</param>
    /// <param name="message">Validation error message</param>
    /// <param name="attemptedValue">The value that failed validation</param>
    /// <returns>A new validation error</returns>
    public static ValidationError ForField(string fieldName, string message, object? attemptedValue = null) =>
        new("VALIDATION_ERROR", message)
        {
            FieldName = fieldName,
            AttemptedValue = attemptedValue
        };

    /// <summary>
    /// Creates a validation error for missing required field.
    /// </summary>
    /// <param name="fieldName">Name of the missing field</param>
    /// <returns>A new validation error</returns>
    public static ValidationError MissingRequiredField(string fieldName) =>
        ForField(fieldName, $"Field '{fieldName}' is required but was not provided");

    /// <summary>
    /// Creates a validation error for invalid format.
    /// </summary>
    /// <param name="fieldName">Name of the field with invalid format</param>
    /// <param name="expectedFormat">Description of expected format</param>
    /// <param name="attemptedValue">The value that failed validation</param>
    /// <returns>A new validation error</returns>
    public static ValidationError InvalidFormat(string fieldName, string expectedFormat, object? attemptedValue) =>
        ForField(fieldName, $"Field '{fieldName}' has invalid format. Expected: {expectedFormat}", attemptedValue);

    /// <summary>
    /// Creates a validation error for out of range values.
    /// </summary>
    /// <param name="fieldName">Name of the field with out of range value</param>
    /// <param name="minValue">Minimum allowed value</param>
    /// <param name="maxValue">Maximum allowed value</param>
    /// <param name="attemptedValue">The value that failed validation</param>
    /// <returns>A new validation error</returns>
    public static ValidationError OutOfRange(string fieldName, object minValue, object maxValue, object? attemptedValue) =>
        ForField(fieldName, $"Field '{fieldName}' must be between {minValue} and {maxValue}", attemptedValue);
}

/// <summary>
/// Represents parsing errors when converting external data to domain objects.
/// </summary>
public sealed record ParseError : DomainError
{
    public override string ErrorCode => "PARSE_ERROR";
    public override string Message { get; }

    /// <summary>
    /// Gets the source data that failed to parse.
    /// </summary>
    public string? SourceData { get; init; }

    /// <summary>
    /// Gets the expected data format or structure.
    /// </summary>
    public string? ExpectedFormat { get; init; }

    public ParseError(string message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// Creates a parse error for missing required field.
    /// </summary>
    /// <param name="fieldName">Name of the missing field</param>
    /// <returns>A new parse error</returns>
    public static ParseError MissingRequiredField(string fieldName) =>
        new($"Required field '{fieldName}' is missing from input");

    /// <summary>
    /// Creates a parse error for invalid JSON structure.
    /// </summary>
    /// <param name="expectedFormat">Description of expected JSON structure</param>
    /// <param name="sourceData">The JSON data that failed to parse</param>
    /// <returns>A new parse error</returns>
    public static ParseError InvalidJsonStructure(string expectedFormat, string? sourceData = null) =>
        new($"Invalid JSON structure. Expected: {expectedFormat}")
        {
            ExpectedFormat = expectedFormat,
            SourceData = sourceData
        };

    /// <summary>
    /// Creates a parse error for invalid data type.
    /// </summary>
    /// <param name="fieldName">Name of the field with wrong type</param>
    /// <param name="expectedType">Expected data type</param>
    /// <param name="actualType">Actual data type encountered</param>
    /// <returns>A new parse error</returns>
    public static ParseError InvalidDataType(string fieldName, string expectedType, string actualType) =>
        new($"Field '{fieldName}' has wrong data type. Expected: {expectedType}, Actual: {actualType}");
}

/// <summary>
/// Represents errors in business logic operations.
/// </summary>
public sealed record BusinessError : DomainError
{
    public override string ErrorCode { get; }
    public override string Message { get; }
    public ErrorSeverity BusinessSeverity { get; init; } = ErrorSeverity.Error;

    public override ErrorSeverity Severity => BusinessSeverity;

    public BusinessError(string errorCode, string message)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// Creates a business error for invalid operation.
    /// </summary>
    /// <param name="operation">Name of the operation</param>
    /// <param name="reason">Reason why operation is invalid</param>
    /// <returns>A new business error</returns>
    public static BusinessError InvalidOperation(string operation, string reason) =>
        new("INVALID_OPERATION", $"Operation '{operation}' is not valid: {reason}");

    /// <summary>
    /// Creates a business error for resource not found.
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <param name="resourceId">ID of the resource</param>
    /// <returns>A new business error</returns>
    public static BusinessError ResourceNotFound(string resourceType, string resourceId) =>
        new("RESOURCE_NOT_FOUND", $"{resourceType} with ID '{resourceId}' was not found");

    /// <summary>
    /// Creates a business error for constraint violation.
    /// </summary>
    /// <param name="constraintName">Name of the violated constraint</param>
    /// <param name="details">Details about the violation</param>
    /// <returns>A new business error</returns>
    public static BusinessError ConstraintViolation(string constraintName, string details) =>
        new("CONSTRAINT_VIOLATION", $"Constraint '{constraintName}' was violated: {details}")
        {
            BusinessSeverity = ErrorSeverity.Warning
        };
}

/// <summary>
/// Defines severity levels for domain errors.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Informational message, no action required.
    /// </summary>
    Info,

    /// <summary>
    /// Warning condition, may require attention.
    /// </summary>
    Warning,

    /// <summary>
    /// Error condition, operation failed but system can continue.
    /// </summary>
    Error,

    /// <summary>
    /// Critical error, system functionality may be compromised.
    /// </summary>
    Critical
}
