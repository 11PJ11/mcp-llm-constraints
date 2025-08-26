using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Context;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Application.Triggers;

/// <summary>
/// Core trigger matching engine for context-aware constraint activation.
/// Implements business logic for evaluating trigger configurations against context.
/// </summary>
public class TriggerMatchingEngine : ITriggerMatchingEngine
{
    private IConstraintResolver _constraintResolver;
    private readonly TriggerMatchingConfiguration _configuration;

    /// <summary>
    /// Gets the current configuration settings for the trigger matching engine.
    /// </summary>
    public TriggerMatchingConfiguration Configuration => _configuration;

    /// <summary>
    /// Creates a new trigger matching engine with default configuration.
    /// </summary>
    public TriggerMatchingEngine()
        : this(new TriggerMatchingConfiguration())
    {
    }

    /// <summary>
    /// Creates a new trigger matching engine with custom configuration.
    /// </summary>
    /// <param name="configuration">Engine configuration</param>
    public TriggerMatchingEngine(TriggerMatchingConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _constraintResolver = new DefaultConstraintResolver(); // Temporary implementation
    }

    /// <summary>
    /// Evaluates constraints against the provided context and returns activated constraints
    /// ordered by confidence score (highest first).
    /// </summary>
    /// <param name="context">Current development context</param>
    /// <returns>List of activated constraints ordered by confidence</returns>
    public async Task<IReadOnlyList<ConstraintActivation>> EvaluateConstraints(TriggerContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var activations = new List<ConstraintActivation>();

        try
        {
            // Get all available constraints
            var constraints = await GetAvailableConstraints();

            // Evaluate each constraint against the context
            foreach (var constraint in constraints)
            {
                var activation = await EvaluateConstraintActivation(constraint, context);
                if (activation != null && activation.MeetsConfidenceThreshold(_configuration.DefaultConfidenceThreshold))
                {
                    activations.Add(activation);
                }
            }

            // Sort by confidence score (highest first) and limit results
            return activations
                .OrderByDescending(a => a.ConfidenceScore)
                .Take(_configuration.MaxActiveConstraints)
                .ToList()
                .AsReadOnly();
        }
        catch (Exception ex)
        {
            // Log error and return empty result to maintain system stability
            // In production, this would use proper logging
            Console.WriteLine($"Error evaluating constraints: {ex.Message}");
            return Array.Empty<ConstraintActivation>().AsReadOnly();
        }
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
        await Task.CompletedTask; // Placeholder for async configuration
    }

    /// <summary>
    /// Evaluates a single constraint against the context to determine activation.
    /// </summary>
    private async Task<ConstraintActivation?> EvaluateConstraintActivation(
        IConstraint constraint,
        TriggerContext context)
    {
        if (constraint is not AtomicConstraint atomicConstraint)
        {
            return null; // Currently only handle atomic constraints
        }

        var triggerConfig = atomicConstraint.Triggers;
        if (triggerConfig == null)
        {
            return null;
        }

        // Calculate relevance score using existing domain logic
        var relevanceScore = context.CalculateRelevanceScore(triggerConfig);
        
        if (relevanceScore <= 0.0)
        {
            return null; // No relevance
        }

        // Determine activation reason based on matching factors
        var reason = DetermineActivationReason(context, triggerConfig);

        return new ConstraintActivation(
            constraintId: atomicConstraint.Id.Value,
            confidenceScore: relevanceScore,
            reason: reason,
            triggerContext: context,
            constraint: constraint
        );
    }

    /// <summary>
    /// Determines the primary reason for constraint activation.
    /// </summary>
    private ActivationReason DetermineActivationReason(TriggerContext context, TriggerConfiguration config)
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
    private async Task<IEnumerable<IConstraint>> GetAvailableConstraints()
    {
        // For now, return hard-coded test constraints
        // This will be replaced with proper constraint resolver integration
        return new List<IConstraint>
        {
            CreateTestConstraint("tdd.test-first", "Write a failing test first", 
                keywords: new[] { "test", "tdd", "failing", "red", "implement", "feature" }),
            CreateTestConstraint("tdd.simplest-code", "Write simplest code to pass",
                keywords: new[] { "implement", "green", "pass", "simple" }),
            CreateTestConstraint("refactoring.level1", "Improve readability",
                keywords: new[] { "refactor", "cleanup", "readability", "improve" }),
            CreateTestConstraint("arch.hexagonal", "Apply hexagonal architecture",
                keywords: new[] { "architecture", "design", "hexagonal" })
        };
    }

    /// <summary>
    /// Creates a test constraint for development purposes.
    /// </summary>
    private IConstraint CreateTestConstraint(string id, string title, string[] keywords)
    {
        var triggerConfig = new TriggerConfiguration(
            keywords: keywords,
            filePatterns: new[] { "*.cs", "*.js", "*.py" },
            contextPatterns: new[] { "development", "implementation", "testing", "refactoring", "feature_development" },
            antiPatterns: new[] { "hotfix", "urgent", "emergency" }
        );

        return new AtomicConstraint(
            id: new ConstraintId(id),
            title: title,
            priority: 0.8,
            triggers: triggerConfig,
            reminders: new[] { $"Reminder: {title}" }
        );
    }
}

/// <summary>
/// Temporary default constraint resolver for testing purposes.
/// Will be replaced with proper integration.
/// </summary>
internal class DefaultConstraintResolver : IConstraintResolver
{
    public IConstraint ResolveConstraint(ConstraintId constraintId)
    {
        throw new NotImplementedException("Temporary resolver for testing");
    }

    public IResolutionMetrics GetResolutionMetrics()
    {
        throw new NotImplementedException("Temporary resolver for testing");
    }
}