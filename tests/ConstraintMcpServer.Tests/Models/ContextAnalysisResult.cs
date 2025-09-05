using System;

namespace ConstraintMcpServer.Tests.Models;

/// <summary>
/// Result of context analysis performed by the domain layer.
/// Contains structured information about the development context
/// that enables intelligent constraint activation.
/// </summary>
public sealed class ContextAnalysisResult
{
    /// <summary>
    /// The original context description that was analyzed.
    /// </summary>
    public string OriginalContext { get; init; } = string.Empty;

    /// <summary>
    /// Keywords extracted from the context analysis.
    /// These drive constraint activation decisions.
    /// </summary>
    public string[] AnalyzedKeywords { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Confidence score of the context analysis (0.0 to 1.0).
    /// Higher scores indicate more certain context identification.
    /// </summary>
    public double ConfidenceScore { get; init; }

    /// <summary>
    /// Indicates if the context shows TDD development indicators.
    /// Used for TDD constraint activation.
    /// </summary>
    public bool HasTddIndicators { get; init; }

    /// <summary>
    /// Indicates if the context shows refactoring indicators.
    /// Used for refactoring constraint activation.
    /// </summary>
    public bool HasRefactoringIndicators { get; init; }

    /// <summary>
    /// Indicates if the context shows architecture work indicators.
    /// Used for architecture constraint activation.
    /// </summary>
    public bool HasArchitectureIndicators { get; init; }

    /// <summary>
    /// The primary category of the development context.
    /// </summary>
    public string Category { get; init; } = "general";

    /// <summary>
    /// Indicates if the context analysis has sufficient confidence for constraint activation.
    /// </summary>
    public bool HasSufficientConfidence => ConfidenceScore >= 0.5;

    /// <summary>
    /// Gets a summary of the context analysis.
    /// </summary>
    public string GetSummary()
    {
        var indicators = new System.Collections.Generic.List<string>();

        if (HasTddIndicators)
        {
            indicators.Add("TDD");
        }

        if (HasRefactoringIndicators)
        {
            indicators.Add("Refactoring");
        }

        if (HasArchitectureIndicators)
        {
            indicators.Add("Architecture");
        }

        var indicatorText = indicators.Count > 0 ? string.Join(", ", indicators) : "None";

        return $"Category: {Category}, Confidence: {ConfidenceScore:F2}, " +
               $"Indicators: {indicatorText}, Keywords: {string.Join(", ", AnalyzedKeywords)}";
    }
}
