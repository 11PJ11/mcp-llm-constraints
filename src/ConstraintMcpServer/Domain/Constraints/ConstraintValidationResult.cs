namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Domain value object representing the result of constraint validation.
/// Contains success status and detailed error information for failed validations.
/// </summary>
internal sealed record ConstraintValidationResult
{
    /// <summary>
    /// Whether the validation succeeded.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Collection of validation errors (empty if IsValid is true).
    /// </summary>
    public required IReadOnlyList<ValidationError> Errors { get; init; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ConstraintValidationResult Success() => new()
    {
        IsValid = true,
        Errors = Array.Empty<ValidationError>()
    };

    /// <summary>
    /// Creates a failed validation result with the specified errors.
    /// </summary>
    public static ConstraintValidationResult Failure(params ValidationError[] errors) => new()
    {
        IsValid = false,
        Errors = errors
    };

    /// <summary>
    /// Creates a failed validation result with a single error message.
    /// </summary>
    public static ConstraintValidationResult Failure(string errorMessage) => new()
    {
        IsValid = false,
        Errors = new[] { new ValidationError(errorMessage) }
    };
}

/// <summary>
/// Domain value object representing a specific validation error.
/// </summary>
internal sealed record ValidationError
{
    /// <summary>
    /// Human-readable error message describing the validation failure.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Optional property or field name where the error occurred.
    /// </summary>
    public string? PropertyName { get; init; }

    /// <summary>
    /// Optional error code for programmatic error handling.
    /// </summary>
    public string? ErrorCode { get; init; }

    public ValidationError(string message, string? propertyName = null, string? errorCode = null)
    {
        Message = message;
        PropertyName = propertyName;
        ErrorCode = errorCode;
    }
}