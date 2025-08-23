using System;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Represents the development phases where constraints can be applied.
/// Provides type safety and validation for phase values.
/// </summary>
internal enum Phase
{
    /// <summary>
    /// Initial project setup and kickoff phase.
    /// </summary>
    Kickoff,

    /// <summary>
    /// Test-driven development red phase (failing test).
    /// </summary>
    Red,

    /// <summary>
    /// Test-driven development green phase (passing test).
    /// </summary>
    Green,

    /// <summary>
    /// Refactoring phase for code improvement.
    /// </summary>
    Refactor,

    /// <summary>
    /// Code commit and version control phase.
    /// </summary>
    Commit
}

/// <summary>
/// Extension methods for Phase enum to provide string conversion and parsing.
/// </summary>
internal static class PhaseExtensions
{


    /// <summary>
    /// Parses a string value to a Phase enum.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The corresponding Phase enum value.</returns>
    /// <exception cref="ArgumentException">Thrown when the value is not a valid phase.</exception>
    public static Phase ParsePhase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Phase value cannot be null, empty, or whitespace", nameof(value));
        }

        return value.ToLowerInvariant() switch
        {
            "kickoff" => Phase.Kickoff,
            "red" => Phase.Red,
            "green" => Phase.Green,
            "refactor" => Phase.Refactor,
            "commit" => Phase.Commit,
            _ => throw new ArgumentException($"Unknown phase value: {value}", nameof(value))
        };
    }


}
