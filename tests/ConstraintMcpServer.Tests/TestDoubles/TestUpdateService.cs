using ConstraintMcpServer.Domain.Distribution;

namespace ConstraintMcpServer.Tests.TestDoubles;

/// <summary>
/// Focused test double for update functionality testing.
/// Implements only the update behavior needed for unit tests.
/// </summary>
internal sealed class TestUpdateService : IInstallationManager
{
    private const string CurrentVersion = "v1.0.0";
    private const string UpdateVersion = "v1.1.0";
    private const double UpdateTimeSeconds = 1.5;

    public Task<UpdateResult> UpdateSystemAsync(UpdateOptions options, CancellationToken cancellationToken = default)
    {
        var result = UpdateResult.Success(
            installedVersion: options.TargetVersion ?? UpdateVersion,
            previousVersion: CurrentVersion,
            timeSeconds: UpdateTimeSeconds,
            configPreserved: options.PreserveConfiguration);

        return Task.FromResult(result);
    }

    public Task<InstallationResult> InstallSystemAsync(InstallationOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Use TestInstallationService for installation testing");
    }

    public Task<HealthCheckResult> ValidateSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Use TestHealthCheckService for health check testing");
    }

    public Task<UninstallResult> UninstallSystemAsync(UninstallOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Use TestUninstallService for uninstall testing");
    }
}
