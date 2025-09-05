using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Tests.Steps;

/// <summary>
/// Level 2 Refactoring: Extract performance metrics calculation logic.
/// Eliminates duplication from McpServerSteps and provides focused responsibility.
/// </summary>
internal sealed class PerformanceMetricsCalculator
{
    private readonly List<long> _metrics = new();

    // Performance budgets (constants from main class)
    private const int P95_LATENCY_BUDGET_MS = 50;
    private const int P99_LATENCY_BUDGET_MS = 100;

    public void AddMetric(long latencyMs)
    {
        lock (_metrics)
        {
            _metrics.Add(latencyMs);
        }
    }

    public void AddMetrics(IEnumerable<long> metrics)
    {
        lock (_metrics)
        {
            _metrics.AddRange(metrics);
        }
    }

    public void Clear()
    {
        lock (_metrics)
        {
            _metrics.Clear();
        }
    }

    public long CalculatePercentile(double percentile)
    {
        List<long> sortedMetrics = GetSortedMetricsCopy();
        int percentileIndex = (int)Math.Ceiling(sortedMetrics.Count * percentile) - 1;
        return sortedMetrics[percentileIndex];
    }

    public void ValidateP95Budget()
    {
        long p95Latency = CalculatePercentile(0.95);
        ValidateLatencyBudget(p95Latency, P95_LATENCY_BUDGET_MS, "P95");
        Console.WriteLine($"✅ P95 latency: {p95Latency}ms (within {P95_LATENCY_BUDGET_MS}ms budget)");
    }

    public void ValidateP99Budget()
    {
        long p99Latency = CalculatePercentile(0.99);
        ValidateLatencyBudget(p99Latency, P99_LATENCY_BUDGET_MS, "P99");
        Console.WriteLine($"✅ P99 latency: {p99Latency}ms (within {P99_LATENCY_BUDGET_MS}ms budget)");
    }

    public PerformanceStats CalculateStats()
    {
        List<long> metricsCopy = GetSortedMetricsCopy();

        return new PerformanceStats
        {
            Average = _metrics.Average(),
            Min = metricsCopy.First(),
            Max = metricsCopy.Last(),
            Count = metricsCopy.Count,
            P95 = CalculatePercentile(0.95),
            P99 = CalculatePercentile(0.99)
        };
    }

    private List<long> GetSortedMetricsCopy()
    {
        lock (_metrics)
        {
            if (_metrics.Count == 0)
            {
                throw new InvalidOperationException("No performance metrics collected");
            }
            return _metrics.OrderBy(x => x).ToList();
        }
    }

    private static void ValidateLatencyBudget(long actualLatency, int budgetMs, string percentileName)
    {
        if (actualLatency > budgetMs)
        {
            throw new InvalidOperationException(
                $"{percentileName} latency {actualLatency}ms exceeds budget of {budgetMs}ms.");
        }
    }
}

/// <summary>
/// Performance statistics container for metrics analysis.
/// </summary>
internal sealed record PerformanceStats
{
    public double Average { get; init; }
    public long Min { get; init; }
    public long Max { get; init; }
    public int Count { get; init; }
    public long P95 { get; init; }
    public long P99 { get; init; }
}
