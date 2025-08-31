namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Represents the current phase in the TDD (Test-Driven Development) cycle.
/// Used by Sequential composition to coordinate constraint activation timing.
/// </summary>
public enum TddPhase
{
    /// <summary>
    /// RED phase: Write a failing test first.
    /// Focus on defining behavior and API through test specifications.
    /// </summary>
    Red,

    /// <summary>
    /// GREEN phase: Write the simplest code to make the test pass.
    /// Focus on making the test pass with minimal implementation.
    /// </summary>
    Green,

    /// <summary>
    /// REFACTOR phase: Improve code design while keeping tests green.
    /// Focus on code quality, maintainability, and design patterns.
    /// </summary>
    Refactor
}
