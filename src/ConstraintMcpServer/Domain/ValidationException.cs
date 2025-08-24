using System;

namespace ConstraintMcpServer.Domain;

/// <summary>
/// Exception thrown when constraint validation fails.
/// Provides actionable error messages for configuration issues.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of ValidationException with a message.
    /// </summary>
    /// <param name="message">The error message that explains the validation failure.</param>
    public ValidationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of ValidationException with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the validation failure.</param>
    /// <param name="innerException">The exception that caused this validation failure.</param>
    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
