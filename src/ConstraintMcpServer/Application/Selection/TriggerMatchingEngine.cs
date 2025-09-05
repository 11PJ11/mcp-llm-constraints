using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Context;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Application.Selection.ConfidenceStrategies;
using ConstraintMcpServer.Infrastructure.Logging;

namespace ConstraintMcpServer.Application.Selection;

/// <summary>
/// Core trigger matching engine for context-aware constraint activation.
/// Implements business logic for evaluating trigger configurations against context.
/// </summary>
public class TriggerMatchingEngine : ITriggerMatchingEngine
{
    private const double NoRelevanceThreshold = 0.0;
    private const double DefaultConstraintPriority = 0.8;

    // Known constraint IDs - should be moved to configuration in future iterations
    private const string TddTestFirstConstraintId = "tdd.test-first";
    private const string RefactoringCleanCodeConstraintId = "refactoring.clean-code";
    private IConstraintResolver _constraintResolver;
    private readonly TriggerMatchingConfiguration _configuration;
    private readonly IReadOnlyList<IConfidenceBoostStrategy> _confidenceStrategies;
    private readonly IStructuredEventLogger _logger;

    /// <summary>
    /// Gets the current configuration settings for the trigger matching engine.
    /// </summary>
    public TriggerMatchingConfiguration Configuration => _configuration;

    /// <summary>
    /// Creates a new trigger matching engine with default configuration.
    /// </summary>
    public TriggerMatchingEngine()
        : this(new TriggerMatchingConfiguration(), CreateDefaultStrategies())
    {
    }

    /// <summary>
    /// Creates a new trigger matching engine with custom configuration.
    /// </summary>
    /// <param name="configuration">Engine configuration</param>
    public TriggerMatchingEngine(TriggerMatchingConfiguration configuration)
        : this(configuration, CreateDefaultStrategies())
    {
    }

    /// <summary>
    /// Creates a new trigger matching engine with custom configuration and strategies.
    /// </summary>
    /// <param name="configuration">Engine configuration</param>
    /// <param name="confidenceStrategies">Confidence boosting strategies</param>
    public TriggerMatchingEngine(
        TriggerMatchingConfiguration configuration,
        IReadOnlyList<IConfidenceBoostStrategy> confidenceStrategies)
        : this(configuration, confidenceStrategies, new DefaultConstraintResolver())
    {
    }

    /// <summary>
    /// Creates a new trigger matching engine with full dependency injection.
    /// </summary>
    /// <param name="configuration">Engine configuration</param>
    /// <param name="confidenceStrategies">Confidence boosting strategies</param>
    /// <param name="constraintResolver">Constraint resolver implementation</param>
    public TriggerMatchingEngine(
        TriggerMatchingConfiguration configuration,
        IReadOnlyList<IConfidenceBoostStrategy> confidenceStrategies,
        IConstraintResolver constraintResolver)
        : this(configuration, confidenceStrategies, constraintResolver, new StructuredEventLogger())
    {
    }

