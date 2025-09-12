using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Visualization;
using ConstraintMcpServer.Domain.Visualization;

namespace ConstraintMcpServer.Tests.Application.Visualization;

/// <summary>
/// Unit tests for ConsoleFormatter application service.
/// Drives implementation through TDD Red-Green-Refactor cycles.
/// Tests focus on Claude Code console compatibility and formatting.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("Application")]
[Category("Console")]
public sealed class ConsoleFormatterTests
{
    private ConsoleFormatter _formatter = null!;

    [SetUp]
    public void Setup()
    {
        _formatter = new ConsoleFormatter();
    }

    [TearDown]
    public void TearDown()
    {
        // No cleanup needed for stateless formatter
    }

    /// <summary>
    /// RED: Test should fail initially - basic text should be formatted for Claude Code console
    /// </summary>
    [Test]
    public void FormatForClaudeCode_WhenGivenBasicText_ShouldFormatForConsoleDisplay()
    {
        // Given
        var inputText = "Basic constraint tree content";

        // When
        var result = _formatter.FormatForClaudeCode(inputText);

        // Then
        Assert.That(result, Is.Not.Null, "Should return formatted result");
        Assert.That(result, Does.Contain(inputText), "Should preserve original content");
        Assert.That(result.Length, Is.LessThanOrEqualTo(ConsoleFormatterConstants.MAX_CONSOLE_WIDTH * 50),
            "Should fit reasonable console dimensions");
    }

    /// <summary>
    /// RED: Test should fail initially - long lines should be wrapped properly
    /// </summary>
    [Test]
    public void FormatForClaudeCode_WhenGivenLongLines_ShouldWrapAtConsoleWidth()
    {
        // Given
        var longLine = new string('X', 200); // Exceeds console width
        var inputText = $"Start\n{longLine}\nEnd";

        // When
        var result = _formatter.FormatForClaudeCode(inputText);

        // Then
        Assert.That(result, Is.Not.Null);
        var lines = result.Split('\n');
        foreach (var line in lines)
        {
            Assert.That(line.Length, Is.LessThanOrEqualTo(ConsoleFormatterConstants.MAX_CONSOLE_WIDTH),
                $"Line should not exceed {ConsoleFormatterConstants.MAX_CONSOLE_WIDTH} characters: '{line.Substring(0, Math.Min(50, line.Length))}...'");
        }
        Assert.That(result, Does.Contain("Start"), "Should preserve start content");
        Assert.That(result, Does.Contain("End"), "Should preserve end content");
    }

    /// <summary>
    /// RED: Test should fail initially - Unicode characters should be preserved when possible
    /// </summary>
    [Test]
    public void FormatForClaudeCode_WhenGivenUnicodeCharacters_ShouldPreserveWhenCompatible()
    {
        // Given
        var unicodeText = "├── constraint.test\n│   Priority: 0.85 🔴";

        // When
        var result = _formatter.FormatForClaudeCode(unicodeText);

        // Then
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("├──").Or.Contain("+--"), "Should preserve or convert tree characters");
        Assert.That(result, Does.Contain("🔴").Or.Contain("HIGH"), "Should preserve or convert priority indicators");
    }

    /// <summary>
    /// RED: Test should fail initially - should handle empty and null input gracefully
    /// </summary>
    [Test]
    public void FormatForClaudeCode_WhenGivenEmptyContent_ShouldHandleGracefully()
    {
        // Given
        var emptyText = string.Empty;

        // When
        var result = _formatter.FormatForClaudeCode(emptyText);

        // Then
        Assert.That(result, Is.Not.Null, "Should not return null for empty input");
        Assert.That(result, Is.EqualTo(string.Empty), "Should return empty string for empty input");
    }

    /// <summary>
    /// RED: Test should fail initially - null input should throw ArgumentException
    /// </summary>
    [Test]
    public void FormatForClaudeCode_WhenGivenNullContent_ShouldThrowArgumentException()
    {
        // Given
        string? nullText = null;

        // When
        Action formatNull = () => _formatter.FormatForClaudeCode(nullText!);

        // Then
        Assert.That(formatNull, Throws.TypeOf<ArgumentException>().With.Message.Contains("cannot be null"));
    }

    /// <summary>
    /// RED: Test should fail initially - should optimize whitespace for better readability
    /// </summary>
    [Test]
    public void FormatForClaudeCode_WhenGivenExcessiveWhitespace_ShouldOptimizeSpacing()
    {
        // Given
        var messyText = "Line1\n\n\n\nLine2\n   \n\t\nLine3";

        // When
        var result = _formatter.FormatForClaudeCode(messyText);

        // Then
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("Line1"), "Should preserve Line1");
        Assert.That(result, Does.Contain("Line2"), "Should preserve Line2");
        Assert.That(result, Does.Contain("Line3"), "Should preserve Line3");

        // Should not have excessive consecutive blank lines
        Assert.That(result, Does.Not.Contain("\n\n\n\n"), "Should reduce excessive blank lines");
    }

    /// <summary>
    /// RED: Test should fail initially - should add proper markdown code block formatting
    /// </summary>
    [Test]
    public void FormatForClaudeCode_WhenFormattingTreeContent_ShouldUseMarkdownCodeBlocks()
    {
        // Given
        var treeContent = "Constraint Library: Test\n├── constraint.test\n│   Priority: 0.85";

        // When
        var result = _formatter.FormatForClaudeCode(treeContent);

        // Then
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("```"), "Should include markdown code block markers");
        Assert.That(result, Does.StartWith("```"), "Should start with code block");
        Assert.That(result, Does.EndWith("```"), "Should end with code block");
    }

    /// <summary>
    /// RED: Test should fail initially - should handle performance requirements
    /// </summary>
    [Test]
    public void FormatForClaudeCode_WhenFormattingLargeContent_ShouldMeetPerformanceRequirements()
    {
        // Given
        var largeContent = string.Join("\n", System.Linq.Enumerable.Range(1, 500).Select(i => $"Line {i}: Some constraint content here"));
        var startTime = DateTime.UtcNow;

        // When
        var result = _formatter.FormatForClaudeCode(largeContent);
        var formatTime = DateTime.UtcNow - startTime;

        // Then
        Assert.That(result, Is.Not.Null);
        Assert.That(formatTime.TotalMilliseconds, Is.LessThan(100), "Should format large content quickly");
        Assert.That(result, Does.Contain("Line 1:"), "Should preserve first line");
        Assert.That(result, Does.Contain("Line 500:"), "Should preserve last line");
    }

    /// <summary>
    /// RED: Test should fail initially - should provide formatting options
    /// </summary>
    [Test]
    public void FormatForClaudeCode_WithOptions_ShouldApplyFormattingPreferences()
    {
        // Given
        var content = "├── constraint.test\n│   Priority: 0.85 🔴";
        var options = new ConsoleFormattingOptions
        {
            PreferAscii = true,
            MaxWidth = 80,
            UseCodeBlocks = false
        };

        // When
        var result = _formatter.FormatForClaudeCode(content, options);

        // Then
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Not.Contain("```"), "Should not use code blocks when disabled");
        Assert.That(result, Does.Contain("+--").Or.Contain("├──"), "Should handle ASCII preference");

        var lines = result.Split('\n');
        foreach (var line in lines)
        {
            Assert.That(line.Length, Is.LessThanOrEqualTo(80), "Should respect custom max width");
        }
    }
}
