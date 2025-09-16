using ConstraintMcpServer.Infrastructure.Configuration;

namespace ConstraintMcpServer.Infrastructure.Hosting;

/// <summary>
/// Hosts the MCP server with abstracted console I/O and argument processing.
/// </summary>
public sealed class McpServerHost : IHostingService
{
    private const string ServerName = "Constraint MCP Server";
    private const string ServerVersion = "0.1.0";
    private const string ProductDescription = "Constraint Enforcement MCP Server - keeps LLM coding agents aligned during code generation with composable software-craft constraints";

    private readonly IConsoleAbstraction _console;
    private readonly IConfigurationService? _configurationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerHost"/> class.
    /// </summary>
    /// <param name="console">Console abstraction for I/O operations.</param>
    public McpServerHost(IConsoleAbstraction? console = null)
    {
        _console = console ?? new ConsoleAbstraction();
        _configurationService = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerHost"/> class with configuration service.
    /// </summary>
    /// <param name="console">Console abstraction for I/O operations.</param>
    /// <param name="configurationService">Configuration service for enhanced argument processing.</param>
    public McpServerHost(IConsoleAbstraction? console, IConfigurationService? configurationService)
    {
        _console = console ?? new ConsoleAbstraction();
        _configurationService = configurationService;
    }

    /// <inheritdoc />
    public async Task<int> RunAsync(string[] args)
    {
        if (args.Length > 0 && IsHelpRequest(args[0]))
        {
            await DisplayHelpMessage();
            return 0;
        }

        if (args.Length > 0 && IsVersionRequest(args[0]))
        {
            await DisplayVersionMessage();
            return 0;
        }

        return 0;
    }

    private static bool IsHelpRequest(string arg) => arg == "--help" || arg == "-h";

    private static bool IsVersionRequest(string arg) => arg == "--version" || arg == "-v";

    private Task DisplayHelpMessage()
    {
        _console.WriteLine($"{ServerName} - {ProductDescription}");
        _console.WriteLine("Usage: dotnet run [--help|--version]");
        _console.WriteLine("  --help, -h       Show this help message");
        _console.WriteLine("  --version, -v    Show version information");
        _console.WriteLine("");
        _console.WriteLine("This is a walking skeleton implementation. Full MCP protocol support");
        _console.WriteLine("will be added through test-driven development.");

        return Task.CompletedTask;
    }

    private Task DisplayVersionMessage()
    {
        _console.WriteLine($"{ServerName} {ServerVersion}");
        _console.WriteLine(ProductDescription);

        return Task.CompletedTask;
    }
}
