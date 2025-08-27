using System;
using NUnit.Framework;
using ConstraintMcpServer.Application.Scheduling;

namespace ConstraintMcpServer.Tests;

/// <summary>
/// Unit tests for deterministic scheduler behavior.
/// Tests drive the implementation to satisfy the E2E test requirements:
/// - First interaction always injects (kickoff)
/// - Every Nth interaction thereafter injects (cadence)
/// - Deterministic behavior for same inputs
/// </summary>
[TestFixture]
public sealed class SchedulerTests
{
    private const int TestCadence = 3; // Standard test cadence for validation
    [Test]
    public void ShouldInject_FirstInteraction_ReturnsTrue()
    {
        // Arrange: This test drives the "first interaction always gets constraints" requirement
        var scheduler = new Scheduler(everyNInteractions: TestCadence);

        // Act & Assert: First interaction should always inject
        Assert.That(scheduler.ShouldInject(interactionCount: 1), Is.True,
            "First interaction must inject constraints to establish patterns");
    }

    [Test]
    public void ShouldInject_EveryNthInteraction_FollowsCadence()
    {
        // Arrange: This test drives the "every 3rd interaction thereafter" requirement
        var scheduler = new Scheduler(everyNInteractions: TestCadence);

        Assert.Multiple(() =>
        {
            // Act & Assert: Should inject on 3rd, 6th, 9th interactions (plus first)
            Assert.That(scheduler.ShouldInject(interactionCount: 1), Is.True, "First interaction");
            Assert.That(scheduler.ShouldInject(interactionCount: 3), Is.True, "3rd interaction");
            Assert.That(scheduler.ShouldInject(interactionCount: 6), Is.True, "6th interaction");
            Assert.That(scheduler.ShouldInject(interactionCount: 9), Is.True, "9th interaction");
        });
    }

    [Test]
    public void ShouldInject_BetweenCadence_ReturnsFalse()
    {
        // Arrange: This test drives the "other interactions pass through unchanged" requirement
        var scheduler = new Scheduler(everyNInteractions: TestCadence);

        Assert.Multiple(() =>
        {
            // Act & Assert: Should NOT inject between cadence points
            Assert.That(scheduler.ShouldInject(interactionCount: 2), Is.False, "2nd interaction should not inject");
            Assert.That(scheduler.ShouldInject(interactionCount: 4), Is.False, "4th interaction should not inject");
            Assert.That(scheduler.ShouldInject(interactionCount: 5), Is.False, "5th interaction should not inject");
            Assert.That(scheduler.ShouldInject(interactionCount: 7), Is.False, "7th interaction should not inject");
            Assert.That(scheduler.ShouldInject(interactionCount: 8), Is.False, "8th interaction should not inject");
        });
    }

    [Test]
    public void ShouldInject_DeterministicBehavior_SameInputsSameOutputs()
    {
        // Arrange: This test drives the "behavior is deterministic" requirement
        var scheduler1 = new Scheduler(everyNInteractions: 3);
        var scheduler2 = new Scheduler(everyNInteractions: 3);

        // Act & Assert: Same inputs should produce same outputs
        for (int i = 1; i <= 10; i++)
        {
            bool result1 = scheduler1.ShouldInject(interactionCount: i);
            bool result2 = scheduler2.ShouldInject(interactionCount: i);

            Assert.That(result1, Is.EqualTo(result2),
                $"Deterministic failure at interaction {i}: scheduler1={result1}, scheduler2={result2}");
        }
    }

    [Test]
    public void Constructor_ValidCadence_DoesNotThrow()
    {
        // Arrange & Act & Assert: Should accept valid cadence values
        Assert.DoesNotThrow(() => new Scheduler(everyNInteractions: 1));
        Assert.DoesNotThrow(() => new Scheduler(everyNInteractions: 3));
        Assert.DoesNotThrow(() => new Scheduler(everyNInteractions: 10));
    }

    [Test]
    public void Constructor_InvalidCadence_ThrowsArgumentException()
    {
        // Arrange & Act & Assert: Should reject invalid cadence values
        Assert.Throws<ArgumentException>(() => new Scheduler(everyNInteractions: 0));
        Assert.Throws<ArgumentException>(() => new Scheduler(everyNInteractions: -1));
    }

    [Test]
    public void ShouldInject_BoundaryCondition_N1_AlwaysInjects()
    {
        // Arrange: Edge case - inject every interaction
        var scheduler = new Scheduler(everyNInteractions: 1);

        Assert.Multiple(() =>
        {
            // Act & Assert: Should inject on every interaction
            Assert.That(scheduler.ShouldInject(interactionCount: 1), Is.True);
            Assert.That(scheduler.ShouldInject(interactionCount: 2), Is.True);
            Assert.That(scheduler.ShouldInject(interactionCount: 100), Is.True);
        });
    }

    [Test]
    public void ShouldInject_ExactE2EPattern_MatchesExpectation()
    {
        // Arrange: This test exactly matches what the E2E test expects
        var scheduler = new Scheduler(everyNInteractions: TestCadence);

        Assert.Multiple(() =>
        {
            // Act & Assert: Simulate the exact 5 interactions from E2E test
            Assert.That(scheduler.ShouldInject(interactionCount: 1), Is.True, "E2E expects injection on 1st");
            Assert.That(scheduler.ShouldInject(interactionCount: 2), Is.False, "E2E expects NO injection on 2nd");
            Assert.That(scheduler.ShouldInject(interactionCount: 3), Is.True, "E2E expects injection on 3rd");
            Assert.That(scheduler.ShouldInject(interactionCount: 4), Is.False, "E2E expects NO injection on 4th");
            Assert.That(scheduler.ShouldInject(interactionCount: 5), Is.False, "E2E expects NO injection on 5th");
        });
    }
}
