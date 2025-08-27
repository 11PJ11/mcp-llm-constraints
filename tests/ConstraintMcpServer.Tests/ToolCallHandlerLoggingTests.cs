using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Scheduling;
using ConstraintMcpServer.Infrastructure.Logging;
using ConstraintMcpServer.Presentation.Hosting;

namespace ConstraintMcpServer.Tests;

/// <summary>
/// Business-focused tests for structured logging integration in ToolCallHandler.
/// Validates NDJSON event emission during constraint injection and pass-through operations.
/// </summary>
[TestFixture]
public sealed class ToolCallHandlerLoggingTests
{
    private ToolCallHandler? _handler;
    private TestableStructuredEventLogger? _logger;

    [SetUp]
    public void SetUp()
    {
        // Arrange: Create handler with every-interaction scheduler for predictable logging
        var scheduler = new Scheduler(everyNInteractions: 1); // Inject every interaction
        _logger = new TestableStructuredEventLogger();
        _handler = new ToolCallHandler(scheduler, _logger);
    }

    /// <summary>
    /// Business scenario: When constraint injection occurs, structured events are logged for analysis
    /// </summary>
    [Test]
    public async Task HandleAsync_ConstraintInjection_EmitsConstraintInjectionEvent()
    {
        // Arrange: Create MCP request
        JsonElement request = JsonDocument.Parse("""{"method": "tools/call", "params": {}}""").RootElement;

        // Act: Handle request that should trigger constraint injection
        await _handler!.HandleAsync(requestId: 1, request);

        // Assert: Verify constraint injection event was logged
        Assert.That(_logger!.ConstraintInjectionEvents, Has.Count.EqualTo(1));
        ConstraintInjectionEvent loggedEvent = _logger.ConstraintInjectionEvents[0];

        Assert.Multiple(() =>
        {
            Assert.That(loggedEvent.EventType, Is.EqualTo("inject"));
            Assert.That(loggedEvent.InteractionNumber, Is.EqualTo(1));
            Assert.That(loggedEvent.Phase, Is.EqualTo("red"));
            Assert.That(loggedEvent.SelectedConstraintIds, Is.Not.Empty);
            Assert.That(loggedEvent.Reason, Is.EqualTo("scheduled"));
            Assert.That(loggedEvent.Timestamp, Is.EqualTo(DateTimeOffset.UtcNow).Within(TimeSpan.FromSeconds(1)));
        });
    }

    /// <summary>
    /// Business scenario: When no constraint injection occurs, pass-through events are logged
    /// </summary>
    [Test]
    public async Task HandleAsync_PassThrough_EmitsPassThroughEvent()
    {
        // Arrange: Create handler with low-frequency scheduler and MCP request
        var lowFrequencyScheduler = new Scheduler(everyNInteractions: 3); // Inject on 1st, then every 3rd
        var handlerWithPassThrough = new ToolCallHandler(lowFrequencyScheduler, _logger!);
        JsonElement request = JsonDocument.Parse("""{"method": "tools/call", "params": {}}""").RootElement;

        // Act: Handle first interaction (should inject), then second (should pass through)
        await handlerWithPassThrough.HandleAsync(requestId: 1, request); // Should inject
        await handlerWithPassThrough.HandleAsync(requestId: 2, request); // Should pass through

        // Assert: Verify pass-through event was logged for second interaction
        Assert.That(_logger!.PassThroughEvents, Has.Count.EqualTo(1));
        PassThroughEvent loggedEvent = _logger.PassThroughEvents[0];

        Assert.Multiple(() =>
        {
            Assert.That(loggedEvent.EventType, Is.EqualTo("pass"));
            Assert.That(loggedEvent.InteractionNumber, Is.EqualTo(2));
            Assert.That(loggedEvent.Reason, Is.EqualTo("not_scheduled"));
            Assert.That(loggedEvent.Timestamp, Is.EqualTo(DateTimeOffset.UtcNow).Within(TimeSpan.FromSeconds(1)));
        });
    }

