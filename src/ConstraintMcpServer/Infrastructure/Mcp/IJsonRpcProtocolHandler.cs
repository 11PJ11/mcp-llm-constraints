namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Interface for handling JSON-RPC protocol over stdio streams.
/// </summary>
internal interface IJsonRpcProtocolHandler
{
    Task<string?> ReadRequestAsync(StreamReader reader);
    Task WriteResponseAsync(StreamWriter writer, object response);
}