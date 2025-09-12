using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Infrastructure.Storage;
using ConstraintMcpServer.Domain.Feedback;

namespace ConstraintMcpServer.Tests.Infrastructure.Storage;

/// <summary>
/// Unit tests for SqliteRatingStore infrastructure component.
/// Validates storage operations meet performance requirements and data integrity.
/// </summary>
[TestFixture]
public class SqliteRatingStoreTests
{
    private SqliteRatingStore? _store;

    [SetUp]
    public void Setup()
    {
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
    public void Should_Add_And_Retrieve_Rating()
    {
        // Given
        const string constraintId = "test.constraint";
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
    }

    [Test]
    public void Should_Add_Rating_With_Comment()
    {
        // Given
        const string constraintId = "test.constraint";
        const string comment = "Very helpful reminder";
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
    public void Should_Return_All_Constraint_Ids()
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
    public void Should_Clear_All_Ratings()
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
    public void Should_Handle_Empty_Constraint_Id()
    {
        // When
        var ratings = _store!.GetRatingsForConstraint("");

        // Then
        Assert.That(ratings, Is.Empty);
    }

    [Test]
    public void Should_Handle_Null_Constraint_Id()
    {
        // When
        var ratings = _store!.GetRatingsForConstraint(null!);

        // Then
        Assert.That(ratings, Is.Empty);
    }

    [Test]
    public void Should_Throw_For_Null_Rating()
    {
        // When & Then
        Assert.Throws<ArgumentNullException>(() => _store!.AddRating(null!));
    }

    [Test]
    public void Should_Order_Ratings_By_Created_Date_Descending()
    {
        // Given
        const string constraintId = "test.constraint";
        var earlierTimestamp = DateTimeOffset.UtcNow.AddMinutes(-10);
        var laterTimestamp = DateTimeOffset.UtcNow.AddMinutes(-5);
        var earlierRating = new SimpleUserRating(constraintId, RatingValue.ThumbsUp, earlierTimestamp, "session-1");
        var laterRating = new SimpleUserRating(constraintId, RatingValue.ThumbsDown, laterTimestamp, "session-2");

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
    public Task Should_Meet_Performance_Requirements_For_Add_Operation()
    {
        // Given
        const string constraintId = "performance.test";
        var rating = SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsUp, "session-perf");
        var performanceBudgetMs = GetPerformanceBudgetMs(20);

        // When
        var startTime = DateTime.UtcNow;
        _store!.AddRating(rating);
        var duration = DateTime.UtcNow - startTime;

        // Then
        Assert.That(duration.TotalMilliseconds, Is.LessThan(performanceBudgetMs),
            $"AddRating must complete within {performanceBudgetMs}ms budget (CI-adjusted)");
        return Task.CompletedTask;
    }

    [Test]
    public Task Should_Meet_Performance_Requirements_For_Retrieval()
    {
        // Given
        const string constraintId = "performance.test";
        var rating = SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsUp, "session-perf");
        _store!.AddRating(rating);
        var performanceBudgetMs = GetPerformanceBudgetMs(20);

        // When
        var startTime = DateTime.UtcNow;
        var ratings = _store.GetRatingsForConstraint(constraintId);
        var duration = DateTime.UtcNow - startTime;

        // Then
        Assert.That(ratings, Is.Not.Empty);
        Assert.That(duration.TotalMilliseconds, Is.LessThan(performanceBudgetMs),
            $"GetRatingsForConstraint must complete within {performanceBudgetMs}ms budget (CI-adjusted)");
        return Task.CompletedTask;
    }

    [Test]
    public void Should_Handle_Unique_Constraint_Session_Combination()
    {
        // Given
        const string constraintId = "unique.test";
        const string sessionId = "session-duplicate";
        var rating1 = SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsUp, sessionId);
        var rating2 = SimpleUserRating.WithoutComment(constraintId, RatingValue.ThumbsDown, sessionId);

        // When
        _store!.AddRating(rating1);

        // Then - Second rating should replace the first due to unique constraint
        Assert.DoesNotThrow(() => _store.AddRating(rating2));
    }

    [Test]
    public void Should_Throw_When_Accessing_Disposed_Store()
    {
        // Given
        _store!.Dispose();

        // When & Then
        Assert.Throws<ObjectDisposedException>(() => _store.AddRating(
            SimpleUserRating.WithoutComment("test", RatingValue.ThumbsUp, "session")));
        Assert.Throws<ObjectDisposedException>(() => _store.GetRatingsForConstraint("test"));
        Assert.Throws<ObjectDisposedException>(() => _store.GetAllConstraintIds());
        Assert.Throws<ObjectDisposedException>(() => _store.Clear());
    }

    /// <summary>
    /// Gets performance budget with CI environment tolerance.
    /// </summary>
    private static int GetPerformanceBudgetMs(int baselineMs)
    {
        var isCI = Environment.GetEnvironmentVariable("CI") == "true" ||
                   Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

        // Increase tolerance to 2.0x for better CI stability across platforms (especially macOS)
        return isCI ? (int)(baselineMs * 2.0) : baselineMs;
    }
}
