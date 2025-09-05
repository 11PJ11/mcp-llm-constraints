namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Result of system update operation.
/// Business value: Detailed feedback on update success and configuration preservation.
/// </summary>
public sealed record UpdateResult
{
    /// <summary>
    /// Whether the update completed successfully.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Version that was installed.
    /// </summary>
    public string? InstalledVersion { get; init; }

    /// <summary>
    /// Previous version that was replaced.
    /// </summary>
    public string? PreviousVersion { get; init; }

    /// <summary>
    /// Time taken for update in seconds.
    /// </summary>
    public double UpdateTimeSeconds { get; init; }

    /// <summary>
    /// Whether user configurations were preserved.
    /// </summary>
    public bool ConfigurationPreserved { get; init; }

    /// <summary>
    /// Error message if update failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Additional details about the update process.
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// Creates successful update result.
    /// </summary>
    public static UpdateResult Success(string installedVersion, string previousVersion, double timeSeconds, bool configPreserved) =>
        new()
        {
            IsSuccess = true,
            InstalledVersion = installedVersion,
            PreviousVersion = previousVersion,
            UpdateTimeSeconds = timeSeconds,
            ConfigurationPreserved = configPreserved
        };

    /// <summary>
    /// Creates failed update result.
    /// </summary>
    public static UpdateResult Failure(string errorMessage, string? details = null) =>
        new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Details = details
        };
}
