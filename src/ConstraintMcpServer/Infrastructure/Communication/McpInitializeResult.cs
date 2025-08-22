using System.Text.Json.Serialization;

namespace ConstraintMcpServer.Infrastructure.Communication;

/// <summary>
/// Represents the result payload for MCP initialize response.
/// </summary>
internal sealed record McpInitializeResult(
    [property: JsonPropertyName("protocolVersion")] string ProtocolVersion,
    [property: JsonPropertyName("capabilities")] McpCapabilities Capabilities,
    [property: JsonPropertyName("serverInfo")] McpServerInfo ServerInfo);
