using System;
using System.Text;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain.Common;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Domain.Visualization;

namespace ConstraintMcpServer.Application.Visualization;

/// <summary>
/// Enhanced tree renderer with Unicode symbols, colors, and Claude Code optimization.
/// Builds upon ConstraintTreeRenderer with enhanced visual formatting and symbol support.
/// Follows CUPID properties: Composable, Predictable, Idiomatic, Domain-based.
/// </summary>
public sealed class EnhancedTreeRenderer
{
    private const double HIGH_PRIORITY_THRESHOLD = 0.9;
    private const double MEDIUM_PRIORITY_THRESHOLD = 0.7;
    private const double NORMAL_PRIORITY_THRESHOLD = 0.5;
    private const string HIGH_PRIORITY_INDICATOR = "🔴";
    private const string MEDIUM_PRIORITY_INDICATOR = "🟡";
    private const string NORMAL_PRIORITY_INDICATOR = "🟢";
    private const string LOW_PRIORITY_INDICATOR = "🔵";
    
    private readonly ConstraintTreeRenderer _baseRenderer;

    public EnhancedTreeRenderer(ConstraintTreeRenderer baseRenderer)
    {
        _baseRenderer = baseRenderer ?? throw new ArgumentNullException(nameof(baseRenderer));
    }

    /// <summary>
    /// Renders enhanced constraint tree with colors, symbols, and improved formatting.
    /// </summary>
    /// <param name="library">The constraint library to visualize</param>
    /// <param name="options">Enhanced rendering options with color and symbol support</param>
    /// <returns>Result containing enhanced tree visualization or error information</returns>
    public async Task<Result<EnhancedTreeVisualization, DomainError>> RenderEnhancedTreeAsync(
        ConstraintLibrary library,
        TreeVisualizationOptions options)
    {
        if (library == null)
        {
            return Result<EnhancedTreeVisualization, DomainError>.Failure(
                ValidationError.ForField(nameof(library), "Constraint library cannot be null"));
        }

        var startTime = DateTime.UtcNow;

        try
        {
            // Get base rendering first
            var baseResult = await _baseRenderer.RenderTreeAsync(library, options);
            if (baseResult.IsError)
            {
                return Result<EnhancedTreeVisualization, DomainError>.Failure(baseResult.Error);
            }

            var baseVisualization = baseResult.Value;
            var enhancedContent = ApplyEnhancements(baseVisualization.TreeContent, options, library);
            
            var renderTime = DateTime.UtcNow - startTime;
            var enhancedVisualization = new EnhancedTreeVisualization(
                enhancedContent,
                renderTime,
                baseVisualization.ConstraintCount,
                GetColorProfile(options),
                GetSymbolSet(options),
                options.ClaudeCodeCompatible);

            return Result<EnhancedTreeVisualization, DomainError>.Success(enhancedVisualization);
        }
        catch (Exception ex)
        {
            return Result<EnhancedTreeVisualization, DomainError>.Failure(
                new ValidationError("ENHANCED_TREE_RENDERING_ERROR", $"Failed to render enhanced constraint tree: {ex.Message}"));
        }
    }

    private string ApplyEnhancements(string baseContent, TreeVisualizationOptions options, ConstraintLibrary library)
    {
        var lines = SplitContentIntoLines(baseContent);
        return ProcessLinesWithEnhancements(lines, options, library);
    }

    private static string[] SplitContentIntoLines(string content)
    {
        return content.Split('\n', StringSplitOptions.None);
    }

    private string ProcessLinesWithEnhancements(string[] lines, TreeVisualizationOptions options, ConstraintLibrary library)
    {
        var enhancedBuilder = new StringBuilder();

        foreach (var line in lines)
        {
            var enhancedLine = ApplyAllEnhancementsToLine(line, options, library);
            enhancedBuilder.AppendLine(enhancedLine);
        }

        return enhancedBuilder.ToString().TrimEnd();
    }

    private string ApplyAllEnhancementsToLine(string line, TreeVisualizationOptions options, ConstraintLibrary library)
    {
        var enhancedLine = ApplySymbolEnhancements(line, options);
        enhancedLine = ApplyPriorityEnhancements(enhancedLine, options, library);
        enhancedLine = ApplySectionEnhancements(enhancedLine, options);
        return enhancedLine;
    }

