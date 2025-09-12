using System.Threading.Tasks;
using ConstraintMcpServer.Domain.Distribution;

namespace ConstraintMcpServer.Tests.TestDoubles;

/// <summary>
/// Test implementation of IInstallationManager that performs real file system operations.
/// This drives the creation of the actual production implementation through TDD.
/// Business value: Orchestrates installation workflows and coordinates specialized operations.
/// </summary>
public sealed class TestInstallationManager : IInstallationManager
{
    private const double FastInstallationTimeSeconds = 1.0;
    private const double HealthCheckTimeSeconds = 0.5;
    private const string AllSystemsOperationalReport = "All critical components operational";

    public Task<InstallationResult> InstallSystemAsync(InstallationOptions options, CancellationToken cancellationToken = default)
    {
        var installationPath = options.InstallationPath ?? PlatformPathResolver.GetDefaultInstallationPath(options.Platform);

        if (options.CreateDefaultConfiguration)
        {
            SystemFileOperations.CreateInstallationDirectories(installationPath);
        }

        if (options.ConfigureEnvironmentPath)
        {
            EnvironmentPathManager.ConfigureEnvironmentPath(installationPath);
        }

        var result = InstallationResult.Success(
            installationPath: installationPath,
            platform: options.Platform,
            timeSeconds: FastInstallationTimeSeconds,
            configurationCreated: options.CreateDefaultConfiguration,
            pathConfigured: options.ConfigureEnvironmentPath);

        return Task.FromResult(result);
    }

    public Task<HealthCheckResult> ValidateSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        var checks = HealthCheckFactory.CreateStandardHealthChecks();

        var result = HealthCheckResult.Healthy(
            checkTimeSeconds: HealthCheckTimeSeconds,
            checks: checks,
            report: AllSystemsOperationalReport);

        return Task.FromResult(result);
    }

    public Task<UpdateResult> UpdateSystemAsync(UpdateOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Update functionality not yet implemented");
    }

    public Task<UninstallResult> UninstallSystemAsync(UninstallOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Uninstall functionality not yet implemented");
    }
}
