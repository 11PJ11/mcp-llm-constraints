using System.Text;
using System.Text.Json;

namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Simple JSON-RPC stdio handler for custom server methods like server.help.
/// Implements minimal JSON-RPC 2.0 support for server introspection.
/// </summary>
public static class JsonRpcStdioHandler
{
    internal const string ProtocolVersion = "2024-11-05";
    internal const string ServerVersion = "0.1.0";
    internal const string ServerName = "Constraint Enforcement MCP Server";
    internal const string JsonRpcVersion = "2.0";
    

    private static readonly IJsonRpcProtocolHandler ProtocolHandler = new JsonRpcProtocolHandler();
    private static readonly IJsonRpcResponseFactory ResponseFactory = new JsonRpcResponseFactory();
    private static readonly IClientInfoExtractor ClientInfoExtractor = new ClientInfoExtractor();
    private static readonly IRequestDispatcher RequestDispatcher = new McpRequestDispatcher(ResponseFactory, ClientInfoExtractor);
    /// <summary>
    /// Processes incoming JSON-RPC requests from stdin and writes responses to stdout.
    /// Handles the custom server.help method for server discoverability.
    /// </summary>
    public static async Task ProcessRequestsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using Stream stdin = Console.OpenStandardInput();
            using Stream stdout = Console.OpenStandardOutput();
            using var reader = new StreamReader(stdin);
            using var writer = new StreamWriter(stdout);

            while (!cancellationToken.IsCancellationRequested)
            {
                string? requestJson = await ProtocolHandler.ReadRequestAsync(reader);
                if (requestJson == null)
                {
                    continue;
                }

                object response = await RequestDispatcher.DispatchAsync(requestJson);
                await ProtocolHandler.WriteResponseAsync(writer, response);
            }
        }
        catch (Exception ex)
        {
            // Log to stderr to avoid interfering with stdout JSON-RPC communication
            await Console.Error.WriteLineAsync($"JsonRpc error: {ex.Message}");
        }
    }
}
