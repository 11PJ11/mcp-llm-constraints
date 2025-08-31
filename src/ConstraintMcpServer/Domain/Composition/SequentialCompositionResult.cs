namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Result of sequential composition constraint selection.
/// Contains the next constraint to activate and the reasoning.
/// </summary>
public sealed record SequentialCompositionResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the constraint ID to activate next (if successful).
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Gets the reason for this activation decision.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// Gets the error message (if not successful).
    /// </summary>
    public string Error { get; init; } = string.Empty;

    /// <summary>
    /// Creates a successful result with constraint ID and reason.
    /// </summary>
    public static SequentialCompositionResult Success(string constraintId, string reason) =>
        new() { IsSuccess = true, Value = constraintId, Reason = reason };

    /// <summary>
    /// Creates a failed result with error message.
    /// </summary>
    public static SequentialCompositionResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
