using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Tests.Infrastructure;

namespace ConstraintMcpServer.Tests.Models;

/// <summary>
/// Result of constraint activation containing all business validation data.
/// Replaces string searching with structured business validation for E2E tests.
/// This enables actual business logic validation instead of cosmetic validation.
/// </summary>
public sealed class ConstraintActivationResult
{
    /// <summary>
    /// The original context description that was analyzed.
    /// </summary>
    public string Context { get; init; } = string.Empty;

    /// <summary>
    /// The result of context analysis performed by the domain layer.
    /// </summary>
    public ContextAnalysisResult? AnalysisResult { get; init; }

    /// <summary>
    /// The constraints that were activated based on the context analysis.
    /// This is the core business validation data - actual constraints, not mock data.
    /// </summary>
    public IReadOnlyList<IConstraint> ActivatedConstraints { get; init; } = Array.Empty<IConstraint>();

    /// <summary>
    /// When the constraint activation occurred.
    /// </summary>
    public DateTime ActivationTimestamp { get; init; }

    /// <summary>
    /// How long the constraint activation process took.
    /// Used for performance budget validation (sub-50ms requirement).
    /// </summary>
    public TimeSpan ProcessingTime { get; init; }

    /// <summary>
    /// Error message if constraint activation failed.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Gets the number of activated constraints.
    /// Core business validation metric.
    /// </summary>
    public int ConstraintCount => ActivatedConstraints.Count;

