namespace ConstraintMcpServer.Domain.Context;

/// <summary>
/// Represents the reason why a constraint was activated.
/// Provides business context for constraint activation decisions.
/// </summary>
public enum ActivationReason
{
    /// <summary>
    /// Unknown activation reason (default/fallback).
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Activated due to keyword matching in user context.
    /// </summary>
    KeywordMatch = 1,

    /// <summary>
    /// Activated due to file pattern matching.
    /// </summary>
    FilePatternMatch = 2,

    /// <summary>
    /// Activated due to development context pattern matching.
    /// </summary>
    ContextPatternMatch = 3,

    /// <summary>
    /// Activated due to combination of multiple factors.
    /// </summary>
    CombinedFactors = 4,

    /// <summary>
    /// Activated due to session-based activity pattern recognition.
    /// </summary>
    SessionPattern = 5,

    /// <summary>
    /// Activated due to methodology workflow progression.
    /// </summary>
    WorkflowProgression = 6,

    /// <summary>
    /// Activated due to high-confidence user intent detection.
    /// </summary>
    HighConfidenceIntent = 7,

    /// <summary>
    /// Activated as part of constraint composition or hierarchy.
    /// </summary>
    CompositionMember = 8,

    /// <summary>
    /// Manually activated or forced activation.
    /// </summary>
    ManualActivation = 9
}

/// <summary>
/// Extension methods for ActivationReason enum.
/// </summary>
public static class ActivationReasonExtensions
{
    /// <summary>
    /// Gets a human-readable description of the activation reason.
    /// </summary>
    /// <param name="reason">Activation reason</param>
    /// <returns>Human-readable description</returns>
    public static string GetDescription(this ActivationReason reason)
    {
        return reason switch
        {
            ActivationReason.Unknown => "Unknown reason",
            ActivationReason.KeywordMatch => "Keywords matched development context",
            ActivationReason.FilePatternMatch => "File pattern indicated relevance",
            ActivationReason.ContextPatternMatch => "Development activity pattern matched",
            ActivationReason.CombinedFactors => "Multiple factors indicated relevance",
            ActivationReason.SessionPattern => "Session activity pattern recognized",
            ActivationReason.WorkflowProgression => "Part of methodology workflow",
            ActivationReason.HighConfidenceIntent => "High-confidence user intent detected",
            ActivationReason.CompositionMember => "Member of constraint composition",
            ActivationReason.ManualActivation => "Manually activated",
            _ => "Unknown reason"
        };
    }

    /// <summary>
    /// Determines if this reason indicates high confidence activation.
    /// </summary>
    /// <param name="reason">Activation reason</param>
    /// <returns>True if reason indicates high confidence</returns>
    public static bool IsHighConfidence(this ActivationReason reason)
    {
        return reason switch
        {
            ActivationReason.HighConfidenceIntent => true,
            ActivationReason.CombinedFactors => true,
            ActivationReason.WorkflowProgression => true,
            ActivationReason.ManualActivation => true,
            _ => false
        };
    }
}
