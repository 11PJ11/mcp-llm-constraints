using System;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Feedback;

/// <summary>
/// Immutable representation of user feedback rating for a constraint.
/// Follows CUPID principles: Composable, Predictable, Idiomatic, Domain-based.
/// Uses Option<T> pattern to avoid nulls and ensure data integrity.
/// </summary>
public sealed record SimpleUserRating
{
    /// <summary>
    /// Gets the constraint being rated.
    /// </summary>
    public string ConstraintId { get; }

    /// <summary>
    /// Gets the user's rating value.
    /// </summary>
    public RatingValue Rating { get; }

    /// <summary>
    /// Gets when the rating was provided.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets optional user comment about the constraint.
    /// Uses Option<T> to avoid null and represent optional data explicitly.
    /// </summary>
    public Option<string> Comment { get; }

    /// <summary>
    /// Gets the session context when rating was provided.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Creates a new user rating with required fields.
    /// </summary>
    /// <param name="constraintId">The constraint being rated</param>
    /// <param name="rating">The rating value</param>
    /// <param name="timestamp">When the rating was provided</param>
    /// <param name="sessionId">Session context for the rating</param>
    /// <param name="comment">Optional user comment</param>
    /// <exception cref="ArgumentException">Thrown when constraintId or sessionId is invalid</exception>
    public SimpleUserRating(
        string constraintId,
        RatingValue rating,
        DateTimeOffset timestamp,
        string sessionId,
        Option<string>? comment = null)
    {
        if (string.IsNullOrWhiteSpace(constraintId))
        {
            throw new ArgumentException("Constraint ID cannot be null or empty", nameof(constraintId));
        }

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));
        }

        if (!Enum.IsDefined(typeof(RatingValue), rating))
        {
            throw new ArgumentException($"Invalid rating value: {rating}", nameof(rating));
        }

        ConstraintId = constraintId;
        Rating = rating;
        Timestamp = timestamp;
        SessionId = sessionId;
        Comment = comment ?? Option<string>.None();
    }

    /// <summary>
    /// Creates a rating with comment.
    /// </summary>
    /// <param name="constraintId">The constraint being rated</param>
    /// <param name="rating">The rating value</param>
    /// <param name="sessionId">Session context</param>
    /// <param name="comment">User comment about the constraint</param>
    /// <returns>New rating with comment</returns>
    public static SimpleUserRating WithComment(string constraintId, RatingValue rating, string sessionId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new ArgumentException("Comment cannot be empty when specified", nameof(comment));
        }

        return new SimpleUserRating(
            constraintId,
            rating,
            DateTimeOffset.UtcNow,
            sessionId,
            Option<string>.Some(comment));
    }

    /// <summary>
    /// Creates a rating without comment.
    /// </summary>
    /// <param name="constraintId">The constraint being rated</param>
    /// <param name="rating">The rating value</param>
    /// <param name="sessionId">Session context</param>
    /// <returns>New rating without comment</returns>
    public static SimpleUserRating WithoutComment(string constraintId, RatingValue rating, string sessionId)
    {
        return new SimpleUserRating(
            constraintId,
            rating,
            DateTimeOffset.UtcNow,
            sessionId,
            Option<string>.None());
    }

    /// <summary>
    /// Gets whether this rating is positive (thumbs up).
    /// </summary>
    public bool IsPositive => Rating == RatingValue.ThumbsUp;

    /// <summary>
    /// Gets whether this rating is negative (thumbs down).
    /// </summary>
    public bool IsNegative => Rating == RatingValue.ThumbsDown;

    /// <summary>
    /// Gets whether this rating is neutral.
    /// </summary>
    public bool IsNeutral => Rating == RatingValue.Neutral;

    /// <summary>
    /// Gets numeric value for calculations.
    /// </summary>
    public int NumericValue => (int)Rating;
}

/// <summary>
/// Rating values for constraint feedback.
/// Uses explicit values for clear numeric calculations.
/// </summary>
public enum RatingValue
{
    /// <summary>
    /// Thumbs down - constraint was not helpful.
    /// </summary>
    ThumbsDown = -1,

    /// <summary>
    /// Neutral - constraint was neither helpful nor unhelpful.
    /// </summary>
    Neutral = 0,

    /// <summary>
    /// Thumbs up - constraint was helpful.
    /// </summary>
    ThumbsUp = 1
}
