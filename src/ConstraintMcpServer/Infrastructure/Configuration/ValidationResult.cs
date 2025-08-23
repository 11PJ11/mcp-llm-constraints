using System.Collections.Generic;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Represents the result of constraint pack validation.
/// Contains success/failure status and any validation errors.
/// </summary>
internal sealed record ValidationResult
{
    /// <summary>
    /// Indicates whether the validation was successful.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Collection of validation error messages.
    /// Empty when IsValid is true.
    /// </summary>
    public required IReadOnlyList<string> Errors { get; init; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success => new()
    {
        IsValid = true,
        Errors = Array.Empty<string>()
    };

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    public static ValidationResult Failure(IReadOnlyList<string> errors) => new()
    {
        IsValid = false,
        Errors = errors
    };

    /// <summary>
    /// Creates a failed validation result with a single error.
    /// </summary>
    public static ValidationResult Failure(string error) => new()
    {
        IsValid = false,
        Errors = new[] { error }
    };
}
