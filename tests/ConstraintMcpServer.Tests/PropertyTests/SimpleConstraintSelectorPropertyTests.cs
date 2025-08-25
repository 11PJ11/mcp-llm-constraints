using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Domain;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.PropertyTests;

/// <summary>
/// Simplified property-based tests for ConstraintSelector focusing on business rules.
/// Uses explicit test cases to validate constraint selection logic.
/// </summary>
[TestFixture]
public sealed class SimpleConstraintSelectorPropertyTests
{
    private readonly ConstraintSelector _selector = new();

    [Test]
    public void High_Priority_Constraints_Get_Precedence_In_Selection()
    {
        // Arrange: Create constraints with different priorities for same phase
        var phase = new Phase("red");
        var constraints = new List<Constraint>
        {
            CreateConstraint("low-priority", 0.1, phase),
            CreateConstraint("high-priority", 0.9, phase),
            CreateConstraint("medium-priority", 0.5, phase)
        };

        // Act: Select all constraints
        var selected = _selector.SelectConstraints(constraints, phase, topK: 3).ToList();

        // Assert: Verify priority order (highest first)
        Assert.That(selected.Count, Is.EqualTo(3), "Should select all applicable constraints");
        Assert.That(selected[0].Id.Value, Is.EqualTo("high-priority"), "Highest priority first");
        Assert.That(selected[1].Id.Value, Is.EqualTo("medium-priority"), "Medium priority second");
        Assert.That(selected[2].Id.Value, Is.EqualTo("low-priority"), "Lowest priority last");
    }

    [Test]
    public void Phase_Relevant_Constraints_Are_Only_Ones_Considered()
    {
        // Arrange: Create constraints for different phases
        var redPhase = new Phase("red");
        var greenPhase = new Phase("green");
        var refactorPhase = new Phase("refactor");

        var constraints = new List<Constraint>
        {
            CreateConstraint("red-constraint", 0.8, redPhase),
            CreateConstraint("green-constraint", 0.7, greenPhase),
            CreateConstraint("refactor-constraint", 0.6, refactorPhase)
        };

        // Act: Select constraints for RED phase only
        var selectedForRed = _selector.SelectConstraints(constraints, redPhase, topK: 10).ToList();

        // Assert: Only RED phase constraint should be selected
        Assert.That(selectedForRed.Count, Is.EqualTo(1), "Should select only phase-relevant constraints");
        Assert.That(selectedForRed[0].Id.Value, Is.EqualTo("red-constraint"), "Should select RED phase constraint");
        Assert.That(selectedForRed.All(c => c.AppliesTo(redPhase)), Is.True, "All selected must apply to target phase");
    }

    [Test]
    public void TopK_Limit_Respects_Developer_Cognitive_Load_Constraints()
    {
        // Arrange: Create many constraints for same phase
        var phase = new Phase("green");
        var constraints = new List<Constraint>();
        for (int i = 0; i < 10; i++)
        {
            constraints.Add(CreateConstraint($"constraint-{i}", 0.5 + (i * 0.04), phase));
        }

        // Test different topK limits
        int[] testCases = new[] { 1, 3, 5, 7, 10, 15 };

        foreach (int topK in testCases)
        {
            // Act: Select with cognitive load limit
            var selected = _selector.SelectConstraints(constraints, phase, topK).ToList();

            // Assert: Selection respects cognitive load limits
            int expectedCount = Math.Min(topK, constraints.Count);
            Assert.That(selected.Count, Is.EqualTo(expectedCount),
                $"Cognitive load limit must be respected: topK={topK}");
        }
    }

    [Test]
    public void Selection_Is_Deterministic_For_Consistent_Developer_Experience()
    {
        // Arrange: Create constraint set
        var phase = new Phase("refactor");
        var constraints = new List<Constraint>
        {
            CreateConstraint("constraint-a", 0.8, phase),
            CreateConstraint("constraint-b", 0.6, phase),
            CreateConstraint("constraint-c", 0.9, phase),
            CreateConstraint("constraint-d", 0.4, phase),
            CreateConstraint("constraint-e", 0.7, phase)
        };
        int topK = 3;

        // Act: Select same constraints multiple times
        var selection1 = _selector.SelectConstraints(constraints, phase, topK).Select(c => c.Id.Value).ToList();
        var selection2 = _selector.SelectConstraints(constraints, phase, topK).Select(c => c.Id.Value).ToList();
        var selection3 = _selector.SelectConstraints(constraints, phase, topK).Select(c => c.Id.Value).ToList();

        // Assert: All selections should be identical
        Assert.That(selection1, Is.EqualTo(selection2), "Selection 1 vs 2 must be deterministic");
        Assert.That(selection2, Is.EqualTo(selection3), "Selection 2 vs 3 must be deterministic");

        // Verify correct order (by priority)
        string[] expectedOrder = new[] { "constraint-c", "constraint-a", "constraint-e" }; // 0.9, 0.8, 0.7
        Assert.That(selection1, Is.EqualTo(expectedOrder), "Selection must follow priority order");
    }

