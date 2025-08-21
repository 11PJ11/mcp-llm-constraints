using System.Text;
using System.Text.Json;

namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Represents client information from MCP initialization request.
/// </summary>
public record ClientInfo(string Name, string Version)
{
    public static readonly ClientInfo Unknown = new("unknown", "unknown");
}

/// <summary>
/// Simple JSON-RPC stdio handler for custom server methods like server.help.
/// Implements minimal JSON-RPC 2.0 support for server introspection.
/// </summary>
public static class JsonRpcStdioHandler
{
    private const string ProtocolVersion = "2024-11-05";
    private const string ServerVersion = "0.1.0";
    private const string ServerName = "Constraint Enforcement MCP Server";
    private const string JsonRpcVersion = "2.0";
    
    private const int JsonRpcMethodNotFoundError = -32601;
    private const int JsonRpcParseError = -32700;
    private const int DefaultRequestId = 1;
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
            return CreateErrorResponse(DefaultRequestId, JsonRpcParseError, "Parse error");
        }
    }

    private static async Task<object> DispatchMcpMethod(string? method, int id, JsonElement root)
    {
        return method switch
        {
            "server.help" => await HandleServerHelp(id),
            "initialize" => await HandleInitialize(id, root),
            "shutdown" => await HandleShutdown(id),
            _ => CreateErrorResponse(id, JsonRpcMethodNotFoundError, "Method not found")
        };
    }

    private static async Task<object> HandleServerHelp(int id)
    {
        await Task.CompletedTask;

        var result = new
        {
            product = ServerName,
            description = "Deterministic system that keeps LLM coding agents aligned during code generation with composable software-craft constraints (TDD, Hexagonal Architecture, SOLID, YAGNI, etc.). Injects constraint reminders at MCP tool boundaries to prevent model drift.",
            commands = new[] { "server.help", "initialize", "shutdown" }
        };

        return CreateSuccessResponse(id, result);
    }

    private static async Task<object> HandleInitialize(int id, JsonElement root)
    {
        await Task.CompletedTask;

        // Extract client info if provided (optional for MCP compatibility)
        var clientInfo = ExtractClientInfo(root);

        // Log the initialization for debugging
        await Console.Error.WriteLineAsync($"MCP Initialize from client: {clientInfo.Name} v{clientInfo.Version}");

        var result = new
        {
            protocolVersion = ProtocolVersion,
            capabilities = new
            {
                tools = new { },
                resources = new { },
                notifications = new
                {
                    constraints = true
                }
            },
            serverInfo = new
            {
                name = ServerName,
                version = ServerVersion
            }
        };

        return CreateSuccessResponse(id, result);
    }

    private static async Task<object> HandleShutdown(int id)
    {
        await Task.CompletedTask;

        // Log shutdown for debugging
        await Console.Error.WriteLineAsync("MCP Shutdown requested");

        return CreateSuccessResponse(id, new { });
    }

    private static object CreateErrorResponse(int id, int code, string message)
    {
        return new
        {
            jsonrpc = JsonRpcVersion,
            id,
            error = new
            {
                code,
                message
            }
        };
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

    private static object CreateSuccessResponse(int id, object result)
    {
        return new
        {
            jsonrpc = JsonRpcVersion,
            id,
            result
        };
    }

    private static ClientInfo ExtractClientInfo(JsonElement root)
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
