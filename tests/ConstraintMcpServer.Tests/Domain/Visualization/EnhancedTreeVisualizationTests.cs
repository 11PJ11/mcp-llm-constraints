using System;
using NUnit.Framework;
using ConstraintMcpServer.Domain.Visualization;

namespace ConstraintMcpServer.Tests.Domain.Visualization;

/// <summary>
/// Unit tests for EnhancedTreeVisualization domain type.
/// Drives implementation through TDD Red-Green-Refactor cycles.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("Domain")]
[Category("Visualization")]
public sealed class EnhancedTreeVisualizationTests
{
    /// <summary>
    /// RED: Test should fail initially - EnhancedTreeVisualization should capture enhanced visualization data
    /// </summary>
    [Test]
    public void Constructor_WhenCreatedWithValidData_ShouldStoreAllProperties()
    {
        // Given
        var treeContent = "Enhanced tree content with symbols";
        var renderTime = TimeSpan.FromMilliseconds(25);
        var constraintCount = 5;
        var colorProfile = ColorProfile.Standard;
        var symbolSet = EnhancedSymbolSet.Unicode;
        var isClaudeCodeCompatible = true;

        // When
        var visualization = new EnhancedTreeVisualization(
            treeContent,
            renderTime,
            constraintCount,
            colorProfile,
            symbolSet,
            isClaudeCodeCompatible);

        // Then
        Assert.That(visualization.TreeContent, Is.EqualTo(treeContent));
        Assert.That(visualization.RenderTime, Is.EqualTo(renderTime));
        Assert.That(visualization.ConstraintCount, Is.EqualTo(constraintCount));
        Assert.That(visualization.ColorProfile, Is.EqualTo(colorProfile));
        Assert.That(visualization.SymbolSet, Is.EqualTo(symbolSet));
        Assert.That(visualization.IsClaudeCodeCompatible, Is.EqualTo(isClaudeCodeCompatible));
    }

    /// <summary>
    /// RED: Test should fail initially - performance tracking should identify when within thresholds
    /// </summary>
    [Test]
    public void MeetsPerformanceThreshold_WhenRenderTimeBelow50Ms_ShouldReturnTrue()
    {
        // Given
        var visualization = new EnhancedTreeVisualization(
            "content",
            TimeSpan.FromMilliseconds(25),
            5,
            ColorProfile.Monochrome,
            EnhancedSymbolSet.Ascii,
            true);

        // When
        var meetsThreshold = visualization.MeetsPerformanceThreshold;

        // Then
        Assert.That(meetsThreshold, Is.True, "render time of 25ms should meet <50ms threshold");
    }

    /// <summary>
    /// RED: Test should fail initially - performance tracking should identify when exceeds thresholds
    /// </summary>
    [Test]
    public void MeetsPerformanceThreshold_WhenRenderTimeAbove50Ms_ShouldReturnFalse()
    {
        // Given
        var visualization = new EnhancedTreeVisualization(
            "content",
            TimeSpan.FromMilliseconds(75),
            5,
            ColorProfile.Monochrome,
            EnhancedSymbolSet.Ascii,
            true);

        // When
        var meetsThreshold = visualization.MeetsPerformanceThreshold;

        // Then
        Assert.That(meetsThreshold, Is.False, "render time of 75ms should exceed 50ms threshold");
    }

    /// <summary>
    /// RED: Test should fail initially - enhanced visualization should indicate hierarchical structure
    /// </summary>
    [Test]
    public void HasHierarchicalStructure_WhenTreeContentContainsBranchMarkers_ShouldReturnTrue()
    {
        // Given
        var visualization = new EnhancedTreeVisualization(
            "Root\n├─ Child 1\n├─ Child 2",
            TimeSpan.FromMilliseconds(10),
            3,
            ColorProfile.Standard,
            EnhancedSymbolSet.Unicode,
            true);

        // When
        var hasHierarchy = visualization.HasHierarchicalStructure;

        // Then
        Assert.That(hasHierarchy, Is.True, "content with tree structure should indicate hierarchical structure");
    }

    /// <summary>
    /// RED: Test should fail initially - relationship indicators should be detected
    /// </summary>
    [Test]
    public void ShowsCompositionRelationships_WhenContentContainsCompositionMarkers_ShouldReturnTrue()
    {
        // Given
        var visualization = new EnhancedTreeVisualization(
            "Composite Constraints:\n├─ Quality Standards (Composite)",
            TimeSpan.FromMilliseconds(10),
            1,
            ColorProfile.Standard,
            EnhancedSymbolSet.Unicode,
            true);

        // When
        var showsRelationships = visualization.ShowsCompositionRelationships;

        // Then
        Assert.That(showsRelationships, Is.True, "content with composition markers should indicate relationships");
    }

    /// <summary>
    /// RED: Test should fail initially - metadata display should be detected
    /// </summary>
    [Test]
    public void DisplaysConstraintMetadata_WhenContentContainsMetadataMarkers_ShouldReturnTrue()
    {
        // Given
        var visualization = new EnhancedTreeVisualization(
            "constraint.id\n│ Priority: 0.85\n│ Title: Test Constraint",
            TimeSpan.FromMilliseconds(10),
            1,
            ColorProfile.Standard,
            EnhancedSymbolSet.Unicode,
            true);

        // When
        var displaysMetadata = visualization.DisplaysConstraintMetadata;

        // Then
        Assert.That(displaysMetadata, Is.True, "content with metadata markers should indicate metadata display");
    }

    /// <summary>
    /// RED: Test should fail initially - null tree content should not be allowed
    /// </summary>
    [Test]
    public void Constructor_WhenTreeContentIsNull_ShouldThrowArgumentException()
    {
        // Given
        string? nullContent = null;

        // When
        Action createWithNull = () => new EnhancedTreeVisualization(
            nullContent!,
            TimeSpan.FromMilliseconds(10),
            1,
            ColorProfile.Standard,
            EnhancedSymbolSet.Unicode,
            true);

        // Then
        Assert.That(createWithNull, Throws.TypeOf<ArgumentException>().With.Message.Contains("Tree content cannot be null"));
    }

    /// <summary>
    /// RED: Test should fail initially - empty tree content should be allowed but flagged
    /// </summary>
    [Test]
    public void Constructor_WhenTreeContentIsEmpty_ShouldAllowButIndicateEmptyContent()
    {
        // Given
        var emptyContent = string.Empty;

        // When
        var visualization = new EnhancedTreeVisualization(
            emptyContent,
            TimeSpan.FromMilliseconds(10),
            0,
            ColorProfile.Standard,
            EnhancedSymbolSet.Unicode,
            true);

        // Then
        Assert.That(visualization.TreeContent, Is.Empty);
        Assert.That(visualization.ConstraintCount, Is.EqualTo(0));
        Assert.That(visualization.HasHierarchicalStructure, Is.False, "empty content has no hierarchical structure");
    }
}
