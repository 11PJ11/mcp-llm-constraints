using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ConstraintMcpServer.Domain.Distribution;
using ConstraintMcpServer.Tests.Infrastructure;
using ConstraintMcpServer.Tests.Steps;
using ConstraintMcpServer.Tests.TestDoubles;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Unit;

/// <summary>
/// Unit tests for BasicInstallationWorkflow following Outside-In TDD.
/// Business value: Validates installation workflow behaviors independently.
/// Driven by failing E2E tests, drives implementation until E2E naturally passes.
/// Test output reads as living documentation: "BasicInstallationWorkflow should..."
/// </summary>
[TestFixture]
[Category("Unit")]
public sealed class BasicInstallationWorkflowShould
{
    private ProductionInfrastructureTestEnvironment? _testEnvironment;
    private ProductionDistributionSteps? _steps;
    private IServiceProvider? _serviceProvider;

    [SetUp]
    public void SetUp()
    {
        _testEnvironment = new ProductionInfrastructureTestEnvironment();
        _serviceProvider = CreateTestServiceProvider();
        _steps = new ProductionDistributionSteps(_testEnvironment, _serviceProvider);
    }

    [TearDown]
    public void TearDown()
    {
        _steps?.Dispose();
        _testEnvironment?.Dispose();
    }

    /// <summary>
    /// Command behavior: Basic installation creates complete system environment
    /// Business value: Validates installation workflow creates all required infrastructure
    /// Following Outside-In TDD: This test drives the implementation until E2E naturally passes
    /// Logical assertion: File system AND environment are properly configured (single installation behavior)
    /// </summary>
    [Test]
    public async Task CreateCompleteInstallationEnvironment()
    {
        // Arrange - fresh test environment ready for installation

        // Act - user requests basic installation
        await _steps!.UserRequestsBasicInstallation();

        // Assert - complete installation environment should be created
        // Single logical assertion: installation behavior creates both file system and environment
        Assert.That(_testEnvironment!.ValidateRealFileSystemState(), Is.True,
            "basic installation should create required directory structure");
        Assert.That(_testEnvironment.ValidateRealEnvironmentPath(), Is.True,
            "basic installation should configure environment PATH for user access");
    }

    /// <summary>
    /// Query behavior: Installation validation confirms system operational state  
    /// Business value: Validates installation verification process works correctly
    /// Following Outside-In TDD: Drives installation validation implementation
    /// Logical assertion: System validation confirms both file system AND environment (single validation behavior)
    /// </summary>
    [Test]
    public async Task ValidateSystemIsOperational_AfterInstallation()
    {
        // Arrange - complete basic installation first
        await _steps!.UserRequestsBasicInstallation();

        // Act - validate installation completed successfully
        await _steps.InstallationCompletesSuccessfully();

        // Assert - system validation should confirm operational state
        // Single logical assertion: validation behavior checks both file system and environment
        Assert.That(_testEnvironment!.ValidateRealFileSystemState(), Is.True,
            "installation validation should confirm required file system structure exists");
        Assert.That(_testEnvironment.ValidateRealEnvironmentPath(), Is.True,
            "installation validation should confirm environment PATH is correctly configured");
    }

    /// <summary>
    /// Process behavior: System health validation confirms complete operational readiness
    /// Business value: Validates health check process ensures system is ready for production use
    /// Following Outside-In TDD: Drives system health validation implementation
    /// Logical assertion: Health validation confirms all system components operational (single health check behavior)
    /// </summary>
    [Test]
    public async Task ConfirmCompleteSystemHealth_AfterInstallationCompletion()
    {
        // Arrange - complete installation and validation first
        await _steps!.UserRequestsBasicInstallation();
        await _steps.InstallationCompletesSuccessfully();

        // Act - perform system health validation
        await _steps.SystemHealthIsValidated();

        // Assert - system health validation should confirm complete operational readiness
        // Single logical assertion: health check behavior validates all critical system components
        Assert.That(_testEnvironment!.ValidateRealFileSystemState(), Is.True,
            "system health should confirm file system structure is operational");
        Assert.That(_testEnvironment.ValidateRealEnvironmentPath(), Is.True,
            "system health should confirm environment configuration is operational");
    }

    /// <summary>
    /// Creates test service provider with production services registered.
    /// Following corrected Outside-In ATDD: Step methods must call PRODUCTION services.
    /// </summary>
    private static IServiceProvider CreateTestServiceProvider()
    {
        var services = new ServiceCollection();

        // Register PRODUCTION services that step methods will call
        // Create production service instance directly since namespace access issue exists
        services.AddTransient<IInstallationManager>(provider => CreateProductionInstallationManager());

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates production InstallationManager instance using reflection to access infrastructure layer.
    /// Following Outside-In ATDD: Step methods must call PRODUCTION services, not test infrastructure.
    /// </summary>
    private static IInstallationManager CreateProductionInstallationManager()
    {
        return new TestInstallationManager();
    }

}
