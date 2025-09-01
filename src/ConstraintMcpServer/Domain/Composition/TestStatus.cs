namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Represents a user-defined evaluation status for any user-defined criteria.
/// This replaces the methodology-specific TestStatus with a generic system
/// that allows users to define their own evaluation criteria and states.
/// </summary>
public sealed class UserDefinedEvaluationStatus
{
    /// <summary>
    /// Gets the user-defined name of this evaluation status.
    /// Examples: "not-started", "in-progress", "completed", "validated", "blocked"
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the user-defined category this evaluation belongs to.
    /// Examples: "testing", "review", "validation", "quality-check", "approval"
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Gets whether this status indicates a positive/successful state.
    /// Users define what constitutes success in their methodology.
    /// </summary>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Initializes a new instance of UserDefinedEvaluationStatus.
    /// </summary>
    /// <param name="name">The user-defined status name.</param>
    /// <param name="category">The user-defined evaluation category.</param>
    /// <param name="isSuccessful">Whether this represents a successful state.</param>
    public UserDefinedEvaluationStatus(string name, string category, bool isSuccessful)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        IsSuccessful = isSuccessful;
    }

    /// <summary>
    /// Creates commonly used evaluation statuses from user configuration.
    /// </summary>
    /// <param name="statusName">User-defined status name.</param>
    /// <param name="categoryName">User-defined category name.</param>
    /// <param name="isSuccessful">Whether this status indicates success.</param>
    /// <returns>A new UserDefinedEvaluationStatus instance.</returns>
    public static UserDefinedEvaluationStatus FromUserDefinition(string statusName, string categoryName, bool isSuccessful)
    {
        return new UserDefinedEvaluationStatus(statusName, categoryName, isSuccessful);
    }

    public override string ToString() => $"{Category}: {Name} ({(IsSuccessful ? "Success" : "Pending")})";
}
