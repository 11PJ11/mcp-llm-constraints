using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Feedback;
using ConstraintMcpServer.Domain.Feedback;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Tests.Application.Feedback;

/// <summary>
/// Unit tests for BasicEffectivenessTracker application service.
/// Validates effectiveness calculation logic and performance requirements.
/// </summary>
[TestFixture]
public class BasicEffectivenessTrackerTests
{
    private BasicEffectivenessTracker? _tracker;

    [SetUp]
    public void Setup()
    {
        var store = new InMemoryRatingStore();
        _tracker = new BasicEffectivenessTracker(store);
    }

    [TearDown]
    public void TearDown()
    {
        _tracker?.Dispose();
    }

    [Test]
    public async Task Should_Calculate_Effectiveness_For_Constraint_With_Mixed_Ratings()
    {
        // Given
        const string constraintId = "tdd.test-first";

        // Create and add sample ratings: 3 positive, 1 negative, 1 neutral
        var ratings = new List<SimpleUserRating>
        {
            SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsUp, "session-1"),
            SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsUp, "session-2"),
            SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsUp, "session-3"),
            SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsDown, "session-4"),
            SimpleUserRating.WithoutComment(constraintId, RatingValue.Neutral, "session-5")
        };

        // Add ratings to tracker through UpdateWithFeedback
        foreach (var rating in ratings)
        {
            await _tracker!.UpdateWithFeedback(rating);
        }

        // When
        var result = await _tracker!.CalculateEffectiveness(constraintId);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Effectiveness calculation should succeed");

        var score = result.Value;
        Assert.That(score.ConstraintId, Is.EqualTo(constraintId));
        Assert.That(score.TotalRatings, Is.EqualTo(5));
        Assert.That(score.PositiveRatings, Is.EqualTo(3));
        Assert.That(score.NegativeRatings, Is.EqualTo(1));

        // Effectiveness = (3 positive + 0.5 * 1 neutral) / 5 = 0.7
        Assert.That(score.EffectivenessScore, Is.EqualTo(0.7).Within(0.001));
    }

    [Test]
    public async Task Should_Return_Neutral_Effectiveness_For_Constraint_With_No_Ratings()
    {
        // Given
        const string constraintId = "unused-constraint";

        // When
        var result = await _tracker!.CalculateEffectiveness(constraintId);

        // Then
        Assert.That(result.IsSuccess, Is.True);

        var score = result.Value;
        Assert.That(score.ConstraintId, Is.EqualTo(constraintId));
        Assert.That(score.TotalRatings, Is.EqualTo(0));
        Assert.That(score.EffectivenessScore, Is.EqualTo(0.5)); // Neutral when no ratings
    }

    [Test]
    public async Task Should_Update_Effectiveness_Score_With_New_Rating()
    {
        // Given
        const string constraintId = "refactoring.level1";
        var newRating = SimpleUserRating.WithComment(
            constraintId,
            RatingValue.ThumbsUp,
            "session-123",
            "Very helpful");

        // When
        var result = await _tracker!.UpdateWithFeedback(newRating);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Feedback update should succeed");

        // Verify the rating was incorporated
        var updatedScore = await _tracker.CalculateEffectiveness(constraintId);
        Assert.That(updatedScore.IsSuccess, Is.True);
        Assert.That(updatedScore.Value.TotalRatings, Is.GreaterThan(0));
    }

    [Test]
    public async Task Should_Return_Top_Constraints_Ordered_By_Effectiveness()
    {
        // Given - Setup constraints with different effectiveness scores
        var highEffectivenessConstraint = "high-effective";
        var mediumEffectivenessConstraint = "medium-effective";
        var lowEffectivenessConstraint = "low-effective";

        // Add ratings to create different effectiveness scores
        await _tracker!.UpdateWithFeedback(
            SimpleUserRating.WithoutComment(highEffectivenessConstraint, RatingValue.ThumbsUp, "session-1"));
        await _tracker.UpdateWithFeedback(
            SimpleUserRating.WithoutComment(highEffectivenessConstraint, RatingValue.ThumbsUp, "session-2"));

        await _tracker.UpdateWithFeedback(
            SimpleUserRating.WithoutComment(mediumEffectivenessConstraint, RatingValue.ThumbsUp, "session-3"));
        await _tracker.UpdateWithFeedback(
            SimpleUserRating.WithoutComment(mediumEffectivenessConstraint, RatingValue.ThumbsDown, "session-4"));

        await _tracker.UpdateWithFeedback(
            SimpleUserRating.WithoutComment(lowEffectivenessConstraint, RatingValue.ThumbsDown, "session-5"));
        await _tracker.UpdateWithFeedback(
            SimpleUserRating.WithoutComment(lowEffectivenessConstraint, RatingValue.ThumbsDown, "session-6"));

        // When
        var result = await _tracker.GetTopConstraints(3);

        // Then
        Assert.That(result.IsSuccess, Is.True);

        var topConstraints = result.Value;
        Assert.That(topConstraints, Is.Not.Empty);
        Assert.That(topConstraints.Count, Is.LessThanOrEqualTo(3));

        // Verify ordering by effectiveness score (descending)
        for (int i = 0; i < topConstraints.Count - 1; i++)
        {
            Assert.That(topConstraints[i].EffectivenessScore,
                Is.GreaterThanOrEqualTo(topConstraints[i + 1].EffectivenessScore),
                "Top constraints should be ordered by effectiveness score descending");
        }
    }

    [Test]
    public async Task Should_Complete_Effectiveness_Calculation_Within_Performance_Budget()
    {
        // Given
        const string constraintId = "performance-test-constraint";
        const int performanceBudgetMs = 50;

        // When
        var startTime = DateTime.UtcNow;
        var result = await _tracker!.CalculateEffectiveness(constraintId);
        var duration = DateTime.UtcNow - startTime;

        // Then
        Assert.That(result.IsSuccess, Is.True, "Performance test calculation should succeed");
        Assert.That(duration.TotalMilliseconds, Is.LessThan(performanceBudgetMs),
            $"Effectiveness calculation must complete within {performanceBudgetMs}ms budget");
    }

    [Test]
    public async Task Should_Complete_Feedback_Update_Within_Performance_Budget()
    {
        // Given
        const string constraintId = "performance-update-constraint";
        const int performanceBudgetMs = 50;
        var rating = SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsUp, "session-perf");

        // When
        var startTime = DateTime.UtcNow;
        var result = await _tracker!.UpdateWithFeedback(rating);
        var duration = DateTime.UtcNow - startTime;

        // Then
        Assert.That(result.IsSuccess, Is.True, "Performance test update should succeed");
        Assert.That(duration.TotalMilliseconds, Is.LessThan(performanceBudgetMs),
            $"Feedback update must complete within {performanceBudgetMs}ms budget");
    }

    [Test]
    public async Task Should_Handle_Invalid_Constraint_Id_Gracefully()
    {
        // Given
        var invalidConstraintIds = new[] { "", "   ", null! };

        // When & Then
        foreach (var invalidId in invalidConstraintIds)
        {
            var result = await _tracker!.CalculateEffectiveness(invalidId);
            Assert.That(result.IsError, Is.True,
                $"Should return error for invalid constraint ID: '{invalidId}'");
        }
    }

    [Test]
    public async Task Should_Handle_Null_Rating_In_Update_Gracefully()
    {
        // Given
        SimpleUserRating? nullRating = null;

        // When
        var result = await _tracker!.UpdateWithFeedback(nullRating!);

        // Then
        Assert.That(result.IsError, Is.True, "Should return error for null rating");
    }
}
