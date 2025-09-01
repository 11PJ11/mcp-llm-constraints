using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Common;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests;

/// <summary>
/// Unit tests for constraint selection logic.
/// Tests priority-based sorting and phase filtering.
/// </summary>
[TestFixture]
public sealed class SelectionTests
{
    private const int DefaultTopK = 2;

    [Test]
    public void SelectConstraints_SortsConstraintsByPriorityDescending()
    {
        // Arrange - constraints with different priorities
        var redContext = new UserDefinedContext("tdd-phase", "red", 0.9);
        Constraint constraintLow = CreateTestConstraint("low", 0.3, redContext);
        Constraint constraintHigh = CreateTestConstraint("high", 0.9, redContext);
        Constraint constraintMid = CreateTestConstraint("mid", 0.6, redContext);
        var constraints = new List<Constraint> { constraintLow, constraintHigh, constraintMid };

        var selector = new ConstraintSelector();

        // Act - select constraints for red phase
        IReadOnlyList<Constraint> selected = ConstraintSelector.SelectConstraints(constraints, redContext, topK: 3);

        // Assert - should be sorted by priority (high to low)
        Assert.That(selected, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(selected[0].Id.Value, Is.EqualTo("high"));
            Assert.That(selected[1].Id.Value, Is.EqualTo("mid"));
            Assert.That(selected[2].Id.Value, Is.EqualTo("low"));
        });
    }

    [Test]
    public void SelectConstraints_FiltersConstraintsByPhase()
    {
        // Arrange - constraints for different contexts
        var redContext = new UserDefinedContext("tdd-phase", "red", 0.9);
        var greenContext = new UserDefinedContext("tdd-phase", "green", 0.8);
        var refactorContext = new UserDefinedContext("tdd-phase", "refactor", 0.7);

        Constraint redConstraint = CreateTestConstraint("red", 0.9, redContext);
        Constraint greenConstraint = CreateTestConstraint("green", 0.8, greenContext);
        Constraint refactorConstraint = CreateTestConstraint("refactor", 0.7, refactorContext);
        var constraints = new List<Constraint> { redConstraint, greenConstraint, refactorConstraint };

        var selector = new ConstraintSelector();

        // Act - select only red context constraints
        IReadOnlyList<Constraint> selected = ConstraintSelector.SelectConstraints(constraints, redContext, topK: 10);

        // Assert - should only include red context constraint
        Assert.That(selected, Has.Count.EqualTo(1));
        Assert.That(selected[0].Id.Value, Is.EqualTo("red"));
    }

    [Test]
    public void SelectConstraints_RespectsTopKLimit()
    {
        // Arrange - many constraints for same context
        var redContext = new UserDefinedContext("tdd-phase", "red", 0.9);
        var constraints = new List<Constraint>
        {
            CreateTestConstraint("first", 0.9, redContext),
            CreateTestConstraint("second", 0.8, redContext),
            CreateTestConstraint("third", 0.7, redContext),
            CreateTestConstraint("fourth", 0.6, redContext),
            CreateTestConstraint("fifth", 0.5, redContext)
        };

        var selector = new ConstraintSelector();

        // Act - select only top 2 constraints
        IReadOnlyList<Constraint> selected = ConstraintSelector.SelectConstraints(constraints, redContext, topK: DefaultTopK);

        // Assert - should only return top 2 by priority
        Assert.That(selected, Has.Count.EqualTo(DefaultTopK));
        Assert.Multiple(() =>
        {
            Assert.That(selected[0].Id.Value, Is.EqualTo("first"));
            Assert.That(selected[1].Id.Value, Is.EqualTo("second"));
        });
    }

    [Test]
    public void SelectConstraints_MultiPhaseConstraintMatchesPhase()
    {
        // Arrange - constraint that applies to multiple contexts
        var redContext = new UserDefinedContext("tdd-phase", "red", 0.9);
        var greenContext = new UserDefinedContext("tdd-phase", "green", 0.8);
        Constraint multiPhaseConstraint = CreateTestConstraint("multi", 0.9, redContext, greenContext);
        var constraints = new List<Constraint> { multiPhaseConstraint };

        var selector = new ConstraintSelector();

        // Act - select for green context
        IReadOnlyList<Constraint> selected = ConstraintSelector.SelectConstraints(constraints, greenContext, topK: 10);

        // Assert - multi-phase constraint should be included
        Assert.That(selected, Has.Count.EqualTo(1));
        Assert.That(selected[0].Id.Value, Is.EqualTo("multi"));
    }

    [Test]
    public void SelectConstraints_EmptyListWhenNoPhaseMatch()
    {
        // Arrange - constraint for different context
        var redContext = new UserDefinedContext("tdd-phase", "red", 0.9);
        var greenContext = new UserDefinedContext("tdd-phase", "green", 0.8);
        Constraint redConstraint = CreateTestConstraint("red", 0.9, redContext);
        var constraints = new List<Constraint> { redConstraint };

        var selector = new ConstraintSelector();

        // Act - select for different context
        IReadOnlyList<Constraint> selected = ConstraintSelector.SelectConstraints(constraints, greenContext, topK: 10);

        // Assert - should return empty list
        Assert.That(selected, Is.Empty);
    }

    private static Constraint CreateTestConstraint(string id, double priority, params UserDefinedContext[] workflowContexts)
    {
        return new Constraint(
            new ConstraintId(id),
            $"Test constraint {id}",
            new Priority(priority),
            workflowContexts,
            new[] { $"Reminder for {id}" }
        );
    }
}
