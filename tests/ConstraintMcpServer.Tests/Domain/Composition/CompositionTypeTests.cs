using NUnit.Framework;
using ConstraintMcpServer.Domain.Composition;

namespace ConstraintMcpServer.Tests.Domain.Composition;

/// <summary>
/// Unit tests for CompositionType enum - drives implementation through TDD.
/// Tests composition type values needed for Sequential, Hierarchical, and Progressive strategies.
/// </summary>
[TestFixture]
[Category("Unit")]
public class CompositionTypeTests
{
    /// <summary>
    /// RED: This test will fail and drive CompositionType enum implementation
    /// Business requirement: Support Sequential composition for TDD workflow (RED → GREEN → REFACTOR)
    /// </summary>
    [Test]
    public void CompositionType_Should_Have_Sequential_Value_For_TDD_Workflow()
    {
        // Arrange & Act
        var sequentialType = CompositionType.Sequential;

        // Assert
        Assert.That(sequentialType, Is.EqualTo(CompositionType.Sequential));
        Assert.That(sequentialType.ToString(), Is.EqualTo("Sequential"));
    }

    /// <summary>
    /// RED: Test will drive additional composition type implementations
    /// Business requirement: Support complex methodology workflows
    /// </summary>
    [Test]
    public void CompositionType_Should_Have_All_Required_Composition_Strategies()
    {
        // Arrange & Act - These will fail until CompositionType enum is implemented
        var sequential = CompositionType.Sequential;
        var hierarchical = CompositionType.Hierarchical;
        var progressive = CompositionType.Progressive;

        // Assert - Validates all composition strategies are available
        Assert.That(sequential, Is.Not.EqualTo(hierarchical));
        Assert.That(hierarchical, Is.Not.EqualTo(progressive));
        Assert.That(progressive, Is.Not.EqualTo(sequential));

        // Validates enum values are properly defined
        Assert.That(sequential.ToString(), Contains.Substring("Sequential"));
        Assert.That(hierarchical.ToString(), Contains.Substring("Hierarchical"));
        Assert.That(progressive.ToString(), Contains.Substring("Progressive"));
    }

    /// <summary>
    /// Business requirement: Composition types must be comparable for strategy selection
    /// </summary>
    [Test]
    public void CompositionType_Should_Support_Equality_Comparison()
    {
        // Arrange
        var sequential1 = CompositionType.Sequential;
        var sequential2 = CompositionType.Sequential;
        var hierarchical = CompositionType.Hierarchical;

        // Act & Assert
        Assert.That(sequential1, Is.EqualTo(sequential2));
        Assert.That(sequential1, Is.Not.EqualTo(hierarchical));
        Assert.That(sequential1.GetHashCode(), Is.EqualTo(sequential2.GetHashCode()));
    }
}
