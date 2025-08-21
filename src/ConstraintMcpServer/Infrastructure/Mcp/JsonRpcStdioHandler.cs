using System.Text;
using System.Text.Json;

namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Simple JSON-RPC stdio handler for custom server methods like server.help.
/// Implements minimal JSON-RPC 2.0 support for server introspection.
/// </summary>
public static class JsonRpcStdioHandler
{
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
                // Read Content-Length header
                string? headerLine = await reader.ReadLineAsync();
                if (headerLine == null)
                {
                    break;
                }

                if (!headerLine.StartsWith("Content-Length:"))
                {
                    continue;
                }

                if (!int.TryParse(headerLine.Substring("Content-Length:".Length).Trim(), out int contentLength))
                {
                    continue;
                }

                // Read blank line
                await reader.ReadLineAsync();

                // Read JSON content
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

                string requestJson = new string(buffer, 0, totalRead).Trim();

                // Parse and handle the request
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
            int id = root.TryGetProperty("id", out JsonElement idElement) ? idElement.GetInt32() : 1;

            return method switch
            {
                "server.help" => await HandleServerHelp(id),
                _ => CreateErrorResponse(id, -32601, "Method not found")
            };
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error parsing JSON-RPC request: {ex.Message}");
            return CreateErrorResponse(1, -32700, "Parse error");
        }
    }

    private static async Task<object> HandleServerHelp(int id)
    {
        await Task.CompletedTask; // Make method async

        return new
        {
            jsonrpc = "2.0",
            id,
            result = new
            {
                product = "Constraint Enforcement MCP Server",
                description = "Deterministic system that keeps LLM coding agents aligned during code generation with composable software-craft constraints (TDD, Hexagonal Architecture, SOLID, YAGNI, etc.). Injects constraint reminders at MCP tool boundaries to prevent model drift.",
                commands = new[] { "server.help", "initialize", "shutdown" }
            }
        };
    }

    private static object CreateErrorResponse(int id, int code, string message)
    {
        return new
        {
            jsonrpc = "2.0",
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
}
