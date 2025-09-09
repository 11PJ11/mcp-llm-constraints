using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Selection;

namespace ConstraintMcpServer.Tests.Unit;

/// <summary>
/// Simple test to verify ConfigurationMigrationManager works in isolation.
/// </summary>
[TestFixture]
public class ConfigurationMigrationManagerSimpleTest
{
    [Test]
    public async Task ConfigurationMigrationManager_ShouldWork()
    {
        var manager = new ConfigurationMigrationManager();
        var settings = new Dictionary<string, object>
        {
            ["constraint_packs"] = new[] { "tdd", "solid" },
            ["injection_cadence"] = 5
        };

        var result = await manager.SaveUserSettingsAsync(settings);
        Assert.That(result, Is.True);

        var migrationResult = await manager.MigrateUserSettingsAsync("v0.1", "v0.2");
        Assert.That(migrationResult.IsSuccess, Is.True);
        Assert.That(migrationResult.PreservedSettingsCount, Is.EqualTo(2));
    }
}