using System.Collections.Generic;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Context;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Tests.Utilities;

/// <summary>
/// Factory for creating test constraints during testing.
/// Provides predefined constraints for unit and integration tests.
/// </summary>
public static class TestConstraintFactory
{
    private const double DefaultPriority = 0.8;

    private static readonly string[] StandardFilePatterns = { "*.cs", "*.js", "*.py" };
    private static readonly string[] DevelopmentContextPatterns =
    {
        "development",
        "implementation",
        "testing",
        "refactoring",
        "feature_development"
    };
    private static readonly string[] EmergencyAntiPatterns = { "hotfix", "urgent", "emergency" };
    internal static readonly string[] stringArray = new[] { "test", "tdd", "failing", "red", "implement", "feature" };

    /// <summary>
    /// Creates a collection of predefined test constraints.
    /// </summary>
    public static IEnumerable<IConstraint> CreateStandardTestConstraints()
    {
        return new List<IConstraint>
        {
            CreateTddTestFirstConstraint(),
            CreateTddSimplestCodeConstraint(),
            CreateRefactoringLevel1Constraint(),
            CreateHexagonalArchitectureConstraint()
        };
    }

    private static IConstraint CreateTddTestFirstConstraint()
    {
        return CreateConstraint(
            id: "tdd.test-first",
            title: "Write a failing test first",
            keywords: stringArray);
    }

    private static IConstraint CreateTddSimplestCodeConstraint()
    {
        return CreateConstraint(
            id: "tdd.simplest-code",
            title: "Write simplest code to pass",
            keywords: new[] { "implement", "green", "pass", "simple" });
    }

    private static IConstraint CreateRefactoringLevel1Constraint()
    {
        return CreateConstraint(
            id: "refactoring.level1",
            title: "Improve readability",
            keywords: new[] { "refactor", "cleanup", "readability", "improve" });
    }

    private static IConstraint CreateHexagonalArchitectureConstraint()
    {
        return CreateConstraint(
            id: "arch.hexagonal",
            title: "Apply hexagonal architecture",
            keywords: new[] { "architecture", "design", "hexagonal" });
    }

    private static IConstraint CreateConstraint(string id, string title, string[] keywords)
    {
        var triggerConfig = new TriggerConfiguration(
            keywords: keywords,
            filePatterns: StandardFilePatterns,
            contextPatterns: DevelopmentContextPatterns,
            antiPatterns: EmergencyAntiPatterns
        );

        return new AtomicConstraint(
            id: new ConstraintId(id),
            title: title,
            priority: DefaultPriority,
            triggers: triggerConfig,
            reminders: new[] { $"Reminder: {title}" }
        );
    }
}
