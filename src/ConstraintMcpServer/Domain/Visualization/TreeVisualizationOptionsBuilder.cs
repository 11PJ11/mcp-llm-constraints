using System.Collections.Immutable;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Fluent builder for creating tree visualization options with validation.
/// Provides composable configuration with method chaining.
/// Implements CUPID properties: Composable, Unix Philosophy, Predictable.
/// </summary>
public sealed class TreeVisualizationOptionsBuilder
{
    private RenderingStyle _style = RenderingStyle.Compact;
    private CharacterSet _characterSet = CharacterSet.Ascii;
    private ColorScheme _colors = ColorScheme.Monochrome;
    private int _maxDepth = 10;
    private int _maxWidth = 120;
    private bool _showMetadata = true;
    private bool _showRelationshipTypes = false;
    private ImmutableHashSet<NodeType> _expandedNodeTypes = ImmutableHashSet<NodeType>.Empty;
    private bool _sortByPriority = true;
    private bool _includePerformanceMetrics = false;
    private bool _claudeCodeCompatible = true;

    internal TreeVisualizationOptionsBuilder()
    {
    }

    /// <summary>
    /// Sets the rendering style for the tree display.
    /// </summary>
    /// <param name="style">Rendering style to use</param>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder WithStyle(RenderingStyle style)
    {
        _style = style;
        return this;
    }

    /// <summary>
    /// Sets the character set for tree drawing.
    /// </summary>
    /// <param name="characterSet">Character set to use</param>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder WithCharacterSet(CharacterSet characterSet)
    {
        _characterSet = characterSet;
        return this;
    }

    /// <summary>
    /// Sets the color scheme for tree rendering.
    /// </summary>
    /// <param name="colors">Color scheme to use</param>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder WithColors(ColorScheme colors)
    {
        _colors = colors;
        return this;
    }

    /// <summary>
    /// Sets the maximum rendering depth to prevent infinite recursion.
    /// </summary>
    /// <param name="maxDepth">Maximum depth (1-50)</param>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder WithMaxDepth(int maxDepth)
    {
        _maxDepth = maxDepth;
        return this;
    }

    /// <summary>
    /// Sets the maximum width in characters for tree output.
    /// </summary>
    /// <param name="maxWidth">Maximum width (40-300)</param>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder WithMaxWidth(int maxWidth)
    {
        _maxWidth = maxWidth;
        return this;
    }

    /// <summary>
    /// Enables or disables metadata display (priority, keywords, etc.).
    /// </summary>
    /// <param name="show">True to show metadata</param>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder ShowMetadata(bool show = true)
    {
        _showMetadata = show;
        return this;
    }

    /// <summary>
    /// Enables or disables relationship type display between nodes.
    /// </summary>
    /// <param name="show">True to show relationship types</param>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder ShowRelationshipTypes(bool show = true)
    {
        _showRelationshipTypes = show;
        return this;
    }

    /// <summary>
    /// Sets which node types should be expanded by default.
    /// </summary>
    /// <param name="nodeTypes">Node types to expand</param>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder ExpandNodeTypes(params NodeType[] nodeTypes)
    {
        _expandedNodeTypes = nodeTypes.ToImmutableHashSet();
        return this;
    }

    /// <summary>
    /// Enables or disables sorting child nodes by priority.
    /// </summary>
    /// <param name="sort">True to sort by priority</param>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder SortByPriority(bool sort = true)
    {
        _sortByPriority = sort;
        return this;
    }

    /// <summary>
    /// Enables or disables performance metrics inclusion in output.
    /// </summary>
    /// <param name="include">True to include performance metrics</param>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder IncludePerformanceMetrics(bool include = true)
    {
        _includePerformanceMetrics = include;
        return this;
    }

    /// <summary>
    /// Enables or disables Claude Code console compatibility mode.
    /// </summary>
    /// <param name="compatible">True for Claude Code compatibility</param>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder ClaudeCodeCompatible(bool compatible = true)
    {
        _claudeCodeCompatible = compatible;
        return this;
    }

    /// <summary>
    /// Configures options for compact display (minimal information).
    /// </summary>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder ForCompactDisplay()
    {
        _style = RenderingStyle.Compact;
        _showMetadata = false;
        _showRelationshipTypes = false;
        _maxDepth = 8;
        _maxWidth = 100;
        return this;
    }

    /// <summary>
    /// Configures options for detailed analysis (comprehensive information).
    /// </summary>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder ForDetailedAnalysis()
    {
        _style = RenderingStyle.Detailed;
        _showMetadata = true;
        _showRelationshipTypes = true;
        _includePerformanceMetrics = true;
        _maxDepth = 15;
        _maxWidth = 160;
        return this;
    }

    /// <summary>
    /// Configures options for hierarchical structure emphasis.
    /// </summary>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder ForHierarchicalView()
    {
        _style = RenderingStyle.Hierarchical;
        _characterSet = CharacterSet.BoxDrawing;
        _showRelationshipTypes = true;
        _sortByPriority = false;
        return this;
    }

    /// <summary>
    /// Configures options optimized for Claude Code console display.
    /// </summary>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder ForClaudeCodeConsole()
    {
        _characterSet = CharacterSet.Ascii;
        _colors = ColorScheme.Monochrome;
        _maxWidth = 120;
        _claudeCodeCompatible = true;
        return this;
    }

    /// <summary>
    /// Configures options for maximum performance (minimal rendering).
    /// </summary>
    /// <returns>Builder instance for method chaining</returns>
    public TreeVisualizationOptionsBuilder ForPerformance()
    {
        _style = RenderingStyle.Compact;
        _showMetadata = false;
        _showRelationshipTypes = false;
        _maxDepth = 5;
        _includePerformanceMetrics = false;
        return this;
    }

    /// <summary>
    /// Builds the tree visualization options with validation.
    /// </summary>
    /// <returns>Validated options or validation error</returns>
    public Result<TreeVisualizationOptions, ValidationError> Build()
    {
        var options = new TreeVisualizationOptions
        {
            Style = _style,
            CharacterSet = _characterSet,
            Colors = _colors,
            MaxDepth = _maxDepth,
            MaxWidth = _maxWidth,
            ShowMetadata = _showMetadata,
            ShowRelationshipTypes = _showRelationshipTypes,
            ExpandedNodeTypes = _expandedNodeTypes,
            SortByPriority = _sortByPriority,
            IncludePerformanceMetrics = _includePerformanceMetrics,
            ClaudeCodeCompatible = _claudeCodeCompatible
        };

        return options.Validate();
    }

    /// <summary>
    /// Builds the tree visualization options without validation.
    /// Use only when you're certain the configuration is valid.
    /// </summary>
    /// <returns>Tree visualization options</returns>
    public TreeVisualizationOptions BuildUnsafe() =>
        new()
        {
            Style = _style,
            CharacterSet = _characterSet,
            Colors = _colors,
            MaxDepth = _maxDepth,
            MaxWidth = _maxWidth,
            ShowMetadata = _showMetadata,
            ShowRelationshipTypes = _showRelationshipTypes,
            ExpandedNodeTypes = _expandedNodeTypes,
            SortByPriority = _sortByPriority,
            IncludePerformanceMetrics = _includePerformanceMetrics,
            ClaudeCodeCompatible = _claudeCodeCompatible
        };
}
