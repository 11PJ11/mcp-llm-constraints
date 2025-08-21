namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Interface for dispatching JSON-RPC requests to appropriate handlers.
/// </summary>
internal interface IRequestDispatcher
{
    Task<object> DispatchAsync(string requestJson);
}