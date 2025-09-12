using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Visualization;
using ConstraintMcpServer.Domain.Visualization;

namespace ConstraintMcpServer.Tests.Application.Visualization;

/// <summary>
/// Unit tests for SyntaxHighlighter application service.
/// Drives implementation through TDD Red-Green-Refactor cycles.
/// Tests focus on syntax highlighting for enhanced constraint tree readability.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("Application")]
[Category("Syntax")]
public sealed class SyntaxHighlighterTests
{
    private SyntaxHighlighter _highlighter = null!;

    [SetUp]
    public void Setup()
    {
        _highlighter = new SyntaxHighlighter();
    }

    [TearDown]
    public void TearDown()
    {
        // No cleanup needed for stateless highlighter
    }

    /// <summary>
    /// RED: Test should fail initially - should apply syntax highlighting to constraint trees
    /// </summary>
    [Test]
    public async Task ApplySyntaxHighlightingAsync_WhenGivenConstraintTreeContent_ShouldEnhanceReadability()
    {
        // Given
        var treeContent = "Constraint Library: Test\n├── test.constraint.1\n│   Priority: 0.8";
        var options = new SyntaxHighlightingOptions();

        // When
        var result = await _highlighter.ApplySyntaxHighlightingAsync(treeContent, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully apply syntax highlighting");
        Assert.That(result.Value, Is.Not.Null, "Should return highlighted content");
        Assert.That(result.Value.HighlightedContent, Does.Contain(treeContent), "Should preserve original content");
        Assert.That(result.Value.HasHighlighting, Is.True, "Should indicate highlighting was applied");
    }

    /// <summary>
    /// RED: Test should fail initially - should highlight constraint IDs with special formatting
    /// </summary>
    [Test]
    public async Task ApplySyntaxHighlightingAsync_WhenGivenConstraintIds_ShouldHighlightIds()
    {
        // Given
        var treeContent = "├── security.validation.required\n│   Priority: 0.95\n└── performance.optimization.cache";

        // When
        var result = await _highlighter.ApplySyntaxHighlightingAsync(treeContent, SyntaxHighlightingOptions.Default);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully highlight IDs");
        Assert.That(result.Value.HighlightedContent, Does.Contain("security.validation.required"), "Should preserve ID");
        Assert.That(result.Value.HighlightedContent, Does.Contain("performance.optimization.cache"), "Should preserve ID");
        Assert.That(result.Value.HighlightingApplied, Does.Contain("ConstraintId"), "Should mark constraint IDs as highlighted");
    }

    /// <summary>
    /// RED: Test should fail initially - should highlight priority values with color coding
    /// </summary>
    [Test]
    public async Task ApplySyntaxHighlightingAsync_WhenGivenPriorityValues_ShouldHighlightByImportance()
    {
        // Given
        var treeContent = "│   Priority: 0.95 (High)\n│   Priority: 0.5 (Medium)\n│   Priority: 0.2 (Low)";

        // When
        var result = await _highlighter.ApplySyntaxHighlightingAsync(treeContent, SyntaxHighlightingOptions.Default);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully highlight priorities");
        Assert.That(result.Value.HighlightedContent, Does.Contain("0.95"), "Should preserve high priority value");
        Assert.That(result.Value.HighlightedContent, Does.Contain("0.5"), "Should preserve medium priority value");
        Assert.That(result.Value.HighlightedContent, Does.Contain("0.2"), "Should preserve low priority value");
        Assert.That(result.Value.HighlightingApplied, Does.Contain("Priority"), "Should mark priorities as highlighted");
    }

    /// <summary>
    /// RED: Test should fail initially - should highlight tree structure symbols
    /// </summary>
    [Test]
    public async Task ApplySyntaxHighlightingAsync_WhenGivenTreeSymbols_ShouldHighlightStructure()
    {
        // Given
        var treeContent = "├── constraint.test\n│   Details\n└── last.constraint";

        // When
        var result = await _highlighter.ApplySyntaxHighlightingAsync(treeContent, SyntaxHighlightingOptions.Default);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully highlight tree structure");
        Assert.That(result.Value.HighlightedContent, Does.Contain("├──"), "Should preserve tree branch");
        Assert.That(result.Value.HighlightedContent, Does.Contain("│"), "Should preserve tree vertical");
        Assert.That(result.Value.HighlightedContent, Does.Contain("└──"), "Should preserve tree end");
        Assert.That(result.Value.HighlightingApplied, Does.Contain("TreeStructure"), "Should mark tree symbols as highlighted");
    }

    /// <summary>
    /// RED: Test should fail initially - should highlight keywords in constraint descriptions
    /// </summary>
    [Test]
    public async Task ApplySyntaxHighlightingAsync_WhenGivenKeywords_ShouldHighlightImportantTerms()
    {
        // Given
        var treeContent = "│   Triggers: [test, unit, validation]\n│   Keywords: security, performance, reliability";

        // When
        var result = await _highlighter.ApplySyntaxHighlightingAsync(treeContent, SyntaxHighlightingOptions.Default);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully highlight keywords");
        Assert.That(result.Value.HighlightedContent, Does.Contain("test"), "Should preserve keyword");
        Assert.That(result.Value.HighlightedContent, Does.Contain("security"), "Should preserve keyword");
        Assert.That(result.Value.HighlightingApplied, Does.Contain("Keywords"), "Should mark keywords as highlighted");
    }

    /// <summary>
    /// RED: Test should fail initially - should handle empty content gracefully
    /// </summary>
    [Test]
    public async Task ApplySyntaxHighlightingAsync_WhenGivenEmptyContent_ShouldHandleGracefully()
    {
        // Given
        var emptyContent = string.Empty;

        // When
        var result = await _highlighter.ApplySyntaxHighlightingAsync(emptyContent, SyntaxHighlightingOptions.Default);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should handle empty content successfully");
        Assert.That(result.Value.HighlightedContent, Is.EqualTo(string.Empty), "Should return empty content");
        Assert.That(result.Value.HasHighlighting, Is.False, "Should indicate no highlighting applied");
    }

    /// <summary>
    /// RED: Test should fail initially - should throw exception for null content
    /// </summary>
    [Test]
    public async Task ApplySyntaxHighlightingAsync_WhenGivenNullContent_ShouldReturnValidationError()
    {
        // Given
        string? nullContent = null;

        // When
        var result = await _highlighter.ApplySyntaxHighlightingAsync(nullContent!, SyntaxHighlightingOptions.Default);

        // Then
        Assert.That(result.IsError, Is.True, "Should return error for null content");
        Assert.That(result.Error.Message, Does.Contain("cannot be null"), "Should explain null content error");
    }

    /// <summary>
    /// RED: Test should fail initially - should respect highlighting options
    /// </summary>
    [Test]
    public async Task ApplySyntaxHighlightingAsync_WithCustomOptions_ShouldRespectSettings()
    {
        // Given
        var treeContent = "├── test.constraint\n│   Priority: 0.8";
        var options = new SyntaxHighlightingOptions
        {
            HighlightConstraintIds = true,
            HighlightPriorities = false,
            HighlightTreeStructure = true,
            HighlightKeywords = false
        };

        // When
        var result = await _highlighter.ApplySyntaxHighlightingAsync(treeContent, options);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully apply custom options");
        Assert.That(result.Value.HighlightingApplied, Does.Contain("ConstraintId"), "Should highlight IDs when enabled");
        Assert.That(result.Value.HighlightingApplied, Does.Not.Contain("Priority"), "Should not highlight priorities when disabled");
        Assert.That(result.Value.HighlightingApplied, Does.Contain("TreeStructure"), "Should highlight structure when enabled");
        Assert.That(result.Value.HighlightingApplied, Does.Not.Contain("Keywords"), "Should not highlight keywords when disabled");
    }

    /// <summary>
    /// RED: Test should fail initially - should provide highlighting statistics
    /// </summary>
    [Test]
    public async Task ApplySyntaxHighlightingAsync_WhenApplyingHighlighting_ShouldProvideStatistics()
    {
        // Given
        var treeContent = "├── test.constraint.1\n│   Priority: 0.8\n└── test.constraint.2\n    Priority: 0.6";

        // When
        var result = await _highlighter.ApplySyntaxHighlightingAsync(treeContent, SyntaxHighlightingOptions.Default);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully provide statistics");
        var statistics = result.Value.Statistics;
        Assert.That(statistics, Is.Not.Null, "Should provide highlighting statistics");
        Assert.That(statistics.TotalHighlights, Is.GreaterThan(0), "Should count total highlights");
        Assert.That(statistics.HighlightTypes.Count, Is.GreaterThan(0), "Should identify highlight types");
    }

    /// <summary>
    /// RED: Test should fail initially - should meet performance requirements
    /// </summary>
    [Test]
    public async Task ApplySyntaxHighlightingAsync_WhenProcessingLargeContent_ShouldMeetPerformanceRequirements()
    {
        // Given
        var largeContent = string.Join("\n", System.Linq.Enumerable.Range(1, 100).Select(i =>
            $"├── constraint.test.{i}\n│   Priority: 0.{i % 10}\n│   Keywords: test, performance, validation"));
        var startTime = DateTime.UtcNow;

        // When
        var result = await _highlighter.ApplySyntaxHighlightingAsync(largeContent, SyntaxHighlightingOptions.Default);
        var processingTime = DateTime.UtcNow - startTime;

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully process large content");
        Assert.That(processingTime.TotalMilliseconds, Is.LessThan(100), "Should meet performance requirements (<100ms)");
        Assert.That(result.Value.Statistics.ProcessingTime.TotalMilliseconds, Is.LessThan(100), "Should track performance internally");
    }
}
