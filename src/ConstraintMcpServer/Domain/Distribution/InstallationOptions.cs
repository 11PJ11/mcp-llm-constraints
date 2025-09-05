namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Configuration options for professional installation process.
/// Business value: Provides user control over installation behavior and preferences.
/// </summary>
public sealed record InstallationOptions
{
    /// <summary>
    /// Target platform for installation.
    /// </summary>
    public PlatformType Platform { get; init; }

    /// <summary>
    /// Installation directory path. If null, uses platform default.
    /// </summary>
    public string? InstallationPath { get; init; }

    /// <summary>
    /// Whether to automatically configure environment PATH.
    /// </summary>
    public bool ConfigureEnvironmentPath { get; init; } = true;

    /// <summary>
    /// Whether to create default configuration files.
    /// </summary>
    public bool CreateDefaultConfiguration { get; init; } = true;

    /// <summary>
    /// Whether to perform installation validation after completion.
    /// </summary>
    public bool ValidateInstallation { get; init; } = true;

    /// <summary>
    /// Maximum time allowed for installation process in seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Creates installation options for the specified platform with default settings.
    /// </summary>
    public static InstallationOptions ForPlatform(PlatformType platform) =>
        new() { Platform = platform };
}
