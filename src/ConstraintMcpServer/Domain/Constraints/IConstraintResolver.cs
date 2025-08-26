using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Interface for constraint resolution from library.
/// Resolves constraints by ID from constraint libraries with caching and performance optimization.
/// </summary>
public interface IConstraintResolver
{
    /// <summary>
    /// Resolves a constraint by its ID from the constraint library.
    /// </summary>
    /// <param name="constraintId">The constraint ID to resolve</param>
    /// <returns>The resolved constraint (atomic or composite)</returns>
    /// <exception cref="ConstraintNotFoundException">Thrown when constraint ID is not found</exception>
    /// <exception cref="CircularReferenceException">Thrown when circular references are detected</exception>
    /// <exception cref="ArgumentNullException">Thrown when constraintId is null</exception>
    IConstraint ResolveConstraint(ConstraintId constraintId);
    
    /// <summary>
    /// Gets performance metrics for constraint resolution.
    /// </summary>
    /// <returns>Resolution metrics for monitoring and optimization</returns>
    IResolutionMetrics GetResolutionMetrics();
}

/// <summary>
/// Interface for async constraint resolution.
/// </summary>
public interface IAsyncConstraintResolver : IConstraintResolver
{
    /// <summary>
    /// Asynchronously resolves a constraint by its ID from the constraint library.
    /// </summary>
    /// <param name="constraintId">The constraint ID to resolve</param>
    /// <returns>Task containing the resolved constraint</returns>
    Task<IConstraint> ResolveConstraintAsync(ConstraintId constraintId);
}

/// <summary>
/// Interface for constraint resolution performance metrics.
/// </summary>
public interface IResolutionMetrics
{
    /// <summary>
    /// Total number of constraint resolutions performed.
    /// </summary>
    int TotalResolutions { get; }
    
    /// <summary>
    /// Cache hit rate as a percentage (0.0 to 1.0).
    /// </summary>
    double CacheHitRate { get; }
    
    /// <summary>
    /// Average time taken to resolve constraints.
    /// </summary>
    TimeSpan AverageResolutionTime { get; }
    
    /// <summary>
    /// Peak resolution time (p95 performance metric).
    /// </summary>
    TimeSpan PeakResolutionTime { get; }
}

/// <summary>
/// Base interface for all constraints (atomic and composite).
/// </summary>
public interface IConstraint
{
    /// <summary>
    /// Unique identifier for the constraint.
    /// </summary>
    ConstraintId Id { get; }
    
    /// <summary>
    /// Human-readable title of the constraint.
    /// </summary>
    string Title { get; }
    
    /// <summary>
    /// Priority for constraint selection (0.0 to 1.0).
    /// </summary>
    double Priority { get; }
    
    /// <summary>
    /// Trigger configuration for context-aware activation.
    /// </summary>
    TriggerConfiguration Triggers { get; }
    
    /// <summary>
    /// Reminder messages to inject when constraint is active.
    /// </summary>
    IReadOnlyList<string> Reminders { get; }
    
    /// <summary>
    /// Optional metadata for the constraint.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}

/// <summary>
/// Exception thrown when a constraint cannot be found by its ID.
/// </summary>
public class ConstraintNotFoundException : Exception
{
    public ConstraintId ConstraintId { get; }
    
    public ConstraintNotFoundException(ConstraintId constraintId) 
        : base($"Constraint not found: {constraintId}")
    {
        ConstraintId = constraintId;
    }
}

/// <summary>
/// Exception thrown when circular references are detected in constraint resolution.
/// </summary>
public class CircularReferenceException : Exception
{
    public List<ConstraintId> ReferenceChain { get; }
    
    public CircularReferenceException(List<ConstraintId> referenceChain)
        : base($"Circular reference detected: {string.Join(" -> ", referenceChain)}")
    {
        ReferenceChain = referenceChain;
    }
}

/// <summary>
/// Exception thrown when constraint reference validation fails.
/// </summary>
public class ConstraintReferenceValidationException : Exception
{
    public List<ConstraintId> MissingReferences { get; }
    
    public ConstraintReferenceValidationException(List<ConstraintId> missingReferences)
        : base($"Missing constraint references: {string.Join(", ", missingReferences)}")
    {
        MissingReferences = missingReferences;
    }
}

/// <summary>
/// Exception thrown when attempting to remove a constraint that's in use.
/// </summary>
public class ConstraintInUseException : Exception
{
    public ConstraintId ConstraintId { get; }
    public List<ConstraintId> ReferencingConstraints { get; }
    
    public ConstraintInUseException(ConstraintId constraintId, List<ConstraintId> referencingConstraints)
        : base($"Cannot remove constraint '{constraintId}' - referenced by: {string.Join(", ", referencingConstraints)}")
    {
        ConstraintId = constraintId;
        ReferencingConstraints = referencingConstraints;
    }
}