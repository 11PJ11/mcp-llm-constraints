using System;
using System.Collections.Immutable;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Immutable configuration for tree visualization rendering.
/// Follows Command Pattern principles for rendering options with validation.
/// Implements CUPID properties: Composable, Predictable, Idiomatic.
/// </summary>
public sealed record TreeVisualizationOptions
{
    /// <summary>
    /// Gets the rendering style for the tree display.
    /// </summary>
    public RenderingStyle Style { get; init; } = RenderingStyle.Compact;

    /// <summary>
    /// Gets the character set to use for tree drawing.
    /// </summary>
    public CharacterSet CharacterSet { get; init; } = CharacterSet.Ascii;

    /// <summary>
    /// Gets the color scheme for tree rendering.
    /// </summary>
    public ColorScheme Colors { get; init; } = ColorScheme.Monochrome;

    /// <summary>
    /// Gets the maximum depth to render (prevents infinite recursion).
    /// </summary>
    public int MaxDepth { get; init; } = 10;

    /// <summary>
    /// Gets the maximum width in characters for tree output.
    /// </summary>
    public int MaxWidth { get; init; } = 120;

    /// <summary>
    /// Gets whether to show node metadata (priority, keywords, etc.).
    /// </summary>
    public bool ShowMetadata { get; init; } = true;

    /// <summary>
    /// Gets whether to show relationship types between nodes.
    /// </summary>
    public bool ShowRelationshipTypes { get; init; } = false;

    /// <summary>
    /// Gets the set of node types to expand by default.
    /// </summary>
    public ImmutableHashSet<NodeType> ExpandedNodeTypes { get; init; } =
        ImmutableHashSet<NodeType>.Empty;

    /// <summary>
    /// Gets whether to sort child nodes by priority.
    /// </summary>
    public bool SortByPriority { get; init; } = true;

    /// <summary>
    /// Gets whether to include performance metrics in output.
    /// </summary>
    public bool IncludePerformanceMetrics { get; init; } = false;

    /// <summary>
    /// Gets whether to use compact mode for Claude Code console compatibility.
    /// </summary>
    public bool ClaudeCodeCompatible { get; init; } = true;

    /// <summary>
    /// Default configuration optimized for Claude Code console display.
    /// </summary>
    public static TreeVisualizationOptions Default => new()
    {
        Style = RenderingStyle.Compact,
        CharacterSet = CharacterSet.Ascii,
        Colors = ColorScheme.Monochrome,
        MaxDepth = 10,
        MaxWidth = 120,
        ShowMetadata = true,
        ShowRelationshipTypes = false,
        SortByPriority = true,
        IncludePerformanceMetrics = false,
        ClaudeCodeCompatible = true
    };

    /// <summary>
    /// Configuration optimized for detailed analysis and debugging.
    /// </summary>
    public static TreeVisualizationOptions Detailed => new()
    {
        Style = RenderingStyle.Detailed,
        CharacterSet = CharacterSet.Unicode,
        Colors = ColorScheme.Colored,
        MaxDepth = 15,
        MaxWidth = 160,
        ShowMetadata = true,
        ShowRelationshipTypes = true,
        SortByPriority = true,
        IncludePerformanceMetrics = true,
        ClaudeCodeCompatible = false
    };

    /// <summary>
    /// Configuration optimized for hierarchical relationship visualization.
    /// </summary>
    public static TreeVisualizationOptions Hierarchical => new()
    {
        Style = RenderingStyle.Hierarchical,
        CharacterSet = CharacterSet.BoxDrawing,
        Colors = ColorScheme.HighContrast,
        MaxDepth = 12,
        MaxWidth = 140,
        ShowMetadata = false,
        ShowRelationshipTypes = true,
        SortByPriority = false,
        IncludePerformanceMetrics = false,
        ClaudeCodeCompatible = true
    };

    /// <summary>
    /// Validates the configuration and returns any validation errors.
    /// Ensures all settings are within acceptable bounds.
    /// </summary>
    /// <returns>Success with validated options or validation errors</returns>
    public Result<TreeVisualizationOptions, ValidationError> Validate()
    {
        if (MaxDepth < 1 || MaxDepth > 50)
        {
            return Result<TreeVisualizationOptions, ValidationError>.Failure(
                ValidationError.OutOfRange(nameof(MaxDepth), 1, 50, MaxDepth)
            );
        }

        if (MaxWidth < 40 || MaxWidth > 300)
        {
            return Result<TreeVisualizationOptions, ValidationError>.Failure(
                ValidationError.OutOfRange(nameof(MaxWidth), 40, 300, MaxWidth)
            );
        }

        // Compatibility validation
        if (ClaudeCodeCompatible && CharacterSet == CharacterSet.BoxDrawing)
        {
            return Result<TreeVisualizationOptions, ValidationError>.Failure(
                ValidationError.ForField(
                    nameof(CharacterSet),
                    "Box drawing characters may not render correctly in Claude Code console",
                    CharacterSet
                )
            );
        }

        if (ClaudeCodeCompatible && Colors == ColorScheme.Colored)
        {
            return Result<TreeVisualizationOptions, ValidationError>.Failure(
                ValidationError.ForField(
                    nameof(Colors),
                    "Colored output may not display correctly in Claude Code console",
                    Colors
                )
            );
        }

        return Result<TreeVisualizationOptions, ValidationError>.Success(this);
    }

