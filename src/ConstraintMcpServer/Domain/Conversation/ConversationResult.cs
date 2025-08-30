using System;
using System.Collections.Immutable;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Conversation;

/// <summary>
/// Immutable record representing the result of conversational constraint processing.
/// Contains the generated constraint definition and associated feedback.
/// Implements CUPID properties: Predictable, Domain-based, Idiomatic.
/// </summary>
public sealed record ConversationResult
{
    /// <summary>
    /// Gets the generated constraint definition from the conversation.
    /// </summary>
    public ConstraintDefinition Definition { get; }

    /// <summary>
    /// Gets the validation feedback for the generated constraint.
    /// </summary>
    public ImmutableList<ValidationFeedback> ValidationFeedback { get; }

    /// <summary>
    /// Gets the updated conversation session after processing.
    /// </summary>
    public ConversationSession UpdatedSession { get; }

    /// <summary>
    /// Gets the processing metadata including performance metrics.
    /// </summary>
    public ProcessingMetadata Metadata { get; }

    /// <summary>
    /// Gets optional suggestions for further refinement.
    /// </summary>
    public Option<RefinementSuggestions> RefinementSuggestions { get; }

    /// <summary>
    /// Gets the confidence score for the generated constraint (0.0 to 1.0).
    /// </summary>
    public double ConfidenceScore { get; }

    private ConversationResult(
        ConstraintDefinition definition,
        ImmutableList<ValidationFeedback> validationFeedback,
        ConversationSession updatedSession,
        ProcessingMetadata metadata,
        Option<RefinementSuggestions> refinementSuggestions,
        double confidenceScore)
    {
        Definition = definition;
        ValidationFeedback = validationFeedback;
        UpdatedSession = updatedSession;
        Metadata = metadata;
        RefinementSuggestions = refinementSuggestions;
        ConfidenceScore = confidenceScore;
    }

    /// <summary>
    /// Creates a successful conversation result with validation.
    /// </summary>
    /// <param name="definition">Generated constraint definition</param>
    /// <param name="validationFeedback">Validation feedback list</param>
    /// <param name="updatedSession">Updated session state</param>
    /// <param name="metadata">Processing metadata</param>
    /// <param name="confidenceScore">Confidence score (0.0 to 1.0)</param>
    /// <returns>Validated conversation result</returns>
    public static Result<ConversationResult, ValidationError> CreateSuccessful(
        ConstraintDefinition definition,
        ImmutableList<ValidationFeedback> validationFeedback,
        ConversationSession updatedSession,
        ProcessingMetadata metadata,
        double confidenceScore)
    {
        if (confidenceScore < 0.0 || confidenceScore > 1.0)
        {
            return Result<ConversationResult, ValidationError>.Failure(
                ValidationError.OutOfRange(nameof(confidenceScore), 0.0, 1.0, confidenceScore)
            );
        }

        return Result<ConversationResult, ValidationError>.Success(
            new ConversationResult(
                definition,
                validationFeedback,
                updatedSession,
                metadata,
                Option<RefinementSuggestions>.None(),
                confidenceScore
            )
        );
    }

    /// <summary>
    /// Creates a conversation result with refinement suggestions.
    /// Used when the constraint needs further improvement or clarification.
    /// </summary>
    /// <param name="definition">Generated constraint definition</param>
    /// <param name="validationFeedback">Validation feedback list</param>
    /// <param name="updatedSession">Updated session state</param>
    /// <param name="metadata">Processing metadata</param>
    /// <param name="refinementSuggestions">Suggestions for improvement</param>
    /// <param name="confidenceScore">Confidence score (0.0 to 1.0)</param>
    /// <returns>Validated conversation result with refinement suggestions</returns>
    public static Result<ConversationResult, ValidationError> CreateWithRefinementSuggestions(
        ConstraintDefinition definition,
        ImmutableList<ValidationFeedback> validationFeedback,
        ConversationSession updatedSession,
        ProcessingMetadata metadata,
        RefinementSuggestions refinementSuggestions,
        double confidenceScore)
    {
        if (confidenceScore < 0.0 || confidenceScore > 1.0)
        {
            return Result<ConversationResult, ValidationError>.Failure(
                ValidationError.OutOfRange(nameof(confidenceScore), 0.0, 1.0, confidenceScore)
            );
        }

        return Result<ConversationResult, ValidationError>.Success(
            new ConversationResult(
                definition,
                validationFeedback,
                updatedSession,
                metadata,
                Option<RefinementSuggestions>.Some(refinementSuggestions),
                confidenceScore
            )
        );
    }

