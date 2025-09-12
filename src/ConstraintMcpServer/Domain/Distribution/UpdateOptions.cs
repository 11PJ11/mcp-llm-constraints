namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Configuration options for system update process.
/// Business value: User control over update behavior and configuration preservation.
/// </summary>
public sealed record UpdateOptions
{
    /// <summary>
    /// Target version to update to. If null, updates to latest version.
    /// </summary>
    public string? TargetVersion { get; init; }

    /// <summary>
    /// Whether to preserve user configuration files during update.
    /// </summary>
    public bool PreserveConfiguration { get; init; } = true;

    /// <summary>
    /// Whether to create backup before update.
    /// </summary>
    public bool CreateBackup { get; init; } = true;

    /// <summary>
    /// Whether to validate update integrity.
    /// </summary>
    public bool ValidateIntegrity { get; init; } = true;

    /// <summary>
    /// Maximum time allowed for update process in seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 10;

    /// <summary>
    /// Whether to minimize service interruption during update.
    /// When true, prioritizes keeping services running during update process.
    /// </summary>
    public bool MinimizeServiceInterruption { get; init; } = false;

    /// <summary>
    /// Creates update options with default settings.
    /// </summary>
    public static UpdateOptions Default => new();
}
