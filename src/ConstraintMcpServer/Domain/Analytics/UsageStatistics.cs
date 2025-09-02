using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Analytics;

/// <summary>
/// Immutable representation of usage statistics for constraints.
/// Provides simple analytics without complex algorithms or machine learning.
/// Follows CUPID principles with domain-focused language and predictable behavior.
/// </summary>
public sealed record UsageStatistics
{
    private const int RecentUsageDays = 7;
    private const int ActiveUsageThreshold = 5;
    private const int RarelyUsedMinimum = 1;
    private const int RarelyUsedMaximum = 2;
    private const int OccasionalUsageMinimum = 3;
    private const int OccasionalUsageMaximum = 10;
    private const int RegularUsageMinimum = 11;
    private const int RegularUsageMaximum = 50;
    /// <summary>
    /// Gets the constraint these statistics relate to.
    /// </summary>
    public string ConstraintId { get; }

    /// <summary>
    /// Gets the total number of times this constraint was activated.
    /// </summary>
    public int TotalActivations { get; }

    /// <summary>
    /// Gets the number of unique sessions where this constraint was activated.
    /// </summary>
    public int UniqueSessions { get; }

    /// <summary>
    /// Gets the average activations per session.
    /// </summary>
    public double AverageActivationsPerSession { get; }

    /// <summary>
    /// Gets when this constraint was first activated.
    /// </summary>
    public Option<DateTimeOffset> FirstActivation { get; }

    /// <summary>
    /// Gets when this constraint was last activated.
    /// </summary>
    public Option<DateTimeOffset> LastActivation { get; }

    /// <summary>
    /// Gets the time span between first and last activation.
    /// </summary>
    public Option<TimeSpan> UsageTimespan { get; }

    /// <summary>
    /// Gets the activation frequency category.
    /// </summary>
    public UsageFrequency Frequency { get; }

    /// <summary>
    /// Creates new usage statistics.
    /// </summary>
    /// <param name="constraintId">The constraint these statistics relate to</param>
    /// <param name="totalActivations">Total number of activations</param>
    /// <param name="uniqueSessions">Number of unique sessions</param>
    /// <param name="firstActivation">When first activated</param>
    /// <param name="lastActivation">When last activated</param>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
    public UsageStatistics(
        string constraintId,
        int totalActivations,
        int uniqueSessions,
        Option<DateTimeOffset> firstActivation,
        Option<DateTimeOffset> lastActivation)
    {
        if (string.IsNullOrWhiteSpace(constraintId))
            throw new ArgumentException("Constraint ID cannot be null or empty", nameof(constraintId));

        if (totalActivations < 0)
            throw new ArgumentException("Total activations cannot be negative", nameof(totalActivations));

        if (uniqueSessions < 0)
            throw new ArgumentException("Unique sessions cannot be negative", nameof(uniqueSessions));

        if (totalActivations > 0 && uniqueSessions == 0)
            throw new ArgumentException("Cannot have activations without sessions");

        ConstraintId = constraintId;
        TotalActivations = totalActivations;
        UniqueSessions = uniqueSessions;
        FirstActivation = firstActivation;
        LastActivation = lastActivation;
        
        AverageActivationsPerSession = uniqueSessions == 0 
            ? 0.0 
            : Math.Round((double)totalActivations / uniqueSessions, 2);

        UsageTimespan = firstActivation.HasValue && lastActivation.HasValue
            ? Option<TimeSpan>.Some(lastActivation.Value - firstActivation.Value)
            : Option<TimeSpan>.None();

        Frequency = DetermineFrequency(totalActivations);
    }