    private string ApplySymbolEnhancements(string line, TreeVisualizationOptions options)
    {
        if (ShouldUseAsciiSymbols(options))
        {
            return line;
        }

        return ReplaceAsciiSymbolsWithUnicode(line, options);
    }

    private static bool ShouldUseAsciiSymbols(TreeVisualizationOptions options)
    {
        return options.CharacterSet == CharacterSet.Ascii;
    }

    private string ReplaceAsciiSymbolsWithUnicode(string line, TreeVisualizationOptions options)
    {
        var symbolSet = GetSymbolSet(options);
        var enhancedLine = line;

        enhancedLine = ReplaceTreeBranches(enhancedLine, symbolSet);
        enhancedLine = ReplaceTreeVerticals(enhancedLine, symbolSet);

        return enhancedLine;
    }

    private static string ReplaceTreeBranches(string line, EnhancedSymbolSet symbolSet)
    {
        return line.Replace("+-- ", symbolSet.Branch);
    }

    private static string ReplaceTreeVerticals(string line, EnhancedSymbolSet symbolSet)
    {
        return line.Replace("|   ", symbolSet.Vertical);
    }

    private string ApplyPriorityEnhancements(string line, TreeVisualizationOptions options, ConstraintLibrary library)
    {
        if (!ShouldShowMetadata(options))
        {
            return line;
        }

        return AddPriorityIndicatorToLine(line);
    }

    private static bool ShouldShowMetadata(TreeVisualizationOptions options)
    {
        return options.ShowMetadata;
    }

    private string AddPriorityIndicatorToLine(string line)
    {
        if (!ContainsPriorityValue(line))
        {
            return line;
        }

        var priorityMatch = System.Text.RegularExpressions.Regex.Match(line, @"Priority: (\d+\.\d+)");
        if (!priorityMatch.Success || !double.TryParse(priorityMatch.Groups[1].Value, out var priority))
        {
            return line;
        }

        return ReplacePriorityWithIndicator(line, priorityMatch.Groups[1].Value, priority);
    }

    private static bool ContainsPriorityValue(string line)
    {
        return line.Contains("Priority:");
    }

    private string ReplacePriorityWithIndicator(string line, string priorityValue, double priority)
    {
        var priorityIndicator = DeterminePriorityIndicator(priority);
        if (string.IsNullOrEmpty(priorityIndicator))
        {
            return line;
        }

        return line.Replace($"Priority: {priorityValue}", $"Priority: {priorityValue} {priorityIndicator}");
    }

    private string ApplySectionEnhancements(string line, TreeVisualizationOptions options)
    {
        if (options.CharacterSet == CharacterSet.Ascii)
        {
            return line; // Keep simple for ASCII
        }

        var symbolSet = GetSymbolSet(options);

        // Add section symbols
        if (line.Contains("Atomic Constraints:"))
        {
            line = line.Replace("Atomic Constraints:", $"{symbolSet.AtomicSection} Atomic Constraints:");
        }
        else if (line.Contains("Composite Constraints:"))
        {
            line = line.Replace("Composite Constraints:", $"{symbolSet.CompositeSection} Composite Constraints:");
        }

        return line;
    }

    private string DeterminePriorityIndicator(double priority)
    {
        return priority switch
        {
            >= HIGH_PRIORITY_THRESHOLD => HIGH_PRIORITY_INDICATOR,
            >= MEDIUM_PRIORITY_THRESHOLD => MEDIUM_PRIORITY_INDICATOR,
            >= NORMAL_PRIORITY_THRESHOLD => NORMAL_PRIORITY_INDICATOR,
            _ => LOW_PRIORITY_INDICATOR
        };
    }

    private EnhancedSymbolSet GetSymbolSet(TreeVisualizationOptions options)
    {
        return options.CharacterSet switch
        {
            CharacterSet.Unicode => EnhancedSymbolSet.Unicode,
            CharacterSet.BoxDrawing => EnhancedSymbolSet.BoxDrawing,
            _ => EnhancedSymbolSet.Ascii
        };
    }

    private ColorProfile GetColorProfile(TreeVisualizationOptions options)
    {
        return options.Colors switch
        {
            ColorScheme.Colored => ColorProfile.Standard,
            ColorScheme.HighContrast => ColorProfile.HighContrast,
            _ => ColorProfile.Monochrome
        };
    }
}