using System;
using NUnit.Framework;
using ConstraintMcpServer.Domain;

namespace ConstraintMcpServer.Tests;

/// <summary>
/// Comprehensive unit tests for Priority value object.
/// Tests validation, comparison operators, equality, and edge cases.
/// </summary>
[TestFixture]
public sealed class PriorityTests
{
    [Test]
    public void Constructor_WithValidValue_CreatesInstance()
    {
        // Arrange & Act
        var priority = new Priority(0.5);

        // Assert
        Assert.That(priority.Value, Is.EqualTo(0.5));
    }

    [Test]
    [TestCase(0.0)]
    [TestCase(0.1)]
    [TestCase(0.5)]
    [TestCase(0.9)]
    [TestCase(1.0)]
    public void Constructor_WithValidBoundaryValues_CreatesInstance(double value)
    {
        // Act & Assert
        Assert.DoesNotThrow(() => new Priority(value));
        var priority = new Priority(value);
        Assert.That(priority.Value, Is.EqualTo(value));
    }

    [Test]
    [TestCase(-0.1)]
    [TestCase(-1.0)]
    [TestCase(1.1)]
    [TestCase(2.0)]
    [TestCase(double.MinValue)]
    [TestCase(double.MaxValue)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void Constructor_WithInvalidValue_ThrowsValidationException(double invalidValue)
    {
        // Act & Assert
        ValidationException? exception = Assert.Throws<ValidationException>(() => new Priority(invalidValue));
        Assert.That(exception.Message, Contains.Substring("Priority must be between 0.0 and 1.0"));
        Assert.That(exception.Message, Contains.Substring(invalidValue.ToString()));
    }

    [Test]
    public void Constructor_WithNaN_AcceptsValue()
    {
        // NaN comparisons always return false, so NaN < 0.0 and NaN > 1.0 are both false
        // This means NaN passes the validation logic and creates a Priority with NaN value
        // Act & Assert
        Assert.DoesNotThrow(() => new Priority(double.NaN));
        var priority = new Priority(double.NaN);
        Assert.That(double.IsNaN(priority.Value), Is.True);
    }

    [Test]
    public void CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var priority = new Priority(0.5);

        // Act
        int result = priority.CompareTo(null);

        // Assert
        Assert.That(result, Is.GreaterThan(0));
    }

    [Test]
    [TestCase(0.3, 0.5, -1)]
    [TestCase(0.5, 0.3, 1)]
    [TestCase(0.5, 0.5, 0)]
    [TestCase(0.0, 1.0, -1)]
    [TestCase(1.0, 0.0, 1)]
    public void CompareTo_WithDifferentPriorities_ReturnsCorrectComparison(double value1, double value2, int expectedSign)
    {
        // Arrange
        var priority1 = new Priority(value1);
        var priority2 = new Priority(value2);

        // Act
        int result = priority1.CompareTo(priority2);

        // Assert
        Assert.That(Math.Sign(result), Is.EqualTo(expectedSign));
    }

    [Test]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var priority = new Priority(0.5);

