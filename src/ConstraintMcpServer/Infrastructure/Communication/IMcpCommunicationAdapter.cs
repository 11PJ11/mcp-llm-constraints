namespace ConstraintMcpServer.Infrastructure.Communication;

/// <summary>
/// Interface for MCP communication protocol handling over stdio streams.
/// </summary>
internal interface IMcpCommunicationAdapter
{
    Task<string?> ReadRequestAsync(StreamReader reader);
    Task<string?> ReadRequestAsync(StreamReader reader, TimeSpan timeout);
    Task WriteResponseAsync(StreamWriter writer, object response);
}
