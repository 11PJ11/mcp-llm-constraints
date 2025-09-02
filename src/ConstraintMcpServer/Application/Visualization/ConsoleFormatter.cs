using System;
using System.Linq;
using System.Text;
using ConstraintMcpServer.Domain.Visualization;

namespace ConstraintMcpServer.Application.Visualization;

/// <summary>
/// Console formatter optimized for Claude Code integration.
/// Provides text formatting for optimal console display and readability.
/// Follows CUPID properties: Composable, Predictable, Idiomatic, Domain-based.
/// </summary>
public sealed class ConsoleFormatter
{
    private const string CODE_BLOCK_OPEN = "```";
    private const string CODE_BLOCK_CLOSE = "```";
    private const int WRAP_SEARCH_DISTANCE = 20;
    private const char SPACE_CHARACTER = ' ';
    private const string UNICODE_BRANCH_LONG = "├──";
    private const string UNICODE_BRANCH_SHORT = "├─";
    private const string UNICODE_VERTICAL = "│";
    private const string UNICODE_END_BRANCH = "└──";
    private const string ASCII_BRANCH = "+--";
    private const string ASCII_VERTICAL = "|";

    /// <summary>
    /// Formats text content for Claude Code console display with default options.
    /// </summary>
    /// <param name="content">The text content to format</param>
    /// <returns>Formatted text optimized for Claude Code console</returns>
    /// <exception cref="ArgumentException">Thrown when content is null</exception>
    public string FormatForClaudeCode(string content)
    {
        return FormatForClaudeCode(content, ConsoleFormattingOptions.Default);
    }

    /// <summary>
    /// Formats text content for Claude Code console display with specified options.
    /// </summary>
    /// <param name="content">The text content to format</param>
    /// <param name="options">Formatting options to apply</param>
    /// <returns>Formatted text optimized for Claude Code console</returns>
    /// <exception cref="ArgumentException">Thrown when content is null</exception>
    public string FormatForClaudeCode(string content, ConsoleFormattingOptions options)
    {
        if (content == null)
        {
            throw new ArgumentException("Content cannot be null", nameof(content));
        }

        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        var processedContent = ApplyFormattingOptions(content, options);
        return WrapWithCodeBlocksIfRequested(processedContent, options);
    }

    private static string ApplyFormattingOptions(string content, ConsoleFormattingOptions options)
    {
        var formatted = content;

        if (options.OptimizeWhitespace)
        {
            formatted = OptimizeWhitespace(formatted, options);
        }

        formatted = WrapLongLines(formatted, options);

        if (options.PreferAscii)
        {
            formatted = ConvertUnicodeToAscii(formatted);
        }

        return formatted;
    }

    private static string OptimizeWhitespace(string content, ConsoleFormattingOptions options)
    {
        var lines = content.Split('\n');
        var optimizedLines = RemoveExcessiveBlankLines(lines, options.MaxConsecutiveBlankLines);
        return string.Join("\n", optimizedLines);
    }

    private static string[] RemoveExcessiveBlankLines(string[] lines, int maxConsecutiveBlankLines)
    {
        var result = new StringBuilder();
        var consecutiveBlankLines = 0;

        foreach (var line in lines)
        {
            var isBlankLine = string.IsNullOrWhiteSpace(line);

            if (isBlankLine)
            {
                consecutiveBlankLines++;
                if (consecutiveBlankLines <= maxConsecutiveBlankLines)
                {
                    result.AppendLine(line);
                }
            }
            else
            {
                consecutiveBlankLines = 0;
                result.AppendLine(line);
            }
        }

        return result.ToString().TrimEnd().Split('\n');
    }

    private static string WrapLongLines(string content, ConsoleFormattingOptions options)
    {
        var lines = content.Split('\n');
        var wrappedLines = lines.SelectMany(line => WrapLineIfNeeded(line, options)).ToArray();
        return string.Join("\n", wrappedLines);
    }

    private static string[] WrapLineIfNeeded(string line, ConsoleFormattingOptions options)
    {
        if (line.Length <= options.MaxWidth)
        {
            return new[] { line };
        }

        var wrappedLines = new StringBuilder();
        var remainingLine = line;

        while (remainingLine.Length > options.MaxWidth)
        {
            var wrapPoint = FindBestWrapPoint(remainingLine, options.MaxWidth);
            var wrappedPortion = remainingLine.Substring(0, wrapPoint);
            wrappedLines.AppendLine(wrappedPortion);

            remainingLine = options.WrapIndent + remainingLine.Substring(wrapPoint).TrimStart();
        }

        if (!string.IsNullOrEmpty(remainingLine))
        {
            wrappedLines.Append(remainingLine);
        }

        return wrappedLines.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }

    private static int FindBestWrapPoint(string line, int maxWidth)
    {
        if (maxWidth >= line.Length)
        {
            return line.Length;
        }

        // Try to find a good break point (space, punctuation)
        for (int i = maxWidth - 1; i >= maxWidth - WRAP_SEARCH_DISTANCE && i > 0; i--)
        {
            if (IsGoodBreakPoint(line[i]))
            {
                return i + 1;
            }
        }

        // If no good break point found, break at max width
        return maxWidth;
    }

    private static string ConvertUnicodeToAscii(string content)
    {
        var asciiContent = ConvertTreeCharactersToAscii(content);
        asciiContent = ConvertPriorityEmojisToAscii(asciiContent);
        asciiContent = ConvertSectionSymbolsToAscii(asciiContent);
        return asciiContent;
    }

    private static string ConvertTreeCharactersToAscii(string content)
    {
        return content
            .Replace(UNICODE_BRANCH_LONG, ASCII_BRANCH)
            .Replace(UNICODE_BRANCH_SHORT, ASCII_BRANCH)
            .Replace(UNICODE_VERTICAL, ASCII_VERTICAL)
            .Replace(UNICODE_END_BRANCH, ASCII_BRANCH);
    }

    private static string ConvertPriorityEmojisToAscii(string content)
    {
        return content
            .Replace("🔴", "HIGH")
            .Replace("🟡", "MED")
            .Replace("🟢", "NORM")
            .Replace("🔵", "LOW");
    }

    private static string ConvertSectionSymbolsToAscii(string content)
    {
        return content
            .Replace("📋", "=")
            .Replace("🏗️", "#")
            .Replace("⚛️", "*")
            .Replace("🧩", "+");
    }

    private static bool IsGoodBreakPoint(char character)
    {
        return char.IsWhiteSpace(character) || char.IsPunctuation(character);
    }

    private static string WrapWithCodeBlocksIfRequested(string content, ConsoleFormattingOptions options)
    {
        if (!options.UseCodeBlocks)
        {
            return content;
        }

        return $"{CODE_BLOCK_OPEN}{ConsoleFormatterConstants.CODE_BLOCK_LANGUAGE}\n{content}\n{CODE_BLOCK_CLOSE}";
    }
}