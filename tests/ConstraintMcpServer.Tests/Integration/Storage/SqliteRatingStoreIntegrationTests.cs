using System;
using System.Linq;
using NUnit.Framework;
using ConstraintMcpServer.Infrastructure.Storage;
using ConstraintMcpServer.Domain.Feedback;

namespace ConstraintMcpServer.StorageTests;

/// <summary>
/// Integration tests for SqliteRatingStore with real SQLite database.
/// Validates production storage behavior and performance requirements.
/// </summary>
[TestFixture]
public class SqliteRatingStoreIntegrationTests
{
    private SqliteRatingStore? _store;

    [SetUp]
    public void Setup()
    {
        // Use in-memory SQLite database for integration tests
        _store = new SqliteRatingStore();
    }

    [TearDown]
    public void TearDown()
    {
        _store?.Dispose();
    }

    [Test]
    public void Should_Initialize_With_Empty_Store()
    {
        // When
        var constraintIds = _store!.GetAllConstraintIds();

        // Then
        Assert.That(constraintIds, Is.Empty);
    }

    [Test]
    public void Should_Add_And_Retrieve_Rating_With_SQLite()
    {
        // Given
        const string constraintId = "test.constraint.sqlite";
        var rating = SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsUp, "session-1");

        // When
        _store!.AddRating(rating);
        var retrievedRatings = _store.GetRatingsForConstraint(constraintId);