    /// <summary>
    /// Gets constraints filtered by category (e.g., "tdd", "refactoring", "architecture").
    /// Enables specific business validation for different development contexts.
    /// </summary>
    /// <param name="category">The constraint category to filter by</param>
    /// <returns>Constraints matching the specified category</returns>
    public IReadOnlyList<IConstraint> GetConstraintsByCategory(string category)
    {
        return ActivatedConstraints
            .Where(c => c.Id.Value.StartsWith($"{category}.", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Checks if any constraint was activated for a specific phase.
    /// Enables validation of TDD phase-specific constraint activation.
    /// </summary>
    /// <param name="phase">The phase to check (e.g., "red", "green", "refactor")</param>
    /// <returns>True if any constraint matches the phase</returns>
    public bool HasConstraintWithPhase(string phase)
    {
        return ActivatedConstraints.Any(c => GetConstraintPhases(c).Contains(phase, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets constraints filtered by phase.
    /// </summary>
    /// <param name="phase">The phase to filter by</param>
    /// <returns>Constraints matching the specified phase</returns>
    public IReadOnlyList<IConstraint> GetConstraintsByPhase(string phase)
    {
        return ActivatedConstraints
            .Where(c => GetConstraintPhases(c).Contains(phase, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Helper method to get phases from a constraint.
    /// Checks if the constraint has phases in metadata or if it's a MockConstraint with the Phases property.
    /// </summary>
    private static IReadOnlyList<string> GetConstraintPhases(IConstraint constraint)
    {
        // Check if it's a mock constraint with Phases property
        if (constraint is MockConstraint mockConstraint)
        {
            return mockConstraint.Phases;
        }

        // Check if phases are stored in metadata
        if (constraint.Metadata.TryGetValue("phases", out var phasesObj) && phasesObj is string[] phases)
        {
            return phases;
        }

        // Default to empty list if no phases found
        return Array.Empty<string>();
    }

    /// <summary>
    /// Gets the highest priority among all activated constraints.
    /// Useful for validating that high-priority constraints are being activated.
    /// </summary>
    /// <returns>The highest priority value, or 0.0 if no constraints</returns>
    public double GetHighestPriority()
    {
        return ActivatedConstraints.DefaultIfEmpty().Max(c => c?.Priority ?? 0.0);
    }

    /// <summary>
    /// Gets the average priority of activated constraints.
    /// </summary>
    public double GetAveragePriority()
    {
        return ActivatedConstraints.Any()
            ? ActivatedConstraints.Average(c => c.Priority)
            : 0.0;
    }

    /// <summary>
    /// Checks if constraint activation was successful.
    /// </summary>
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    /// <summary>
    /// Checks if the constraint activation met performance requirements (sub-50ms).
    /// </summary>
    public bool MeetsPerformanceBudget => ProcessingTime.TotalMilliseconds < 50;

    /// <summary>
    /// Gets constraints that contain specific reminder text.
    /// Enables validation of constraint content quality.
    /// </summary>
    /// <param name="reminderText">Text to search for in reminders</param>
    /// <returns>Constraints containing the specified reminder text</returns>
    public IReadOnlyList<IConstraint> GetConstraintsWithReminder(string reminderText)
    {
        return ActivatedConstraints
            .Where(c => c.Reminders.Any(r => r.Contains(reminderText, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>
    /// Validates that TDD-specific constraints were activated properly.
    /// Business validation method for TDD scenarios.
    /// </summary>
    /// <returns>TDD validation result</returns>
    public TddValidationResult ValidateTddConstraints()
    {
        var tddConstraints = GetConstraintsByCategory("tdd");
        var redPhaseConstraints = GetConstraintsByPhase("red");
        var testRelatedConstraints = GetConstraintsWithReminder("test");

        return new TddValidationResult
        {
            HasTddConstraints = tddConstraints.Count > 0,
            HasRedPhaseConstraints = redPhaseConstraints.Count > 0,
            HasTestGuidance = testRelatedConstraints.Count > 0,
            TddConstraintCount = tddConstraints.Count,
            HighestTddPriority = tddConstraints.DefaultIfEmpty().Max(c => c?.Priority ?? 0.0),
            TddConstraints = tddConstraints
        };
    }

    /// <summary>
    /// Validates that refactoring constraints were activated properly.
    /// </summary>
    public RefactoringValidationResult ValidateRefactoringConstraints()
    {
        var refactoringConstraints = GetConstraintsByCategory("refactoring");
        var solidConstraints = GetConstraintsByCategory("solid");
        var cleanCodeConstraints = GetConstraintsWithReminder("clean");

        return new RefactoringValidationResult
        {
            HasRefactoringConstraints = refactoringConstraints.Count > 0,
            HasSolidPrinciples = solidConstraints.Count > 0,
            HasCleanCodeGuidance = cleanCodeConstraints.Count > 0,
            RefactoringConstraintCount = refactoringConstraints.Count,
            RefactoringConstraints = refactoringConstraints.Concat(solidConstraints).ToList()
        };
    }

    /// <summary>
    /// Gets a summary of the constraint activation for logging and debugging.
    /// </summary>
    public string GetSummary()
    {
        if (!IsSuccess)
        {
            return $"FAILED: {Error}";
        }

        var categories = ActivatedConstraints
            .GroupBy(c => c.Id.Value.Split('.').FirstOrDefault() ?? "unknown")
            .Select(g => $"{g.Key}:{g.Count()}")
            .ToList();

        return $"SUCCESS: {ConstraintCount} constraints activated " +
               $"({string.Join(", ", categories)}) " +
               $"in {ProcessingTime.TotalMilliseconds:F1}ms";
    }
}

/// <summary>
/// Result of TDD-specific constraint validation.
/// </summary>
public sealed class TddValidationResult
{
    public bool HasTddConstraints { get; init; }
    public bool HasRedPhaseConstraints { get; init; }
    public bool HasTestGuidance { get; init; }
    public int TddConstraintCount { get; init; }
    public double HighestTddPriority { get; init; }
    public IReadOnlyList<IConstraint> TddConstraints { get; init; } = Array.Empty<IConstraint>();

    /// <summary>
    /// Checks if TDD constraints are properly activated.
    /// </summary>
    public bool IsValid => HasTddConstraints && HasTestGuidance;

    /// <summary>
    /// Checks if high-priority TDD constraints are activated.
    /// </summary>
    public bool HasHighPriorityTddConstraints => HighestTddPriority >= 0.8;
}

/// <summary>
/// Result of refactoring-specific constraint validation.
/// </summary>
public sealed class RefactoringValidationResult
{
    public bool HasRefactoringConstraints { get; init; }
    public bool HasSolidPrinciples { get; init; }
    public bool HasCleanCodeGuidance { get; init; }
    public int RefactoringConstraintCount { get; init; }
    public IReadOnlyList<IConstraint> RefactoringConstraints { get; init; } = Array.Empty<IConstraint>();

    /// <summary>
    /// Checks if refactoring constraints are properly activated.
    /// </summary>
    public bool IsValid => HasRefactoringConstraints || HasSolidPrinciples;

    /// <summary>
    /// Checks if comprehensive refactoring guidance is provided.
    /// </summary>
    public bool HasComprehensiveGuidance => HasRefactoringConstraints && HasSolidPrinciples && HasCleanCodeGuidance;
}
