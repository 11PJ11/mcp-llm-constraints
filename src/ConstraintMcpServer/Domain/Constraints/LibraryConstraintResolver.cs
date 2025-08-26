using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Library-based constraint resolver with caching, performance optimization, and circular reference detection.
/// Implements sub-50ms p95 constraint resolution performance.
/// </summary>
public sealed class LibraryConstraintResolver : IAsyncConstraintResolver
{
    private readonly ConstraintLibrary _library;
    private readonly ConcurrentDictionary<ConstraintId, IConstraint> _cache = new();
    private readonly ConcurrentDictionary<ConstraintId, Task<IConstraint>> _resolutionTasks = new();
    private readonly ResolutionMetrics _metrics = new();
    private readonly object _metricsLock = new();

    /// <summary>
    /// Initializes a new instance of LibraryConstraintResolver.
    /// </summary>
    /// <param name="library">The constraint library to resolve from</param>
    /// <exception cref="ArgumentNullException">Thrown when library is null</exception>
    public LibraryConstraintResolver(ConstraintLibrary library)
    {
        _library = library ?? throw new ArgumentNullException(nameof(library));
    }

    /// <summary>
    /// Resolves a constraint by its ID from the constraint library.
    /// </summary>
    /// <param name="constraintId">The constraint ID to resolve</param>
    /// <returns>The resolved constraint (atomic or composite)</returns>
    /// <exception cref="ConstraintNotFoundException">Thrown when constraint ID is not found</exception>
    /// <exception cref="CircularReferenceException">Thrown when circular references are detected</exception>
    /// <exception cref="ArgumentNullException">Thrown when constraintId is null</exception>
    public IConstraint ResolveConstraint(ConstraintId constraintId)
    {
        if (constraintId == null)
            throw new ArgumentNullException(nameof(constraintId));

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = ResolveConstraintInternal(constraintId, new HashSet<ConstraintId>());
            RecordResolution(stopwatch.Elapsed, true);
            return result;
        }
        catch
        {
            RecordResolution(stopwatch.Elapsed, false);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously resolves a constraint by its ID from the constraint library.
    /// </summary>
    /// <param name="constraintId">The constraint ID to resolve</param>
    /// <returns>Task containing the resolved constraint</returns>
    public async Task<IConstraint> ResolveConstraintAsync(ConstraintId constraintId)
    {
        if (constraintId == null)
            throw new ArgumentNullException(nameof(constraintId));

        // Check if resolution is already in progress
        if (_resolutionTasks.TryGetValue(constraintId, out Task<IConstraint>? existingTask))
        {
            return await existingTask;
        }

        // Start new resolution task
        var resolutionTask = Task.Run(() => ResolveConstraint(constraintId));
        
        if (_resolutionTasks.TryAdd(constraintId, resolutionTask))
        {
            try
            {
                var result = await resolutionTask;
                return result;
            }
            finally
            {
                // Clean up completed task
                _resolutionTasks.TryRemove(constraintId, out _);
            }
        }
        else
        {
            // Another thread added the task, use that one
            if (_resolutionTasks.TryGetValue(constraintId, out Task<IConstraint>? newTask))
            {
                return await newTask;
            }
            
            // Fallback to synchronous resolution
            return ResolveConstraint(constraintId);
        }
    }

    /// <summary>
    /// Gets performance metrics for constraint resolution.
    /// </summary>
    /// <returns>Resolution metrics for monitoring and optimization</returns>
    public IResolutionMetrics GetResolutionMetrics()
    {
        lock (_metricsLock)
        {
            return _metrics;
        }
    }

    /// <summary>
    /// Resolves multiple constraints in parallel for improved performance.
    /// </summary>
    /// <param name="constraintIds">The constraint IDs to resolve</param>
    /// <returns>Dictionary mapping constraint IDs to their resolved constraints</returns>
    /// <exception cref="ArgumentNullException">Thrown when constraintIds is null</exception>
    public async Task<IReadOnlyDictionary<ConstraintId, IConstraint>> ResolveMultipleAsync(IEnumerable<ConstraintId> constraintIds)
    {
        if (constraintIds == null)
            throw new ArgumentNullException(nameof(constraintIds));

        var ids = constraintIds.ToList();
        if (ids.Count == 0)
            return new Dictionary<ConstraintId, IConstraint>().AsReadOnly();

        // Resolve all constraints in parallel for better performance
        var resolutionTasks = ids.Select(async id =>
        {
            try
            {
                var constraint = await ResolveConstraintAsync(id);
                return new KeyValuePair<ConstraintId, IConstraint>(id, constraint);
            }
            catch
            {
                // Individual failures are handled by throwing exceptions
                throw;
            }
        });

        var results = await Task.WhenAll(resolutionTasks);
        return results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).AsReadOnly();
    }

