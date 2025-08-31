namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Represents the result of attempting to skip refactoring levels.
/// Indicates success or failure with appropriate error messaging.
/// </summary>
public sealed record ProgressiveSkipResult
{
    /// <summary>
    /// Gets a value indicating whether the skip attempt was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the target level if successful, or null if failed.
    /// </summary>
    public int? TargetLevel { get; }

    /// <summary>
    /// Gets the error message if the skip attempt failed.
    /// </summary>
    public string Error { get; }

    private ProgressiveSkipResult(bool isSuccess, int? targetLevel, string error)
    {
        IsSuccess = isSuccess;
        TargetLevel = targetLevel;
        Error = error;
    }

    /// <summary>
    /// Creates a successful skip result.
    /// </summary>
    /// <param name="targetLevel">The target level that was successfully reached</param>
    /// <returns>Successful skip result</returns>
    public static ProgressiveSkipResult Success(int targetLevel)
    {
        return new ProgressiveSkipResult(true, targetLevel, string.Empty);
    }

    /// <summary>
    /// Creates a failed skip result.
    /// </summary>
    /// <param name="error">The error message explaining why the skip failed</param>
    /// <returns>Failed skip result with error message</returns>
    public static ProgressiveSkipResult Failure(string error)
    {
        if (string.IsNullOrEmpty(error))
        {
            throw new ArgumentException("Error message cannot be null or empty", nameof(error));
        }

        return new ProgressiveSkipResult(false, null, error);
    }

    /// <summary>
    /// Gets the error reason category for failed skip attempts.
    /// </summary>
    /// <returns>Error category for the skip failure</returns>
    public SkipFailureReason GetFailureReason()
    {
        if (IsSuccess)
        {
            return SkipFailureReason.None;
        }

        if (Error.Contains("prerequisite"))
        {
            return SkipFailureReason.MissingPrerequisites;
        }

        if (Error.Contains("systematic"))
        {
            return SkipFailureReason.SystematicProgressionRequired;
        }

        if (Error.Contains("current") || Error.Contains("previous"))
        {
            return SkipFailureReason.InvalidTargetLevel;
        }

        return SkipFailureReason.Other;
    }
}

/// <summary>
/// Represents the reason why a level skip attempt failed.
/// </summary>
public enum SkipFailureReason
{
    None,
    MissingPrerequisites,
    SystematicProgressionRequired,
    InvalidTargetLevel,
    Other
}
