using System;
using System.Linq;
using NUnit.Framework;
using ConstraintMcpServer.Application.Selection;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Tests.Application.Selection;

/// <summary>
/// Unit tests for ContextAnalyzer driven by E2E test requirements.
/// These tests define the behavior needed to make the failing E2E test pass.
/// </summary>
[TestFixture]
public sealed class ContextAnalyzerTests
{
    private ContextAnalyzer _contextAnalyzer = null!;

    [SetUp]
    public void SetUp()
    {
        _contextAnalyzer = new ContextAnalyzer();
    }

    [Test]
    public void AnalyzeToolCallContext_WithTddContext_DetectsTddDevelopment()
    {
        // Arrange - This matches the context from the failing E2E test
        var methodName = "tools/call";
        var parameters = new object[]
        {
            "/src/auth/UserAuthTests.cs",
            "writing unit tests for user authentication feature"
        };
        var sessionId = "test-session-tdd";

        // Act
        var context = _contextAnalyzer.AnalyzeToolCallContext(methodName, parameters, sessionId);

        // Assert - This context should enable TDD constraint activation
        Assert.That(context.Keywords, Contains.Item("unit"));
        Assert.That(context.Keywords, Contains.Item("tests"));
        Assert.That(context.Keywords, Contains.Item("writing"));
        Assert.That(context.ContextType, Is.EqualTo("testing"));
        Assert.That(context.FilePath, Is.EqualTo("/src/auth/UserAuthTests.cs"));
    }

    [Test]
    public void AnalyzeToolCallContext_WithRefactoringContext_DetectsRefactoring()
    {
        // Arrange - This matches the refactoring E2E test context
        var methodName = "tools/call";
        var parameters = new object[]
        {
            "/src/legacy/OldModule.cs",
            "refactoring legacy code to improve maintainability"
        };
        var sessionId = "test-session-refactor";

        // Act
        var context = _contextAnalyzer.AnalyzeToolCallContext(methodName, parameters, sessionId);

        // Assert
        Assert.That(context.Keywords, Contains.Item("refactoring"));
        Assert.That(context.Keywords, Contains.Item("legacy"));
        Assert.That(context.ContextType, Is.EqualTo("refactoring"));
        Assert.That(context.FilePath, Is.EqualTo("/src/legacy/OldModule.cs"));
    }

    [Test]
    public void AnalyzeToolCallContext_WithUnclearContext_DetectsUnknown()
    {
        // Arrange - This matches the unclear context E2E test
        var methodName = "tools/call";
        var parameters = new object[]
        {
            "/src/utils/Helper.cs",
            "working on general tasks"
        };
        var sessionId = "test-session-unclear";

        // Act
        var context = _contextAnalyzer.AnalyzeToolCallContext(methodName, parameters, sessionId);

        // Assert
        Assert.That(context.Keywords, Contains.Item("working"));
        Assert.That(context.Keywords, Contains.Item("general"));
        Assert.That(context.Keywords, Contains.Item("tasks"));
        Assert.That(context.ContextType, Is.EqualTo("unknown")); // No clear development pattern
    }

    [Test]
    public void DetectContextType_WithTestKeywords_ReturnsTestingContext()
    {
        // Arrange
        var keywords = new[] { "unit", "tests", "validate" };
        var filePath = "/src/tests/UserTests.cs";

        // Act
        var contextType = _contextAnalyzer.DetectContextType(keywords, filePath);

        // Assert
        Assert.That(contextType, Is.EqualTo("testing"));
    }

    [Test]
    public void DetectContextType_WithRefactorKeywords_ReturnsRefactoringContext()
    {
        // Arrange
        var keywords = new[] { "refactor", "clean" };
        var filePath = "/src/legacy/OldCode.cs";

        // Act  
        var contextType = _contextAnalyzer.DetectContextType(keywords, filePath);

        // Assert
        Assert.That(contextType, Is.EqualTo("refactoring"));
    }
}
