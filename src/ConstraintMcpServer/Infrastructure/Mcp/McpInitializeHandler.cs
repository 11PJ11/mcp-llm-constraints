using System.Text.Json;

namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Command handler for MCP initialize method.
/// </summary>
internal sealed class McpInitializeHandler : IMcpCommandHandler
{
    private readonly IJsonRpcResponseFactory _responseFactory;
    private readonly IClientInfoExtractor _clientInfoExtractor;

    public McpInitializeHandler(IJsonRpcResponseFactory responseFactory, IClientInfoExtractor clientInfoExtractor)
    {
        _responseFactory = responseFactory;
        _clientInfoExtractor = clientInfoExtractor;
    }

    public async Task<object> HandleAsync(int id, JsonElement requestRoot)
    {
        await Task.CompletedTask;

        // Extract client info if provided (optional for MCP compatibility)
        var clientInfo = _clientInfoExtractor.ExtractClientInfo(requestRoot);

        // Log the initialization for debugging
        await Console.Error.WriteLineAsync($"MCP Initialize from client: {clientInfo.Name} v{clientInfo.Version}");

        var capabilities = McpCapabilities.CreateDefault();
        var serverInfo = new McpServerInfo(JsonRpcStdioHandler.ServerName, JsonRpcStdioHandler.ServerVersion);
        var result = new McpInitializeResult(JsonRpcStdioHandler.ProtocolVersion, capabilities, serverInfo);

        return _responseFactory.CreateSuccessResponse(id, result);
    }
}