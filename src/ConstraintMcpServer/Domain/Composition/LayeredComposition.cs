using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Layered composition strategy for Clean Architecture enforcement.
/// Ensures proper layer dependency ordering: Domain → Application → Infrastructure → Presentation.
/// Prevents architectural violations by enforcing dependency direction rules.
/// </summary>
public sealed class LayeredComposition : ICompositionStrategy
{
    public CompositionType Type => CompositionType.Layered;

    /// <summary>
    /// Gets the next constraint based on layered architecture rules.
    /// Enforces layer ordering and dependency direction validation.
    /// </summary>
    public Result<ConstraintActivation, ActivationError> GetNextConstraint(
        LayeredCompositionState state,
        IReadOnlyList<LayerConstraintInfo> layers,
        CompositionContext context)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (layers == null)
        {
            throw new ArgumentNullException(nameof(layers));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Check for layer dependency violations first
        var violations = DetectLayerViolations(context, layers);
        if (violations.Any())
        {
            var violationConstraint = CreateViolationConstraint(violations.First());
            return Result.Success<ConstraintActivation, ActivationError>(violationConstraint);
        }

        // Get next layer constraint based on current layer focus
        var currentLayer = DetermineCurrentLayer(context);
        var nextLayer = GetNextRequiredLayer(state, currentLayer, layers);

        if (nextLayer == null)
        {
            return Result.Success<ConstraintActivation, ActivationError>(ConstraintActivation.None);
        }

        var constraint = GetConstraintForLayer(nextLayer, layers);
        var guidance = CreateLayerGuidance(nextLayer, context);

        return Result.Success<ConstraintActivation, ActivationError>(
            new ConstraintActivation(
                constraint.ConstraintId,
                nextLayer.LayerLevel,
                guidance,
                DateTime.UtcNow));
    }

    /// <summary>
    /// Advances the layered composition state after constraint completion.
    /// </summary>
    public LayeredCompositionState AdvanceState(
        LayeredCompositionState currentState,
        ConstraintActivation completedActivation,
        CompositionContext context)
    {
        if (currentState == null)
        {
            throw new ArgumentNullException(nameof(currentState));
        }

        if (completedActivation == null)
        {
            throw new ArgumentNullException(nameof(completedActivation));
        }

        return currentState with
        {
            CompletedLayers = currentState.CompletedLayers.Add(completedActivation.LayerLevel),
            CurrentLayer = DetermineNextLayer(completedActivation.LayerLevel),
            LastActivation = completedActivation.Timestamp,
            ViolationsDetected = DetectLayerViolations(context, GetAllLayers()).ToList()
        };
    }

    private IEnumerable<LayerViolation> DetectLayerViolations(
        CompositionContext context,
        IReadOnlyList<LayerConstraintInfo> layers)
    {
        // Detect when inner layers depend on outer layers (architectural violation)
        // Domain (0) should not depend on Application (1), Infrastructure (2), or Presentation (3)
        // Application (1) should not depend on Infrastructure (2) or Presentation (3)
        // Infrastructure (2) should not depend on Presentation (3)

        var violations = new List<LayerViolation>();

        if (context.CodeAnalysis?.Dependencies != null)
        {
            foreach (var dependency in context.CodeAnalysis.Dependencies)
            {
                var sourceLayer = GetLayerForNamespace(dependency.Source);
                var targetLayer = GetLayerForNamespace(dependency.Target);

                if (sourceLayer < targetLayer) // Inner layer depending on outer layer
                {
                    violations.Add(new LayerViolation(
                        sourceLayer,
                        targetLayer,
                        dependency.Source,
                        dependency.Target,
                        $"Layer {sourceLayer} should not depend on layer {targetLayer}"
                    ));
                }
            }
        }

        return violations;
    }

