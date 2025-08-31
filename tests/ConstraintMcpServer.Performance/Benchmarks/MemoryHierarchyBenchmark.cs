using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Performance.Benchmarks;

/// <summary>
/// Memory usage monitoring benchmarks for complex constraint hierarchies.
/// Ensures sub-100MB memory ceiling and tracks allocation patterns for
/// nested constraint compositions and deep reference chains.
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[MeanColumn]
public class MemoryHierarchyBenchmark
{
    private ConstraintLibrary _flatLibrary = null!;
    private ConstraintLibrary _shallowHierarchy = null!;
    private ConstraintLibrary _deepHierarchy = null!;
    private ConstraintLibrary _wideHierarchy = null!;
    private LibraryConstraintResolver _resolver = null!;

    private ConstraintId[] _flatIds = null!;
    private ConstraintId[] _hierarchicalIds = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _flatLibrary = CreateFlatLibrary(100); // 100 independent atomic constraints
        _shallowHierarchy = CreateShallowHierarchy(50, 3); // 3 levels, 50 constraints per level
        _deepHierarchy = CreateDeepHierarchy(20); // 20 nested levels
        _wideHierarchy = CreateWideHierarchy(100); // 1 composite with 100 references

        _resolver = new LibraryConstraintResolver(_deepHierarchy);

        _flatIds = _flatLibrary.AtomicConstraints.Select(c => c.Id).ToArray();
        _hierarchicalIds = _deepHierarchy.CompositeConstraints.Select(c => c.Id).ToArray();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Library_Memory_Footprint")]
    public int FlatLibrary_ConstraintCount()
    {
        return _flatLibrary.TotalConstraints;
    }

    [Benchmark]
    [BenchmarkCategory("Library_Memory_Footprint")]
    public int ShallowHierarchy_ConstraintCount()
    {
        return _shallowHierarchy.TotalConstraints;
    }

    [Benchmark]
    [BenchmarkCategory("Library_Memory_Footprint")]
    public int DeepHierarchy_ConstraintCount()
    {
        return _deepHierarchy.TotalConstraints;
    }

    [Benchmark]
    [BenchmarkCategory("Library_Memory_Footprint")]
    public int WideHierarchy_ConstraintCount()
    {
        return _wideHierarchy.TotalConstraints;
    }

