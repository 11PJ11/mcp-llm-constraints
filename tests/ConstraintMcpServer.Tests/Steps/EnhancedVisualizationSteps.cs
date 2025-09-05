using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Visualization;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Common;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Domain.Visualization;

namespace ConstraintMcpServer.Tests.Steps;

/// <summary>
/// Business-focused step implementations for Enhanced Visualization E2E scenarios.
/// Uses business language and focuses on outcomes, not implementation details.
/// </summary>
public sealed class EnhancedVisualizationSteps : IDisposable
{
    private ConstraintLibrary? _constraintLibrary;
    private TreeVisualizationOptions? _visualizationOptions;
    private EnhancedTreeVisualization? _enhancedVisualization;
    private DateTime _renderStartTime;
    private TimeSpan _actualRenderTime;

    // ========================
    // Given - Context Setup
    // ========================

    public async Task AConstraintLibraryWithMixedConstraints()
    {
        await Task.CompletedTask;

        _constraintLibrary = CreateConstraintLibraryWithMixedConstraints();
    }

    public async Task AConstraintLibraryWithVariousPriorities()
    {
        await Task.CompletedTask;

        _constraintLibrary = CreateConstraintLibraryWithVariousPriorities();
    }

    public async Task AComplexConstraintLibraryWithCompositeHierarchy()
    {
        await Task.CompletedTask;

        _constraintLibrary = CreateComplexConstraintLibraryWithCompositeHierarchy();
    }

    public async Task ALargeConstraintLibraryWith100Constraints()
    {
        await Task.CompletedTask;

        _constraintLibrary = CreateLargeConstraintLibrary(100);
    }

    public async Task EnhancedVisualizationOptionsWithSymbolsAndColors()
    {
        await Task.CompletedTask;

        _visualizationOptions = TreeVisualizationOptions.Default with
        {
            CharacterSet = CharacterSet.Unicode,
            Colors = ColorScheme.HighContrast,
            ShowMetadata = true,
            ClaudeCodeCompatible = true
        };
    }

    public async Task ClaudeCodeOptimizedVisualizationOptions()
    {
        await Task.CompletedTask;

        _visualizationOptions = TreeVisualizationOptions.Default.WithClaudeCodeOptimization();
    }

    public async Task DetailedVisualizationOptionsWithRelationshipDisplay()
    {
        await Task.CompletedTask;

        _visualizationOptions = TreeVisualizationOptions.Detailed with
        {
            ShowRelationshipTypes = true,
            ClaudeCodeCompatible = true
        };
    }

    public async Task PerformanceOptimizedVisualizationOptions()
    {
        await Task.CompletedTask;

        _visualizationOptions = TreeVisualizationOptions.Default.WithPerformanceMode();
    }

    // ========================
    // When - Actions
    // ========================

    public async Task DeveloperRequestsEnhancedTreeVisualization()
    {
        _renderStartTime = DateTime.UtcNow;

        // Create enhanced renderer and generate visualization
        var renderer = CreateEnhancedTreeRenderer();
        var result = await renderer.RenderEnhancedTreeAsync(_constraintLibrary!, _visualizationOptions!);

        _actualRenderTime = DateTime.UtcNow - _renderStartTime;

        Assert.That(result.IsSuccess, Is.True, "enhanced visualization should succeed");
        _enhancedVisualization = result.Value;
    }

    public async Task DeveloperRequestsClaudeCodeOptimizedVisualization()
    {
        _renderStartTime = DateTime.UtcNow;

        var renderer = CreateEnhancedTreeRenderer();
        var result = await renderer.RenderEnhancedTreeAsync(_constraintLibrary!, _visualizationOptions!);

        _actualRenderTime = DateTime.UtcNow - _renderStartTime;

        Assert.That(result.IsSuccess, Is.True, "Claude Code optimized visualization should succeed");
        _enhancedVisualization = result.Value;
    }

    public async Task DeveloperAnalyzesConstraintHierarchy()
    {
        await DeveloperRequestsEnhancedTreeVisualization();
    }

    public async Task DeveloperRequestsVisualizationOfLargeLibrary()
    {
        await DeveloperRequestsEnhancedTreeVisualization();
    }

    // ========================
    // Then - Assertions (Business Outcomes)
    // ========================

    public async Task VisualizationShouldContainUnicodeTreeSymbols()
    {
        await Task.CompletedTask;

        Assert.That(_enhancedVisualization, Is.Not.Null);
        Assert.That(_enhancedVisualization!.TreeContent, Is.Not.Empty);

        // Business outcome: Enhanced tree symbols improve readability
        Assert.That(_enhancedVisualization.TreeContent, Does.Contain("├─"), "should use Unicode tree branches for better visual hierarchy");
        Assert.That(_enhancedVisualization.TreeContent, Does.Contain("│"), "should use Unicode vertical lines for tree structure");
    }

