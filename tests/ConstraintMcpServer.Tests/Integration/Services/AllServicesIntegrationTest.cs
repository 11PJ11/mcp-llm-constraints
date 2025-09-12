using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Selection;

namespace ConstraintMcpServer.Tests.Integration.Services;

/// <summary>
/// Integration test to verify all implemented services work together.
/// </summary>
[TestFixture]
public class AllServicesIntegrationTest
{
    [Test]
    public async Task AllImplementedServices_ShouldWorkCorrectly()
    {
        // Test ConfigurationMigrationManager
        var migrationManager = new ConfigurationMigrationManager();
        var settings = new Dictionary<string, object>
        {
            ["constraint_packs"] = new[] { "tdd", "solid" },
            ["injection_cadence"] = 5
        };

        var saveResult = await migrationManager.SaveUserSettingsAsync(settings);
        Assert.That(saveResult, Is.True, "ConfigurationMigrationManager should save settings");

        var migrationResult = await migrationManager.MigrateUserSettingsAsync("v0.1", "v0.2");
        Assert.That(migrationResult.IsSuccess, Is.True, "ConfigurationMigrationManager should migrate successfully");
        Assert.That(migrationResult.PreservedSettingsCount, Is.EqualTo(2), "Should preserve 2 settings");

        // Test ConfigurationValidator
        var validator = new ConfigurationValidator();
        var validConfig = new
        {
            version = "v0.2.0",
            constraint_management = new
            {
                injection_cadence = 5
            }
        };

        var validationResult = await validator.ValidateAsync(validConfig);
        Assert.That(validationResult.IsValid, Is.True, "ConfigurationValidator should validate correct config");

        // Test BackwardCompatibilityManager
        var compatibilityManager = new BackwardCompatibilityManager();
        var compatibilityResult = await compatibilityManager.EnsureRollbackCompatibilityAsync("v0.2.0", "v0.1.0", validConfig);
        Assert.That(compatibilityResult.IsCompatible, Is.True, "BackwardCompatibilityManager should support v0.2->v0.1 rollback");
        Assert.That(compatibilityResult.RequiredTransformations, Is.Not.Empty, "Should identify required transformations");
    }
}
