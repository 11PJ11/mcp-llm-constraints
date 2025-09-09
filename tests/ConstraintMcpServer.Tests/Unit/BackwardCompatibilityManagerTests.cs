using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Selection;

namespace ConstraintMcpServer.Tests.Unit;

/// <summary>
/// Unit tests for BackwardCompatibilityManager using proper Outside-In TDD.
/// These tests will fail initially and drive the implementation of real business logic.
/// </summary>
[TestFixture]
public class BackwardCompatibilityManagerTests
{
    [Test]
    public async Task EnsureRollbackCompatibility_WithV02ToV01_ShouldReturnTrue()
    {
        // Arrange - This will fail because BackwardCompatibilityManager doesn't exist yet
        // This is the RIGHT REASON for the test to fail - missing implementation

        var compatibilityManager = new BackwardCompatibilityManager();
        var newVersionConfig = new
        {
            version = "v0.2.0",
            constraint_management = new
            {
                default_packs = new[] { "tdd", "solid", "clean-code" },
                enhanced_features = new { ai_tuning = true, adaptive_cadence = true }
            }
        };

        // Act
        var result = await compatibilityManager.EnsureRollbackCompatibilityAsync("v0.2.0", "v0.1.0", newVersionConfig);

        // Assert
        Assert.That(result.IsCompatible, Is.True, "Should support rollback from v0.2.0 to v0.1.0");
        Assert.That(result.RequiredTransformations, Is.Not.Empty, "Should identify required transformations for rollback");
    }

    [Test]
    public async Task EnsureRollbackCompatibility_WithUnsupportedRollback_ShouldReturnFalse()
    {
        // Arrange
        var compatibilityManager = new BackwardCompatibilityManager();
        var futureVersionConfig = new
        {
            version = "v2.0.0",
            breaking_changes = new { major_schema_change = true }
        };

        // Act
        var result = await compatibilityManager.EnsureRollbackCompatibilityAsync("v2.0.0", "v0.1.0", futureVersionConfig);

        // Assert
        Assert.That(result.IsCompatible, Is.False, "Should reject incompatible rollback scenarios");
        Assert.That(result.Reason, Contains.Substring("breaking changes"), "Should explain why rollback is not possible");
    }

    [Test]
    public async Task CheckVersionCompatibility_WithCompatibleVersions_ShouldReturnTrue()
    {
        // Arrange
        var compatibilityManager = new BackwardCompatibilityManager();

        // Act
        var isCompatible = await compatibilityManager.CheckVersionCompatibilityAsync("v0.2.0", "v0.1.0");

        // Assert
        Assert.That(isCompatible, Is.True, "v0.2.0 should be compatible with rollback to v0.1.0");
    }
}
