using System;
using System.Collections.Immutable;

namespace ConstraintMcpServer.Domain.Conversation;

/// <summary>
/// Immutable value object representing a constraint definition created from conversation.
/// Contains all information needed to create an enforceable constraint in the system.
/// Implements CUPID properties: Composable, Predictable, Domain-based.
/// </summary>
public sealed record ConstraintDefinition
{
    /// <summary>
    /// Gets the unique identifier for this constraint.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Gets the human-readable title for this constraint.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Gets the detailed description of what this constraint enforces.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Gets the priority level for this constraint (0.0 to 1.0).
    /// </summary>
    public double Priority { get; init; }

    /// <summary>
    /// Gets the keywords for trigger matching.
    /// </summary>
    public ImmutableList<string> Keywords { get; init; }

    /// <summary>
    /// Gets the context patterns that activate this constraint.
    /// </summary>
    public ImmutableList<string> ContextPatterns { get; init; }

    /// <summary>
    /// Gets the development phases where this constraint applies.
    /// </summary>
    public ImmutableList<string> ApplicablePhases { get; init; }

    /// <summary>
    /// Gets the reminder text to show when the constraint is activated.
    /// </summary>
    public ImmutableList<string> ReminderText { get; init; }

    /// <summary>
    /// Gets when this constraint definition was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets optional metadata for this constraint.
    /// </summary>
    public ImmutableDictionary<string, object> Metadata { get; init; }

    public ConstraintDefinition(
        string id,
        string title,
        string description,
        double priority,
        ImmutableList<string> keywords,
        ImmutableList<string> contextPatterns,
        ImmutableList<string> applicablePhases,
        ImmutableList<string> reminderText,
        DateTimeOffset? createdAt = null,
        ImmutableDictionary<string, object>? metadata = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Priority = priority;
        Keywords = keywords ?? throw new ArgumentNullException(nameof(keywords));
        ContextPatterns = contextPatterns ?? throw new ArgumentNullException(nameof(contextPatterns));
        ApplicablePhases = applicablePhases ?? throw new ArgumentNullException(nameof(applicablePhases));
        ReminderText = reminderText ?? throw new ArgumentNullException(nameof(reminderText));
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        Metadata = metadata ?? ImmutableDictionary<string, object>.Empty;
    }

    /// <summary>
    /// Creates a constraint definition with basic information.
    /// Additional properties can be set using With* methods.
    /// </summary>
    /// <param name="id">Unique constraint identifier</param>
    /// <param name="title">Human-readable title</param>
    /// <param name="description">Detailed description</param>
    /// <param name="priority">Priority level (0.0 to 1.0)</param>
    /// <returns>Basic constraint definition</returns>
    public static ConstraintDefinition Create(
        string id,
        string title,
        string description,
        double priority) =>
        new(
            id,
            title,
            description,
            priority,
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty
        );

    /// <summary>
    /// Creates a constraint definition with keywords for trigger matching.
    /// </summary>
    /// <param name="keywords">Keywords for trigger matching</param>
    /// <returns>Updated constraint definition</returns>
    public ConstraintDefinition WithKeywords(params string[] keywords) =>
        this with { Keywords = keywords.ToImmutableList() };

    /// <summary>
    /// Creates a constraint definition with context patterns.
    /// </summary>
    /// <param name="patterns">Context patterns for activation</param>
    /// <returns>Updated constraint definition</returns>
    public ConstraintDefinition WithContextPatterns(params string[] patterns) =>
        this with { ContextPatterns = patterns.ToImmutableList() };

    /// <summary>
    /// Creates a constraint definition with applicable phases.
    /// </summary>
    /// <param name="phases">Development phases where constraint applies</param>
    /// <returns>Updated constraint definition</returns>
    public ConstraintDefinition WithApplicablePhases(params string[] phases) =>
        this with { ApplicablePhases = phases.ToImmutableList() };

    /// <summary>
    /// Creates a constraint definition with reminder text.
    /// </summary>
    /// <param name="reminders">Reminder text lines</param>
    /// <returns>Updated constraint definition</returns>
    public ConstraintDefinition WithReminderText(params string[] reminders) =>
        this with { ReminderText = reminders.ToImmutableList() };

    /// <summary>
    /// Creates a constraint definition with additional metadata.
    /// </summary>
    /// <param name="key">Metadata key</param>
    /// <param name="value">Metadata value</param>
    /// <returns>Updated constraint definition</returns>
    public ConstraintDefinition WithMetadata(string key, object value) =>
        this with { Metadata = Metadata.SetItem(key, value) };

    /// <summary>
    /// Checks if this constraint matches the given trigger context.
    /// Used by the constraint activation system.
    /// </summary>
    /// <param name="context">Current development context</param>
    /// <returns>Match confidence score (0.0 to 1.0)</returns>
    public double CalculateMatchConfidence(string context)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            return 0.0;
        }

        var contextLower = context.ToLowerInvariant();
        var score = 0.0;

        // Keyword matching (40% of score)
        var keywordMatches = Keywords.Count(k => contextLower.Contains(k.ToLowerInvariant()));
        if (Keywords.Count > 0)
        {
            score += 0.4 * (keywordMatches / (double)Keywords.Count);
        }

        // Context pattern matching (40% of score)
        var patternMatches = ContextPatterns.Count(p => contextLower.Contains(p.ToLowerInvariant()));
        if (ContextPatterns.Count > 0)
        {
            score += 0.4 * (patternMatches / (double)ContextPatterns.Count);
        }

        // Priority boost (20% of score)
        score += 0.2 * Priority;

        return Math.Min(1.0, score);
    }

    /// <summary>
    /// Generates a summary of this constraint for display.
    /// </summary>
    /// <returns>Human-readable constraint summary</returns>
    public string GenerateSummary() =>
        $"{Title} (Priority: {Priority:F2}) - {(Description.Length > 100 ? Description[..97] + "..." : Description)}";

    public override string ToString() =>
        $"ConstraintDefinition(Id: {Id}, Title: {Title}, Priority: {Priority:F2}, Keywords: {Keywords.Count})";
}

