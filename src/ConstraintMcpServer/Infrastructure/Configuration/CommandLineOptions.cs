namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Represents validated command line options for the constraint MCP server.
/// Provides type-safe access to configuration and help options.
/// </summary>
internal sealed record CommandLineOptions
{
    /// <summary>
    /// Path to the constraint configuration file, if specified.
    /// </summary>
    public string? ConfigurationPath { get; init; }

    /// <summary>
    /// Whether help information was requested.
    /// </summary>
    public bool ShowHelp { get; init; }

    /// <summary>
    /// Creates command line options for help display.
    /// </summary>
    public static CommandLineOptions ShowHelpOptions() => new() { ShowHelp = true };

    /// <summary>
    /// Creates command line options with a configuration path.
    /// </summary>
    public static CommandLineOptions WithConfiguration(string configurationPath) => new()
    {
        ConfigurationPath = configurationPath
    };

    /// <summary>
    /// Creates default command line options with no configuration.
    /// </summary>
    public static CommandLineOptions Default() => new();
}
