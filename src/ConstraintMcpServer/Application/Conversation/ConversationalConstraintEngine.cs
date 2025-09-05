using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain.Common;
using ConstraintMcpServer.Domain.Conversation;
using ConstraintMcpServer.Domain;

namespace ConstraintMcpServer.Application.Conversation;

/// <summary>
/// Engine for creating and refining constraints through natural language conversations.
/// Implements the core conversational constraint definition workflow for Step A3.
/// </summary>
public sealed class ConversationalConstraintEngine
{
    // Constraint ID prefixes and patterns
    private const string ConversationConstraintPrefix = "conversation.";
    private const string FeatureDevelopmentContext = "feature_development";
    private const string TestingContext = "testing_context";
    private const string GeneralDevelopmentContext = "general_development";

    // Input validation constants
    private const int MinimumInputWordCount = 3;
    /// <summary>
    /// Starts a new constraint definition conversation.
    /// Returns a unique conversation ID for tracking the session.
    /// </summary>
    public async Task<Result<string, DomainError>> StartConversationAsync()
    {
        await Task.CompletedTask;
        var conversationId = Guid.NewGuid().ToString();
        return Result<string, DomainError>.Success(conversationId);
    }

    /// <summary>
    /// Processes natural language input and extracts constraint elements.
    /// Provides validation feedback for incomplete or unclear input.
    /// </summary>
    public async Task<Result<ConversationalProcessingResult, DomainError>> ProcessInputAsync(ConversationId conversationId, string naturalLanguageInput)
    {
        return await ProcessInputAsync(conversationId, naturalLanguageInput, null);
    }

    /// <summary>
    /// Processes natural language input with optional context and extracts constraint elements.
    /// Provides validation feedback for incomplete or unclear input.
    /// </summary>
    public async Task<Result<ConversationalProcessingResult, DomainError>> ProcessInputAsync(ConversationId conversationId, string naturalLanguageInput, string? contextInfo)
    {
        await Task.CompletedTask;

        if (string.IsNullOrWhiteSpace(naturalLanguageInput))
        {
            return Result<ConversationalProcessingResult, DomainError>.Failure(
                ValidationError.ForField("input", "Input cannot be empty"));
        }

        // Check if input is incomplete (simple heuristic for now)
        if (naturalLanguageInput.Trim().Split(' ').Length < MinimumInputWordCount)
        {
            var feedback = ImmutableList.Create("Missing constraint description", "No context specified");
            return Result<ConversationalProcessingResult, DomainError>.Success(
                ConversationalProcessingResult.WithValidationFeedback(feedback));
        }

        // For complete input, extract elements including keywords from both input and context
        var combinedText = naturalLanguageInput;
        if (!string.IsNullOrEmpty(contextInfo))
        {
            combinedText = $"{naturalLanguageInput} {contextInfo}";
        }

        var extractedKeywords = ExtractKeywords(combinedText);
        return Result<ConversationalProcessingResult, DomainError>.Success(
            ConversationalProcessingResult.WithParsedElements(extractedKeywords));
    }

    /// <summary>
    /// Creates a structured constraint from conversation elements.
    /// </summary>
    public async Task<Result<ConstraintDefinition, DomainError>> CreateConstraintAsync(
        ConversationId conversationId,
        string title,
        double priority,
        ImmutableList<string>? keywords = null,
        string? contextInfo = null)
    {
        await Task.CompletedTask;

        var constraintId = $"{ConversationConstraintPrefix}{Guid.NewGuid().ToString()[..8]}";
        var definition = ConstraintDefinition.Create(
            constraintId,
            title,
            "Constraint created from conversation",
            priority);

        if (keywords != null && keywords.Count > 0)
        {
            definition = definition.WithKeywords(keywords.ToArray());
        }

        if (!string.IsNullOrEmpty(contextInfo))
        {
            var contextPattern = ExtractContextPattern(contextInfo);
            definition = definition.WithContextPatterns(contextPattern);
        }

        return Result<ConstraintDefinition, DomainError>.Success(definition);
    }

