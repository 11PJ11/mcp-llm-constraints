namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Immutable options for console formatting behavior.
/// Provides configuration for Claude Code console integration.
/// Follows CUPID properties: Composable, Predictable, Idiomatic.
/// </summary>
public sealed record ConsoleFormattingOptions
{
    /// <summary>
    /// Gets whether to prefer ASCII characters over Unicode for maximum compatibility.
    /// </summary>
    public bool PreferAscii { get; init; } = false;

    /// <summary>
    /// Gets the maximum width for console output lines.
    /// </summary>
    public int MaxWidth { get; init; } = ConsoleFormatterConstants.MAX_CONSOLE_WIDTH;

    /// <summary>
    /// Gets whether to wrap content in markdown code blocks.
    /// </summary>
    public bool UseCodeBlocks { get; init; } = true;

    /// <summary>
    /// Gets whether to optimize whitespace by removing excessive blank lines.
    /// </summary>
    public bool OptimizeWhitespace { get; init; } = true;

    /// <summary>
    /// Gets the maximum number of consecutive blank lines to allow.
    /// </summary>
    public int MaxConsecutiveBlankLines { get; init; } = ConsoleFormatterConstants.MAX_CONSECUTIVE_BLANK_LINES;

    /// <summary>
    /// Gets the indentation to use for wrapped lines.
    /// </summary>
    public string WrapIndent { get; init; } = ConsoleFormatterConstants.WRAP_INDENT;

    /// <summary>
    /// Default options optimized for Claude Code console display.
    /// </summary>
    public static ConsoleFormattingOptions Default => new();

    /// <summary>
    /// Options optimized for maximum ASCII compatibility.
    /// </summary>
    public static ConsoleFormattingOptions AsciiCompatible => new()
    {
        PreferAscii = true,
        UseCodeBlocks = true,
        OptimizeWhitespace = true
    };

    /// <summary>
    /// Options optimized for compact display without code blocks.
    /// </summary>
    public static ConsoleFormattingOptions Compact => new()
    {
        UseCodeBlocks = false,
        OptimizeWhitespace = true,
        MaxWidth = 100
    };

    /// <summary>
    /// Creates options with a specific maximum width while maintaining other defaults.
    /// </summary>
    /// <param name="width">The maximum width for console output</param>
    /// <returns>Options with specified width</returns>
    public ConsoleFormattingOptions WithMaxWidth(int width) => this with { MaxWidth = width };

    /// <summary>
    /// Creates options with ASCII preference while maintaining other settings.
    /// </summary>
    /// <param name="preferAscii">Whether to prefer ASCII characters</param>
    /// <returns>Options with specified ASCII preference</returns>
    public ConsoleFormattingOptions WithAsciiPreference(bool preferAscii) => this with { PreferAscii = preferAscii };

    /// <summary>
    /// Creates options with code block setting while maintaining other settings.
    /// </summary>
    /// <param name="useCodeBlocks">Whether to use markdown code blocks</param>
    /// <returns>Options with specified code block setting</returns>
    public ConsoleFormattingOptions WithCodeBlocks(bool useCodeBlocks) => this with { UseCodeBlocks = useCodeBlocks };
}
