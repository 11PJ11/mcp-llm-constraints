using System;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Represents a constraint identifier with validation rules.
/// Ensures constraint IDs follow the expected format and are not empty.
/// </summary>
internal readonly record struct ConstraintId
{
    private readonly string _value;

    /// <summary>
    /// Gets the constraint identifier value.
    /// </summary>
    public string Value => _value;

    /// <summary>
    /// Initializes a new instance of the ConstraintId struct.
    /// </summary>
    /// <param name="value">The constraint identifier value.</param>
    /// <exception cref="ArgumentException">Thrown when value is null, empty, or whitespace.</exception>
    public ConstraintId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Constraint ID cannot be null, empty, or whitespace", nameof(value));
        }
        _value = value;
    }

}
