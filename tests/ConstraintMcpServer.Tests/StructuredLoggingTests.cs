using System.Collections.Generic;
using System.Text.Json;
using ConstraintMcpServer.Infrastructure.Logging;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests;

/// <summary>
/// Unit tests for structured logging functionality.
/// Tests NDJSON format compliance and required event fields.
/// </summary>
[TestFixture]
public sealed class StructuredLoggingTests
{
    [Test]
    public void ConstraintInjectionEvent_SerializesToValidNDJSON()
    {
        // Arrange
        var constraintEvent = new ConstraintInjectionEvent
        {
            EventType = "inject",
            InteractionNumber = 1,
            Phase = "red",
            SelectedConstraintIds = new[] { "tdd.test-first", "arch.hex.domain-pure" },
            Reason = "scheduled_injection",
            Timestamp = System.DateTimeOffset.UtcNow
        };

        // Act
        string json = JsonSerializer.Serialize(constraintEvent);
        ConstraintInjectionEvent? deserialized = JsonSerializer.Deserialize<ConstraintInjectionEvent>(json);

        // Assert - should be valid JSON with all required fields
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.EventType, Is.EqualTo("inject"));
        Assert.That(deserialized.InteractionNumber, Is.EqualTo(1));
        Assert.That(deserialized.Phase, Is.EqualTo("red"));
        Assert.That(deserialized.SelectedConstraintIds, Has.Length.EqualTo(2));
        Assert.That(deserialized.Reason, Is.EqualTo("scheduled_injection"));
    }

    [Test]
    public void PassThroughEvent_SerializesToValidNDJSON()
    {
        // Arrange
        var passThroughEvent = new PassThroughEvent
        {
            EventType = "pass",
            InteractionNumber = 2,
            Reason = "not_scheduled",
            Timestamp = System.DateTimeOffset.UtcNow
        };

        // Act
        string json = JsonSerializer.Serialize(passThroughEvent);
        PassThroughEvent? deserialized = JsonSerializer.Deserialize<PassThroughEvent>(json);

        // Assert - should be valid JSON with required fields
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.EventType, Is.EqualTo("pass"));
        Assert.That(deserialized.InteractionNumber, Is.EqualTo(2));
        Assert.That(deserialized.Reason, Is.EqualTo("not_scheduled"));
    }

    [Test]
    public void ErrorEvent_SerializesToValidNDJSON()
    {
        // Arrange
        var errorEvent = new ErrorEvent
        {
            EventType = "error",
            InteractionNumber = 3,
            ErrorMessage = "Constraint selection failed",
            Timestamp = System.DateTimeOffset.UtcNow
        };

        // Act
        string json = JsonSerializer.Serialize(errorEvent);
        ErrorEvent? deserialized = JsonSerializer.Deserialize<ErrorEvent>(json);

        // Assert - should be valid JSON with required fields
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.EventType, Is.EqualTo("error"));
        Assert.That(deserialized.InteractionNumber, Is.EqualTo(3));
        Assert.That(deserialized.ErrorMessage, Is.EqualTo("Constraint selection failed"));
    }

    [Test]
    public void StructuredLogger_EmitsValidNDJSONEvents()
    {
        // Arrange
        var logger = new StructuredEventLogger();
        string[] constraintIds = new[] { "tdd.test-first" };

        // Act & Assert - should not throw exceptions
        Assert.DoesNotThrow(() => logger.LogConstraintInjection(1, "red", constraintIds, "scheduled"));
        Assert.DoesNotThrow(() => logger.LogPassThrough(2, "not_scheduled"));
        Assert.DoesNotThrow(() => logger.LogError(3, "Test error"));
    }

    [Test]
    public void EventTimestamp_IsWithinReasonableTimeRange()
    {
        // Arrange
        DateTimeOffset beforeCreation = System.DateTimeOffset.UtcNow;

        var constraintEvent = new ConstraintInjectionEvent
        {
            EventType = "inject",
            InteractionNumber = 1,
            Phase = "red",
            SelectedConstraintIds = new[] { "test" },
            Reason = "test",
            Timestamp = System.DateTimeOffset.UtcNow
        };

        DateTimeOffset afterCreation = System.DateTimeOffset.UtcNow;

        // Assert - timestamp should be within reasonable range
        Assert.That(constraintEvent.Timestamp, Is.GreaterThanOrEqualTo(beforeCreation));
        Assert.That(constraintEvent.Timestamp, Is.LessThanOrEqualTo(afterCreation));
    }
}
