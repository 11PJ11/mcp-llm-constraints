namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Represents a void value for functional programming patterns.
/// Used in Result types when no meaningful value is returned.
/// Implements CUPID principles with predictable and idiomatic design.
/// </summary>
public readonly record struct Unit
{
    /// <summary>
    /// Gets the singleton unit value.
    /// </summary>
    public static Unit Value { get; } = new();

    /// <summary>
    /// Returns the string representation of Unit.
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString() => "()";
}