    /// <summary>
    /// Checks if the result indicates successful constraint generation.
    /// Based on validation feedback and confidence score.
    /// </summary>
    /// <returns>True if constraint generation was successful</returns>
    public bool IsSuccessful() =>
        ConfidenceScore >= 0.7 &&
        !ValidationFeedback.Any(f => f.Severity == FeedbackSeverity.Error);

    /// <summary>
    /// Checks if the result suggests the constraint needs refinement.
    /// </summary>
    /// <returns>True if refinement is recommended</returns>
    public bool NeedsRefinement() =>
        RefinementSuggestions.HasValue ||
        ConfidenceScore < 0.8 ||
        ValidationFeedback.Any(f => f.Severity >= FeedbackSeverity.Warning);

    /// <summary>
    /// Gets all validation issues that require attention.
    /// </summary>
    /// <returns>List of validation issues to address</returns>
    public ImmutableList<ValidationFeedback> GetValidationIssues() =>
        ValidationFeedback
            .Where(f => f.Severity >= FeedbackSeverity.Warning)
            .ToImmutableList();

    /// <summary>
    /// Gets processing performance summary for monitoring.
    /// </summary>
    /// <returns>Performance summary string</returns>
    public string GetPerformanceSummary() =>
        $"Processing: {Metadata.ProcessingTimeMs}ms, " +
        $"Confidence: {ConfidenceScore:F2}, " +
        $"Issues: {GetValidationIssues().Count}";

    /// <summary>
    /// Creates a summary of the conversation result for display.
    /// </summary>
    /// <returns>Human-readable result summary</returns>
    public string GetSummary() =>
        $"Constraint '{Definition.Id}' generated with {ConfidenceScore:P0} confidence. " +
        $"{ValidationFeedback.Count} validation items, processed in {Metadata.ProcessingTimeMs}ms.";

    public override string ToString() =>
        $"ConversationResult(ConstraintId: {Definition.Id}, " +
        $"Confidence: {ConfidenceScore:F2}, " +
        $"ValidationItems: {ValidationFeedback.Count}, " +
        $"NeedsRefinement: {NeedsRefinement()})";
}

/// <summary>
/// Immutable record containing validation feedback for constraint definitions.
/// </summary>
public sealed record ValidationFeedback
{
    /// <summary>
    /// Gets the severity level of this feedback item.
    /// </summary>
    public FeedbackSeverity Severity { get; }

    /// <summary>
    /// Gets the feedback message for the user.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the specific field or aspect this feedback relates to.
    /// </summary>
    public Option<string> Field { get; }

    /// <summary>
    /// Gets optional suggestion for addressing this feedback.
    /// </summary>
    public Option<string> Suggestion { get; }

    private ValidationFeedback(
        FeedbackSeverity severity,
        string message,
        Option<string> field,
        Option<string> suggestion)
    {
        Severity = severity;
        Message = message;
        Field = field;
        Suggestion = suggestion;
    }

    /// <summary>
    /// Creates informational feedback.
    /// </summary>
    /// <param name="message">Feedback message</param>
    /// <param name="field">Related field (optional)</param>
    /// <returns>Informational feedback</returns>
    public static ValidationFeedback Info(string message, string? field = null) =>
        new(FeedbackSeverity.Info, message, Option<string>.FromNullable(field), Option<string>.None());

