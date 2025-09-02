using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Represents a command to edit a constraint field.
/// </summary>
public sealed record EditCommand
{
    public ConstraintId ConstraintId { get; init; } = null!;
    public ConstraintField Field { get; init; }
    public string NewValue { get; init; } = string.Empty;
}

/// <summary>
/// Enumeration of constraint fields that can be edited.
/// </summary>
public enum ConstraintField
{
    Title,
    Priority
}
