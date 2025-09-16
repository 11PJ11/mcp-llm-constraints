using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ConstraintMcpServer.Infrastructure.Configuration;

namespace ConstraintMcpServer.Tests.Infrastructure.Configuration;

/// <summary>
/// Unit tests for ConfigurationModule DI integration.
/// </summary>
[TestFixture]
public sealed class ConfigurationModuleTests
{
    [Test]
    public void ConfigurationModule_Should_RegisterIConfigurationService()
    {
        // This test validates that ConfigurationModule properly registers IConfigurationService
        var services = new ServiceCollection();

        // Add basic configuration
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        var module = new ConfigurationModule();
        module.RegisterServices(services);

        var serviceProvider = services.BuildServiceProvider();
        var configurationService = serviceProvider.GetService<IConfigurationService>();

        Assert.That(configurationService, Is.Not.Null);
        Assert.That(configurationService, Is.InstanceOf<ConfigurationService>());
    }

    [Test]
    public void ConfigurationModule_Should_HaveCorrectPriority()
    {
        // This test validates that ConfigurationModule has priority 20 as specified
        var module = new ConfigurationModule();

        Assert.That(module.Priority, Is.EqualTo(20));
    }
}
