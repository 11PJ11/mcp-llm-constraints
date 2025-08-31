using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Performance.Benchmarks;

/// <summary>
/// Simplified constraint library performance benchmarks using the actual API.
/// Focuses on critical operations for sub-50ms p95 latency requirements.
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[MeanColumn]
public class SimplifiedConstraintLibraryBenchmark
{
    private ConstraintLibrary _smallLibrary = null!;
    private ConstraintLibrary _mediumLibrary = null!;
    private ConstraintLibrary _largeLibrary = null!;
    private LibraryConstraintResolver _resolver = null!;

    private ConstraintId[] _atomicIds = null!;
    private ConstraintId[] _compositeIds = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _smallLibrary = CreateLibrary(10, 5);  // 10 atomic, 5 composite
        _mediumLibrary = CreateLibrary(50, 25); // 50 atomic, 25 composite
        _largeLibrary = CreateLibrary(200, 100); // 200 atomic, 100 composite

        _resolver = new LibraryConstraintResolver(_mediumLibrary);

        _atomicIds = _mediumLibrary.AtomicConstraints.Select(c => c.Id).ToArray();
        _compositeIds = _mediumLibrary.CompositeConstraints.Select(c => c.Id).ToArray();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Library_Operations")]
    public int SmallLibrary_TotalConstraints()
    {
        return _smallLibrary.TotalConstraints;
    }

    [Benchmark]
    [BenchmarkCategory("Library_Operations")]
    public int MediumLibrary_TotalConstraints()
    {
        return _mediumLibrary.TotalConstraints;
    }

    [Benchmark]
    [BenchmarkCategory("Library_Operations")]
    public int LargeLibrary_TotalConstraints()
    {
        return _largeLibrary.TotalConstraints;
    }

    [Benchmark]
    [BenchmarkCategory("Constraint_Resolution")]
    public IConstraint? ResolveAtomicConstraint()
    {
        ConstraintId id = _atomicIds[0];
        return _resolver.ResolveConstraint(id);
    }

    [Benchmark]
    [BenchmarkCategory("Constraint_Resolution")]
    public IConstraint? ResolveCompositeConstraint()
    {
        ConstraintId id = _compositeIds[0];
        return _resolver.ResolveConstraint(id);
    }

    [Benchmark]
    [BenchmarkCategory("Constraint_Resolution")]
    public IConstraint? ResolveNonExistentConstraint()
    {
        var id = new ConstraintId("nonexistent.constraint");
        return _resolver.ResolveConstraint(id);
    }

    [Benchmark]
    [BenchmarkCategory("Batch_Resolution")]
    public IConstraint?[] ResolveBatch_5Constraints()
    {
        return _atomicIds.Take(5).Select(id => _resolver.ResolveConstraint(id)).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Batch_Resolution")]
    public IConstraint?[] ResolveBatch_10Constraints()
    {
        return _atomicIds.Take(10).Select(id => _resolver.ResolveConstraint(id)).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Library_Creation")]
    public static ConstraintLibrary CreateSmallLibrary()
    {
        return CreateLibrary(5, 2);
    }

    [Benchmark]
    [BenchmarkCategory("Library_Creation")]
    public static ConstraintLibrary CreateMediumLibrary()
    {
        return CreateLibrary(20, 10);
    }

    [Benchmark]
    [BenchmarkCategory("Collection_Operations")]
    public AtomicConstraint[] GetAllAtomicConstraints()
    {
        return _mediumLibrary.AtomicConstraints.ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Collection_Operations")]
    public CompositeConstraint[] GetAllCompositeConstraints()
    {
        return _mediumLibrary.CompositeConstraints.ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Validation")]
    public bool ValidateLibraryIntegrity()
    {
        // Simple validation: check that all composites reference existing constraints
        foreach (CompositeConstraint composite in _mediumLibrary.CompositeConstraints)
        {
            foreach (ConstraintReference reference in composite.ComponentReferences)
            {
                IConstraint resolved = _resolver.ResolveConstraint(reference.ConstraintId);
                if (resolved == null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static ConstraintLibrary CreateLibrary(int atomicCount, int compositeCount)
    {
        var library = new ConstraintLibrary($"benchmark-v1", $"Benchmark library with {atomicCount}+{compositeCount} constraints");

        // Create atomic constraints
        for (int i = 0; i < atomicCount; i++)
        {
            var atomic = new AtomicConstraint(
                id: new ConstraintId($"atomic.benchmark.{i:D3}"),
                title: $"Benchmark Atomic Constraint {i}",
                priority: 0.5 + (i % 10) * 0.05, // Varied priorities
                triggers: new TriggerConfiguration(),
                reminders: new[] { $"Reminder for atomic constraint {i}" }
            );
            library.AddAtomicConstraint(atomic);
        }

        // Create composite constraints with references to atomics
        for (int i = 0; i < compositeCount; i++)
        {
            int componentCount = 2 + (i % 3); // 2-4 components per composite
            var componentRefs = new List<ConstraintReference>();

            for (int j = 0; j < componentCount; j++)
            {
                int atomicIndex = (i + j) % atomicCount;
                componentRefs.Add(new ConstraintReference(new ConstraintId($"atomic.benchmark.{atomicIndex:D3}")));
            }

            var composite = CompositeConstraintBuilder.CreateWithReferences(
                id: new ConstraintId($"composite.benchmark.{i:D3}"),
                title: $"Benchmark Composite Constraint {i}",
                priority: 0.7 + (i % 5) * 0.05, // Higher priorities for composites
                compositionType: (CompositionType)((i % 4) + 1), // Cycle through composition types
                componentReferences: componentRefs
            );
            library.AddCompositeConstraint(composite);
        }

        return library;
    }
}
