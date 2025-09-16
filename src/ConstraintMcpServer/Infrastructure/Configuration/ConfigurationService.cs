using Microsoft.Extensions.Configuration;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Implementation of configuration service with multi-source loading.
/// Priority order: command-line → environment → file → defaults.
/// </summary>
public sealed class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc />
    public T? GetValue<T>(string key)
    {
        return _configuration.GetValue<T>(key);
    }

    /// <inheritdoc />
    public IConfigurationSection GetSection(string key)
    {
        return _configuration.GetSection(key);
    }

    /// <inheritdoc />
    public bool Validate()
    {
        // Basic validation - for now just return true
        // TODO: Implement actual configuration validation logic
        return true;
    }
}