/// <summary>
/// Immutable record containing suggestions for refining constraint definitions.
/// Provides actionable feedback for improving constraint quality and effectiveness.
/// </summary>
public sealed record RefinementSuggestions
{
    /// <summary>
    /// Gets suggestions for improving the constraint title.
    /// </summary>
    public ImmutableList<string> TitleSuggestions { get; init; }

    /// <summary>
    /// Gets suggestions for enhancing the constraint description.
    /// </summary>
    public ImmutableList<string> DescriptionSuggestions { get; init; }

    /// <summary>
    /// Gets suggestions for additional keywords.
    /// </summary>
    public ImmutableList<string> KeywordSuggestions { get; init; }

    /// <summary>
    /// Gets suggestions for context patterns.
    /// </summary>
    public ImmutableList<string> ContextPatternSuggestions { get; init; }

    /// <summary>
    /// Gets suggestions for priority adjustment.
    /// </summary>
    public ImmutableList<string> PrioritySuggestions { get; init; }

    /// <summary>
    /// Gets general improvement suggestions.
    /// </summary>
    public ImmutableList<string> GeneralSuggestions { get; init; }

    public RefinementSuggestions(
        ImmutableList<string>? titleSuggestions = null,
        ImmutableList<string>? descriptionSuggestions = null,
        ImmutableList<string>? keywordSuggestions = null,
        ImmutableList<string>? contextPatternSuggestions = null,
        ImmutableList<string>? prioritySuggestions = null,
        ImmutableList<string>? generalSuggestions = null)
    {
        TitleSuggestions = titleSuggestions ?? ImmutableList<string>.Empty;
        DescriptionSuggestions = descriptionSuggestions ?? ImmutableList<string>.Empty;
        KeywordSuggestions = keywordSuggestions ?? ImmutableList<string>.Empty;
        ContextPatternSuggestions = contextPatternSuggestions ?? ImmutableList<string>.Empty;
        PrioritySuggestions = prioritySuggestions ?? ImmutableList<string>.Empty;
        GeneralSuggestions = generalSuggestions ?? ImmutableList<string>.Empty;
    }

    /// <summary>
    /// Creates empty refinement suggestions.
    /// </summary>
    /// <returns>Empty suggestions</returns>
    public static RefinementSuggestions Empty => new();

    /// <summary>
    /// Creates refinement suggestions with general recommendations.
    /// </summary>
    /// <param name="suggestions">General suggestions</param>
    /// <returns>Suggestions with general recommendations</returns>
    public static RefinementSuggestions WithGeneral(params string[] suggestions) =>
        new(generalSuggestions: suggestions.ToImmutableList());

    /// <summary>
    /// Adds title suggestions to existing suggestions.
    /// </summary>
    /// <param name="suggestions">Title suggestions to add</param>
    /// <returns>Updated suggestions</returns>
    public RefinementSuggestions WithTitleSuggestions(params string[] suggestions) =>
        this with { TitleSuggestions = TitleSuggestions.AddRange(suggestions) };

    /// <summary>
    /// Adds description suggestions to existing suggestions.
    /// </summary>
    /// <param name="suggestions">Description suggestions to add</param>
    /// <returns>Updated suggestions</returns>
    public RefinementSuggestions WithDescriptionSuggestions(params string[] suggestions) =>
        this with { DescriptionSuggestions = DescriptionSuggestions.AddRange(suggestions) };

    /// <summary>
    /// Adds keyword suggestions to existing suggestions.
    /// </summary>
    /// <param name="suggestions">Keyword suggestions to add</param>
    /// <returns>Updated suggestions</returns>
    public RefinementSuggestions WithKeywordSuggestions(params string[] suggestions) =>
        this with { KeywordSuggestions = KeywordSuggestions.AddRange(suggestions) };

    /// <summary>
    /// Checks if there are any suggestions available.
    /// </summary>
    /// <returns>True if any suggestions are present</returns>
    public bool HasSuggestions() =>
        TitleSuggestions.Count > 0 ||
        DescriptionSuggestions.Count > 0 ||
        KeywordSuggestions.Count > 0 ||
        ContextPatternSuggestions.Count > 0 ||
        PrioritySuggestions.Count > 0 ||
        GeneralSuggestions.Count > 0;

    /// <summary>
    /// Gets the total number of suggestions across all categories.
    /// </summary>
    /// <returns>Total suggestion count</returns>
    public int GetTotalSuggestionCount() =>
        TitleSuggestions.Count +
        DescriptionSuggestions.Count +
        KeywordSuggestions.Count +
        ContextPatternSuggestions.Count +
        PrioritySuggestions.Count +
        GeneralSuggestions.Count;

    public override string ToString() =>
        $"RefinementSuggestions(Total: {GetTotalSuggestionCount()}, " +
        $"Categories: {(HasSuggestions() ? "Available" : "None")})";
}
