using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Selection;

namespace ConstraintMcpServer.Tests.Unit;

/// <summary>
/// Isolated test to verify BackwardCompatibilityManager works without other compilation errors.
/// </summary>
[TestFixture]
public class BackwardCompatibilityManagerIsolatedTest
{
    [Test]
    public async Task BackwardCompatibilityManager_ValidRollback_ShouldPass()
    {
        var compatibilityManager = new BackwardCompatibilityManager();
        var config = new
        {
            version = "v0.2.0",
            constraint_management = new
            {
                default_packs = new[] { "tdd", "solid" },
                enhanced_features = new { ai_tuning = true }
            }
        };

        var result = await compatibilityManager.EnsureRollbackCompatibilityAsync("v0.2.0", "v0.1.0", config);
        
        Assert.That(result.IsCompatible, Is.True);
        Assert.That(result.RequiredTransformations, Is.Not.Empty);
    }

    [Test]
    public async Task BackwardCompatibilityManager_VersionCheck_ShouldWork()
    {
        var compatibilityManager = new BackwardCompatibilityManager();

        var isCompatible = await compatibilityManager.CheckVersionCompatibilityAsync("v0.2.0", "v0.1.0");
        
        Assert.That(isCompatible, Is.True);
    }
}