    /// <summary>
    /// Creates a new trigger matching engine with full dependency injection including logger.
    /// </summary>
    /// <param name="configuration">Engine configuration</param>
    /// <param name="confidenceStrategies">Confidence boosting strategies</param>
    /// <param name="constraintResolver">Constraint resolver implementation</param>
    /// <param name="logger">Structured event logger</param>
    public TriggerMatchingEngine(
        TriggerMatchingConfiguration configuration,
        IReadOnlyList<IConfidenceBoostStrategy> confidenceStrategies,
        IConstraintResolver constraintResolver,
        IStructuredEventLogger logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _confidenceStrategies = confidenceStrategies ?? throw new ArgumentNullException(nameof(confidenceStrategies));
        _constraintResolver = constraintResolver ?? throw new ArgumentNullException(nameof(constraintResolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Evaluates constraints against the provided context and returns activated constraints
    /// ordered by confidence score (highest first).
    /// </summary>
    /// <param name="context">Current development context</param>
    /// <returns>List of activated constraints ordered by confidence</returns>
    public async Task<IReadOnlyList<ConstraintActivation>> EvaluateConstraints(TriggerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            var activations = await CollectRelevantActivations(context);
            return SelectTopActivationsByConfidence(activations);
        }
        catch (Exception ex)
        {
            _logger.LogError(-1, $"Error evaluating constraints: {ex.Message}");
            return Array.Empty<ConstraintActivation>().AsReadOnly();
        }
    }

    private async Task<List<ConstraintActivation>> CollectRelevantActivations(TriggerContext context)
    {
        var activations = new List<ConstraintActivation>();
        var constraints = await RetrieveAllAvailableConstraints();

        foreach (var constraint in constraints)
        {
            var activation = await EvaluateSingleConstraintForActivation(constraint, context);

            if (activation != null && IsActivationAboveThreshold(activation))
            {
                activations.Add(activation);
            }
        }
        return activations;
    }

    private bool IsActivationAboveThreshold(ConstraintActivation? activation)
    {
        return activation != null;
    }

    private IReadOnlyList<ConstraintActivation> SelectTopActivationsByConfidence(
        List<ConstraintActivation> activations)
    {
        return activations
            .OrderByDescending(a => a.ConfidenceScore)
            .Take(_configuration.MaxActiveConstraints)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets constraints that meet or exceed the specified confidence threshold.
    /// </summary>
    /// <param name="context">Current development context</param>
    /// <param name="minConfidence">Minimum confidence threshold</param>
    /// <returns>Constraints meeting the confidence threshold</returns>
    public async Task<IReadOnlyList<ConstraintActivation>> GetRelevantConstraints(
        TriggerContext context,
        double minConfidence = 0.7)
    {
        var allActivations = await EvaluateConstraints(context);

        return allActivations
            .Where(a => a.ConfidenceScore >= minConfidence)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Configures the constraint library used for trigger matching.
    /// </summary>
    /// <param name="constraintResolver">Constraint library resolver</param>
    public async Task ConfigureConstraintLibrary(IConstraintResolver constraintResolver)
    {
        _constraintResolver = constraintResolver ?? throw new ArgumentNullException(nameof(constraintResolver));
        await Task.CompletedTask;
    }

    /// <summary>
    /// Evaluates a single constraint against the context to determine activation.
    /// </summary>
    private Task<ConstraintActivation?> EvaluateSingleConstraintForActivation(
        IConstraint constraint,
        TriggerContext context)
    {
        try
        {
            var evaluationResult = EvaluateConstraintForActivation(constraint, context);
            var activation = evaluationResult.ToActivation(constraint.Id.Value);
            return Task.FromResult(activation);
        }
        catch (Exception ex)
        {
            _logger.LogError(-1, $"Error evaluating constraint {constraint.Id.Value}: {ex.Message}");
            return Task.FromResult<ConstraintActivation?>(null);
        }
    }

    /// <summary>
    /// Evaluates a single constraint and returns detailed evaluation result.
    /// </summary>
    private ConstraintEvaluationResult EvaluateConstraintForActivation(
        IConstraint constraint,
        TriggerContext context)
    {
        if (!TryGetAtomicConstraintWithTriggers(constraint, out var atomicConstraint, out var triggerConfig))
        {
            return CreateInvalidConstraintResult(constraint, context);
        }

        var relevanceScore = CalculateConstraintRelevance(context, atomicConstraint!, triggerConfig!);
        var shouldActivate = DetermineConstraintActivation(relevanceScore, triggerConfig!);
        var reason = shouldActivate ? IdentifyPrimaryActivationCause(context, triggerConfig!) : ActivationReason.Unknown;

        return CreateEvaluationResult(constraint, context, relevanceScore, shouldActivate, reason);
    }

    /// <summary>
    /// Creates an evaluation result for constraints that cannot be processed.
    /// </summary>
    private static ConstraintEvaluationResult CreateInvalidConstraintResult(IConstraint constraint, TriggerContext context)
    {
        return new ConstraintEvaluationResult(
            relevanceScore: 0.0,
            shouldActivate: false,
            reason: ActivationReason.Unknown,
            constraint: constraint,
            context: context);
    }

    /// <summary>
    /// Creates a complete constraint evaluation result with all parameters.
    /// </summary>
    private static ConstraintEvaluationResult CreateEvaluationResult(
        IConstraint constraint,
        TriggerContext context,
        double relevanceScore,
        bool shouldActivate,
        ActivationReason reason)
    {
        return new ConstraintEvaluationResult(
            relevanceScore: relevanceScore,
            shouldActivate: shouldActivate,
            reason: reason,
            constraint: constraint,
            context: context);
    }

    /// <summary>
    /// Determines whether a constraint should be activated based on relevance and threshold.
    /// </summary>
    private static bool DetermineConstraintActivation(double relevanceScore, TriggerConfiguration triggerConfig)
    {
        return IsRelevantConstraint(relevanceScore) && relevanceScore >= triggerConfig.ConfidenceThreshold;
    }

    private static bool TryGetAtomicConstraintWithTriggers(
        IConstraint constraint,
        out AtomicConstraint? atomicConstraint,
        out TriggerConfiguration? triggerConfig)
    {
        atomicConstraint = constraint as AtomicConstraint;
        triggerConfig = atomicConstraint?.Triggers;
        return atomicConstraint != null && triggerConfig != null;
    }

    private double CalculateConstraintRelevance(
        TriggerContext context,
        AtomicConstraint atomicConstraint,
        TriggerConfiguration triggerConfig)
    {
        var baseScore = context.CalculateRelevanceScore(triggerConfig);
        return ApplyContextSpecificBoosts(context, atomicConstraint, baseScore);
    }

    private double ApplyContextSpecificBoosts(
        TriggerContext context,
        AtomicConstraint atomicConstraint,
        double baseScore)
    {
        var applicableStrategies = _confidenceStrategies
            .Where(strategy => strategy.AppliesTo(atomicConstraint, context))
            .ToList();

        return applicableStrategies.Aggregate(baseScore, (current, strategy) => strategy.ApplyBoost(current));
    }

    private static bool IsRelevantConstraint(double relevanceScore)
    {
        return relevanceScore > NoRelevanceThreshold;
    }


    /// <summary>
    /// Determines the primary reason for constraint activation.
    /// </summary>
    private static ActivationReason IdentifyPrimaryActivationCause(TriggerContext context, TriggerConfiguration config)
    {
        var hasKeywordMatch = config.Keywords.Count > 0 && context.ContainsAnyKeyword(config.Keywords);
        var hasFileMatch = config.FilePatterns.Count > 0 && context.MatchesAnyFilePattern(config.FilePatterns);
        var hasContextMatch = config.ContextPatterns.Count > 0 && context.MatchesAnyContextPattern(config.ContextPatterns);

        var matchCount = (hasKeywordMatch ? 1 : 0) + (hasFileMatch ? 1 : 0) + (hasContextMatch ? 1 : 0);

        return matchCount switch
        {
            > 1 => ActivationReason.CombinedFactors,
            1 when hasKeywordMatch => ActivationReason.KeywordMatch,
            1 when hasFileMatch => ActivationReason.FilePatternMatch,
            1 when hasContextMatch => ActivationReason.ContextPatternMatch,
            _ => ActivationReason.Unknown
        };
    }

    /// <summary>
    /// Gets available constraints from the configured resolver.
    /// </summary>
    private Task<IEnumerable<IConstraint>> RetrieveAllAvailableConstraints()
    {
        var constraintIds = GetKnownConstraintIds();
        var constraints = ResolveConstraintsSafely(constraintIds);
        return Task.FromResult<IEnumerable<IConstraint>>(constraints);
    }

    /// <summary>
    /// Gets the list of known constraint IDs that should be available.
    /// TODO: Replace with dynamic discovery from constraint resolver.
    /// </summary>
    private static IEnumerable<ConstraintId> GetKnownConstraintIds()
    {
        return new[]
        {
            new ConstraintId(TddTestFirstConstraintId),
            new ConstraintId(RefactoringCleanCodeConstraintId)
        };
    }

    /// <summary>
    /// Resolves constraints safely, skipping any that cannot be resolved.
    /// </summary>
    private IEnumerable<IConstraint> ResolveConstraintsSafely(IEnumerable<ConstraintId> constraintIds)
    {
        return constraintIds
            .Select(TryResolveConstraint)
            .Where(constraint => constraint != null)
            .Cast<IConstraint>()
            .ToList();
    }

    /// <summary>
    /// Attempts to resolve a single constraint, returning null if resolution fails.
    /// </summary>
    private IConstraint? TryResolveConstraint(ConstraintId constraintId)
    {
        try
        {
            return _constraintResolver.ResolveConstraint(constraintId);
        }
        catch (ConstraintNotFoundException)
        {
            return null; // Skip constraints that don't exist
        }
    }

    /// <summary>
    /// Creates the default set of confidence boost strategies.
    /// </summary>
    private static IReadOnlyList<IConfidenceBoostStrategy> CreateDefaultStrategies()
    {
        return new List<IConfidenceBoostStrategy>
        {
            new TddKeywordBoostStrategy()
        }.AsReadOnly();
    }
}
