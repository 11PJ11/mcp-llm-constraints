using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Immutable state for user-defined layered composition tracking.
/// Tracks completed layers, current layer focus, and detected violations.
/// Works with any user-defined layered architecture methodology.
/// </summary>
public sealed record LayeredCompositionState
{
    /// <summary>
    /// Set of completed layer levels.
    /// </summary>
    public ImmutableHashSet<int> CompletedLayers { get; init; } = ImmutableHashSet<int>.Empty;

    /// <summary>
    /// Current layer level being worked on.
    /// </summary>
    public int CurrentLayer { get; init; } = 0;

    /// <summary>
    /// Timestamp of the last constraint activation.
    /// </summary>
    public DateTime? LastActivation { get; init; }

    /// <summary>
    /// List of detected layer violations.
    /// </summary>
    public IReadOnlyList<UserDefinedLayerViolation> ViolationsDetected { get; init; } = Array.Empty<UserDefinedLayerViolation>();

    /// <summary>
    /// Whether all layers have been processed.
    /// </summary>
    public bool IsComplete(int totalLayerCount) => CompletedLayers.Count >= totalLayerCount;

    /// <summary>
    /// Creates a new initial state for layered composition.
    /// </summary>
    public static LayeredCompositionState Initial => new();

    /// <summary>
    /// Creates a new state with additional completed layer.
    /// </summary>
    public LayeredCompositionState WithCompletedLayer(int layerLevel)
    {
        if (layerLevel < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(layerLevel), "Layer level must be non-negative");
        }

        return this with
        {
            CompletedLayers = CompletedLayers.Add(layerLevel),
            CurrentLayer = DetermineNextLayer(layerLevel),
            LastActivation = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a new state with detected violations.
    /// </summary>
    public LayeredCompositionState WithViolations(IEnumerable<UserDefinedLayerViolation> violations)
    {
        if (violations == null)
        {
            throw new ArgumentNullException(nameof(violations));
        }

        return this with
        {
            ViolationsDetected = violations.ToArray()
        };
    }

    /// <summary>
    /// Creates a new state with updated current layer.
    /// </summary>
    public LayeredCompositionState WithCurrentLayer(int layerLevel)
    {
        if (layerLevel < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(layerLevel), "Layer level must be non-negative");
        }

        return this with
        {
            CurrentLayer = layerLevel
        };
    }

    /// <summary>
    /// Checks if a specific layer has been completed.
    /// </summary>
    public bool IsLayerCompleted(int layerLevel) => CompletedLayers.Contains(layerLevel);

    /// <summary>
    /// Gets the count of completed layers.
    /// </summary>
    public int CompletedLayerCount => CompletedLayers.Count;

    /// <summary>
    /// Gets the count of detected violations.
    /// </summary>
    public int ViolationCount => ViolationsDetected.Count;

    /// <summary>
    /// Validates the current state for consistency.
    /// </summary>
    public ValidationResult Validate()
    {
        var errors = new List<string>();

        // Check if current layer is reasonable
        if (CurrentLayer < 0 || CurrentLayer > 10)
        {
            errors.Add($"Current layer {CurrentLayer} is out of reasonable range (0-10)");
        }

        // Check if completed layers are in reasonable range
        foreach (var layer in CompletedLayers)
        {
            if (layer < 0 || layer > 10)
            {
                errors.Add($"Completed layer {layer} is out of reasonable range (0-10)");
            }
        }

        // Check for violations with invalid layer references
        foreach (var violation in ViolationsDetected)
        {
            if (violation.SourceLayer < 0 || violation.TargetLayer < 0)
            {
                errors.Add($"Violation has invalid layer reference: {violation.SourceLayer} → {violation.TargetLayer}");
            }
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }

    /// <summary>
    /// Determines the next layer based on user-defined hierarchy.
    /// </summary>
    /// <param name="completedLayer">The completed layer level.</param>
    /// <param name="maxLayerLevel">The maximum layer level in user hierarchy.</param>
    /// <returns>The next layer level to work on.</returns>
    public int DetermineNextLayer(int completedLayer, int maxLayerLevel)
    {
        return Math.Min(completedLayer + 1, maxLayerLevel);
    }

    private int DetermineNextLayer(int completedLayer)
    {
        // Default behavior for backward compatibility - assumes 4-layer architecture
        return DetermineNextLayer(completedLayer, 3);
    }

    /// <summary>
    /// String representation for debugging and logging.
    /// </summary>
    public override string ToString()
    {
        var completedLayers = string.Join(", ", CompletedLayers);
        var violationCount = ViolationsDetected.Count;

        return $"LayeredCompositionState(Current: {CurrentLayer}, " +
               $"Completed: [{completedLayers}], " +
               $"Violations: {violationCount}, " +
               $"Complete: {IsComplete})";
    }
}

/// <summary>
/// Result of state validation.
/// </summary>
public sealed record ValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    private ValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static ValidationResult Success() => new(true, Array.Empty<string>());

    public static ValidationResult Failure(IEnumerable<string> errors) =>
        new(false, errors.ToArray());
}
