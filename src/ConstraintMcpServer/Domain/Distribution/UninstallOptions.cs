namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Configuration options for system uninstall process.
/// Business value: User control over removal behavior and configuration preservation.
/// </summary>
public sealed record UninstallOptions
{
    /// <summary>
    /// Whether to preserve user configuration files during uninstall.
    /// </summary>
    public bool PreserveConfiguration { get; init; } = false;

    /// <summary>
    /// Whether to clean up environment PATH entries.
    /// </summary>
    public bool CleanupEnvironmentPath { get; init; } = true;

    /// <summary>
    /// Whether to remove configuration directories.
    /// </summary>
    public bool RemoveConfigurationDirectories { get; init; } = true;

    /// <summary>
    /// Whether to force removal even if conflicts exist.
    /// </summary>
    public bool ForceRemoval { get; init; } = false;

    /// <summary>
    /// Creates uninstall options with configuration preservation.
    /// </summary>
    public static UninstallOptions PreserveConfig => new() { PreserveConfiguration = true };

    /// <summary>
    /// Creates uninstall options for complete removal.
    /// </summary>
    public static UninstallOptions CompleteRemoval => new()
    {
        PreserveConfiguration = false,
        CleanupEnvironmentPath = true,
        RemoveConfigurationDirectories = true
    };
}
