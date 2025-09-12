using System.Threading.Tasks;
using ConstraintMcpServer.Tests.Framework;
using ConstraintMcpServer.Tests.Infrastructure;
using ConstraintMcpServer.Tests.Steps;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// End-to-End tests for Production Installation workflows.
/// Uses Outside-In TDD methodology with Given-When-Then BDD structure.
/// These tests drive the implementation of professional distribution features.
/// </summary>
[TestFixture]
[Category("E2E")]
public class ProductionInstallationE2E
{
    private ProductionDistributionSteps? _steps;

    [SetUp]
    public void SetUp()
    {
        // This will fail initially - we need to implement ProductionInfrastructureTestEnvironment
        // Following Outside-In TDD: E2E test fails for the RIGHT REASON (missing implementation)
        var testEnvironment = new ProductionInfrastructureTestEnvironment();
        _steps = new ProductionDistributionSteps(testEnvironment);
    }

    [TearDown]
    public void TearDown()
    {
        _steps?.Dispose();
    }

    /// <summary>
    /// E2E Test: Basic Installation Workflow
    /// This test drives the implementation of core installation features.
    /// Following Outside-In TDD: This test should FAIL initially and drive inner unit test loops.
    /// </summary>
    [Test]
    [Ignore("Temporarily disabled until SimpleInstallationE2E implementation completes - following One E2E at a Time ATDD rule")]
    public async Task Real_Production_Installation_Should_Complete_Basic_Workflow()
    {
        // This E2E test will fail initially with NotImplementedException
        // Inner unit tests will drive the implementation until this naturally passes
        await ConstraintMcpServer.Tests.Framework.ScenarioBuilder.Given(_steps!.SystemHasRequiredPermissions)
            .And(_steps.PlatformIsDetectedCorrectly)
            .When(_steps.UserRequestsBasicInstallation)
            .Then(_steps.InstallationCompletesSuccessfully)
            .And(_steps.SystemHealthIsValidated)
            .ExecuteAsync();
    }

    /// <summary>
    /// E2E Test: GitHub Integration Workflow
    /// Tests real GitHub API integration for automated updates.
    /// </summary>
    [Test]
    [Ignore("Will enable after basic installation E2E passes")]
    public async Task Real_GitHub_Integration_Should_Work_With_Live_API()
    {
        await ConstraintMcpServer.Tests.Framework.ScenarioBuilder.Given(_steps!.NetworkConnectivityIsConfirmed)
            .And(_steps.GitHubReleasesAreAvailable)
            .When(_steps.UserRequestsAutomaticUpdate)
            .Then(_steps.UpdateCompletesWithinTimeLimit)
            .And(_steps.ConfigurationIsPreserved)
            .ExecuteAsync();
    }
}
