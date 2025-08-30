using System;
using System.Collections.Immutable;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Conversation;

/// <summary>
/// Immutable aggregate root representing a conversation session state.
/// Tracks conversation history and progression for context-aware constraint definition.
/// Implements CUPID properties: Composable, Predictable, Domain-based.
/// </summary>
public sealed record ConversationSession
{
    /// <summary>
    /// Gets the unique identifier for this conversation session.
    /// </summary>
    public ConversationId ConversationId { get; init; }

    /// <summary>
    /// Gets when this conversation session started.
    /// </summary>
    public RequestTimestamp StartTime { get; init; }

    /// <summary>
    /// Gets when this session was last updated.
    /// </summary>
    public RequestTimestamp LastUpdated { get; init; }

    /// <summary>
    /// Gets the current conversation phase.
    /// </summary>
    public ConversationPhase CurrentPhase { get; init; }

    /// <summary>
    /// Gets the history of constraint definitions created in this session.
    /// </summary>
    public ImmutableList<string> ConstraintDefinitionHistory { get; init; }

    /// <summary>
    /// Gets the user interaction count in this session.
    /// </summary>
    public int InteractionCount { get; init; }

    /// <summary>
    /// Gets the accumulated context from previous interactions.
    /// </summary>
    public Option<ConstraintContext> AccumulatedContext { get; init; }

    /// <summary>
    /// Gets session-specific preferences and settings.
    /// </summary>
    public ImmutableDictionary<string, object> SessionPreferences { get; init; }

    private ConversationSession(
        ConversationId conversationId,
        RequestTimestamp startTime,
        RequestTimestamp lastUpdated,
        ConversationPhase currentPhase,
        ImmutableList<string> constraintDefinitionHistory,
        int interactionCount,
        Option<ConstraintContext> accumulatedContext,
        ImmutableDictionary<string, object> sessionPreferences)
    {
        ConversationId = conversationId;
        StartTime = startTime;
        LastUpdated = lastUpdated;
        CurrentPhase = currentPhase;
        ConstraintDefinitionHistory = constraintDefinitionHistory;
        InteractionCount = interactionCount;
        AccumulatedContext = accumulatedContext;
        SessionPreferences = sessionPreferences;
    }

    /// <summary>
    /// Starts a new conversation session with the given identifier.
    /// </summary>
    /// <param name="conversationId">Unique conversation identifier</param>
    /// <returns>New conversation session</returns>
    public static ConversationSession StartNew(ConversationId conversationId)
    {
        var now = RequestTimestamp.Now();
        return new ConversationSession(
            conversationId,
            now,
            now,
            ConversationPhase.InitialDefinition,
            ImmutableList<string>.Empty,
            0,
            Option<ConstraintContext>.None(),
            ImmutableDictionary<string, object>.Empty
        );
    }

    /// <summary>
    /// Records a new user interaction and advances session state.
    /// </summary>
    /// <param name="constraintId">ID of constraint created (if any)</param>
    /// <param name="context">Context from this interaction (if any)</param>
    /// <returns>Updated session state</returns>
    public ConversationSession RecordInteraction(string? constraintId = null, ConstraintContext? context = null)
    {
        var updatedHistory = constraintId is not null
            ? ConstraintDefinitionHistory.Add(constraintId)
            : ConstraintDefinitionHistory;

        var updatedContext = context is not null
            ? Option<ConstraintContext>.Some(MergeContext(context))
            : AccumulatedContext;

        var newPhase = DetermineNextPhase(constraintId is not null);

        return this with
        {
            LastUpdated = RequestTimestamp.Now(),
            CurrentPhase = newPhase,
            ConstraintDefinitionHistory = updatedHistory,
            InteractionCount = InteractionCount + 1,
            AccumulatedContext = updatedContext
        };
    }

    /// <summary>
    /// Updates session preferences with new settings.
    /// </summary>
    /// <param name="key">Preference key</param>
    /// <param name="value">Preference value</param>
    /// <returns>Updated session with new preference</returns>
    public ConversationSession WithPreference(string key, object value) =>
        this with
        {
            SessionPreferences = SessionPreferences.SetItem(key, value),
            LastUpdated = RequestTimestamp.Now()
        };

