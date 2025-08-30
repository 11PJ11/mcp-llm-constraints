using System;
using System.Text.RegularExpressions;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Conversation;

/// <summary>
/// Immutable value object representing a unique conversation identifier.
/// Enforces format validation and provides type safety for conversation tracking.
/// Implements CUPID properties: Predictable, Idiomatic, Domain-based.
/// </summary>
public sealed partial record ConversationId
{
    private static readonly Regex ValidIdPattern = CreateValidIdRegex();

    /// <summary>
    /// Gets the raw identifier value.
    /// </summary>
    public string Value { get; }

    private ConversationId(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a conversation ID with validation.
    /// Ensures the ID follows expected format and constraints.
    /// </summary>
    /// <param name="value">The identifier value</param>
    /// <returns>Valid conversation ID or validation error</returns>
    public static Result<ConversationId, ValidationError> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<ConversationId, ValidationError>.Failure(
                ValidationError.MissingRequiredField(nameof(value))
            );
        }

        if (value.Length > 100)
        {
            return Result<ConversationId, ValidationError>.Failure(
                ValidationError.OutOfRange(nameof(value), 1, 100, value.Length)
            );
        }

        if (!ValidIdPattern.IsMatch(value))
        {
            return Result<ConversationId, ValidationError>.Failure(
                ValidationError.InvalidFormat(
                    nameof(value),
                    "alphanumeric characters, hyphens, and underscores only",
                    value
                )
            );
        }

        return Result<ConversationId, ValidationError>.Success(new ConversationId(value));
    }

    /// <summary>
    /// Generates a new unique conversation ID using current timestamp and random component.
    /// Provides collision-resistant identifiers for new conversations.
    /// </summary>
    /// <returns>A new unique conversation ID</returns>
    public static ConversationId Generate()
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        var random = Guid.NewGuid().ToString("N")[..8]; // First 8 characters of GUID
        var value = $"conv-{timestamp}-{random}";

        // This should never fail since we control the format, but use Create for consistency
        var result = Create(value);
        return result.IsSuccess ? result.Value : throw new InvalidOperationException("Generated ID should be valid");
    }

    /// <summary>
    /// Creates a session-specific conversation ID based on MCP session identifier.
    /// Enables correlation between MCP sessions and conversation contexts.
    /// </summary>
    /// <param name="sessionId">MCP session identifier</param>
    /// <returns>Conversation ID derived from session or validation error</returns>
    public static Result<ConversationId, ValidationError> FromSessionId(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return Create(Generate().Value);
        }

        // Sanitize session ID to valid conversation ID format
        var sanitized = Regex.Replace(sessionId, @"[^a-zA-Z0-9\-_]", "-");
        if (sanitized.Length > 100)
        {
            sanitized = sanitized[..100];
        }

        // Ensure it doesn't start or end with separator
        sanitized = sanitized.Trim('-', '_');
        if (string.IsNullOrEmpty(sanitized))
        {
            return Create(Generate().Value);
        }

        return Create($"session-{sanitized}");
    }

    /// <summary>
    /// Checks if this conversation ID appears to be session-derived.
    /// Used for determining conversation lifecycle management.
    /// </summary>
    /// <returns>True if ID appears to be derived from MCP session</returns>
    public bool IsSessionDerived() => Value.StartsWith("session-", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if this conversation ID appears to be auto-generated.
    /// Used for conversation tracking and analytics.
    /// </summary>
    /// <returns>True if ID appears to be auto-generated</returns>
    public bool IsGenerated() => Value.StartsWith("conv-", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(@"^[a-zA-Z0-9\-_]+$", RegexOptions.Compiled)]
    private static partial Regex CreateValidIdRegex();

    /// <summary>
    /// Implicit conversion from string for convenience.
    /// Note: This bypasses validation, use Create() for validation.
    /// </summary>
    public static implicit operator string(ConversationId conversationId) => conversationId.Value;

    public override string ToString() => Value;
}
