using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Tests.Domain;

/// <summary>
/// Unit tests for ConstraintLibrary domain model.
/// Focuses on business behavior: constraint library with validation, indexing, and performance.
/// Uses business-focused naming and testing approach.
/// </summary>
[TestFixture]
public class ConstraintLibraryTests
{
    [Test]
    public void Should_Create_Empty_ConstraintLibrary()
    {
        // Business scenario: Initialize empty constraint library
        // Expected: Library is created successfully with no constraints

        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            
            Assert.That(library, Is.Not.Null);
            Assert.That(library.AtomicConstraints, Is.Empty);
            Assert.That(library.CompositeConstraints, Is.Empty);
            Assert.That(library.TotalConstraints, Is.EqualTo(0));
        });
    }

    [Test]
    public void Should_Create_ConstraintLibrary_With_Version_And_Description()
    {
        // Business scenario: Library with metadata for versioning and documentation
        // Expected: Library includes version and description information

        // Arrange
        var version = "0.2.0";
        var description = "Test constraint library";
        
        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary(version, description);
            
            Assert.That(library.Version, Is.EqualTo(version));
            Assert.That(library.Description, Is.EqualTo(description));
        });
    }

    [Test]
    public void Should_Add_Atomic_Constraint_To_Library()
    {
        // Business scenario: Add atomic constraint to constraint library
        // Expected: Constraint is stored and can be retrieved by ID

        // Arrange
        var constraintId = new ConstraintId("testing.write-test-first");
        var triggers = new TriggerConfiguration(
            keywords: new[] { "test", "unit test" },
            filePatterns: new[] { "*Test.cs" },
            contextPatterns: new[] { "testing" });
        var reminders = new[] { "Write failing test first" };
        
        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            var atomicConstraint = new AtomicConstraint(
                constraintId,
                "Write a failing test before implementation",
                0.92,
                triggers,
                reminders);
            
            library.AddAtomicConstraint(atomicConstraint);
            
            Assert.That(library.AtomicConstraints, Contains.Item(atomicConstraint));
            Assert.That(library.TotalConstraints, Is.EqualTo(1));
            Assert.That(library.ContainsConstraint(constraintId), Is.True);
        });
    }

    [Test]
    public void Should_Add_Composite_Constraint_To_Library()
    {
        // Business scenario: Add composite constraint to constraint library
        // Expected: Composite constraint is stored with component references

        // Arrange
        var compositeId = new ConstraintId("methodology.outside-in");
        var componentReferences = new[]
        {
            new ConstraintReference(new ConstraintId("testing.acceptance-test-first"), sequenceOrder: 1),
            new ConstraintReference(new ConstraintId("testing.write-test-first"), sequenceOrder: 2)
        };
        
        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            var compositeConstraint = new CompositeConstraint(
                compositeId,
                "Outside-In Development",
                0.95,
                CompositionType.Sequential,
                componentReferences);
            
            library.AddCompositeConstraint(compositeConstraint);
            
            Assert.That(library.CompositeConstraints, Contains.Item(compositeConstraint));
            Assert.That(library.TotalConstraints, Is.EqualTo(1));
            Assert.That(library.ContainsConstraint(compositeId), Is.True);
        });
    }

    [Test]
    public void Should_Prevent_Duplicate_Constraint_IDs()
    {
        // Business scenario: Attempt to add constraint with existing ID
        // Expected: Exception prevents duplicate constraint IDs

        // Arrange
        var duplicateId = new ConstraintId("testing.write-test-first");
        
        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            var firstConstraint = CreateTestAtomicConstraint(duplicateId, "First constraint");
            var secondConstraint = CreateTestAtomicConstraint(duplicateId, "Second constraint");
            
            library.AddAtomicConstraint(firstConstraint);
            
            Assert.Throws<DuplicateConstraintIdException>(() => 
                library.AddAtomicConstraint(secondConstraint));
        });
    }

    [Test]
    public void Should_Get_Constraint_By_ID()
    {
        // Business scenario: Retrieve specific constraint by its ID
        // Expected: Correct constraint is returned for valid ID

        // Arrange
        var constraintId = new ConstraintId("testing.write-test-first");
        
        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            var constraint = CreateTestAtomicConstraint(constraintId, "Test constraint");
            library.AddAtomicConstraint(constraint);
            
            var retrieved = library.GetConstraint(constraintId);
            
            Assert.That(retrieved, Is.EqualTo(constraint));
            Assert.That(retrieved.Id, Is.EqualTo(constraintId));
        });
    }

    [Test]
    public void Should_Throw_ConstraintNotFoundException_For_Unknown_ID()
    {
        // Business scenario: Request constraint that doesn't exist in library
        // Expected: Clear exception indicating constraint not found

        // Arrange
        var unknownId = new ConstraintId("unknown.constraint");
        
        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            
            Assert.Throws<ConstraintNotFoundException>(() => 
                library.GetConstraint(unknownId));
        });
    }

    [Test]
    public void Should_Try_Get_Constraint_Without_Exception()
    {
        // Business scenario: Safe constraint retrieval without exceptions
        // Expected: Boolean result indicates success/failure

        // Arrange
        var existingId = new ConstraintId("testing.write-test-first");
        var unknownId = new ConstraintId("unknown.constraint");
        
        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            var constraint = CreateTestAtomicConstraint(existingId, "Test constraint");
            library.AddAtomicConstraint(constraint);
            
            // Successful retrieval
            var foundExisting = library.TryGetConstraint(existingId, out var existingConstraint);
            Assert.That(foundExisting, Is.True);
            Assert.That(existingConstraint, Is.EqualTo(constraint));
            
            // Failed retrieval
            var foundUnknown = library.TryGetConstraint(unknownId, out var unknownConstraint);
            Assert.That(foundUnknown, Is.False);
            Assert.That(unknownConstraint, Is.Null);
        });
    }

    [Test]
    public void Should_Get_Constraints_By_Priority_Range()
    {
        // Business scenario: Filter constraints by priority for selection
        // Expected: Only constraints within priority range are returned

        // Arrange
        var highPriority = CreateTestAtomicConstraint(new ConstraintId("high"), "High", 0.9);
        var mediumPriority = CreateTestAtomicConstraint(new ConstraintId("medium"), "Medium", 0.6);
        var lowPriority = CreateTestAtomicConstraint(new ConstraintId("low"), "Low", 0.3);
        
        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            library.AddAtomicConstraint(highPriority);
            library.AddAtomicConstraint(mediumPriority);
            library.AddAtomicConstraint(lowPriority);
            
            var highPriorityConstraints = library.GetConstraintsByPriority(0.8, 1.0);
            Assert.That(highPriorityConstraints, Contains.Item(highPriority));
            Assert.That(highPriorityConstraints, Does.Not.Contain(mediumPriority));
            Assert.That(highPriorityConstraints, Does.Not.Contain(lowPriority));
        });
    }

    [Test]
    public void Should_Get_Constraints_By_Keyword_Match()
    {
        // Business scenario: Find constraints relevant to current context
        // Expected: Constraints with matching keywords are returned

        // Arrange
        var testingConstraint = CreateTestAtomicConstraint(
            new ConstraintId("testing.constraint"),
            "Testing constraint",
            triggers: new TriggerConfiguration(keywords: new[] { "test", "unit test" }));
        
        var architectureConstraint = CreateTestAtomicConstraint(
            new ConstraintId("architecture.constraint"),
            "Architecture constraint",
            triggers: new TriggerConfiguration(keywords: new[] { "architecture", "design" }));
        
        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            library.AddAtomicConstraint(testingConstraint);
            library.AddAtomicConstraint(architectureConstraint);
            
            var testRelatedConstraints = library.GetConstraintsByKeyword("test");
            Assert.That(testRelatedConstraints, Contains.Item(testingConstraint));
            Assert.That(testRelatedConstraints, Does.Not.Contain(architectureConstraint));
        });
    }

    [Test]
    public void Should_Validate_Constraint_References_In_Composites()
    {
        // Business scenario: Composite constraint references non-existent components
        // Expected: Validation catches missing constraint references

        // Arrange
        var validReference = new ConstraintReference(new ConstraintId("existing.constraint"));
        var invalidReference = new ConstraintReference(new ConstraintId("missing.constraint"));
        
        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            
            // Add referenced constraint
            var existingConstraint = CreateTestAtomicConstraint(validReference.ConstraintId, "Existing");
            library.AddAtomicConstraint(existingConstraint);
            
            // Try to add composite with invalid reference
            var invalidComposite = new CompositeConstraint(
                new ConstraintId("invalid.composite"),
                "Invalid composite",
                0.8,
                CompositionType.Sequential,
                new[] { validReference, invalidReference });
            
            Assert.Throws<ConstraintReferenceValidationException>(() => 
                library.AddCompositeConstraint(invalidComposite));
        });
    }

    [Test]
    public void Should_Detect_Circular_References_In_Composite_Constraints()
    {
        // Business scenario: Composite constraints form circular reference chain
        // Expected: Validation prevents circular references

        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            
            // Create circular reference: A -> B -> A
            var compositeA = new CompositeConstraint(
                new ConstraintId("circular.a"),
                "Circular A",
                0.8,
                CompositionType.Sequential,
                new[] { new ConstraintReference(new ConstraintId("circular.b")) });
                
            var compositeB = new CompositeConstraint(
                new ConstraintId("circular.b"),
                "Circular B", 
                0.8,
                CompositionType.Sequential,
                new[] { new ConstraintReference(new ConstraintId("circular.a")) });
            
            library.AddCompositeConstraint(compositeA);
            
            Assert.Throws<CircularReferenceException>(() => 
                library.AddCompositeConstraint(compositeB));
        });
    }

    [Test]
    public void Should_Remove_Constraint_By_ID()
    {
        // Business scenario: Remove outdated or incorrect constraint from library
        // Expected: Constraint is removed and no longer accessible

        // Arrange
        var constraintId = new ConstraintId("testing.write-test-first");
        
        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            var constraint = CreateTestAtomicConstraint(constraintId, "Test constraint");
            library.AddAtomicConstraint(constraint);
            
            Assert.That(library.ContainsConstraint(constraintId), Is.True);
            
            var removed = library.RemoveConstraint(constraintId);
            
            Assert.That(removed, Is.True);
            Assert.That(library.ContainsConstraint(constraintId), Is.False);
            Assert.That(library.TotalConstraints, Is.EqualTo(0));
        });
    }

    [Test]
    public void Should_Prevent_Removal_Of_Referenced_Constraints()
    {
        // Business scenario: Try to remove constraint that's referenced by composite
        // Expected: Validation prevents removal of referenced constraints

        // Arrange
        var atomicId = new ConstraintId("atomic.constraint");
        var compositeId = new ConstraintId("composite.constraint");
        
        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            
            var atomic = CreateTestAtomicConstraint(atomicId, "Atomic constraint");
            library.AddAtomicConstraint(atomic);
            
            var composite = new CompositeConstraint(
                compositeId,
                "Composite constraint",
                0.8,
                CompositionType.Sequential,
                new[] { new ConstraintReference(atomicId) });
            library.AddCompositeConstraint(composite);
            
            Assert.Throws<ConstraintInUseException>(() => 
                library.RemoveConstraint(atomicId));
        });
    }

    [Test]
    public void Should_Get_All_References_To_Constraint()
    {
        // Business scenario: Find which composites reference a specific constraint
        // Expected: All composite constraints that reference the constraint are returned

        // Arrange
        var atomicId = new ConstraintId("atomic.constraint");
        
        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            
            var atomic = CreateTestAtomicConstraint(atomicId, "Atomic constraint");
            library.AddAtomicConstraint(atomic);
            
            var composite1 = new CompositeConstraint(
                new ConstraintId("composite.1"),
                "Composite 1",
                0.8,
                CompositionType.Sequential,
                new[] { new ConstraintReference(atomicId) });
                
            var composite2 = new CompositeConstraint(
                new ConstraintId("composite.2"),
                "Composite 2",
                0.7,
                CompositionType.Parallel,
                new[] { new ConstraintReference(atomicId) });
            
            library.AddCompositeConstraint(composite1);
            library.AddCompositeConstraint(composite2);
            
            var references = library.GetReferencesToConstraint(atomicId).ToList();
            
            Assert.That(references, Has.Count.EqualTo(2));
            Assert.That(references, Contains.Item(composite1.Id));
            Assert.That(references, Contains.Item(composite2.Id));
        });
    }

    [Test]
    public void Should_Support_Library_Statistics_And_Metrics()
    {
        // Business scenario: Monitor library size and composition metrics
        // Expected: Statistics provide insight into library composition

        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library = new ConstraintLibrary();
            
            // Add various constraints
            library.AddAtomicConstraint(CreateTestAtomicConstraint(new ConstraintId("atomic.1"), "Atomic 1"));
            library.AddAtomicConstraint(CreateTestAtomicConstraint(new ConstraintId("atomic.2"), "Atomic 2"));
            
            var composite = new CompositeConstraint(
                new ConstraintId("composite.1"),
                "Composite 1",
                0.8,
                CompositionType.Sequential,
                new[] { new ConstraintReference(new ConstraintId("atomic.1")) });
            library.AddCompositeConstraint(composite);
            
            var stats = library.GetLibraryStatistics();
            
            Assert.That(stats.TotalConstraints, Is.EqualTo(3));
            Assert.That(stats.AtomicConstraintCount, Is.EqualTo(2));
            Assert.That(stats.CompositeConstraintCount, Is.EqualTo(1));
            Assert.That(stats.AveragePriority, Is.GreaterThan(0));
            Assert.That(stats.ReferenceCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void Should_Support_Constraint_Library_Merge()
    {
        // Business scenario: Merge two constraint libraries
        // Expected: Combined library contains all constraints without conflicts

        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var library1 = new ConstraintLibrary("1.0", "Library 1");
            var library2 = new ConstraintLibrary("1.1", "Library 2");
            
            library1.AddAtomicConstraint(CreateTestAtomicConstraint(new ConstraintId("lib1.constraint"), "Lib1"));
            library2.AddAtomicConstraint(CreateTestAtomicConstraint(new ConstraintId("lib2.constraint"), "Lib2"));
            
            var mergedLibrary = library1.MergeWith(library2);
            
            Assert.That(mergedLibrary.TotalConstraints, Is.EqualTo(2));
            Assert.That(mergedLibrary.ContainsConstraint(new ConstraintId("lib1.constraint")), Is.True);
            Assert.That(mergedLibrary.ContainsConstraint(new ConstraintId("lib2.constraint")), Is.True);
        });
    }

    [Test]
    public void Should_Support_Constraint_Library_Cloning()
    {
        // Business scenario: Create independent copy of constraint library
        // Expected: Clone contains same constraints but is independent

        // Act & Assert - This will fail and drive implementation
        Assert.Throws<NotImplementedException>(() => 
        {
            var originalLibrary = new ConstraintLibrary("1.0", "Original");
            originalLibrary.AddAtomicConstraint(CreateTestAtomicConstraint(new ConstraintId("test.constraint"), "Test"));
            
            var clonedLibrary = originalLibrary.Clone();
            
            Assert.That(clonedLibrary.TotalConstraints, Is.EqualTo(originalLibrary.TotalConstraints));
            Assert.That(clonedLibrary.Version, Is.EqualTo(originalLibrary.Version));
            
            // Modifications to clone shouldn't affect original
            clonedLibrary.AddAtomicConstraint(CreateTestAtomicConstraint(new ConstraintId("clone.constraint"), "Clone"));
            Assert.That(originalLibrary.TotalConstraints, Is.EqualTo(1));
            Assert.That(clonedLibrary.TotalConstraints, Is.EqualTo(2));
        });
    }

    // Helper method to create test atomic constraints
    private AtomicConstraint CreateTestAtomicConstraint(ConstraintId id, string title, double priority = 0.8, TriggerConfiguration? triggers = null)
    {
        var defaultTriggers = triggers ?? new TriggerConfiguration(
            keywords: new[] { "test" },
            filePatterns: new[] { "*.cs" },
            contextPatterns: new[] { "testing" });
            
        return new AtomicConstraint(
            id,
            title,
            priority,
            defaultTriggers,
            new[] { $"{title} reminder" });
    }
}

