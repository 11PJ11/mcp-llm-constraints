using System.Threading.Tasks;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Interface for loading constraint pack configurations from various sources.
/// Abstracts the configuration loading process from the application entry point.
/// </summary>
internal interface IConfigurationLoader
{
    /// <summary>
    /// Loads a constraint pack configuration from the specified source.
    /// </summary>
    /// <param name="configurationSource">The source of the configuration (e.g., file path).</param>
    /// <returns>
    /// The loaded constraint pack, or null if no configuration source is provided.
    /// </returns>
    /// <exception cref="ValidationException">Thrown when configuration loading or validation fails.</exception>
    Task<ConstraintPack?> LoadAsync(string? configurationSource);
}