    private int GetLayerForNamespace(string namespaceName)
    {
        // Simple heuristic to determine layer based on namespace
        return namespaceName.ToLowerInvariant() switch
        {
            var ns when ns.Contains("domain") => 0,
            var ns when ns.Contains("application") => 1,
            var ns when ns.Contains("infrastructure") => 2,
            var ns when ns.Contains("presentation") || ns.Contains("web") || ns.Contains("api") => 3,
            _ => 1 // Default to application layer
        };
    }

    private ConstraintActivation CreateViolationConstraint(LayerViolation violation)
    {
        return new ConstraintActivation(
            $"arch.violation.layer-{violation.SourceLayer}-to-{violation.TargetLayer}",
            violation.SourceLayer,
            $"Architectural violation: {violation.Message}. " +
            $"Consider moving dependency from {violation.SourceNamespace} to follow Clean Architecture principles.",
            DateTime.UtcNow);
    }

    private int DetermineCurrentLayer(CompositionContext context)
    {
        // Analyze current context to determine which layer developer is working on
        if (context.CurrentFile != null)
        {
            return GetLayerForNamespace(context.CurrentFile.Namespace ?? "");
        }

        return 0; // Default to domain layer
    }

    private LayerConstraintInfo? GetNextRequiredLayer(
        LayeredCompositionState state,
        int currentLayer,
        IReadOnlyList<LayerConstraintInfo> layers)
    {
        // Find the next layer that needs constraints activated
        var remainingLayers = layers
            .Where(layer => !state.CompletedLayers.Contains(layer.LayerLevel))
            .OrderBy(layer => layer.LayerLevel);

        return remainingLayers.FirstOrDefault();
    }

    private LayerConstraintInfo GetConstraintForLayer(
        LayerConstraintInfo layer,
        IReadOnlyList<LayerConstraintInfo> layers)
    {
        return layer;
    }

    private string CreateLayerGuidance(LayerConstraintInfo layer, CompositionContext context)
    {
        return layer.LayerLevel switch
        {
            0 => "Focus on Domain layer: Pure business logic with no external dependencies",
            1 => "Focus on Application layer: Use cases and business workflows, depend only on Domain",
            2 => "Focus on Infrastructure layer: External concerns (database, web APIs), implement Application interfaces",
            3 => "Focus on Presentation layer: UI concerns, depend on Application for business logic",
            _ => $"Focus on layer {layer.LayerLevel}: Maintain proper dependency direction"
        };
    }

    private int DetermineNextLayer(int completedLayer)
    {
        return completedLayer + 1;
    }

    private IReadOnlyList<LayerConstraintInfo> GetAllLayers()
    {
        // This would normally come from configuration, using defaults for now
        return new[]
        {
            new LayerConstraintInfo("arch.domain-layer", 0, "Domain Layer"),
            new LayerConstraintInfo("arch.application-layer", 1, "Application Layer"),
            new LayerConstraintInfo("arch.infrastructure-layer", 2, "Infrastructure Layer"),
            new LayerConstraintInfo("arch.presentation-layer", 3, "Presentation Layer")
        };
    }
}

/// <summary>
/// Information about a layer constraint in the Clean Architecture.
/// </summary>
public sealed record LayerConstraintInfo(
    string ConstraintId,
    int LayerLevel,
    string LayerName)
{
    /// <summary>
    /// Validates layer constraint information.
    /// </summary>
    public static LayerConstraintInfo Create(string constraintId, int layerLevel, string layerName)
    {
        if (string.IsNullOrWhiteSpace(constraintId))
        {
            throw new ArgumentException("Constraint ID cannot be null or empty", nameof(constraintId));
        }

        if (layerLevel < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(layerLevel), "Layer level must be non-negative");
        }

        if (string.IsNullOrWhiteSpace(layerName))
        {
            throw new ArgumentException("Layer name cannot be null or empty", nameof(layerName));
        }

        return new LayerConstraintInfo(constraintId, layerLevel, layerName);
    }
}

/// <summary>
/// Represents a layer dependency violation in Clean Architecture.
/// </summary>
public sealed record LayerViolation(
    int SourceLayer,
    int TargetLayer,
    string SourceNamespace,
    string TargetNamespace,
    string Message);
