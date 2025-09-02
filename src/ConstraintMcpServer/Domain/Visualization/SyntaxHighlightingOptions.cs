namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Immutable options for syntax highlighting behavior.
/// Provides configuration for enhanced readability through color coding and formatting.
/// Follows CUPID properties: Composable, Predictable, Idiomatic, Domain-based.
/// </summary>
public sealed record SyntaxHighlightingOptions
{
    /// <summary>
    /// Gets whether to highlight constraint IDs with special formatting.
    /// </summary>
    public bool HighlightConstraintIds { get; init; } = true;

    /// <summary>
    /// Gets whether to highlight priority values with color coding based on importance.
    /// </summary>
    public bool HighlightPriorities { get; init; } = true;

    /// <summary>
    /// Gets whether to highlight tree structure symbols for better visual hierarchy.
    /// </summary>
    public bool HighlightTreeStructure { get; init; } = true;

    /// <summary>
    /// Gets whether to highlight keywords in constraint descriptions and triggers.
    /// </summary>
    public bool HighlightKeywords { get; init; } = true;

    /// <summary>
    /// Gets the color scheme to use for highlighting.
    /// </summary>
    public ColorProfile ColorProfile { get; init; } = ColorProfile.Standard;

    /// <summary>
    /// Gets whether to use ANSI color codes for terminal compatibility.
    /// </summary>
    public bool UseAnsiColors { get; init; } = false;

    /// <summary>
    /// Gets the maximum processing time allowed for syntax highlighting.
    /// </summary>
    public TimeSpan MaxProcessingTime { get; init; } = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Default options optimized for enhanced readability.
    /// </summary>
    public static SyntaxHighlightingOptions Default => new();

    /// <summary>
    /// Options optimized for minimal highlighting with focus on structure only.
    /// </summary>
    public static SyntaxHighlightingOptions Minimal => new()
    {
        HighlightConstraintIds = false,
        HighlightPriorities = false,
        HighlightTreeStructure = true,
        HighlightKeywords = false,
        ColorProfile = ColorProfile.Monochrome
    };

    /// <summary>
    /// Options optimized for high contrast accessibility.
    /// </summary>
    public static SyntaxHighlightingOptions HighContrast => new()
    {
        HighlightConstraintIds = true,
        HighlightPriorities = true,
        HighlightTreeStructure = true,
        HighlightKeywords = true,
        ColorProfile = ColorProfile.HighContrast
    };

    /// <summary>
    /// Options optimized for terminal/CLI environments with ANSI colors.
    /// </summary>
    public static SyntaxHighlightingOptions Terminal => new()
    {
        HighlightConstraintIds = true,
        HighlightPriorities = true,
        HighlightTreeStructure = true,
        HighlightKeywords = true,
        UseAnsiColors = true,
        ColorProfile = ColorProfile.Standard
    };

    /// <summary>
    /// Creates options with specific color profile while maintaining other defaults.
    /// </summary>
    /// <param name="colorProfile">The color profile to use</param>
    /// <returns>Options with specified color profile</returns>
    public SyntaxHighlightingOptions WithColorProfile(ColorProfile colorProfile) => this with { ColorProfile = colorProfile };

    /// <summary>
    /// Creates options with ANSI colors enabled/disabled while maintaining other settings.
    /// </summary>
    /// <param name="useAnsiColors">Whether to use ANSI color codes</param>
    /// <returns>Options with specified ANSI color setting</returns>
    public SyntaxHighlightingOptions WithAnsiColors(bool useAnsiColors) => this with { UseAnsiColors = useAnsiColors };

    /// <summary>
    /// Creates options with custom processing time limit while maintaining other settings.
    /// </summary>
    /// <param name="maxProcessingTime">Maximum allowed processing time</param>
    /// <returns>Options with specified time limit</returns>
    public SyntaxHighlightingOptions WithMaxProcessingTime(TimeSpan maxProcessingTime) => this with { MaxProcessingTime = maxProcessingTime };
}
