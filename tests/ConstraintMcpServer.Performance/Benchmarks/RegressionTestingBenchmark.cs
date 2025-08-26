using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Performance.Benchmarks;

/// <summary>
/// Regression testing benchmarks for constraint evaluation chains.
/// Monitors performance stability across different evaluation patterns
/// and detects performance regressions in constraint resolution workflows.
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[MeanColumn]
public class RegressionTestingBenchmark
{
    private ConstraintLibrary _baselineLibrary = null!;
    private LibraryConstraintResolver _baselineResolver = null!;

    // Test patterns that represent common evaluation chains
    private ConstraintId[] _singleStepEvaluation = null!;
    private ConstraintId[] _shortChainEvaluation = null!;
    private ConstraintId[] _longChainEvaluation = null!;
    private ConstraintId[] _complexWebEvaluation = null!;

    // Baseline performance scenarios
    private TriggerContext _basicTriggerContext = null!;
    private TriggerContext _complexTriggerContext = null!;
    private TriggerConfiguration[] _variedTriggerConfigs = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _baselineLibrary = CreateRegressionTestLibrary();
        _baselineResolver = new LibraryConstraintResolver(_baselineLibrary);

        // Set up evaluation chains
        _singleStepEvaluation = CreateSingleStepEvaluationChain();
        _shortChainEvaluation = CreateShortChainEvaluation();
        _longChainEvaluation = CreateLongChainEvaluation();
        _complexWebEvaluation = CreateComplexWebEvaluation();

        // Set up trigger contexts
        _basicTriggerContext = new TriggerContext(
            keywords: new[] { "test", "basic", "simple" },
            filePath: "/test/Basic.cs",
            contextType: "testing"
        );

        _complexTriggerContext = new TriggerContext(
            keywords: new[] { "complex", "integration", "architecture", "performance", "validation" },
            filePath: "/src/complex/integration/PerformanceArchitecture.cs",
            contextType: "complex_integration"
        );

