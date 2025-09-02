namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Immutable options for interactive editing sessions.
/// </summary>
public sealed record EditingSessionOptions
{
    public bool EnableUndo { get; init; } = true;
    public int MaxHistorySize { get; init; } = 100;
    
    public static EditingSessionOptions Default => new();
}