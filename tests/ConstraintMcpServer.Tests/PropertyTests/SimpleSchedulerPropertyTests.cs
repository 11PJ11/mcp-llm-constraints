using ConstraintMcpServer.Application.Scheduling;
using NUnit.Framework;
using System;

namespace ConstraintMcpServer.Tests.PropertyTests;

/// <summary>
/// Simplified property-based tests for Scheduler focusing on key business invariants.
/// Uses explicit test cases to validate business rules without FsCheck complexity.
/// </summary>
[TestFixture]
[Category("Property")]
public sealed class SimpleSchedulerPropertyTests
{
    [Test]
    public void First_Interaction_Always_Establishes_Session_Patterns()
    {
        // Test multiple cadence values to ensure business rule holds
        int[] cadences = { 1, 2, 3, 5, 7, 10, 20 };

        foreach (int cadence in cadences)
        {
            var scheduler = new Scheduler(cadence);

            Assert.That(scheduler.ShouldInject(interactionCount: 1), Is.True,
                $"Session establishment requires constraint injection at first interaction (cadence={cadence})");
        }
    }

    [Test]
    public void Injection_Decisions_Are_Deterministic_And_Reproducible()
    {
        // Test deterministic behavior across multiple schedulers and interaction counts
        int[] cadences = { 2, 3, 5 };
        int[] interactions = { 1, 2, 3, 4, 5, 6, 10, 15, 20 };

        foreach (int cadence in cadences)
        {
            foreach (int interaction in interactions)
            {
                var scheduler1 = new Scheduler(cadence);
                var scheduler2 = new Scheduler(cadence);

                bool decision1 = scheduler1.ShouldInject(interaction);
                bool decision2 = scheduler2.ShouldInject(interaction);

                Assert.That(decision1, Is.EqualTo(decision2),
                    $"Deterministic behavior required: cadence={cadence}, interaction={interaction}");
            }
        }
    }

    [Test]
    public void Injection_Frequency_Respects_Configured_Cadence_Pattern()
    {
        // Test cadence pattern for common values
        var testCases = new[]
        {
            new { Cadence = 2, Expected = new[] { 1, 2, 4, 6, 8, 10 } },
            new { Cadence = 3, Expected = new[] { 1, 3, 6, 9, 12, 15 } },
            new { Cadence = 5, Expected = new[] { 1, 5, 10, 15, 20, 25 } }
        };

        foreach (var testCase in testCases)
        {
            var scheduler = new Scheduler(testCase.Cadence);
            var actualInjections = new List<int>();

            // Test up to 30 interactions
            for (int interaction = 1; interaction <= 30; interaction++)
            {
                if (scheduler.ShouldInject(interaction))
                {
                    actualInjections.Add(interaction);
                }
            }

            // Verify first few expected injection points
            for (int i = 0; i < Math.Min(testCase.Expected.Length, actualInjections.Count); i++)
            {
                Assert.That(actualInjections[i], Is.EqualTo(testCase.Expected[i]),
                    $"Cadence pattern mismatch at index {i} for cadence {testCase.Cadence}");
            }
        }
    }

    [Test]
    public void Session_Establishment_Takes_Precedence_Over_Cadence_Patterns()
    {
        // Test various cadence values to ensure first interaction always injects
        for (int cadence = 1; cadence <= 10; cadence++)
        {
            var scheduler = new Scheduler(cadence);

            Assert.That(scheduler.ShouldInject(interactionCount: 1), Is.True,
                $"Session establishment overrides cadence mathematics (cadence={cadence})");
        }
    }

    [Test]
    public void Constraint_Reinforcement_Maintains_Consistent_Frequency()
    {
        var testCases = new[]
        {
            new { Cadence = 3, Interactions = 15, ExpectedInjections = 6 }, // 1,3,6,9,12,15
            new { Cadence = 4, Interactions = 20, ExpectedInjections = 6 }, // 1,4,8,12,16,20
            new { Cadence = 5, Interactions = 25, ExpectedInjections = 6 }  // 1,5,10,15,20,25
        };

        foreach (var testCase in testCases)
        {
            var scheduler = new Scheduler(testCase.Cadence);
            int injectionCount = 0;

            for (int interaction = 1; interaction <= testCase.Interactions; interaction++)
            {
                if (scheduler.ShouldInject(interaction))
                {
                    injectionCount++;
                }
            }

            Assert.That(injectionCount, Is.EqualTo(testCase.ExpectedInjections),
                $"Consistent frequency required: cadence={testCase.Cadence}, interactions={testCase.Interactions}");
        }
    }

    [Test]
    public void Edge_Cases_Handle_Gracefully()
    {
        // Test edge cases
        var scheduler = new Scheduler(1); // Every interaction

        // Verify every interaction injects with cadence=1
        for (int i = 1; i <= 10; i++)
        {
            Assert.That(scheduler.ShouldInject(i), Is.True,
                $"Cadence=1 should inject every interaction (interaction {i})");
        }

        // Test large cadence
        var largeScheduler = new Scheduler(100);
        Assert.Multiple(() =>
        {
            Assert.That(largeScheduler.ShouldInject(1), Is.True, "First interaction must always inject");
            Assert.That(largeScheduler.ShouldInject(50), Is.False, "50 < 100, should not inject");
            Assert.That(largeScheduler.ShouldInject(100), Is.True, "100 = 100, should inject");
        });
    }

    [Test]
    public void Constructor_Validates_Positive_Cadence()
    {
        // Test invalid cadence values
        Assert.Throws<ArgumentException>(() => new Scheduler(0),
            "Zero cadence should throw ArgumentException");

        Assert.Throws<ArgumentException>(() => new Scheduler(-1),
            "Negative cadence should throw ArgumentException");

        Assert.Throws<ArgumentException>(() => new Scheduler(-100),
            "Large negative cadence should throw ArgumentException");
    }

    [Test]
    public void Business_Scenario_TDD_Workflow()
    {
        // Simulate typical TDD workflow with cadence=3
        var scheduler = new Scheduler(everyNInteractions: 3);
        int[] interactions = new[]
        {
            // TDD Cycle 1
            1, // RED: Write failing test -> INJECT (first interaction)
            2, // GREEN: Make test pass -> no inject
            3, // REFACTOR: Clean code -> INJECT (cadence hit)
            
            // TDD Cycle 2  
            4, // RED: Next failing test -> no inject
            5, // GREEN: Make test pass -> no inject
            6, // REFACTOR: Clean code -> INJECT (cadence hit)
            
            // TDD Cycle 3
            7, // RED: Next failing test -> no inject
            8, // GREEN: Make test pass -> no inject
            9  // REFACTOR: Clean code -> INJECT (cadence hit)
        };

        int[] expectedInjections = new[] { 1, 3, 6, 9 };
        var actualInjections = new List<int>();

        foreach (int interaction in interactions)
        {
            if (scheduler.ShouldInject(interaction))
            {
                actualInjections.Add(interaction);
            }
        }

        Assert.That(actualInjections.ToArray(), Is.EqualTo(expectedInjections),
            "TDD workflow should follow expected injection pattern");
    }
}
