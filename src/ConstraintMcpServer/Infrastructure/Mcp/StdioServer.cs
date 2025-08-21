using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// MCP server implementation providing constraint enforcement tools over stdio JSON-RPC.
/// Follows hexagonal architecture pattern as an infrastructure adapter.
/// </summary>
[McpServerToolType]
public static class StdioServer
{
    /// <summary>
    /// Provides help information about the Constraint Enforcement MCP Server.
    /// Returns product information, description, and available commands.
    /// </summary>
    /// <returns>Help information including product name, description, and commands.</returns>
    [McpServerTool]
    [Description("Provides help information about the Constraint Enforcement MCP Server")]
    public static ServerHelpResponse Help() => new()
    {
        Product = "Constraint Enforcement MCP Server",
        Description = "Deterministic system that keeps LLM coding agents aligned during code generation with composable software-craft constraints (TDD, Hexagonal Architecture, SOLID, YAGNI, etc.). Injects constraint reminders at MCP tool boundaries to prevent model drift.",
        Commands = new[] { "server.help", "initialize", "shutdown" }
    };
}

/// <summary>
/// Response structure for server help information.
/// Follows MCP protocol requirements for server discoverability.
/// </summary>
public record ServerHelpResponse
{
    /// <summary>
    /// Product name for identification.
    /// </summary>
    public required string Product { get; init; }

    /// <summary>
    /// Detailed description of server capabilities and purpose.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Available commands that the server supports.
    /// </summary>
    public required string[] Commands { get; init; }
}
