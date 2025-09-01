using System.Collections.Generic;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain;

/// <summary>
/// Defines business rules for standard constraint types.
/// Centralizes priority values, contexts, and business logic for constraint creation.
/// </summary>
public static class ConstraintBusinessRules
{
    /// <summary>
    /// Business rule: TDD should have the highest priority as it drives development practices.
    /// </summary>
    public static double GetTddPriority() => 0.92;

    /// <summary>
    /// Business rule: Architecture constraints have high priority but below TDD fundamentals.
    /// </summary>
    public static double GetHexagonalArchitecturePriority() => 0.88;

    /// <summary>
    /// Business rule: YAGNI has medium priority, applied during implementation phases.
    /// </summary>
    public static double GetYagniPriority() => 0.75;

    /// <summary>
    /// Business rule: TDD applies to kickoff, red (test-first), and commit phases with high criticality.
    /// </summary>
    public static IReadOnlyList<UserDefinedContext> GetTddContexts()
    {
        return new List<UserDefinedContext>
        {
            new("workflow", "kickoff", 0.9),
            new("workflow", "red", 0.9),
            new("workflow", "commit", 0.9)
        }.AsReadOnly();
    }

    /// <summary>
    /// Business rule: Architecture constraints apply throughout development cycle with standard priority.
    /// </summary>
    public static IReadOnlyList<UserDefinedContext> GetHexagonalArchitectureContexts()
    {
        return new List<UserDefinedContext>
        {
            new("workflow", "red", 0.8),
            new("workflow", "green", 0.8),
            new("workflow", "refactor", 0.8)
        }.AsReadOnly();
    }

    /// <summary>
    /// Business rule: YAGNI applies during implementation and refactoring with medium priority.
    /// </summary>
    public static IReadOnlyList<UserDefinedContext> GetYagniContexts()
    {
        return new List<UserDefinedContext>
        {
            new("workflow", "green", 0.7),
            new("workflow", "refactor", 0.7)
        }.AsReadOnly();
    }

    /// <summary>
    /// Business rule: Standard constraint IDs follow hierarchical naming convention.
    /// </summary>
    public static class StandardConstraintIds
    {
        public const string TddTestFirst = "tdd.test-first";
        public const string HexagonalArchitecture = "arch.hex.domain-pure";
        public const string YagniPrinciple = "quality.yagni";
    }

    /// <summary>
    /// Business rule: Standard constraint titles for consistent messaging.
    /// </summary>
    public static class StandardConstraintTitles
    {
        public const string TddTestFirst = "Write a failing test first";
        public const string HexagonalArchitecture = "Domain must not depend on Infrastructure";
        public const string YagniPrinciple = "You Aren't Gonna Need It";
    }
}
