namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Constants for console formatting specific to Claude Code integration.
/// Provides Claude Code optimized formatting parameters.
/// </summary>
public static class ConsoleFormatterConstants
{
    /// <summary>
    /// Maximum console width for Claude Code compatibility.
    /// </summary>
    public const int MAX_CONSOLE_WIDTH = 120;

    /// <summary>
    /// Maximum number of consecutive blank lines allowed.
    /// </summary>
    public const int MAX_CONSECUTIVE_BLANK_LINES = 2;

    /// <summary>
    /// Performance threshold for formatting operations in milliseconds.
    /// </summary>
    public const int FORMATTING_PERFORMANCE_THRESHOLD_MS = 100;

    /// <summary>
    /// Default indentation for wrapped lines.
    /// </summary>
    public const string WRAP_INDENT = "  ";

    /// <summary>
    /// Code block language identifier for constraint trees.
    /// </summary>
    public const string CODE_BLOCK_LANGUAGE = "text";
}