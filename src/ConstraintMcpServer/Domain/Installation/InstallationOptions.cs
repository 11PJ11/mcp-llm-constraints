using System;
using System.Runtime.InteropServices;

namespace ConstraintMcpServer.Domain.Installation;

/// <summary>
/// Configuration options for constraint server installation.
/// Following DDD: Value object representing installation preferences and constraints.
/// Immutable record with business validation and platform-specific factory methods.
/// </summary>
public sealed record InstallationOptions
{
    /// <summary>
    /// Target installation directory path.
    /// Business constraint: Must be valid, accessible directory path.
    /// </summary>
    public required string InstallationPath { get; init; }

    /// <summary>
    /// Target platform for installation.
    /// Business constraint: Must be supported platform (Windows, Linux, macOS).
    /// </summary>
    public required PlatformType TargetPlatform { get; init; }

    /// <summary>
    /// Whether to configure system PATH environment variable.
    /// Business preference: Enable for user-friendly command access.
    /// </summary>
    public bool ConfigureEnvironmentPath { get; init; } = true;

    /// <summary>
    /// Whether to create desktop shortcuts or application menu entries.
    /// Business preference: Platform-specific user interface integration.
    /// </summary>
    public bool CreateShortcuts { get; init; } = true;

    /// <summary>
    /// Whether to validate system requirements before installation.
    /// Business requirement: Ensure platform compatibility and dependencies.
    /// </summary>
    public bool ValidateRequirements { get; init; } = true;

    /// <summary>
    /// Create platform-specific installation options with sensible defaults.
    /// Business factory method: Platform-aware configuration generation.
    /// </summary>
    /// <param name="platform">Target platform for installation.</param>
    /// <returns>Installation options configured for specified platform.</returns>
    public static InstallationOptions ForPlatform(PlatformType platform)
    {
        return platform switch
        {
            PlatformType.Windows => new InstallationOptions
            {
                InstallationPath = @"C:\Program Files\ConstraintMcpServer",
                TargetPlatform = PlatformType.Windows,
                ConfigureEnvironmentPath = true,
                CreateShortcuts = true,
                ValidateRequirements = true
            },
            PlatformType.Linux => new InstallationOptions
            {
                InstallationPath = "/opt/constraint-mcp-server",
                TargetPlatform = PlatformType.Linux,
                ConfigureEnvironmentPath = true,
                CreateShortcuts = false, // No GUI shortcuts on server Linux
                ValidateRequirements = true
            },
            PlatformType.MacOS => new InstallationOptions
            {
                InstallationPath = "/Applications/ConstraintMcpServer.app",
                TargetPlatform = PlatformType.MacOS,
                ConfigureEnvironmentPath = true,
                CreateShortcuts = true,
                ValidateRequirements = true
            },
            _ => throw new ArgumentException($"Unsupported platform: {platform}", nameof(platform))
        };
    }

    /// <summary>
    /// Create custom installation options with specified path and platform detection.
    /// Business factory method: User-specified path with automatic platform detection.
    /// </summary>
    /// <param name="installationPath">Custom installation directory path.</param>
    /// <returns>Installation options with specified path and detected platform.</returns>
    public static InstallationOptions WithPath(string installationPath)
    {
        if (string.IsNullOrWhiteSpace(installationPath))
        {
            throw new ArgumentException("Installation path cannot be null or whitespace.", nameof(installationPath));
        }

        var currentPlatform = DetectCurrentPlatform();
        var defaults = ForPlatform(currentPlatform);

        return defaults with { InstallationPath = installationPath };
    }

    /// <summary>
    /// Detect current runtime platform for automatic platform selection.
    /// Business utility: Cross-platform compatibility detection.
    /// </summary>
    /// <returns>Current platform type based on runtime detection.</returns>
    private static PlatformType DetectCurrentPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return PlatformType.Windows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return PlatformType.Linux;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return PlatformType.MacOS;
        }

        throw new PlatformNotSupportedException("Current platform is not supported for installation.");
    }
}

/// <summary>
/// Supported platform types for constraint server installation.
/// Following DDD: Enumeration representing business-relevant platform categories.
/// </summary>
public enum PlatformType
{
    /// <summary>Microsoft Windows platform.</summary>
    Windows = 1,

    /// <summary>Linux platform (various distributions).</summary>
    Linux = 2,

    /// <summary>Apple macOS platform.</summary>
    MacOS = 3
}
