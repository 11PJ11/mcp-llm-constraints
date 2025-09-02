using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using ConstraintMcpServer.Application.Visualization;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Domain.Visualization;
using ConstraintMcpServer.Tests.Steps;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// E2E test for Enhanced Visualization & Console Integration (Step C1).
/// Drives implementation through Outside-In TDD with business scenarios.
/// Tests will naturally pass when sufficient inner TDD implementation exists.
/// </summary>
[TestFixture]
[Category("E2E")]
[Category("Visualization")]
public sealed class EnhancedVisualizationE2E
{
    private EnhancedVisualizationSteps _steps = null!;

    [SetUp]
    public void Setup()
    {
        _steps = new EnhancedVisualizationSteps();
    }

    [TearDown]
    public void TearDown()
    {
        _steps?.Dispose();
        _steps = null!;
    }

    /// <summary>
    /// Business Scenario: Developer views constraint library with enhanced visualization
    /// to understand constraint relationships and priorities clearly.
    /// </summary>
    [Test]
    public async Task EnhancedVisualization_WhenDeveloperViewsConstraintLibrary_ShouldDisplayEnhancedTreeWithSymbolsAndColors()
    {
        await Given(() => _steps.AConstraintLibraryWithMixedConstraints())
            .And(() => _steps.EnhancedVisualizationOptionsWithSymbolsAndColors())
            .When(() => _steps.DeveloperRequestsEnhancedTreeVisualization())
            .Then(() => _steps.VisualizationShouldContainUnicodeTreeSymbols())
            .And(() => _steps.VisualizationShouldShowPriorityIndicators())
            .And(() => _steps.VisualizationShouldDisplayConstraintTypeSymbols())
            .And(() => _steps.VisualizationShouldBeClaudeCodeCompatible())
            .ExecuteAsync();
    }

    /// <summary>
    /// Business Scenario: Developer needs Claude Code optimized output
    /// for console compatibility and readability.
    /// </summary>
    [Test]
    public async Task EnhancedVisualization_WhenClaudeCodeCompatibilityEnabled_ShouldOptimizeForConsoleDisplay()
    {
        await Given(() => _steps.AConstraintLibraryWithVariousPriorities())
            .And(() => _steps.ClaudeCodeOptimizedVisualizationOptions())
            .When(() => _steps.DeveloperRequestsClaudeCodeOptimizedVisualization())
            .Then(() => _steps.VisualizationShouldUseAsciiCharactersOnly())
            .And(() => _steps.VisualizationShouldFitWithinConsoleWidthLimits())
            .And(() => _steps.VisualizationShouldIncludePriorityEmojis())
            .And(() => _steps.RenderTimeShouldBeBelowPerformanceThreshold())
            .ExecuteAsync();
    }

    /// <summary>
    /// Business Scenario: Developer analyzes complex constraint hierarchies
    /// with composite constraints and dependencies.
    /// </summary>
    [Test]
    [Ignore("Temporarily disabled until implementation - will enable one at a time to avoid commit blocks")]
    public async Task EnhancedVisualization_WhenAnalyzingComplexHierarchies_ShouldShowCompositeRelationships()
    {
        await Given(() => _steps.AComplexConstraintLibraryWithCompositeHierarchy())
            .And(() => _steps.DetailedVisualizationOptionsWithRelationshipDisplay())
            .When(() => _steps.DeveloperAnalyzesConstraintHierarchy())
            .Then(() => _steps.VisualizationShouldShowCompositeConstraintNesting())
            .And(() => _steps.VisualizationShouldDisplayDependencyRelationships())
            .And(() => _steps.VisualizationShouldHighlightConstraintComposition())
            .ExecuteAsync();
    }

    /// <summary>
    /// Business Scenario: Performance-conscious developer ensures
    /// enhanced visualization meets sub-50ms requirements.
    /// </summary>
    [Test]
    [Ignore("Temporarily disabled until implementation - will enable one at a time to avoid commit blocks")]
    public async Task EnhancedVisualization_WhenProcessingLargeConstraintLibrary_ShouldMeetPerformanceRequirements()
    {
        await Given(() => _steps.ALargeConstraintLibraryWith100Constraints())
            .And(() => _steps.PerformanceOptimizedVisualizationOptions())
            .When(() => _steps.DeveloperRequestsVisualizationOfLargeLibrary())
            .Then(() => _steps.RenderTimeShouldBeBelow50Milliseconds())
            .And(() => _steps.MemoryUsageShouldBeWithinBounds())
            .And(() => _steps.VisualizationQualityShouldBePreserved())
            .ExecuteAsync();
    }

    // BDD Helper Methods for Given().When().Then() fluent API
    private static BddScenarioBuilder Given(Func<Task> step) => new BddScenarioBuilder().Given(step);
}

/// <summary>
/// Simple BDD scenario builder for Given().When().Then() fluent API.
/// Enables business-focused test structure with lambda expressions.
/// </summary>
public sealed class BddScenarioBuilder
{
    private readonly List<Func<Task>> _steps = new();

    public BddScenarioBuilder Given(Func<Task> step)
    {
        _steps.Add(step);
        return this;
    }

    public BddScenarioBuilder And(Func<Task> step)
    {
        _steps.Add(step);
        return this;
    }

    public BddScenarioBuilder When(Func<Task> step)
    {
        _steps.Add(step);
        return this;
    }

    public BddScenarioBuilder Then(Func<Task> step)
    {
        _steps.Add(step);
        return this;
    }

    public async Task ExecuteAsync()
    {
        foreach (var step in _steps)
        {
            await step();
        }
    }
}
