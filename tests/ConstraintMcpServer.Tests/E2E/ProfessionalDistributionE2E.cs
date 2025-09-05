using System;
using System.Threading.Tasks;
using ConstraintMcpServer.Tests.Framework;
using ConstraintMcpServer.Tests.Steps;
using NUnit.Framework;
using static ConstraintMcpServer.Tests.Framework.ScenarioBuilder;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// BDD-style end-to-end tests for Professional Distribution system.
/// Focuses on business value: enabling real-world user adoption through
/// professional installation, updates, and cross-platform support.
/// </summary>
[TestFixture]
public class ProfessionalDistributionE2E
{
    private ProfessionalDistributionSteps? _steps;

    [SetUp]
    public void SetUp()
    {
        _steps = new ProfessionalDistributionSteps();
    }

    [TearDown]
    public void TearDown()
    {
        // Deterministic cleanup ordering to prevent resource conflicts
        try
        {
            _steps?.Dispose();
        }
        catch (Exception ex)
        {
            // Log cleanup errors but don't fail the test
            System.Console.WriteLine($"Warning: Test cleanup encountered error: {ex.Message}");
        }
        finally
        {
            _steps = null;
        }
    }

    [Test]
    public async Task Should_Provide_OneCommand_Installation_Across_Platforms()
    {
        // Business Scenario: User wants professional installation experience
        // Business value: Single command installs system with configuration preservation
        // Expected: Installation completes within 30 seconds and system is immediately usable

        await Given(_steps!.UserHasCleanSystemEnvironment)
            .And(_steps.UserHasRequiredPlatformPrerequisites)
            .When(_steps.ExecutesInstallationCommand)
            .Then(_steps.SystemIsInstalledSuccessfully)
            .And(_steps.ConfigurationDirectoriesAreCreated)
            .And(_steps.EnvironmentPathIsConfigured)
            .And(_steps.InstallationCompletesWithin30Seconds)
            .And(_steps.SystemIsImmediatelyUsable)
            .ExecuteAsync();

        // So that users can start using constraint system immediately
        // without manual setup or configuration complexity
    }

    [Test]
    public async Task Should_Provide_Seamless_Updates_With_Configuration_Preservation()
    {
        // Business Scenario: User needs system updates without losing configurations
        // Business value: Automated updates preserve user constraint configurations
        // Expected: Updates complete within 10 seconds with 100% configuration preservation

        await Given(_steps!.UserHasSystemInstalledWithCustomConstraints)
            .And(_steps.NewVersionIsAvailableOnGitHub)
            .And(_steps.UpdateCheckRunsAutomatically)
            .And(_steps.UserApprovesUpdate)
            .When(_steps.SystemUpdatesInBackground)
            .Then(_steps.UserConstraintsArePreserved)
            .And(_steps.ConfigurationRemainsIntact)
            .And(_steps.UpdateCompletesWithin10Seconds)
            .And(_steps.SystemContinuesOperatingNormally)
            .ExecuteAsync();

        // So that users can keep their custom constraint configurations
        // while benefiting from system improvements and bug fixes
    }

    [Test]
    [Ignore("Temporarily disabled until implementation - Phase 1 E2E test drives implementation")]
    public async Task Should_Support_Installation_Across_Major_Platforms()
    {
        // Business Scenario: Users on different platforms need consistent experience
        // Business value: Installation works identically across Linux/Windows/macOS
        // Expected: All platforms provide identical functionality and configuration

        await Given(_steps!.MultiPlatformTestEnvironmentExists)
            .And(_steps.InstallationRunsOnLinux)
            .And(_steps.InstallationRunsOnWindows)
            .And(_steps.InstallationRunsOnMacOS)
            .When(_steps.AllPlatformsExecuteInstallation)
            .Then(_steps.LinuxInstallationSucceeds)
            .And(_steps.WindowsInstallationSucceeds)
            .And(_steps.MacOSInstallationSucceeds)
            .And(_steps.AllPlatformsHaveIdenticalConfiguration)
            .And(_steps.AllPlatformsPassHealthChecksWithin5Seconds)
            .ExecuteAsync();

        // So that development teams using mixed platforms can have
        // consistent constraint system behavior and configuration
    }

    [Test]
    [Ignore("Temporarily disabled until implementation - Phase 1 E2E test drives implementation")]
    public async Task Should_Provide_Clean_Uninstall_With_Configuration_Options()
    {
        // Business Scenario: User needs clean uninstall with configuration choices
        // Business value: Complete removal with option to preserve user configurations
        // Expected: Clean removal with configuration preservation options

        await Given(_steps!.UserHasSystemInstalledWithConfigurations)
            .And(_steps.ExecutesUninstallCommand)
            .And(_steps.UserChoosesToPreserveConfigurations)
            .When(_steps.UninstallProcessExecutes)
            .Then(_steps.UserIsPromptedAboutConfigurationPreservation)
            .And(_steps.SystemBinariesAreRemoved)
            .And(_steps.UserConfigurationsArePreserved)
            .And(_steps.EnvironmentPathIsCleanedUp)
            .And(_steps.PreviousConfigurationsCanBeRestored)
            .ExecuteAsync();

        // So that users can cleanly remove the system while maintaining
        // the option to restore their custom configurations later
    }

    [Test]
    [Ignore("Temporarily disabled until implementation - Phase 1 E2E test drives implementation")]
    public async Task Should_Provide_System_Health_Diagnostics()
    {
        // Business Scenario: User needs system health validation and troubleshooting
        // Business value: Comprehensive health checks with diagnostic guidance
        // Expected: Health checks complete within 5 seconds with actionable diagnostics

        await Given(_steps!.SystemIsInstalled)
            .When(_steps.ExecutesHealthCheckCommand)
            .Then(_steps.EnvironmentRequirementsAreValidated)
            .And(_steps.ConfigurationIntegrityIsVerified)
            .And(_steps.McpProtocolConnectivityIsConfirmed)
            .And(_steps.ConstraintSystemFunctionalityIsTested)
            .And(_steps.HealthCheckCompletesWithin5Seconds)
            .And(_steps.DiagnosticReportIsGenerated)
            .ExecuteAsync();

        // So that users can quickly validate their constraint system
        // and receive guidance for resolving any environment issues
    }
}
