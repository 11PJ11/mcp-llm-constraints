using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain.Common;
using ConstraintMcpServer.Domain.Visualization;

namespace ConstraintMcpServer.Application.Visualization;

/// <summary>
/// Application service for applying syntax highlighting to constraint tree content.
/// Enhances readability through color coding and formatting of different content types.
/// Follows CUPID properties: Composable, Predictable, Idiomatic, Domain-based.
/// </summary>
public sealed class SyntaxHighlighter
{
    /// <summary>
    /// Applies syntax highlighting to constraint tree content for enhanced readability.
    /// </summary>
    /// <param name="content">The content to highlight</param>
    /// <param name="options">Highlighting options to apply</param>
    /// <returns>Result containing highlighted content and statistics</returns>
    public async Task<Result<SyntaxHighlightingResult, DomainError>> ApplySyntaxHighlightingAsync(
        string content, SyntaxHighlightingOptions options)
    {
        var validationResult = ValidateContent(content);
        if (validationResult.IsError)
        {
            return Result<SyntaxHighlightingResult, DomainError>.Failure(validationResult.Error);
        }

        if (IsEmptyContent(content))
        {
            var emptyResult = SyntaxHighlightingResult.NoHighlighting(content, TimeSpan.Zero);
            return await Task.FromResult(Result<SyntaxHighlightingResult, DomainError>.Success(emptyResult));
        }

        var highlightingResult = ProcessHighlightingWithPerformanceCheck(content, options);
        if (highlightingResult.IsError)
        {
            return Result<SyntaxHighlightingResult, DomainError>.Failure(highlightingResult.Error);
        }

        return await Task.FromResult(Result<SyntaxHighlightingResult, DomainError>.Success(highlightingResult.Value));
    }

    private static SyntaxHighlightingResult ApplyHighlighting(string content, SyntaxHighlightingOptions options)
    {
        var stopwatch = Stopwatch.StartNew();
        var highlightingContext = CreateHighlightingContext(content);

        ApplyAllHighlightingRules(highlightingContext, options);

        stopwatch.Stop();
        return BuildHighlightingResult(content, highlightingContext, stopwatch.Elapsed);
    }

    private static HighlightingContext CreateHighlightingContext(string content)
    {
        return new HighlightingContext
        {
            Content = content,
            AppliedHighlights = new List<string>(),
            HighlightCounts = new Dictionary<string, int>()
        };
    }

    private static void ApplyAllHighlightingRules(HighlightingContext context, SyntaxHighlightingOptions options)
    {
        if (options.HighlightConstraintIds)
        {
            ApplyHighlightingRule(context, "ConstraintId", () => HighlightConstraintIds(context.Content, options));
        }

        if (options.HighlightPriorities)
        {
            ApplyHighlightingRule(context, "Priority", () => HighlightPriorities(context.Content, options));
        }

        if (options.HighlightTreeStructure)
        {
            ApplyHighlightingRule(context, "TreeStructure", () => HighlightTreeStructure(context.Content, options));
        }

        if (options.HighlightKeywords)
        {
            ApplyHighlightingRule(context, "Keywords", () => HighlightKeywords(context.Content, options));
        }
    }

    private static void ApplyHighlightingRule(HighlightingContext context, string highlightType, Func<(string content, int count)> highlightingFunction)
    {
        var result = highlightingFunction();
        context.Content = result.content;
        if (result.count > 0)
        {
            context.AppliedHighlights.Add(highlightType);
            context.HighlightCounts[highlightType] = result.count;
        }
    }

    private static SyntaxHighlightingResult BuildHighlightingResult(string originalContent, HighlightingContext context, TimeSpan processingTime)
    {
        var statistics = new SyntaxHighlightingStatistics(
            TotalHighlights: context.HighlightCounts.Values.Sum(),
            HighlightTypes: context.HighlightCounts,
            ProcessingTime: processingTime
        );

        return new SyntaxHighlightingResult(
            OriginalContent: originalContent,
            HighlightedContent: context.Content,
            HighlightingApplied: context.AppliedHighlights.AsReadOnly(),
            Statistics: statistics
        );
    }

    // Level 1 Refactoring: Validation and helper methods for better readability
    private static Result<bool, DomainError> ValidateContent(string? content)
    {
        if (content == null)
        {
            return Result<bool, DomainError>.Failure(new ValidationError("NULL_CONTENT", "Content cannot be null"));
        }
        return Result<bool, DomainError>.Success(true);
    }

    private static bool IsEmptyContent(string content)
    {
        return string.IsNullOrEmpty(content);
    }

    private static Result<SyntaxHighlightingResult, DomainError> ProcessHighlightingWithPerformanceCheck(string content, SyntaxHighlightingOptions options)
    {
        var stopwatch = Stopwatch.StartNew();
        var highlightingResult = ApplyHighlighting(content, options);
        stopwatch.Stop();

        if (stopwatch.Elapsed > options.MaxProcessingTime)
        {
            return Result<SyntaxHighlightingResult, DomainError>.Failure(
                new ValidationError("PROCESSING_TIMEOUT", $"Syntax highlighting exceeded maximum processing time of {options.MaxProcessingTime.TotalMilliseconds}ms"));
        }

        return Result<SyntaxHighlightingResult, DomainError>.Success(highlightingResult);
    }

