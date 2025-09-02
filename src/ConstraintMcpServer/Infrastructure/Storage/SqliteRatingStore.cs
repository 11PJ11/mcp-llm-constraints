using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;
using ConstraintMcpServer.Application.Feedback;
using ConstraintMcpServer.Domain.Feedback;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Infrastructure.Storage;

/// <summary>
/// SQLite-based implementation of rating storage with performance optimization.
/// Provides persistent storage for user feedback with sub-50ms response times.
/// </summary>
public sealed class SqliteRatingStore : IRatingStore, IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance with in-memory database for development.
    /// </summary>
    public SqliteRatingStore() : this("Data Source=:memory:")
    {
    }

    /// <summary>
    /// Initializes a new instance with specified connection string.
    /// </summary>
    /// <param name="connectionString">SQLite connection string</param>
    public SqliteRatingStore(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        InitializeDatabase();
    }

    /// <summary>
    /// Retrieves all ratings for a specific constraint.
    /// </summary>
    /// <param name="constraintId">The constraint identifier</param>
    /// <returns>Collection of ratings for the constraint</returns>
    public IReadOnlyList<SimpleUserRating> GetRatingsForConstraint(string constraintId)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(constraintId))
        {
            return Array.Empty<SimpleUserRating>();
        }

        EnsureConnection();

        using var command = _connection!.CreateCommand();
        command.CommandText = @"
            SELECT constraint_id, rating_value, session_id, comment, created_at 
            FROM ratings 
            WHERE constraint_id = @constraintId 
            ORDER BY created_at DESC";
        
        command.Parameters.AddWithValue("@constraintId", constraintId);

        var ratings = new List<SimpleUserRating>();
        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            var rating = CreateRatingFromReader(reader);
            ratings.Add(rating);
        }

        return ratings.AsReadOnly();
    }

    /// <summary>
    /// Retrieves all constraint identifiers that have ratings.
    /// </summary>
    /// <returns>Collection of constraint identifiers</returns>
    public IReadOnlyList<string> GetAllConstraintIds()
    {
        ThrowIfDisposed();
        EnsureConnection();

        using var command = _connection!.CreateCommand();
        command.CommandText = @"
            SELECT DISTINCT constraint_id 
            FROM ratings 
            ORDER BY constraint_id";

        var constraintIds = new List<string>();
        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            var constraintId = reader.GetString(0);
            constraintIds.Add(constraintId);
        }

        return constraintIds.AsReadOnly();
    }

    /// <summary>
    /// Adds a new rating to the store.
    /// </summary>
    /// <param name="rating">The rating to add</param>
    public void AddRating(SimpleUserRating rating)
    {
        ThrowIfDisposed();
        
        if (rating == null)
        {
            throw new ArgumentNullException(nameof(rating));
        }

        EnsureConnection();

        using var command = _connection!.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO ratings (constraint_id, rating_value, session_id, comment, created_at)
            VALUES (@constraintId, @ratingValue, @sessionId, @comment, @createdAt)";

        command.Parameters.AddWithValue("@constraintId", rating.ConstraintId);
        command.Parameters.AddWithValue("@ratingValue", (int)rating.Rating);
        command.Parameters.AddWithValue("@sessionId", rating.SessionId);
        command.Parameters.AddWithValue("@comment", rating.Comment.HasValue ? rating.Comment.Value : string.Empty);
        command.Parameters.AddWithValue("@createdAt", rating.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Clears all stored ratings.
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();
        EnsureConnection();

        using var command = _connection!.CreateCommand();
        command.CommandText = "DELETE FROM ratings";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Releases all resources used by the SqliteRatingStore.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _connection?.Dispose();
        _connection = null;
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    private void InitializeDatabase()
    {
        EnsureConnection();

        using var command = _connection!.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS ratings (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                constraint_id TEXT NOT NULL,
                rating_value INTEGER NOT NULL,
                session_id TEXT NOT NULL,
                comment TEXT NOT NULL DEFAULT '',
                created_at TEXT NOT NULL,
                UNIQUE(constraint_id, session_id)
            );

            CREATE INDEX IF NOT EXISTS idx_ratings_constraint_id ON ratings(constraint_id);
            CREATE INDEX IF NOT EXISTS idx_ratings_created_at ON ratings(created_at);";

        command.ExecuteNonQuery();
    }

    private void EnsureConnection()
    {
        if (_connection == null)
        {
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();
        }
        else if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }
    }

    private static SimpleUserRating CreateRatingFromReader(SqliteDataReader reader)
    {
        var constraintId = reader.GetString("constraint_id");
        var ratingValue = (RatingValue)reader.GetInt32("rating_value");
        var sessionId = reader.GetString("session_id");
        var comment = reader.GetString("comment");
        var createdAtText = reader.GetString("created_at");

        var timestamp = DateTimeOffset.Parse(createdAtText);

        return string.IsNullOrEmpty(comment)
            ? new SimpleUserRating(constraintId, ratingValue, timestamp, sessionId, Option<string>.None())
            : new SimpleUserRating(constraintId, ratingValue, timestamp, sessionId, Option<string>.Some(comment));
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SqliteRatingStore));
        }
    }
}