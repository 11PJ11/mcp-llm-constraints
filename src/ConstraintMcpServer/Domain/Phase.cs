using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain;

/// <summary>
/// Value object representing a TDD/development phase.
/// Defines valid phases for constraint enforcement scheduling.
/// </summary>
public sealed class Phase : IEquatable<Phase>
{
    private static readonly HashSet<string> ValidPhases = new(StringComparer.OrdinalIgnoreCase)
    {
        "kickoff",
        "red", 
        "green",
        "refactor",
        "commit"
    };

    /// <summary>
    /// Gets the string value of the phase.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of Phase.
    /// </summary>
    /// <param name="value">The phase value. Must be a valid phase name.</param>
    /// <exception cref="ValidationException">Thrown when phase value is invalid.</exception>
    public Phase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("Phase cannot be null, empty, or whitespace");
        }

        if (!ValidPhases.Contains(value))
        {
            string validPhasesText = string.Join(", ", ValidPhases.OrderBy(p => p));
            throw new ValidationException($"Invalid phase '{value}'. Valid phases are: {validPhasesText}");
        }

        Value = value.ToLowerInvariant();
    }

    /// <summary>
    /// Gets all valid phase names.
    /// </summary>
    public static IReadOnlyCollection<string> GetValidPhases()
    {
        return ValidPhases.OrderBy(p => p).ToList();
    }

    /// <summary>
    /// Attempts to create a Phase from a string value.
    /// </summary>
    /// <param name="value">The phase value to validate.</param>
    /// <param name="phase">The created Phase if valid.</param>
    /// <returns>True if the phase is valid, false otherwise.</returns>
    public static bool TryCreate(string value, out Phase? phase)
    {
        try
        {
            phase = new Phase(value);
            return true;
        }
        catch (ValidationException)
        {
            phase = null;
            return false;
        }
    }

    /// <inheritdoc />
    public bool Equals(Phase? other)
    {
        return other is not null && Value == other.Value;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Phase other && Equals(other);
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
    /// Equality operator for Phase.
    /// </summary>
    public static bool operator ==(Phase? left, Phase? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    /// <summary>
    /// Inequality operator for Phase.
    /// </summary>
    public static bool operator !=(Phase? left, Phase? right)
    {
        return !(left == right);
    }
}