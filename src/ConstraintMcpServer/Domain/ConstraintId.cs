using System;

namespace ConstraintMcpServer.Domain;

/// <summary>
/// Value object representing a unique constraint identifier.
/// Enforces constraint ID format and uniqueness validation.
/// </summary>
public sealed class ConstraintId : IEquatable<ConstraintId>
{
    /// <summary>
    /// Gets the string value of the constraint ID.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of ConstraintId.
    /// </summary>
    /// <param name="value">The constraint ID value. Cannot be null, empty, or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when value is null, empty, or whitespace.</exception>
    public ConstraintId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Constraint ID cannot be null, empty, or whitespace", nameof(value));
        }
        Value = value;
    }

    /// <inheritdoc />
    public bool Equals(ConstraintId? other)
    {
        return other is not null && Value == other.Value;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is ConstraintId other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    /// Equality operator for ConstraintId.
    /// </summary>
    public static bool operator ==(ConstraintId? left, ConstraintId? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    /// <summary>
    /// Inequality operator for ConstraintId.
    /// </summary>
    public static bool operator !=(ConstraintId? left, ConstraintId? right)
    {
        return !(left == right);
    }
}