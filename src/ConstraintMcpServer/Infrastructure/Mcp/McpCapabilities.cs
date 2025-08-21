using System.Text.Json.Serialization;

namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Represents MCP server capabilities in initialization response.
/// </summary>
internal sealed record McpCapabilities
{
    [JsonPropertyName("tools")]
    public object Tools { get; init; } = new { };
    
    [JsonPropertyName("resources")]
    public object Resources { get; init; } = new { };
    
    [JsonPropertyName("notifications")]
    public object Notifications { get; init; } = new { };

    /// <summary>
    /// Creates default MCP capabilities for the Constraint Enforcement Server.
    /// </summary>
    public static McpCapabilities CreateDefault()
    {
        return new McpCapabilities
        {
            Tools = new { },
            Resources = new { },
            Notifications = new { constraints = true }
        };
    }
}