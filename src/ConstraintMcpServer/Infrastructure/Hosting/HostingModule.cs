using ConstraintMcpServer.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace ConstraintMcpServer.Infrastructure.Hosting;

/// <summary>
/// Service installer for hosting-related dependencies.
/// </summary>
public sealed class HostingModule : IServiceInstaller
{
    /// <summary>
    /// Gets the priority for this service installer.
    /// Lower values indicate higher priority (executed first).
    /// </summary>
    public int Priority => 10;

    /// <summary>
    /// Registers hosting services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IConsoleAbstraction, ConsoleAbstraction>();
        services.AddTransient<IHostingService, McpServerHost>();
    }
}