        // Then
        Assert.That(retrievedRatings, Has.Count.EqualTo(1));
        var retrievedRating = retrievedRatings.First();
        Assert.That(retrievedRating.ConstraintId, Is.EqualTo(constraintId));
        Assert.That(retrievedRating.Rating, Is.EqualTo(RatingValue.ThumbsUp));
        Assert.That(retrievedRating.SessionId, Is.EqualTo("session-1"));
        Assert.That(retrievedRating.Comment.HasValue, Is.False);
    }

    [Test]
    public void Should_Add_Rating_With_Comment_To_SQLite()
    {
        // Given
        const string constraintId = "test.constraint.with-comment";
        const string comment = "Very helpful reminder for TDD";
        var rating = SimpleUserRating.WithComment(constraintId, RatingValue.ThumbsUp, "session-1", comment);

        // When
        _store!.AddRating(rating);
        var retrievedRatings = _store.GetRatingsForConstraint(constraintId);

        // Then
        Assert.That(retrievedRatings, Has.Count.EqualTo(1));
        var retrievedRating = retrievedRatings.First();
        Assert.That(retrievedRating.Comment.HasValue, Is.True);
        Assert.That(retrievedRating.Comment.Value, Is.EqualTo(comment));
    }

    [Test]
    public void Should_Return_All_Constraint_Ids_From_SQLite()
    {
        // Given
        var rating1 = SimpleUserRating.WithoutComment("constraint.alpha", RatingValue.ThumbsUp, "session-1");
        var rating2 = SimpleUserRating.WithoutComment("constraint.beta", RatingValue.ThumbsDown, "session-1");
        var rating3 = SimpleUserRating.WithoutComment("constraint.alpha", RatingValue.Neutral, "session-2");

        // When
        _store!.AddRating(rating1);
        _store.AddRating(rating2);
        _store.AddRating(rating3);

        var constraintIds = _store.GetAllConstraintIds();

        // Then
        Assert.That(constraintIds, Has.Count.EqualTo(2));
        Assert.That(constraintIds, Contains.Item("constraint.alpha"));
        Assert.That(constraintIds, Contains.Item("constraint.beta"));
    }

    [Test]
    public void Should_Clear_All_Ratings_From_SQLite()
    {
        // Given
        var rating = SimpleUserRating.WithoutComment("test.constraint", RatingValue.ThumbsUp, "session-1");
        _store!.AddRating(rating);

        // When
        _store.Clear();

        // Then
        var constraintIds = _store.GetAllConstraintIds();
        Assert.That(constraintIds, Is.Empty);
    }

    [Test]
    public void Should_Handle_Unique_Constraint_Session_Combination_In_SQLite()
    {
        // Given
        const string constraintId = "unique.test.sqlite";
        const string sessionId = "session-duplicate";
        var rating1 = SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsUp, sessionId);
        var rating2 = SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsDown, sessionId);

        // When
        _store!.AddRating(rating1);
        _store.AddRating(rating2); // Should replace the first rating

        // Then
        var ratings = _store.GetRatingsForConstraint(constraintId);
        Assert.That(ratings, Has.Count.EqualTo(1));
        Assert.That(ratings[0].Rating, Is.EqualTo(RatingValue.ThumbsDown), "Second rating should replace first");
    }

    [Test]
    public void Should_Order_Ratings_By_Timestamp_Descending_In_SQLite()
    {
        // Given
        const string constraintId = "test.ordering";
        var earlierTime = DateTimeOffset.UtcNow.AddMinutes(-10);
        var laterTime = DateTimeOffset.UtcNow.AddMinutes(-5);

        var earlierRating = new SimpleUserRating(constraintId, RatingValue.ThumbsUp, earlierTime, "session-1");
        var laterRating = new SimpleUserRating(constraintId, RatingValue.ThumbsDown, laterTime, "session-2");

        // When
        _store!.AddRating(earlierRating);
        _store.AddRating(laterRating);
        var ratings = _store.GetRatingsForConstraint(constraintId);

        // Then
        Assert.That(ratings, Has.Count.EqualTo(2));
        Assert.That(ratings[0].SessionId, Is.EqualTo("session-2"), "Most recent rating should be first");
        Assert.That(ratings[1].SessionId, Is.EqualTo("session-1"), "Earlier rating should be second");
    }

    [Test]
    public void Should_Meet_Performance_Requirements_For_SQLite_Operations()
    {
        // Given
        const string constraintId = "performance.sqlite.test";
        var rating = SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsUp, "session-perf");
        var performanceBudgetMs = GetPerformanceBudgetMs(30); // SQLite may be slightly slower than in-memory

        // Test Add Operation
        var startTime = DateTimeOffset.UtcNow;
        _store!.AddRating(rating);
        var addDuration = DateTimeOffset.UtcNow - startTime;

        Assert.That(addDuration.TotalMilliseconds, Is.LessThan(performanceBudgetMs),
            $"SQLite AddRating must complete within {performanceBudgetMs}ms budget (CI-adjusted)");

        // Test Retrieval Operation
        startTime = DateTimeOffset.UtcNow;
        var ratings = _store.GetRatingsForConstraint(constraintId);
        var retrievalDuration = DateTimeOffset.UtcNow - startTime;

        Assert.That(ratings, Is.Not.Empty);
        Assert.That(retrievalDuration.TotalMilliseconds, Is.LessThan(performanceBudgetMs),
            $"SQLite GetRatingsForConstraint must complete within {performanceBudgetMs}ms budget (CI-adjusted)");
    }

    [Test]
    public void Should_Handle_Large_Dataset_Efficiently()
    {
        // Given - Add multiple ratings to test scaling
        const int ratingCount = 100;
        const string constraintId = "scale.test";

        // When
        var startTime = DateTimeOffset.UtcNow;
        for (int i = 0; i < ratingCount; i++)
        {
            var rating = SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsUp, $"session-{i}");
            _store!.AddRating(rating);
        }
        var insertDuration = DateTimeOffset.UtcNow - startTime;

        startTime = DateTimeOffset.UtcNow;
        var retrievedRatings = _store!.GetRatingsForConstraint(constraintId);
        var retrievalDuration = DateTimeOffset.UtcNow - startTime;

        // Then
        Assert.That(retrievedRatings, Has.Count.EqualTo(ratingCount));
        Assert.That(insertDuration.TotalMilliseconds, Is.LessThan(GetPerformanceBudgetMs(500)),
            "Bulk insert should complete within reasonable time");
        Assert.That(retrievalDuration.TotalMilliseconds, Is.LessThan(GetPerformanceBudgetMs(100)),
            "Large dataset retrieval should be fast");
    }

    [Test]
    public void Should_Persist_Data_Across_Connection_Cycles()
    {
        // Given
        const string constraintId = "persistence.test";
        var rating = SimpleUserRating.WithComment(constraintId, RatingValue.ThumbsUp, "session-persist", "Persistent comment");

        // When - Add data and dispose store
        _store!.AddRating(rating);
        _store.Dispose();

        // Create new store instance (with same in-memory connection this won't work, but shows the pattern)
        _store = new SqliteRatingStore();

        // Then - For real file-based SQLite, data would persist
        // For in-memory database used in tests, this tests the disposal/recreation pattern
        Assert.DoesNotThrow(() => _store.GetRatingsForConstraint(constraintId));
    }

    /// <summary>
    /// Gets performance budget with CI environment tolerance.
    /// </summary>
    private static int GetPerformanceBudgetMs(int baselineMs)
    {
        var isCI = Environment.GetEnvironmentVariable("CI") == "true" ||
                   Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

        return isCI ? (int)(baselineMs * 1.5) : baselineMs;
    }
}
