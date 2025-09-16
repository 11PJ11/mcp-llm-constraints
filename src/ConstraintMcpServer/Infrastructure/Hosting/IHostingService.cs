namespace ConstraintMcpServer.Infrastructure.Hosting;

/// <summary>
/// Defines the contract for hosting MCP server operations with console abstraction.
/// </summary>
public interface IHostingService
{
    /// <summary>
    /// Runs the MCP server with the specified command-line arguments.
    /// </summary>
    /// <param name="args">Command-line arguments for server configuration.</param>
    /// <returns>A task representing the asynchronous server operation.</returns>
    Task<int> RunAsync(string[] args);
}