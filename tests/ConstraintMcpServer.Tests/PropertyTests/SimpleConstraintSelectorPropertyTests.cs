using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Common;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.PropertyTests;

/// <summary>
/// Simplified property-based tests for ConstraintSelector focusing on business rules.
/// Uses explicit test cases to validate constraint selection logic.
/// </summary>
[TestFixture]
[Category("Property")]
public sealed class SimpleConstraintSelectorPropertyTests
{
    private readonly ConstraintSelector _selector = new();
    private static readonly string[] reminders = new[] { "This applies to RED and GREEN phases" };

    [Test]
    public void High_Priority_Constraints_Get_Precedence_In_Selection()
    {
        // Arrange: Create constraints with different priorities for same context
        var context = new UserDefinedContext("workflow", "red", 0.9);
        var constraints = new List<Constraint>
        {
            CreateConstraint("low-priority", 0.1, context),
            CreateConstraint("high-priority", 0.9, context),
            CreateConstraint("medium-priority", 0.5, context)
        };

        // Act: Select all constraints
        var selected = ConstraintSelector.SelectConstraints(constraints, context, topK: 3).ToList();

        // Assert: Verify priority order (highest first)
        Assert.That(selected, Has.Count.EqualTo(3), "Should select all applicable constraints");
        Assert.Multiple(() =>
        {
            Assert.That(selected[0].Id.Value, Is.EqualTo("high-priority"), "Highest priority first");
            Assert.That(selected[1].Id.Value, Is.EqualTo("medium-priority"), "Medium priority second");
            Assert.That(selected[2].Id.Value, Is.EqualTo("low-priority"), "Lowest priority last");
        });
    }

    [Test]
    public void Phase_Relevant_Constraints_Are_Only_Ones_Considered()
    {
        // Arrange: Create constraints for different phases
        var redContext = new UserDefinedContext("workflow", "red", 0.9);
        var greenContext = new UserDefinedContext("workflow", "green", 0.8);
        var refactorContext = new UserDefinedContext("workflow", "refactor", 0.7);

        var constraints = new List<Constraint>
        {
            CreateConstraint("red-constraint", 0.8, redContext),
            CreateConstraint("green-constraint", 0.7, greenContext),
            CreateConstraint("refactor-constraint", 0.6, refactorContext)
        };

        // Act: Select constraints for RED phase only
        var selectedForRed = ConstraintSelector.SelectConstraints(constraints, redContext, topK: 10).ToList();

        // Assert: Only RED phase constraint should be selected
        Assert.That(selectedForRed, Has.Count.EqualTo(1), "Should select only phase-relevant constraints");
        Assert.Multiple(() =>
        {
            Assert.That(selectedForRed[0].Id.Value, Is.EqualTo("red-constraint"), "Should select RED phase constraint");
            Assert.That(selectedForRed.All(c => c.AppliesTo(redContext)), Is.True, "All selected must apply to target phase");
        });
    }

    [Test]
    public void TopK_Limit_Respects_Developer_Cognitive_Load_Constraints()
    {
        // Arrange: Create many constraints for same phase
        var context = new UserDefinedContext("workflow", "green", 0.8);
        var constraints = new List<Constraint>();
        for (int i = 0; i < 10; i++)
        {
            constraints.Add(CreateConstraint($"constraint-{i}", 0.5 + (i * 0.04), context));
        }

        // Test different topK limits
        int[] testCases = new[] { 1, 3, 5, 7, 10, 15 };

        foreach (int topK in testCases)
        {
            // Act: Select with cognitive load limit
            var selected = ConstraintSelector.SelectConstraints(constraints, context, topK).ToList();

            // Assert: Selection respects cognitive load limits
            int expectedCount = Math.Min(topK, constraints.Count);
            Assert.That(selected, Has.Count.EqualTo(expectedCount),
                $"Cognitive load limit must be respected: topK={topK}");
        }
    }

    [Test]
    public void Selection_Is_Deterministic_For_Consistent_Developer_Experience()
    {
        // Arrange: Create constraint set
        var context = new UserDefinedContext("workflow", "refactor", 0.7);
        var constraints = new List<Constraint>
        {
            CreateConstraint("constraint-a", 0.8, context),
            CreateConstraint("constraint-b", 0.6, context),
            CreateConstraint("constraint-c", 0.9, context),
            CreateConstraint("constraint-d", 0.4, context),
            CreateConstraint("constraint-e", 0.7, context)
        };
        int topK = 3;

        // Act: Select same constraints multiple times
        var selection1 = ConstraintSelector.SelectConstraints(constraints, context, topK).Select(c => c.Id.Value).ToList();
        var selection2 = ConstraintSelector.SelectConstraints(constraints, context, topK).Select(c => c.Id.Value).ToList();
        var selection3 = ConstraintSelector.SelectConstraints(constraints, context, topK).Select(c => c.Id.Value).ToList();

        Assert.Multiple(() =>
        {
            // Assert: All selections should be identical
            Assert.That(selection1, Is.EqualTo(selection2), "Selection 1 vs 2 must be deterministic");
            Assert.That(selection2, Is.EqualTo(selection3), "Selection 2 vs 3 must be deterministic");
        });

        // Verify correct order (by priority)
        string[] expectedOrder = new[] { "constraint-c", "constraint-a", "constraint-e" }; // 0.9, 0.8, 0.7
        Assert.That(selection1, Is.EqualTo(expectedOrder), "Selection must follow priority order");
    }

