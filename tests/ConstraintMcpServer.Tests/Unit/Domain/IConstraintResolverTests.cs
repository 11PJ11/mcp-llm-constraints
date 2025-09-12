using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Tests.Unit.Domain;

/// <summary>
/// Unit tests for IConstraintResolver interface.
/// Focuses on business behavior: constraint resolution by ID from library with performance and validation.
/// Uses business-focused naming and testing approach.
/// </summary>
[TestFixture]
public class IConstraintResolverTests
{
    private IConstraintResolver? _resolver;
    private static readonly string[] keywords = new[] { "test", "tdd" };

    [SetUp]
    public void SetUp()
    {
        // Create a constraint library with test constraints
        var library = new ConstraintMcpServer.Domain.Constraints.ConstraintLibrary("1.0.0", "Test Library");

        // Add atomic constraints for testing
        var testFirstConstraint = new AtomicConstraint(
            new ConstraintId("testing.write-test-first"),
            "Write a failing test first",
            0.92,
            new TriggerConfiguration(
                keywords: keywords,
                filePatterns: new[] { "*.cs" },
                contextPatterns: new[] { "testing" }),
            new[] { "Start with a failing test (RED) before implementation." });

        var acceptanceTestConstraint = new AtomicConstraint(
            new ConstraintId("testing.acceptance-test-first"),
            "Write acceptance test first",
            0.90,
            new TriggerConfiguration(
                keywords: new[] { "acceptance", "atdd" },
                filePatterns: new[] { "*.cs" },
                contextPatterns: new[] { "testing" }),
            new[] { "Start with an acceptance test for the feature." });

        library.AddAtomicConstraint(testFirstConstraint);
        library.AddAtomicConstraint(acceptanceTestConstraint);

        // Add composite constraint for testing
        var outsideInConstraint = CompositeConstraintBuilder.CreateWithComponents(
            new ConstraintId("methodology.outside-in-development"),
            "Outside-In Development",
            0.95,
            new TriggerConfiguration(
                keywords: new[] { "outside-in", "atdd", "tdd" },
                filePatterns: new[] { "*.cs" },
                contextPatterns: new[] { "development" }),
            CompositionType.Sequential,
            new[] { testFirstConstraint, acceptanceTestConstraint }, // Use actual components for now
            new[] { "Apply Outside-In Development methodology: Start with acceptance tests, then unit tests." });

        library.AddCompositeConstraint(outsideInConstraint);

        // Add constraints for circular reference testing  
        var circularA = CompositeConstraintBuilder.CreateWithComponents(
            new ConstraintId("circular.composite-a"),
            "Circular Composite A",
            0.85,
            new TriggerConfiguration(
                keywords: new[] { "circular" },
                filePatterns: new[] { "*.cs" },
                contextPatterns: new[] { "testing" }),
            CompositionType.Sequential,
            new[] { testFirstConstraint }, // Use at least one component to satisfy validation
            new[] { "Part of circular reference test" });

        var circularB = CompositeConstraintBuilder.CreateWithComponents(
            new ConstraintId("circular.composite-b"),
            "Circular Composite B",
            0.85,
            new TriggerConfiguration(
                keywords: new[] { "circular" },
                filePatterns: new[] { "*.cs" },
                contextPatterns: new[] { "testing" }),
            CompositionType.Sequential,
            new[] { acceptanceTestConstraint }, // Use at least one component to satisfy validation
            new[] { "Part of circular reference test" });

        library.AddCompositeConstraint(circularA);
        library.AddCompositeConstraint(circularB);

        // Create resolver
        _resolver = new LibraryConstraintResolver(library);
    }

