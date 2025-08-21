using System.Text.Json;

namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Dispatches MCP requests to appropriate command handlers.
/// Single responsibility: Route JSON-RPC methods to MCP command handlers.
/// </summary>
internal sealed class McpRequestDispatcher : IRequestDispatcher
{
    private const int JsonRpcMethodNotFoundError = -32601;
    private const int JsonRpcParseError = -32700;
    private const int DefaultRequestId = 1;

    private readonly IJsonRpcResponseFactory _responseFactory;
    private readonly Dictionary<string, IMcpCommandHandler> _commandHandlers;

    public McpRequestDispatcher(IJsonRpcResponseFactory responseFactory, IClientInfoExtractor clientInfoExtractor, IServerConfiguration serverConfiguration)
    {
        _responseFactory = responseFactory;
        _commandHandlers = new Dictionary<string, IMcpCommandHandler>
        {
            ["server.help"] = new McpServerHelpHandler(_responseFactory, serverConfiguration),
            ["initialize"] = new McpInitializeHandler(_responseFactory, clientInfoExtractor, serverConfiguration),
            ["shutdown"] = new McpShutdownHandler(_responseFactory)
        };
    }

    public async Task<object> DispatchAsync(string requestJson)
    {
        try
        {
            using var document = JsonDocument.Parse(requestJson);
            JsonElement root = document.RootElement;

            if (!root.TryGetProperty("method", out JsonElement methodElement))
            {
                return _responseFactory.CreateErrorResponse(DefaultRequestId, JsonRpcParseError, "Missing method property");
            }

            string? method = methodElement.GetString();
            int id = root.TryGetProperty("id", out JsonElement idElement) ? idElement.GetInt32() : DefaultRequestId;

            return await DispatchMcpMethod(method, id, root);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error parsing JSON-RPC request: {ex.Message}");
            return _responseFactory.CreateErrorResponse(DefaultRequestId, JsonRpcParseError, "Parse error");
        }
    }

    private async Task<object> DispatchMcpMethod(string? method, int id, JsonElement root)
    {
        if (method != null && _commandHandlers.TryGetValue(method, out var handler))
        {
            return await handler.HandleAsync(id, root);
        }

        return _responseFactory.CreateErrorResponse(id, JsonRpcMethodNotFoundError, "Method not found");
    }
}