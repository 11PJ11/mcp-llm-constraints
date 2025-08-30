using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Conversation;

/// <summary>
/// Immutable value object representing the context in which a constraint should apply.
/// Defines when, where, and under what conditions the constraint is relevant.
/// Implements CUPID properties: Composable, Predictable, Domain-based.
/// </summary>
public sealed partial record ConstraintContext
{
    private static readonly Regex ContextPatternRegex = CreateContextPatternRegex();
    private static readonly ImmutableHashSet<string> DevelopmentPhases = ImmutableHashSet.Create(
        StringComparer.OrdinalIgnoreCase,
        "planning", "design", "implementation", "testing", "review", "deployment", "maintenance"
    );
    private static readonly ImmutableHashSet<string> ActivityTypes = ImmutableHashSet.Create(
        StringComparer.OrdinalIgnoreCase,
        "feature", "bugfix", "refactoring", "optimization", "documentation", "setup", "cleanup"
    );

    /// <summary>
    /// Gets the raw context description provided by the user.
    /// </summary>
    public string RawDescription { get; }

    /// <summary>
    /// Gets the normalized context description.
    /// </summary>
    public string NormalizedDescription { get; }

    /// <summary>
    /// Gets the identified context type based on description analysis.
    /// </summary>
    public ContextType Type { get; }

    /// <summary>
    /// Gets the extracted context patterns for matching.
    /// </summary>
    public ImmutableList<string> Patterns { get; }

    /// <summary>
    /// Gets the identified development phase, if any.
    /// </summary>
    public Option<string> DevelopmentPhase { get; }

    /// <summary>
    /// Gets the identified activity type, if any.
    /// </summary>
    public Option<string> ActivityType { get; }

    private ConstraintContext(
        string rawDescription,
        ContextType type,
        ImmutableList<string> patterns,
        Option<string> developmentPhase,
        Option<string> activityType)
    {
        RawDescription = rawDescription;
        NormalizedDescription = NormalizeDescription(rawDescription);
        Type = type;
        Patterns = patterns;
        DevelopmentPhase = developmentPhase;
        ActivityType = activityType;
    }

    /// <summary>
    /// Parses context information from natural language description.
    /// Analyzes text to identify patterns, phases, and constraint applicability.
    /// </summary>
    /// <param name="description">Natural language context description</param>
    /// <returns>Parsed constraint context</returns>
    public static ConstraintContext Parse(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return new ConstraintContext(
                description ?? string.Empty,
                ContextType.General,
                ImmutableList<string>.Empty,
                Option<string>.None(),
                Option<string>.None()
            );
        }

        var normalized = NormalizeDescription(description);
        var type = DetermineContextType(normalized);
        var patterns = ExtractPatterns(normalized);
        var phase = DetermineDevelopmentPhase(normalized);
        var activity = DetermineActivityType(normalized);