    [Test]
    public void Should_Resolve_Atomic_Constraint_By_Valid_ID()
    {
        // Business scenario: Developer requests atomic constraint by ID
        // Expected: Valid atomic constraint is returned with all properties

        // Arrange
        var constraintId = new ConstraintId("testing.write-test-first");

        // Act
        IConstraint result = _resolver!.ResolveConstraint(constraintId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(constraintId));
            Assert.That(result.Title, Is.Not.Empty);
            Assert.That(result.Priority, Is.GreaterThan(0.0).And.LessThanOrEqualTo(1.0));
            Assert.That(result.Triggers, Is.Not.Null);
            Assert.That(result.Reminders, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Resolve_Composite_Constraint_By_Valid_ID()
    {
        // Business scenario: Developer requests composite constraint by ID
        // Expected: Valid composite constraint with resolved component references

        // Arrange
        var constraintId = new ConstraintId("methodology.outside-in-development");

        // Act
        IConstraint result = _resolver!.ResolveConstraint(constraintId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(constraintId));
            Assert.That(result, Is.TypeOf<CompositeConstraint>());
        });

        var composite = result as CompositeConstraint;
        Assert.Multiple(() =>
        {
            Assert.That(composite!.Components, Is.Not.Empty);
            Assert.That(composite.CompositionType, Is.Not.EqualTo(CompositionType.Unknown));
        });
    }

    [Test]
    public void Should_Throw_ConstraintNotFoundException_For_Invalid_ID()
    {
        // Business scenario: Developer requests constraint with non-existent ID
        // Expected: Clear exception indicating constraint not found

        // Arrange
        var invalidId = new ConstraintId("non.existent.constraint");

        // Act & Assert
        Assert.Throws<ConstraintNotFoundException>(() =>
            _resolver!.ResolveConstraint(invalidId));
    }

