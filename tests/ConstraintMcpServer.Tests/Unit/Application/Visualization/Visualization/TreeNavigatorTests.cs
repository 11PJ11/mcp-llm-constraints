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
/// Unit tests for TreeNavigator application service.
/// Drives implementation through TDD Red-Green-Refactor cycles.
/// Tests focus on navigation, filtering, and search functionality.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("Application")]
[Category("Navigation")]
public sealed class TreeNavigatorTests
{
    private TreeNavigator _navigator = null!;
    private ConstraintLibrary _testLibrary = null!;

    [SetUp]
    public void Setup()
    {
        _navigator = new TreeNavigator();
        _testLibrary = CreateTestConstraintLibrary();
    }

    [TearDown]
    public void TearDown()
    {
        // No cleanup needed for stateless navigator
    }

    /// <summary>
    /// RED: Test should fail initially - should find constraints by ID
    /// </summary>
    [Test]
    public async Task FindConstraintByIdAsync_WhenConstraintExists_ShouldReturnConstraint()
    {
        // Given
        var targetId = new ConstraintId("test.constraint.1");

        // When
        var result = await _navigator.FindConstraintByIdAsync(_testLibrary, targetId);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully find existing constraint");
        Assert.That(result.Value, Is.Not.Null, "Should return constraint");
        Assert.That(result.Value.Id, Is.EqualTo(targetId), "Should return correct constraint");
    }

    /// <summary>
    /// RED: Test should fail initially - should return error when constraint not found
    /// </summary>
    [Test]
    public async Task FindConstraintByIdAsync_WhenConstraintNotExists_ShouldReturnNotFoundError()
    {
        // Given
        var nonExistentId = new ConstraintId("non.existent.constraint");

        // When
        var result = await _navigator.FindConstraintByIdAsync(_testLibrary, nonExistentId);

        // Then
        Assert.That(result.IsError, Is.True, "Should return error for non-existent constraint");
        Assert.That(result.Error.Message, Does.Contain("not found"), "Should explain constraint was not found");
    }

    /// <summary>
    /// RED: Test should fail initially - should filter constraints by priority range
    /// </summary>
    [Test]
    public async Task FilterConstraintsByPriorityAsync_WhenFilteringByRange_ShouldReturnMatchingConstraints()
    {
        // Given
        var minPriority = 0.7;
        var maxPriority = 0.9;

        // When
        var result = await _navigator.FilterConstraintsByPriorityAsync(_testLibrary, minPriority, maxPriority);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully filter constraints");
        Assert.That(result.Value, Is.Not.Empty, "Should return matching constraints");

        foreach (var constraint in result.Value)
        {
            Assert.That(constraint.Priority, Is.GreaterThanOrEqualTo(minPriority),
                $"Constraint {constraint.Id} priority should be >= {minPriority}");
            Assert.That(constraint.Priority, Is.LessThanOrEqualTo(maxPriority),
                $"Constraint {constraint.Id} priority should be <= {maxPriority}");
        }
    }