    /// <summary>
    /// Creates usage statistics from activation records.
    /// </summary>
    /// <param name="constraintId">The constraint being analyzed</param>
    /// <param name="activationRecords">The activation records to analyze</param>
    /// <returns>Usage statistics based on the activation records</returns>
    public static UsageStatistics FromActivationRecords(
        string constraintId, 
        IReadOnlyList<ConstraintActivationRecord> activationRecords)
    {
        if (activationRecords.Count == 0)
            return NoUsage(constraintId);

        var relevantRecords = activationRecords
            .Where(r => r.ConstraintId == constraintId)
            .ToList();

        if (relevantRecords.Count == 0)
            return NoUsage(constraintId);

        var totalActivations = relevantRecords.Count;
        var uniqueSessions = relevantRecords
            .Select(r => r.SessionId)
            .Distinct()
            .Count();

        var orderedRecords = relevantRecords.OrderBy(r => r.Timestamp).ToList();
        var firstActivation = Option<DateTimeOffset>.Some(orderedRecords.First().Timestamp);
        var lastActivation = Option<DateTimeOffset>.Some(orderedRecords.Last().Timestamp);

        return new UsageStatistics(
            constraintId,
            totalActivations,
            uniqueSessions,
            firstActivation,
            lastActivation);
    }

    /// <summary>
    /// Creates usage statistics for a constraint with no usage.
    /// </summary>
    /// <param name="constraintId">The constraint with no usage</param>
    /// <returns>Usage statistics indicating no usage</returns>
    public static UsageStatistics NoUsage(string constraintId)
    {
        return new UsageStatistics(
            constraintId,
            0,
            0,
            Option<DateTimeOffset>.None(),
            Option<DateTimeOffset>.None());
    }

    /// <summary>
    /// Gets whether this constraint has been used recently (within 7 days).
    /// </summary>
    public bool HasRecentUsage => LastActivation.HasValue && 
        (DateTimeOffset.UtcNow - LastActivation.Value).TotalDays <= RecentUsageDays;

    /// <summary>
    /// Gets whether this constraint is actively used (>= 5 activations).
    /// </summary>
    public bool IsActivelyUsed => TotalActivations >= ActiveUsageThreshold;

    /// <summary>
    /// Gets whether this constraint is rarely used (1-2 activations).
    /// </summary>
    public bool IsRarelyUsed => TotalActivations >= RarelyUsedMinimum && TotalActivations <= RarelyUsedMaximum;

    /// <summary>
    /// Gets whether this constraint has never been used.
    /// </summary>
    public bool IsNeverUsed => TotalActivations == 0;

    private static UsageFrequency DetermineFrequency(int totalActivations)
    {
        return totalActivations switch
        {
            <= 0 => UsageFrequency.Never,
            >= RarelyUsedMinimum and <= RarelyUsedMaximum => UsageFrequency.Rarely,
            >= OccasionalUsageMinimum and <= OccasionalUsageMaximum => UsageFrequency.Occasionally,
            >= RegularUsageMinimum and <= RegularUsageMaximum => UsageFrequency.Regularly,
            > RegularUsageMaximum => UsageFrequency.Frequently,
        };
    }
}

/// <summary>
/// Usage frequency categories for constraints.
/// </summary>
public enum UsageFrequency
{
    /// <summary>
    /// Never activated.
    /// </summary>
    Never,

    /// <summary>
    /// Rarely activated (1-2 times).
    /// </summary>
    Rarely,

    /// <summary>
    /// Occasionally activated (3-10 times).
    /// </summary>
    Occasionally,

    /// <summary>
    /// Regularly activated (11-50 times).
    /// </summary>
    Regularly,

    /// <summary>
    /// Frequently activated (>50 times).
    /// </summary>
    Frequently
}

/// <summary>
/// Represents a constraint activation record for analytics.
/// </summary>
/// <param name="ConstraintId">The constraint that was activated</param>
/// <param name="SessionId">The session where activation occurred</param>
/// <param name="Timestamp">When the activation occurred</param>
public readonly record struct ConstraintActivationRecord(
    string ConstraintId,
    string SessionId,
    DateTimeOffset Timestamp
);