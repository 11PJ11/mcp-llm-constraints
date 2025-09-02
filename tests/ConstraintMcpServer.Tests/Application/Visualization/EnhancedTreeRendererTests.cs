using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Visualization;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Common;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Domain.Visualization;

namespace ConstraintMcpServer.Tests.Application.Visualization;

/// <summary>
/// Unit tests for EnhancedTreeRenderer application service.
/// Drives implementation through TDD Red-Green-Refactor cycles.
/// Tests focus on business behavior and domain outcomes.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("Application")]
[Category("Visualization")]
public sealed class EnhancedTreeRendererTests
{
    private EnhancedTreeRenderer _renderer = null!;
    private ConstraintTreeRenderer _baseRenderer = null!;

    [SetUp]
    public void Setup()
    {
        _baseRenderer = new ConstraintTreeRenderer();
        _renderer = new EnhancedTreeRenderer(_baseRenderer);
    }

    [TearDown]
    public void TearDown()
    {
        // No cleanup needed for stateless renderer
    }

    /// <summary>
    /// RED: Test should fail initially - null library should return validation error
    /// </summary>
    [Test]
    public async Task RenderEnhancedTreeAsync_WhenLibraryIsNull_ShouldReturnValidationError()
    {
        // Given
        ConstraintLibrary? nullLibrary = null;
        var options = TreeVisualizationOptions.Default;

        // When
        var result = await _renderer.RenderEnhancedTreeAsync(nullLibrary!, options);

        // Then
        Assert.That(result.IsError, Is.True, "Should return error for null library");
        Assert.That(result.Error, Is.InstanceOf<ValidationError>(), "Should return ValidationError");

        var validationError = (ValidationError)result.Error;
        Assert.That(validationError.Message, Does.Contain("cannot be null"), "Should explain null validation failure");
    }

