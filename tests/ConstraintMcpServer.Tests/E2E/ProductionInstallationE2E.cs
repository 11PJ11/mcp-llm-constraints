using System.Threading.Tasks;
using ConstraintMcpServer.Tests.Framework;
using ConstraintMcpServer.Tests.Infrastructure;
using ConstraintMcpServer.Tests.Steps;
using NUnit.Framework;
using static ConstraintMcpServer.Tests.Framework.ScenarioBuilder;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// BDD-style end-to-end test for professional installation with ZERO test doubles.
/// Uses real GitHub API, real file system, real environment variables, real process execution.
/// Business focus: User can install constraint system with one command and immediately use it.
/// </summary>
[TestFixture]
public class ProductionInstallationE2E
{
    private ProductionDistributionSteps? _steps;
    private ProductionInfrastructureTestEnvironment? _environment;

    [SetUp]
    public void SetUp()
    {
        _environment = new ProductionInfrastructureTestEnvironment();
        _steps = new ProductionDistributionSteps(_environment);
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            _steps?.Dispose();
            _environment?.Dispose();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Warning: Test cleanup encountered error: {ex.Message}");
        }
        finally
        {
            _steps = null;
            _environment = null;
        }
    }

    [Test]
    public async Task Real_Production_Installation_Should_Complete_Within_30_Seconds_With_Full_System_Setup()
    {
        // Scenario: Professional user installs constraint system using real production infrastructure
        // Business value: User can install with one command and immediately start using the system
        // ZERO TEST DOUBLES: Real GitHub API, real file system, real environment, real processes

        await Given(_steps!.SystemHasRequiredPermissions)
            .And(_steps.PlatformIsDetectedCorrectly)
            .And(_steps.GitHubReleasesAreAvailable)
            .And(_steps.NetworkConnectivityIsConfirmed)
            .When(_steps.UserRequestsOneCommandInstallation)
            .Then(_steps.InstallationCompletesWithin30Seconds)
            .And(_steps.ConfigurationDirectoriesAreCreatedOnRealFileSystem)
            .And(_steps.SystemPathIsActuallyModified)
            .And(_steps.BinariesAreDownloadedFromRealGitHub)
            .And(_steps.ConstraintSystemIsFullyOperationalWithRealExecution)
            .ExecuteAsync();
    }

    [Test]
    public async Task Real_Production_Installation_Should_Handle_Network_Failures_Gracefully()
    {
        // Scenario: Installation handles real network conditions and failures
        // Business value: Robust installation experience in real-world network conditions
        // ZERO TEST DOUBLES: Real network connectivity testing

        await Given(_steps!.SystemHasRequiredPermissions)
            .And(_steps.PlatformIsDetectedCorrectly)
            .When(_steps.NetworkConnectivityIsLimited)
            .And(_steps.UserRequestsOneCommandInstallation)
            .Then(_steps.InstallationProvidesHelpfulNetworkErrorGuidance)
            .And(_steps.UserCanRetryInstallationAfterNetworkRecovery)
            .ExecuteAsync();
    }

    [Test]
    public async Task Real_Production_Installation_Should_Validate_Downloaded_Binary_Integrity()
    {
        // Scenario: Installation validates integrity of downloaded binaries from real GitHub
        // Business value: Security validation ensures users get authentic, uncompromised binaries
        // ZERO TEST DOUBLES: Real binary downloads with real integrity validation

        await Given(_steps!.SystemHasRequiredPermissions)
            .And(_steps.GitHubReleasesAreAvailable)
            .When(_steps.UserRequestsOneCommandInstallation)
            .Then(_steps.BinariesAreDownloadedFromRealGitHub)
            .And(_steps.DownloadedBinariesPassIntegrityValidation)
            .And(_steps.CompromisedBinariesAreRejectedWithClearErrorMessage)
            .ExecuteAsync();
    }

    [Test]
    [Platform("Linux")]
    public async Task Real_Linux_Installation_Should_Integrate_With_Package_Manager()
    {
        // Scenario: Linux installation integrates with real package managers
        // Business value: Professional Linux integration using standard package management
        // ZERO TEST DOUBLES: Real package manager detection and integration

        await Given(_steps!.LinuxDistributionIsDetectedCorrectly)
            .And(_steps.PackageManagerIsAvailableAndFunctional)
            .When(_steps.UserRequestsOneCommandInstallation)
            .Then(_steps.InstallationUsesRealPackageManager)
            .And(_steps.SystemServicesAreConfiguredProperly)
            .And(_steps.DesktopIntegrationIsSetupCorrectly)
            .ExecuteAsync();
    }

    [Test]
    [Platform("Win")]
    public async Task Real_Windows_Installation_Should_Configure_Registry_And_PATH()
    {
        // Scenario: Windows installation configures real registry and PATH
        // Business value: Professional Windows integration using standard Windows practices
        // ZERO TEST DOUBLES: Real Windows registry and environment variable modification

        await Given(_steps!.WindowsVersionIsDetectedCorrectly)
            .And(_steps.AdminPrivilegesAreConfirmed)
            .When(_steps.UserRequestsOneCommandInstallation)
            .Then(_steps.RegistryEntriesAreActuallyCreated)
            .And(_steps.StartMenuShortcutsAreCreated)
            .And(_steps.SystemPATHIsActuallyModified)
            .And(_steps.AddRemoveProgramsEntryIsCreated)
            .ExecuteAsync();
    }

    [Test]
    [Platform("MacOSX")]
    public async Task Real_macOS_Installation_Should_Support_Homebrew_And_App_Bundle()
    {
        // Scenario: macOS installation supports real Homebrew and app bundle creation
        // Business value: Professional macOS integration using standard macOS practices
        // ZERO TEST DOUBLES: Real Homebrew detection and app bundle creation

        await Given(_steps!.MacOSVersionIsDetectedCorrectly)
            .And(_steps.HomebrewIsAvailableAndFunctional)
            .When(_steps.UserRequestsOneCommandInstallation)
            .Then(_steps.HomebrewInstallationIsUsed)
            .And(_steps.AppBundleIsCreatedCorrectly)
            .And(_steps.LaunchpadIntegrationIsSetup)
            .And(_steps.ShellProfileIsUpdatedForPATH)
            .ExecuteAsync();
    }
}
