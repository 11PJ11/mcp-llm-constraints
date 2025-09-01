using System;

namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Represents errors that can occur during constraint activation.
/// Used to provide detailed error information for composition strategy failures.
/// </summary>
public sealed record ActivationError
{
    /// <summary>
    /// The error code identifying the type of activation error.
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// The context in which the error occurred.
    /// </summary>
    public string? Context { get; init; }

    /// <summary>
    /// Exception that caused this error, if any.
    /// </summary>
    public Exception? InnerException { get; init; }

    /// <summary>
    /// Creates a new activation error.
    /// </summary>
    public ActivationError(string code, string message, string? context = null, Exception? innerException = null)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Context = context;
        InnerException = innerException;
    }

    /// <summary>
    /// Creates an error for invalid composition state.
    /// </summary>
    public static ActivationError InvalidState(string message, string? context = null) =>
        new("INVALID_STATE", message, context);

    /// <summary>
    /// Creates an error for missing configuration.
    /// </summary>
    public static ActivationError MissingConfiguration(string message, string? context = null) =>
        new("MISSING_CONFIGURATION", message, context);

    /// <summary>
    /// Creates an error for validation failure.
    /// </summary>
    public static ActivationError ValidationFailure(string message, string? context = null) =>
        new("VALIDATION_FAILURE", message, context);

    /// <summary>
    /// Creates an error for layer violations.
    /// </summary>
    public static ActivationError LayerViolation(string message, string? context = null) =>
        new("LAYER_VIOLATION", message, context);

    /// <summary>
    /// Creates an error from an exception.
    /// </summary>
    public static ActivationError FromException(Exception exception, string? context = null) =>
        new("EXCEPTION", exception.Message, context, exception);

    /// <summary>
    /// String representation for debugging and logging.
    /// </summary>
    public override string ToString()
    {
        var contextStr = Context != null ? $" (Context: {Context})" : "";
        return $"ActivationError[{Code}]: {Message}{contextStr}";
    }
}
