using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Domain.Constraints;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Application;

/// <summary>
/// Unit tests for ContextAnalyzer with context extraction and development activity detection.
/// Tests business logic for converting MCP tool calls and user input into trigger contexts.
/// Uses outside-in TDD approach with business scenario focus.
/// </summary>
[TestFixture]
public class ContextAnalyzerTests
{
    private IContextAnalyzer _analyzer = null!;

    [SetUp]
    public void SetUp()
    {
        _analyzer = new ContextAnalyzer();
    }

    /// <summary>
    /// E2E Acceptance Test: MCP Tool Call Context Extraction
    /// This test validates Day 2 ContextAnalyzer implementation
    /// </summary>
    [Test]
    public void AnalyzeToolCallContext_Should_Extract_Development_Context_From_MCP_Call()
    {
        // Business Scenario: Developer calls MCP tool with method indicating test creation
        // Expected: System extracts relevant trigger context with appropriate keywords

        // Arrange - MCP tool call indicating test development activity
        var methodName = "tools/create_test_file";
        var parameters = new object[] { "src/features/UserAuthentication.test.cs", "unit_test" };
        var sessionId = "test-session-123";

        // Act - Core implementation under test
        var context = _analyzer.AnalyzeToolCallContext(methodName, parameters, sessionId);

        // Assert - Business validation
        Assert.That(context, Is.Not.Null, "context should be extracted from MCP tool call");
        Assert.That(context.Keywords, Is.Not.Empty, "keywords should be extracted from method and parameters");
        Assert.That(context.Keywords, Does.Contain("test"), "should extract 'test' keyword from context");
        Assert.That(context.FilePath, Is.EqualTo("src/features/UserAuthentication.test.cs"), "should extract file path from parameters");
        Assert.That(context.ContextType, Is.Not.Null, "should determine context type from analysis");
    }

    [Test]
    public void AnalyzeUserInput_Should_Extract_Keywords_From_Natural_Language()
    {
        // Business Scenario: User types natural language describing development intent
        // Expected: System extracts relevant keywords and context type

        // Arrange
        var userInput = "I need to implement a new feature with test-driven development approach";
        var sessionId = "test-session-456";

        // Act
        var context = _analyzer.AnalyzeUserInput(userInput, sessionId);

        // Assert
        Assert.That(context, Is.Not.Null, "context should be extracted from user input");
        Assert.That(context.Keywords, Is.Not.Empty, "keywords should be extracted from natural language");
        Assert.That(context.Keywords, Does.Contain("implement"), "should extract action keywords");
        Assert.That(context.Keywords, Does.Contain("feature"), "should extract domain keywords");
        Assert.That(context.Keywords, Does.Contain("test"), "should extract methodology keywords");
    }

    [Test]
    public void DetectContextType_Should_Classify_Development_Activity()
    {
        // Business Scenario: System classifies type of development work from context clues
        // Expected: Accurate classification based on keywords and file patterns

        // Arrange
        var keywords = new[] { "implement", "feature", "test" };
        var filePath = "src/features/NewFeature.cs";

        // Act
        var contextType = _analyzer.DetectContextType(keywords, filePath);

        // Assert
        Assert.That(contextType, Is.Not.Null, "should classify context type");
        Assert.That(contextType, Is.EqualTo("feature_development"), "should classify as feature development based on keywords and path");
    }

    [Test]
    public void DetectContextType_Should_Handle_Testing_Context()
    {
        // Business Scenario: Pure testing activity without feature development

        // Arrange
        var keywords = new[] { "test", "unit", "validate" };
        var filePath = "tests/UserServiceTests.cs";

        // Act
        var contextType = _analyzer.DetectContextType(keywords, filePath);

        // Assert
        Assert.That(contextType, Is.EqualTo("testing"), "should classify as testing activity");
    }

    [Test]
    public void DetectContextType_Should_Handle_Refactoring_Context()
    {
        // Business Scenario: Code improvement and refactoring activity

        // Arrange
        var keywords = new[] { "refactor", "clean", "improve" };
        var filePath = "src/services/UserService.cs";

        // Act
        var contextType = _analyzer.DetectContextType(keywords, filePath);

        // Assert
        Assert.That(contextType, Is.EqualTo("refactoring"), "should classify as refactoring activity");
    }

    [Test]
    public void AnalyzeToolCallContext_Should_Handle_Null_Parameters_Gracefully()
    {
        // Edge Case: Robust handling of missing parameters

        // Arrange
        var methodName = "tools/unknown_method";
        object[] parameters = null!;
        var sessionId = "test-session";

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            var context = _analyzer.AnalyzeToolCallContext(methodName, parameters, sessionId);
            Assert.That(context, Is.Not.Null, "should return valid context even with null parameters");
        });
    }

    [Test]
    public void AnalyzeUserInput_Should_Handle_Empty_Input_Gracefully()
    {
        // Edge Case: Robust handling of empty user input

        // Arrange
        var userInput = "";
        var sessionId = "test-session";

        // Act
        var context = _analyzer.AnalyzeUserInput(userInput, sessionId);

        // Assert
        Assert.That(context, Is.Not.Null, "should return valid context even with empty input");
        Assert.That(context.Keywords, Is.Not.Null, "keywords collection should not be null");
    }
}
