using System;
using System.Linq;

namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Represents the result of rendering a constraint tree visualization.
/// Contains the rendered tree string, performance metrics, and metadata.
/// </summary>
public sealed class ConstraintTreeVisualization
{
    /// <summary>
    /// Gets the ASCII tree representation of the constraint hierarchy.
    /// </summary>
    public string TreeContent { get; }

    /// <summary>
    /// Gets the time taken to render the tree.
    /// </summary>
    public TimeSpan RenderTime { get; }

    /// <summary>
    /// Gets the total number of constraints rendered.
    /// </summary>
    public int ConstraintCount { get; }

    /// <summary>
    /// Gets whether the rendering completed within the performance threshold.
    /// </summary>
    public bool MeetsPerformanceThreshold => RenderTime.TotalMilliseconds < TreeRenderingConstants.PerformanceThresholdMs;

    /// <summary>
    /// Gets whether the tree is compatible with Claude Code console display.
    /// </summary>
    public bool IsClaudeCodeCompatible =>
        !string.IsNullOrEmpty(TreeContent) &&
        TreeContent.Length < TreeRenderingConstants.MaxConsoleLength &&
        !TreeContent.Contains('\t'); // Only spaces for indentation

    /// <summary>
    /// Gets whether the tree uses ASCII characters only.
    /// </summary>
    public bool IsAsciiFormat =>
        !string.IsNullOrEmpty(TreeContent) &&
        TreeContent.All(c => c <= 127); // ASCII character range

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstraintTreeVisualization"/> class.
    /// </summary>
    /// <param name="treeContent">The ASCII tree content</param>
    /// <param name="renderTime">The time taken to render</param>
    /// <param name="constraintCount">The number of constraints rendered</param>
    public ConstraintTreeVisualization(string treeContent, TimeSpan renderTime, int constraintCount)
    {
        TreeContent = treeContent ?? throw new ArgumentNullException(nameof(treeContent));
        RenderTime = renderTime;
        ConstraintCount = constraintCount;
    }

    /// <summary>
    /// Gets the hierarchical structure information from the tree.
    /// </summary>
    public bool HasHierarchicalStructure => ContainsAnyPattern(TreeContentPatterns.HierarchicalMarkers);

    /// <summary>
    /// Gets whether the tree shows composition relationships.
    /// </summary>
    public bool ShowsCompositionRelationships => ContainsAnyPattern(TreeContentPatterns.CompositionMarkers);

    /// <summary>
    /// Gets whether the tree displays constraint metadata.
    /// </summary>
    public bool DisplaysConstraintMetadata => ContainsAnyPattern(TreeContentPatterns.MetadataMarkers);

    /// <summary>
    /// Helper method to check if tree content contains any of the specified patterns.
    /// </summary>
    /// <param name="patterns">Pipe-separated list of patterns to search for</param>
    /// <returns>True if any pattern is found in the tree content</returns>
    private bool ContainsAnyPattern(string patterns) =>
        patterns.Split('|').Any(pattern => TreeContent.Contains(pattern, StringComparison.Ordinal));
}
