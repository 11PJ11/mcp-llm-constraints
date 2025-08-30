using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Conversation;

/// <summary>
/// Immutable value object representing user's natural language input for constraint definition.
/// Provides content validation, keyword extraction, and complexity analysis.
/// Implements CUPID properties: Predictable, Unix Philosophy, Domain-based.
/// </summary>
public sealed partial record UserInput
{
    private static readonly Regex KeywordExtractionPattern = CreateKeywordExtractionRegex();
    private static readonly ImmutableHashSet<string> RefinementKeywords = ImmutableHashSet.Create(
        StringComparer.OrdinalIgnoreCase,
        "improve", "refine", "update", "modify", "change", "adjust", "enhance", "better", "fix"
    );
    private static readonly ImmutableHashSet<string> ComplexityKeywords = ImmutableHashSet.Create(
        StringComparer.OrdinalIgnoreCase,
        "when", "if", "unless", "whenever", "provided", "except", "while", "during", "after", "before",
        "complex", "comprehensive", "detailed", "specific", "multiple", "various", "different", "several"
    );

    /// <summary>
    /// Gets the raw user input text.
    /// </summary>
    public string RawText { get; }

    /// <summary>
    /// Gets the normalized text with consistent whitespace and casing.
    /// </summary>
    public string NormalizedText { get; }

    /// <summary>
    /// Gets a summary of the input for display purposes (first 50 characters).
    /// </summary>
    public string Summary => NormalizedText.Length <= 50
        ? NormalizedText
        : $"{NormalizedText[..47]}...";

    /// <summary>
    /// Gets the word count of the input.
    /// </summary>
    public int WordCount { get; }

    /// <summary>
    /// Gets the extracted keywords from the input.
    /// </summary>
    public ImmutableList<string> Keywords { get; }

    private UserInput(string rawText)
    {
        RawText = rawText;
        NormalizedText = NormalizeText(rawText);
        WordCount = CountWords(NormalizedText);
        Keywords = ExtractKeywords(NormalizedText);
    }

