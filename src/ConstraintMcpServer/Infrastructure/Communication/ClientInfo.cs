namespace ConstraintMcpServer.Infrastructure.Communication;

/// <summary>
/// Represents client information from MCP initialization request.
/// </summary>
internal sealed record ClientInfo(string Name, string Version)
{
    public static readonly ClientInfo Unknown = new("unknown", "unknown");
}
