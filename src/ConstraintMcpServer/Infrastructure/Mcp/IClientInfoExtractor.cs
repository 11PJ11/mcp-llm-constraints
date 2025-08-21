using System.Text.Json;

namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Interface for extracting client information from MCP requests.
/// </summary>
internal interface IClientInfoExtractor
{
    ClientInfo ExtractClientInfo(JsonElement root);
}