        return new ConstraintContext(description, type, patterns, phase, activity);
    }

    /// <summary>
    /// Creates a context for a specific development phase.
    /// Used for phase-specific constraint definitions.
    /// </summary>
    /// <param name="phase">Development phase name</param>
    /// <param name="description">Optional additional description</param>
    /// <returns>Phase-specific constraint context</returns>
    public static ConstraintContext ForPhase(string phase, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phase);

        var fullDescription = description is not null
            ? $"during {phase} phase: {description}"
            : $"during {phase} phase";

        var patterns = ImmutableList.Create($"phase_{phase.ToLowerInvariant()}");

        return new ConstraintContext(
            fullDescription,
            ContextType.DevelopmentPhase,
            patterns,
            Option<string>.Some(phase),
            Option<string>.None()
        );
    }

    /// <summary>
    /// Creates a context for a specific file pattern or location.
    /// Used for location-based constraint definitions.
    /// </summary>
    /// <param name="filePattern">File pattern (e.g., "*.cs", "src/**")</param>
    /// <param name="description">Optional additional description</param>
    /// <returns>File-based constraint context</returns>
    public static ConstraintContext ForFiles(string filePattern, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePattern);

        var fullDescription = description is not null
            ? $"when working with {filePattern}: {description}"
            : $"when working with {filePattern}";

        var patterns = ImmutableList.Create($"file_{filePattern.Replace("*", "glob").Replace("/", "_")}");

        return new ConstraintContext(
            fullDescription,
            ContextType.FilePattern,
            patterns,
            Option<string>.None(),
            Option<string>.None()
        );
    }

    /// <summary>
    /// Checks if this context suggests constraint refinement.
    /// Based on keywords and patterns that indicate iterative improvement.
    /// </summary>
    /// <returns>True if context suggests refinement</returns>
    public bool IsRefinementContext() =>
        NormalizedDescription.Contains("refine", StringComparison.OrdinalIgnoreCase) ||
        NormalizedDescription.Contains("improve", StringComparison.OrdinalIgnoreCase) ||
        NormalizedDescription.Contains("enhance", StringComparison.OrdinalIgnoreCase) ||
        Type == ContextType.Refinement;

    /// <summary>
    /// Calculates complexity score based on context characteristics.
    /// Used for processing resource allocation and validation depth.
    /// </summary>
    /// <returns>Complexity score from 0.0 (simple) to 1.0 (complex)</returns>
    public double GetComplexityScore()
    {
        var score = 0.0;

        // Base complexity by type
        score += Type switch
        {
            ContextType.General => 0.1,
            ContextType.DevelopmentPhase => 0.2,
            ContextType.FilePattern => 0.3,
            ContextType.Conditional => 0.6,
            ContextType.Composite => 0.8,
            ContextType.Refinement => 0.4,
            _ => 0.1
        };

        // Pattern complexity
        score += Math.Min(0.3, Patterns.Count / 10.0);

        // Description complexity
        var wordCount = NormalizedDescription.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        score += Math.Min(0.2, wordCount / 50.0);

        return Math.Min(1.0, score);
    }

    /// <summary>
    /// Checks if this context matches the given trigger context.
    /// Used by constraint activation systems to determine relevance.
    /// </summary>
    /// <param name="triggerContext">Context from current development activity</param>
    /// <returns>Match confidence score from 0.0 (no match) to 1.0 (perfect match)</returns>
    public double MatchesTriggerContext(string triggerContext)
    {
        if (string.IsNullOrWhiteSpace(triggerContext))
        {
            return Type == ContextType.General ? 0.8 : 0.0;
        }

        var normalized = NormalizeDescription(triggerContext);
        var score = 0.0;

        // Direct pattern matching
        foreach (var pattern in Patterns)
        {
            if (normalized.Contains(pattern.Replace("_", " "), StringComparison.OrdinalIgnoreCase))
            {
                score += 0.4;
                break;
            }
        }

        // Phase matching
        DevelopmentPhase.IfPresent(phase =>
        {
            if (normalized.Contains(phase, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.3;
            }
        });

        // Activity matching
        ActivityType.IfPresent(activity =>
        {
            if (normalized.Contains(activity, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.3;
            }
        });

        // Keyword overlap
        var contextKeywords = ContextPatternRegex.Matches(normalized)
            .Cast<Match>()
            .Select(m => m.Value.ToLowerInvariant())
            .ToImmutableHashSet();

        var descriptionKeywords = ContextPatternRegex.Matches(NormalizedDescription)
            .Cast<Match>()
            .Select(m => m.Value.ToLowerInvariant())
            .ToImmutableHashSet();

        var overlap = contextKeywords.Intersect(descriptionKeywords).Count();
        var total = contextKeywords.Union(descriptionKeywords).Count();

        if (total > 0)
        {
            score += 0.4 * (overlap / (double)total);
        }

        return Math.Min(1.0, score);
    }

    private static string NormalizeDescription(string description) =>
        Regex.Replace(description.Trim().ToLowerInvariant(), @"\s+", " ");

    private static ContextType DetermineContextType(string normalized)
    {
        if (normalized.Contains("when") || normalized.Contains("if") || normalized.Contains("during"))
        {
            return ContextType.Conditional;
        }

        if (DevelopmentPhases.Any(phase => normalized.Contains(phase)))
        {
            return ContextType.DevelopmentPhase;
        }

        if (normalized.Contains("*") || normalized.Contains("file") || normalized.Contains("directory"))
        {
            return ContextType.FilePattern;
        }

        if (normalized.Contains("refine") || normalized.Contains("improve"))
        {
            return ContextType.Refinement;
        }

        if (normalized.Split(' ').Length > 10)
        {
            return ContextType.Composite;
        }

        return ContextType.General;
    }

    private static ImmutableList<string> ExtractPatterns(string normalized) =>
        ContextPatternRegex
            .Matches(normalized)
            .Cast<Match>()
            .Select(m => m.Value)
            .Where(w => w.Length >= 3)
            .Distinct()
            .ToImmutableList();

    private static Option<string> DetermineDevelopmentPhase(string normalized)
    {
        var phase = DevelopmentPhases.FirstOrDefault(p => normalized.Contains(p.ToLowerInvariant()));
        return phase is not null ? Option<string>.Some(phase) : Option<string>.None();
    }

    private static Option<string> DetermineActivityType(string normalized)
    {
        var activity = ActivityTypes.FirstOrDefault(a => normalized.Contains(a.ToLowerInvariant()));
        return activity is not null ? Option<string>.Some(activity) : Option<string>.None();
    }

    [GeneratedRegex(@"\b[a-zA-Z]{3,}\b", RegexOptions.Compiled)]
    private static partial Regex CreateContextPatternRegex();

    public override string ToString() =>
        $"{Type}: {(NormalizedDescription.Length > 50 ? NormalizedDescription[..47] + "..." : NormalizedDescription)}";
}

/// <summary>
/// Defines the types of constraint contexts based on their characteristics.
/// </summary>
public enum ContextType
{
    /// <summary>
    /// General context that applies broadly.
    /// </summary>
    General,

    /// <summary>
    /// Context specific to a development phase.
    /// </summary>
    DevelopmentPhase,

    /// <summary>
    /// Context based on file patterns or locations.
    /// </summary>
    FilePattern,

    /// <summary>
    /// Conditional context with specific triggers.
    /// </summary>
    Conditional,

    /// <summary>
    /// Complex context combining multiple conditions.
    /// </summary>
    Composite,

    /// <summary>
    /// Context for refining existing constraints.
    /// </summary>
    Refinement
}
