using NUnit.Framework;
using ConstraintMcpServer.Domain.Composition;

namespace ConstraintMcpServer.Tests.Domain.Composition;

/// <summary>
/// Unit tests for HierarchicalComposition strategy - drives implementation through TDD.
/// Tests architectural pattern orchestration with hierarchy-based prioritization.
/// </summary>
[TestFixture]
[Category("Unit")]
public class HierarchicalCompositionTests
{
    /// <summary>
    /// RED: This test will fail and drive HierarchicalComposition implementation
    /// Business requirement: Prioritize constraints by hierarchy levels (Architecture → Implementation → Testing)
    /// </summary>
    [Test]
    public void HierarchicalComposition_Should_Prioritize_Architecture_Level_Constraints_First()
    {
        // Arrange
        var composition = new HierarchicalComposition();
        var architectureConstraints = new[]
        {
            new HierarchicalConstraintInfo("arch.solid-principles", hierarchyLevel: 0, priority: 0.9),
            new HierarchicalConstraintInfo("impl.clean-code", hierarchyLevel: 1, priority: 0.8),
            new HierarchicalConstraintInfo("test.unit-tests", hierarchyLevel: 2, priority: 0.7)
        };

        // Act
        var result = composition.GetConstraintsByHierarchy(architectureConstraints);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(3));

        // Architecture constraints should be first (hierarchy level 0)
        var first = result.First();
        Assert.That(first.ConstraintId, Is.EqualTo("arch.solid-principles"));
        Assert.That(first.HierarchyLevel, Is.EqualTo(0));
    }

    /// <summary>
    /// RED: Test will drive hierarchy level ordering implementation
    /// Business requirement: Maintain strict hierarchy ordering regardless of priority values
    /// </summary>
    [Test]
    public void HierarchicalComposition_Should_Order_By_Hierarchy_Level_Not_Priority()
    {
        // Arrange
        var composition = new HierarchicalComposition();
        var constraints = new[]
        {
            new HierarchicalConstraintInfo("test.integration", hierarchyLevel: 2, priority: 0.95), // High priority, low hierarchy
            new HierarchicalConstraintInfo("arch.patterns", hierarchyLevel: 0, priority: 0.6),    // Low priority, high hierarchy
            new HierarchicalConstraintInfo("impl.refactor", hierarchyLevel: 1, priority: 0.8)     // Medium priority, medium hierarchy
        };

        // Act
        var result = composition.GetConstraintsByHierarchy(constraints).ToList();

        // Assert - Should be ordered by hierarchy level (0, 1, 2), not priority
        Assert.That(result[0].HierarchyLevel, Is.EqualTo(0));
        Assert.That(result[0].ConstraintId, Is.EqualTo("arch.patterns"));

        Assert.That(result[1].HierarchyLevel, Is.EqualTo(1));
        Assert.That(result[1].ConstraintId, Is.EqualTo("impl.refactor"));

        Assert.That(result[2].HierarchyLevel, Is.EqualTo(2));
        Assert.That(result[2].ConstraintId, Is.EqualTo("test.integration"));
    }

    /// <summary>
    /// RED: Test will drive same-level priority handling
    /// Business requirement: Within same hierarchy level, use priority for ordering
    /// </summary>
    [Test]
    public void HierarchicalComposition_Should_Use_Priority_Within_Same_Hierarchy_Level()
    {
        // Arrange
        var composition = new HierarchicalComposition();
        var constraints = new[]
        {
            new HierarchicalConstraintInfo("arch.solid", hierarchyLevel: 0, priority: 0.7),
            new HierarchicalConstraintInfo("arch.patterns", hierarchyLevel: 0, priority: 0.9),
            new HierarchicalConstraintInfo("arch.clean", hierarchyLevel: 0, priority: 0.8)
        };

        // Act
        var result = composition.GetConstraintsByHierarchy(constraints).ToList();

        // Assert - Within level 0, should be ordered by priority (highest first)
        Assert.That(result[0].Priority, Is.EqualTo(0.9));
        Assert.That(result[0].ConstraintId, Is.EqualTo("arch.patterns"));

        Assert.That(result[1].Priority, Is.EqualTo(0.8));
        Assert.That(result[1].ConstraintId, Is.EqualTo("arch.clean"));

        Assert.That(result[2].Priority, Is.EqualTo(0.7));
        Assert.That(result[2].ConstraintId, Is.EqualTo("arch.solid"));
    }
}
