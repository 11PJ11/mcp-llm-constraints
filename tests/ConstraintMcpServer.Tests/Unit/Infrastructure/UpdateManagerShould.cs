using ConstraintMcpServer.Domain.Distribution;
using ConstraintMcpServer.Tests.TestDoubles;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Unit;

/// <summary>
/// Unit tests for UpdateManager functionality.
/// Following Outside-In TDD: These tests drive the implementation of update features.
/// Business focus: Automatic updates with configuration preservation and time limits.
/// </summary>
[TestFixture]
[Category("Unit")]
public class UpdateManagerShould
{
    /// <summary>
    /// Business behavior: Users should be able to request automatic updates that complete within reasonable time limits.
    /// Following TDD: This test will initially fail, driving the implementation.
    /// </summary>
    [Test]
    public async Task RequestAutomaticUpdateWithDefaultOptions()
    {
        var result = await ExecuteUpdateWithOptions(UpdateOptions.Default);

        AssertUpdateSuccessful(result);
        Assert.That(result.UpdateTimeSeconds, Is.LessThan(UpdateOptions.Default.TimeoutSeconds), "Update should complete within time limit");
        Assert.That(result.ConfigurationPreserved, Is.True, "User configuration should be preserved during update");
        Assert.That(result.InstalledVersion, Is.Not.Null.And.Not.Empty, "Should report installed version");
    }

    /// <summary>
    /// Business behavior: Update process should preserve existing user configurations.
    /// Configuration preservation is critical for user experience.
    /// </summary>
    [Test]
    public async Task PreserveConfigurationDuringUpdate()
    {
        var updateOptions = new UpdateOptions { PreserveConfiguration = true };
        var result = await ExecuteUpdateWithOptions(updateOptions);

        Assert.That(result.ConfigurationPreserved, Is.True, "User configurations must be preserved during updates");
    }

    /// <summary>
    /// Business behavior: Update should complete within specified time limits.
    /// Time limit compliance ensures responsive user experience.
    /// </summary>
    [Test]
    public async Task CompleteUpdateWithinTimeLimit()
    {
        var updateOptions = new UpdateOptions { TimeoutSeconds = TestTimeoutSeconds };
        var result = await ExecuteUpdateWithOptions(updateOptions);

        Assert.That(result.UpdateTimeSeconds, Is.LessThan(TestTimeoutSeconds),
            $"Update should complete within {TestTimeoutSeconds} seconds for responsive UX");
    }

    private static IInstallationManager CreateUpdateManager() => new TestUpdateService();

    private static async Task<UpdateResult> ExecuteUpdateWithOptions(UpdateOptions options)
    {
        var updateManager = CreateUpdateManager();
        return await updateManager.UpdateSystemAsync(options);
    }

    private static void AssertUpdateSuccessful(UpdateResult result)
    {
        Assert.That(result.IsSuccess, Is.True, "Automatic update should complete successfully");
    }

    private const int TestTimeoutSeconds = 5;
}
