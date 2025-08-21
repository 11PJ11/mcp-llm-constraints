using System.Text.Json;

namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Command handler for MCP initialize method.
/// </summary>
internal sealed class McpInitializeHandler : IMcpCommandHandler
{
    private readonly IJsonRpcResponseFactory _responseFactory;
    private readonly IClientInfoExtractor _clientInfoExtractor;
    private readonly IServerConfiguration _serverConfiguration;

    public McpInitializeHandler(IJsonRpcResponseFactory responseFactory, IClientInfoExtractor clientInfoExtractor, IServerConfiguration serverConfiguration)
    {
        _responseFactory = responseFactory;
        _clientInfoExtractor = clientInfoExtractor;
        _serverConfiguration = serverConfiguration;
    }

    public async Task<object> HandleAsync(int id, JsonElement requestRoot)
    {
        await Task.CompletedTask;

        // Extract client info if provided (optional for MCP compatibility)
        var clientInfo = _clientInfoExtractor.ExtractClientInfo(requestRoot);

        // Log the initialization for debugging
        await Console.Error.WriteLineAsync($"MCP Initialize from client: {clientInfo.Name} v{clientInfo.Version}");

        var capabilities = McpCapabilities.CreateDefault();
        var serverInfo = new McpServerInfo(_serverConfiguration.ServerName, _serverConfiguration.ServerVersion);
        var result = new McpInitializeResult(_serverConfiguration.ProtocolVersion, capabilities, serverInfo);

        return _responseFactory.CreateSuccessResponse(id, result);
    }
}