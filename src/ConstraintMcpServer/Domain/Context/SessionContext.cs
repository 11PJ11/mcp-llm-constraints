using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Context;

/// <summary>
/// Maintains development session state across multiple tool calls.
/// Aggregate root for session-based constraint activation patterns.
/// Enables learning and adaptation based on development session patterns.
/// </summary>
public sealed class SessionContext
{
    private readonly List<ConstraintActivation> _activationHistory;
    private readonly Dictionary<string, int> _constraintActivationCounts;
    private readonly Dictionary<string, double> _contextTypeFrequencies;
    private DateTimeOffset _sessionStartTime;

    /// <summary>
    /// Unique session identifier.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Track constraint activation history across the session.
    /// Provides audit trail and learning data for pattern recognition.
    /// </summary>
    public IReadOnlyList<ConstraintActivation> ActivationHistory => _activationHistory.AsReadOnly();

    /// <summary>
    /// Detect activity patterns within session based on activation history.
    /// Provides insights into development workflow patterns.
    /// </summary>
    public string DetectedActivityPattern { get; private set; }

    /// <summary>
    /// Total number of tool calls in this session.
    /// </summary>
    public int TotalToolCalls { get; private set; }

    /// <summary>
    /// Session duration for pattern analysis.
    /// </summary>
    public TimeSpan SessionDuration => DateTimeOffset.UtcNow - _sessionStartTime;

    /// <summary>
    /// Dominant context type based on frequency analysis.
    /// </summary>
    public string DominantContextType { get; private set; }

