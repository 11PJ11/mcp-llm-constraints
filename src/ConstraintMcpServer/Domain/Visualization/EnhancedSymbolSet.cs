namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Defines symbol sets for enhanced tree visualization.
/// Provides character sets optimized for different display contexts.
/// </summary>
public sealed record EnhancedSymbolSet
{
    public string Branch { get; init; } = "+-- ";
    public string Vertical { get; init; } = "|   ";
    public string AtomicConstraint { get; init; } = "⚛️ ";
    public string CompositeConstraint { get; init; } = "🧩 ";
    public string AtomicSection { get; init; } = "📋";
    public string CompositeSection { get; init; } = "🏗️ ";

    /// <summary>
    /// ASCII symbols for maximum compatibility.
    /// </summary>
    public static EnhancedSymbolSet Ascii => new()
    {
        Branch = "+-- ",
        Vertical = "|   ",
        AtomicConstraint = "* ",
        CompositeConstraint = "+ ",
        AtomicSection = "=",
        CompositeSection = "#"
    };

    /// <summary>
    /// Unicode symbols for enhanced display.
    /// </summary>
    public static EnhancedSymbolSet Unicode => new()
    {
        Branch = "├─ ",
        Vertical = "│  ",
        AtomicConstraint = "⚛️ ",
        CompositeConstraint = "🧩 ",
        AtomicSection = "📋",
        CompositeSection = "🏗️ "
    };

    /// <summary>
    /// Box drawing symbols for professional appearance.
    /// </summary>
    public static EnhancedSymbolSet BoxDrawing => new()
    {
        Branch = "├── ",
        Vertical = "│   ",
        AtomicConstraint = "○ ",
        CompositeConstraint = "◆ ",
        AtomicSection = "▶",
        CompositeSection = "▶"
    };
}