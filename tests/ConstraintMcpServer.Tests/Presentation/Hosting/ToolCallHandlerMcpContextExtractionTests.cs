using System.Text.Json;
using System.Threading.Tasks;
using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Infrastructure.Logging;
using ConstraintMcpServer.Presentation.Hosting;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Presentation.Hosting;

/// <summary>
/// Unit tests for ToolCallHandler MCP context extraction functionality.
/// Tests the enhancement that extracts context from MCP tool call parameters for intelligent constraint activation.
/// </summary>
[TestFixture]
public class ToolCallHandlerMcpContextExtractionTests
{
    [Test]
    public async Task HandleAsync_WithContextAnalyzer_ShouldExtractContextFromMcpParameters()
    {
        // Arrange - Business requirement: ToolCallHandler should extract development context from MCP tool calls
        var contextAnalyzer = new ContextAnalyzer();
        var triggerEngine = new TriggerMatchingEngine();
        var logger = new StructuredEventLogger();
        var handler = new ToolCallHandler(contextAnalyzer, triggerEngine, logger);

        // MCP tool call with TDD-related parameters indicating test creation
        var mcpRequest = JsonDocument.Parse("""
            {
                "method": "tools/call",
                "params": {
                    "name": "create_test_file",
                    "arguments": {
                        "file_path": "src/features/UserAuthentication.test.cs",
                        "test_type": "unit_test"
                    }
                }
            }
            """).RootElement;

        // Act - Core implementation under test
        var response = await handler.HandleAsync(1, mcpRequest);

        // Assert - Business validation: Response should include context analysis with activation decision
        Assert.That(response, Is.Not.Null, "Handler should return a response");

        var responseJson = JsonSerializer.Serialize(response);
        var responseDoc = JsonDocument.Parse(responseJson);
        var result = responseDoc.RootElement.GetProperty("result");

        // Expect context_analysis object with has_activation boolean (as required by E2E tests)
        Assert.That(result.TryGetProperty("context_analysis", out var contextAnalysis), Is.True,
            "Response should include context_analysis for intelligent constraint activation");
        Assert.That(contextAnalysis.TryGetProperty("has_activation", out var hasActivation), Is.True,
            "Context analysis should indicate whether constraints were activated");
        Assert.That(hasActivation.ValueKind, Is.EqualTo(JsonValueKind.True).Or.EqualTo(JsonValueKind.False),
            "has_activation should be a boolean value");
    }
}