    /// <summary>
    /// Business scenario: Multiple interactions generate sequential structured events
    /// </summary>
    [Test]
    public async Task HandleAsync_MultipleInteractions_GeneratesSequentialEvents()
    {
        // Arrange: Create MCP request
        JsonElement request = JsonDocument.Parse("""{"method": "tools/call", "params": {}}""").RootElement;

        // Act: Handle multiple interactions
        await _handler!.HandleAsync(requestId: 1, request); // Should inject (interaction 1)
        await _handler.HandleAsync(requestId: 2, request);  // Should inject (interaction 2)

        Assert.Multiple(() =>
        {
            // Assert: Verify sequential interaction numbers in events
            Assert.That(_logger!.ConstraintInjectionEvents, Has.Count.EqualTo(2));
            Assert.That(_logger.ConstraintInjectionEvents[0].InteractionNumber, Is.EqualTo(1));
            Assert.That(_logger.ConstraintInjectionEvents[1].InteractionNumber, Is.EqualTo(2));
        });
    }

    /// <summary>
    /// Business scenario: Error handling during constraint operations logs error events
    /// </summary>
    [Test]
    public async Task HandleAsync_ConstraintSelectionError_EmitsErrorEvent()
    {
        // Arrange: Create handler with error-prone logger and MCP request
        var errorLogger = new TestableStructuredEventLogger(simulateError: true);
        var handlerWithErrorLogger = new ToolCallHandler(new Scheduler(everyNInteractions: 1), errorLogger);
        JsonElement request = JsonDocument.Parse("""{"method": "tools/call", "params": {}}""").RootElement;

        // Act: Handle request that should trigger error logging
        await handlerWithErrorLogger.HandleAsync(requestId: 1, request);

        // Assert: Verify error event was logged
        Assert.That(errorLogger.ErrorEvents, Has.Count.EqualTo(1));
        ErrorEvent errorEvent = errorLogger.ErrorEvents[0];
        Assert.Multiple(() =>
        {
            Assert.That(errorEvent.EventType, Is.EqualTo("error"));
            Assert.That(errorEvent.InteractionNumber, Is.EqualTo(1));
            Assert.That(errorEvent.ErrorMessage, Contains.Substring("constraint injection"));
        });
    }
}

/// <summary>
/// Testable implementation of StructuredEventLogger for testing purposes.
/// Captures events in memory instead of writing to logs.
/// </summary>
internal sealed class TestableStructuredEventLogger : IStructuredEventLogger
{
    private readonly bool _simulateError;

    public List<ConstraintInjectionEvent> ConstraintInjectionEvents { get; } = new();
    public List<PassThroughEvent> PassThroughEvents { get; } = new();
    public List<ErrorEvent> ErrorEvents { get; } = new();

    public TestableStructuredEventLogger(bool simulateError = false)
    {
        _simulateError = simulateError;
    }

    public void LogConstraintInjection(int interactionNumber, string phase, string[] constraintIds, string reason)
    {
        if (_simulateError)
        {
            LogError(interactionNumber, "Simulated error during constraint injection");
            return;
        }

        ConstraintInjectionEvents.Add(new ConstraintInjectionEvent
        {
            EventType = "inject",
            InteractionNumber = interactionNumber,
            Phase = phase,
            SelectedConstraintIds = constraintIds,
            Reason = reason,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    public void LogPassThrough(int interactionNumber, string reason)
    {
        PassThroughEvents.Add(new PassThroughEvent
        {
            EventType = "pass",
            InteractionNumber = interactionNumber,
            Reason = reason,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    public void LogError(int interactionNumber, string errorMessage)
    {
        ErrorEvents.Add(new ErrorEvent
        {
            EventType = "error",
            InteractionNumber = interactionNumber,
            ErrorMessage = errorMessage,
            Timestamp = DateTimeOffset.UtcNow
        });
    }
}

