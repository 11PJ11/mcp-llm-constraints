using ConstraintMcpServer.Infrastructure.DependencyInjection;
using ConstraintMcpServer.Infrastructure.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Infrastructure.Hosting;

[TestFixture]
internal sealed class McpServerHostDependencyInjectionTests
{
    [Test]
    public void McpServerHost_Should_WorkWithServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        ModuleLoader.ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var hostingService = serviceProvider.GetRequiredService<IHostingService>();

        // Assert
        Assert.That(hostingService, Is.Not.Null);
        Assert.That(hostingService, Is.InstanceOf<McpServerHost>());
    }

    [Test]
    public void McpServerHost_Should_MaintainBackwardCompatibility()
    {
        // Arrange & Act - Should not throw
        var hostLegacy = new McpServerHost();
        var hostWithConsole = new McpServerHost(new ConsoleAbstraction());

        // Assert
        Assert.That(hostLegacy, Is.Not.Null);
        Assert.That(hostWithConsole, Is.Not.Null);
    }

    [Test]
    public async Task McpServerHost_Should_HandleHelpCommand_WithDI()
    {
        // Arrange
        var services = new ServiceCollection();
        ModuleLoader.ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();
        var hostingService = serviceProvider.GetRequiredService<IHostingService>();

        // Act
        var result = await hostingService.RunAsync(new[] { "--help" });

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }
}