using System;
using System.Collections.Generic;
using NUnit.Framework;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Tests.Domain;

/// <summary>
/// Unit tests for ConstraintReference domain model.
/// Focuses on business behavior: ID-based constraint references with composition metadata.
/// Uses business-focused naming and testing approach.
/// </summary>
[TestFixture]
public class ConstraintReferenceTests
{
    [Test]
    public void Should_Create_ConstraintReference_With_Valid_ID()
    {
        // Business scenario: Composite constraint references atomic constraint by ID
        // Expected: Reference is created with valid constraint ID

        // Arrange
        var constraintId = new ConstraintId("testing.write-test-first");
        
        // Act
        var reference = new ConstraintReference(constraintId);
        
        // Assert
        Assert.That(reference, Is.Not.Null);
        Assert.That(reference.ConstraintId, Is.EqualTo(constraintId));
        Assert.That(reference.ConstraintId.Value, Is.EqualTo("testing.write-test-first"));
    }

    [Test]
    public void Should_Throw_ArgumentNullException_For_Null_ConstraintId()
    {
        // Business scenario: Invalid reference creation with null ID
        // Expected: Defensive programming - null guard exception

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConstraintReference(null!));
    }

    [Test]
    public void Should_Create_ConstraintReference_With_Sequence_Order()
    {
        // Business scenario: Sequential composition needs component ordering
        // Expected: Reference includes sequence order for sequential composition

        // Arrange
        var constraintId = new ConstraintId("testing.write-test-first");
        var sequenceOrder = 1;
        
        // Act
        var reference = new ConstraintReference(constraintId, sequenceOrder);
        
        // Assert
        Assert.That(reference.SequenceOrder, Is.EqualTo(sequenceOrder));
        Assert.That(reference.HasSequenceOrder, Is.True);
    }

    [Test]
    public void Should_Create_ConstraintReference_With_Hierarchy_Level()
    {
        // Business scenario: Hierarchical composition needs component levels
        // Expected: Reference includes hierarchy level for hierarchical composition

        // Arrange
        var constraintId = new ConstraintId("architecture.single-responsibility");
        var hierarchyLevel = 2;
        
        // Act
        var reference = new ConstraintReference(constraintId, hierarchyLevel: hierarchyLevel);
        
        // Assert
        Assert.That(reference.HierarchyLevel, Is.EqualTo(hierarchyLevel));
        Assert.That(reference.HasHierarchyLevel, Is.True);
    }

    [Test]
    public void Should_Create_ConstraintReference_With_Composition_Metadata()
    {
        // Business scenario: Complex composition needs additional metadata
        // Expected: Reference includes custom composition metadata

        // Arrange
        var constraintId = new ConstraintId("testing.acceptance-test-first");
        var metadata = new Dictionary<string, object>
        {
            ["phase"] = "outer-loop",
            ["description"] = "Start with failing acceptance test",
            ["priority_boost"] = 0.05
        };
        
        // Act
        var reference = new ConstraintReference(constraintId, metadata: metadata);
        
        // Assert
        Assert.That(reference.CompositionMetadata, Is.Not.Null);
        Assert.That(reference.CompositionMetadata["phase"], Is.EqualTo("outer-loop"));
        Assert.That(reference.CompositionMetadata["description"], Is.EqualTo("Start with failing acceptance test"));
        Assert.That(reference.CompositionMetadata["priority_boost"], Is.EqualTo(0.05));
    }

    [Test]
    public void Should_Create_ConstraintReference_With_All_Properties()
    {
        // Business scenario: Full reference specification for complex composition
        // Expected: Reference includes all composition properties

        // Arrange
        var constraintId = new ConstraintId("testing.write-test-first");
        var sequenceOrder = 2;
        var hierarchyLevel = 1;
        var metadata = new Dictionary<string, object>
        {
            ["phase"] = "inner-loop",
            ["description"] = "Inner TDD loop implementation",
            ["requires"] = new[] { "testing.acceptance-test-first" }
        };
        
        // Act
        var reference = new ConstraintReference(
            constraintId, 
            sequenceOrder: sequenceOrder,
            hierarchyLevel: hierarchyLevel, 
            metadata: metadata);
        
        // Assert
        Assert.That(reference.ConstraintId, Is.EqualTo(constraintId));
        Assert.That(reference.SequenceOrder, Is.EqualTo(sequenceOrder));
        Assert.That(reference.HierarchyLevel, Is.EqualTo(hierarchyLevel));
        Assert.That(reference.CompositionMetadata, Is.EqualTo(metadata));
        Assert.That(reference.HasSequenceOrder, Is.True);
        Assert.That(reference.HasHierarchyLevel, Is.True);
    }

    [Test]
    public void Should_Support_References_Without_Optional_Properties()
    {
        // Business scenario: Simple reference without composition metadata
        // Expected: Reference works with just constraint ID

        // Arrange
        var constraintId = new ConstraintId("architecture.dependency-inversion");
        
        // Act
        var reference = new ConstraintReference(constraintId);
        
        // Assert
        Assert.That(reference.ConstraintId, Is.EqualTo(constraintId));
        Assert.That(reference.HasSequenceOrder, Is.False);
        Assert.That(reference.HasHierarchyLevel, Is.False);
        Assert.That(reference.CompositionMetadata, Is.Empty);
    }

    [Test]
    public void Should_Validate_Sequence_Order_Range()
    {
        // Business scenario: Invalid sequence order specification
        // Expected: Validation prevents invalid sequence orders

        // Arrange
        var constraintId = new ConstraintId("testing.write-test-first");
        var invalidSequenceOrder = -1;
        
        // Act & Assert
        Assert.Throws<ValidationException>(() => 
            new ConstraintReference(constraintId, sequenceOrder: invalidSequenceOrder));
    }

    [Test]
    public void Should_Validate_Hierarchy_Level_Range()
    {
        // Business scenario: Invalid hierarchy level specification
        // Expected: Validation prevents negative hierarchy levels

        // Arrange
        var constraintId = new ConstraintId("architecture.single-responsibility");
        var invalidHierarchyLevel = -1;
        
        // Act & Assert
        Assert.Throws<ValidationException>(() => 
            new ConstraintReference(constraintId, hierarchyLevel: invalidHierarchyLevel));
    }

    [Test]
    public void Should_Support_Equality_Comparison_By_ConstraintId()
    {
        // Business scenario: Duplicate reference detection and comparison
        // Expected: References with same constraint ID are considered equal

        // Arrange
        var constraintId = new ConstraintId("testing.write-test-first");
        
        // Act
        var reference1 = new ConstraintReference(constraintId, sequenceOrder: 1);
        var reference2 = new ConstraintReference(constraintId, sequenceOrder: 2);
        
        // Assert - Equality based on constraint ID, not composition metadata
        Assert.That(reference1, Is.EqualTo(reference2));
        Assert.That(reference1.GetHashCode(), Is.EqualTo(reference2.GetHashCode()));
    }

    [Test]
    public void Should_Support_String_Representation_For_Debugging()
    {
        // Business scenario: Debugging and logging constraint references
        // Expected: Clear string representation with relevant information

        // Arrange
        var constraintId = new ConstraintId("methodology.outside-in-development");
        var sequenceOrder = 1;
        var hierarchyLevel = 2;
        
        // Act
        var reference = new ConstraintReference(
            constraintId, 
            sequenceOrder: sequenceOrder,
            hierarchyLevel: hierarchyLevel);
        
        var stringRepresentation = reference.ToString();
        
        // Assert
        Assert.That(stringRepresentation, Contains.Substring("methodology.outside-in-development"));
        Assert.That(stringRepresentation, Contains.Substring("Sequence: 1"));
        Assert.That(stringRepresentation, Contains.Substring("Level: 2"));
    }

    [Test]
    public void Should_Create_Copy_With_Updated_Composition_Metadata()
    {
        // Business scenario: Runtime composition metadata updates
        // Expected: Immutable reference with copy-with-update capability

        // Arrange
        var constraintId = new ConstraintId("testing.acceptance-test-first");
        var originalMetadata = new Dictionary<string, object> { ["phase"] = "outer-loop" };
        var updatedMetadata = new Dictionary<string, object> { ["phase"] = "validation" };
        
        // Act
        var originalReference = new ConstraintReference(constraintId, metadata: originalMetadata);
        var updatedReference = originalReference.WithCompositionMetadata(updatedMetadata);
        
        // Assert
        Assert.That(updatedReference.ConstraintId, Is.EqualTo(constraintId));
        Assert.That(updatedReference.CompositionMetadata["phase"], Is.EqualTo("validation"));
        
        // Original should be unchanged (immutable)
        Assert.That(originalReference.CompositionMetadata["phase"], Is.EqualTo("outer-loop"));
    }

    [Test]
    public void Should_Create_Copy_With_Updated_Sequence_Order()
    {
        // Business scenario: Runtime sequence reordering during composition
        // Expected: Immutable reference with sequence order update capability

        // Arrange
        var constraintId = new ConstraintId("testing.write-test-first");
        var originalSequence = 1;
        var updatedSequence = 3;
        
        // Act
        var originalReference = new ConstraintReference(constraintId, sequenceOrder: originalSequence);
        var updatedReference = originalReference.WithSequenceOrder(updatedSequence);
        
        // Assert
        Assert.That(updatedReference.SequenceOrder, Is.EqualTo(updatedSequence));
        Assert.That(originalReference.SequenceOrder, Is.EqualTo(originalSequence));
    }

    [Test]
    public void Should_Provide_Reference_Context_For_Error_Messages()
    {
        // Business scenario: Error reporting with context about which reference failed
        // Expected: Rich context information for debugging and error reporting

        // Arrange
        var constraintId = new ConstraintId("non.existent.constraint");
        var metadata = new Dictionary<string, object> 
        { 
            ["source"] = "outside-in-methodology",
            ["step"] = "acceptance-testing"
        };
        
        // Act
        var reference = new ConstraintReference(constraintId, sequenceOrder: 1, hierarchyLevel: 2, metadata: metadata);
        var context = reference.GetReferenceContext();
        
        // Assert
        Assert.That(context.ConstraintId, Is.EqualTo(constraintId));
        Assert.That(context.Source, Is.EqualTo("ConstraintReference")); // Based on actual implementation
        Assert.That(context.Step, Is.EqualTo("1")); // SequenceOrder as string
        Assert.That(context.Position, Is.EqualTo("2")); // HierarchyLevel as string
    }
}

// ConstraintReference and ReferenceContext are now implemented in the main project
// under ConstraintMcpServer.Domain.Constraints namespace