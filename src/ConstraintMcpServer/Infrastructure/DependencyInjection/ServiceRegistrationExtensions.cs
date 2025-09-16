using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ConstraintMcpServer.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering services from assemblies using reflection-based discovery.
/// </summary>
public static class ServiceRegistrationExtensions
{
    /// <summary>
    /// Registers all service installers from the specified assembly.
    /// Service installers are executed in priority order (lower values first).
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="assembly">The assembly to scan for service installers.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection RegisterModulesFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var installers = GetServiceInstallersFromAssembly(assembly);
        var sortedInstallers = installers.OrderBy(installer => installer.Priority);

        foreach (var installer in sortedInstallers)
        {
            installer.RegisterServices(services);
        }

        return services;
    }

    private static IEnumerable<IServiceInstaller> GetServiceInstallersFromAssembly(Assembly assembly)
    {
        var installerType = typeof(IServiceInstaller);
        var installerTypes = assembly.GetTypes()
            .Where(type => installerType.IsAssignableFrom(type) &&
                          !type.IsInterface &&
                          !type.IsAbstract);

        foreach (var type in installerTypes)
        {
            if (Activator.CreateInstance(type) is IServiceInstaller installer)
            {
                yield return installer;
            }
        }
    }
}