    [Benchmark]
    [BenchmarkCategory("Reference_Chain_Resolution")]
    public IConstraint?[] ResolveShallowChain()
    {
        CompositeConstraint[] topLevelComposites = _shallowHierarchy.CompositeConstraints
            .Where(c => c.ComponentReferences.Count > 0)
            .Take(5)
            .ToArray();

        return topLevelComposites.Select(c => _resolver.ResolveConstraint(c.Id)).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Reference_Chain_Resolution")]
    public IConstraint? ResolveDeepestConstraint()
    {
        ConstraintId? deepest = _hierarchicalIds.LastOrDefault();
        return deepest != null ? _resolver.ResolveConstraint(deepest) : null;
    }

    [Benchmark]
    [BenchmarkCategory("Reference_Chain_Resolution")]
    public IConstraint? ResolveWideComposite()
    {
        CompositeConstraint? wideComposite = _wideHierarchy.CompositeConstraints.FirstOrDefault();
        return wideComposite != null ? _resolver.ResolveConstraint(wideComposite.Id) : null;
    }

    [Benchmark]
    [BenchmarkCategory("Memory_Allocation_Patterns")]
    public ConstraintReference[] ExtractAllReferences_Shallow()
    {
        var references = new List<ConstraintReference>();
        foreach (CompositeConstraint composite in _shallowHierarchy.CompositeConstraints)
        {
            references.AddRange(composite.ComponentReferences);
        }
        return references.ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Memory_Allocation_Patterns")]
    public ConstraintReference[] ExtractAllReferences_Deep()
    {
        var references = new List<ConstraintReference>();
        foreach (CompositeConstraint composite in _deepHierarchy.CompositeConstraints)
        {
            references.AddRange(composite.ComponentReferences);
        }
        return references.ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Memory_Allocation_Patterns")]
    public ConstraintReference[] ExtractAllReferences_Wide()
    {
        var references = new List<ConstraintReference>();
        foreach (CompositeConstraint composite in _wideHierarchy.CompositeConstraints)
        {
            references.AddRange(composite.ComponentReferences);
        }
        return references.ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Constraint_Graph_Traversal")]
    public Dictionary<ConstraintId, int> CountReferencesPerConstraint_Shallow()
    {
        var refCounts = new Dictionary<ConstraintId, int>();

        foreach (CompositeConstraint composite in _shallowHierarchy.CompositeConstraints)
        {
            foreach (ConstraintReference reference in composite.ComponentReferences)
            {
                refCounts[reference.ConstraintId] = refCounts.GetValueOrDefault(reference.ConstraintId, 0) + 1;
            }
        }

        return refCounts;
    }

    [Benchmark]
    [BenchmarkCategory("Constraint_Graph_Traversal")]
    public Dictionary<ConstraintId, int> CountReferencesPerConstraint_Deep()
    {
        var refCounts = new Dictionary<ConstraintId, int>();

        foreach (CompositeConstraint composite in _deepHierarchy.CompositeConstraints)
        {
            foreach (ConstraintReference reference in composite.ComponentReferences)
            {
                refCounts[reference.ConstraintId] = refCounts.GetValueOrDefault(reference.ConstraintId, 0) + 1;
            }
        }

        return refCounts;
    }

    [Benchmark]
    [BenchmarkCategory("Large_Collection_Operations")]
    public ConstraintId[] GetAllConstraintIds_Combined()
    {
        var allIds = new List<ConstraintId>();

        allIds.AddRange(_flatLibrary.AtomicConstraints.Select(c => c.Id));
        allIds.AddRange(_shallowHierarchy.AtomicConstraints.Select(c => c.Id));
        allIds.AddRange(_shallowHierarchy.CompositeConstraints.Select(c => c.Id));
        allIds.AddRange(_deepHierarchy.CompositeConstraints.Select(c => c.Id));

        return allIds.ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Large_Collection_Operations")]
    public string[] GetAllConstraintTitles_Combined()
    {
        var allTitles = new List<string>();

        allTitles.AddRange(_flatLibrary.AtomicConstraints.Select(c => c.Title));
        allTitles.AddRange(_shallowHierarchy.AtomicConstraints.Select(c => c.Title));
        allTitles.AddRange(_shallowHierarchy.CompositeConstraints.Select(c => c.Title));
        allTitles.AddRange(_deepHierarchy.CompositeConstraints.Select(c => c.Title));

        return allTitles.ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Hierarchy_Creation_Overhead")]
    public static ConstraintLibrary CreateAndPopulateShallowHierarchy()
    {
        return CreateShallowHierarchy(20, 2); // Smaller for benchmark
    }

    [Benchmark]
    [BenchmarkCategory("Hierarchy_Creation_Overhead")]
    public static ConstraintLibrary CreateAndPopulateDeepHierarchy()
    {
        return CreateDeepHierarchy(8); // Smaller for benchmark
    }

    [Benchmark]
    [BenchmarkCategory("Hierarchy_Creation_Overhead")]
    public static ConstraintLibrary CreateAndPopulateWideHierarchy()
    {
        return CreateWideHierarchy(30); // Smaller for benchmark
    }

    private static ConstraintLibrary CreateFlatLibrary(int count)
    {
        var library = new ConstraintLibrary("flat-v1", $"Flat library with {count} atomic constraints");

        for (int i = 0; i < count; i++)
        {
            var atomic = new AtomicConstraint(
                id: new ConstraintId($"atomic.flat.{i:D3}"),
                title: $"Flat Atomic Constraint {i}",
                priority: 0.3 + (i % 20) * 0.02,
                triggers: new TriggerConfiguration(),
                reminders: new[] { $"Flat constraint {i} reminder" }
            );
            library.AddAtomicConstraint(atomic);
        }

        return library;
    }

    private static ConstraintLibrary CreateShallowHierarchy(int constraintsPerLevel, int levels)
    {
        var library = new ConstraintLibrary("shallow-v1", $"Shallow hierarchy: {levels} levels, {constraintsPerLevel} per level");

        // Create base atomic constraints
        for (int i = 0; i < constraintsPerLevel; i++)
        {
            var atomic = new AtomicConstraint(
                id: new ConstraintId($"atomic.shallow.L0.{i:D3}"),
                title: $"Shallow Base Constraint L0-{i}",
                priority: 0.4,
                triggers: new TriggerConfiguration(keywords: new[] { $"level0", $"base{i}" }),
                reminders: new[] { $"Base level constraint {i}" }
            );
            library.AddAtomicConstraint(atomic);
        }

        // Create hierarchical composites
        for (int level = 1; level < levels; level++)
        {
            for (int i = 0; i < constraintsPerLevel; i++)
            {
                var componentRefs = new List<ConstraintReference>();

                // Reference constraints from previous level
                int refsFromPrevLevel = Math.Min(3, constraintsPerLevel);
                for (int j = 0; j < refsFromPrevLevel; j++)
                {
                    int prevIndex = (i + j) % constraintsPerLevel;
                    string prevType = level == 1 ? "atomic" : "composite";
                    componentRefs.Add(new ConstraintReference(
                        new ConstraintId($"{prevType}.shallow.L{level - 1}.{prevIndex:D3}")
                    ));
                }

                var composite = CompositeConstraintBuilder.CreateWithReferences(
                    id: new ConstraintId($"composite.shallow.L{level}.{i:D3}"),
                    title: $"Shallow Composite L{level}-{i}",
                    priority: 0.5 + (level * 0.1),
                    compositionType: (CompositionType)((level % 4) + 1),
                    componentReferences: componentRefs
                );
                library.AddCompositeConstraint(composite);
            }
        }

        return library;
    }

    private static ConstraintLibrary CreateDeepHierarchy(int depth)
    {
        var library = new ConstraintLibrary("deep-v1", $"Deep hierarchy: {depth} levels deep");

        // Create base atomic constraints
        for (int i = 0; i < 5; i++)
        {
            var atomic = new AtomicConstraint(
                id: new ConstraintId($"atomic.deep.base.{i:D3}"),
                title: $"Deep Base Constraint {i}",
                priority: 0.4,
                triggers: new TriggerConfiguration(keywords: new[] { "deep", $"base{i}" }),
                reminders: new[] { $"Deep base constraint {i}" }
            );
            library.AddAtomicConstraint(atomic);
        }

        // Create deep nested chain
        for (int level = 1; level <= depth; level++)
        {
            var componentRefs = new List<ConstraintReference>();

            if (level == 1)
            {
                // First level references base atomics
                componentRefs.Add(new ConstraintReference(new ConstraintId("atomic.deep.base.000")));
                componentRefs.Add(new ConstraintReference(new ConstraintId("atomic.deep.base.001")));
            }
            else
            {
                // Higher levels reference previous level composite
                componentRefs.Add(new ConstraintReference(new ConstraintId($"composite.deep.L{level - 1}")));

                // Also reference one base atomic for complexity
                componentRefs.Add(new ConstraintReference(new ConstraintId($"atomic.deep.base.{(level % 5):D3}")));
            }

            var composite = CompositeConstraintBuilder.CreateWithReferences(
                id: new ConstraintId($"composite.deep.L{level}"),
                title: $"Deep Composite Level {level}",
                priority: 0.6 + (level * 0.02),
                compositionType: CompositionType.Sequential,
                componentReferences: componentRefs
            );
            library.AddCompositeConstraint(composite);
        }

        return library;
    }

    private static ConstraintLibrary CreateWideHierarchy(int componentCount)
    {
        var library = new ConstraintLibrary("wide-v1", $"Wide hierarchy: {componentCount} components");

        // Create many atomic constraints
        for (int i = 0; i < componentCount; i++)
        {
            var atomic = new AtomicConstraint(
                id: new ConstraintId($"atomic.wide.{i:D3}"),
                title: $"Wide Atomic Constraint {i}",
                priority: 0.3 + (i % 30) * 0.01,
                triggers: new TriggerConfiguration(keywords: new[] { "wide", $"component{i}" }),
                reminders: new[] { $"Wide component {i}" }
            );
            library.AddAtomicConstraint(atomic);
        }

        // Create wide composite that references all atomics
        ConstraintReference[] allComponentRefs = Enumerable.Range(0, componentCount)
            .Select(i => new ConstraintReference(new ConstraintId($"atomic.wide.{i:D3}")))
            .ToArray();

        var wideComposite = CompositeConstraintBuilder.CreateWithReferences(
            id: new ConstraintId("composite.wide.all"),
            title: $"Wide Composite ({componentCount} components)",
            priority: 0.9,
            compositionType: CompositionType.Parallel,
            componentReferences: allComponentRefs
        );
        library.AddCompositeConstraint(wideComposite);

        return library;
    }
}
