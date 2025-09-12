using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Selection;

namespace ConstraintMcpServer.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for McpProtocolValidator using proper Outside-In TDD.
/// These tests will fail initially and drive the implementation of real business logic.
/// </summary>
[TestFixture]
public class McpProtocolValidatorTests
{
    [Test]
    public async Task ValidateProtocolCompliance_WithValidTestSuite_ShouldReturnTrue()
    {
        // Arrange - This will fail because McpProtocolValidator doesn't exist yet
        // This is the RIGHT REASON for the test to fail - missing implementation

        var mcpValidator = new McpProtocolValidator();
        var protocolTestSuite = new McpProtocolTestSuite
        {
            TestInitialization = true,
            TestCapabilityNegotiation = true,
            TestToolInvocation = true,
            TestErrorHandling = true,
            TestResourceManagement = true
        };

        // Act
        var result = await mcpValidator.ValidateProtocolComplianceAsync(protocolTestSuite);

        // Assert
        Assert.That(result.IsCompliant, Is.True, "Should validate MCP protocol compliance");
        Assert.That(result.PassedTests, Is.EqualTo(5), "Should pass all 5 protocol tests");
        Assert.That(result.TestResults, Has.Count.EqualTo(5), "Should have 5 test results");
    }

    [Test]
    public async Task ValidateProtocolCompliance_WithFailingTests_ShouldReturnFalse()
    {
        // Arrange
        var mcpValidator = new McpProtocolValidator();
        var protocolTestSuite = new McpProtocolTestSuite
        {
            TestInitialization = true,
            TestCapabilityNegotiation = false, // This test will fail
            TestToolInvocation = true,
            TestErrorHandling = false, // This test will fail
            TestResourceManagement = true
        };

        // Act
        var result = await mcpValidator.ValidateProtocolComplianceAsync(protocolTestSuite);

        // Assert
        Assert.That(result.IsCompliant, Is.False, "Should reject incomplete MCP protocol compliance");
        Assert.That(result.PassedTests, Is.EqualTo(3), "Should pass 3 out of 5 tests");
        Assert.That(result.FailedTests, Has.Count.EqualTo(2), "Should have 2 failed tests");
    }

    [Test]
    public async Task CheckMcpCompatibility_WithValidVersion_ShouldReturnTrue()
    {
        // Arrange
        var mcpValidator = new McpProtocolValidator();

        // Act
        var isCompatible = await mcpValidator.CheckMcpCompatibilityAsync("v0.2.0");

        // Assert
        Assert.That(isCompatible, Is.True, "Should support MCP compatibility for v0.2.0");
    }
}
