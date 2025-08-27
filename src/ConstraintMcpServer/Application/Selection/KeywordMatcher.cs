using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConstraintMcpServer.Application.Selection;

/// <summary>
/// Intelligent keyword matching with confidence scoring and fuzzy matching.
/// Handles variations in terminology and development language.
/// </summary>
public sealed class KeywordMatcher : IKeywordMatcher
{
    // Level 1 Refactoring: Named constants for readability
    private const double ExactMatchScore = 1.0;
    private const double FuzzyMatchScore = 0.7;
    private const double SynonymMatchScore = 0.9;
    private const double NoMatchScore = 0.0;
    private const int MinimumFuzzyMatchLength = 3;
    private const double FuzzySimilarityThreshold = 0.7;

    private static readonly HashSet<string> StopWords = new()
    {
        "a", "an", "and", "are", "as", "at", "be", "by", "for", "from",
        "has", "he", "in", "is", "it", "its", "of", "on", "that", "the",
        "to", "was", "were", "will", "with", "i", "need", "want", "would",
        "could", "should", "can", "may", "might", "must", "shall", "do",
        "does", "did", "have", "had", "this", "these", "those", "they",
        "them", "their", "there", "then", "than", "but", "or", "so", "if"
    };

    private static readonly Dictionary<string, HashSet<string>> SynonymMap = new()
    {
        ["test"] = new() { "test", "testing", "unittest", "unit-test", "spec", "specification" },
        ["testing"] = new() { "test", "testing", "unittest", "unit-test", "spec", "specification" },
        ["unittest"] = new() { "test", "testing", "unittest", "unit-test", "spec", "specification" },
        ["unit-test"] = new() { "test", "testing", "unittest", "unit-test", "spec", "specification" },
        ["tdd"] = new() { "tdd", "test-driven", "test-driven-development", "testing" },
        ["test-driven"] = new() { "tdd", "test-driven", "test-driven-development", "testing" },
        ["hexagonal"] = new() { "hexagonal", "ports-adapters", "clean-architecture", "layered" },
        ["clean-architecture"] = new() { "hexagonal", "ports-adapters", "clean-architecture", "layered", "domain-driven" },
        ["ports-adapters"] = new() { "hexagonal", "ports-adapters", "clean-architecture", "layered" },
        ["domain-driven"] = new() { "domain-driven", "ddd", "clean-architecture", "layered" },
        ["implement"] = new() { "implement", "implementation", "create", "build", "develop" },
        ["implementation"] = new() { "implement", "implementation", "create", "build", "develop" },
        ["refactor"] = new() { "refactor", "refactoring", "restructure", "reorganize", "cleanup" },
        ["refactoring"] = new() { "refactor", "refactoring", "restructure", "reorganize", "cleanup" }
    };

    public double CalculateMatchConfidence(IEnumerable<string> targetKeywords, IEnumerable<string> contextKeywords)
    {
        if (targetKeywords == null || contextKeywords == null)
        {
            return NoMatchScore;
        }

        var targetList = targetKeywords.ToList();
        var contextList = contextKeywords.ToList();

        if (targetList.Count == 0 || contextList.Count == 0)
        {
            return NoMatchScore;
        }

        // Level 2 Refactoring: Extract method for complexity reduction
        return CalculateWeightedMatchScore(targetList, contextList);
    }

    public IEnumerable<string> ExtractKeywords(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Enumerable.Empty<string>();
        }

        // Level 2 Refactoring: Extract method for complexity reduction
        var words = TokenizeInput(input);
        var filteredWords = FilterStopWords(words);
        var normalizedWords = NormalizeKeywords(filteredWords);

