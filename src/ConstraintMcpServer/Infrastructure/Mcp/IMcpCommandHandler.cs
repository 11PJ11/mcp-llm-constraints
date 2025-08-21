using System.Text.Json;

namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Base interface for MCP command handlers.
/// </summary>
internal interface IMcpCommandHandler
{
    Task<object> HandleAsync(int id, JsonElement requestRoot);
}