using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConstraintMcpServer.Infrastructure.Configuration;
using ConstraintMcpServer.Infrastructure.Hosting;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// E2E tests for Configuration Infrastructure integration with DI container.
/// Tests the complete configuration loading and DI registration workflow.
/// </summary>
[TestFixture]
public sealed class ConfigurationInfrastructureE2E
{
    [Test]
    public void ConfigurationService_Should_LoadFromMultipleSources_When_RegisteredInDIContainer()
    {
        // This test validates that configuration service is properly integrated with DI
        // and can load configuration from command-line, environment, and file sources
        // following the priority order defined in requirements.

        var services = new ServiceCollection();

        // Add configuration with multiple sources demonstrating priority
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "test-key", "file-value" }
            })
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "test-key", "command-line-value" }  // This should override file value
            })
            .Build());

        var module = new ConfigurationModule();
        module.RegisterServices(services);

        var serviceProvider = services.BuildServiceProvider();
        var configService = serviceProvider.GetRequiredService<IConfigurationService>();

        var value = configService.GetValue<string>("test-key");

        Assert.That(value, Is.EqualTo("command-line-value"));
        Assert.That(configService.Validate(), Is.True);
    }

    [Test]
    public void McpServerHost_Should_UseConfigurationService_When_Available()
    {
        // This test validates that McpServerHost can optionally use IConfigurationService
        // while maintaining backward compatibility with existing CLI behavior.

        var configuration = new ConfigurationBuilder().Build();
        var configurationService = new ConfigurationService(configuration);

        // Test new constructor with configuration service
        var hostWithConfig = new McpServerHost(null, configurationService);
        Assert.That(hostWithConfig, Is.Not.Null);

        // Test backward compatibility - old constructor still works
        var hostWithoutConfig = new McpServerHost();
        Assert.That(hostWithoutConfig, Is.Not.Null);
    }

    [Test]
    public void ConfigurationLoading_Should_CompleteWithinPerformanceBudget()
    {
        // This test validates that configuration loading meets performance requirements:
        // - Configuration loading: <10ms for typical YAML files
        // - Total startup overhead: <20ms additional

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Simulate typical configuration loading
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "server:port", "8080" },
                { "server:host", "localhost" },
                { "logging:level", "Information" },
                { "constraints:enabled", "true" }
            })
            .Build();

        var configurationService = new ConfigurationService(configuration);

        // Test typical operations
        configurationService.GetValue<string>("server:port");
        configurationService.GetSection("server");
        configurationService.Validate();

        stopwatch.Stop();

        // Validate performance requirements
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(10),
            $"Configuration loading took {stopwatch.ElapsedMilliseconds}ms, should be <10ms");
    }
}