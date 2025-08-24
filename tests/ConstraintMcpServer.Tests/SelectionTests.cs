using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Domain;
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
        var redPhase = new Phase("red");
        Constraint constraintLow = CreateTestConstraint("low", 0.3, redPhase);
        Constraint constraintHigh = CreateTestConstraint("high", 0.9, redPhase);
        Constraint constraintMid = CreateTestConstraint("mid", 0.6, redPhase);
        var constraints = new List<Constraint> { constraintLow, constraintHigh, constraintMid };

        var selector = new ConstraintSelector();

        // Act - select constraints for red phase
        IReadOnlyList<Constraint> selected = selector.SelectConstraints(constraints, redPhase, topK: 3);

        // Assert - should be sorted by priority (high to low)
        Assert.That(selected.Count, Is.EqualTo(3));
        Assert.That(selected[0].Id.Value, Is.EqualTo("high"));
        Assert.That(selected[1].Id.Value, Is.EqualTo("mid"));
        Assert.That(selected[2].Id.Value, Is.EqualTo("low"));
    }

    [Test]
    public void SelectConstraints_FiltersConstraintsByPhase()
    {
        // Arrange - constraints for different phases
        var redPhase = new Phase("red");
        var greenPhase = new Phase("green");
        var refactorPhase = new Phase("refactor");

        Constraint redConstraint = CreateTestConstraint("red", 0.9, redPhase);
        Constraint greenConstraint = CreateTestConstraint("green", 0.8, greenPhase);
        Constraint refactorConstraint = CreateTestConstraint("refactor", 0.7, refactorPhase);
        var constraints = new List<Constraint> { redConstraint, greenConstraint, refactorConstraint };

        var selector = new ConstraintSelector();

        // Act - select only red phase constraints
        IReadOnlyList<Constraint> selected = selector.SelectConstraints(constraints, redPhase, topK: 10);

        // Assert - should only include red phase constraint
        Assert.That(selected.Count, Is.EqualTo(1));
        Assert.That(selected[0].Id.Value, Is.EqualTo("red"));
    }

    [Test]
    public void SelectConstraints_RespectsTopKLimit()
    {
        // Arrange - many constraints for same phase
        var redPhase = new Phase("red");
        var constraints = new List<Constraint>
        {
            CreateTestConstraint("first", 0.9, redPhase),
            CreateTestConstraint("second", 0.8, redPhase),
            CreateTestConstraint("third", 0.7, redPhase),
            CreateTestConstraint("fourth", 0.6, redPhase),
            CreateTestConstraint("fifth", 0.5, redPhase)
        };

        var selector = new ConstraintSelector();

        // Act - select only top 2 constraints
        IReadOnlyList<Constraint> selected = selector.SelectConstraints(constraints, redPhase, topK: DefaultTopK);

        // Assert - should only return top 2 by priority
        Assert.That(selected.Count, Is.EqualTo(DefaultTopK));
        Assert.That(selected[0].Id.Value, Is.EqualTo("first"));
        Assert.That(selected[1].Id.Value, Is.EqualTo("second"));
    }

    [Test]
    public void SelectConstraints_MultiPhaseConstraintMatchesPhase()
    {
        // Arrange - constraint that applies to multiple phases
        var redPhase = new Phase("red");
        var greenPhase = new Phase("green");
        Constraint multiPhaseConstraint = CreateTestConstraint("multi", 0.9, redPhase, greenPhase);
        var constraints = new List<Constraint> { multiPhaseConstraint };

        var selector = new ConstraintSelector();

        // Act - select for green phase
        IReadOnlyList<Constraint> selected = selector.SelectConstraints(constraints, greenPhase, topK: 10);

        // Assert - multi-phase constraint should be included
        Assert.That(selected.Count, Is.EqualTo(1));
        Assert.That(selected[0].Id.Value, Is.EqualTo("multi"));
    }

    [Test]
    public void SelectConstraints_EmptyListWhenNoPhaseMatch()
    {
        // Arrange - constraint for different phase
        var redPhase = new Phase("red");
        var greenPhase = new Phase("green");
        Constraint redConstraint = CreateTestConstraint("red", 0.9, redPhase);
        var constraints = new List<Constraint> { redConstraint };

        var selector = new ConstraintSelector();

        // Act - select for different phase
        IReadOnlyList<Constraint> selected = selector.SelectConstraints(constraints, greenPhase, topK: 10);

        // Assert - should return empty list
        Assert.That(selected, Is.Empty);
    }

    private static Constraint CreateTestConstraint(string id, double priority, params Phase[] phases)
    {
        return new Constraint(
            new ConstraintId(id),
            $"Test constraint {id}",
            new Priority(priority),
            phases,
            new[] { $"Reminder for {id}" }
        );
    }
}