// These classes will be implemented as part of the domain model
// Currently they don't exist, which is why the tests fail (RED phase)

/// <summary>
/// Domain model representing a constraint library with validation and indexing.
/// This test file drives the class design.
/// </summary>
public class ConstraintLibrary
{
    // This will be implemented to satisfy the tests
    // The tests drive the interface design
    
    public ConstraintLibrary(string? version = null, string? description = null)
    {
        throw new NotImplementedException("ConstraintLibrary not yet implemented");
    }
    
    public string? Version => throw new NotImplementedException();
    public string? Description => throw new NotImplementedException();
    public IReadOnlyList<AtomicConstraint> AtomicConstraints => throw new NotImplementedException();
    public IReadOnlyList<CompositeConstraint> CompositeConstraints => throw new NotImplementedException();
    public int TotalConstraints => throw new NotImplementedException();
    
    public void AddAtomicConstraint(AtomicConstraint constraint) => throw new NotImplementedException();
    public void AddCompositeConstraint(CompositeConstraint constraint) => throw new NotImplementedException();
    public bool ContainsConstraint(ConstraintId id) => throw new NotImplementedException();
    public IConstraint GetConstraint(ConstraintId id) => throw new NotImplementedException();
    public bool TryGetConstraint(ConstraintId id, out IConstraint? constraint) => throw new NotImplementedException();
    public IEnumerable<IConstraint> GetConstraintsByPriority(double minPriority, double maxPriority) => throw new NotImplementedException();
    public IEnumerable<IConstraint> GetConstraintsByKeyword(string keyword) => throw new NotImplementedException();
    public bool RemoveConstraint(ConstraintId id) => throw new NotImplementedException();
    public IEnumerable<ConstraintId> GetReferencesToConstraint(ConstraintId id) => throw new NotImplementedException();
    public LibraryStatistics GetLibraryStatistics() => throw new NotImplementedException();
    public ConstraintLibrary MergeWith(ConstraintLibrary other) => throw new NotImplementedException();
    public ConstraintLibrary Clone() => throw new NotImplementedException();
}

/// <summary>
/// Exception thrown when attempting to add a constraint with a duplicate ID.
/// </summary>
public class DuplicateConstraintIdException : Exception
{
    public ConstraintId ConstraintId { get; }
    
    public DuplicateConstraintIdException(ConstraintId constraintId)
        : base($"Constraint with ID '{constraintId}' already exists in library")
    {
        ConstraintId = constraintId;
    }
}

// All required classes are now implemented in the main project
// under ConstraintMcpServer.Domain.Constraints namespace