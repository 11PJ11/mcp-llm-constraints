using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain.Common;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Renders constraint hierarchies as ASCII tree visualizations.
/// Provides hierarchical structure display with metadata and performance optimization.
/// Follows CUPID properties: Composable, Predictable, Domain-based.
/// </summary>
public sealed class ConstraintTreeRenderer
{
    /// <summary>
    /// Renders a constraint library as an ASCII tree showing hierarchical relationships.
    /// </summary>
    /// <param name="library">The constraint library to visualize</param>
    /// <param name="options">Rendering options for format and display preferences</param>
    /// <returns>Result containing the rendered tree or error information</returns>
    public async Task<Result<ConstraintTreeVisualization, DomainError>> RenderTreeAsync(
        ConstraintLibrary library,
        TreeVisualizationOptions options)
    {
        await Task.CompletedTask;

        if (library == null)
        {
            return Result<ConstraintTreeVisualization, DomainError>.Failure(
                ValidationError.ForField(nameof(library), "Constraint library cannot be null"));
        }

        var startTime = DateTime.UtcNow;

        try
        {
            var treeBuilder = new StringBuilder();

            // Add header with library information
            if (!string.IsNullOrEmpty(library.Description))
            {
                treeBuilder.AppendLine($"Constraint Library: {library.Description}");
            }
            if (!string.IsNullOrEmpty(library.Version))
            {
                treeBuilder.AppendLine($"Version: {library.Version}");
            }
            treeBuilder.AppendLine();

            // Render atomic constraints
            if (library.AtomicConstraints.Count > 0)
            {
                treeBuilder.AppendLine("Atomic Constraints:");
                foreach (var constraint in library.AtomicConstraints)
                {
                    RenderAtomicConstraint(treeBuilder, constraint, options);
                }
                treeBuilder.AppendLine();
            }

            // Render composite constraints (hierarchical)
            if (library.CompositeConstraints.Count > 0)
            {
                treeBuilder.AppendLine("Composite Constraints:");
                foreach (var constraint in library.CompositeConstraints)
                {
                    RenderCompositeConstraint(treeBuilder, constraint, options, 0);
                }
            }

            var renderTime = DateTime.UtcNow - startTime;
            var visualization = new ConstraintTreeVisualization(
                treeBuilder.ToString(),
                renderTime,
                library.AtomicConstraints.Count + library.CompositeConstraints.Count);

            return Result<ConstraintTreeVisualization, DomainError>.Success(visualization);
        }
        catch (Exception ex)
        {
            return Result<ConstraintTreeVisualization, DomainError>.Failure(
                new ValidationError("TREE_RENDERING_ERROR", $"Failed to render constraint tree: {ex.Message}"));
        }
    }

    private void RenderAtomicConstraint(StringBuilder builder, AtomicConstraint constraint, TreeVisualizationOptions options)
    {
        RenderConstraintHeader(builder, constraint.Id.ToString(), constraint.Priority, "", null, options.ShowMetadata);

        if (options.ShowMetadata)
        {
            RenderConstraintMetadata(builder, constraint.Title, "", constraint.Triggers?.Keywords, null);
        }
    }

    private void RenderCompositeConstraint(StringBuilder builder, CompositeConstraint constraint, TreeVisualizationOptions options, int depth)
    {
        var indent = CalculateIndent(depth);

        RenderConstraintHeader(builder, constraint.Id.ToString(), constraint.Priority, indent, "Composite", options.ShowMetadata);

        if (options.ShowMetadata)
        {
            RenderConstraintMetadata(builder, constraint.Title, indent, null, constraint.CompositionType);
        }
    }

    private const int IndentSpacesPerLevel = 2;
    private const string PriorityFormat = "F2";

    private static string CalculateIndent(int depth)
    {
        return new string(' ', depth * IndentSpacesPerLevel);
    }

    private static void RenderConstraintHeader(StringBuilder builder, string constraintId, double priority, string indent, string? constraintType = null, bool showMetadata = true)
    {
        builder.Append($"{indent}{TreeRenderingConstants.Branch}");
        builder.Append(constraintId);

        if (showMetadata)
        {
            if (constraintType != null)
            {
                builder.Append($" (Priority: {priority.ToString(PriorityFormat)}, {constraintType})");
            }
            else
            {
                builder.Append($" (Priority: {priority.ToString(PriorityFormat)})");
            }
        }

        builder.AppendLine();
    }

    private static void RenderConstraintMetadata(StringBuilder builder, string title, string indent, IReadOnlyList<string>? keywords, CompositionType? compositionType)
    {
        builder.AppendLine($"{indent}{TreeRenderingConstants.Vertical}Title: {title}");

        if (keywords?.Count > 0)
        {
            builder.AppendLine($"{indent}{TreeRenderingConstants.Vertical}Keywords: {string.Join(", ", keywords)}");
        }

        if (compositionType.HasValue && compositionType.Value != CompositionType.Sequential)
        {
            builder.AppendLine($"{indent}{TreeRenderingConstants.Vertical}Composition: {compositionType}");
        }
    }
}