        _variedTriggerConfigs = CreateVariedTriggerConfigurations();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Evaluation_Chain_Regression")]
    public IConstraint?[] SingleStep_EvaluationChain()
    {
        return _singleStepEvaluation.Select(id => _baselineResolver.ResolveConstraint(id)).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation_Chain_Regression")]
    public IConstraint?[] ShortChain_EvaluationChain()
    {
        return _shortChainEvaluation.Select(id => _baselineResolver.ResolveConstraint(id)).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation_Chain_Regression")]
    public IConstraint?[] LongChain_EvaluationChain()
    {
        return _longChainEvaluation.Select(id => _baselineResolver.ResolveConstraint(id)).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation_Chain_Regression")]
    public IConstraint?[] ComplexWeb_EvaluationChain()
    {
        return _complexWebEvaluation.Select(id => _baselineResolver.ResolveConstraint(id)).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Trigger_Evaluation_Regression")]
    public double[] BasicTrigger_RelevanceScores()
    {
        return _variedTriggerConfigs.Select(config =>
            _basicTriggerContext.CalculateRelevanceScore(config)).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Trigger_Evaluation_Regression")]
    public double[] ComplexTrigger_RelevanceScores()
    {
        return _variedTriggerConfigs.Select(config =>
            _complexTriggerContext.CalculateRelevanceScore(config)).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Mixed_Workload_Regression")]
    public (IConstraint?[], double[]) CombinedEvaluation_BasicWorkload()
    {
        IConstraint[] constraints = _shortChainEvaluation.Take(5).Select(id =>
            _baselineResolver.ResolveConstraint(id)).ToArray();

        double[] scores = _variedTriggerConfigs.Take(5).Select(config =>
            _basicTriggerContext.CalculateRelevanceScore(config)).ToArray();

        return (constraints, scores);
    }

    [Benchmark]
    [BenchmarkCategory("Mixed_Workload_Regression")]
    public (IConstraint?[], double[]) CombinedEvaluation_ComplexWorkload()
    {
        IConstraint[] constraints = _complexWebEvaluation.Take(8).Select(id =>
            _baselineResolver.ResolveConstraint(id)).ToArray();

        double[] scores = _variedTriggerConfigs.Select(config =>
            _complexTriggerContext.CalculateRelevanceScore(config)).ToArray();

        return (constraints, scores);
    }

    [Benchmark]
    [BenchmarkCategory("Composition_Pattern_Regression")]
    public CompositeConstraint[] ResolveSequentialComposites()
    {
        return _baselineLibrary.CompositeConstraints
            .Where(c => c.CompositionType == CompositionType.Sequential)
            .ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Composition_Pattern_Regression")]
    public CompositeConstraint[] ResolveParallelComposites()
    {
        return _baselineLibrary.CompositeConstraints
            .Where(c => c.CompositionType == CompositionType.Parallel)
            .ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Composition_Pattern_Regression")]
    public CompositeConstraint[] ResolveHierarchicalComposites()
    {
        return _baselineLibrary.CompositeConstraints
            .Where(c => c.CompositionType == CompositionType.Hierarchical)
            .ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Library_Operations_Regression")]
    public (int atomicCount, int compositeCount, int totalCount) LibraryBasicStats()
    {
        return (
            _baselineLibrary.AtomicConstraints.Count,
            _baselineLibrary.CompositeConstraints.Count,
            _baselineLibrary.TotalConstraints
        );
    }

    [Benchmark]
    [BenchmarkCategory("Library_Operations_Regression")]
    public Dictionary<CompositionType, int> CompositionTypeDistribution()
    {
        return _baselineLibrary.CompositeConstraints
            .GroupBy(c => c.CompositionType)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    [Benchmark]
    [BenchmarkCategory("Reference_Resolution_Regression")]
    public int TotalReferencesInLibrary()
    {
        return _baselineLibrary.CompositeConstraints
            .SelectMany(c => c.ComponentReferences)
            .Count();
    }

    [Benchmark]
    [BenchmarkCategory("Reference_Resolution_Regression")]
    public Dictionary<ConstraintId, int> ReferenceFrequency()
    {
        var frequency = new Dictionary<ConstraintId, int>();

        foreach (CompositeConstraint composite in _baselineLibrary.CompositeConstraints)
        {
            foreach (ConstraintReference reference in composite.ComponentReferences)
            {
                frequency[reference.ConstraintId] =
                    frequency.GetValueOrDefault(reference.ConstraintId, 0) + 1;
            }
        }

        return frequency;
    }

    [Benchmark]
    [BenchmarkCategory("Performance_Threshold_Validation")]
    public bool ValidateP95LatencyThreshold()
    {
        // Simulate validation that p95 latency is under 50ms
        var latencies = new List<double>();

        foreach (ConstraintId id in _shortChainEvaluation)
        {
            DateTimeOffset start = DateTimeOffset.UtcNow;
            _baselineResolver.ResolveConstraint(id);
            double elapsed = (DateTimeOffset.UtcNow - start).TotalMilliseconds;
            latencies.Add(elapsed);
        }

        if (latencies.Count == 0)
        {
            return false;
        }

        latencies.Sort();
        int p95Index = (int)Math.Ceiling(latencies.Count * 0.95) - 1;
        double p95Latency = latencies[p95Index];

        return p95Latency <= 50.0; // 50ms threshold
    }

    [Benchmark]
    [BenchmarkCategory("Performance_Threshold_Validation")]
    public bool ValidateMemoryUsageThreshold()
    {
        // Simulate memory usage validation
        long beforeGC = GC.GetTotalMemory(true);

        // Perform memory-intensive operations
        var results = new List<IConstraint?>();
        foreach (ConstraintId id in _complexWebEvaluation)
        {
            results.Add(_baselineResolver.ResolveConstraint(id));
        }

        long afterGC = GC.GetTotalMemory(false);
        long memoryDelta = afterGC - beforeGC;

        // Keep results alive to prevent optimization
        int totalResults = results.Count(r => r != null);

        // Check if memory delta is reasonable (less than 10MB for this operation)
        return memoryDelta < 10 * 1024 * 1024 && totalResults > 0;
    }

    private static ConstraintLibrary CreateRegressionTestLibrary()
    {
        var library = new ConstraintLibrary("regression-v1", "Regression testing library with varied patterns");

        // Create base atomic constraints
        for (int i = 0; i < 30; i++)
        {
            var atomic = new AtomicConstraint(
                id: new ConstraintId($"atomic.regression.{i:D3}"),
                title: $"Regression Test Atomic {i}",
                priority: 0.3 + (i % 15) * 0.03,
                triggers: new TriggerConfiguration(
                    keywords: new[] { $"atomic{i}", "regression", "test" }
                ),
                reminders: new[] { $"Regression atomic constraint {i}" }
            );
            library.AddAtomicConstraint(atomic);
        }

        // Create composites with different composition patterns
        CompositionType[] compositionTypes = new[] {
            CompositionType.Sequential,
            CompositionType.Parallel,
            CompositionType.Hierarchical,
            CompositionType.Progressive,
            CompositionType.Layered
        };

        for (int i = 0; i < 25; i++)
        {
            int componentCount = 2 + (i % 4); // 2-5 components
            var componentRefs = new List<ConstraintReference>();

            for (int j = 0; j < componentCount; j++)
            {
                int atomicIndex = (i * 2 + j) % 30;
                componentRefs.Add(new ConstraintReference(
                    new ConstraintId($"atomic.regression.{atomicIndex:D3}")
                ));
            }

            var composite = new CompositeConstraint(
                id: new ConstraintId($"composite.regression.{i:D3}"),
                title: $"Regression Test Composite {i}",
                priority: 0.6 + (i % 8) * 0.03,
                compositionType: compositionTypes[i % compositionTypes.Length],
                componentReferences: componentRefs
            );
            library.AddCompositeConstraint(composite);
        }

        return library;
    }

    private ConstraintId[] CreateSingleStepEvaluationChain()
    {
        // Simple chain: just atomic constraints
        return _baselineLibrary.AtomicConstraints.Take(5).Select(c => c.Id).ToArray();
    }

    private ConstraintId[] CreateShortChainEvaluation()
    {
        // Short chain: mix of atomics and composites
        var ids = new List<ConstraintId>();
        ids.AddRange(_baselineLibrary.AtomicConstraints.Take(3).Select(c => c.Id));
        ids.AddRange(_baselineLibrary.CompositeConstraints.Take(2).Select(c => c.Id));
        return ids.ToArray();
    }

    private ConstraintId[] CreateLongChainEvaluation()
    {
        // Long chain: many composites with dependencies
        return _baselineLibrary.CompositeConstraints.Take(10).Select(c => c.Id).ToArray();
    }

    private ConstraintId[] CreateComplexWebEvaluation()
    {
        // Complex web: all composites (represents complex dependency web)
        return _baselineLibrary.CompositeConstraints.Select(c => c.Id).ToArray();
    }

    private static TriggerConfiguration[] CreateVariedTriggerConfigurations()
    {
        return new[]
        {
            new TriggerConfiguration(keywords: new[] { "simple" }),
            new TriggerConfiguration(keywords: new[] { "complex", "integration" }),
            new TriggerConfiguration(
                keywords: new[] { "performance" },
                filePatterns: new[] { "*.cs" }
            ),
            new TriggerConfiguration(
                keywords: new[] { "test", "validation" },
                contextPatterns: new[] { "testing" }
            ),
            new TriggerConfiguration(
                keywords: new[] { "architecture", "design" },
                filePatterns: new[] { "*Architecture*" },
                contextPatterns: new[] { "complex" }
            ),
            new TriggerConfiguration(
                keywords: new[] { "regression" },
                antiPatterns: new[] { "skip", "ignore" }
            )
        };
    }
}
