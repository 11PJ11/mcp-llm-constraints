using ConstraintMcpServer.Infrastructure.DependencyInjection;
using ConstraintMcpServer.Infrastructure.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Infrastructure.DependencyInjection;

[TestFixture]
internal sealed class FullDependencyInjectionIntegrationTests
{
    [Test]
    public void FullPipeline_Should_ConfigureAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        ModuleLoader.ConfigureServices(services);
        var serviceProvider = ModuleLoader.BuildServiceProvider(services);

        // Assert - Verify all expected services are registered
        Assert.That(serviceProvider.GetService<IConsoleAbstraction>(), Is.Not.Null);
        Assert.That(serviceProvider.GetService<IHostingService>(), Is.Not.Null);

        // Verify concrete types are registered correctly
        var hostingService = serviceProvider.GetRequiredService<IHostingService>();
        Assert.That(hostingService, Is.InstanceOf<McpServerHost>());
    }

    [Test]
    public async Task FullPipeline_Should_ExecuteApplicationCommands()
    {
        // Arrange
        var services = new ServiceCollection();
        ModuleLoader.ConfigureServices(services);
        var serviceProvider = ModuleLoader.BuildServiceProvider(services);

        // Act
        var hostingService = serviceProvider.GetRequiredService<IHostingService>();
        var exitCode = await hostingService.RunAsync(new[] { "--help" });

        // Assert
        Assert.That(exitCode, Is.EqualTo(0));
    }

    [Test]
    public void HostingModule_Should_HaveCorrectPriority()
    {
        // Arrange & Act
        var module = new HostingModule();

        // Assert
        Assert.That(module.Priority, Is.EqualTo(10));
    }
}