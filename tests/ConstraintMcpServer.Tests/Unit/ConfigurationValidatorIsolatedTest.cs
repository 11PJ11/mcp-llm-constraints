using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Selection;

namespace ConstraintMcpServer.Tests.Unit;

/// <summary>
/// Isolated test to verify ConfigurationValidator works without other compilation errors.
/// </summary>
[TestFixture]
public class ConfigurationValidatorIsolatedTest
{
    [Test]
    public async Task ConfigurationValidator_ValidConfig_ShouldPass()
    {
        var validator = new ConfigurationValidator();
        var validConfig = new
        {
            version = "v0.2.0",
            constraint_management = new
            {
                injection_cadence = 5
            }
        };

        var result = await validator.ValidateAsync(validConfig);
        
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.ErrorMessages, Is.Empty);
    }

    [Test]
    public async Task ConfigurationValidator_MissingVersion_ShouldFail()
    {
        var validator = new ConfigurationValidator();
        var invalidConfig = new
        {
            constraint_management = new
            {
                injection_cadence = 5
            }
        };

        var result = await validator.ValidateAsync(invalidConfig);
        
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessages, Contains.Item("Version is required"));
    }
}