    /// <summary>
    /// Advances the conversation to the refinement phase.
    /// Used when user wants to improve existing constraint definitions.
    /// </summary>
    /// <returns>Session in refinement phase</returns>
    public ConversationSession StartRefinementPhase() =>
        this with
        {
            CurrentPhase = ConversationPhase.Refinement,
            LastUpdated = RequestTimestamp.Now()
        };

    /// <summary>
    /// Advances the conversation to the validation phase.
    /// Used when user wants to validate and finalize constraint definitions.
    /// </summary>
    /// <returns>Session in validation phase</returns>
    public ConversationSession StartValidationPhase() =>
        this with
        {
            CurrentPhase = ConversationPhase.Validation,
            LastUpdated = RequestTimestamp.Now()
        };

    /// <summary>
    /// Checks if this session has created any constraint definitions.
    /// </summary>
    /// <returns>True if constraints have been defined in this session</returns>
    public bool HasPreviousConstraintDefinitions() =>
        ConstraintDefinitionHistory.Count > 0;

    /// <summary>
    /// Checks if the session is in an active state (recent interaction).
    /// </summary>
    /// <returns>True if session had recent activity</returns>
    public bool IsActive() =>
        LastUpdated.IsWithinAge(TimeSpan.FromMinutes(30));

    /// <summary>
    /// Gets the age of this conversation session.
    /// </summary>
    /// <returns>Time elapsed since session started</returns>
    public TimeSpan GetSessionAge() => StartTime.Age;

    /// <summary>
    /// Gets the complexity score based on session characteristics.
    /// Used for processing resource allocation.
    /// </summary>
    /// <returns>Complexity score from 0.0 (simple) to 1.0 (complex)</returns>
    public double GetComplexityScore()
    {
        var score = 0.0;

        // Interaction complexity
        score += Math.Min(0.4, InteractionCount / 20.0);

        // Context complexity
        score += AccumulatedContext.Map(c => c.GetComplexityScore() * 0.3).GetValueOrDefault(0.0);

        // Phase complexity
        score += CurrentPhase switch
        {
            ConversationPhase.InitialDefinition => 0.1,
            ConversationPhase.Refinement => 0.2,
            ConversationPhase.Validation => 0.15,
            ConversationPhase.Completed => 0.05,
            _ => 0.1
        };

        // History complexity
        score += Math.Min(0.2, ConstraintDefinitionHistory.Count / 10.0);

        return Math.Min(1.0, score);
    }

    /// <summary>
    /// Gets a preference value by key with type conversion.
    /// </summary>
    /// <typeparam name="T">Expected preference type</typeparam>
    /// <param name="key">Preference key</param>
    /// <returns>Preference value if found and convertible</returns>
    public Option<T> GetPreference<T>(string key)
    {
        if (!SessionPreferences.TryGetValue(key, out var value))
        {
            return Option<T>.None();
        }

        try
        {
            return value is T typedValue
                ? Option<T>.Some(typedValue)
                : Option<T>.None();
        }
        catch
        {
            return Option<T>.None();
        }
    }

    private ConstraintContext MergeContext(ConstraintContext newContext) =>
        AccumulatedContext.Match(
            existing => existing, // TODO: Implement context merging logic
            () => newContext
        );

    private ConversationPhase DetermineNextPhase(bool constraintCreated)
    {
        return CurrentPhase switch
        {
            ConversationPhase.InitialDefinition when constraintCreated => ConversationPhase.Refinement,
            ConversationPhase.Refinement when InteractionCount > 5 => ConversationPhase.Validation,
            ConversationPhase.Validation when constraintCreated => ConversationPhase.Completed,
            _ => CurrentPhase
        };
    }

    public override string ToString() =>
        $"ConversationSession(Id: {ConversationId}, Phase: {CurrentPhase}, " +
        $"Interactions: {InteractionCount}, Constraints: {ConstraintDefinitionHistory.Count})";
}

/// <summary>
/// Defines the phases of a conversation session for constraint definition.
/// </summary>
public enum ConversationPhase
{
    /// <summary>
    /// Initial constraint definition phase.
    /// </summary>
    InitialDefinition,

    /// <summary>
    /// Constraint refinement and improvement phase.
    /// </summary>
    Refinement,

    /// <summary>
    /// Constraint validation and finalization phase.
    /// </summary>
    Validation,

    /// <summary>
    /// Conversation completed successfully.
    /// </summary>
    Completed
}
