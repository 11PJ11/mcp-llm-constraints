using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Application.Selection;

/// <summary>
/// Analyzes user input and development session to extract trigger context.
/// Bridges MCP tool calls to business domain context.
/// </summary>
public sealed class ContextAnalyzer : IContextAnalyzer
{
    /// <summary>
    /// Extract development context from MCP tool calls.
    /// Analyzes method name, parameters, and session state to determine trigger context.
    /// </summary>
    /// <param name="methodName">MCP method being called</param>
    /// <param name="parameters">Method parameters containing contextual information</param>
    /// <param name="sessionId">Session identifier for context tracking</param>
    /// <returns>Trigger context extracted from tool call</returns>
    public TriggerContext AnalyzeToolCallContext(string methodName, object[] parameters, string sessionId)
    {
        if (parameters == null)
        {
            // Handle null parameters gracefully
            return new TriggerContext(
                keywords: new[] { ExtractKeywordFromMethod(methodName) },
                filePath: string.Empty,
                contextType: "unknown"
            );
        }

        // Extract keywords from method name and parameters
        var keywords = new List<string> { ExtractKeywordFromMethod(methodName) };
        var filePath = string.Empty;

        // Extract file path from parameters if available
        if (parameters.Length > 0 && parameters[0] is string firstParam)
        {
            filePath = firstParam;
        }

        // Add parameter-based keywords
        foreach (var param in parameters)
        {
            if (param is string paramStr && !string.IsNullOrEmpty(paramStr))
            {
                keywords.AddRange(ExtractKeywordsFromText(paramStr));
            }
        }

        var contextType = DetectContextType(keywords, filePath);

        return new TriggerContext(
            keywords: keywords.Distinct().ToArray(),
            filePath: filePath,
            contextType: contextType
        );
    }

    /// <summary>
    /// Extract keywords and patterns from user input.
    /// Processes natural language input to identify development intent and context.
    /// </summary>
    /// <param name="userInput">Natural language input from user</param>
    /// <param name="sessionId">Session identifier for context tracking</param>
    /// <returns>Trigger context extracted from user input</returns>
    public TriggerContext AnalyzeUserInput(string userInput, string sessionId)
    {
        if (string.IsNullOrEmpty(userInput))
        {
            // Handle empty input gracefully
            return new TriggerContext(
                keywords: Array.Empty<string>(),
                filePath: string.Empty,
                contextType: "unknown"
            );
        }

        // Extract keywords from natural language input
        var keywords = ExtractKeywordsFromText(userInput);
        var contextType = DetectContextType(keywords, string.Empty);

        return new TriggerContext(
            keywords: keywords.ToArray(),
            filePath: string.Empty,
            contextType: contextType
        );
    }
    private static readonly string[] stringArray = new[] { "refactor", "clean" };

    /// <summary>
    /// Detect development activity type based on keywords and file patterns.
    /// Classifies the type of development work being performed.
    /// </summary>
    /// <param name="keywords">Keywords extracted from context</param>
    /// <param name="filePath">File path indicating context</param>
    /// <returns>Context type classification (e.g., "feature_development", "refactoring", "testing")</returns>
    public string DetectContextType(IEnumerable<string> keywords, string filePath)
    {
        var keywordList = keywords.ToList();

        // Check for refactoring context first - specific refactoring keywords take precedence
        if (ContainsAny(keywordList, stringArray))
        {
            return "refactoring";
        }

        // Check for feature development context - new development work
        if (ContainsAny(keywordList, new[] { "implement", "feature", "develop" }) ||
            filePath.Contains("src/", StringComparison.OrdinalIgnoreCase))
        {
            return "feature_development";
        }

        // Check for pure testing context (test files or only test keywords)
        if ((ContainsAny(keywordList, new[] { "test", "unit", "validate" }) &&
             !ContainsAny(keywordList, new[] { "implement", "feature", "develop" })) ||
            filePath.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            return "testing";
        }

        // Check for general improvement context
        if (ContainsAny(keywordList, new[] { "improve" }))
        {
            return "refactoring";
        }

        return "unknown";
    }

    private static string ExtractKeywordFromMethod(string methodName)
    {
        if (string.IsNullOrEmpty(methodName))
        {
            return string.Empty;
        }

        // Extract meaningful keyword from method name (e.g., "tools/create_test_file" -> "test")
        if (methodName.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            return "test";
        }

        if (methodName.Contains("create", StringComparison.OrdinalIgnoreCase))
        {
            return "create";
        }

        if (methodName.Contains("implement", StringComparison.OrdinalIgnoreCase))
        {
            return "implement";
        }

        return methodName.Split('/').LastOrDefault() ?? string.Empty;
    }
    private static readonly char[] separator = new[] { ' ', '_', '.', '/', '\\', '-' };

    private static List<string> ExtractKeywordsFromText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<string>();
        }

        // Simple keyword extraction - split on common separators and filter meaningful words
        var words = text.ToLowerInvariant()
            .Split(separator, StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length > 2) // Filter out very short words
            .ToList();

        return words;
    }

    private static bool ContainsAny(IList<string> source, IEnumerable<string> candidates)
    {
        return candidates.Any(candidate =>
            source.Any(item => item.Contains(candidate, StringComparison.OrdinalIgnoreCase)));
    }
}
