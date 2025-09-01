using System.Collections.Generic;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain;

/// <summary>
/// Factory for creating well-known constraint instances with separated concerns.
/// Uses dependency injection for reminders and business rules for clean separation.
/// </summary>
public sealed class StandardConstraintFactory
{
    private readonly IConstraintReminderProvider _reminderProvider;

    /// <summary>
    /// Initializes a new instance with the specified reminder provider.
    /// </summary>
    /// <param name="reminderProvider">Provider for constraint reminder text.</param>
    public StandardConstraintFactory(IConstraintReminderProvider reminderProvider)
    {
        ValidationHelpers.RequireNotNull(reminderProvider, nameof(reminderProvider), "Reminder provider");
        _reminderProvider = reminderProvider;
    }

    /// <summary>
    /// Creates a new instance with the standard reminder provider.
    /// </summary>
    public static StandardConstraintFactory WithStandardReminders()
    {
        return new StandardConstraintFactory(new StandardConstraintReminderProvider());
    }

    /// <summary>
    /// Creates the Test-Driven Development constraint for encouraging failing tests first.
    /// </summary>
    /// <returns>TDD constraint with high priority for kickoff, red, and commit phases.</returns>
    public Constraint CreateTddConstraint()
    {
        return new Constraint(
            new ConstraintId(ConstraintBusinessRules.StandardConstraintIds.TddTestFirst),
            ConstraintBusinessRules.StandardConstraintTitles.TddTestFirst,
            new Priority(ConstraintBusinessRules.GetTddPriority()),
            ConstraintBusinessRules.GetTddContexts(),
            _reminderProvider.GetReminders("tdd")
        );
    }

    /// <summary>
    /// Creates the Hexagonal Architecture constraint for maintaining domain purity.
    /// </summary>
    /// <returns>Architecture constraint with high priority for red, green, and refactor phases.</returns>
    public Constraint CreateArchitectureConstraint()
    {
        return new Constraint(
            new ConstraintId(ConstraintBusinessRules.StandardConstraintIds.HexagonalArchitecture),
            ConstraintBusinessRules.StandardConstraintTitles.HexagonalArchitecture,
            new Priority(ConstraintBusinessRules.GetHexagonalArchitecturePriority()),
            ConstraintBusinessRules.GetHexagonalArchitectureContexts(),
            _reminderProvider.GetReminders("architecture")
        );
    }

    /// <summary>
    /// Creates the YAGNI (You Aren't Gonna Need It) constraint for avoiding over-engineering.
    /// </summary>
    /// <returns>YAGNI constraint with medium priority for green and refactor phases.</returns>
    public Constraint CreateYagniConstraint()
    {
        return new Constraint(
            new ConstraintId(ConstraintBusinessRules.StandardConstraintIds.YagniPrinciple),
            ConstraintBusinessRules.StandardConstraintTitles.YagniPrinciple,
            new Priority(ConstraintBusinessRules.GetYagniPriority()),
            ConstraintBusinessRules.GetYagniContexts(),
            _reminderProvider.GetReminders("yagni")
        );
    }

    /// <summary>
    /// Creates a collection of all walking skeleton constraints for initial implementation.
    /// </summary>
    /// <returns>Read-only collection of core constraints ordered by priority (highest first).</returns>
    public IReadOnlyList<Constraint> CreateWalkingSkeletonConstraints()
    {
        return new List<Constraint>
        {
            CreateTddConstraint(),        // Highest priority: Test-first development
            CreateArchitectureConstraint(), // High priority: Domain purity
            CreateYagniConstraint()       // Medium priority: Avoid over-engineering
        }.AsReadOnly();
    }
}

/// <summary>
/// Backward compatibility static facade for existing consumers.
/// Uses the standard factory implementation internally.
/// </summary>
public static class ConstraintFactory
{
    private static readonly StandardConstraintFactory _standardFactory = StandardConstraintFactory.WithStandardReminders();

    /// <summary>
    /// Creates the Test-Driven Development constraint for encouraging failing tests first.
    /// </summary>
    /// <returns>TDD constraint with high priority for kickoff, red, and commit phases.</returns>
    public static Constraint CreateTddConstraint() => _standardFactory.CreateTddConstraint();

    /// <summary>
    /// Creates the Hexagonal Architecture constraint for maintaining domain purity.
    /// </summary>
    /// <returns>Architecture constraint with high priority for red, green, and refactor phases.</returns>
    public static Constraint CreateArchitectureConstraint() => _standardFactory.CreateArchitectureConstraint();

    /// <summary>
    /// Creates the YAGNI (You Aren't Gonna Need It) constraint for avoiding over-engineering.
    /// </summary>
    /// <returns>YAGNI constraint with medium priority for green and refactor phases.</returns>
    public static Constraint CreateYagniConstraint() => _standardFactory.CreateYagniConstraint();

    /// <summary>
    /// Creates a collection of all walking skeleton constraints for initial implementation.
    /// </summary>
    /// <returns>Read-only collection of core constraints ordered by priority (highest first).</returns>
    public static IReadOnlyList<Constraint> CreateWalkingSkeletonConstraints() => _standardFactory.CreateWalkingSkeletonConstraints();
}