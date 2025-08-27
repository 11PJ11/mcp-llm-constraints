using System.Collections.Generic;

namespace ConstraintMcpServer.Application.Selection;

/// <summary>
/// Interface for intelligent keyword matching with fuzzy logic and synonym expansion.
/// Provides confidence-based matching for constraint trigger evaluation.
/// </summary>
public interface IKeywordMatcher
{
    /// <summary>
    /// Calculates match confidence between target keywords and context keywords.
    /// Uses fuzzy matching and synonym expansion to provide intelligent scoring.
    /// </summary>
    /// <param name="targetKeywords">Keywords from constraint trigger configuration</param>
    /// <param name="contextKeywords">Keywords extracted from current development context</param>
    /// <returns>Confidence score between 0.0 (no match) and 1.0 (perfect match)</returns>
    double CalculateMatchConfidence(IEnumerable<string> targetKeywords, IEnumerable<string> contextKeywords);

    /// <summary>
    /// Extracts relevant keywords from user input or context text.
    /// Filters out stop words and normalizes technical terms.
    /// </summary>
    /// <param name="input">Raw text input from user context</param>
    /// <returns>Collection of extracted keywords without stop words</returns>
    IEnumerable<string> ExtractKeywords(string input);

    /// <summary>
    /// Expands keywords to include synonyms and variations.
    /// Handles domain-specific terminology and common variations.
    /// </summary>
    /// <param name="keywords">Original keywords to expand</param>
    /// <returns>Expanded keyword collection including synonyms</returns>
    IEnumerable<string> ExpandSynonyms(IEnumerable<string> keywords);
}
