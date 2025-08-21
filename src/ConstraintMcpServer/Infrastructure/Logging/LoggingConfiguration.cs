using Serilog;
using Serilog.Events;

namespace ConstraintMcpServer.Infrastructure.Logging;

/// <summary>
/// Logging configuration for the Constraint Enforcement MCP Server.
/// Ensures logs go to stderr to avoid interfering with JSON-RPC stdio communication.
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configures Serilog to write NDJSON logs to stderr.
    /// This prevents interference with MCP JSON-RPC communication on stdout.
    /// </summary>
    /// <returns>Configured logger</returns>
    public static ILogger CreateLogger()
    {
        try
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("ModelContextProtocol", LogEventLevel.Warning)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    standardErrorFromLevel: LogEventLevel.Verbose)
                .CreateLogger();
        }
        catch
        {
            // Fallback to simple console logger if Serilog configuration fails
            return new LoggerConfiguration()
                .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
                .CreateLogger();
        }
    }
}
