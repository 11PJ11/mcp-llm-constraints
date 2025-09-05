using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConstraintMcpServer.Tests.Steps;

/// <summary>
/// Level 3 Refactoring: Extract performance validation logic.
/// Focused responsibility for performance metrics collection and budget validation.
/// Follows CUPID properties: Composable, Unix Philosophy, Predictable, Idiomatic, Domain-based.
/// </summary>
internal sealed class PerformanceValidationSteps
{
    private readonly PerformanceMetricsCalculator _calculator = new();

    /// <summary>
    /// Business-focused step: Add mock performance metrics for testing scenarios
    /// </summary>
    public void AddMockPerformanceMetrics(IEnumerable<long> metrics)
    {
        _calculator.AddMetrics(metrics);
    }

    /// <summary>
    /// Business-focused step: Record latency metric from operation
    /// </summary>
    public void RecordLatencyMetric(long latencyMs)
    {
        _calculator.AddMetric(latencyMs);
    }

    /// <summary>
    /// Business-focused step: Record latency from stopwatch measurement
    /// </summary>
    public void RecordLatencyFromStopwatch(Stopwatch stopwatch)
    {
        RecordLatencyMetric(stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Business-focused step: Verify P95 latency meets performance budget (≤ 50ms)
    /// </summary>
    public void ValidateP95LatencyBudget()
    {
        _calculator.ValidateP95Budget();
    }

    /// <summary>
    /// Business-focused step: Verify P99 latency meets performance budget (≤ 100ms)
    /// </summary>
    public void ValidateP99LatencyBudget()
    {
        _calculator.ValidateP99Budget();
    }

    /// <summary>
    /// Business-focused step: Verify no performance regression detected
    /// </summary>
    public void ValidateNoPerformanceRegression()
    {
        var stats = _calculator.CalculateStats();

        // Basic regression detection - allow for startup costs, focus on P99 rather than outliers
        // If P99 exceeds budget significantly (2x), that indicates a real performance issue
        const int RegressionThresholdMs = 200; // 2x the P99 budget of 100ms

        if (stats.P99 > RegressionThresholdMs)
        {
            throw new InvalidOperationException($"Performance regression detected. P99 latency {stats.P99}ms exceeds acceptable threshold of {RegressionThresholdMs}ms");
        }

        // Performance is consistent
        Console.WriteLine($"✅ Performance consistent - Avg: {stats.Average:F2}ms, Min: {stats.Min}ms, Max: {stats.Max}ms");
    }

    /// <summary>
    /// Business-focused step: Verify latency is within specified budget
    /// </summary>
    public void ValidateLatencyBudget(long maxLatencyMs = 50)
    {
        var stats = _calculator.CalculateStats();

        if (stats.P95 > maxLatencyMs)
        {
            throw new InvalidOperationException($"Latency budget exceeded: P95 {stats.P95}ms > {maxLatencyMs}ms");
        }
    }

    /// <summary>
    /// Business-focused step: Clear all collected metrics (for test isolation)
    /// </summary>
    public void ClearMetrics()
    {
        _calculator.Clear();
    }

    /// <summary>
    /// Business-focused step: Get performance statistics summary
    /// </summary>
    public PerformanceStats GetPerformanceStats()
    {
        return _calculator.CalculateStats();
    }

    /// <summary>
    /// Business-focused step: Validate performance meets all budgets and thresholds
    /// </summary>
    public void ValidateAllPerformanceBudgets()
    {
        ValidateP95LatencyBudget();
        ValidateP99LatencyBudget();
        ValidateNoPerformanceRegression();
    }
}