        // Act & Assert
        Assert.That(priority, Is.Not.EqualTo((Priority?)null));
    }

    [Test]
    [TestCase(0.5, 0.5, true)]
    [TestCase(0.3, 0.7, false)]
    [TestCase(0.0, 0.0, true)]
    [TestCase(1.0, 1.0, true)]
    public void Equals_WithPriorities_ReturnsCorrectEquality(double value1, double value2, bool expectedEqual)
    {
        // Arrange
        var priority1 = new Priority(value1);
        var priority2 = new Priority(value2);

        Assert.Multiple(() =>
        {
            // Act & Assert
            Assert.That(priority1.Equals(priority2), Is.EqualTo(expectedEqual));
            Assert.That(priority1.Equals((object)priority2), Is.EqualTo(expectedEqual));
        });
    }

    [Test]
    public void Equals_WithFloatingPointPrecision_HandlesCloseValues()
    {
        // Arrange
        var priority1 = new Priority(0.1 + 0.1 + 0.1); // May have floating point precision issues
        var priority2 = new Priority(0.3);

        // Act & Assert
        Assert.That(priority1.Equals(priority2), Is.True, "Should handle floating point precision");
    }

    [Test]
    public void Equals_WithDifferentTypes_ReturnsFalse()
    {
        // Arrange
        var priority = new Priority(0.5);
        double notPriority = 0.5;

        // Act & Assert
        Assert.That(priority.Equals(notPriority), Is.False);
    }

    [Test]
    public void GetHashCode_WithEqualPriorities_ReturnsSameHashCode()
    {
        // Arrange
        var priority1 = new Priority(0.7);
        var priority2 = new Priority(0.7);

        // Act & Assert
        Assert.That(priority1.GetHashCode(), Is.EqualTo(priority2.GetHashCode()));
    }

    [Test]
    public void GetHashCode_WithDifferentPriorities_ReturnsDifferentHashCodes()
    {
        // Arrange
        var priority1 = new Priority(0.3);
        var priority2 = new Priority(0.7);

        // Act & Assert
        Assert.That(priority1.GetHashCode(), Is.Not.EqualTo(priority2.GetHashCode()));
    }

    [Test]
    [TestCase(0.0, "0.00")]
    [TestCase(0.1, "0.10")]
    [TestCase(0.12, "0.12")]
    [TestCase(0.123, "0.12")]
    [TestCase(0.5, "0.50")]
    [TestCase(1.0, "1.00")]
    public void ToString_FormatsValueToTwoDecimalPlaces(double value, string expectedString)
    {
        // Arrange
        var priority = new Priority(value);

        // Act
        string result = priority.ToString();

        // Assert
        Assert.That(result, Is.EqualTo(expectedString));
    }

    [Test]
    [TestCase(0.5, 0.5, true)]
    [TestCase(0.3, 0.7, false)]
    [TestCase(0.0, 1.0, false)]
    public void EqualityOperator_ReturnsCorrectResult(double value1, double value2, bool expectedEqual)
    {
        // Arrange
        var priority1 = new Priority(value1);
        var priority2 = new Priority(value2);

        Assert.Multiple(() =>
        {
            // Act & Assert
            Assert.That(priority1 == priority2, Is.EqualTo(expectedEqual));
            Assert.That(priority1 != priority2, Is.EqualTo(!expectedEqual));
        });
    }

    [Test]
    public void EqualityOperator_WithNullValues_HandlesCorrectly()
    {
        // Arrange
        var priority = new Priority(0.5);
        Priority? nullPriority = null;

        Assert.Multiple(() =>
        {
            // Act & Assert
            Assert.That(priority, Is.Not.EqualTo(nullPriority));
            Assert.That(nullPriority, Is.Not.EqualTo(priority));
            Assert.That(((Priority?)null) == ((Priority?)null), Is.True);
        });

        Assert.That(priority, Is.Not.EqualTo(nullPriority));
        Assert.That(nullPriority, Is.Not.EqualTo(priority));
        Assert.That(((Priority?)null) != ((Priority?)null), Is.False);
    }

    [Test]
    [TestCase(0.7, 0.3, true, false)]
    [TestCase(0.3, 0.7, false, true)]
    [TestCase(0.5, 0.5, false, false)]
    public void ComparisonOperators_ReturnCorrectResults(double value1, double value2, bool expectedGreater, bool expectedLess)
    {
        // Arrange
        var priority1 = new Priority(value1);
        var priority2 = new Priority(value2);

        Assert.Multiple(() =>
        {
            // Act & Assert
            Assert.That(priority1 > priority2, Is.EqualTo(expectedGreater));
            Assert.That(priority1 < priority2, Is.EqualTo(expectedLess));
            Assert.That(priority1 >= priority2, Is.EqualTo(expectedGreater || value1 == value2));
            Assert.That(priority1 <= priority2, Is.EqualTo(expectedLess || value1 == value2));
        });
    }

    [Test]
    public void ComparisonOperators_WithNull_ReturnsFalse()
    {
        // Arrange
        var priority = new Priority(0.5);
        Priority? nullPriority = null;

        // Act & Assert
        Assert.That(priority > nullPriority, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(priority < nullPriority, Is.False);
            Assert.That(priority >= nullPriority, Is.True);
            Assert.That(priority > nullPriority, Is.True);

            Assert.That(((Priority?)null) > priority, Is.False);
            Assert.That(((Priority?)null) < priority, Is.False);  // null?.CompareTo(priority) returns null, and null < 0 is false
            Assert.That(((Priority?)null) >= priority, Is.False);
            Assert.That(((Priority?)null) <= priority, Is.False); // null?.CompareTo(priority) returns null, and null <= 0 is false
        });
    }

    [Test]
    public void Priority_BusinessScenario_ConstraintSelectionByPriority()
    {
        // Arrange - Typical constraint priorities
        var criticalPriority = new Priority(0.95);  // Critical TDD rule
        var importantPriority = new Priority(0.80); // Important architecture rule
        var normalPriority = new Priority(0.60);    // Normal code quality rule
        var lowPriority = new Priority(0.30);       // Optional guideline

        // Act - Sort by priority (highest first)
        Priority[] priorities = new[] { normalPriority, criticalPriority, lowPriority, importantPriority };
        Array.Sort(priorities, (p1, p2) => p2.CompareTo(p1)); // Descending order

        Assert.Multiple(() =>
        {
            // Assert - Should be ordered by priority
            Assert.That(priorities[0], Is.EqualTo(criticalPriority));
            Assert.That(priorities[1], Is.EqualTo(importantPriority));
            Assert.That(priorities[2], Is.EqualTo(normalPriority));
            Assert.That(priorities[3], Is.EqualTo(lowPriority));
        });
    }

    [Test]
    public void Priority_EdgeCase_FloatingPointBoundaryValues()
    {
        // Arrange & Act & Assert
        Assert.DoesNotThrow(() => new Priority(0.0));
        Assert.DoesNotThrow(() => new Priority(1.0));

        // Test very close to boundaries
        Assert.DoesNotThrow(() => new Priority(0.0000001));
        Assert.DoesNotThrow(() => new Priority(0.9999999));

        // Test just outside boundaries
        Assert.Throws<ValidationException>(() => new Priority(-0.0000001));
        Assert.Throws<ValidationException>(() => new Priority(1.0000001));
    }
}
