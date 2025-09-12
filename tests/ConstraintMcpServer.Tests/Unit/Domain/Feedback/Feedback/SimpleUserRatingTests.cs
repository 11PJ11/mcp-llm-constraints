using System;
using NUnit.Framework;
using ConstraintMcpServer.Domain.Feedback;

namespace ConstraintMcpServer.Tests.Domain.Feedback;

/// <summary>
/// Unit tests for SimpleUserRating domain model.
/// Validates immutable design, null safety, and business logic.
/// </summary>
[TestFixture]
public class SimpleUserRatingTests
{
    [Test]
    public void Should_Create_Rating_With_Required_Fields()
    {
        // Given
        const string constraintId = "tdd.test-first";
        const RatingValue rating = RatingValue.ThumbsUp;
        var timestamp = DateTimeOffset.UtcNow;
        const string sessionId = "session-123";

        // When
        var userRating = new SimpleUserRating(constraintId, rating, timestamp, sessionId);

        // Then
        Assert.That(userRating.ConstraintId, Is.EqualTo(constraintId));
        Assert.That(userRating.Rating, Is.EqualTo(rating));
        Assert.That(userRating.Timestamp, Is.EqualTo(timestamp));
        Assert.That(userRating.SessionId, Is.EqualTo(sessionId));
        Assert.That(userRating.Comment.HasValue, Is.False);
    }

    [Test]
    public void Should_Create_Rating_With_Comment_Using_Factory_Method()
    {
        // Given
        const string constraintId = "refactoring.level1";
        const RatingValue rating = RatingValue.ThumbsUp;
        const string sessionId = "session-456";
        const string comment = "Very helpful for code cleanup";

        // When
        var userRating = SimpleUserRating.WithComment(constraintId, rating, sessionId, comment);

        // Then
        Assert.That(userRating.ConstraintId, Is.EqualTo(constraintId));
        Assert.That(userRating.Rating, Is.EqualTo(rating));
        Assert.That(userRating.SessionId, Is.EqualTo(sessionId));
        Assert.That(userRating.Comment.HasValue, Is.True);
        Assert.That(userRating.Comment.Value, Is.EqualTo(comment));
        Assert.That(userRating.IsPositive, Is.True);
    }

    [Test]
    public void Should_Create_Rating_Without_Comment_Using_Factory_Method()
    {
        // Given
        const string constraintId = "bdd.given-when-then";
        const RatingValue rating = RatingValue.ThumbsDown;
        const string sessionId = "session-789";

        // When
        var userRating = SimpleUserRating.WithoutComment(constraintId, rating, sessionId);

        // Then
        Assert.That(userRating.ConstraintId, Is.EqualTo(constraintId));
        Assert.That(userRating.Rating, Is.EqualTo(rating));
        Assert.That(userRating.SessionId, Is.EqualTo(sessionId));
        Assert.That(userRating.Comment.HasValue, Is.False);
        Assert.That(userRating.IsNegative, Is.True);
    }

    [Test]
    public void Should_Throw_Exception_When_ConstraintId_Is_Null_Or_Empty()
    {
        // Given
        var timestamp = DateTimeOffset.UtcNow;
        const string sessionId = "session-123";

        // When & Then
        Assert.Throws<ArgumentException>(() =>
            new SimpleUserRating(null!, RatingValue.ThumbsUp, timestamp, sessionId));

        Assert.Throws<ArgumentException>(() =>
            new SimpleUserRating("", RatingValue.ThumbsUp, timestamp, sessionId));

        Assert.Throws<ArgumentException>(() =>
            new SimpleUserRating("   ", RatingValue.ThumbsUp, timestamp, sessionId));
    }

    [Test]
    public void Should_Throw_Exception_When_SessionId_Is_Null_Or_Empty()
    {
        // Given
        const string constraintId = "test-constraint";
        var timestamp = DateTimeOffset.UtcNow;

        // When & Then
        Assert.Throws<ArgumentException>(() =>
            new SimpleUserRating(constraintId, RatingValue.ThumbsUp, timestamp, null!));

        Assert.Throws<ArgumentException>(() =>
            new SimpleUserRating(constraintId, RatingValue.ThumbsUp, timestamp, ""));

        Assert.Throws<ArgumentException>(() =>
            new SimpleUserRating(constraintId, RatingValue.ThumbsUp, timestamp, "   "));
    }

