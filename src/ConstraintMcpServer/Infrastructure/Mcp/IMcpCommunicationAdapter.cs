namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Interface for MCP communication protocol handling over stdio streams.
/// </summary>
internal interface IMcpCommunicationAdapter
{
    Task<string?> ReadRequestAsync(StreamReader reader);
    Task WriteResponseAsync(StreamWriter writer, object response);
}