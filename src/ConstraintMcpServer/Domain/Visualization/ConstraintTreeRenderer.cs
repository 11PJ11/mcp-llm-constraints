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
        builder.Append("+-- ");  // ASCII substitute for ├──
        builder.Append($"{constraint.Id}");

        if (options.ShowMetadata)
        {
            builder.Append($" (Priority: {constraint.Priority:F2})");
        }

        builder.AppendLine();

        if (options.ShowMetadata)
        {
            builder.AppendLine($"|   Title: {constraint.Title}");  // ASCII substitute for │
            if (constraint.Triggers?.Keywords.Count > 0)
            {
                builder.AppendLine($"|   Keywords: {string.Join(", ", constraint.Triggers.Keywords)}");  // ASCII substitute for │
            }
        }
    }

    private void RenderCompositeConstraint(StringBuilder builder, CompositeConstraint constraint, TreeVisualizationOptions options, int depth)
    {
        var indent = new string(' ', depth * 2);

        builder.Append($"{indent}+-- ");  // ASCII substitute for ├──
        builder.Append($"{constraint.Id}");

        if (options.ShowMetadata)
        {
            builder.Append($" (Priority: {constraint.Priority:F2}, Composite)");
        }

        builder.AppendLine();

        if (options.ShowMetadata)
        {
            builder.AppendLine($"{indent}|   Title: {constraint.Title}");  // ASCII substitute for │
            if (constraint.CompositionType != CompositionType.Sequential)
            {
                builder.AppendLine($"{indent}|   Composition: {constraint.CompositionType}");  // ASCII substitute for │
            }
        }
    }
}
