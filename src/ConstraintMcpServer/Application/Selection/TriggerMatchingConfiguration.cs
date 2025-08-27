using System;

namespace ConstraintMcpServer.Application.Selection;

/// <summary>
/// Configuration settings for the trigger matching engine.
/// Controls confidence thresholds, matching behavior, and performance parameters.
/// TEMPORARY: Moved to Selection namespace due to compilation issues in Triggers namespace.
/// </summary>
public sealed class TriggerMatchingConfiguration
{
    /// <summary>
    /// Standard confidence threshold for production constraint activation.
    /// </summary>
    public const double StandardConfidenceThreshold = 0.7;

    /// <summary>
    /// Relaxed confidence threshold for broader constraint matching.
    /// </summary>
    public const double RelaxedConfidenceThreshold = 0.6;

    /// <summary>
    /// Strict confidence threshold for high-precision constraint matching.
    /// </summary>
    public const double StrictConfidenceThreshold = 0.8;

    /// <summary>
    /// Default confidence threshold for constraint activation.
    /// Only constraints meeting this threshold will be activated.
    /// </summary>
    public double DefaultConfidenceThreshold { get; init; } = StandardConfidenceThreshold;

    /// <summary>
    /// Maximum number of constraints to activate simultaneously.
    /// Prevents cognitive overload by limiting active constraints.
    /// </summary>
    public int MaxActiveConstraints { get; init; } = 5;

    /// <summary>
    /// Weight given to keyword matches in relevance calculations.
    /// Should sum to 1.0 with other weights for normalized scoring.
    /// </summary>
    public double KeywordMatchWeight { get; init; } = 0.4;

    /// <summary>
    /// Weight given to file pattern matches in relevance calculations.
    /// </summary>
    public double FilePatternMatchWeight { get; init; } = 0.3;

    /// <summary>
    /// Weight given to context pattern matches in relevance calculations.
    /// </summary>
    public double ContextPatternMatchWeight { get; init; } = 0.3;

    /// <summary>
    /// Enables fuzzy matching for keywords and patterns.
    /// Allows for spelling variations and synonyms.
    /// </summary>
    public bool EnableFuzzyMatching { get; init; } = false;

    /// <summary>
    /// Minimum keyword match ratio required for fuzzy matching.
    /// Only used when fuzzy matching is enabled.
    /// </summary>
    public double FuzzyMatchThreshold { get; init; } = 0.8;

    /// <summary>
    /// Enables session-based learning and pattern recognition.
    /// Improves accuracy over time based on user behavior.
    /// </summary>
    public bool EnableSessionLearning { get; init; } = false;

    /// <summary>
    /// Maximum time to spend on constraint evaluation (in milliseconds).
    /// Ensures performance budget compliance (<50ms requirement).
    /// </summary>
    public int MaxEvaluationTimeMs { get; init; } = 45;

    /// <summary>
    /// Creates a new trigger matching configuration with validation.
    /// </summary>
    public TriggerMatchingConfiguration()
    {
        ValidateConfiguration();
    }

    /// <summary>
    /// Creates a configuration with custom parameters.
    /// </summary>
    public TriggerMatchingConfiguration(
        double defaultConfidenceThreshold = StandardConfidenceThreshold,
        int maxActiveConstraints = 5,
        double keywordMatchWeight = 0.4,
        double filePatternMatchWeight = 0.3,
        double contextPatternMatchWeight = 0.3,
        bool enableFuzzyMatching = false,
        double fuzzyMatchThreshold = 0.8,
        bool enableSessionLearning = false,
        int maxEvaluationTimeMs = 45)
    {
        DefaultConfidenceThreshold = defaultConfidenceThreshold;
        MaxActiveConstraints = maxActiveConstraints;
        KeywordMatchWeight = keywordMatchWeight;
        FilePatternMatchWeight = filePatternMatchWeight;
        ContextPatternMatchWeight = contextPatternMatchWeight;
        EnableFuzzyMatching = enableFuzzyMatching;
        FuzzyMatchThreshold = fuzzyMatchThreshold;
        EnableSessionLearning = enableSessionLearning;
        MaxEvaluationTimeMs = maxEvaluationTimeMs;

        ValidateConfiguration();
    }

    /// <summary>
    /// Validates that configuration parameters are within acceptable ranges.
    /// </summary>
    private void ValidateConfiguration()
    {
        if (DefaultConfidenceThreshold < 0.0 || DefaultConfidenceThreshold > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(DefaultConfidenceThreshold),
                "Default confidence threshold must be between 0.0 and 1.0");
        }

        if (MaxActiveConstraints < 1 || MaxActiveConstraints > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxActiveConstraints),
                "Max active constraints must be between 1 and 20");
        }

        var totalWeight = KeywordMatchWeight + FilePatternMatchWeight + ContextPatternMatchWeight;
        if (Math.Abs(totalWeight - 1.0) > 0.001)
        {
            throw new ArgumentException("Match weights must sum to approximately 1.0");
        }

        if (FuzzyMatchThreshold < 0.0 || FuzzyMatchThreshold > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(FuzzyMatchThreshold),
                "Fuzzy match threshold must be between 0.0 and 1.0");
        }

        if (MaxEvaluationTimeMs < 1 || MaxEvaluationTimeMs > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxEvaluationTimeMs),
                "Max evaluation time must be between 1 and 1000 milliseconds");
        }
    }

    /// <summary>
    /// Creates a configuration optimized for high performance scenarios.
    /// Reduces complexity and disables optional features for speed.
    /// </summary>
    /// <returns>High performance configuration</returns>
    public static TriggerMatchingConfiguration CreateHighPerformance()
    {
        return new TriggerMatchingConfiguration(
            defaultConfidenceThreshold: StrictConfidenceThreshold, // Higher threshold for fewer evaluations
            maxActiveConstraints: 3,          // Fewer constraints to process
            enableFuzzyMatching: false,       // Disable expensive fuzzy matching
            enableSessionLearning: false,     // Disable learning overhead
            maxEvaluationTimeMs: 30           // Stricter time limit
        );
    }

    /// <summary>
    /// Creates a configuration optimized for accuracy and learning.
    /// Enables all features for maximum constraint matching precision.
    /// </summary>
    /// <returns>High accuracy configuration</returns>
    public static TriggerMatchingConfiguration CreateHighAccuracy()
    {
        return new TriggerMatchingConfiguration(
            defaultConfidenceThreshold: RelaxedConfidenceThreshold, // Lower threshold for more matches
            maxActiveConstraints: 8,          // More constraints for comprehensive coverage
            enableFuzzyMatching: true,        // Enable fuzzy matching for flexibility
            fuzzyMatchThreshold: 0.7,         // Lower threshold for fuzzy matches
            enableSessionLearning: true,      // Enable learning for accuracy improvement
            maxEvaluationTimeMs: 50           // Allow more time for thorough evaluation
        );
    }

    public override string ToString()
    {
        return $"TriggerMatchingConfiguration(Threshold: {DefaultConfidenceThreshold:F2}, " +
               $"MaxConstraints: {MaxActiveConstraints}, " +
               $"Weights: K{KeywordMatchWeight:F1}/F{FilePatternMatchWeight:F1}/C{ContextPatternMatchWeight:F1}, " +
               $"Fuzzy: {EnableFuzzyMatching}, Learning: {EnableSessionLearning})";
    }
}