    /// <summary>
    /// Creates a new session context.
    /// </summary>
    /// <param name="sessionId">Unique session identifier</param>
    /// <exception cref="ArgumentException">Thrown when sessionId is null or empty</exception>
    public SessionContext(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));
        }

        SessionId = sessionId;
        _activationHistory = new List<ConstraintActivation>();
        _constraintActivationCounts = new Dictionary<string, int>();
        _contextTypeFrequencies = new Dictionary<string, double>();
        _sessionStartTime = DateTimeOffset.UtcNow;
        DetectedActivityPattern = "unknown";
        DominantContextType = "unknown";
    }

    /// <summary>
    /// Record a constraint activation in the session history.
    /// Updates session patterns and analytics.
    /// </summary>
    /// <param name="activation">Constraint activation to record</param>
    /// <exception cref="ArgumentNullException">Thrown when activation is null</exception>
    public void RecordActivation(ConstraintActivation activation)
    {
        if (activation == null)
        {
            throw new ArgumentNullException(nameof(activation));
        }

        _activationHistory.Add(activation);

        // Update activation counts
        var constraintId = activation.ConstraintId;
        _constraintActivationCounts[constraintId] = _constraintActivationCounts.GetValueOrDefault(constraintId) + 1;

        // Update context type frequencies
        var contextType = activation.TriggerContext.ContextType ?? "unknown";
        _contextTypeFrequencies[contextType] = _contextTypeFrequencies.GetValueOrDefault(contextType) + 1.0;

        // Update derived patterns
        UpdateActivityPattern();
        UpdateDominantContextType();
    }

    /// <summary>
    /// Record a tool call in the session.
    /// Updates session statistics for pattern analysis.
    /// </summary>
    public void RecordToolCall()
    {
        TotalToolCalls++;
    }

    /// <summary>
    /// Calculate session-based relevance adjustments for constraint selection.
    /// Provides learning-based scoring modifications based on session patterns.
    /// </summary>
    /// <param name="constraintId">Constraint to calculate adjustment for</param>
    /// <returns>Relevance adjustment factor (0.0 to 2.0, where 1.0 = no adjustment)</returns>
    public double GetSessionRelevanceAdjustment(string constraintId)
    {
        if (string.IsNullOrEmpty(constraintId))
        {
            return 1.0; // No adjustment for invalid input
        }

        // Base adjustment factor
        var adjustmentFactor = 1.0;

        // Boost constraints that have been successful in this session
        if (_constraintActivationCounts.TryGetValue(constraintId, out var activationCount) && activationCount > 0)
        {
            // Slight boost for previously activated constraints (indicates relevance)
            adjustmentFactor += 0.1 * Math.Min(activationCount, 3); // Max boost of 0.3
        }

        // Context-based adjustments based on session patterns
        if (DetectedActivityPattern != "unknown")
        {
            adjustmentFactor += GetPatternBasedAdjustment(constraintId, DetectedActivityPattern);
        }

        // Session length adjustments - longer sessions may need different constraint emphasis
        var sessionMinutes = SessionDuration.TotalMinutes;
        if (sessionMinutes > 60) // Long sessions
        {
            // Slightly reduce adjustment for very active constraints to encourage variety
            if (activationCount > 5)
            {
                adjustmentFactor *= 0.95;
            }
        }

        return Math.Max(0.5, Math.Min(2.0, adjustmentFactor)); // Clamp to reasonable range
    }

    /// <summary>
    /// Get session-based insights for debugging and analysis.
    /// </summary>
    /// <returns>Session analysis summary</returns>
    public SessionAnalytics GetSessionAnalytics()
    {
        return new SessionAnalytics(
            SessionId: SessionId,
            Duration: SessionDuration,
            TotalToolCalls: TotalToolCalls,
            TotalActivations: _activationHistory.Count,
            DominantContextType: DominantContextType,
            DetectedPattern: DetectedActivityPattern,
            MostActivatedConstraint: GetMostActivatedConstraint(),
            ContextTypeBreakdown: _contextTypeFrequencies.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        );
    }

    private void UpdateActivityPattern()
    {
        if (_activationHistory.Count < 2)
        {
            DetectedActivityPattern = "startup";
            return;
        }

        var recentActivations = _activationHistory.TakeLast(5).ToList();
        var contextTypes = recentActivations.Select(a => a.TriggerContext.ContextType).ToList();

        // Analyze patterns in recent activations
        if (contextTypes.All(ct => ct == "testing"))
        {
            DetectedActivityPattern = "test-driven";
        }
        else if (contextTypes.All(ct => ct == "feature_development"))
        {
            DetectedActivityPattern = "feature-focused";
        }
        else if (contextTypes.All(ct => ct == "refactoring"))
        {
            DetectedActivityPattern = "refactoring-session";
        }
        else if (contextTypes.Distinct().Count() >= 3)
        {
            DetectedActivityPattern = "mixed-development";
        }
        else if (contextTypes.Contains("feature_development") && contextTypes.Contains("testing"))
        {
            DetectedActivityPattern = "tdd-workflow";
        }
        else
        {
            DetectedActivityPattern = "exploratory";
        }
    }

    private void UpdateDominantContextType()
    {
        if (_contextTypeFrequencies.Count == 0)
        {
            DominantContextType = "unknown";
            return;
        }

        var totalActivations = _contextTypeFrequencies.Values.Sum();
        var dominant = _contextTypeFrequencies
            .OrderByDescending(kvp => kvp.Value)
            .First();

        // Only set as dominant if it represents at least 40% of activations
        if (dominant.Value / totalActivations >= 0.4)
        {
            DominantContextType = dominant.Key;
        }
        else
        {
            DominantContextType = "mixed";
        }
    }

    private double GetPatternBasedAdjustment(string constraintId, string pattern)
    {
        return pattern switch
        {
            "test-driven" when constraintId.Contains("tdd", StringComparison.OrdinalIgnoreCase) => 0.2,
            "feature-focused" when constraintId.Contains("feature", StringComparison.OrdinalIgnoreCase) => 0.15,
            "refactoring-session" when constraintId.Contains("refactor", StringComparison.OrdinalIgnoreCase) => 0.15,
            "tdd-workflow" when constraintId.Contains("tdd", StringComparison.OrdinalIgnoreCase) => 0.25,
            _ => 0.0
        };
    }

    private string GetMostActivatedConstraint()
    {
        if (_constraintActivationCounts.Count == 0)
        {
            return "none";
        }

        return _constraintActivationCounts
            .OrderByDescending(kvp => kvp.Value)
            .First()
            .Key;
    }

    /// <summary>
    /// Reset session state for new development session.
    /// Maintains session ID but clears history and patterns.
    /// </summary>
    public void Reset()
    {
        _activationHistory.Clear();
        _constraintActivationCounts.Clear();
        _contextTypeFrequencies.Clear();
        _sessionStartTime = DateTimeOffset.UtcNow;
        TotalToolCalls = 0;
        DetectedActivityPattern = "unknown";
        DominantContextType = "unknown";
    }
}

/// <summary>
/// Session analytics data for debugging and analysis.
/// </summary>
/// <param name="SessionId">Session identifier</param>
/// <param name="Duration">Session duration</param>
/// <param name="TotalToolCalls">Total tool calls made</param>
/// <param name="TotalActivations">Total constraint activations</param>
/// <param name="DominantContextType">Most frequent context type</param>
/// <param name="DetectedPattern">Detected development pattern</param>
/// <param name="MostActivatedConstraint">Most frequently activated constraint</param>
/// <param name="ContextTypeBreakdown">Breakdown of context types</param>
public readonly record struct SessionAnalytics(
    string SessionId,
    TimeSpan Duration,
    int TotalToolCalls,
    int TotalActivations,
    string DominantContextType,
    string DetectedPattern,
    string MostActivatedConstraint,
    Dictionary<string, double> ContextTypeBreakdown
);
