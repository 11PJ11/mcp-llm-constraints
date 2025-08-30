using System.Collections.Immutable;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Conversation;

/// <summary>
/// Immutable record representing a request to process conversational constraint definition.
/// Contains all information needed for natural language processing and constraint generation.
/// Implements CUPID properties: Composable, Predictable, Domain-based.
/// </summary>
public sealed record ConversationRequest
{
    /// <summary>
    /// Gets the user's input for constraint definition.
    /// </summary>
    public UserInput UserInput { get; init; }

    /// <summary>
    /// Gets optional existing context from previous interactions.
    /// </summary>
    public Option<ConstraintContext> ExistingContext { get; init; }

    /// <summary>
    /// Gets the conversation session state.
    /// </summary>
    public ConversationSession Session { get; init; }

    /// <summary>
    /// Gets optional processing hints for the conversational engine.
    /// </summary>
    public Option<ProcessingHints> ProcessingHints { get; init; }

    private ConversationRequest(
        UserInput userInput,
        Option<ConstraintContext> existingContext,
        ConversationSession session,
        Option<ProcessingHints> processingHints)
    {
        UserInput = userInput;
        ExistingContext = existingContext;
        Session = session;
        ProcessingHints = processingHints;
    }

    /// <summary>
    /// Factory method to create a conversation request with validation.
    /// </summary>
    /// <param name="userInput">Natural language input from user</param>
    /// <param name="sessionId">Conversation session identifier</param>
    /// <returns>Successfully created request or validation error</returns>
    public static Result<ConversationRequest, ValidationError> Create(string userInput, string sessionId)
    {
        var validatedInput = UserInput.Create(userInput);
        if (!validatedInput.IsSuccess)
        {
            return Result<ConversationRequest, ValidationError>.Failure(validatedInput.Error);
        }

        var validatedSessionId = ConversationId.Create(sessionId);
        if (!validatedSessionId.IsSuccess)
        {
            return Result<ConversationRequest, ValidationError>.Failure(validatedSessionId.Error);
        }

        var session = ConversationSession.StartNew(validatedSessionId.Value);

        return Result<ConversationRequest, ValidationError>.Success(
            new ConversationRequest(
                validatedInput.Value,
                Option<ConstraintContext>.None(),
                session,
                Option<ProcessingHints>.None()
            )
        );
    }

    /// <summary>
    /// Factory method to create a conversation request with existing context.
    /// Used for constraint refinement and continuation scenarios.
    /// </summary>
    /// <param name="userInput">Natural language input from user</param>
    /// <param name="existingContext">Context from previous interactions</param>
    /// <param name="session">Active conversation session</param>
    /// <returns>Successfully created request or validation error</returns>
    public static Result<ConversationRequest, ValidationError> CreateWithContext(
        string userInput,
        ConstraintContext existingContext,
        ConversationSession session)
    {
        var validatedInput = UserInput.Create(userInput);
        if (!validatedInput.IsSuccess)
        {
            return Result<ConversationRequest, ValidationError>.Failure(validatedInput.Error);
        }

        return Result<ConversationRequest, ValidationError>.Success(
            new ConversationRequest(
                validatedInput.Value,
                Option<ConstraintContext>.Some(existingContext),
                session,
                Option<ProcessingHints>.None()
            )
        );
    }

    /// <summary>
    /// Creates a new request with processing hints for the conversational engine.
    /// </summary>
    /// <param name="hints">Processing hints and preferences</param>
    /// <returns>Updated request with processing hints</returns>
    public ConversationRequest WithProcessingHints(ProcessingHints hints) =>
        this with { ProcessingHints = Option<ProcessingHints>.Some(hints) };

    /// <summary>
    /// Creates a new request with updated session state.
    /// Used for session state progression during conversation.
    /// </summary>
    /// <param name="updatedSession">Updated session state</param>
    /// <returns>Updated request with new session state</returns>
    public ConversationRequest WithSession(ConversationSession updatedSession) =>
        this with { Session = updatedSession };

    /// <summary>
    /// Creates a new request with additional context information.
    /// </summary>
    /// <param name="context">Additional context to apply</param>
    /// <returns>Updated request with context</returns>
    public ConversationRequest WithContext(ConstraintContext context) =>
        this with { ExistingContext = Option<ConstraintContext>.Some(context) };

    /// <summary>
    /// Determines if this request represents a refinement operation.
    /// Based on user input patterns and session history.
    /// </summary>
    /// <returns>True if request appears to be for constraint refinement</returns>
    public bool IsRefinementRequest() =>
        UserInput.ContainsRefinementKeywords() ||
        ExistingContext.Map(c => c.IsRefinementContext()).GetValueOrDefault(false) ||
        Session.HasPreviousConstraintDefinitions();

