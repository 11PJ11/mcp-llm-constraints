using System.Text.Json.Serialization;

namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Represents MCP server information in initialization response.
/// </summary>
internal sealed record McpServerInfo(
    [property: JsonPropertyName("name")] string Name, 
    [property: JsonPropertyName("version")] string Version);