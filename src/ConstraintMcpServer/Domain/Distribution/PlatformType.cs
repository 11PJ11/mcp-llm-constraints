namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Represents the target platform for constraint system installation.
/// Business value: Enables cross-platform installation with consistent experience.
/// </summary>
public enum PlatformType
{
    /// <summary>
    /// Linux platform with package manager support (apt, yum, snap).
    /// </summary>
    Linux,

    /// <summary>
    /// Windows platform with MSI installer and registry integration.
    /// </summary>
    Windows,

    /// <summary>
    /// macOS platform with Homebrew and .app bundle support.
    /// </summary>
    MacOS
}
