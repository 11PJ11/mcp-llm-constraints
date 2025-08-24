using System.Collections.Generic;
using ConstraintMcpServer.Application.Injection;
using ConstraintMcpServer.Domain;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests;

/// <summary>
/// Unit tests for constraint injection formatting.
/// Tests anchor creation and reminder formatting.
/// </summary>
[TestFixture]
public sealed class InjectionTests
{
    [Test]
    public void FormatConstraintMessage_IncludesAnchorPrologue()
    {
        // Arrange
        var constraints = new List<Constraint>
        {
            CreateTestConstraint("test", 0.9, "Test reminder")
        };
        var injector = new Injector();

        // Act
        string message = injector.FormatConstraintMessage(constraints, interactionNumber: 1);

        // Assert - should include anchor prologue
        Assert.That(message, Does.Contain("Remember:"));
        Assert.That(message, Does.Contain("Test-first"));
    }

    [Test]
    public void FormatConstraintMessage_IncludesConstraintReminders()
    {
        // Arrange
        var constraints = new List<Constraint>
        {
            CreateTestConstraint("tdd", 0.9, "Write a failing test first"),
            CreateTestConstraint("arch", 0.8, "Domain must not depend on Infrastructure")
        };
        var injector = new Injector();

        // Act
        string message = injector.FormatConstraintMessage(constraints, interactionNumber: 1);

        // Assert - should include constraint reminders
        Assert.That(message, Does.Contain("Write a failing test first"));
        Assert.That(message, Does.Contain("Domain must not depend on Infrastructure"));
    }

    [Test]
    public void FormatConstraintMessage_IncludesAnchorEpilogue()
    {
        // Arrange
        var constraints = new List<Constraint>
        {
            CreateTestConstraint("test", 0.9, "Test reminder")
        };
        var injector = new Injector();

        // Act
        string message = injector.FormatConstraintMessage(constraints, interactionNumber: 1);

        // Assert - should include anchor epilogue
        Assert.That(message, Does.Contain("Before commit:"));
        Assert.That(message, Does.Contain("tests green"));
    }

    [Test]
    public void FormatConstraintMessage_EmptyConstraintsReturnsBasicAnchors()
    {
        // Arrange
        var constraints = new List<Constraint>();
        var injector = new Injector();

        // Act
        string message = injector.FormatConstraintMessage(constraints, interactionNumber: 1);

        // Assert - should still include anchors even with no constraints
        Assert.That(message, Does.Contain("Remember:"));
        Assert.That(message, Does.Contain("Before commit:"));
    }

    [Test]
    public void FormatConstraintMessage_IncludesInteractionContext()
    {
        // Arrange
        var constraints = new List<Constraint>
        {
            CreateTestConstraint("test", 0.9, "Test reminder")
        };
        var injector = new Injector();

        // Act
        string message = injector.FormatConstraintMessage(constraints, interactionNumber: 3);

        // Assert - should reference interaction context
        Assert.That(message, Does.Contain("Tool call 3"));
    }

    private static Constraint CreateTestConstraint(string id, double priority, string reminder)
    {
        return new Constraint(
            new ConstraintId(id),
            $"Test constraint {id}",
            new Priority(priority),
            new[] { new Phase("red") },
            new[] { reminder }
        );
    }
}
