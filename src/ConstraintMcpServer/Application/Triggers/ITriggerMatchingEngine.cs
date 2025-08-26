using System.Collections.Generic;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain.Context;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Application.Triggers;

/// <summary>
/// Core trigger matching engine for context-aware constraint activation.
/// Implements business logic for evaluating trigger configurations against context.
/// </summary>
public interface ITriggerMatchingEngine
{
    /// <summary>
    /// Evaluates constraints against the provided context and returns activated constraints
    /// ordered by confidence score (highest first).
    /// </summary>
    /// <param name="context">Current development context containing keywords, file info, and activity type</param>
    /// <returns>List of activated constraints with confidence scores, ordered by relevance</returns>
    Task<IReadOnlyList<ConstraintActivation>> EvaluateConstraints(TriggerContext context);

    /// <summary>
    /// Gets constraints that meet or exceed the specified confidence threshold.
    /// Useful for filtering constraints based on activation confidence.
    /// </summary>
    /// <param name="context">Current development context</param>
    /// <param name="minConfidence">Minimum confidence threshold (default: 0.7)</param>
    /// <returns>Constraints meeting the confidence threshold, ordered by confidence</returns>
    Task<IReadOnlyList<ConstraintActivation>> GetRelevantConstraints(
        TriggerContext context, 
        double minConfidence = 0.7);

    /// <summary>
    /// Configures the constraint library used for trigger matching.
    /// Allows dynamic constraint library updates without recreating the engine.
    /// </summary>
    /// <param name="constraintLibrary">Constraint library containing available constraints</param>
    Task ConfigureConstraintLibrary(IConstraintResolver constraintLibrary);

    /// <summary>
    /// Gets the current configuration settings for the trigger matching engine.
    /// Provides visibility into confidence thresholds and matching behavior.
    /// </summary>
    TriggerMatchingConfiguration Configuration { get; }
}