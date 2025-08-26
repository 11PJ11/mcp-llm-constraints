using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Represents the current context for trigger evaluation.
/// Contains information about user activity, file context, and development situation.
/// </summary>
public sealed class TriggerContext
{
    /// <summary>
    /// Keywords extracted from current user context (tool calls, commands, etc.).
    /// </summary>
    public IReadOnlyList<string> Keywords { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Current file path being worked on, if applicable.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Type of development context/activity (testing, implementation, refactoring, etc.).
    /// </summary>
    public string? ContextType { get; init; }

    /// <summary>
    /// Additional metadata about the current development session.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = 
        new Dictionary<string, object>().AsReadOnly();

    /// <summary>
    /// Timestamp when this context was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Session identifier for tracking context across tool calls.
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Creates a new trigger context.
    /// </summary>
    public TriggerContext()
    {
        // Default constructor for property initialization
    }

    /// <summary>
    /// Creates a new trigger context with specified parameters.
    /// </summary>
    /// <param name="keywords">Keywords from current context</param>
    /// <param name="filePath">Current file path</param>
    /// <param name="contextType">Type of development activity</param>
    /// <param name="metadata">Additional context metadata</param>
    /// <param name="sessionId">Session identifier</param>
    public TriggerContext(
        IEnumerable<string>? keywords = null,
        string? filePath = null,
        string? contextType = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        string? sessionId = null)
    {
        Keywords = keywords?.Where(k => !string.IsNullOrWhiteSpace(k)).ToList().AsReadOnly() 
                  ?? new List<string>().AsReadOnly();
        FilePath = filePath;
        ContextType = contextType;
        Metadata = metadata ?? new Dictionary<string, object>().AsReadOnly();
        SessionId = sessionId;
    }

    /// <summary>
    /// Checks if this context contains any of the specified keywords (case-insensitive).
    /// </summary>
    /// <param name="targetKeywords">Keywords to search for</param>
    /// <returns>True if any target keyword is found</returns>
    public bool ContainsAnyKeyword(IEnumerable<string> targetKeywords)
    {
        if (targetKeywords == null)
            return false;

        var contextText = string.Join(" ", Keywords).ToLowerInvariant();
        return targetKeywords.Any(keyword => 
            contextText.Contains(keyword.ToLowerInvariant()));
    }

    /// <summary>
    /// Checks if the file path matches any of the specified patterns.
    /// </summary>
    /// <param name="patterns">File patterns to match against</param>
    /// <returns>True if file path matches any pattern</returns>
    public bool MatchesAnyFilePattern(IEnumerable<string> patterns)
    {
        if (string.IsNullOrWhiteSpace(FilePath) || patterns == null)
            return false;

        foreach (string pattern in patterns)
        {
            if (MatchesFilePattern(pattern))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the context type matches any of the specified patterns.
    /// </summary>
    /// <param name="patterns">Context patterns to match against</param>
    /// <returns>True if context type matches any pattern</returns>
    public bool MatchesAnyContextPattern(IEnumerable<string> patterns)
    {
        if (string.IsNullOrWhiteSpace(ContextType) || patterns == null)
            return false;

        var contextTypeLower = ContextType.ToLowerInvariant();
        return patterns.Any(pattern => 
            contextTypeLower.Contains(pattern.ToLowerInvariant()));
    }

    /// <summary>
    /// Checks if this context has any anti-pattern indicators.
    /// </summary>
    /// <param name="antiPatterns">Anti-patterns to check for</param>
    /// <returns>True if any anti-pattern is detected</returns>
    public bool HasAnyAntiPattern(IEnumerable<string> antiPatterns)
    {
        if (antiPatterns == null)
            return false;

        return ContainsAnyKeyword(antiPatterns) || 
               MatchesAnyContextPattern(antiPatterns);
    }

    /// <summary>
    /// Gets the combined relevance score for this context against trigger configuration.
    /// </summary>
    /// <param name="config">Trigger configuration to evaluate against</param>
    /// <returns>Relevance score between 0.0 and 1.0</returns>
    public double CalculateRelevanceScore(TriggerConfiguration config)
    {
        if (config == null)
            return 0.0;

        // Check for anti-patterns first - they override everything
        if (config.AntiPatterns.Count > 0 && HasAnyAntiPattern(config.AntiPatterns))
            return 0.0;

        // Refactoring Level 2: Extract method to reduce complexity
        return CalculateWeightedMatchScore(config);
    }

    /// <summary>
    /// Calculates weighted match score across all trigger criteria.
    /// Refactoring Level 2: Extracted method for complexity reduction.
    /// </summary>
    private double CalculateWeightedMatchScore(TriggerConfiguration config)
    {
        // Refactoring Level 1: Named constants for readability
        const double KeywordMatchWeight = 0.4;
        const double FilePatternMatchWeight = 0.3;
        const double ContextPatternMatchWeight = 0.3;
        const double PerfectMatchScore = 1.0;

        double score = 0.0;
        int factors = 0;

        // Keyword matching component
        if (config.Keywords.Count > 0)
        {
            double keywordScore = CalculateKeywordMatchScore(config.Keywords);
            score += keywordScore * KeywordMatchWeight;
            factors++;
        }

        // File pattern matching component
        if (config.FilePatterns.Count > 0)
        {
            double fileScore = MatchesAnyFilePattern(config.FilePatterns) ? PerfectMatchScore : 0.0;
            score += fileScore * FilePatternMatchWeight;
            factors++;
        }

        // Context pattern matching component
        if (config.ContextPatterns.Count > 0)
        {
            double contextScore = MatchesAnyContextPattern(config.ContextPatterns) ? PerfectMatchScore : 0.0;
            score += contextScore * ContextPatternMatchWeight;
            factors++;
        }

        return factors > 0 ? score : 0.0;
    }

    /// <summary>
    /// Returns a string representation of the trigger context.
    /// </summary>
    public override string ToString()
    {
        var parts = new List<string>();
        
        if (Keywords.Count > 0)
            parts.Add($"Keywords: [{string.Join(", ", Keywords.Take(3))}]{(Keywords.Count > 3 ? "..." : "")}");
        if (!string.IsNullOrWhiteSpace(FilePath))
            parts.Add($"File: {System.IO.Path.GetFileName(FilePath)}");
        if (!string.IsNullOrWhiteSpace(ContextType))
            parts.Add($"Context: {ContextType}");

        return $"TriggerContext({string.Join(", ", parts)})";
    }

    private bool MatchesFilePattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern) || string.IsNullOrWhiteSpace(FilePath))
            return false;

        // Simple glob pattern matching for common cases
        if (pattern.StartsWith("*"))
        {
            string suffix = pattern.Substring(1);
            return FilePath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.EndsWith("*"))
        {
            string prefix = pattern.Substring(0, pattern.Length - 1);
            return FilePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.Contains("*"))
        {
            // More complex glob pattern - simplified implementation
            string[] parts = pattern.Split('*');
            if (parts.Length == 2)
            {
                return FilePath.StartsWith(parts[0], StringComparison.OrdinalIgnoreCase) &&
                       FilePath.EndsWith(parts[1], StringComparison.OrdinalIgnoreCase);
            }
        }

        // Exact match
        return string.Equals(FilePath, pattern, StringComparison.OrdinalIgnoreCase);
    }

    private double CalculateKeywordMatchScore(IReadOnlyList<string> targetKeywords)
    {
        if (targetKeywords.Count == 0 || Keywords.Count == 0)
            return 0.0;

        int matches = 0;
        var contextText = string.Join(" ", Keywords).ToLowerInvariant();

        foreach (string keyword in targetKeywords)
        {
            if (contextText.Contains(keyword.ToLowerInvariant()))
                matches++;
        }

        return (double)matches / targetKeywords.Count;
    }
}