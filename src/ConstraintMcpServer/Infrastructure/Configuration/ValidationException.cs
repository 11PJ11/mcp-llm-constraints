using System;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Exception thrown when constraint configuration validation fails.
/// </summary>
internal sealed class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ValidationException class.
    /// </summary>
    /// <param name="message">The error message describing the validation failure.</param>
    public ValidationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class.
    /// </summary>
    /// <param name="message">The error message describing the validation failure.</param>
    /// <param name="innerException">The exception that caused this validation failure.</param>
    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
