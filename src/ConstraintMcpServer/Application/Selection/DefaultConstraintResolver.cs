using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Application.Selection;

/// <summary>
/// Default constraint resolver implementation for testing and development.
/// Provides basic TDD constraints to support Step A2 integration testing.
/// </summary>
internal sealed class DefaultConstraintResolver : IConstraintResolver
{
    private static readonly IReadOnlyList<IConstraint> DefaultConstraints = new List<IConstraint>
    {
        // TDD test-first constraint for integration testing
        new AtomicConstraint(
            id: new ConstraintId("tdd.test-first"),
            title: "Write failing test first",
            priority: 0.9,
            triggers: new TriggerConfiguration(
                keywords: new[] { "test", "tdd", "feature", "implement" }.ToHashSet(),
                contextPatterns: new[] { "feature_development", "testing" }.ToHashSet(),
                filePatterns: new[] { "*.test.*", "*.spec.*", "*Test.cs", "*Tests.cs" }.ToHashSet(),
                confidenceThreshold: 0.6
            ),
            reminders: new[] { "Start with a failing test (RED) before implementation" }
        ),
        
        // Refactoring constraint
        new AtomicConstraint(
            id: new ConstraintId("refactoring.clean-code"),
            title: "Keep code clean and maintainable",
            priority: 0.8,
            triggers: new TriggerConfiguration(
                keywords: new[] { "refactor", "clean", "improve", "cleanup" }.ToHashSet(),
                contextPatterns: new[] { "refactoring", "maintenance" }.ToHashSet(),
                filePatterns: new[] { "*.cs", "*.js", "*.ts" }.ToHashSet(),
                confidenceThreshold: 0.6
            ),
            reminders: new[] { "Refactor to improve code readability and maintainability" }
        )
    }.AsReadOnly();

    public IConstraint ResolveConstraint(ConstraintId constraintId)
    {
        var constraint = DefaultConstraints.FirstOrDefault(c => c.Id.Equals(constraintId));
        if (constraint == null)
        {
            throw new ConstraintNotFoundException($"Constraint '{constraintId.Value}' not found in default resolver");
        }
        return constraint;
    }

    public IResolutionMetrics GetResolutionMetrics()
    {
        return new SimpleResolutionMetrics(
            totalResolutions: DefaultConstraints.Count,
            cacheHitRate: 1.0
        );
    }
}

/// <summary>
/// Simple implementation of resolution metrics for testing purposes.
/// </summary>
internal sealed class SimpleResolutionMetrics : IResolutionMetrics
{
    public int TotalResolutions { get; }
    public double CacheHitRate { get; }
    public TimeSpan AverageResolutionTime { get; }
    public TimeSpan PeakResolutionTime { get; }

    public SimpleResolutionMetrics(int totalResolutions, double cacheHitRate)
    {
        TotalResolutions = totalResolutions;
        CacheHitRate = cacheHitRate;
        AverageResolutionTime = TimeSpan.FromMilliseconds(1); // Sub-millisecond for tests
        PeakResolutionTime = TimeSpan.FromMilliseconds(2);
    }
}

/// <summary>
/// Exception thrown when a constraint cannot be resolved.
/// </summary>
public class ConstraintNotFoundException : Exception
{
    public ConstraintNotFoundException(string message) : base(message)
    {
    }

    public ConstraintNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
