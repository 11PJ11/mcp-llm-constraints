using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Selection;

namespace ConstraintMcpServer.Tests.Unit;

/// <summary>
/// Unit tests for ConfigurationValidator using proper Outside-In TDD.
/// These tests will fail initially and drive the implementation of real business logic.
/// </summary>
[TestFixture]
public class ConfigurationValidatorTests
{
    [Test]
    public async Task ValidateConfiguration_WithValidV2Format_ShouldReturnTrue()
    {
        // Arrange - This will fail because ConfigurationValidator doesn't exist yet
        // This is the RIGHT REASON for the test to fail - missing implementation
        
        var validator = new ConfigurationValidator();
        var validConfig = new
        {
            version = "v0.2.0",
            constraint_management = new
            {
                injection_cadence = 5,
                priority_thresholds = new { high = 0.8, medium = 0.5 }
            },
            constraint_packs = new[] { "tdd", "solid", "clean-architecture" }
        };

        // Act
        var result = await validator.ValidateAsync(validConfig);

        // Assert
        Assert.That(result.IsValid, Is.True, "Should validate correct v0.2.0 format");
        Assert.That(result.ErrorMessages, Is.Empty, "Should have no validation errors for valid config");
    }

    [Test]
    public async Task ValidateConfiguration_WithMissingVersion_ShouldReturnFalse()
    {
        // Arrange
        var validator = new ConfigurationValidator();
        var invalidConfig = new
        {
            constraint_management = new
            {
                injection_cadence = 5
            }
        };

        // Act
        var result = await validator.ValidateAsync(invalidConfig);

        // Assert
        Assert.That(result.IsValid, Is.False, "Should reject configuration without version");
        Assert.That(result.ErrorMessages, Contains.Item("Version is required"), "Should specify missing version error");
    }

    [Test]
    public async Task ValidateConfiguration_WithInvalidInjectionCadence_ShouldReturnFalse()
    {
        // Arrange
        var validator = new ConfigurationValidator();
        var invalidConfig = new
        {
            version = "v0.2.0",
            constraint_management = new
            {
                injection_cadence = -1  // Invalid: negative cadence
            }
        };

        // Act
        var result = await validator.ValidateAsync(invalidConfig);

        // Assert
        Assert.That(result.IsValid, Is.False, "Should reject negative injection cadence");
        Assert.That(result.ErrorMessages, Contains.Item("Injection cadence must be positive"), "Should specify cadence validation error");
    }
}