    /// <summary>
    /// Builder for creating custom tree visualization configurations.
    /// Provides fluent API for option composition.
    /// </summary>
    /// <returns>A new builder instance</returns>
    public static TreeVisualizationOptionsBuilder Create() =>
        new();

    /// <summary>
    /// Creates options with specific style while maintaining other defaults.
    /// </summary>
    /// <param name="style">Rendering style to use</param>
    /// <returns>Options with specified style</returns>
    public TreeVisualizationOptions WithStyle(RenderingStyle style) =>
        this with { Style = style };

    /// <summary>
    /// Creates options with specific character set while maintaining other settings.
    /// </summary>
    /// <param name="characterSet">Character set to use</param>
    /// <returns>Options with specified character set</returns>
    public TreeVisualizationOptions WithCharacterSet(CharacterSet characterSet) =>
        this with { CharacterSet = characterSet };

    /// <summary>
    /// Creates options with specific color scheme while maintaining other settings.
    /// </summary>
    /// <param name="colors">Color scheme to use</param>
    /// <returns>Options with specified color scheme</returns>
    public TreeVisualizationOptions WithColors(ColorScheme colors) =>
        this with { Colors = colors };

    /// <summary>
    /// Creates options with specific node types expanded by default.
    /// </summary>
    /// <param name="nodeTypes">Node types to expand</param>
    /// <returns>Options with specified expanded types</returns>
    public TreeVisualizationOptions WithExpandedTypes(params NodeType[] nodeTypes) =>
        this with { ExpandedNodeTypes = nodeTypes.ToImmutableHashSet() };

    /// <summary>
    /// Creates options optimized for performance (minimal rendering).
    /// </summary>
    /// <returns>Performance-optimized options</returns>
    public TreeVisualizationOptions WithPerformanceMode() =>
        this with
        {
            Style = RenderingStyle.Compact,
            ShowMetadata = false,
            ShowRelationshipTypes = false,
            MaxDepth = 5,
            IncludePerformanceMetrics = false
        };

    /// <summary>
    /// Creates options optimized for Claude Code console display.
    /// </summary>
    /// <returns>Claude Code-optimized options</returns>
    public TreeVisualizationOptions WithClaudeCodeOptimization() =>
        this with
        {
            CharacterSet = CharacterSet.Ascii,
            Colors = ColorScheme.Monochrome,
            MaxWidth = 120,
            ClaudeCodeCompatible = true
        };

    /// <summary>
    /// Estimates the rendering complexity based on current options.
    /// Used for performance planning and timeout calculations.
    /// </summary>
    /// <returns>Complexity score from 0.0 (simple) to 1.0 (complex)</returns>
    public double GetRenderingComplexity()
    {
        var complexity = 0.0;

        // Style complexity
        complexity += Style switch
        {
            RenderingStyle.Compact => 0.1,
            RenderingStyle.Detailed => 0.4,
            RenderingStyle.Hierarchical => 0.3,
            _ => 0.2
        };

        // Depth complexity
        complexity += Math.Min(0.3, MaxDepth / 50.0);

        // Feature complexity
        if (ShowMetadata)
        {
            complexity += 0.2;
        }

        if (ShowRelationshipTypes)
        {
            complexity += 0.2;
        }

        if (Colors != ColorScheme.Monochrome)
        {
            complexity += 0.1;
        }

        if (IncludePerformanceMetrics)
        {
            complexity += 0.1;
        }

        return Math.Min(1.0, complexity);
    }

    public override string ToString() =>
        $"TreeVisualizationOptions(Style: {Style}, Charset: {CharacterSet}, " +
        $"Depth: {MaxDepth}, Width: {MaxWidth}, Metadata: {ShowMetadata})";
}

/// <summary>
/// Defines rendering styles for tree visualization.
/// </summary>
public enum RenderingStyle
{
    /// <summary>
    /// Compact style with minimal spacing and decoration.
    /// </summary>
    Compact,

    /// <summary>
    /// Detailed style with comprehensive information display.
    /// </summary>
    Detailed,

    /// <summary>
    /// Hierarchical style emphasizing structure and relationships.
    /// </summary>
    Hierarchical
}

/// <summary>
/// Defines character sets for tree drawing.
/// </summary>
public enum CharacterSet
{
    /// <summary>
    /// Basic ASCII characters (maximum compatibility).
    /// </summary>
    Ascii,

    /// <summary>
    /// Unicode characters for enhanced display.
    /// </summary>
    Unicode,

    /// <summary>
    /// Box drawing characters for professional appearance.
    /// </summary>
    BoxDrawing
}

/// <summary>
/// Defines color schemes for tree rendering.
/// </summary>
public enum ColorScheme
{
    /// <summary>
    /// No colors, plain text only.
    /// </summary>
    Monochrome,

    /// <summary>
    /// Colored output with semantic highlighting.
    /// </summary>
    Colored,

    /// <summary>
    /// High contrast colors for accessibility.
    /// </summary>
    HighContrast
}

/// <summary>
/// Defines types of nodes in constraint hierarchy.
/// </summary>
public enum NodeType
{
    /// <summary>
    /// Root constraint node.
    /// </summary>
    Root,

    /// <summary>
    /// Category or grouping node.
    /// </summary>
    Category,

    /// <summary>
    /// Individual constraint definition.
    /// </summary>
    Constraint,

    /// <summary>
    /// Contextual condition node.
    /// </summary>
    Context,

    /// <summary>
    /// Metadata or property node.
    /// </summary>
    Property
}