    [Test]
    public void Empty_Constraint_Set_Produces_Empty_Selection_Gracefully()
    {
        // Arrange: Empty constraint set
        var emptyConstraints = new List<Constraint>();
        var phase = new Phase("commit");

        // Act: Attempt selection with empty set
        var selected = _selector.SelectConstraints(emptyConstraints, phase, topK: 5).ToList();

        // Assert: Graceful handling of empty case
        Assert.That(selected, Is.Not.Null, "Empty selection must not return null");
        Assert.That(selected.Count, Is.EqualTo(0), "Empty constraint set should produce empty selection");
    }

    [Test]
    public void Priority_Based_Selection_Maximizes_Constraint_Value_Delivery()
    {
        // Arrange: Create constraints with known priorities
        var phase = new Phase("commit");
        var constraints = new List<Constraint>
        {
            CreateConstraint("priority-50", 0.50, phase),
            CreateConstraint("priority-90", 0.90, phase),
            CreateConstraint("priority-30", 0.30, phase),
            CreateConstraint("priority-70", 0.70, phase),
            CreateConstraint("priority-80", 0.80, phase)
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
            string[] selected = _selector.SelectConstraints(constraints, phase, testCase.TopK)
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
        var redPhase = new Phase("red");
        var greenPhase = new Phase("green");
        var refactorPhase = new Phase("refactor");

        var multiPhaseConstraint = new Constraint(
            new ConstraintId("multi-phase-constraint"),
            "Multi-phase business rule",
            new Priority(0.8),
            new[] { redPhase, greenPhase }, // Applies to RED and GREEN
            new[] { "This applies to RED and GREEN phases" }
        );

        var constraints = new List<Constraint> { multiPhaseConstraint };

        // Act & Assert: Test each phase
        var redSelection = _selector.SelectConstraints(constraints, redPhase, topK: 5).ToList();
        Assert.That(redSelection.Count, Is.EqualTo(1), "Multi-phase constraint should apply to RED");

        var greenSelection = _selector.SelectConstraints(constraints, greenPhase, topK: 5).ToList();
        Assert.That(greenSelection.Count, Is.EqualTo(1), "Multi-phase constraint should apply to GREEN");

        var refactorSelection = _selector.SelectConstraints(constraints, refactorPhase, topK: 5).ToList();
        Assert.That(refactorSelection.Count, Is.EqualTo(0), "Multi-phase constraint should NOT apply to REFACTOR");
    }

    [Test]
    public void Business_Scenario_TDD_Constraint_Selection()
    {
        // Arrange: Typical TDD constraint scenario
        var redPhase = new Phase("red");
        var greenPhase = new Phase("green");
        var refactorPhase = new Phase("refactor");

        var constraints = new List<Constraint>
        {
            CreateConstraint("tdd-test-first", 0.95, redPhase), // Critical for RED phase
            CreateConstraint("simple-solution", 0.80, greenPhase), // Important for GREEN phase
            CreateConstraint("clean-code", 0.85, refactorPhase), // Important for REFACTOR phase
            CreateConstraint("yagni", 0.75, greenPhase, refactorPhase), // Applies to both GREEN and REFACTOR
            CreateConstraint("general-principle", 0.40, redPhase, greenPhase, refactorPhase) // Low priority, all phases
        };

        // Act & Assert: Test RED phase selection
        string[] redSelection = _selector.SelectConstraints(constraints, redPhase, topK: 3)
            .Select(c => c.Id.Value).ToArray();
        Assert.That(redSelection, Is.EqualTo(new[] { "tdd-test-first", "general-principle" }),
            "RED phase should prioritize test-first approach");

        // Act & Assert: Test GREEN phase selection  
        string[] greenSelection = _selector.SelectConstraints(constraints, greenPhase, topK: 3)
            .Select(c => c.Id.Value).ToArray();
        Assert.That(greenSelection, Is.EqualTo(new[] { "simple-solution", "yagni", "general-principle" }),
            "GREEN phase should prioritize simple solutions");

        // Act & Assert: Test REFACTOR phase selection
        string[] refactorSelection = _selector.SelectConstraints(constraints, refactorPhase, topK: 3)
            .Select(c => c.Id.Value).ToArray();
        Assert.That(refactorSelection, Is.EqualTo(new[] { "clean-code", "yagni", "general-principle" }),
            "REFACTOR phase should prioritize clean code");
    }

    #region Helper Methods

    private static Constraint CreateConstraint(string id, double priority, params Phase[] phases)
    {
        return new Constraint(
            new ConstraintId(id),
            $"Test constraint: {id}",
            new Priority(priority),
            phases,
            new[] { $"Reminder for {id}" }
        );
    }

    #endregion
}
