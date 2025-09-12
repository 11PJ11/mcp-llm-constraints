using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ConstraintMcpServer.Domain.Composition;
using ConstraintMcpServer.Domain.Common;

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
            new UserDefinedHierarchicalConstraintInfo("arch.solid-principles", 0, 0.9, "SOLID principles constraint"),
            new UserDefinedHierarchicalConstraintInfo("impl.clean-code", 1, 0.8, "Clean code practices constraint"),
            new UserDefinedHierarchicalConstraintInfo("test.unit-tests", 2, 0.7, "Unit testing constraint")
        };

        var userDefinedHierarchy = new UserDefinedHierarchy(
            "clean-architecture",
            new Dictionary<int, string>
            {
                { 0, "Architecture Principles" },
                { 1, "Implementation" },
                { 2, "Testing" }
            },
            "Clean Architecture hierarchy");

        // Act
        var result = composition.GetConstraintsByHierarchy(architectureConstraints, userDefinedHierarchy);

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
            new UserDefinedHierarchicalConstraintInfo("test.integration", 2, 0.95, "Integration testing constraint"), // High priority, low hierarchy
            new UserDefinedHierarchicalConstraintInfo("arch.patterns", 0, 0.6, "Architecture patterns constraint"),    // Low priority, high hierarchy
            new UserDefinedHierarchicalConstraintInfo("impl.refactor", 1, 0.8, "Refactoring constraint")     // Medium priority, medium hierarchy
        };

        var userDefinedHierarchy = new UserDefinedHierarchy(
            "clean-architecture",
            new Dictionary<int, string>
            {
                { 0, "Architecture Patterns" },
                { 1, "Implementation" },
                { 2, "Testing Integration" }
            },
            "Clean Architecture hierarchy");

        // Act
        var result = composition.GetConstraintsByHierarchy(constraints, userDefinedHierarchy).ToList();

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
            new UserDefinedHierarchicalConstraintInfo("arch.solid", 0, 0.7, "SOLID architecture constraint"),
            new UserDefinedHierarchicalConstraintInfo("arch.patterns", 0, 0.9, "Architecture patterns constraint"),
            new UserDefinedHierarchicalConstraintInfo("arch.clean", 0, 0.8, "Clean architecture constraint")
        };

        var userDefinedHierarchy = new UserDefinedHierarchy(
            "clean-architecture",
            new Dictionary<int, string>
            {
                { 0, "Architecture Principles" }
            },
            "Clean Architecture hierarchy");

        // Act
        var result = composition.GetConstraintsByHierarchy(constraints, userDefinedHierarchy).ToList();

        // Assert - Within level 0, should be ordered by priority (highest first)
        Assert.That(result[0].Priority, Is.EqualTo(0.9));
        Assert.That(result[0].ConstraintId, Is.EqualTo("arch.patterns"));

        Assert.That(result[1].Priority, Is.EqualTo(0.8));
        Assert.That(result[1].ConstraintId, Is.EqualTo("arch.clean"));

        Assert.That(result[2].Priority, Is.EqualTo(0.7));
        Assert.That(result[2].ConstraintId, Is.EqualTo("arch.solid"));
    }
}
