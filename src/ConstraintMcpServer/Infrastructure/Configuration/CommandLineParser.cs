using System;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Parses command line arguments into structured options.
/// Provides validation and error handling for command line parsing.
/// </summary>
internal sealed class CommandLineParser
{
    /// <summary>
    /// Parses command line arguments into structured options.
    /// </summary>
    /// <param name="arguments">The command line arguments to parse.</param>
    /// <returns>Parsed command line options.</returns>
    /// <exception cref="ArgumentException">Thrown when invalid arguments are provided.</exception>
    public CommandLineOptions Parse(string[] arguments)
    {
        if (arguments == null)
        {
            return CommandLineOptions.Default();
        }

        for (int i = 0; i < arguments.Length; i++)
        {
            string arg = arguments[i];

            switch (arg)
            {
                case "--help":
                case "-h":
                    return CommandLineOptions.ShowHelpOptions();

                case "--config":
                    if (i + 1 >= arguments.Length)
                    {
                        throw new ArgumentException("--config requires a file path argument");
                    }
                    string configPath = arguments[i + 1];
                    if (string.IsNullOrWhiteSpace(configPath))
                    {
                        throw new ArgumentException("--config requires a non-empty file path");
                    }
                    return CommandLineOptions.WithConfiguration(configPath);

                default:
                    if (arg.StartsWith("-"))
                    {
                        throw new ArgumentException($"Unknown option: {arg}");
                    }
                    break;
            }
        }

        return CommandLineOptions.Default();
    }

    /// <summary>
    /// Gets help text describing available command line options.
    /// </summary>
    public string GetHelpText()
    {
        return """
        Constraint MCP Server - Keeps LLM coding agents aligned with composable constraints

        Usage:
          ConstraintMcpServer [options]

        Options:
          --config <path>    Specify path to constraint configuration YAML file
          --help, -h         Show this help information

        Examples:
          ConstraintMcpServer --config ./constraints.yaml
          ConstraintMcpServer --help

        The server implements the MCP (Model Context Protocol) to inject constraint
        reminders at tool boundaries, preventing model drift during code generation.
        """;
    }
}
