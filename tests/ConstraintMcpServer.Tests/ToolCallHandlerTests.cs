using System;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Scheduling;
using ConstraintMcpServer.Presentation.Hosting;

namespace ConstraintMcpServer.Tests;

/// <summary>
/// Unit tests for ToolCallHandler integration with scheduler.
/// Tests drive the MCP tools/call pipeline integration to satisfy E2E requirements.
/// </summary>
[TestFixture]
public sealed class ToolCallHandlerTests
{
    private const int TestCadence = 3; // Standard test cadence matching E2E expectations
    [Test]
    public async Task HandleAsync_FirstToolCall_InjectsConstraints()
    {
        // Arrange: This test drives the "first interaction always injects" requirement in MCP pipeline
        var scheduler = new Scheduler(everyNInteractions: TestCadence);
        var handler = new ToolCallHandler(scheduler);

        var toolCallRequest = JsonDocument.Parse(@"{
            ""jsonrpc"": ""2.0"",
            ""id"": 1,
            ""method"": ""tools/call"",
            ""params"": {
                ""name"": ""sample-tool"",
                ""arguments"": { ""test"": ""first-call"" }
            }
        }");

        // Act: Handle the first tool call
        object result = await handler.HandleAsync(requestId: 1, toolCallRequest.RootElement);

        // Assert: Response should contain constraint injection markers
        string responseJson = JsonSerializer.Serialize(result);
        Assert.That(responseJson, Does.Contain("CONSTRAINT"),
            "First tool call must inject constraints for E2E test to pass");
    }

    [Test]
    public async Task HandleAsync_SecondToolCall_NoConstraintInjection()
    {
        // Arrange: This test drives the "2nd interaction should not inject" requirement
        var scheduler = new Scheduler(everyNInteractions: TestCadence);
        var handler = new ToolCallHandler(scheduler);

        var toolCallRequest = JsonDocument.Parse(@"{
            ""jsonrpc"": ""2.0"",
            ""id"": 2,
            ""method"": ""tools/call"",
            ""params"": {
                ""name"": ""sample-tool"",
                ""arguments"": { ""test"": ""second-call"" }
            }
        }");

        // Act: Simulate first call (to increment counter), then handle second call
        await handler.HandleAsync(1, toolCallRequest.RootElement); // First call
        object result = await handler.HandleAsync(requestId: 2, toolCallRequest.RootElement); // Second call

        // Assert: Response should NOT contain constraint injection
        string responseJson = JsonSerializer.Serialize(result);
        Assert.That(responseJson, Does.Not.Contain("CONSTRAINT"),
            "Second tool call should not inject constraints per E2E requirement");
    }

    [Test]
    public async Task HandleAsync_ThirdToolCall_InjectsConstraints()
    {
        // Arrange: This test drives the "3rd interaction should inject" requirement
        var scheduler = new Scheduler(everyNInteractions: TestCadence);
        var handler = new ToolCallHandler(scheduler);

        var toolCallRequest = JsonDocument.Parse(@"{
            ""jsonrpc"": ""2.0"",
            ""id"": 3,
            ""method"": ""tools/call"",
            ""params"": {
                ""name"": ""sample-tool"",
                ""arguments"": { ""test"": ""third-call"" }
            }
        }");

        // Act: Simulate first two calls, then handle third call
        await handler.HandleAsync(1, toolCallRequest.RootElement); // First call
        await handler.HandleAsync(2, toolCallRequest.RootElement); // Second call
        object result = await handler.HandleAsync(requestId: 3, toolCallRequest.RootElement); // Third call

        // Assert: Response should contain constraint injection
        string responseJson = JsonSerializer.Serialize(result);
        Assert.That(responseJson, Does.Contain("CONSTRAINT"),
            "Third tool call must inject constraints per cadence requirement");
    }

    [Test]
    public async Task HandleAsync_MultipleSequences_FollowsDeterministicPattern()
    {
        // Arrange: This test drives the "deterministic behavior" requirement across multiple sequences
        var scheduler = new Scheduler(everyNInteractions: TestCadence);
        var handler = new ToolCallHandler(scheduler);

        var toolCallRequest = JsonDocument.Parse(@"{
            ""jsonrpc"": ""2.0"",
            ""id"": 1,
            ""method"": ""tools/call"",
            ""params"": {
                ""name"": ""sample-tool"",
                ""arguments"": { ""test"": ""pattern-test"" }
            }
        }");

        // Act: Simulate the exact pattern the E2E test expects (5 calls)
        object[] results = new object[5];
        for (int i = 0; i < 5; i++)
        {
            results[i] = await handler.HandleAsync(requestId: i + 1, toolCallRequest.RootElement);
        }

        // Assert: Check the exact pattern E2E test expects
        // Calls 1 and 3 should have constraints, calls 2, 4, 5 should not
        string result1 = JsonSerializer.Serialize(results[0]);
        string result2 = JsonSerializer.Serialize(results[1]);
        string result3 = JsonSerializer.Serialize(results[2]);
        string result4 = JsonSerializer.Serialize(results[3]);
        string result5 = JsonSerializer.Serialize(results[4]);

        Assert.Multiple(() =>
        {
            Assert.That(result1, Does.Contain("CONSTRAINT"), "Call 1 should inject constraints");
            Assert.That(result2, Does.Not.Contain("CONSTRAINT"), "Call 2 should not inject constraints");
            Assert.That(result3, Does.Contain("CONSTRAINT"), "Call 3 should inject constraints");
            Assert.That(result4, Does.Not.Contain("CONSTRAINT"), "Call 4 should not inject constraints");
            Assert.That(result5, Does.Not.Contain("CONSTRAINT"), "Call 5 should not inject constraints");
        });
    }

    [Test]
    public void Constructor_ValidScheduler_DoesNotThrow()
    {
        // Arrange & Act & Assert: Should accept valid scheduler
        var scheduler = new Scheduler(everyNInteractions: TestCadence);
        Assert.DoesNotThrow(() => new ToolCallHandler(scheduler));
    }

    [Test]
    public void Constructor_NullScheduler_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert: Should reject null scheduler
        Assert.Throws<ArgumentNullException>(() => new ToolCallHandler(null!));
    }
}
