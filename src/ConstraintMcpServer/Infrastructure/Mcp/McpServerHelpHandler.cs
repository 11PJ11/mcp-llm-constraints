using System.Text.Json;

namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Command handler for MCP server.help method.
/// </summary>
internal sealed class McpServerHelpHandler : IMcpCommandHandler
{
    private readonly IJsonRpcResponseFactory _responseFactory;
    private readonly IServerConfiguration _serverConfiguration;

    public McpServerHelpHandler(IJsonRpcResponseFactory responseFactory, IServerConfiguration serverConfiguration)
    {
        _responseFactory = responseFactory;
        _serverConfiguration = serverConfiguration;
    }

    public async Task<object> HandleAsync(int id, JsonElement requestRoot)
    {
        await Task.CompletedTask;

        var result = new
        {
            product = _serverConfiguration.ServerName,
            description = "Deterministic system that keeps LLM coding agents aligned during code generation with composable software-craft constraints (TDD, Hexagonal Architecture, SOLID, YAGNI, etc.). Injects constraint reminders at MCP tool boundaries to prevent model drift.",
            commands = new[] { "server.help", "initialize", "shutdown" }
        };

        return _responseFactory.CreateSuccessResponse(id, result);
    }
}