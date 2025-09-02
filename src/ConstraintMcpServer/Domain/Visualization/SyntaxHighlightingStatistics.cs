using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Immutable statistics about syntax highlighting operation.
/// Provides insights into highlighting effectiveness and performance.
/// Follows CUPID properties: Composable, Predictable, Idiomatic, Domain-based.
/// </summary>
/// <param name="TotalHighlights">Total number of highlights applied</param>
/// <param name="HighlightTypes">Dictionary of highlight types and their counts</param>
/// <param name="ProcessingTime">Time taken to process the highlighting</param>
public sealed record SyntaxHighlightingStatistics(
    int TotalHighlights,
    IReadOnlyDictionary<string, int> HighlightTypes,
    TimeSpan ProcessingTime
)
{
    /// <summary>
    /// Gets the most common highlight type applied.
    /// </summary>
    public string? MostCommonHighlightType => HighlightTypes.Count > 0
        ? HighlightTypes.OrderByDescending(kvp => kvp.Value).First().Key
        : null;

    /// <summary>
    /// Gets the number of different highlight types used.
    /// </summary>
    public int UniqueHighlightTypes => HighlightTypes.Count;

    /// <summary>
    /// Gets whether the processing completed within optimal performance bounds.
    /// </summary>
    public bool OptimalPerformance => ProcessingTime.TotalMilliseconds <= 50.0;

    /// <summary>
    /// Gets whether the processing completed within acceptable performance bounds.
    /// </summary>
    public bool AcceptablePerformance => ProcessingTime.TotalMilliseconds <= 100.0;

    /// <summary>
    /// Gets the highlights per millisecond processing rate.
    /// </summary>
    public double HighlightsPerMillisecond => ProcessingTime.TotalMilliseconds > 0
        ? TotalHighlights / ProcessingTime.TotalMilliseconds
        : 0.0;

    /// <summary>
    /// Gets the count of a specific highlight type.
    /// </summary>
    /// <param name="highlightType">The highlight type to count</param>
    /// <returns>Number of highlights of the specified type</returns>
    public int GetHighlightCount(string highlightType)
    {
        return HighlightTypes.TryGetValue(highlightType, out var count) ? count : 0;
    }

    /// <summary>
    /// Gets whether the highlighting operation had significant impact.
    /// </summary>
    public bool HasSignificantImpact => TotalHighlights >= 5;

    /// <summary>
    /// Creates statistics for an operation with no highlights.
    /// </summary>
    /// <param name="processingTime">Time taken for the operation</param>
    /// <returns>Statistics with zero highlights</returns>
    public static SyntaxHighlightingStatistics Empty(TimeSpan processingTime)
    {
        return new SyntaxHighlightingStatistics(
            TotalHighlights: 0,
            HighlightTypes: new Dictionary<string, int>(),
            ProcessingTime: processingTime
        );
    }
}
