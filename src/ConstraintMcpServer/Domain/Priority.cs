using System;

namespace ConstraintMcpServer.Domain;

/// <summary>
/// Value object representing constraint priority.
/// Valid range is 0.0 to 1.0 inclusive, with 1.0 being highest priority.
/// </summary>
public sealed class Priority : IComparable<Priority>, IEquatable<Priority>
{
    /// <summary>
    /// Gets the numeric priority value (0.0 to 1.0).
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Initializes a new instance of Priority.
    /// </summary>
    /// <param name="value">The priority value. Must be between 0.0 and 1.0 inclusive.</param>
    /// <exception cref="ValidationException">Thrown when priority is outside valid range.</exception>
    public Priority(double value)
    {
        if (value < 0.0 || value > 1.0)
        {
            throw new ValidationException($"Priority must be between 0.0 and 1.0, but got {value}");
        }
        Value = value;
    }

    /// <inheritdoc />
    public int CompareTo(Priority? other)
    {
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }

    /// <inheritdoc />
    public bool Equals(Priority? other)
    {
        return other is not null && Math.Abs(Value - other.Value) < 0.0001;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Priority other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString("F2");
    }

    /// <summary>
    /// Equality operator for Priority.
    /// </summary>
    public static bool operator ==(Priority? left, Priority? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    /// <summary>
    /// Inequality operator for Priority.
    /// </summary>
    public static bool operator !=(Priority? left, Priority? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Greater than operator for Priority.
    /// </summary>
    public static bool operator >(Priority? left, Priority? right)
    {
        return left?.CompareTo(right) > 0;
    }

    /// <summary>
    /// Less than operator for Priority.
    /// </summary>
    public static bool operator <(Priority? left, Priority? right)
    {
        return left?.CompareTo(right) < 0;
    }

    /// <summary>
    /// Greater than or equal operator for Priority.
    /// </summary>
    public static bool operator >=(Priority? left, Priority? right)
    {
        return left?.CompareTo(right) >= 0;
    }

    /// <summary>
    /// Less than or equal operator for Priority.
    /// </summary>
    public static bool operator <=(Priority? left, Priority? right)
    {
        return left?.CompareTo(right) <= 0;
    }
}