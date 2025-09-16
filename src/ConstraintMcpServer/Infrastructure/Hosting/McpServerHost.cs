namespace ConstraintMcpServer.Infrastructure.Hosting;

/// <summary>
/// Hosts the MCP server with abstracted console I/O and argument processing.
/// This is the new hosting layer that will replace static Program.cs methods.
/// </summary>
internal sealed class McpServerHost : IHostingService
{
    private readonly IConsoleAbstraction _console;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerHost"/> class.
    /// </summary>
    /// <param name="console">Console abstraction for I/O operations.</param>
    public McpServerHost(IConsoleAbstraction? console = null)
    {
        _console = console ?? new ConsoleAbstraction();
    }

    /// <inheritdoc />
    public async Task<int> RunAsync(string[] args)
    {
        // EXPAND PHASE: Extract argument parsing gradually

        // Extract help flag detection first (baby step)
        if (args.Length > 0 && IsHelpRequest(args[0]))
        {
            // For now, still delegate to existing functionality
            // Will be fully extracted in subsequent steps
            return await Task.FromResult(0);
        }

        // Continue with other argument processing in next baby steps
        return await Task.FromResult(0);
    }

    private static bool IsHelpRequest(string arg)
    {
        return arg == "--help" || arg == "-h";
    }
}