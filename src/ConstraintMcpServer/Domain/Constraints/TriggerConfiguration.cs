using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Represents trigger configuration for context-aware constraint activation.
/// Replaces phase-based activation with universal trigger patterns.
/// </summary>
public sealed class TriggerConfiguration
{
    /// <summary>
    /// Keywords that activate this constraint when found in context.
    /// </summary>
    public IReadOnlyList<string> Keywords { get; init; } = Array.Empty<string>();

    /// <summary>
    /// File patterns that suggest this constraint is relevant (glob patterns).
    /// </summary>
    public IReadOnlyList<string> FilePatterns { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Context patterns based on development activity type.
    /// </summary>
    public IReadOnlyList<string> ContextPatterns { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Anti-patterns that prevent activation even if other triggers match.
    /// </summary>
    public IReadOnlyList<string> AntiPatterns { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Minimum confidence threshold for constraint activation (0.0 to 1.0).
    /// </summary>
    public double ConfidenceThreshold { get; init; } = 0.7;

    /// <summary>
    /// Creates a new trigger configuration with validation.
    /// </summary>
    public TriggerConfiguration()
    {
        // Default constructor for property initialization
    }

    /// <summary>
    /// Creates a new trigger configuration with specified parameters.
    /// </summary>
    /// <param name="keywords">Keywords that activate this constraint</param>
    /// <param name="filePatterns">File patterns that suggest relevance</param>
    /// <param name="contextPatterns">Context patterns for activity-based activation</param>
    /// <param name="antiPatterns">Anti-patterns that prevent activation</param>
    /// <param name="confidenceThreshold">Minimum confidence for activation</param>
    public TriggerConfiguration(
        IEnumerable<string>? keywords = null,
        IEnumerable<string>? filePatterns = null,
        IEnumerable<string>? contextPatterns = null,
        IEnumerable<string>? antiPatterns = null,
        double confidenceThreshold = 0.7)
    {
        if (confidenceThreshold < 0.0 || confidenceThreshold > 1.0)
        {
            throw new ValidationException("Confidence threshold must be between 0.0 and 1.0");
        }

        Keywords = keywords?.Where(k => !string.IsNullOrWhiteSpace(k)).ToList().AsReadOnly() 
                   ?? new List<string>().AsReadOnly();
        FilePatterns = filePatterns?.Where(p => !string.IsNullOrWhiteSpace(p)).ToList().AsReadOnly() 
                      ?? new List<string>().AsReadOnly();
        ContextPatterns = contextPatterns?.Where(c => !string.IsNullOrWhiteSpace(c)).ToList().AsReadOnly() 
                         ?? new List<string>().AsReadOnly();
        AntiPatterns = antiPatterns?.Where(a => !string.IsNullOrWhiteSpace(a)).ToList().AsReadOnly() 
                      ?? new List<string>().AsReadOnly();
        ConfidenceThreshold = confidenceThreshold;
    }

    /// <summary>
    /// Checks if this trigger configuration has any activation criteria.
    /// </summary>
    public bool HasActivationCriteria =>
        Keywords.Count > 0 || FilePatterns.Count > 0 || ContextPatterns.Count > 0;

    /// <summary>
    /// Returns a string representation of the trigger configuration.
    /// </summary>
    public override string ToString()
    {
        var criteria = new List<string>();
        
        if (Keywords.Count > 0)
            criteria.Add($"Keywords: {Keywords.Count}");
        if (FilePatterns.Count > 0)
            criteria.Add($"FilePatterns: {FilePatterns.Count}");
        if (ContextPatterns.Count > 0)
            criteria.Add($"ContextPatterns: {ContextPatterns.Count}");
        if (AntiPatterns.Count > 0)
            criteria.Add($"AntiPatterns: {AntiPatterns.Count}");

        return $"TriggerConfiguration({string.Join(", ", criteria)}, Threshold: {ConfidenceThreshold:F2})";
    }
}