    public async Task VisualizationShouldShowPriorityIndicators()
    {
        await Task.CompletedTask;

        Assert.That(_enhancedVisualization, Is.Not.Null);

        // Business outcome: Priority indicators help developers understand constraint importance
        Assert.That(_enhancedVisualization!.TreeContent, Does.Match(@"🔴|🟡|🟢|🔵"),
            "should display priority emoji indicators for quick constraint priority assessment");
    }

    public async Task VisualizationShouldDisplayConstraintTypeSymbols()
    {
        await Task.CompletedTask;

        Assert.That(_enhancedVisualization, Is.Not.Null);

        // Business outcome: Constraint type symbols enable quick constraint type identification
        Assert.That(_enhancedVisualization!.TreeContent, Does.Match(@"⚛️|🧩"),
            "should display atomic and composite constraint symbols for type identification");
    }

    public async Task VisualizationShouldBeClaudeCodeCompatible()
    {
        await Task.CompletedTask;

        Assert.That(_enhancedVisualization, Is.Not.Null);
        Assert.That(_enhancedVisualization!.IsClaudeCodeCompatible, Is.True, "must be compatible with Claude Code console");
    }

    public async Task VisualizationShouldUseAsciiCharactersOnly()
    {
        await Task.CompletedTask;

        Assert.That(_enhancedVisualization, Is.Not.Null);

        // Business outcome: ASCII characters ensure universal console compatibility
        Assert.That(_enhancedVisualization!.TreeContent, Does.Not.Contain("├─"), "Claude Code optimized mode should use ASCII characters");
        Assert.That(_enhancedVisualization.TreeContent, Does.Contain("+--"), "should use ASCII tree branches for maximum compatibility");
    }

    public async Task VisualizationShouldFitWithinConsoleWidthLimits()
    {
        await Task.CompletedTask;

        Assert.That(_enhancedVisualization, Is.Not.Null);

        // Business outcome: Console width compliance ensures proper display
        var lines = _enhancedVisualization!.TreeContent.Split('\n');
        foreach (var line in lines)
        {
            Assert.That(line.Length, Is.LessThanOrEqualTo(120), "each line should fit within Claude Code console width limits");
        }
    }

    public async Task VisualizationShouldIncludePriorityEmojis()
    {
        await Task.CompletedTask;

        Assert.That(_enhancedVisualization, Is.Not.Null);

        // Business outcome: Priority emojis provide immediate visual priority feedback
        Assert.That(_enhancedVisualization!.TreeContent, Does.Match(@"🔴|🟡|🟢|🔵"),
            "should include priority emojis even in Claude Code compatible mode for developer clarity");
    }

    public async Task RenderTimeShouldBeBelowPerformanceThreshold()
    {
        await Task.CompletedTask;

        // Business outcome: Fast rendering maintains developer productivity
        Assert.That(_actualRenderTime.TotalMilliseconds, Is.LessThan(50),
            "enhanced visualization should meet sub-50ms performance requirements");
    }

    public async Task VisualizationShouldShowCompositeConstraintNesting()
    {
        await Task.CompletedTask;

        Assert.That(_enhancedVisualization, Is.Not.Null);

        // Business outcome: Nested display clarifies composite constraint structure
        Assert.That(_enhancedVisualization!.TreeContent, Does.Contain("Composite"), "should clearly identify composite constraints");
        Assert.That(_enhancedVisualization.ShowsCompositionRelationships, Is.True, "should display composition relationships");
    }

    public async Task VisualizationShouldDisplayDependencyRelationships()
    {
        await Task.CompletedTask;

        Assert.That(_enhancedVisualization, Is.Not.Null);

        // Business outcome: Dependency visualization helps understand constraint relationships
        var treeContent = _enhancedVisualization!.TreeContent;
        Assert.That(treeContent.Contains("→") || treeContent.Contains("depends on") || treeContent.Contains("references"),
            Is.True, "should show dependency relationships between constraints");
    }

    public async Task VisualizationShouldHighlightConstraintComposition()
    {
        await Task.CompletedTask;

        Assert.That(_enhancedVisualization, Is.Not.Null);

        // Business outcome: Composition highlighting clarifies constraint hierarchies
        Assert.That(_enhancedVisualization!.TreeContent, Does.Contain("Composition:"), "should highlight composition type information");
    }

    public async Task RenderTimeShouldBeBelow50Milliseconds()
    {
        await Task.CompletedTask;

        // Business outcome: Sub-50ms rendering ensures responsive developer experience
        Assert.That(_actualRenderTime.TotalMilliseconds, Is.LessThan(50),
            "large library visualization must meet performance requirements");
    }

    public async Task MemoryUsageShouldBeWithinBounds()
    {
        await Task.CompletedTask;

        // Business outcome: Memory efficiency prevents performance degradation
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var memoryUsed = GC.GetTotalMemory(false);

        Assert.That(memoryUsed, Is.LessThan(10_000_000), "should use less than 10MB for large library visualization");
    }