    /// <summary>
    /// RED: Test should fail initially - should search constraints by keyword
    /// </summary>
    [Test]
    public async Task SearchConstraintsByKeywordAsync_WhenSearchingExistingKeyword_ShouldReturnMatches()
    {
        // Given
        var searchKeyword = "test";

        // When
        var result = await _navigator.SearchConstraintsByKeywordAsync(_testLibrary, searchKeyword);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully search constraints");
        Assert.That(result.Value, Is.Not.Empty, "Should return matching constraints");

        foreach (var constraint in result.Value)
        {
            var hasKeywordInTriggers = constraint.Triggers.Keywords.Any(k =>
                k.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase));
            var hasKeywordInTitle = constraint.Title.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase);
            var hasKeywordInId = constraint.Id.Value.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase);

            Assert.That(hasKeywordInTriggers || hasKeywordInTitle || hasKeywordInId, Is.True,
                $"Constraint {constraint.Id} should contain keyword '{searchKeyword}' in triggers, title, or ID");
        }
    }

    /// <summary>
    /// RED: Test should fail initially - should get constraint path in hierarchy
    /// </summary>
    [Test]
    public async Task GetConstraintPathAsync_WhenConstraintInHierarchy_ShouldReturnPath()
    {
        // Given
        var targetId = new ConstraintId("child.constraint.1");

        // When
        var result = await _navigator.GetConstraintPathAsync(_testLibrary, targetId);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully get constraint path");
        Assert.That(result.Value, Is.Not.Empty, "Should return path");
        Assert.That(result.Value.Last().Id, Is.EqualTo(targetId), "Path should end with target constraint");
    }

    /// <summary>
    /// RED: Test should fail initially - should get related constraints
    /// </summary>
    [Test]
    public async Task GetRelatedConstraintsAsync_WhenConstraintHasRelations_ShouldReturnRelated()
    {
        // Given
        var compositeConstraintId = new ConstraintId("composite.test");

        // When
        var result = await _navigator.GetRelatedConstraintsAsync(_testLibrary, compositeConstraintId);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully get related constraints");
        Assert.That(result.Value, Is.Not.Empty, "Should return related constraints");

        // Should include child constraints referenced by the composite
        Assert.That(result.Value.Select(c => c.Id.Value), Does.Contain("child.constraint.1"));
        Assert.That(result.Value.Select(c => c.Id.Value), Does.Contain("child.constraint.2"));
    }

    /// <summary>
    /// RED: Test should fail initially - should handle null library gracefully
    /// </summary>
    [Test]
    public async Task FindConstraintByIdAsync_WhenLibraryIsNull_ShouldReturnValidationError()
    {
        // Given
        ConstraintLibrary? nullLibrary = null;
        var targetId = new ConstraintId("any.constraint");

        // When
        var result = await _navigator.FindConstraintByIdAsync(nullLibrary!, targetId);

        // Then
        Assert.That(result.IsError, Is.True, "Should return error for null library");
        Assert.That(result.Error, Is.InstanceOf<ValidationError>(), "Should return ValidationError");
    }

    /// <summary>
    /// RED: Test should fail initially - should validate priority range
    /// </summary>
    [Test]
    public async Task FilterConstraintsByPriorityAsync_WhenInvalidRange_ShouldReturnValidationError()
    {
        // Given
        var minPriority = 0.9;
        var maxPriority = 0.1; // Invalid: min > max

        // When
        var result = await _navigator.FilterConstraintsByPriorityAsync(_testLibrary, minPriority, maxPriority);

        // Then
        Assert.That(result.IsError, Is.True, "Should return error for invalid range");
        Assert.That(result.Error.Message, Does.Contain("Invalid"), "Should explain invalid range");
    }

    /// <summary>
    /// RED: Test should fail initially - should handle empty search keyword
    /// </summary>
    [Test]
    public async Task SearchConstraintsByKeywordAsync_WhenKeywordEmpty_ShouldReturnAllConstraints()
    {
        // Given
        var emptyKeyword = string.Empty;

        // When
        var result = await _navigator.SearchConstraintsByKeywordAsync(_testLibrary, emptyKeyword);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should handle empty keyword gracefully");
        Assert.That(result.Value.Count(), Is.EqualTo(_testLibrary.AtomicConstraints.Count + _testLibrary.CompositeConstraints.Count),
            "Should return all constraints when keyword is empty");
    }

    /// <summary>
    /// RED: Test should fail initially - should provide navigation statistics
    /// </summary>
    [Test]
    public async Task GetNavigationStatisticsAsync_WhenAnalyzingLibrary_ShouldProvideStats()
    {
        // When
        var result = await _navigator.GetNavigationStatisticsAsync(_testLibrary);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully generate statistics");

        var stats = result.Value;
        Assert.That(stats, Is.Not.Null, "Should return statistics");
        Assert.That(stats.TotalConstraints, Is.GreaterThan(0), "Should count total constraints");
        Assert.That(stats.AtomicConstraintsCount, Is.EqualTo(_testLibrary.AtomicConstraints.Count), "Should count atomic constraints");
        Assert.That(stats.CompositeConstraintsCount, Is.EqualTo(_testLibrary.CompositeConstraints.Count), "Should count composite constraints");
    }

    // ===================
    // Helper Methods
    // ===================

    private static ConstraintLibrary CreateTestConstraintLibrary()
    {
        var library = new ConstraintLibrary("1.0.0", "Test Navigation Library");

        // Add atomic constraints
        library.AddAtomicConstraint(CreateTestAtomicConstraint("test.constraint.1", "Test Constraint 1", 0.8));
        library.AddAtomicConstraint(CreateTestAtomicConstraint("test.constraint.2", "Test Constraint 2", 0.6));
        library.AddAtomicConstraint(CreateTestAtomicConstraint("child.constraint.1", "Child Constraint 1", 0.7));
        library.AddAtomicConstraint(CreateTestAtomicConstraint("child.constraint.2", "Child Constraint 2", 0.9));
        library.AddAtomicConstraint(CreateTestAtomicConstraint("other.constraint", "Other Constraint", 0.5));

        // Add composite constraint that references children
        var composite = CreateTestCompositeConstraint("composite.test", "Test Composite", 0.85);
        library.AddCompositeConstraint(composite);

        return library;
    }

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
}
