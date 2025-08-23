using System;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Represents a constraint priority value between 0.0 and 1.0.
/// Encapsulates validation rules and ensures type safety for priority values.
/// </summary>
internal readonly record struct Priority
{
    private readonly double _value;

    /// <summary>
    /// Gets the priority value.
    /// </summary>
    public double Value => _value;

    /// <summary>
    /// Initializes a new instance of the Priority struct.
    /// </summary>
    /// <param name="value">The priority value between 0.0 and 1.0.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is not between 0.0 and 1.0.</exception>
    public Priority(double value)
    {
        if (value < 0.0 || value > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Priority must be between 0.0 and 1.0");
        }
        _value = value;
    }

}