    /// <summary>
    /// Creates warning feedback with optional suggestion.
    /// </summary>
    /// <param name="message">Warning message</param>
    /// <param name="field">Related field (optional)</param>
    /// <param name="suggestion">Suggested improvement (optional)</param>
    /// <returns>Warning feedback</returns>
    public static ValidationFeedback Warning(string message, string? field = null, string? suggestion = null) =>
        new(FeedbackSeverity.Warning, message,
            Option<string>.FromNullable(field),
            Option<string>.FromNullable(suggestion));

    /// <summary>
    /// Creates error feedback with optional suggestion.
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="field">Related field (optional)</param>
    /// <param name="suggestion">Suggested fix (optional)</param>
    /// <returns>Error feedback</returns>
    public static ValidationFeedback Error(string message, string? field = null, string? suggestion = null) =>
        new(FeedbackSeverity.Error, message,
            Option<string>.FromNullable(field),
            Option<string>.FromNullable(suggestion));

    public override string ToString() =>
        $"{Severity}: {Message}" +
        (Field.HasValue ? $" (Field: {Field.Value})" : string.Empty);
}

/// <summary>
/// Immutable record containing processing metadata and performance information.
/// </summary>
public sealed record ProcessingMetadata
{
    /// <summary>
    /// Gets the processing time in milliseconds.
    /// </summary>
    public long ProcessingTimeMs { get; }

    /// <summary>
    /// Gets the timestamp when processing started.
    /// </summary>
    public DateTimeOffset StartTime { get; }

    /// <summary>
    /// Gets the timestamp when processing completed.
    /// </summary>
    public DateTimeOffset EndTime { get; }

    /// <summary>
    /// Gets the number of NLP processing steps performed.
    /// </summary>
    public int ProcessingSteps { get; }

    /// <summary>
    /// Gets whether advanced NLP processing was used.
    /// </summary>
    public bool UsedAdvancedNLP { get; }

    /// <summary>
    /// Gets additional performance metrics.
    /// </summary>
    public ImmutableDictionary<string, object> Metrics { get; }

    public ProcessingMetadata(
        long processingTimeMs,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int processingSteps,
        bool usedAdvancedNLP,
        ImmutableDictionary<string, object>? metrics = null)
    {
        ProcessingTimeMs = processingTimeMs;
        StartTime = startTime;
        EndTime = endTime;
        ProcessingSteps = processingSteps;
        UsedAdvancedNLP = usedAdvancedNLP;
        Metrics = metrics ?? ImmutableDictionary<string, object>.Empty;
    }

    /// <summary>
    /// Creates processing metadata from start/end times.
    /// </summary>
    /// <param name="startTime">Processing start time</param>
    /// <param name="endTime">Processing end time</param>
    /// <param name="processingSteps">Number of processing steps</param>
    /// <param name="usedAdvancedNLP">Whether advanced NLP was used</param>
    /// <returns>Processing metadata</returns>
    public static ProcessingMetadata FromTimes(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int processingSteps,
        bool usedAdvancedNLP) =>
        new(
            (long)(endTime - startTime).TotalMilliseconds,
            startTime,
            endTime,
            processingSteps,
            usedAdvancedNLP
        );

    public override string ToString() =>
        $"ProcessingMetadata(Duration: {ProcessingTimeMs}ms, " +
        $"Steps: {ProcessingSteps}, AdvancedNLP: {UsedAdvancedNLP})";
}

/// <summary>
/// Defines severity levels for validation feedback.
/// </summary>
public enum FeedbackSeverity
{
    /// <summary>
    /// Informational feedback, no action required.
    /// </summary>
    Info,

    /// <summary>
    /// Warning, improvement recommended but not required.
    /// </summary>
    Warning,

    /// <summary>
    /// Error, action required to fix the constraint.
    /// </summary>
    Error
}