    /// <summary>
    /// RED: Test should fail initially - empty library should render enhanced empty structure
    /// </summary>
    [Test]
    public async Task RenderEnhancedTreeAsync_WhenLibraryIsEmpty_ShouldRenderEnhancedEmptyLibraryStructure()
    {
        // Given
        var emptyLibrary = new ConstraintLibrary("1.0.0", "Empty Enhanced Library");
        var options = TreeVisualizationOptions.Default with { CharacterSet = CharacterSet.Unicode };

        // When
        var result = await _renderer.RenderEnhancedTreeAsync(emptyLibrary, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully render enhanced empty library");

        var visualization = result.Value;
        Assert.That(visualization, Is.Not.Null, "Should return enhanced visualization result");
        Assert.That(visualization.ConstraintCount, Is.EqualTo(0), "Should show zero constraints");
        Assert.That(visualization.TreeContent, Does.Contain("Empty Enhanced Library"), "Should include library description");
        Assert.That(visualization.ColorProfile, Is.Not.Null, "Should have color profile");
        Assert.That(visualization.SymbolSet, Is.Not.Null, "Should have symbol set");
        Assert.That(visualization.IsClaudeCodeCompatible, Is.True, "Should be Claude Code compatible");
    }

    /// <summary>
    /// RED: Test should fail initially - single atomic constraint should render with enhanced symbols
    /// </summary>
    [Test]
    public async Task RenderEnhancedTreeAsync_WhenLibraryHasSingleAtomicConstraint_ShouldRenderWithEnhancedSymbols()
    {
        // Given
        var constraint = CreateTestAtomicConstraint("test.constraint", "Test Constraint", 0.8);
        var library = new ConstraintLibrary("1.0.0", "Enhanced Test Library");
        library.AddAtomicConstraint(constraint);
        var options = TreeVisualizationOptions.Default with 
        { 
            CharacterSet = CharacterSet.Unicode,
            ShowMetadata = true
        };

        // When
        var result = await _renderer.RenderEnhancedTreeAsync(library, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully render enhanced single constraint");

        var visualization = result.Value;
        Assert.That(visualization.ConstraintCount, Is.EqualTo(1), "Should count one constraint");
        Assert.That(visualization.TreeContent, Does.Contain("Atomic Constraints:"), "Should show atomic constraints section");
        Assert.That(visualization.HasHierarchicalStructure, Is.True, "Should indicate hierarchical structure");
        Assert.That(visualization.SymbolSet, Is.EqualTo(EnhancedSymbolSet.Unicode), "Should use Unicode symbols");
        Assert.That(visualization.MeetsPerformanceThreshold, Is.True, "Should meet performance requirements");
    }

    /// <summary>
    /// RED: Test should fail initially - Claude Code compatibility should use ASCII symbols
    /// </summary>
    [Test]
    public async Task RenderEnhancedTreeAsync_WhenClaudeCodeCompatibilityEnabled_ShouldUseAsciiSymbols()
    {
        // Given
        var constraint = CreateTestAtomicConstraint("test.constraint", "Test Constraint", 0.9);
        var library = new ConstraintLibrary("1.0.0", "Claude Code Library");
        library.AddAtomicConstraint(constraint);
        var options = TreeVisualizationOptions.Default.WithClaudeCodeOptimization();

        // When
        var result = await _renderer.RenderEnhancedTreeAsync(library, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully render Claude Code compatible visualization");

        var visualization = result.Value;
        Assert.That(visualization.IsClaudeCodeCompatible, Is.True, "Should be Claude Code compatible");
        Assert.That(visualization.SymbolSet, Is.EqualTo(EnhancedSymbolSet.Ascii), "Should use ASCII symbols");
        Assert.That(visualization.FitsConsoleWidth, Is.True, "Should fit within console width limits");
        Assert.That(visualization.IncludesPriorityIndicators, Is.True, "Should include priority emoji indicators");
    }

    /// <summary>
    /// RED: Test should fail initially - high priority constraints should get priority indicators
    /// </summary>
    [Test]
    public async Task RenderEnhancedTreeAsync_WhenConstraintHasHighPriority_ShouldIncludePriorityIndicator()
    {
        // Given
        var highPriorityConstraint = CreateTestAtomicConstraint("critical.constraint", "Critical Constraint", 0.95);
        var library = new ConstraintLibrary("1.0.0", "Priority Test Library");
        library.AddAtomicConstraint(highPriorityConstraint);
        var options = TreeVisualizationOptions.Default with { ShowMetadata = true };

        // When
        var result = await _renderer.RenderEnhancedTreeAsync(library, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully render high priority constraint");

        var visualization = result.Value;
        Assert.That(visualization.IncludesPriorityIndicators, Is.True, "Should include priority indicators");
        Assert.That(visualization.TreeContent, Does.Contain("🔴").Or.Contain("Priority: 0.95"), "Should show high priority indicator or priority value");
    }

    /// <summary>
    /// RED: Test should fail initially - composite constraints should render with composition symbols
    /// </summary>
    [Test]
    public async Task RenderEnhancedTreeAsync_WhenLibraryHasCompositeConstraint_ShouldRenderWithCompositionSymbols()
    {
        // Given
        var library = new ConstraintLibrary("1.0.0", "Composite Enhanced Library");

        // Add referenced atomic constraints first
        var childConstraint1 = CreateTestAtomicConstraint("child.constraint.1", "Child Constraint 1", 0.7);
        var childConstraint2 = CreateTestAtomicConstraint("child.constraint.2", "Child Constraint 2", 0.8);
        library.AddAtomicConstraint(childConstraint1);
        library.AddAtomicConstraint(childConstraint2);

        // Add composite constraint
        var compositeConstraint = CreateTestCompositeConstraint("composite.test", "Test Composite", 0.85);
        library.AddCompositeConstraint(compositeConstraint);

        var options = TreeVisualizationOptions.Default with 
        { 
            CharacterSet = CharacterSet.Unicode,
            ShowMetadata = true 
        };

        // When
        var result = await _renderer.RenderEnhancedTreeAsync(library, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully render composite constraint");

        var visualization = result.Value;
        Assert.That(visualization.ConstraintCount, Is.EqualTo(3), "Should count three constraints (2 atomic + 1 composite)");
        Assert.That(visualization.ShowsCompositionRelationships, Is.True, "Should indicate composition relationships");
        Assert.That(visualization.TreeContent, Does.Contain("Composite"), "Should identify composite constraints");
    }

    /// <summary>
    /// RED: Test should fail initially - performance requirements should be met for large libraries
    /// </summary>
    [Test]
    public async Task RenderEnhancedTreeAsync_WhenRenderingLargeLibrary_ShouldMeetPerformanceRequirements()
    {
        // Given
        var constraints = CreateLargeConstraintSet(25); // Moderate size for unit test
        var library = new ConstraintLibrary("1.0.0", "Large Enhanced Library");
        foreach (var constraint in constraints)
        {
            library.AddAtomicConstraint(constraint);
        }
        var options = TreeVisualizationOptions.Default.WithPerformanceMode();

        // When
        var result = await _renderer.RenderEnhancedTreeAsync(library, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully render large library");

        var visualization = result.Value;
        Assert.That(visualization.ConstraintCount, Is.EqualTo(25), "Should count all constraints");
        Assert.That(visualization.MeetsPerformanceThreshold, Is.True, "Should complete within performance threshold");
        Assert.That(visualization.RenderTime.TotalMilliseconds, Is.LessThan(100), "Should render quickly for large libraries");
    }

    /// <summary>
    /// RED: Test should fail initially - null base renderer should throw ArgumentNullException
    /// </summary>
    [Test]
    public void Constructor_WhenBaseRendererIsNull_ShouldThrowArgumentNullException()
    {
        // Given
        ConstraintTreeRenderer? nullBaseRenderer = null;

        // When
        Action createWithNull = () => new EnhancedTreeRenderer(nullBaseRenderer!);

        // Then
        Assert.That(createWithNull, Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("baseRenderer"));
    }

    // ===================
    // Helper Methods
    // ===================

    private static AtomicConstraint CreateTestAtomicConstraint(string id, string title, double priority)
    {
        var constraintId = new ConstraintId(id);
        var keywords = ImmutableList.Create("test", "unit");
        var filePatterns = ImmutableList.Create("*.cs");
        var contextPatterns = ImmutableList.Create("testing");
        var triggers = new TriggerConfiguration(keywords, filePatterns, contextPatterns);
        var reminders = ImmutableList.Create($"Remember: {title}");

        return new AtomicConstraint(constraintId, title, priority, triggers, reminders);
    }

    private static CompositeConstraint CreateTestCompositeConstraint(string id, string title, double priority)
    {
        var constraintId = new ConstraintId(id);
        var childConstraints = ImmutableList.Create(
            new ConstraintReference(new ConstraintId("child.constraint.1")),
            new ConstraintReference(new ConstraintId("child.constraint.2"))
        );

        return CompositeConstraintBuilder.CreateWithReferences(constraintId, title, priority, CompositionType.Sequential, childConstraints);
    }

    private static ImmutableList<AtomicConstraint> CreateLargeConstraintSet(int count)
    {
        var constraints = ImmutableList.CreateBuilder<AtomicConstraint>();

        for (int i = 1; i <= count; i++)
        {
            var constraint = CreateTestAtomicConstraint($"constraint.{i:D3}", $"Test Constraint {i}", 0.5 + (i % 5) * 0.1);
            constraints.Add(constraint);
        }

        return constraints.ToImmutable();
    }
}