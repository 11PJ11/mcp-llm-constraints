using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintMcpServer.Domain.Common;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Layered composition strategy for user-defined layered architectures.
/// Enforces proper layer dependency ordering based on user-defined layer hierarchy.
/// Prevents architectural violations by enforcing user-configured dependency direction rules.
/// This strategy is methodology-agnostic and works with any user-defined layered architecture.
/// </summary>
public sealed class LayeredComposition : ICompositionStrategy
{
    public CompositionType Type => CompositionType.Layered;

    /// <summary>
    /// Gets the next constraint based on user-defined layered architecture rules.
    /// Enforces user-configured layer ordering and dependency direction validation.
    /// </summary>
    public Result<ConstraintActivation, ActivationError> GetNextConstraint(
        LayeredCompositionState state,
        UserDefinedLayerHierarchy layerHierarchy,
        CompositionContext context)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (layerHierarchy == null)
        {
            throw new ArgumentNullException(nameof(layerHierarchy));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Check for user-defined layer dependency violations first
        var violations = DetectLayerViolations(context, layerHierarchy);
        if (violations.Any())
        {
            var violationConstraint = CreateViolationConstraint(violations.First(), layerHierarchy);
            return Result.Success<ConstraintActivation, ActivationError>(violationConstraint);
        }

        // Get next layer constraint based on current layer focus
        var currentLayer = DetermineCurrentLayer(context, layerHierarchy);
        var nextLayer = GetNextRequiredLayer(state, currentLayer, layerHierarchy);

        if (nextLayer == null)
        {
            return Result.Success<ConstraintActivation, ActivationError>(ConstraintActivation.None);
        }

        var constraint = GetConstraintForLayer(nextLayer);
        var guidance = CreateLayerGuidance(nextLayer, layerHierarchy);

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
        UserDefinedLayerHierarchy layerHierarchy,
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
            CurrentLayer = DetermineNextLayer(completedActivation.LayerLevel, layerHierarchy),
            LastActivation = completedActivation.Timestamp,
            ViolationsDetected = DetectLayerViolations(context, layerHierarchy).ToList()
        };
    }

    private IEnumerable<UserDefinedLayerViolation> DetectLayerViolations(
        CompositionContext context,
        UserDefinedLayerHierarchy layerHierarchy)
    {
        var violations = new List<UserDefinedLayerViolation>();

        if (context.CodeAnalysis?.Dependencies != null)
        {
            foreach (var dependency in context.CodeAnalysis.Dependencies)
            {
                var sourceLayer = GetLayerForNamespace(dependency.Source, layerHierarchy);
                var targetLayer = GetLayerForNamespace(dependency.Target, layerHierarchy);

                // Check if this violates user-defined dependency rules
                if (layerHierarchy.IsViolation(sourceLayer, targetLayer))
                {
                    var sourceLayerName = layerHierarchy.GetLayerName(sourceLayer);
                    var targetLayerName = layerHierarchy.GetLayerName(targetLayer);
                    
                    violations.Add(new UserDefinedLayerViolation(
                        sourceLayer,
                        targetLayer,
                        dependency.Source,
                        dependency.Target,
                        $"Layer '{sourceLayerName}' should not depend on layer '{targetLayerName}' per user configuration"
                    ));
                }
            }
        }

        return violations;
    }

    private int GetLayerForNamespace(string namespaceName, UserDefinedLayerHierarchy layerHierarchy)
    {
        return layerHierarchy.DetermineLayerFromNamespace(namespaceName);
    }

    private ConstraintActivation CreateViolationConstraint(
        UserDefinedLayerViolation violation,
        UserDefinedLayerHierarchy layerHierarchy)
    {
        var sourceLayerName = layerHierarchy.GetLayerName(violation.SourceLayer);
        var targetLayerName = layerHierarchy.GetLayerName(violation.TargetLayer);
        
        return new ConstraintActivation(
            $"arch.violation.layer-{sourceLayerName}-to-{targetLayerName}",
            violation.SourceLayer,
            $"User-defined architectural violation: {violation.Message}. " +
            $"Consider restructuring dependency from {violation.SourceNamespace} to follow your configured architecture principles.",
            DateTime.UtcNow);
    }

    private int DetermineCurrentLayer(CompositionContext context, UserDefinedLayerHierarchy layerHierarchy)
    {
        // Analyze current context to determine which layer developer is working on
        if (context.CurrentFile != null)
        {
            return GetLayerForNamespace(context.CurrentFile.Namespace ?? "", layerHierarchy);
        }

        return layerHierarchy.GetInnerMostLayer(); // Default to innermost layer
    }

    private UserDefinedLayerInfo? GetNextRequiredLayer(
        LayeredCompositionState state,
        int currentLayer,
        UserDefinedLayerHierarchy layerHierarchy)
    {
        // Find the next layer that needs constraints activated based on user configuration
        var remainingLayers = layerHierarchy.Layers
            .Where(layer => !state.CompletedLayers.Contains(layer.LayerLevel))
            .OrderBy(layer => layer.LayerLevel);

        return remainingLayers.FirstOrDefault();
    }

    private UserDefinedLayerInfo GetConstraintForLayer(UserDefinedLayerInfo layer)
    {
        return layer;
    }

    private string CreateLayerGuidance(UserDefinedLayerInfo layer, UserDefinedLayerHierarchy layerHierarchy)
    {
        var layerGuidance = layer.Description;
        var allowedDependencies = layerHierarchy.GetAllowedDependencies(layer.LayerLevel);
        
        if (allowedDependencies.Any())
        {
            var dependencyNames = allowedDependencies.Select(layerHierarchy.GetLayerName);
            layerGuidance += $" | Allowed dependencies: {string.Join(", ", dependencyNames)}";
        }
        
        return layerGuidance;
    }

    private int DetermineNextLayer(int completedLayer, UserDefinedLayerHierarchy layerHierarchy)
    {
        return layerHierarchy.GetNextLayer(completedLayer);
    }

}


/// <summary>
/// Represents a user-defined layer dependency violation in layered architecture.
/// </summary>
public sealed record UserDefinedLayerViolation(
    int SourceLayer,
    int TargetLayer,
    string SourceNamespace,
    string TargetNamespace,
    string Message);
