using System.Text.Json;

namespace ConstraintMcpServer.Presentation.Hosting;

/// <summary>
/// Base interface for MCP command handlers.
/// </summary>
internal interface IMcpCommandHandler
{
    Task<object> HandleAsync(int id, JsonElement requestRoot);
}