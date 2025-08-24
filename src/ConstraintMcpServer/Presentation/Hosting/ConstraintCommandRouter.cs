using System.Text.Json;
using ConstraintMcpServer.Infrastructure.Communication;
using ConstraintMcpServer.Infrastructure.Configuration;
using ConstraintMcpServer.Application.Scheduling;
using ConstraintMcpServer.Infrastructure.Logging;

namespace ConstraintMcpServer.Presentation.Hosting;

/// <summary>
/// Routes constraint enforcement commands to appropriate handlers.
/// Single responsibility: Route MCP commands to constraint enforcement handlers.
/// </summary>
internal sealed class ConstraintCommandRouter : IConstraintCommandRouter
{

    private readonly IConstraintResponseBuilder _responseFactory;
    private readonly Dictionary<string, IMcpCommandHandler> _commandHandlers;

    public ConstraintCommandRouter(IConstraintResponseBuilder responseFactory, IClientInfoExtractor clientInfoExtractor, IServerConfiguration serverConfiguration)
    {
        _responseFactory = responseFactory;

        // Create scheduler for constraint injection
        var scheduler = new Scheduler(everyNInteractions: InjectionConfiguration.DefaultCadence);

        // Create structured event logger
        var logger = new StructuredEventLogger();

        _commandHandlers = new Dictionary<string, IMcpCommandHandler>
        {
            ["server.help"] = new McpServerHelpHandler(_responseFactory, serverConfiguration),
            ["initialize"] = new McpInitializeHandler(_responseFactory, clientInfoExtractor, serverConfiguration),
            ["shutdown"] = new McpShutdownHandler(_responseFactory),
            ["tools/call"] = new ToolCallHandler(scheduler, logger)
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
                return _responseFactory.CreateErrorResponse(JsonRpcConstants.Defaults.RequestId, JsonRpcConstants.ErrorCodes.ParseError, "Missing method property");
            }

            string? method = methodElement.GetString();
            int id = root.TryGetProperty("id", out JsonElement idElement) ? idElement.GetInt32() : JsonRpcConstants.Defaults.RequestId;

            return await DispatchMcpMethod(method, id, root);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error parsing JSON-RPC request: {ex.Message}");
            return _responseFactory.CreateErrorResponse(JsonRpcConstants.Defaults.RequestId, JsonRpcConstants.ErrorCodes.ParseError, "Parse error");
        }
    }

    private async Task<object> DispatchMcpMethod(string? method, int id, JsonElement root)
    {
        if (method != null && _commandHandlers.TryGetValue(method, out IMcpCommandHandler? handler))
        {
            return await handler.HandleAsync(id, root);
        }

        return _responseFactory.CreateErrorResponse(id, JsonRpcConstants.ErrorCodes.MethodNotFound, "Method not found");
    }
}