    // Level 3 Refactoring: Internal context class for better organization
    private sealed class HighlightingContext
    {
        public string Content { get; set; } = string.Empty;
        public List<string> AppliedHighlights { get; set; } = null!;
        public Dictionary<string, int> HighlightCounts { get; set; } = null!;
    }

    // Level 2 Refactoring: Specific highlighting methods with clear responsibilities
    private static (string content, int count) HighlightConstraintIds(string content, SyntaxHighlightingOptions options)
    {
        const string constraintIdPattern = @"(?<=├──\s|└──\s)[\w\.-]+(?=\s|$)";
        return ApplyRegexHighlighting(content, constraintIdPattern, options, ApplyConstraintIdFormatting);
    }

    private static (string content, int count) HighlightPriorities(string content, SyntaxHighlightingOptions options)
    {
        const string priorityPattern = @"Priority:\s*(0\.\d+|\d+\.\d+)";
        var matches = Regex.Matches(content, priorityPattern);
        
        if (matches.Count == 0)
        {
            return (content, 0);
        }

        return ApplyPriorityHighlighting(content, matches, options);
    }

    private static (string content, int count) ApplyPriorityHighlighting(string content, MatchCollection matches, SyntaxHighlightingOptions options)
    {
        var highlighted = content;
        foreach (Match match in matches)
        {
            var priorityValue = match.Groups[1].Value;
            var highlightedPriority = ApplyPriorityFormatting(priorityValue, options);
            highlighted = highlighted.Replace(match.Value, $"Priority: {highlightedPriority}");
        }
        return (highlighted, matches.Count);
    }

    private static (string content, int count) HighlightTreeStructure(string content, SyntaxHighlightingOptions options)
    {
        var symbols = new[] { "├──", "│", "└──" };
        var count = 0;
        var highlighted = content;

        foreach (var symbol in symbols)
        {
            var symbolCount = CountOccurrences(highlighted, symbol);
            if (symbolCount > 0)
            {
                var highlightedSymbol = ApplyTreeStructureFormatting(symbol, options);
                highlighted = highlighted.Replace(symbol, highlightedSymbol);
                count += symbolCount;
            }
        }

        return (highlighted, count);
    }

    private static (string content, int count) HighlightKeywords(string content, SyntaxHighlightingOptions options)
    {
        var keywordPatterns = new[]
        {
            @"(?<=Triggers:\s*\[)[^\]]+(?=\])",
            @"(?<=Keywords:\s*)[\w\s,]+(?=\n|$)"
        };

        var count = 0;
        var highlighted = content;

        foreach (var pattern in keywordPatterns)
        {
            var matches = Regex.Matches(highlighted, pattern);
            foreach (Match match in matches)
            {
                var keywords = match.Value;
                var highlightedKeywords = ApplyKeywordFormatting(keywords, options);
                highlighted = highlighted.Replace(match.Value, highlightedKeywords);
                count += CountWordsInString(keywords);
            }
        }

        return (highlighted, count);
    }

    private static string ApplyConstraintIdFormatting(string constraintId, SyntaxHighlightingOptions options)
    {
        if (options.UseAnsiColors)
        {
            return $"\u001b[36m{constraintId}\u001b[0m"; // Cyan
        }
        return constraintId; // For now, return as-is (placeholder for future formatting)
    }

    private static string ApplyPriorityFormatting(string priorityValue, SyntaxHighlightingOptions options)
    {
        if (options.UseAnsiColors && double.TryParse(priorityValue, out var priority))
        {
            return priority switch
            {
                >= 0.8 => $"\u001b[31m{priorityValue}\u001b[0m", // Red for high
                >= 0.5 => $"\u001b[33m{priorityValue}\u001b[0m", // Yellow for medium  
                _ => $"\u001b[32m{priorityValue}\u001b[0m" // Green for low
            };
        }
        return priorityValue;
    }

    private static string ApplyTreeStructureFormatting(string symbol, SyntaxHighlightingOptions options)
    {
        if (options.UseAnsiColors)
        {
            return $"\u001b[90m{symbol}\u001b[0m"; // Gray
        }
        return symbol;
    }

    private static string ApplyKeywordFormatting(string keywords, SyntaxHighlightingOptions options)
    {
        if (options.UseAnsiColors)
        {
            return $"\u001b[35m{keywords}\u001b[0m"; // Magenta
        }
        return keywords;
    }

    private static int CountOccurrences(string content, string substring)
    {
        return (content.Length - content.Replace(substring, "").Length) / substring.Length;
    }

    private static int CountWordsInString(string text)
    {
        return text.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    // Level 2 Refactoring: Generic regex highlighting method to eliminate duplication
    private static (string content, int count) ApplyRegexHighlighting(string content, string pattern, SyntaxHighlightingOptions options, Func<string, SyntaxHighlightingOptions, string> formatter)
    {
        var matches = Regex.Matches(content, pattern);
        
        if (matches.Count == 0)
        {
            return (content, 0);
        }

        var highlighted = content;
        foreach (Match match in matches)
        {
            var originalValue = match.Value;
            var formattedValue = formatter(originalValue, options);
            highlighted = highlighted.Replace(originalValue, formattedValue);
        }

        return (highlighted, matches.Count);
    }
}