    /// <summary>
    /// Gets the estimated processing complexity for resource allocation.
    /// </summary>
    /// <returns>Complexity score from 0.0 (simple) to 1.0 (complex)</returns>
    public double GetProcessingComplexity()
    {
        var baseComplexity = UserInput.GetComplexityScore();
        var contextComplexity = ExistingContext.Map(c => c.GetComplexityScore()).GetValueOrDefault(0.0);
        var sessionComplexity = Session.GetComplexityScore();

        // Weight the different factors
        return (baseComplexity * 0.5) + (contextComplexity * 0.3) + (sessionComplexity * 0.2);
    }

    /// <summary>
    /// Gets the primary intent of this conversation request.
    /// </summary>
    /// <returns>Primary intent for processing pipeline routing</returns>
    public InputIntent GetPrimaryIntent() => UserInput.GetPrimaryIntent();

    public override string ToString() =>
        $"ConversationRequest(Session: {Session.ConversationId}, " +
        $"Input: {UserInput.Summary}, HasContext: {ExistingContext.HasValue}, " +
        $"Complexity: {GetProcessingComplexity():F2})";
}

/// <summary>
/// Immutable record representing processing hints for conversational constraint engine.
/// Provides guidance for natural language processing and constraint generation.
/// </summary>
public sealed record ProcessingHints
{
    /// <summary>
    /// Gets the preferred processing mode for this request.
    /// </summary>
    public ProcessingMode Mode { get; init; } = ProcessingMode.Standard;

    /// <summary>
    /// Gets whether to prioritize speed over thoroughness.
    /// </summary>
    public bool PrioritizeSpeed { get; init; } = false;

    /// <summary>
    /// Gets the expected constraint types for focused processing.
    /// </summary>
    public ImmutableList<string> ExpectedConstraintTypes { get; init; } = ImmutableList<string>.Empty;

    /// <summary>
    /// Gets whether to enable advanced natural language processing.
    /// </summary>
    public bool EnableAdvancedNLP { get; init; } = true;

    /// <summary>
    /// Gets the maximum processing time allowance in milliseconds.
    /// </summary>
    public int MaxProcessingTimeMs { get; init; } = 5000;

    /// <summary>
    /// Gets whether to include detailed validation feedback.
    /// </summary>
    public bool IncludeValidationDetails { get; init; } = true;

    /// <summary>
    /// Default processing hints for standard operation.
    /// </summary>
    public static ProcessingHints Default => new();

    /// <summary>
    /// Processing hints optimized for speed and responsiveness.
    /// </summary>
    public static ProcessingHints Fast => new()
    {
        Mode = ProcessingMode.Fast,
        PrioritizeSpeed = true,
        EnableAdvancedNLP = false,
        MaxProcessingTimeMs = 2000,
        IncludeValidationDetails = false
    };

    /// <summary>
    /// Processing hints optimized for thoroughness and accuracy.
    /// </summary>
    public static ProcessingHints Thorough => new()
    {
        Mode = ProcessingMode.Thorough,
        PrioritizeSpeed = false,
        EnableAdvancedNLP = true,
        MaxProcessingTimeMs = 10000,
        IncludeValidationDetails = true
    };

    /// <summary>
    /// Creates processing hints with specific constraint type expectations.
    /// </summary>
    /// <param name="constraintTypes">Expected constraint types</param>
    /// <returns>Hints with constraint type expectations</returns>
    public ProcessingHints WithExpectedTypes(params string[] constraintTypes) =>
        this with { ExpectedConstraintTypes = constraintTypes.ToImmutableList() };

    /// <summary>
    /// Creates processing hints with specific time limit.
    /// </summary>
    /// <param name="maxTimeMs">Maximum processing time in milliseconds</param>
    /// <returns>Hints with time limit</returns>
    public ProcessingHints WithTimeLimit(int maxTimeMs) =>
        this with { MaxProcessingTimeMs = maxTimeMs };

    public override string ToString() =>
        $"ProcessingHints(Mode: {Mode}, Speed: {PrioritizeSpeed}, " +
        $"TimeLimit: {MaxProcessingTimeMs}ms)";
}

/// <summary>
/// Defines processing modes for conversational constraint engine.
/// </summary>
public enum ProcessingMode
{
    /// <summary>
    /// Standard processing with balanced speed and accuracy.
    /// </summary>
    Standard,

    /// <summary>
    /// Fast processing prioritizing speed over thoroughness.
    /// </summary>
    Fast,

    /// <summary>
    /// Thorough processing prioritizing accuracy over speed.
    /// </summary>
    Thorough,

    /// <summary>
    /// Interactive processing with real-time feedback.
    /// </summary>
    Interactive
}
