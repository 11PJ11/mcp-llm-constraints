using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ConstraintMcpServer.Application.Selection;

namespace ConstraintMcpServer.Tests.Unit.Application;

/// <summary>
/// Tests for KeywordMatcher fuzzy matching and synonym expansion.
/// Tests business logic for intelligent keyword matching with confidence scoring.
/// </summary>
[TestFixture]
public sealed class KeywordMatcherTests
{
    private KeywordMatcher _keywordMatcher = null!;

    [SetUp]
    public void SetUp()
    {
        _keywordMatcher = new KeywordMatcher();
    }

    [Test]
    public void CalculateMatchConfidence_ExactMatch_ReturnsHighConfidence()
    {
        // Arrange
        var targetKeywords = new[] { "test", "implementation", "TDD" };
        var contextKeywords = new[] { "test", "implementation", "TDD" };

        // Act
        double confidence = _keywordMatcher.CalculateMatchConfidence(targetKeywords, contextKeywords);

        // Assert
        Assert.That(confidence, Is.GreaterThanOrEqualTo(0.9), "exact matches should have high confidence");
    }

    [Test]
    public void CalculateMatchConfidence_FuzzyMatch_ReturnsModerateConfidence()
    {
        // Arrange - similar but not exact terms (typos or variations)
        var targetKeywords = new[] { "optimize", "validate" };
        var contextKeywords = new[] { "optimise", "validete" }; // British spelling + typo

        // Act
        double confidence = _keywordMatcher.CalculateMatchConfidence(targetKeywords, contextKeywords);

        // Assert
        Assert.That(confidence, Is.InRange(0.5, 0.9), "fuzzy matches should have moderate confidence");
    }

    [Test]
    public void CalculateMatchConfidence_SynonymMatch_ReturnsHighConfidence()
    {
        // Arrange - synonyms should be treated as equivalent
        var targetKeywords = new[] { "test", "unittest" };
        var contextKeywords = new[] { "testing", "unit-test" };

        // Act
        double confidence = _keywordMatcher.CalculateMatchConfidence(targetKeywords, contextKeywords);

        // Assert
        Assert.That(confidence, Is.GreaterThanOrEqualTo(0.8), "synonym matches should have high confidence");
    }

    [Test]
    public void CalculateMatchConfidence_NoMatch_ReturnsLowConfidence()
    {
        // Arrange
        var targetKeywords = new[] { "test", "implementation" };
        var contextKeywords = new[] { "deploy", "monitor" };

        // Act
        double confidence = _keywordMatcher.CalculateMatchConfidence(targetKeywords, contextKeywords);

        // Assert
        Assert.That(confidence, Is.LessThanOrEqualTo(0.2), "no matches should have low confidence");
    }

    [Test]
    public void ExtractKeywords_FromUserInput_ReturnsRelevantKeywords()
    {
        // Arrange
        string userInput = "I need to implement a test for the authentication feature using TDD approach";

        // Act
        var keywords = _keywordMatcher.ExtractKeywords(userInput);

        // Assert
        Assert.That(keywords, Contains.Item("implement"));
        Assert.That(keywords, Contains.Item("test"));
        Assert.That(keywords, Contains.Item("authentication"));
        Assert.That(keywords, Contains.Item("TDD"));

        Assert.That(keywords, Does.Not.Contain("I"), "should filter out common stop words");
        Assert.That(keywords, Does.Not.Contain("need"), "should filter out common stop words");
        Assert.That(keywords, Does.Not.Contain("to"), "should filter out common stop words");
        Assert.That(keywords, Does.Not.Contain("a"), "should filter out common stop words");
        Assert.That(keywords, Does.Not.Contain("the"), "should filter out common stop words");
        Assert.That(keywords, Does.Not.Contain("for"), "should filter out common stop words");
    }

    [Test]
    public void ExtractKeywords_EmptyInput_ReturnsEmptyCollection()
    {
        // Arrange
        string userInput = "";

        // Act
        var keywords = _keywordMatcher.ExtractKeywords(userInput);

        // Assert
        Assert.That(keywords, Is.Empty);
    }

    [Test]
    public void ExtractKeywords_OnlyStopWords_ReturnsEmptyCollection()
    {
        // Arrange
        string userInput = "the a an and or but";

        // Act
        var keywords = _keywordMatcher.ExtractKeywords(userInput);

        // Assert
        Assert.That(keywords, Is.Empty, "stop words should be filtered out");
    }

