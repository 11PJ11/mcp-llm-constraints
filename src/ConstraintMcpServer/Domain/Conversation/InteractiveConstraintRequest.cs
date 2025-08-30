using System;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Conversation;

/// <summary>
/// Immutable value object representing a request for interactive constraint definition.
/// Follows Domain-Driven Design principles with ubiquitous language and null safety.
/// Implements CUPID properties: Composable, Predictable, Domain-based.
/// </summary>
public sealed record InteractiveConstraintRequest
{
    /// <summary>
    /// Gets the unique identifier for the conversation session.
    /// </summary>
    public ConversationId ConversationId { get; init; }

    /// <summary>
    /// Gets the user's natural language input for constraint definition.
    /// </summary>
    public UserInput Input { get; init; }

    /// <summary>
    /// Gets optional context information about when/where the constraint should apply.
    /// </summary>
    public Option<ConstraintContext> Context { get; init; }

    /// <summary>
    /// Gets the timestamp when this request was created.
    /// </summary>
    public RequestTimestamp Timestamp { get; init; }

    /// <summary>
    /// Gets the optional priority level specified by the user (0.0 to 1.0).
    /// </summary>
    public Option<double> PriorityLevel { get; init; }

    // Private constructor enforces factory usage for validation
    private InteractiveConstraintRequest(
        ConversationId conversationId,
        UserInput input,
        Option<ConstraintContext> context,
        RequestTimestamp timestamp,
        Option<double> priorityLevel)
    {
        ConversationId = conversationId;
        Input = input;
        Context = context;
        Timestamp = timestamp;
        PriorityLevel = priorityLevel;
    }

    /// <summary>
    /// Factory method to create a constraint request with validation.
    /// Ensures all business rules are enforced at creation time.
    /// </summary>
    /// <param name="conversationId">Unique conversation identifier</param>
    /// <param name="userInput">Natural language constraint description</param>
    /// <param name="contextInfo">Optional context information</param>
    /// <param name="priorityLevel">Optional priority level (0.0 to 1.0)</param>
    /// <returns>Successfully validated request or validation error</returns>
    public static Result<InteractiveConstraintRequest, ValidationError> Create(
        string conversationId,
        string userInput,
        string? contextInfo = null,
        double? priorityLevel = null)
    {
        // Validate conversation ID
        var validatedId = ConversationId.Create(conversationId);
        if (!validatedId.IsSuccess)
        {
            return Result<InteractiveConstraintRequest, ValidationError>.Failure(validatedId.Error);
        }

        // Validate user input
        var validatedInput = UserInput.Create(userInput);
        if (!validatedInput.IsSuccess)
        {
            return Result<InteractiveConstraintRequest, ValidationError>.Failure(validatedInput.Error);
        }

        // Parse optional context
        var context = contextInfo is not null
            ? Option<ConstraintContext>.Some(ConstraintContext.Parse(contextInfo))
            : Option<ConstraintContext>.None();

        // Validate optional priority level
        var validatedPriority = priorityLevel.HasValue
            ? ValidatePriorityLevel(priorityLevel.Value).Map(Option<double>.Some)
            : Result<Option<double>, ValidationError>.Success(Option<double>.None());

        if (!validatedPriority.IsSuccess)
        {
            return Result<InteractiveConstraintRequest, ValidationError>.Failure(validatedPriority.Error);
        }

        return Result<InteractiveConstraintRequest, ValidationError>.Success(
            new InteractiveConstraintRequest(
                validatedId.Value,
                validatedInput.Value,
                context,
                RequestTimestamp.Now(),
                validatedPriority.Value
            )
        );
    }

    /// <summary>
    /// Creates a new request with additional context information.
    /// Follows immutable update pattern for constraint refinement.
    /// </summary>
    /// <param name="newContext">New context to apply</param>
    /// <returns>Updated request with new context</returns>
    public InteractiveConstraintRequest WithContext(ConstraintContext newContext) =>
        this with { Context = Option<ConstraintContext>.Some(newContext) };

    /// <summary>
    /// Creates a new request with updated user input.
    /// Used for constraint refinement iterations.
    /// </summary>
    /// <param name="newInput">Updated user input</param>
    /// <returns>Updated request with new input</returns>
    public Result<InteractiveConstraintRequest, ValidationError> WithInput(string newInput)
    {
        var validatedInput = UserInput.Create(newInput);
        if (!validatedInput.IsSuccess)
        {
            return Result<InteractiveConstraintRequest, ValidationError>.Failure(validatedInput.Error);
        }

        return Result<InteractiveConstraintRequest, ValidationError>.Success(
            this with { Input = validatedInput.Value }
        );
    }

    /// <summary>
    /// Creates a new request with updated priority level.
    /// Used when user wants to adjust constraint importance.
    /// </summary>
    /// <param name="newPriorityLevel">New priority level (0.0 to 1.0)</param>
    /// <returns>Updated request with new priority or validation error</returns>
    public Result<InteractiveConstraintRequest, ValidationError> WithPriorityLevel(double newPriorityLevel)
    {
        var validatedPriority = ValidatePriorityLevel(newPriorityLevel);
        if (!validatedPriority.IsSuccess)
        {
            return Result<InteractiveConstraintRequest, ValidationError>.Failure(validatedPriority.Error);
        }

        return Result<InteractiveConstraintRequest, ValidationError>.Success(
            this with { PriorityLevel = Option<double>.Some(validatedPriority.Value) }
        );
    }

    /// <summary>
    /// Checks if this request represents a refinement of an existing constraint.
    /// Based on conversation history and context patterns.
    /// </summary>
    /// <returns>True if this appears to be a refinement request</returns>
    public bool IsRefinementRequest() =>
        Input.ContainsRefinementKeywords() || Context.Map(c => c.IsRefinementContext()).GetValueOrDefault(false);

    /// <summary>
    /// Gets the estimated complexity of this constraint request.
    /// Used for processing resource allocation and validation depth.
    /// </summary>
    /// <returns>Complexity score from 0.0 (simple) to 1.0 (complex)</returns>
    public double GetComplexityEstimate() =>
        (Input.GetComplexityScore() +
         Context.Map(c => c.GetComplexityScore()).GetValueOrDefault(0.0) +
         (PriorityLevel.Map(p => p > 0.8 ? 0.2 : 0.0).GetValueOrDefault(0.0))) / 3.0;

    private static Result<double, ValidationError> ValidatePriorityLevel(double priorityLevel)
    {
        if (priorityLevel < 0.0 || priorityLevel > 1.0)
        {
            return Result<double, ValidationError>.Failure(
                ValidationError.OutOfRange(nameof(PriorityLevel), 0.0, 1.0, priorityLevel)
            );
        }

        return Result<double, ValidationError>.Success(priorityLevel);
    }

    public override string ToString() =>
        $"InteractiveConstraintRequest(Id: {ConversationId}, Input: {Input.Summary}, " +
        $"HasContext: {Context.HasValue}, Priority: {PriorityLevel.Map(p => p.ToString("F2")).GetValueOrDefault("Not Set")})";
}