    [Test]
    public void Empty_Constraint_Set_Produces_Empty_Selection_Gracefully()
    {
        // Arrange: Empty constraint set
        var emptyConstraints = new List<Constraint>();
        var context = new UserDefinedContext("workflow", "commit", 0.5);

        // Act: Attempt selection with empty set
        var selected = ConstraintSelector.SelectConstraints(emptyConstraints, context, topK: 5).ToList();

        // Assert: Graceful handling of empty case
        Assert.That(selected, Is.Not.Null, "Empty selection must not return null");
        Assert.That(selected.Count, Is.EqualTo(0), "Empty constraint set should produce empty selection");
    }

    [Test]
    public void Priority_Based_Selection_Maximizes_Constraint_Value_Delivery()
    {
        // Arrange: Create constraints with known priorities
        var context = new UserDefinedContext("workflow", "commit", 0.5);
        var constraints = new List<Constraint>
        {
            CreateConstraint("priority-50", 0.50, context),
            CreateConstraint("priority-90", 0.90, context),
            CreateConstraint("priority-30", 0.30, context),
            CreateConstraint("priority-70", 0.70, context),
            CreateConstraint("priority-80", 0.80, context)
        };

        // Test different topK values
        var testCases = new[]
        {
            new { TopK = 1, Expected = new[] { "priority-90" } },
            new { TopK = 2, Expected = new[] { "priority-90", "priority-80" } },
            new { TopK = 3, Expected = new[] { "priority-90", "priority-80", "priority-70" } },
            new { TopK = 5, Expected = new[] { "priority-90", "priority-80", "priority-70", "priority-50", "priority-30" } }
        };

        foreach (var testCase in testCases)
        {
            // Act: Select top-K constraints
            string[] selected = ConstraintSelector.SelectConstraints(constraints, context, testCase.TopK)
                .Select(c => c.Id.Value).ToArray();

            // Assert: Selected constraints maximize value
            Assert.That(selected, Is.EqualTo(testCase.Expected),
                $"Priority-based selection must maximize value delivery for topK={testCase.TopK}");
        }
    }

    [Test]
    public void Multi_Phase_Constraints_Apply_To_All_Relevant_Phases()
    {
        // Arrange: Create constraint that applies to multiple phases
        var redContext = new UserDefinedContext("workflow", "red", 0.9);
        var greenContext = new UserDefinedContext("workflow", "green", 0.8);
        var refactorContext = new UserDefinedContext("workflow", "refactor", 0.7);

        var multiPhaseConstraint = new Constraint(
            new ConstraintId("multi-phase-constraint"),
            "Multi-phase business rule",
            new Priority(0.8),
            new[] { redContext, greenContext }, // Applies to RED and GREEN
            reminders
        );

        var constraints = new List<Constraint> { multiPhaseConstraint };

        // Act & Assert: Test each phase
        var redSelection = ConstraintSelector.SelectConstraints(constraints, redContext, topK: 5).ToList();
        Assert.That(redSelection, Has.Count.EqualTo(1), "Multi-phase constraint should apply to RED");

        var greenSelection = ConstraintSelector.SelectConstraints(constraints, greenContext, topK: 5).ToList();
        Assert.That(greenSelection, Has.Count.EqualTo(1), "Multi-phase constraint should apply to GREEN");

        var refactorSelection = ConstraintSelector.SelectConstraints(constraints, refactorContext, topK: 5).ToList();
        Assert.That(refactorSelection.Count, Is.EqualTo(0), "Multi-phase constraint should NOT apply to REFACTOR");
    }

    [Test]
    public void Business_Scenario_TDD_Constraint_Selection()
    {
        // Arrange: Typical TDD constraint scenario
        var redContext = new UserDefinedContext("workflow", "red", 0.9);
        var greenContext = new UserDefinedContext("workflow", "green", 0.8);
        var refactorContext = new UserDefinedContext("workflow", "refactor", 0.7);

        var constraints = new List<Constraint>
        {
            CreateConstraint("tdd-test-first", 0.95, redContext), // Critical for RED phase
            CreateConstraint("simple-solution", 0.80, greenContext), // Important for GREEN phase
            CreateConstraint("clean-code", 0.85, refactorContext), // Important for REFACTOR phase
            CreateConstraint("yagni", 0.75, greenContext, refactorContext), // Applies to both GREEN and REFACTOR
            CreateConstraint("general-principle", 0.40, redContext, greenContext, refactorContext) // Low priority, all phases
        };

        // Act & Assert: Test RED phase selection
        string[] redSelection = ConstraintSelector.SelectConstraints(constraints, redContext, topK: 3)
            .Select(c => c.Id.Value).ToArray();
        Assert.That(redSelection, Is.EqualTo(new[] { "tdd-test-first", "general-principle" }),
            "RED phase should prioritize test-first approach");

        // Act & Assert: Test GREEN phase selection  
        string[] greenSelection = ConstraintSelector.SelectConstraints(constraints, greenContext, topK: 3)
            .Select(c => c.Id.Value).ToArray();
        Assert.That(greenSelection, Is.EqualTo(new[] { "simple-solution", "yagni", "general-principle" }),
            "GREEN phase should prioritize simple solutions");

        // Act & Assert: Test REFACTOR phase selection
        string[] refactorSelection = ConstraintSelector.SelectConstraints(constraints, refactorContext, topK: 3)
            .Select(c => c.Id.Value).ToArray();
        Assert.That(refactorSelection, Is.EqualTo(new[] { "clean-code", "yagni", "general-principle" }),
            "REFACTOR phase should prioritize clean code");
    }

    #region Helper Methods

    private static Constraint CreateConstraint(string id, double priority, params UserDefinedContext[] workflowContexts)
    {
        return new Constraint(
            new ConstraintId(id),
            $"Test constraint: {id}",
            new Priority(priority),
            workflowContexts,
            new[] { $"Reminder for {id}" }
        );
    }

    #endregion
}