    public async Task VisualizationQualityShouldBePreserved()
    {
        await Task.CompletedTask;

        Assert.That(_enhancedVisualization, Is.Not.Null);

        // Business outcome: Quality preservation ensures visualization value at scale
        Assert.That(_enhancedVisualization!.ConstraintCount, Is.EqualTo(100), "should accurately represent all constraints");
        Assert.That(_enhancedVisualization.TreeContent, Is.Not.Empty, "should generate meaningful content even at scale");
    }

    // ========================
    // Helper Methods (Domain-Focused)
    // ========================

    private static EnhancedTreeRenderer CreateEnhancedTreeRenderer()
    {
        // Create base renderer and wrap it with enhanced functionality
        var baseRenderer = new ConstraintTreeRenderer();
        return new EnhancedTreeRenderer(baseRenderer);
    }

    private static ConstraintLibrary CreateConstraintLibraryWithMixedConstraints()
    {
        var library = new ConstraintLibrary("1.0.0", "Mixed Constraint Test Library");

        library.AddAtomicConstraint(CreateAtomicConstraint("tdd.test-first", "Write Test First", 0.95));
        library.AddAtomicConstraint(CreateAtomicConstraint("solid.srp", "Single Responsibility", 0.85));

        var compositeConstraint = CreateCompositeConstraint("quality.standards", "Quality Standards", 0.90);
        library.AddCompositeConstraint(compositeConstraint);

        return library;
    }

    private static ConstraintLibrary CreateConstraintLibraryWithVariousPriorities()
    {
        var library = new ConstraintLibrary("1.0.0", "Priority Test Library");

        library.AddAtomicConstraint(CreateAtomicConstraint("critical.constraint", "Critical Constraint", 0.95));
        library.AddAtomicConstraint(CreateAtomicConstraint("high.constraint", "High Constraint", 0.8));
        library.AddAtomicConstraint(CreateAtomicConstraint("medium.constraint", "Medium Constraint", 0.6));
        library.AddAtomicConstraint(CreateAtomicConstraint("low.constraint", "Low Constraint", 0.3));

        return library;
    }

    private static ConstraintLibrary CreateComplexConstraintLibraryWithCompositeHierarchy()
    {
        var library = new ConstraintLibrary("1.0.0", "Complex Hierarchy Library");

        // Add atomic constraints first
        library.AddAtomicConstraint(CreateAtomicConstraint("base.constraint.1", "Base Constraint 1", 0.7));
        library.AddAtomicConstraint(CreateAtomicConstraint("base.constraint.2", "Base Constraint 2", 0.8));

        // Add composite constraints that reference the atomic ones
        var composite1 = CreateCompositeConstraintWithBaseReferences("composite.level1", "Level 1 Composite", 0.85);
        library.AddCompositeConstraint(composite1);

        var composite2 = CreateCompositeConstraintWithBaseReferences("composite.level2", "Level 2 Composite", 0.90);
        library.AddCompositeConstraint(composite2);

        return library;
    }

    private static ConstraintLibrary CreateLargeConstraintLibrary(int constraintCount)
    {
        var library = new ConstraintLibrary("1.0.0", "Large Performance Test Library");

        for (int i = 1; i <= constraintCount; i++)
        {
            var priority = 0.5 + (i % 5) * 0.1; // Vary priorities
            library.AddAtomicConstraint(CreateAtomicConstraint($"constraint.{i:D3}", $"Constraint {i}", priority));
        }

        return library;
    }

    private static AtomicConstraint CreateAtomicConstraint(string id, string title, double priority)
    {
        var constraintId = new ConstraintId(id);
        var keywords = ImmutableList.Create("test");
        var filePatterns = ImmutableList.Create("*.cs");
        var contextPatterns = ImmutableList.Create("testing");
        var triggers = new TriggerConfiguration(keywords, filePatterns, contextPatterns);
        var reminders = ImmutableList.Create($"Remember: {title}");

        return new AtomicConstraint(constraintId, title, priority, triggers, reminders);
    }

    private static CompositeConstraint CreateCompositeConstraint(string id, string title, double priority)
    {
        var constraintId = new ConstraintId(id);
        var childReferences = ImmutableList.Create(
            new ConstraintReference(new ConstraintId("tdd.test-first")),
            new ConstraintReference(new ConstraintId("solid.srp"))
        );

        return CompositeConstraintBuilder.CreateWithReferences(constraintId, title, priority, CompositionType.Sequential, childReferences);
    }

    private static CompositeConstraint CreateCompositeConstraintWithBaseReferences(string id, string title, double priority)
    {
        var constraintId = new ConstraintId(id);
        var childReferences = ImmutableList.Create(
            new ConstraintReference(new ConstraintId("base.constraint.1")),
            new ConstraintReference(new ConstraintId("base.constraint.2"))
        );

        return CompositeConstraintBuilder.CreateWithReferences(constraintId, title, priority, CompositionType.Parallel, childReferences);
    }

    public void Dispose()
    {
        // Clean up any resources if needed
        _constraintLibrary = null;
        _visualizationOptions = null;
        _enhancedVisualization = null;
    }
}
