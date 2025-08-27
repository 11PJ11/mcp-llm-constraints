using System.Collections.Generic;

namespace ConstraintMcpServer.Domain;

/// <summary>
/// Factory for creating well-known constraint instances.
/// Centralizes constraint creation logic and eliminates duplication across the codebase.
/// </summary>
public static class ConstraintFactory
{
    private static readonly string[] TddReminders = new[] { "Start with a failing test (RED) before implementation.", "Let the test drive the API design and behavior." };
    private static readonly string[] ArchitectureReminders = new[] { "Domain layer: pure business logic, no framework dependencies.", "Use ports (interfaces) to define infrastructure contracts." };
    private static readonly string[] YagniReminders = new[] { "Implement only what's needed right now.", "Avoid speculative generality and over-engineering." };

    /// <summary>
    /// Creates the Test-Driven Development constraint for encouraging failing tests first.
    /// </summary>
    /// <returns>TDD constraint with high priority (0.92) for kickoff, red, and commit phases.</returns>
    public static Constraint CreateTddConstraint()
    {
        return new Constraint(
            new ConstraintId("tdd.test-first"),
            "Write a failing test first",
            new Priority(0.92),
            new[] { new Phase("kickoff"), new Phase("red"), new Phase("commit") },
            TddReminders
        );
    }

    /// <summary>
    /// Creates the Hexagonal Architecture constraint for maintaining domain purity.
    /// </summary>
    /// <returns>Architecture constraint with high priority (0.88) for red, green, and refactor phases.</returns>
    public static Constraint CreateArchitectureConstraint()
    {
        return new Constraint(
            new ConstraintId("arch.hex.domain-pure"),
            "Domain must not depend on Infrastructure",
            new Priority(0.88),
            new[] { new Phase("red"), new Phase("green"), new Phase("refactor") },
            ArchitectureReminders
        );
    }

    /// <summary>
    /// Creates the YAGNI (You Aren't Gonna Need It) constraint for avoiding over-engineering.
    /// </summary>
    /// <returns>YAGNI constraint with medium priority (0.75) for green and refactor phases.</returns>
    public static Constraint CreateYagniConstraint()
    {
        return new Constraint(
            new ConstraintId("quality.yagni"),
            "You Aren't Gonna Need It",
            new Priority(0.75),
            new[] { new Phase("green"), new Phase("refactor") },
            YagniReminders
        );
    }

    /// <summary>
    /// Creates a collection of all walking skeleton constraints for initial implementation.
    /// </summary>
    /// <returns>Read-only collection of core constraints ordered by priority (highest first).</returns>
    public static IReadOnlyList<Constraint> CreateWalkingSkeletonConstraints()
    {
        return new List<Constraint>
        {
            CreateTddConstraint(),        // Priority: 0.92
            CreateArchitectureConstraint(), // Priority: 0.88
            CreateYagniConstraint()       // Priority: 0.75
        }.AsReadOnly();
    }
}
