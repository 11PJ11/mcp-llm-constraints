using Microsoft.Extensions.DependencyInjection;

namespace ConstraintMcpServer.Infrastructure.DependencyInjection;

/// <summary>
/// Interface for service registration modules that configure dependency injection.
/// </summary>
public interface IServiceInstaller
{
    /// <summary>
    /// Gets the priority for this service installer.
    /// Lower values indicate higher priority (executed first).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Registers services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    void RegisterServices(IServiceCollection services);
}
