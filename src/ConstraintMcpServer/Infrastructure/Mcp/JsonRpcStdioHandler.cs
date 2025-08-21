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
    
    private const int JsonRpcMethodNotFoundError = -32601;
    private const int JsonRpcParseError = -32700;
    private const int DefaultRequestId = 1;

    private static readonly IJsonRpcResponseFactory ResponseFactory = new JsonRpcResponseFactory();
    
    private static readonly Dictionary<string, IMcpCommandHandler> CommandHandlers = new()
    {
        ["server.help"] = new McpServerHelpHandler(ResponseFactory),
        ["initialize"] = new McpInitializeHandler(ResponseFactory),
        ["shutdown"] = new McpShutdownHandler(ResponseFactory)
    };
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
                int? contentLength = await ReadContentLengthHeader(reader);
                if (contentLength == null)
                {
                    continue;
                }

                await reader.ReadLineAsync();

                string requestJson = await ReadJsonContent(reader, contentLength.Value);

                object? response = await HandleJsonRpcRequest(requestJson);
                if (response != null)
                {
                    await WriteJsonRpcResponse(writer, response);
                }
            }
        }
        catch (Exception ex)
        {
            // Log to stderr to avoid interfering with stdout JSON-RPC communication
            await Console.Error.WriteLineAsync($"JsonRpc error: {ex.Message}");
        }
    }

    private static async Task<object?> HandleJsonRpcRequest(string requestJson)
    {
        try
        {
            using var document = JsonDocument.Parse(requestJson);
            JsonElement root = document.RootElement;

            if (!root.TryGetProperty("method", out JsonElement methodElement))
            {
                return null;
            }

            string? method = methodElement.GetString();
            int id = root.TryGetProperty("id", out JsonElement idElement) ? idElement.GetInt32() : DefaultRequestId;

            return await DispatchMcpMethod(method, id, root);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error parsing JSON-RPC request: {ex.Message}");
            return ResponseFactory.CreateErrorResponse(DefaultRequestId, JsonRpcParseError, "Parse error");
        }
    }

    private static async Task<object> DispatchMcpMethod(string? method, int id, JsonElement root)
    {
        if (method != null && CommandHandlers.TryGetValue(method, out var handler))
        {
            return await handler.HandleAsync(id, root);
        }

        return ResponseFactory.CreateErrorResponse(id, JsonRpcMethodNotFoundError, "Method not found");
    }

    private static async Task WriteJsonRpcResponse(StreamWriter writer, object response)
    {
        string json = JsonSerializer.Serialize(response);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        await writer.WriteLineAsync($"Content-Length: {bytes.Length}");
        await writer.WriteLineAsync();
        await writer.WriteAsync(json);
        await writer.FlushAsync();
    }

    private static async Task<int?> ReadContentLengthHeader(StreamReader reader)
    {
        string? headerLine = await reader.ReadLineAsync();
        if (headerLine == null)
        {
            return null;
        }

        if (!headerLine.StartsWith("Content-Length:"))
        {
            return null;
        }

        if (!int.TryParse(headerLine.Substring("Content-Length:".Length).Trim(), out int contentLength))
        {
            return null;
        }

        return contentLength;
    }

    private static async Task<string> ReadJsonContent(StreamReader reader, int contentLength)
    {
        char[] buffer = new char[contentLength];
        int totalRead = 0;
        while (totalRead < contentLength)
        {
            int read = await reader.ReadAsync(buffer, totalRead, contentLength - totalRead);
            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        return new string(buffer, 0, totalRead).Trim();
    }

    internal static ClientInfo ExtractClientInfo(JsonElement root)
    {
        if (!root.TryGetProperty("params", out JsonElement @params) ||
            !@params.TryGetProperty("clientInfo", out JsonElement clientInfoElement))
        {
            return ClientInfo.Unknown;
        }

        string clientName = clientInfoElement.TryGetProperty("name", out JsonElement nameElement)
            ? nameElement.GetString() ?? "unknown"
            : "unknown";

        string clientVersion = clientInfoElement.TryGetProperty("version", out JsonElement versionElement)
            ? versionElement.GetString() ?? "unknown"
            : "unknown";

        return new ClientInfo(clientName, clientVersion);
    }
}