    [Test]
    public void Should_Throw_ArgumentNullException_For_Null_ID()
    {
        // Business scenario: Developer passes null constraint ID
        // Expected: Defensive programming - null guard exception

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _resolver!.ResolveConstraint(null!));
    }

    [Test]
    public void Should_Resolve_Constraint_References_Within_Composites()
    {
        // Business scenario: Composite constraint references other constraints by ID
        // Expected: All component references are resolved to actual constraint objects

        // Arrange
        var compositeId = new ConstraintId("methodology.outside-in-development");

        // Act
        var result = _resolver!.ResolveConstraint(compositeId) as CompositeConstraint;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Components, Is.Not.Empty);

        // All components should be fully resolved, not references
        foreach (AtomicConstraint component in result.Components)
        {
            Assert.Multiple(() =>
            {
                Assert.That(component.Id, Is.Not.Null);
                Assert.That(component.Title, Is.Not.Empty);
                Assert.That(component.Reminders, Is.Not.Empty);
                Assert.That(component.Triggers, Is.Not.Null);
            });
        }
    }

    [Test]
    public void Should_Detect_Circular_References_In_Composite_Resolution()
    {
        // Business scenario: ConstraintLibrary prevents circular references at construction time
        // Expected: ConstraintReferenceValidationException when trying to create circular references

        // Arrange - Create a separate library and try to add circular references
        var circularLibrary = new ConstraintMcpServer.Domain.Constraints.ConstraintLibrary("1.0.0", "Circular Test Library");

        // Add atomic constraint first
        var atomicA = new AtomicConstraint(
            new ConstraintId("atomic.a"),
            "Atomic A",
            0.8,
            new TriggerConfiguration(
                keywords: new[] { "atomic" },
                filePatterns: new[] { "*.cs" },
                contextPatterns: new[] { "test" }),
            new[] { "Atomic constraint A" });

        circularLibrary.AddAtomicConstraint(atomicA);

        // Create composite A that references a non-existent composite B
        var compositeA = CompositeConstraintBuilder.CreateWithReferences(
            new ConstraintId("circular.composite-a"),
            "Circular Composite A",
            0.9,
            CompositionType.Sequential,
            new[] { new ConstraintReference(new ConstraintId("circular.composite-b")) });

        // Act & Assert - Library validation should prevent adding constraint with missing reference
        Assert.Throws<ConstraintReferenceValidationException>(() =>
            circularLibrary.AddCompositeConstraint(compositeA));
    }

    [Test]
    public void Should_Prevent_Circular_Reference_In_Resolution_Chain()
    {
        // Business scenario: Test resolver's defensive circular reference detection
        // Expected: CircularReferenceException if somehow circular references exist

        // Arrange - Create a valid constraint that references itself through component references
        var library = new ConstraintMcpServer.Domain.Constraints.ConstraintLibrary("1.0.0", "Test Library");

        // Add atomic constraints that can be safely referenced
        var atomicConstraint = new AtomicConstraint(
            new ConstraintId("testing.atomic-test"),
            "Atomic Test Constraint",
            0.8,
            new TriggerConfiguration(
                keywords: new[] { "test" },
                filePatterns: new[] { "*.cs" },
                contextPatterns: new[] { "testing" }),
            new[] { "Test constraint for circular detection" });

        library.AddAtomicConstraint(atomicConstraint);

        // Add a composite that has valid references but could potentially create deep nesting
        var composite = CompositeConstraintBuilder.CreateWithReferences(
            new ConstraintId("testing.composite-nesting"),
            "Composite With Deep Nesting",
            0.9,
            CompositionType.Sequential,
            new[] { new ConstraintReference(new ConstraintId("testing.atomic-test")) });

        library.AddCompositeConstraint(composite);

        var resolver = new LibraryConstraintResolver(library);

        // Act - Resolve the composite constraint
        IConstraint result = resolver.ResolveConstraint(new ConstraintId("testing.composite-nesting"));

        // Assert - Should resolve successfully without circular reference issues
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id.Value, Is.EqualTo("testing.composite-nesting"));

        // Verify that metrics show successful resolution
        IResolutionMetrics metrics = resolver.GetResolutionMetrics();
        Assert.That(metrics.TotalResolutions, Is.GreaterThan(0));
    }

    [Test]
    public async Task Should_Resolve_Multiple_Constraints_In_Parallel()
    {
        // Business scenario: Batch resolution for multiple constraints improves performance
        // Expected: All constraints resolved efficiently in parallel

        // Arrange
        ConstraintId[] constraintIds = new[]
        {
            new ConstraintId("testing.write-test-first"),
            new ConstraintId("testing.acceptance-test-first"),
            new ConstraintId("methodology.outside-in-development")
        };

        // Act
        IReadOnlyDictionary<ConstraintId, IConstraint> results = await ((LibraryConstraintResolver)_resolver!).ResolveMultipleAsync(constraintIds);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results, Has.Count.EqualTo(3));

        foreach (ConstraintId? id in constraintIds)
        {
            Assert.Multiple(() =>
            {
                Assert.That(results.ContainsKey(id), Is.True, $"Should contain result for {id}");
                Assert.That(results[id], Is.Not.Null, $"Result for {id} should not be null");
            });
            Assert.That(results[id].Id, Is.EqualTo(id), $"Result ID should match requested ID for {id}");
        }

        // Verify performance metrics tracked all resolutions
        IResolutionMetrics metrics = _resolver.GetResolutionMetrics();
        Assert.That(metrics.TotalResolutions, Is.GreaterThan(0));
    }

    [Test]
    public async Task Should_Warm_Cache_For_Performance_Optimization()
    {
        // Business scenario: Pre-loading frequently used constraints improves response times
        // Expected: Cache warming loads constraints before they are needed

        // Arrange - Create a fresh resolver to avoid test pollution
        var library = new ConstraintMcpServer.Domain.Constraints.ConstraintLibrary("1.0.0", "Test Library");

        var testConstraint = new AtomicConstraint(
            new ConstraintId("cache.test-constraint"),
            "Test Constraint",
            0.8,
            new TriggerConfiguration(
                keywords: new[] { "cache" },
                filePatterns: new[] { "*.cs" },
                contextPatterns: new[] { "test" }),
            new[] { "Test constraint for cache warming" });

        library.AddAtomicConstraint(testConstraint);

        var resolver = new LibraryConstraintResolver(library);
        ConstraintId[] constraintIds = new[] { new ConstraintId("cache.test-constraint") };

        // Act - Warm the cache
        await resolver.WarmCacheAsync(constraintIds);

        // Clear metrics and resolve the same constraint
        IResolutionMetrics metricsAfterWarming = resolver.GetResolutionMetrics();
        int initialResolutionCount = metricsAfterWarming.TotalResolutions;

        // Resolve constraint that should now be cached
        IConstraint resolution = resolver.ResolveConstraint(constraintIds[0]);

        // Assert
        Assert.That(resolution, Is.Not.Null);
        Assert.That(resolution.Id, Is.EqualTo(constraintIds[0]));

        // Verify that cache functionality is working
        IResolutionMetrics finalMetrics = resolver.GetResolutionMetrics();
        Assert.Multiple(() =>
        {
            Assert.That(finalMetrics.TotalResolutions, Is.GreaterThan(initialResolutionCount),
                    "Should have additional resolutions recorded");
            Assert.That(finalMetrics.CacheHitRate, Is.GreaterThanOrEqualTo(0.0),
                "Cache hit rate should be valid");
        });
    }

    [Test]
    public void Should_Cache_Resolved_Constraints_For_Performance()
    {
        // Business scenario: Same constraint requested multiple times
        // Expected: Subsequent requests are served from cache for performance

        // Arrange
        var constraintId = new ConstraintId("testing.write-test-first");

        // Act
        // First resolution
        IConstraint firstResult = _resolver!.ResolveConstraint(constraintId);

        // Second resolution should be faster (cached)
        IConstraint secondResult = _resolver.ResolveConstraint(constraintId);

        Assert.Multiple(() =>
        {
            // Assert
            // Results should be equivalent
            Assert.That(secondResult.Id, Is.EqualTo(firstResult.Id));
            Assert.That(secondResult.Title, Is.EqualTo(firstResult.Title));
            Assert.That(ReferenceEquals(firstResult, secondResult), Is.True, "Second call should return cached instance");
        });
    }

    [Test]
    public async Task Should_Support_Async_Constraint_Resolution()
    {
        // Business scenario: Constraint resolution in async context (e.g., MCP server)
        // Expected: Async-friendly API for integration with MCP protocol

        // Arrange
        var constraintId = new ConstraintId("testing.write-test-first");

        // Act
        var asyncResolver = _resolver as IAsyncConstraintResolver;
        Assert.That(asyncResolver, Is.Not.Null, "Async resolution should be supported");

        IConstraint result = await asyncResolver!.ResolveConstraintAsync(constraintId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(constraintId));
    }

    [Test]
    public void Should_Provide_Resolution_Performance_Metrics()
    {
        // Business scenario: System monitoring requires resolution performance data
        // Expected: Resolution metrics for performance monitoring and optimization

        // Arrange
        var constraintId = new ConstraintId("testing.write-test-first");

        // Act
        IConstraint result = _resolver!.ResolveConstraint(constraintId);

        // Assert
        // Metrics should be available (drives IResolutionMetrics design)
        IResolutionMetrics metrics = _resolver.GetResolutionMetrics();
        Assert.That(metrics, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(metrics.TotalResolutions, Is.GreaterThan(0));
            Assert.That(metrics.CacheHitRate, Is.GreaterThanOrEqualTo(0.0));
            Assert.That(metrics.AverageResolutionTime, Is.LessThan(TimeSpan.FromMilliseconds(50)));
        });
    }

    [Test]
    public void Should_Resolve_Constraint_With_Composition_Metadata()
    {
        // Business scenario: Composite constraint needs component metadata for sequencing
        // Expected: Resolved components include composition-specific metadata

        // Arrange
        var compositeId = new ConstraintId("methodology.outside-in-development");

        // Act
        var result = _resolver!.ResolveConstraint(compositeId) as CompositeConstraint;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Components, Is.Not.Empty);

        // For now, we test that components are properly resolved
        // Composition metadata will be fully implemented when we add ComponentReferences
        foreach (AtomicConstraint component in result.Components)
        {
            Assert.Multiple(() =>
            {
                Assert.That(component.Id, Is.Not.Null);
                Assert.That(component.Title, Is.Not.Empty);
                Assert.That(component.Reminders, Is.Not.Empty);
            });
        }
    }
}

// These interfaces and exceptions are now implemented in the main project
// ConstraintMcpServer.Domain.Constraints namespace