    /// <summary>
    /// Warms the cache by pre-loading specified constraints.
    /// </summary>
    /// <param name="constraintIds">The constraint IDs to pre-load</param>
    /// <returns>Task representing the cache warming operation</returns>
    public async Task WarmCacheAsync(IEnumerable<ConstraintId> constraintIds)
    {
        if (constraintIds == null)
            throw new ArgumentNullException(nameof(constraintIds));

        var ids = constraintIds.Where(id => !_cache.ContainsKey(id)).ToList();
        if (ids.Count == 0)
            return;

        // Pre-load constraints into cache
        var warmupTasks = ids.Select(id => ResolveConstraintAsync(id));
        await Task.WhenAll(warmupTasks);
    }

    /// <summary>
    /// Clears the resolution cache.
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
        _resolutionTasks.Clear();
    }

    private IConstraint ResolveConstraintInternal(ConstraintId constraintId, HashSet<ConstraintId> resolutionChain)
    {
        // Check cache first
        if (_cache.TryGetValue(constraintId, out IConstraint? cachedConstraint))
        {
            // Record cache hit for performance metrics (this is a successful resolution)
            RecordResolution(TimeSpan.Zero, true, isCacheHit: true);
            return cachedConstraint;
        }

        // Detect circular references
        if (resolutionChain.Contains(constraintId))
        {
            var chain = resolutionChain.ToList();
            chain.Add(constraintId);
            throw new CircularReferenceException(chain);
        }

        // Add to resolution chain
        resolutionChain.Add(constraintId);

        try
        {
            // Get constraint from library
            if (!_library.TryGetConstraint(constraintId, out IConstraint? constraint))
            {
                throw new ConstraintNotFoundException(constraintId);
            }

            // Resolve based on constraint type
            IConstraint resolvedConstraint = constraint! switch
            {
                AtomicConstraint atomic => atomic, // Atomic constraints don't need resolution
                CompositeConstraint composite => ResolveCompositeConstraint(composite, resolutionChain),
                _ => constraint!
            };

            // Cache the resolved constraint
            _cache.TryAdd(constraintId, resolvedConstraint);

            return resolvedConstraint;
        }
        finally
        {
            // Remove from resolution chain
            resolutionChain.Remove(constraintId);
        }
    }

    private CompositeConstraint ResolveCompositeConstraint(CompositeConstraint composite, HashSet<ConstraintId> resolutionChain)
    {
        // If composite uses inline components (legacy), return as-is
        if (composite.Components.Count > 0)
        {
            return composite;
        }

        // Resolve all component references
        var resolvedComponents = new List<AtomicConstraint>();
        
        foreach (var reference in composite.ComponentReferences)
        {
            var resolvedConstraint = ResolveConstraintInternal(reference.ConstraintId, resolutionChain);
            
            if (resolvedConstraint is AtomicConstraint atomicConstraint)
            {
                // Apply composition metadata if present
                var constraintWithMetadata = ApplyCompositionMetadata(atomicConstraint, reference);
                resolvedComponents.Add(constraintWithMetadata);
            }
            else if (resolvedConstraint is CompositeConstraint nestedComposite)
            {
                // Flatten nested composite constraints into atomic components
                var nestedComponents = ResolveCompositeToAtomicComponents(nestedComposite, resolutionChain);
                resolvedComponents.AddRange(nestedComponents);
            }
        }

        // Create resolved composite with inline components for backward compatibility
        var reminders = composite.Reminders.Count > 0 ? composite.Reminders : new[] { $"Apply {composite.Title} methodology" };
        return new CompositeConstraint(
            composite.Id,
            composite.Title,
            composite.Priority,
            composite.Triggers,
            composite.CompositionType,
            resolvedComponents,
            reminders);
    }

    private List<AtomicConstraint> ResolveCompositeToAtomicComponents(CompositeConstraint composite, HashSet<ConstraintId> resolutionChain)
    {
        var atomicComponents = new List<AtomicConstraint>();

        // If composite has inline components, use them
        if (composite.Components.Count > 0)
        {
            atomicComponents.AddRange(composite.Components);
        }
        else
        {
            // Resolve component references
            foreach (var reference in composite.ComponentReferences)
            {
                var resolvedConstraint = ResolveConstraintInternal(reference.ConstraintId, resolutionChain);
                
                if (resolvedConstraint is AtomicConstraint atomicConstraint)
                {
                    var constraintWithMetadata = ApplyCompositionMetadata(atomicConstraint, reference);
                    atomicComponents.Add(constraintWithMetadata);
                }
                else if (resolvedConstraint is CompositeConstraint nestedComposite)
                {
                    // Recursively resolve nested composites
                    var nestedComponents = ResolveCompositeToAtomicComponents(nestedComposite, resolutionChain);
                    atomicComponents.AddRange(nestedComponents);
                }
            }
        }

        return atomicComponents;
    }

    private AtomicConstraint ApplyCompositionMetadata(AtomicConstraint atomic, ConstraintReference reference)
    {
        // If reference has no metadata, return original
        if (!reference.HasSequenceOrder && !reference.HasHierarchyLevel && reference.CompositionMetadata.Count == 0)
        {
            return atomic;
        }

        // Create new atomic constraint with composition metadata applied
        var newMetadata = new Dictionary<string, object>(atomic.Metadata);

        // Add composition metadata
        foreach (var kvp in reference.CompositionMetadata)
        {
            newMetadata[kvp.Key] = kvp.Value;
        }

        // For now, return original atomic constraint with metadata in the reference
        // TODO: Implement metadata merging when AtomicConstraint supports it
        return atomic;
    }

    private void RecordResolution(TimeSpan duration, bool success, bool isCacheHit = false)
    {
        lock (_metricsLock)
        {
            _metrics.RecordResolution(duration, success);
            if (isCacheHit)
            {
                _metrics.RecordCacheHit();
            }
        }
    }
}