    [Test]
    public void ExpandSynonyms_TddTerms_ReturnsExpandedSynonyms()
    {
        // Arrange
        var originalKeywords = new[] { "TDD", "unit-test" };

        // Act
        var expandedKeywords = _keywordMatcher.ExpandSynonyms(originalKeywords);

        // Assert
        Assert.That(expandedKeywords, Contains.Item("TDD"));
        Assert.That(expandedKeywords, Contains.Item("unit-test"));
        Assert.That(expandedKeywords, Contains.Item("test-driven"));
        Assert.That(expandedKeywords, Contains.Item("testing"));
        Assert.That(expandedKeywords, Contains.Item("unittest"));
    }

    [Test]
    public void ExpandSynonyms_ArchitecturalTerms_ReturnsExpandedSynonyms()
    {
        // Arrange
        var originalKeywords = new[] { "hexagonal", "clean-architecture" };

        // Act
        var expandedKeywords = _keywordMatcher.ExpandSynonyms(originalKeywords);

        // Assert
        Assert.That(expandedKeywords, Contains.Item("hexagonal"));
        Assert.That(expandedKeywords, Contains.Item("clean-architecture"));
        Assert.That(expandedKeywords, Contains.Item("ports-adapters"));
        Assert.That(expandedKeywords, Contains.Item("domain-driven"));
        Assert.That(expandedKeywords, Contains.Item("layered"));
    }

    [Test]
    public void ExpandSynonyms_UnknownTerms_ReturnsOriginalKeywords()
    {
        // Arrange
        var originalKeywords = new[] { "unknown-term", "custom-keyword" };

        // Act
        var expandedKeywords = _keywordMatcher.ExpandSynonyms(originalKeywords);

        // Assert
        Assert.That(expandedKeywords, Is.EquivalentTo(originalKeywords),
            "unknown terms should pass through unchanged");
    }

    [Test]
    public void CalculateMatchConfidence_PartialMatch_ReturnsProportionalConfidence()
    {
        // Arrange - 2 out of 4 keywords match
        var targetKeywords = new[] { "test", "implement", "deploy", "monitor" };
        var contextKeywords = new[] { "testing", "implementation" };

        // Act
        double confidence = _keywordMatcher.CalculateMatchConfidence(targetKeywords, contextKeywords);

        // Assert
        Assert.That(confidence, Is.InRange(0.4, 0.6), "50% match should give proportional confidence");
    }

    [Test]
    public void CalculateMatchConfidence_CaseInsensitive_ReturnsHighConfidence()
    {
        // Arrange
        var targetKeywords = new[] { "TEST", "Implementation" };
        var contextKeywords = new[] { "test", "IMPLEMENTATION" };

        // Act
        double confidence = _keywordMatcher.CalculateMatchConfidence(targetKeywords, contextKeywords);

        // Assert
        Assert.That(confidence, Is.GreaterThanOrEqualTo(0.9), "matching should be case-insensitive");
    }

    [Test]
    public void CalculateMatchConfidence_NullOrEmptyInput_ReturnsZeroConfidence()
    {
        // Arrange & Act & Assert
        Assert.That(_keywordMatcher.CalculateMatchConfidence(null!, new[] { "test" }), Is.EqualTo(0.0));
        Assert.That(_keywordMatcher.CalculateMatchConfidence(new[] { "test" }, null!), Is.EqualTo(0.0));
        Assert.That(_keywordMatcher.CalculateMatchConfidence(Array.Empty<string>(), new[] { "test" }), Is.EqualTo(0.0));
        Assert.That(_keywordMatcher.CalculateMatchConfidence(new[] { "test" }, Array.Empty<string>()), Is.EqualTo(0.0));
    }

    [Test]
    public void ExtractKeywords_TechnicalTerms_PreservesCasingForAcronyms()
    {
        // Arrange
        string userInput = "Implement TDD approach using SOLID principles and DDD patterns";

        // Act
        var keywords = _keywordMatcher.ExtractKeywords(userInput);

        // Assert
        Assert.That(keywords, Contains.Item("TDD"), "should preserve acronym casing");
        Assert.That(keywords, Contains.Item("SOLID"), "should preserve acronym casing");
        Assert.That(keywords, Contains.Item("DDD"), "should preserve acronym casing");
    }

    [Test]
    public void CalculateMatchConfidence_WeightedByImportance_ReturnsAdjustedConfidence()
    {
        // Arrange - keywords with different importance weights
        var targetKeywords = new[] { "test", "critical-feature", "performance" };
        var contextKeywords = new[] { "testing", "critical-feature" }; // matches 2 out of 3, including important term

        // Act
        double confidence = _keywordMatcher.CalculateMatchConfidence(targetKeywords, contextKeywords);

        // Assert
        Assert.That(confidence, Is.GreaterThan(0.6), "important keyword matches should increase confidence");
    }
}
