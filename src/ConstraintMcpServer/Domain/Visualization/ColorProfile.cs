namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Defines color profiles for enhanced tree visualization.
/// Provides color schemes optimized for different display contexts.
/// </summary>
public sealed record ColorProfile
{
    public string LibraryHeader { get; init; } = string.Empty;
    public string VersionInfo { get; init; } = string.Empty;
    public string SectionHeader { get; init; } = string.Empty;
    public string AtomicConstraint { get; init; } = string.Empty;
    public string CompositeConstraint { get; init; } = string.Empty;
    public string Metadata { get; init; } = string.Empty;

    /// <summary>
    /// Monochrome profile for Claude Code compatibility.
    /// </summary>
    public static ColorProfile Monochrome => new();

    /// <summary>
    /// Standard color profile with semantic colors.
    /// </summary>
    public static ColorProfile Standard => new()
    {
        LibraryHeader = "blue",
        VersionInfo = "gray",
        SectionHeader = "green",
        AtomicConstraint = "yellow",
        CompositeConstraint = "cyan",
        Metadata = "white"
    };

    /// <summary>
    /// High contrast profile for accessibility.
    /// </summary>
    public static ColorProfile HighContrast => new()
    {
        LibraryHeader = "white",
        VersionInfo = "bright_black",
        SectionHeader = "bright_green",
        AtomicConstraint = "bright_yellow",
        CompositeConstraint = "bright_cyan",
        Metadata = "bright_white"
    };
}
