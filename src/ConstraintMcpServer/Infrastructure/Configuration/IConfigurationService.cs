using Microsoft.Extensions.Configuration;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Interface for configuration access with multi-source loading.
/// Provides priority-based configuration from command-line, environment, and file sources.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets a configuration value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value or default if not found.</returns>
    T? GetValue<T>(string key);

    /// <summary>
    /// Gets a configuration section.
    /// </summary>
    /// <param name="key">The section key.</param>
    /// <returns>The configuration section.</returns>
    IConfigurationSection GetSection(string key);

    /// <summary>
    /// Validates the current configuration.
    /// </summary>
    /// <returns>True if configuration is valid, false otherwise.</returns>
    bool Validate();
}
