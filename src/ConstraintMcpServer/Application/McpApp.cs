using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Serilog;
using ConstraintMcpServer.Infrastructure.Logging;

namespace ConstraintMcpServer.Application;

/// <summary>
/// Application layer orchestrator for the Constraint Enforcement MCP Server.
/// Coordinates between infrastructure adapters and domain logic.
/// </summary>
public static class McpApp
{
    /// <summary>
    /// Configures and starts the MCP server with stdio transport.
    /// Follows hexagonal architecture by keeping infrastructure concerns separate.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Configured host ready to run</returns>
    public static IHost CreateMcpServer(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        // Configure Serilog to write to stderr (not stdout) to avoid interfering with JSON-RPC
        builder.Services.AddSerilog(LoggingConfiguration.CreateLogger());

        // Clear default logging providers that write to stdout
        builder.Logging.ClearProviders();

        // Configure MCP server with stdio transport and tool discovery
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        return builder.Build();
    }
}