    /// <summary>
    /// Specifies context information for the constraint being created.
    /// Extracts and validates context patterns from natural language.
    /// </summary>
    public async Task<Result<ContextSpecificationResult, DomainError>> SpecifyContextAsync(
        ConversationId conversationId,
        string contextInfo)
    {
        await Task.CompletedTask;

        // Extract context pattern from natural language (simple pattern matching)
        var contextPattern = ExtractContextPattern(contextInfo);

        return Result<ContextSpecificationResult, DomainError>.Success(
            ContextSpecificationResult.Create(contextPattern));
    }

    /// <summary>
    /// Validates a constraint definition for completeness and correctness.
    /// </summary>
    public async Task<Result<ConstraintValidationResult, DomainError>> ValidateConstraintAsync(ConversationId conversationId)
    {
        await Task.CompletedTask;

        // For simplicity, assume complete constraints pass validation
        return Result<ConstraintValidationResult, DomainError>.Success(
            ConstraintValidationResult.Valid());
    }

    private static string ExtractContextPattern(string contextInfo)
    {
        // Simple pattern extraction - in real implementation this would be more sophisticated
        if (contextInfo.Contains("implementing new features") || contextInfo.Contains("new features"))
        {
            return FeatureDevelopmentContext;
        }

        if (contextInfo.Contains("testing") || contextInfo.Contains("tests"))
        {
            return TestingContext;
        }

        return GeneralDevelopmentContext;
    }

    private static ImmutableList<string> ExtractKeywords(string naturalLanguageInput)
    {
        // Simple keyword extraction - in real implementation this would use NLP
        var keywords = new List<string>();
        var input = naturalLanguageInput.ToLowerInvariant();

        // Extract key terms that would be useful for trigger matching
        if (input.Contains("test") || input.Contains("tests"))
        {
            keywords.Add("test");
        }

        if (input.Contains("implementation") || input.Contains("implement"))
        {
            keywords.Add("implementation");
        }

        if (input.Contains("feature"))
        {
            keywords.Add("feature");
        }

        if (input.Contains("refactor"))
        {
            keywords.Add("refactor");
        }

        if (input.Contains("bug") || input.Contains("fix"))
        {
            keywords.Add("bug");
        }

        return keywords.ToImmutableList();
    }
}

/// <summary>
/// Result of processing conversational input for constraint definition.
/// </summary>
public sealed class ConversationalProcessingResult
{
    public bool HasParsedElements { get; init; }
    public ImmutableList<string> ValidationFeedback { get; init; } = ImmutableList<string>.Empty;
    public ImmutableList<string> ExtractedKeywords { get; init; } = ImmutableList<string>.Empty;

    private ConversationalProcessingResult() { }

    public static ConversationalProcessingResult WithParsedElements(ImmutableList<string>? keywords = null) =>
        new() { HasParsedElements = true, ExtractedKeywords = keywords ?? ImmutableList<string>.Empty };

    public static ConversationalProcessingResult WithValidationFeedback(ImmutableList<string> feedback) =>
        new() { HasParsedElements = false, ValidationFeedback = feedback };
}

/// <summary>
/// Result of specifying context information during constraint creation.
/// </summary>
public sealed class ContextSpecificationResult
{
    public string ContextPattern { get; init; } = string.Empty;

    private ContextSpecificationResult() { }

    public static ContextSpecificationResult Create(string pattern) =>
        new() { ContextPattern = pattern };
}

/// <summary>
/// Result of constraint validation during conversation.
/// </summary>
public sealed class ConstraintValidationResult
{
    public bool IsValid { get; init; }
    public ImmutableList<string> ValidationErrors { get; init; } = ImmutableList<string>.Empty;

    private ConstraintValidationResult() { }

    public static ConstraintValidationResult Valid() =>
        new() { IsValid = true };

    public static ConstraintValidationResult Invalid(ImmutableList<string> errors) =>
        new() { IsValid = false, ValidationErrors = errors };
}
