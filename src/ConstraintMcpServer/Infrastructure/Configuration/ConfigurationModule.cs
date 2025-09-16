using ConstraintMcpServer.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Service installer for configuration-related dependencies.
/// </summary>
public sealed class ConfigurationModule : IServiceInstaller
{
    /// <summary>
    /// Gets the priority for this service installer.
    /// Lower values indicate higher priority (executed first).
    /// </summary>
    public int Priority => 20;

    /// <summary>
    /// Registers configuration services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    public void RegisterServices(IServiceCollection services)
    {
        // Register IConfiguration if not already registered
        if (!services.Any(s => s.ServiceType == typeof(IConfiguration)))
        {
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        }

        services.AddSingleton<IConfigurationService, ConfigurationService>();
    }
}