    /// <summary>
    /// Creates a user input with validation.
    /// Ensures input meets minimum quality requirements for constraint processing.
    /// </summary>
    /// <param name="input">The user's natural language input</param>
    /// <returns>Valid user input or validation error</returns>
    public static Result<UserInput, ValidationError> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<UserInput, ValidationError>.Failure(
                ValidationError.MissingRequiredField(nameof(input))
            );
        }

        var normalized = NormalizeText(input);
        if (normalized.Length < 5)
        {
            return Result<UserInput, ValidationError>.Failure(
                (ValidationError)ValidationError.OutOfRange(nameof(input), 5, 1000, normalized.Length)
                    .WithContext("reason", "Input too short to be meaningful")
            );
        }

        if (normalized.Length > 1000)
        {
            return Result<UserInput, ValidationError>.Failure(
                (ValidationError)ValidationError.OutOfRange(nameof(input), 5, 1000, normalized.Length)
                    .WithContext("reason", "Input too long for effective processing")
            );
        }

        var wordCount = CountWords(normalized);
        if (wordCount < 2)
        {
            return Result<UserInput, ValidationError>.Failure(
                (ValidationError)ValidationError.ForField(nameof(input), "Input must contain at least 2 words", input)
                    .WithContext("wordCount", wordCount)
            );
        }

        return Result<UserInput, ValidationError>.Success(new UserInput(input));
    }

    /// <summary>
    /// Checks if the input contains keywords that suggest constraint refinement.
    /// Used to identify iterative improvement requests.
    /// </summary>
    /// <returns>True if input suggests refinement of existing constraint</returns>
    public bool ContainsRefinementKeywords() =>
        Keywords.Any(keyword => RefinementKeywords.Contains(keyword));

    /// <summary>
    /// Calculates complexity score based on input characteristics.
    /// Used for processing resource allocation and validation depth.
    /// </summary>
    /// <returns>Complexity score from 0.0 (simple) to 1.0 (complex)</returns>
    public double GetComplexityScore()
    {
        var score = 0.0;

        // Length-based complexity (0.0 to 0.3)
        score += Math.Min(0.3, WordCount / 100.0);

        // Conditional complexity (0.0 to 0.4)
        var conditionalKeywords = Keywords.Count(k => ComplexityKeywords.Contains(k));
        score += Math.Min(0.4, conditionalKeywords / 10.0);

        // Keyword diversity (0.0 to 0.3)
        var uniqueKeywords = Keywords.Distinct(StringComparer.OrdinalIgnoreCase).Count();
        score += Math.Min(0.3, uniqueKeywords / 20.0);

        return Math.Min(1.0, score);
    }

    /// <summary>
    /// Identifies the primary intent of the input.
    /// Used for routing to appropriate processing pipelines.
    /// </summary>
    /// <returns>Primary intent category</returns>
    public InputIntent GetPrimaryIntent()
    {
        var keywords = Keywords.Select(k => k.ToLowerInvariant()).ToImmutableHashSet();

        if (keywords.Overlaps(RefinementKeywords.Select(k => k.ToLowerInvariant())))
        {
            return InputIntent.Refinement;
        }

        if (keywords.Contains("remind") || keywords.Contains("reminder"))
        {
            return InputIntent.Reminder;
        }

        if (keywords.Contains("test") || keywords.Contains("testing"))
        {
            return InputIntent.TestingConstraint;
        }

        if (keywords.Contains("architecture") || keywords.Contains("design"))
        {
            return InputIntent.ArchitecturalConstraint;
        }

        if (keywords.Contains("security") || keywords.Contains("secure"))
        {
            return InputIntent.SecurityConstraint;
        }

        return InputIntent.GeneralConstraint;
    }

    /// <summary>
    /// Extracts potential constraint keywords for matching with existing constraints.
    /// Used by constraint selection algorithms.
    /// </summary>
    /// <returns>Keywords relevant for constraint matching</returns>
    public ImmutableList<string> GetConstraintKeywords() =>
        Keywords
            .Where(k => k.Length >= 3) // Filter out short words
            .Where(k => !CommonStopWords.Contains(k.ToLowerInvariant()))
            .ToImmutableList();

    private static string NormalizeText(string input) =>
        Regex.Replace(input.Trim(), @"\s+", " ");

    private static int CountWords(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

    private static ImmutableList<string> ExtractKeywords(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return ImmutableList<string>.Empty;
        }

        return KeywordExtractionPattern
            .Matches(text.ToLowerInvariant())
            .Cast<Match>()
            .Select(m => m.Value)
            .Where(w => w.Length >= 2)
            .Where(w => !CommonStopWords.Contains(w))
            .Distinct()
            .ToImmutableList();
    }

    [GeneratedRegex(@"\b[a-zA-Z]{2,}\b", RegexOptions.Compiled)]
    private static partial Regex CreateKeywordExtractionRegex();

    private static readonly ImmutableHashSet<string> CommonStopWords = ImmutableHashSet.Create(
        StringComparer.OrdinalIgnoreCase,
        "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by",
        "is", "are", "was", "were", "be", "been", "have", "has", "had", "do", "does", "did",
        "will", "would", "could", "should", "can", "may", "might", "must", "shall",
        "this", "that", "these", "those", "i", "you", "he", "she", "it", "we", "they"
    );

    public override string ToString() => Summary;
}

/// <summary>
/// Defines categories of user input intent for processing pipeline routing.
/// </summary>
public enum InputIntent
{
    /// <summary>
    /// General constraint definition request.
    /// </summary>
    GeneralConstraint,

    /// <summary>
    /// Request to refine or modify existing constraint.
    /// </summary>
    Refinement,

    /// <summary>
    /// Request for reminder-type constraint.
    /// </summary>
    Reminder,

    /// <summary>
    /// Testing-related constraint.
    /// </summary>
    TestingConstraint,

    /// <summary>
    /// Architectural or design constraint.
    /// </summary>
    ArchitecturalConstraint,

    /// <summary>
    /// Security-related constraint.
    /// </summary>
    SecurityConstraint
}
