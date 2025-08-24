using System.Text;
using System.Text.Json;
using ConstraintMcpServer.Infrastructure.Communication;

namespace ConstraintMcpServer.Presentation.Hosting;

/// <summary>
/// Hosts the constraint enforcement server that keeps LLM coding agents aligned.
/// Provides MCP protocol support for constraint injection and model drift prevention.
/// </summary>
public static class ConstraintServerHost
{
    internal const string ProtocolVersion = "2024-11-05";
    internal const string ServerVersion = "0.1.0";
    internal const string ServerName = "Constraint Enforcement MCP Server";

    private static readonly IMcpServer Server = ConstraintServerFactory.Create();

    /// <summary>
    /// Processes incoming JSON-RPC requests from stdin and writes responses to stdout.
    /// Handles the custom server.help method for server discoverability.
    /// </summary>
    public static async Task ProcessRequestsAsync(CancellationToken cancellationToken = default)
    {
        await Server.ProcessRequestsAsync(cancellationToken);
    }
}
