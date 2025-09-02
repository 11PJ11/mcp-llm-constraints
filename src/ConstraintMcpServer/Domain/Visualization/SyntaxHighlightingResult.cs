using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Immutable result of syntax highlighting operation.
/// Contains highlighted content and metadata about highlighting applied.
/// Follows CUPID properties: Composable, Predictable, Idiomatic, Domain-based.
/// </summary>
/// <param name="OriginalContent">The original content before highlighting</param>
/// <param name="HighlightedContent">The content with syntax highlighting applied</param>
/// <param name="HighlightingApplied">List of highlighting types that were applied</param>
/// <param name="Statistics">Statistics about the highlighting operation</param>
public sealed record SyntaxHighlightingResult(
    string OriginalContent,
    string HighlightedContent,
    IReadOnlyList<string> HighlightingApplied,
    SyntaxHighlightingStatistics Statistics
)
{
    /// <summary>
    /// Gets whether any highlighting was applied to the content.
    /// </summary>
    public bool HasHighlighting => HighlightingApplied.Count > 0 && !string.IsNullOrEmpty(HighlightedContent);

    /// <summary>
    /// Gets whether the highlighting operation completed within performance thresholds.
    /// </summary>
    public bool MeetsPerformanceThreshold => Statistics.ProcessingTime.TotalMilliseconds < 100.0;

    /// <summary>
    /// Gets whether specific highlighting type was applied.
    /// </summary>
    /// <param name="highlightType">The highlighting type to check</param>
    /// <returns>True if the highlighting type was applied</returns>
    public bool HasHighlightType(string highlightType)
    {
        return HighlightingApplied.Contains(highlightType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the improvement ratio of content readability (estimated).
    /// </summary>
    public double ReadabilityImprovement => HasHighlighting ? Math.Min(Statistics.TotalHighlights * 0.1, 0.5) : 0.0;

    /// <summary>
    /// Creates a result indicating no highlighting was applied.
    /// </summary>
    /// <param name="originalContent">The original content</param>
    /// <param name="processingTime">Time taken for the operation</param>
    /// <returns>Result with no highlighting applied</returns>
    public static SyntaxHighlightingResult NoHighlighting(string originalContent, TimeSpan processingTime)
    {
        var statistics = new SyntaxHighlightingStatistics(
            TotalHighlights: 0,
            HighlightTypes: new Dictionary<string, int>(),
            ProcessingTime: processingTime
        );

        return new SyntaxHighlightingResult(
            OriginalContent: originalContent,
            HighlightedContent: originalContent,
            HighlightingApplied: Array.Empty<string>(),
            Statistics: statistics
        );
    }
}