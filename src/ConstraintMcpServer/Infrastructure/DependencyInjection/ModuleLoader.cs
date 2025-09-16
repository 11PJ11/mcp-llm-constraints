using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ConstraintMcpServer.Infrastructure.DependencyInjection;

/// <summary>
/// Centralized service registration module loader for dependency injection configuration.
/// </summary>
public static class ModuleLoader
{
    /// <summary>
    /// Configures all services for the application using reflection-based module discovery.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return services.RegisterModulesFromAssembly(assembly);
    }

    /// <summary>
    /// Builds a service provider from the configured service collection.
    /// </summary>
    /// <param name="services">The configured service collection.</param>
    /// <returns>A configured service provider.</returns>
    public static IServiceProvider BuildServiceProvider(IServiceCollection services)
    {
        return services.BuildServiceProvider();
    }
}