using System;
using ConstraintMcpServer.Domain.Visualization;

namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Immutable representation of enhanced constraint tree visualization with colors and symbols.
/// Extends base tree visualization with enhanced formatting and Claude Code optimization.
/// Follows CUPID properties: Composable, Predictable, Idiomatic, Domain-based.
/// </summary>
public sealed record EnhancedTreeVisualization
{
    /// <summary>
    /// Gets the enhanced tree content with symbols and formatting.
    /// </summary>
    public string TreeContent { get; init; }

    /// <summary>
    /// Gets the total time taken to render the enhanced tree.
    /// </summary>
    public TimeSpan RenderTime { get; init; }

    /// <summary>
    /// Gets the total number of constraints in the visualization.
    /// </summary>
    public int ConstraintCount { get; init; }

    /// <summary>
    /// Gets the color profile used for rendering.
    /// </summary>
    public ColorProfile ColorProfile { get; init; }

    /// <summary>
    /// Gets the symbol set used for rendering.
    /// </summary>
    public EnhancedSymbolSet SymbolSet { get; init; }

    /// <summary>
    /// Gets whether this visualization is optimized for Claude Code console display.
    /// </summary>
    public bool IsClaudeCodeCompatible { get; init; }

    /// <summary>
    /// Creates a new enhanced tree visualization.
    /// </summary>
    /// <param name="treeContent">The enhanced tree content with symbols and formatting</param>
    /// <param name="renderTime">The total time taken to render the enhanced tree</param>
    /// <param name="constraintCount">The total number of constraints in the visualization</param>
    /// <param name="colorProfile">The color profile used for rendering</param>
    /// <param name="symbolSet">The symbol set used for rendering</param>
    /// <param name="isClaudeCodeCompatible">Whether optimized for Claude Code console display</param>
    /// <exception cref="ArgumentException">Thrown when treeContent is null</exception>
    public EnhancedTreeVisualization(
        string treeContent,
        TimeSpan renderTime,
        int constraintCount,
        ColorProfile colorProfile,
        EnhancedSymbolSet symbolSet,
        bool isClaudeCodeCompatible)
    {
        TreeContent = treeContent ?? throw new ArgumentException("Tree content cannot be null", nameof(treeContent));
        RenderTime = renderTime;
        ConstraintCount = constraintCount;
        ColorProfile = colorProfile ?? throw new ArgumentNullException(nameof(colorProfile));
        SymbolSet = symbolSet ?? throw new ArgumentNullException(nameof(symbolSet));
        IsClaudeCodeCompatible = isClaudeCodeCompatible;
    }

    /// <summary>
    /// Gets whether the rendering meets the performance threshold of 50ms.
    /// </summary>
    public bool MeetsPerformanceThreshold => RenderTime.TotalMilliseconds < TreeRenderingConstants.PerformanceThresholdMs;

    /// <summary>
    /// Gets whether the visualization displays hierarchical structure.
    /// </summary>
    public bool HasHierarchicalStructure => 
        !string.IsNullOrEmpty(TreeContent) && 
        (TreeContent.Contains(TreeRenderingConstants.Branch) || 
         TreeContent.Contains("├─") || 
         TreeContent.Contains("├──"));

    /// <summary>
    /// Gets whether the visualization shows composition relationships.
    /// </summary>
    public bool ShowsCompositionRelationships => 
        !string.IsNullOrEmpty(TreeContent) && 
        TreeContent.Contains("Composite", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets whether the visualization displays constraint metadata.
    /// </summary>
    public bool DisplaysConstraintMetadata => 
        !string.IsNullOrEmpty(TreeContent) && 
        (TreeContent.Contains("Priority:", StringComparison.OrdinalIgnoreCase) || 
         TreeContent.Contains("Title:", StringComparison.OrdinalIgnoreCase) ||
         TreeContent.Contains("Keywords:", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets whether the visualization fits within Claude Code console width limits.
    /// </summary>
    public bool FitsConsoleWidth
    {
        get
        {
            if (string.IsNullOrEmpty(TreeContent))
                return true;

            var lines = TreeContent.Split('\n', StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (line.Length > 120) // Claude Code console width limit
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Gets whether the visualization uses ASCII characters only.
    /// </summary>
    public bool UsesAsciiOnly => 
        string.IsNullOrEmpty(TreeContent) || 
        (!TreeContent.Contains("├─") && !TreeContent.Contains("│") && !TreeContent.Contains("──"));

    /// <summary>
    /// Gets whether the visualization includes priority indicators.
    /// </summary>
    public bool IncludesPriorityIndicators =>
        !string.IsNullOrEmpty(TreeContent) &&
        (TreeContent.Contains("🔴") || TreeContent.Contains("🟡") || 
         TreeContent.Contains("🟢") || TreeContent.Contains("🔵"));
}