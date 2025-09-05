using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Result of professional installation operation.
/// Business value: Provides detailed feedback on installation success and system state.
/// </summary>
public sealed record InstallationResult
{
    /// <summary>
    /// Whether the installation completed successfully.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Path where the system was installed.
    /// </summary>
    public string? InstallationPath { get; init; }

    /// <summary>
    /// Platform where installation was performed.
    /// </summary>
    public PlatformType Platform { get; init; }

    /// <summary>
    /// Time taken for installation in seconds.
    /// </summary>
    public double InstallationTimeSeconds { get; init; }

    /// <summary>
    /// Whether configuration directories were created.
    /// </summary>
    public bool ConfigurationCreated { get; init; }

    /// <summary>
    /// Whether environment PATH was configured.
    /// </summary>
    public bool PathConfigured { get; init; }

    /// <summary>
    /// Error message if installation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Additional details about the installation process.
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// Creates successful installation result.
    /// </summary>
    public static InstallationResult Success(string installationPath, PlatformType platform, double timeSeconds, bool configurationCreated, bool pathConfigured) =>
        new()
        {
            IsSuccess = true,
            InstallationPath = installationPath,
            Platform = platform,
            InstallationTimeSeconds = timeSeconds,
            ConfigurationCreated = configurationCreated,
            PathConfigured = pathConfigured
        };

    /// <summary>
    /// Creates failed installation result.
    /// </summary>
    public static InstallationResult Failure(PlatformType platform, string errorMessage, string? details = null) =>
        new()
        {
            IsSuccess = false,
            Platform = platform,
            ErrorMessage = errorMessage,
            Details = details
        };
}
