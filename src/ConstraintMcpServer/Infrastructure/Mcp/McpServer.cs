namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// MCP server implementation that processes JSON-RPC requests over stdio.
/// Depends on abstractions only, following Dependency Inversion Principle.
/// </summary>
internal sealed class McpServer : IMcpServer
{
    private readonly IMcpCommunicationAdapter _protocolHandler;
    private readonly IConstraintCommandRouter _requestDispatcher;

    public McpServer(IMcpCommunicationAdapter protocolHandler, IConstraintCommandRouter requestDispatcher)
    {
        _protocolHandler = protocolHandler;
        _requestDispatcher = requestDispatcher;
    }

    public async Task ProcessRequestsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using Stream stdin = Console.OpenStandardInput();
            using Stream stdout = Console.OpenStandardOutput();
            using var reader = new StreamReader(stdin);
            using var writer = new StreamWriter(stdout);

            while (!cancellationToken.IsCancellationRequested)
            {
                string? requestJson = await _protocolHandler.ReadRequestAsync(reader);
                if (requestJson == null)
                {
                    continue;
                }

                object response = await _requestDispatcher.DispatchAsync(requestJson);
                await _protocolHandler.WriteResponseAsync(writer, response);
            }
        }
        catch (Exception ex)
        {
            // Log to stderr to avoid interfering with stdout JSON-RPC communication
            await Console.Error.WriteLineAsync($"MCP server error: {ex.Message}");
        }
    }
}