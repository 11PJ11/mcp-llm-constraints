using System.Text.Json;

namespace ConstraintMcpServer.Presentation.Hosting;

/// <summary>
/// Command handler for MCP shutdown method.
/// </summary>
internal sealed class McpShutdownHandler : IMcpCommandHandler
{
    private readonly IConstraintResponseBuilder _responseFactory;

    public McpShutdownHandler(IConstraintResponseBuilder responseFactory)
    {
        _responseFactory = responseFactory;
    }

    public async Task<object> HandleAsync(int id, JsonElement requestRoot)
    {
        await Task.CompletedTask;

        // Log shutdown for debugging
        await Console.Error.WriteLineAsync("MCP Shutdown requested");

        return _responseFactory.CreateSuccessResponse(id, new { });
    }
}