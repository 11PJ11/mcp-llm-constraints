using ConstraintMcpServer.Infrastructure.DependencyInjection;
using ConstraintMcpServer.Infrastructure.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Infrastructure.DependencyInjection;

[TestFixture]
internal sealed class ModuleLoaderBasicTests
{
    [Test]
    public void ConfigureServices_Should_RegisterHostingServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = ModuleLoader.ConfigureServices(services);

        // Assert
        Assert.That(result, Is.SameAs(services));
        Assert.That(services.Count, Is.GreaterThan(0));

        // Verify that some expected services are registered
        var provider = services.BuildServiceProvider();
        var consoleAbstraction = provider.GetService<IConsoleAbstraction>();
        var hostingService = provider.GetService<IHostingService>();

        Assert.That(consoleAbstraction, Is.Not.Null);
        Assert.That(hostingService, Is.Not.Null);
    }

    [Test]
    public void BuildServiceProvider_Should_ReturnConfiguredProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        ModuleLoader.ConfigureServices(services);

        // Act
        var provider = ModuleLoader.BuildServiceProvider(services);

        // Assert
        Assert.That(provider, Is.Not.Null);
        Assert.That(provider.GetService<IConsoleAbstraction>(), Is.Not.Null);
        Assert.That(provider.GetService<IHostingService>(), Is.Not.Null);
    }
}