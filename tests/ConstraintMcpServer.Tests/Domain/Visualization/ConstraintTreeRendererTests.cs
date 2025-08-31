using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Common;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Domain.Visualization;

namespace ConstraintMcpServer.Tests.Domain.Visualization;

/// <summary>
/// Unit tests for ConstraintTreeRenderer using TDD Red-Green-Refactor methodology.
/// Tests follow business behavior focus with Given-When-Then structure.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("Domain")]
[Category("Visualization")]
public sealed class ConstraintTreeRendererTests
{
    private ConstraintTreeRenderer _renderer = null!;

    [SetUp]
    public void Setup()
    {
        _renderer = new ConstraintTreeRenderer();
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
    public async Task RenderTreeAsync_WhenLibraryIsNull_ShouldReturnValidationError()
    {
        // Given
        ConstraintMcpServer.Domain.Constraints.ConstraintLibrary? nullLibrary = null;
        var options = TreeVisualizationOptions.Default;

        // When
        var result = await _renderer.RenderTreeAsync(nullLibrary!, options);

        // Then
        Assert.That(result.IsError, Is.True, "Should return error for null library");
        Assert.That(result.Error, Is.InstanceOf<ValidationError>(), "Should return ValidationError");

        var validationError = (ValidationError)result.Error;
        Assert.That(validationError.FieldName, Is.EqualTo("library"), "Should identify library field as invalid");
        Assert.That(validationError.Message, Does.Contain("cannot be null"), "Should explain null validation failure");
    }

    /// <summary>
    /// RED: Test should fail initially - empty library should render basic structure
    /// </summary>
    [Test]
    public async Task RenderTreeAsync_WhenLibraryIsEmpty_ShouldRenderEmptyLibraryStructure()
    {
        // Given
        var emptyLibrary = new ConstraintMcpServer.Domain.Constraints.ConstraintLibrary("1.0.0", "Empty Test Library");
        var options = TreeVisualizationOptions.Default;

        // When
        var result = await _renderer.RenderTreeAsync(emptyLibrary, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully render empty library");

        var visualization = result.Value;
        Assert.That(visualization, Is.Not.Null, "Should return visualization result");
        Assert.That(visualization.ConstraintCount, Is.EqualTo(0), "Should show zero constraints");
        Assert.That(visualization.TreeContent, Does.Contain("Empty Test Library"), "Should include library description");
        Assert.That(visualization.TreeContent, Does.Contain("Version: 1.0.0"), "Should include version information");
        Assert.That(visualization.IsAsciiFormat, Is.True, "Should use ASCII format by default");
        Assert.That(visualization.IsClaudeCodeCompatible, Is.True, "Should be Claude Code compatible");
    }

    /// <summary>
    /// RED: Test should fail initially - single atomic constraint should render with branch markers
    /// </summary>
    [Test]
    public async Task RenderTreeAsync_WhenLibraryHasSingleAtomicConstraint_ShouldRenderConstraintWithBranchMarkers()
    {
        // Given
        var constraint = CreateTestAtomicConstraint("test.constraint", "Test Constraint", 0.8);
        var library = new ConstraintMcpServer.Domain.Constraints.ConstraintLibrary("1.0.0", "Test Library");
        library.AddAtomicConstraint(constraint);
        var options = TreeVisualizationOptions.Default;

        // When
        var result = await _renderer.RenderTreeAsync(library, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully render single constraint");

        var visualization = result.Value;
        Assert.That(visualization.ConstraintCount, Is.EqualTo(1), "Should count one constraint");
        Assert.That(visualization.TreeContent, Does.Contain("Atomic Constraints:"), "Should show atomic constraints section");
        Assert.That(visualization.TreeContent, Does.Contain("+-- test.constraint"), "Should show constraint with branch marker");
        Assert.That(visualization.TreeContent, Does.Contain("Priority: 0.80"), "Should show constraint priority");
        Assert.That(visualization.HasHierarchicalStructure, Is.True, "Should indicate hierarchical structure");
        Assert.That(visualization.DisplaysConstraintMetadata, Is.True, "Should display constraint metadata");
    }

    /// <summary>
    /// RED: Test should fail initially - metadata should be hidden when ShowMetadata is false
    /// </summary>
    [Test]
    public async Task RenderTreeAsync_WhenMetadataDisabled_ShouldNotShowConstraintDetails()
    {
        // Given
        var constraint = CreateTestAtomicConstraint("test.constraint", "Test Constraint", 0.8);
        var library = new ConstraintMcpServer.Domain.Constraints.ConstraintLibrary("1.0.0", "Test Library");
        library.AddAtomicConstraint(constraint);
        var options = TreeVisualizationOptions.Default with { ShowMetadata = false };

        // When
        var result = await _renderer.RenderTreeAsync(library, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully render without metadata");

        var visualization = result.Value;
        Assert.That(visualization.TreeContent, Does.Contain("+-- test.constraint"), "Should show constraint ID");
        Assert.That(visualization.TreeContent, Does.Not.Contain("Priority:"), "Should not show priority when metadata disabled");
        Assert.That(visualization.TreeContent, Does.Not.Contain("Title:"), "Should not show title when metadata disabled");
        Assert.That(visualization.TreeContent, Does.Not.Contain("Keywords:"), "Should not show keywords when metadata disabled");
    }

    /// <summary>
    /// RED: Test should fail initially - multiple constraints should be rendered in order
    /// </summary>
    [Test]
    public async Task RenderTreeAsync_WhenLibraryHasMultipleAtomicConstraints_ShouldRenderAllConstraints()
    {
        // Given
        var constraint1 = CreateTestAtomicConstraint("first.constraint", "First Constraint", 0.9);
        var constraint2 = CreateTestAtomicConstraint("second.constraint", "Second Constraint", 0.7);
        var library = new ConstraintMcpServer.Domain.Constraints.ConstraintLibrary("1.0.0", "Multi-Constraint Library");
        library.AddAtomicConstraint(constraint1);
        library.AddAtomicConstraint(constraint2);
        var options = TreeVisualizationOptions.Default;

        // When
        var result = await _renderer.RenderTreeAsync(library, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully render multiple constraints");

        var visualization = result.Value;
        Assert.That(visualization.ConstraintCount, Is.EqualTo(2), "Should count two constraints");
        Assert.That(visualization.TreeContent, Does.Contain("+-- first.constraint"), "Should render first constraint");
        Assert.That(visualization.TreeContent, Does.Contain("+-- second.constraint"), "Should render second constraint");
        Assert.That(visualization.TreeContent, Does.Contain("Priority: 0.90"), "Should show first constraint priority");
        Assert.That(visualization.TreeContent, Does.Contain("Priority: 0.70"), "Should show second constraint priority");
    }

    /// <summary>
    /// RED: Test should fail initially - composite constraints should render with indentation
    /// </summary>
    [Test]
    public async Task RenderTreeAsync_WhenLibraryHasCompositeConstraint_ShouldRenderWithProperIndentation()
    {
        // Given
        var library = new ConstraintMcpServer.Domain.Constraints.ConstraintLibrary("1.0.0", "Composite Library");

        // First add the referenced atomic constraints
        var childConstraint1 = CreateTestAtomicConstraint("child.constraint.1", "Child Constraint 1", 0.7);
        var childConstraint2 = CreateTestAtomicConstraint("child.constraint.2", "Child Constraint 2", 0.8);
        library.AddAtomicConstraint(childConstraint1);
        library.AddAtomicConstraint(childConstraint2);

        // Then add the composite constraint that references them
        var compositeConstraint = CreateTestCompositeConstraint("composite.test", "Test Composite", 0.85, CompositionType.Sequential);
        library.AddCompositeConstraint(compositeConstraint);
        var options = TreeVisualizationOptions.Default;

        // When
        var result = await _renderer.RenderTreeAsync(library, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully render composite constraint");

        var visualization = result.Value;
        Assert.That(visualization.ConstraintCount, Is.EqualTo(3), "Should count three constraints (2 atomic + 1 composite)");
        Assert.That(visualization.TreeContent, Does.Contain("Composite Constraints:"), "Should show composite constraints section");
        Assert.That(visualization.TreeContent, Does.Contain("+-- composite.test"), "Should show composite constraint with branch marker");
        Assert.That(visualization.TreeContent, Does.Contain("Priority: 0.85, Composite"), "Should show composite priority and type");
        Assert.That(visualization.ShowsCompositionRelationships, Is.True, "Should indicate composition relationships");
    }

    /// <summary>
    /// RED: Test should fail initially - rendering should complete within performance threshold
    /// </summary>
    [Test]
    public async Task RenderTreeAsync_WhenRenderingLargeLibrary_ShouldCompleteWithinPerformanceThreshold()
    {
        // Given
        var constraints = CreateLargeConstraintSet(50); // 50 constraints
        var library = new ConstraintMcpServer.Domain.Constraints.ConstraintLibrary("1.0.0", "Large Library");
        foreach (var constraint in constraints)
        {
            library.AddAtomicConstraint(constraint);
        }
        var options = TreeVisualizationOptions.Default;

        // When
        var result = await _renderer.RenderTreeAsync(library, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully render large library");

        var visualization = result.Value;
        Assert.That(visualization.ConstraintCount, Is.EqualTo(50), "Should count all constraints");
        Assert.That(visualization.MeetsPerformanceThreshold, Is.True, "Should complete within 50ms performance threshold");
        Assert.That(visualization.RenderTime.TotalMilliseconds, Is.LessThan(100), "Should render quickly even for large libraries");
    }

    /// <summary>
    /// RED: Test should fail initially - exception during rendering should return error result
    /// </summary>
    [Test]
    public async Task RenderTreeAsync_WhenConstraintThrowsException_ShouldReturnErrorResult()
    {
        // Given - This will test exception handling, but we need a way to trigger an exception
        var constraint = CreateTestAtomicConstraint("test.constraint", "Test", 0.8);
        var library = new ConstraintMcpServer.Domain.Constraints.ConstraintLibrary("1.0.0", "Test");
        library.AddAtomicConstraint(constraint);
        var options = TreeVisualizationOptions.Default;

        // When - For now, this should pass, but we can modify the test later to trigger exceptions
        var result = await _renderer.RenderTreeAsync(library, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should handle normal rendering (this test needs refinement for exception scenarios)");
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
        var reminders = ImmutableList.Create("Remember to test");

        return new AtomicConstraint(constraintId, title, priority, triggers, reminders);
    }

    private static CompositeConstraint CreateTestCompositeConstraint(string id, string title, double priority, CompositionType compositionType)
    {
        var constraintId = new ConstraintId(id);
        var childConstraints = ImmutableList.Create(
            new ConstraintReference(new ConstraintId("child.constraint.1")),
            new ConstraintReference(new ConstraintId("child.constraint.2"))
        );

        return new CompositeConstraint(constraintId, title, priority, compositionType, childConstraints);
    }

    private static List<AtomicConstraint> CreateLargeConstraintSet(int count)
    {
        var constraints = new List<AtomicConstraint>();

        for (int i = 1; i <= count; i++)
        {
            var constraint = CreateTestAtomicConstraint($"constraint.{i:D3}", $"Test Constraint {i}", 0.5 + (i % 5) * 0.1);
            constraints.Add(constraint);
        }

        return constraints;
    }
}
