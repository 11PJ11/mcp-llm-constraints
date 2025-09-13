using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Installation;

/// <summary>
/// Result of constraint server installation operation.
/// Following DDD: Value object representing installation outcome with business meaning.
/// Immutable record with success/failure semantics and detailed operation results.
/// </summary>
public sealed record InstallationResult
{
    /// <summary>
    /// Whether installation completed successfully.
    /// Business outcome: Primary success indicator for installation workflow.
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Path where installation was performed.
    /// Business information: Actual installation location for reference and validation.
    /// </summary>
    public required string InstallationPath { get; init; }

    /// <summary>
    /// Platform where installation was performed.
    /// Business context: Target platform for compatibility and troubleshooting reference.
    /// </summary>
    public required PlatformType TargetPlatform { get; init; }

    /// <summary>
    /// Human-readable message describing installation outcome.
    /// Business communication: User-facing description of installation result.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Any errors or warnings encountered during installation.
    /// Business diagnostics: Detailed issue information for troubleshooting.
    /// </summary>
    public IReadOnlyList<string> Issues { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Components successfully installed during the process.
    /// Business tracking: Detailed breakdown of completed installation elements.
    /// </summary>
    public IReadOnlyList<string> InstalledComponents { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Duration of installation process in milliseconds.
    /// Business metrics: Performance tracking for installation operations.
    /// </summary>
    public long InstallationDurationMs { get; init; }

    /// <summary>
    /// Timestamp when installation was completed.
    /// Business audit: Installation completion time for tracking and validation.
    /// </summary>
    public DateTimeOffset CompletedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Create successful installation result with specified details.
    /// Business factory method: Success case with installation confirmation.
    /// </summary>
    /// <param name="installationPath">Path where installation was completed successfully.</param>
    /// <param name="targetPlatform">Platform where installation was performed.</param>
    /// <param name="installedComponents">List of components successfully installed.</param>
    /// <param name="durationMs">Installation duration in milliseconds.</param>
    /// <returns>Installation result indicating successful completion.</returns>
    public static InstallationResult Success(
        string installationPath,
        PlatformType targetPlatform,
        IReadOnlyList<string> installedComponents,
        long durationMs)
    {
        return new InstallationResult
        {
            IsSuccess = true,
            InstallationPath = installationPath,
            TargetPlatform = targetPlatform,
            Message = "Installation completed successfully",
            InstalledComponents = installedComponents,
            InstallationDurationMs = durationMs,
            Issues = Array.Empty<string>()
        };
    }

    /// <summary>
    /// Create failed installation result with error details.
    /// Business factory method: Failure case with diagnostic information.
    /// </summary>
    /// <param name="installationPath">Path where installation was attempted.</param>
    /// <param name="targetPlatform">Platform where installation was attempted.</param>
    /// <param name="errorMessage">Primary error message describing failure.</param>
    /// <param name="issues">Detailed list of issues encountered.</param>
    /// <returns>Installation result indicating failure with diagnostic details.</returns>
    public static InstallationResult Failure(
        string installationPath,
        PlatformType targetPlatform,
        string errorMessage,
        IReadOnlyList<string> issues)
    {
        return new InstallationResult
        {
            IsSuccess = false,
            InstallationPath = installationPath,
            TargetPlatform = targetPlatform,
            Message = errorMessage,
            Issues = issues,
            InstalledComponents = Array.Empty<string>(),
            InstallationDurationMs = 0
        };
    }
}
