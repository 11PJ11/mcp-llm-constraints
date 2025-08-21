namespace ConstraintMcpServer.Infrastructure.Communication;

/// <summary>
/// Interface for MCP server that processes JSON-RPC requests over stdio.
/// </summary>
public interface IMcpServer
{
    Task ProcessRequestsAsync(CancellationToken cancellationToken = default);
}