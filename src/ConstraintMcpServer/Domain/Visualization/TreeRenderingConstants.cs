namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Constants for ASCII tree rendering characters and patterns.
/// Provides consistent character substitutes for Unicode box drawing characters.
/// </summary>
public static class TreeRenderingConstants
{
    /// <summary>
    /// ASCII substitute for tree branch connector (├──).
    /// </summary>
    public const string Branch = "+-- ";

    /// <summary>
    /// ASCII substitute for vertical tree line (│).
    /// Used for indentation and continuation lines.
    /// </summary>
    public const string Vertical = "|   ";

    /// <summary>
    /// Standard indentation for tree levels.
    /// </summary>
    public const string Indent = "    ";

    /// <summary>
    /// Performance threshold for tree rendering in milliseconds.
    /// </summary>
    public const int PerformanceThresholdMs = 50;

    /// <summary>
    /// Maximum tree content length for Claude Code console compatibility.
    /// </summary>
    public const int MaxConsoleLength = 10000;
}

/// <summary>
/// String patterns used for tree content analysis and validation.
/// </summary>
public static class TreeContentPatterns
{
    /// <summary>
    /// Patterns that indicate hierarchical structure in tree content.
    /// </summary>
    public const string HierarchicalMarkers = "├──|└--|+--";

    /// <summary>
    /// Patterns that indicate metadata display in tree content.
    /// </summary>
    public const string MetadataMarkers = "Priority:|Keywords:|Title:";

    /// <summary>
    /// Patterns that indicate composition relationships in tree content.
    /// </summary>
    public const string CompositionMarkers = "Composite|Composition:";
}