    [Test]
    public void Should_Throw_Exception_When_Rating_Value_Is_Invalid()
    {
        // Given
        const string constraintId = "test-constraint";
        var timestamp = DateTimeOffset.UtcNow;
        const string sessionId = "session-123";
        const RatingValue invalidRating = (RatingValue)999;

        // When & Then
        Assert.Throws<ArgumentException>(() =>
            new SimpleUserRating(constraintId, invalidRating, timestamp, sessionId));
    }

    [Test]
    public void Should_Provide_Correct_Boolean_Properties_For_Rating_Values()
    {
        // Given
        const string constraintId = "test-constraint";
        const string sessionId = "session-123";
        var timestamp = DateTimeOffset.UtcNow;

        // When
        var positiveRating = new SimpleUserRating(constraintId, RatingValue.ThumbsUp, timestamp, sessionId);
        var negativeRating = new SimpleUserRating(constraintId, RatingValue.ThumbsDown, timestamp, sessionId);
        var neutralRating = new SimpleUserRating(constraintId, RatingValue.Neutral, timestamp, sessionId);

        // Then
        Assert.That(positiveRating.IsPositive, Is.True);
        Assert.That(positiveRating.IsNegative, Is.False);
        Assert.That(positiveRating.IsNeutral, Is.False);

        Assert.That(negativeRating.IsPositive, Is.False);
        Assert.That(negativeRating.IsNegative, Is.True);
        Assert.That(negativeRating.IsNeutral, Is.False);

        Assert.That(neutralRating.IsPositive, Is.False);
        Assert.That(neutralRating.IsNegative, Is.False);
        Assert.That(neutralRating.IsNeutral, Is.True);
    }

    [Test]
    public void Should_Provide_Correct_Numeric_Values_For_Rating_Values()
    {
        // Given
        const string constraintId = "test-constraint";
        const string sessionId = "session-123";
        var timestamp = DateTimeOffset.UtcNow;

        // When
        var positiveRating = new SimpleUserRating(constraintId, RatingValue.ThumbsUp, timestamp, sessionId);
        var negativeRating = new SimpleUserRating(constraintId, RatingValue.ThumbsDown, timestamp, sessionId);
        var neutralRating = new SimpleUserRating(constraintId, RatingValue.Neutral, timestamp, sessionId);

        // Then
        Assert.That(positiveRating.NumericValue, Is.EqualTo(1));
        Assert.That(negativeRating.NumericValue, Is.EqualTo(-1));
        Assert.That(neutralRating.NumericValue, Is.EqualTo(0));
    }

    [Test]
    public void Should_Be_Immutable_Record_Type()
    {
        // Given
        const string constraintId = "test-constraint";
        const string sessionId = "session-123";
        var timestamp = DateTimeOffset.UtcNow;

        var rating1 = new SimpleUserRating(constraintId, RatingValue.ThumbsUp, timestamp, sessionId);
        var rating2 = new SimpleUserRating(constraintId, RatingValue.ThumbsUp, timestamp, sessionId);

        // When & Then
        Assert.That(rating1, Is.EqualTo(rating2)); // Value equality
        Assert.That(rating1.GetHashCode(), Is.EqualTo(rating2.GetHashCode())); // Hash code consistency

        // Verify immutability - no setters should exist
        var properties = typeof(SimpleUserRating).GetProperties();
        foreach (var property in properties)
        {
            Assert.That(property.CanWrite, Is.False,
                $"Property {property.Name} should be read-only for immutability");
        }
    }

    [Test]
    public void Should_Throw_Exception_When_Comment_Is_Empty_In_WithComment_Factory()
    {
        // Given
        const string constraintId = "test-constraint";
        const string sessionId = "session-123";

        // When & Then
        Assert.Throws<ArgumentException>(() =>
            SimpleUserRating.WithComment(constraintId, RatingValue.ThumbsUp, sessionId, ""));

        Assert.Throws<ArgumentException>(() =>
            SimpleUserRating.WithComment(constraintId, RatingValue.ThumbsUp, sessionId, "   "));
    }
}
