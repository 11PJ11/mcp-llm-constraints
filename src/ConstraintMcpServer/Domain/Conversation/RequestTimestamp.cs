using System;

namespace ConstraintMcpServer.Domain.Conversation;

/// <summary>
/// Immutable value object representing a timestamp for constraint requests.
/// Provides consistent time handling and comparison operations.
/// Implements CUPID properties: Predictable, Idiomatic.
/// </summary>
public sealed record RequestTimestamp : IComparable<RequestTimestamp>
{
    /// <summary>
    /// Gets the UTC timestamp value.
    /// </summary>
    public DateTimeOffset UtcTimestamp { get; }

    /// <summary>
    /// Gets the timestamp as Unix milliseconds for serialization.
    /// </summary>
    public long UnixMilliseconds => UtcTimestamp.ToUnixTimeMilliseconds();

    private RequestTimestamp(DateTimeOffset timestamp)
    {
        UtcTimestamp = timestamp.ToUniversalTime();
    }

    /// <summary>
    /// Creates a timestamp representing the current moment.
    /// </summary>
    /// <returns>Current timestamp</returns>
    public static RequestTimestamp Now() => new(DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a timestamp from a specific DateTimeOffset.
    /// </summary>
    /// <param name="timestamp">The timestamp value</param>
    /// <returns>Request timestamp</returns>
    public static RequestTimestamp FromDateTimeOffset(DateTimeOffset timestamp) =>
        new(timestamp);

    /// <summary>
    /// Creates a timestamp from Unix milliseconds.
    /// Useful for deserialization from JSON or other formats.
    /// </summary>
    /// <param name="unixMilliseconds">Unix timestamp in milliseconds</param>
    /// <returns>Request timestamp</returns>
    public static RequestTimestamp FromUnixMilliseconds(long unixMilliseconds) =>
        new(DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds));

    /// <summary>
    /// Calculates the age of this timestamp relative to current time.
    /// </summary>
    /// <returns>Time elapsed since this timestamp</returns>
    public TimeSpan Age => DateTimeOffset.UtcNow - UtcTimestamp;

    /// <summary>
    /// Checks if this timestamp is within the specified age threshold.
    /// Used for request freshness validation.
    /// </summary>
    /// <param name="maxAge">Maximum acceptable age</param>
    /// <returns>True if timestamp is fresh enough</returns>
    public bool IsWithinAge(TimeSpan maxAge) => Age <= maxAge;

    /// <summary>
    /// Checks if this timestamp is recent (within last 5 minutes).
    /// </summary>
    /// <returns>True if timestamp is recent</returns>
    public bool IsRecent() => IsWithinAge(TimeSpan.FromMinutes(5));

    /// <summary>
    /// Formats the timestamp for display purposes.
    /// </summary>
    /// <param name="format">Optional format string (defaults to ISO 8601)</param>
    /// <returns>Formatted timestamp string</returns>
    public string ToString(string? format = null) =>
        format is not null
            ? UtcTimestamp.ToString(format)
            : UtcTimestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

    /// <summary>
    /// Gets a human-readable relative time description.
    /// </summary>
    /// <returns>Relative time string (e.g., "2 minutes ago")</returns>
    public string GetRelativeTimeDescription()
    {
        var age = Age;

        return age.TotalSeconds switch
        {
            < 60 => "just now",
            < 3600 => $"{(int)age.TotalMinutes} minute{((int)age.TotalMinutes == 1 ? "" : "s")} ago",
            < 86400 => $"{(int)age.TotalHours} hour{((int)age.TotalHours == 1 ? "" : "s")} ago",
            < 2592000 => $"{(int)age.TotalDays} day{((int)age.TotalDays == 1 ? "" : "s")} ago",
            _ => UtcTimestamp.ToString("yyyy-MM-dd")
        };
    }

    /// <summary>
    /// Compares this timestamp with another for ordering.
    /// </summary>
    /// <param name="other">Other timestamp to compare</param>
    /// <returns>Comparison result</returns>
    public int CompareTo(RequestTimestamp? other)
    {
        if (other is null)
        {
            return 1;
        }

        return UtcTimestamp.CompareTo(other.UtcTimestamp);
    }

    /// <summary>
    /// Implicit conversion to DateTimeOffset for convenience.
    /// </summary>
    public static implicit operator DateTimeOffset(RequestTimestamp timestamp) =>
        timestamp.UtcTimestamp;

    /// <summary>
    /// Implicit conversion from DateTimeOffset for convenience.
    /// </summary>
    public static implicit operator RequestTimestamp(DateTimeOffset timestamp) =>
        FromDateTimeOffset(timestamp);

    public override string ToString() => ToString();
}
