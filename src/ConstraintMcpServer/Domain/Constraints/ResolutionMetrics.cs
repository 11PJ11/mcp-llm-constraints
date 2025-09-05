using System;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Tracks performance metrics for constraint resolution operations.
/// Implements thread-safe measurement collection for monitoring and optimization.
/// Implements CUPID properties: Predictable, Domain-based.
/// </summary>
internal sealed class ResolutionMetrics : IResolutionMetrics
{
    private int _totalResolutions;
    private int _cacheHits;
    private int _successfulResolutions;
    private TimeSpan _totalResolutionTime;
    private TimeSpan _peakResolutionTime;
    private readonly object _lock = new();

    /// <summary>
    /// Gets the total number of constraint resolutions performed.
    /// </summary>
    public int TotalResolutions
    {
        get
        {
            lock (_lock)
            {
                return _totalResolutions;
            }
        }
    }

    /// <summary>
    /// Gets the cache hit rate as a percentage (0.0 to 1.0).
    /// </summary>
    public double CacheHitRate
    {
        get
        {
            lock (_lock)
            {
                return _totalResolutions > 0 ? (double)_cacheHits / _totalResolutions : 0.0;
            }
        }
    }

    /// <summary>
    /// Gets the average time taken to resolve constraints.
    /// </summary>
    public TimeSpan AverageResolutionTime
    {
        get
        {
            lock (_lock)
            {
                return _successfulResolutions > 0 ?
                    TimeSpan.FromTicks(_totalResolutionTime.Ticks / _successfulResolutions) :
                    TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// Gets the peak resolution time (p95 performance metric).
    /// </summary>
    public TimeSpan PeakResolutionTime
    {
        get
        {
            lock (_lock)
            {
                return _peakResolutionTime;
            }
        }
    }

    /// <summary>
    /// Records a constraint resolution attempt with timing and success information.
    /// </summary>
    /// <param name="duration">Time taken for resolution</param>
    /// <param name="success">Whether resolution was successful</param>
    internal void RecordResolution(TimeSpan duration, bool success)
    {
        lock (_lock)
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
    }

    /// <summary>
    /// Records a successful cache hit for performance tracking.
    /// </summary>
    internal void RecordCacheHit()
    {
        lock (_lock)
        {
            _cacheHits++;
        }
    }

    /// <summary>
    /// Resets all metrics to initial state.
    /// Useful for testing and metric collection period boundaries.
    /// </summary>
    internal void Reset()
    {
        lock (_lock)
        {
            _totalResolutions = 0;
            _cacheHits = 0;
            _successfulResolutions = 0;
            _totalResolutionTime = TimeSpan.Zero;
            _peakResolutionTime = TimeSpan.Zero;
        }
    }
}
