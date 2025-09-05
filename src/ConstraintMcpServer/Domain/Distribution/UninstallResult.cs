namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Result of system uninstall operation.
/// Business value: Detailed feedback on uninstall success and configuration preservation.
/// </summary>
public sealed record UninstallResult
{
    /// <summary>
    /// Whether the uninstall completed successfully.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Whether user configurations were preserved.
    /// </summary>
    public bool ConfigurationPreserved { get; init; }

    /// <summary>
    /// Whether environment PATH was cleaned up.
    /// </summary>
    public bool EnvironmentPathCleaned { get; init; }

    /// <summary>
    /// List of items that were removed.
    /// </summary>
    public IReadOnlyList<string> RemovedItems { get; init; } = Array.Empty<string>();

    /// <summary>
    /// List of items that were preserved.
    /// </summary>
    public IReadOnlyList<string> PreservedItems { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Error message if uninstall failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Additional details about the uninstall process.
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// Creates successful uninstall result.
    /// </summary>
    public static UninstallResult Success(bool configPreserved, bool pathCleaned,
        IReadOnlyList<string>? removed = null, IReadOnlyList<string>? preserved = null) =>
        new()
        {
            IsSuccess = true,
            ConfigurationPreserved = configPreserved,
            EnvironmentPathCleaned = pathCleaned,
            RemovedItems = removed ?? Array.Empty<string>(),
            PreservedItems = preserved ?? Array.Empty<string>()
        };

    /// <summary>
    /// Creates failed uninstall result.
    /// </summary>
    public static UninstallResult Failure(string errorMessage, string? details = null) =>
        new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Details = details
        };
}