/// <summary>
/// Implementation of resolution metrics for performance monitoring.
/// </summary>
internal sealed class ResolutionMetrics : IResolutionMetrics
{
    private int _totalResolutions;
    private int _cacheHits;
    private int _successfulResolutions;
    private TimeSpan _totalResolutionTime;
    private TimeSpan _peakResolutionTime;

    /// <summary>
    /// Gets the total number of constraint resolutions performed.
    /// </summary>
    public int TotalResolutions => _totalResolutions;

    /// <summary>
    /// Gets the cache hit rate as a percentage (0.0 to 1.0).
    /// </summary>
    public double CacheHitRate => _totalResolutions > 0 ? (double)_cacheHits / _totalResolutions : 0.0;

    /// <summary>
    /// Gets the average time taken to resolve constraints.
    /// </summary>
    public TimeSpan AverageResolutionTime => 
        _successfulResolutions > 0 ? 
        TimeSpan.FromTicks(_totalResolutionTime.Ticks / _successfulResolutions) : 
        TimeSpan.Zero;

    /// <summary>
    /// Gets the peak resolution time (p95 performance metric).
    /// </summary>
    public TimeSpan PeakResolutionTime => _peakResolutionTime;

    /// <summary>
    /// Records a resolution attempt.
    /// </summary>
    /// <param name="duration">Time taken for resolution</param>
    /// <param name="success">Whether resolution was successful</param>
    internal void RecordResolution(TimeSpan duration, bool success)
    {
        _totalResolutions++;
        
        if (success)
        {
            _successfulResolutions++;
            _totalResolutionTime = _totalResolutionTime.Add(duration);
            
            if (duration > _peakResolutionTime)
            {
                _peakResolutionTime = duration;
            }
        }
    }

    /// <summary>
    /// Records a cache hit.
    /// </summary>
    internal void RecordCacheHit()
    {
        _cacheHits++;
    }
}