        return normalizedWords.Distinct().ToList();
    }

    public IEnumerable<string> ExpandSynonyms(IEnumerable<string> keywords)
    {
        if (keywords == null)
        {
            return Enumerable.Empty<string>();
        }

        var expandedKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string keyword in keywords)
        {
            expandedKeywords.Add(keyword);

            string normalizedKeyword = keyword.ToLowerInvariant();
            if (SynonymMap.TryGetValue(normalizedKeyword, out HashSet<string>? synonyms))
            {
                foreach (string synonym in synonyms)
                {
                    expandedKeywords.Add(synonym);
                }
            }
        }

        return expandedKeywords.ToList();
    }

    /// <summary>
    /// Level 2 Refactoring: Extracted method for complexity reduction.
    /// Calculates weighted match score using exact, fuzzy, and synonym matching.
    /// </summary>
    private double CalculateWeightedMatchScore(List<string> targetKeywords, List<string> contextKeywords)
    {
        int totalMatches = 0;
        double totalScore = 0.0;

        // Expand both sets with synonyms for better matching
        var expandedTargetKeywords = ExpandSynonyms(targetKeywords).ToList();
        var expandedContextKeywords = ExpandSynonyms(contextKeywords).ToList();

        foreach (string targetKeyword in targetKeywords)
        {
            double bestMatchScore = FindBestMatch(targetKeyword, expandedContextKeywords);
            if (bestMatchScore > NoMatchScore)
            {
                totalMatches++;
                totalScore += bestMatchScore;
            }
        }

        return totalMatches > 0 ? totalScore / targetKeywords.Count : NoMatchScore;
    }

    /// <summary>
    /// Level 2 Refactoring: Extracted method for complexity reduction.
    /// Finds the best match for a target keyword in the context keywords.
    /// </summary>
    private double FindBestMatch(string targetKeyword, List<string> contextKeywords)
    {
        string normalizedTarget = targetKeyword.ToLowerInvariant();
        double bestScore = NoMatchScore;

        foreach (string contextKeyword in contextKeywords)
        {
            string normalizedContext = contextKeyword.ToLowerInvariant();

            // Exact match
            if (string.Equals(normalizedTarget, normalizedContext, StringComparison.OrdinalIgnoreCase))
            {
                bestScore = Math.Max(bestScore, ExactMatchScore);
            }
            // Fuzzy match
            else if (IsFuzzyMatch(normalizedTarget, normalizedContext))
            {
                bestScore = Math.Max(bestScore, FuzzyMatchScore);
            }
            // Check if they're synonyms
            else if (AreSynonyms(normalizedTarget, normalizedContext))
            {
                bestScore = Math.Max(bestScore, SynonymMatchScore);
            }
        }

        return bestScore;
    }

    /// <summary>
    /// Level 2 Refactoring: Extracted method for complexity reduction.
    /// Tokenizes input text into individual words.
    /// </summary>
    private static List<string> TokenizeInput(string input)
    {
        // Split on word boundaries, keeping acronyms intact
        var matches = Regex.Matches(input, @"\b[A-Z]{2,}\b|\b\w+\b", RegexOptions.IgnoreCase);
        return matches.Cast<Match>().Select(m => m.Value).ToList();
    }

    /// <summary>
    /// Level 2 Refactoring: Extracted method for complexity reduction.
    /// Filters out stop words from the word list.
    /// </summary>
    private static List<string> FilterStopWords(List<string> words)
    {
        return words.Where(word => !StopWords.Contains(word.ToLowerInvariant())).ToList();
    }

    /// <summary>
    /// Level 2 Refactoring: Extracted method for complexity reduction.
    /// Normalizes keywords while preserving acronyms.
    /// </summary>
    private static List<string> NormalizeKeywords(List<string> words)
    {
        return words.Select(word =>
        {
            // Preserve acronyms (all caps words with 2+ letters)
            if (Regex.IsMatch(word, @"^[A-Z]{2,}$"))
            {
                return word;
            }
            // Convert to lowercase for consistency, except for mixed-case technical terms
            return word.ToLowerInvariant();
        }).ToList();
    }

    /// <summary>
    /// Determines if two keywords are synonyms based on the synonym map.
    /// </summary>
    private bool AreSynonyms(string keyword1, string keyword2)
    {
        if (SynonymMap.TryGetValue(keyword1, out HashSet<string>? synonyms1))
        {
            return synonyms1.Contains(keyword2);
        }

        if (SynonymMap.TryGetValue(keyword2, out HashSet<string>? synonyms2))
        {
            return synonyms2.Contains(keyword1);
        }

        return false;
    }

    /// <summary>
    /// Implements basic fuzzy matching using Levenshtein distance.
    /// </summary>
    private bool IsFuzzyMatch(string keyword1, string keyword2)
    {
        if (keyword1.Length < MinimumFuzzyMatchLength || keyword2.Length < MinimumFuzzyMatchLength)
        {
            return false;
        }

        int maxLength = Math.Max(keyword1.Length, keyword2.Length);
        int distance = CalculateLevenshteinDistance(keyword1, keyword2);
        double similarity = 1.0 - (double)distance / maxLength;

        return similarity >= FuzzySimilarityThreshold;
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two strings.
    /// </summary>
    private static int CalculateLevenshteinDistance(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1))
        {
            return s2?.Length ?? 0;
        }

        if (string.IsNullOrEmpty(s2))
        {
            return s1.Length;
        }

        int[,] distance = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
        {
            distance[i, 0] = i;
        }

        for (int j = 0; j <= s2.Length; j++)
        {
            distance[0, j] = j;
        }

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[s1.Length, s2.Length];
    }
}
