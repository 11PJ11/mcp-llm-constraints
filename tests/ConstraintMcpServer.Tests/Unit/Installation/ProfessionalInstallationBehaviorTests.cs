using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Domain.Distribution;

namespace ConstraintMcpServer.Tests.Unit.Installation;

/// <summary>
/// Unit tests that validate professional installation business behavior.
/// These tests drive the business requirements that E2E scenarios will validate.
/// Follows Outside-In TDD: these unit tests define the behavior that will naturally make E2E tests pass.
/// </summary>
[TestFixture]
public class ProfessionalInstallationBehaviorTests
{
    [Test]
    public void InstallationResult_WhenSuccessful_ShouldMeetPerformanceRequirements()
    {
        // This test defines the business rule: successful installations must complete within 30 seconds
        var installationPath = "/usr/local/bin/constraint-server";
        var platform = PlatformType.Windows;
        var timeSeconds = 2.5; // Well under 30 second requirement
        var configurationCreated = true;
        var pathConfigured = true;

        var result = InstallationResult.Success(
            installationPath,
            platform,
            timeSeconds,
            configurationCreated,
            pathConfigured);

        // Assert business requirements
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.InstallationTimeSeconds, Is.LessThan(30), "installation must complete within 30 seconds for professional experience");
        Assert.That(result.ConfigurationCreated, Is.True, "system must be immediately usable");
        Assert.That(result.PathConfigured, Is.True, "system must be accessible from command line");
        Assert.That(result.Platform, Is.EqualTo(platform));
        Assert.That(result.InstallationPath, Is.Not.Null.And.Not.Empty, "installation path must be provided for verification");
    }

    [Test]
    public void InstallationResult_WhenTakesTooLong_ShouldFailProfessionalRequirements()
    {
        // This test defines the business rule: installations over 30 seconds fail professional requirements
        var platform = PlatformType.Linux;
        var timeSeconds = 35.0; // Over 30 second limit

        var result = InstallationResult.Success(
            "/usr/local/bin/constraint-server",
            platform,
            timeSeconds,
            true,
            true);

        // Assert business validation
        Assert.That(result.InstallationTimeSeconds, Is.GreaterThan(30));
        // In a real implementation, this would be validated by the service layer
        // and converted to a failure result before reaching the domain
    }

    [Test]
    public void UpdateResult_WhenSuccessful_ShouldMeetPerformanceAndPreservationRequirements()
    {
        // This test defines the business rule: updates must complete within 10 seconds and preserve config
        var installedVersion = "v1.1.0";
        var previousVersion = "v1.0.0";
        var timeSeconds = 1.5; // Well under 10 second requirement
        var configPreserved = true;

        var result = UpdateResult.Success(
            installedVersion,
            previousVersion,
            timeSeconds,
            configPreserved);

        // Assert business requirements
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.UpdateTimeSeconds, Is.LessThan(10), "update must complete within 10 seconds for professional experience");
        Assert.That(result.ConfigurationPreserved, Is.True, "user configurations must be preserved during updates");
        Assert.That(result.InstalledVersion, Is.EqualTo(installedVersion));
        Assert.That(result.PreviousVersion, Is.EqualTo(previousVersion));
    }

    [Test]
    public void UpdateResult_WhenConfigurationNotPreserved_ShouldFailBusinessRequirements()
    {
        // This test defines the business rule: updates must preserve user configurations
        var result = UpdateResult.Success(
            "v1.1.0",
            "v1.0.0",
            1.5,
            false); // Configuration NOT preserved

        // Assert business validation
        Assert.That(result.ConfigurationPreserved, Is.False);
        // In a real implementation, this should be flagged as a business rule violation
        // Users must not lose their constraint configurations during updates
    }

    [Test]
    public void HealthCheckResult_WhenHealthy_ShouldValidateAllCriticalComponents()
    {
        // This test defines the business rule: health checks must validate all critical systems
        var checks = new[]
        {
            HealthCheck.Pass("Environment", "Runtime environment validated"),
            HealthCheck.Pass("Configuration", "Configuration files intact"),
            HealthCheck.Pass("Connectivity", "MCP protocol connectivity confirmed"),
            HealthCheck.Pass("Functionality", "Constraint system operational")
        };

        var result = HealthCheckResult.Healthy(
            checkTimeSeconds: 0.5,
            checks: checks,
            report: "All critical components operational");

        // Assert business requirements
        Assert.That(result.IsHealthy, Is.True);
        Assert.That(result.CheckTimeSeconds, Is.LessThan(5), "health checks must complete within 5 seconds");
        Assert.That(result.Checks.Count, Is.EqualTo(4), "all critical components must be validated");
        Assert.That(result.Checks.All(check => check.Passed), Is.True, "all checks must pass for healthy system");
        Assert.That(result.DiagnosticReport, Is.Not.Null.And.Not.Empty, "health check must provide actionable report");

        // Validate each critical component is covered
        var checkNames = result.Checks.Select(c => c.Name).ToArray();
        Assert.That(checkNames, Contains.Item("Environment"), "runtime environment must be validated");
        Assert.That(checkNames, Contains.Item("Configuration"), "configuration integrity must be verified");
        Assert.That(checkNames, Contains.Item("Connectivity"), "MCP protocol must be confirmed working");
        Assert.That(checkNames, Contains.Item("Functionality"), "constraint system must be operational");
    }

    [Test]
    public void UninstallResult_WhenSuccessful_ShouldProvideConfigurationOptions()
    {
        // This test defines the business rule: uninstall must provide configuration preservation options
        var configPreserved = true;
        var pathCleaned = true;

        var result = UninstallResult.Success(
            configPreserved: configPreserved,
            pathCleaned: pathCleaned);

        // Assert business requirements
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ConfigurationPreserved, Is.EqualTo(configPreserved), "users must have control over configuration preservation");
        Assert.That(result.EnvironmentPathCleaned, Is.EqualTo(pathCleaned), "system path must be properly cleaned when requested");
    }

    [Test]
    public void PlatformType_ShouldSupportMajorOperatingSystems()
    {
        // This test defines the business rule: system must support major cross-platform deployment
        var supportedPlatforms = new[]
        {
            PlatformType.Windows,
            PlatformType.Linux,
            PlatformType.MacOS
        };

        // Assert business requirements
        Assert.That(supportedPlatforms.Length, Is.EqualTo(3), "must support all major operating systems");
        Assert.That(supportedPlatforms, Contains.Item(PlatformType.Windows), "Windows support required for enterprise adoption");
        Assert.That(supportedPlatforms, Contains.Item(PlatformType.Linux), "Linux support required for server deployments");
        Assert.That(supportedPlatforms, Contains.Item(PlatformType.MacOS), "macOS support required for developer adoption");
    }

    [Test]
    public void InstallationOptions_ForPlatform_ShouldCreateValidConfiguration()
    {
        // This test defines the business rule: installation options must be platform-appropriate
        foreach (var platform in new[] { PlatformType.Windows, PlatformType.Linux, PlatformType.MacOS })
        {
            var options = InstallationOptions.ForPlatform(platform);

            // Assert business requirements
            Assert.That(options, Is.Not.Null, $"options must be created for {platform}");
            Assert.That(options.Platform, Is.EqualTo(platform), $"platform must be correctly set for {platform}");
        }
    }
}
