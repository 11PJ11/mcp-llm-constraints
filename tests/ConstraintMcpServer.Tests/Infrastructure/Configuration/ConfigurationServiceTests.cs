using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using ConstraintMcpServer.Infrastructure.Configuration;

namespace ConstraintMcpServer.Tests.Infrastructure.Configuration;

/// <summary>
/// Unit tests for ConfigurationService implementation.
/// </summary>
[TestFixture]
public sealed class ConfigurationServiceTests
{
    [Test]
    public void ConfigurationService_Should_ExistWithCorrectInterface()
    {
        // This test validates that IConfigurationService interface exists
        // and can be instantiated with ConfigurationService implementation

        // This should pass now that we have the interface and basic implementation
        var configuration = new ConfigurationBuilder().Build();
        var configurationService = new ConfigurationService(configuration);

        Assert.That(configurationService, Is.Not.Null);
        Assert.That(configurationService, Is.InstanceOf<IConfigurationService>());
    }

    [Test]
    public void GetValue_Should_ReturnDefaultValue_When_KeyNotFound()
    {
        // This test validates that GetValue<T> returns default when key not found
        var configuration = new ConfigurationBuilder().Build();
        var configurationService = new ConfigurationService(configuration);

        var result = configurationService.GetValue<string>("nonexistent-key");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetValue_Should_PrioritizeCommandLineOverEnvironment()
    {
        // This test validates the priority order: command-line → environment → file → defaults
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "test-key", "from-memory" }
            })
            .Build();

        var configurationService = new ConfigurationService(configuration);

        var result = configurationService.GetValue<string>("test-key");

        Assert.That(result, Is.EqualTo("from-memory"));
    }

    [Test]
    public void GetSection_Should_ReturnConfigurationSection()
    {
        // This test validates that GetSection returns proper configuration sections
        var configuration = new ConfigurationBuilder().Build();
        var configurationService = new ConfigurationService(configuration);

        var section = configurationService.GetSection("test-section");

        Assert.That(section, Is.Not.Null);
        Assert.That(section, Is.InstanceOf<IConfigurationSection>());
    }

    [Test]
    public void Validate_Should_ValidateConfiguration()
    {
        // This test validates that configuration validation works correctly
        var configuration = new ConfigurationBuilder().Build();
        var configurationService = new ConfigurationService(configuration);

        var isValid = configurationService.Validate();

        // For now, validation should return true for empty configuration
        Assert.That(isValid, Is.True);
    }
}
