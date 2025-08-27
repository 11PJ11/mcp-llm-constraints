using System.Threading.Tasks;
using ConstraintMcpServer.Tests.Framework;
using ConstraintMcpServer.Tests.Steps;
using NUnit.Framework;
using static ConstraintMcpServer.Tests.Framework.ScenarioBuilder;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// BDD-style end-to-end test for constraint configuration loading.
/// Focuses on business value: reliable YAML constraint pack loading with validation.
/// </summary>
[TestFixture]
public class ConfigLoadE2E
{
    private McpServerSteps? _steps;

    [SetUp]
    public void SetUp()
    {
        _steps = new McpServerSteps();
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
    public async Task Constraint_Server_Loads_Valid_Configuration_Successfully()
    {
        // Scenario: Constraint server loads valid YAML configuration
        // Business value: As a developer, I need confidence that the constraint server
        // can load and validate constraint packs from YAML files, ensuring reliable
        // constraint enforcement during LLM code generation sessions

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .And(_steps.ValidConstraintConfigurationExists)
            .When(_steps.StartServerWithConfiguration)
            .Then(_steps.ServerLoadsConfigurationSuccessfully)
            .And(_steps.ServerAdvertisesConstraintCapabilities)
            .And(_steps.ProcessBehavesPredictably)
            .ExecuteAsync();

        // So that I can trust the constraint enforcement system
        // will apply the correct constraints during coding sessions
        // (This is achieved by the successful completion of the above steps)
    }
}
