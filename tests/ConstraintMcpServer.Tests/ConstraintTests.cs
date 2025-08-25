using System;
using System.Linq;
using NUnit.Framework;
using ConstraintMcpServer.Domain;

namespace ConstraintMcpServer.Tests;

/// <summary>
/// Comprehensive unit tests for the Constraint domain entity.
/// Tests validation, business logic, and edge cases with focus on mutation testing coverage.
/// </summary>
[TestFixture]
public sealed class ConstraintTests
{
    [Test]
    public void Constructor_ValidConstraint_CreatesInstance()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        string title = "Test Constraint";
        var priority = new Priority(0.8);
        Phase[] phases = new[] { new Phase("red"), new Phase("green") };
        string[] reminders = new[] { "First reminder", "Second reminder" };

        // Act
        var constraint = new Constraint(id, title, priority, phases, reminders);

        // Assert
        Assert.That(constraint.Id, Is.EqualTo(id));
        Assert.That(constraint.Title, Is.EqualTo(title));
        Assert.That(constraint.Priority, Is.EqualTo(priority));
        Assert.That(constraint.Phases.Count, Is.EqualTo(2));
        Assert.That(constraint.Reminders.Count, Is.EqualTo(2));
    }

    [Test]
    public void Constructor_NullId_ThrowsArgumentNullException()
    {
        // Arrange
        string title = "Test Constraint";
        var priority = new Priority(0.8);
        Phase[] phases = new[] { new Phase("red") };
        string[] reminders = new[] { "Test reminder" };

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => 
            new Constraint((ConstraintId)null!, title, priority, phases, reminders));
        Assert.That(exception.ParamName, Is.EqualTo("id"));
    }

    [Test]
    public void Constructor_NullTitle_ThrowsArgumentNullException()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        var priority = new Priority(0.8);
        Phase[] phases = new[] { new Phase("red") };
        string[] reminders = new[] { "Test reminder" };

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => 
            new Constraint(id, (string)null!, priority, phases, reminders));
        Assert.That(exception.ParamName, Is.EqualTo("title"));
    }

    [Test]
    public void Constructor_NullPriority_ThrowsArgumentNullException()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        string title = "Test Constraint";
        Phase[] phases = new[] { new Phase("red") };
        string[] reminders = new[] { "Test reminder" };

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => 
            new Constraint(id, title, (Priority)null!, phases, reminders));
        Assert.That(exception.ParamName, Is.EqualTo("priority"));
    }

    [Test]
    public void Constructor_EmptyTitle_ThrowsValidationException()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        string title = "";
        var priority = new Priority(0.8);
        Phase[] phases = new[] { new Phase("red") };
        string[] reminders = new[] { "Test reminder" };

        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => 
            new Constraint(id, title, priority, phases, reminders));
        Assert.That(exception.Message, Contains.Substring("Constraint title cannot be empty or whitespace"));
    }

    [Test]
    public void Constructor_WhitespaceTitle_ThrowsValidationException()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        string title = "   ";
        var priority = new Priority(0.8);
        Phase[] phases = new[] { new Phase("red") };
        string[] reminders = new[] { "Test reminder" };

        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => 
            new Constraint(id, title, priority, phases, reminders));
        Assert.That(exception.Message, Contains.Substring("Constraint title cannot be empty or whitespace"));
    }

    [Test]
    public void Constructor_NullPhases_ThrowsArgumentNullException()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        string title = "Test Constraint";
        var priority = new Priority(0.8);
        string[] reminders = new[] { "Test reminder" };

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => 
            new Constraint(id, title, priority, (Phase[])null!, reminders));
        Assert.That(exception.ParamName, Is.EqualTo("phases"));
    }

    [Test]
    public void Constructor_EmptyPhases_ThrowsValidationException()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        string title = "Test Constraint";
        var priority = new Priority(0.8);
        Phase[] phases = Array.Empty<Phase>();
        string[] reminders = new[] { "Test reminder" };

        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => 
            new Constraint(id, title, priority, phases, reminders));
        Assert.That(exception.Message, Contains.Substring("Constraint must have at least one phase"));
    }

    [Test]
    public void Constructor_NullReminders_ThrowsArgumentNullException()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        string title = "Test Constraint";
        var priority = new Priority(0.8);
        Phase[] phases = new[] { new Phase("red") };

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => 
            new Constraint(id, title, priority, phases, (string[])null!));
        Assert.That(exception.ParamName, Is.EqualTo("reminders"));
    }

    [Test]
    public void Constructor_EmptyReminders_ThrowsValidationException()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        string title = "Test Constraint";
        var priority = new Priority(0.8);
        Phase[] phases = new[] { new Phase("red") };
        string[] reminders = Array.Empty<string>();

        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => 
            new Constraint(id, title, priority, phases, reminders));
        Assert.That(exception.Message, Contains.Substring("Constraint must have at least one reminder"));
    }

    [Test]
    public void Constructor_RemindersWithNullElement_ThrowsValidationException()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        string title = "Test Constraint";
        var priority = new Priority(0.8);
        Phase[] phases = new[] { new Phase("red") };
        string[] reminders = new[] { "Valid reminder", null!, "Another valid reminder" };

        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => 
            new Constraint(id, title, priority, phases, reminders));
        Assert.That(exception.Message, Contains.Substring("All reminders must be non-empty and not whitespace"));
    }

    [Test]
    public void Constructor_RemindersWithEmptyElement_ThrowsValidationException()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        string title = "Test Constraint";
        var priority = new Priority(0.8);
        Phase[] phases = new[] { new Phase("red") };
        string[] reminders = new[] { "Valid reminder", "", "Another valid reminder" };

        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => 
            new Constraint(id, title, priority, phases, reminders));
        Assert.That(exception.Message, Contains.Substring("All reminders must be non-empty and not whitespace"));
    }

    [Test]
    public void Constructor_RemindersWithWhitespaceElement_ThrowsValidationException()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        string title = "Test Constraint";
        var priority = new Priority(0.8);
        Phase[] phases = new[] { new Phase("red") };
        // This is the key test that will KILL the survived mutant!
        // Any() -> true (there is at least one whitespace) -> throws exception
        // All() -> false (not all are whitespace) -> does NOT throw exception
        string[] reminders = new[] { "Valid reminder", "   ", "Another valid reminder" };

        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => 
            new Constraint(id, title, priority, phases, reminders));
        Assert.That(exception.Message, Contains.Substring("All reminders must be non-empty and not whitespace"));
    }

    [Test]
    public void AppliesTo_PhaseInList_ReturnsTrue()
    {
        // Arrange
        Constraint constraint = ConstraintFactory.CreateTddConstraint();
        var redPhase = new Phase("red");

        // Act
        bool result = constraint.AppliesTo(redPhase);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void AppliesTo_PhaseNotInList_ReturnsFalse()
    {
        // Arrange
        Constraint constraint = ConstraintFactory.CreateTddConstraint();
        var greenPhase = new Phase("green");

        // Act
        bool result = constraint.AppliesTo(greenPhase);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void AppliesTo_NullPhase_ReturnsFalse()
    {
        // Arrange
        Constraint constraint = ConstraintFactory.CreateTddConstraint();

        // Act
        bool result = constraint.AppliesTo(null!);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        Constraint constraint = ConstraintFactory.CreateTddConstraint();

        // Act
        string result = constraint.ToString();

        // Assert
        Assert.That(result, Is.EqualTo("tdd.test-first (Priority: 0.92)"));
    }

    [Test]
    public void Equals_SameId_ReturnsTrue()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        var constraint1 = new Constraint(id, "Title1", new Priority(0.5), new[] { new Phase("red") }, new[] { "Reminder1" });
        var constraint2 = new Constraint(id, "Title2", new Priority(0.7), new[] { new Phase("green") }, new[] { "Reminder2" });

        // Act & Assert
        Assert.That(constraint1.Equals(constraint2), Is.True);
        Assert.That(constraint1.Equals((object)constraint2), Is.True);
    }

    [Test]
    public void Equals_DifferentId_ReturnsFalse()
    {
        // Arrange
        var constraint1 = new Constraint(new ConstraintId("test.constraint1"), "Title", new Priority(0.5), new[] { new Phase("red") }, new[] { "Reminder" });
        var constraint2 = new Constraint(new ConstraintId("test.constraint2"), "Title", new Priority(0.5), new[] { new Phase("red") }, new[] { "Reminder" });

        // Act & Assert
        Assert.That(constraint1.Equals(constraint2), Is.False);
        Assert.That(constraint1.Equals((object)constraint2), Is.False);
    }

    [Test]
    public void Equals_Null_ReturnsFalse()
    {
        // Arrange
        Constraint constraint = ConstraintFactory.CreateTddConstraint();
        Assert.That(constraint, Is.Not.Null);

        // Act & Assert
        Assert.That(constraint.Equals(null), Is.False);
        Assert.That(constraint.Equals((object)"not a constraint"), Is.False);
    }

    [Test]
    public void Equals_DifferentType_ReturnsFalse()
    {
        // Arrange
        Constraint constraint = ConstraintFactory.CreateTddConstraint();
        string notConstraint = "not a constraint";

        // Act & Assert
        Assert.That(constraint.Equals(notConstraint), Is.False);
    }

    [Test]
    public void GetHashCode_SameId_ReturnsSameHashCode()
    {
        // Arrange
        var id = new ConstraintId("test.constraint");
        var constraint1 = new Constraint(id, "Title1", new Priority(0.5), new[] { new Phase("red") }, new[] { "Reminder1" });
        var constraint2 = new Constraint(id, "Title2", new Priority(0.7), new[] { new Phase("green") }, new[] { "Reminder2" });

        // Act & Assert
        Assert.That(constraint1.GetHashCode(), Is.EqualTo(constraint2.GetHashCode()));
    }

    [Test]
    public void GetHashCode_DifferentId_ReturnsDifferentHashCode()
    {
        // Arrange
        var constraint1 = new Constraint(new ConstraintId("test.constraint1"), "Title", new Priority(0.5), new[] { new Phase("red") }, new[] { "Reminder" });
        var constraint2 = new Constraint(new ConstraintId("test.constraint2"), "Title", new Priority(0.5), new[] { new Phase("red") }, new[] { "Reminder" });

        // Act & Assert
        Assert.That(constraint1.GetHashCode(), Is.Not.EqualTo(constraint2.GetHashCode()));
    }
}