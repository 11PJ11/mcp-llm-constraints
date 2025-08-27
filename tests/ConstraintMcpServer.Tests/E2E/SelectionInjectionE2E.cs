using System.Threading.Tasks;
using ConstraintMcpServer.Tests.Framework;
using ConstraintMcpServer.Tests.Steps;
using NUnit.Framework;
using static ConstraintMcpServer.Tests.Framework.ScenarioBuilder;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// BDD-style end-to-end test for constraint selection and injection.
/// Focuses on business value: intelligent constraint selection by priority and phase.
/// </summary>
[TestFixture]
public class SelectionInjectionE2E
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
    public async Task Constraint_Server_Injects_Prioritized_Constraints_By_Phase()
    {
        // Scenario: Server selects and injects constraints based on priority and phase
        // Business value: As a developer, I need the server to intelligently select 
        // the most relevant constraints for my current development phase, ensuring 
        // focused and contextual constraint enforcement

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .And(_steps.ConstraintPackWithMultiplePriorities)
            .And(_steps.ServerStartsWithPhaseTracking)
            .When(_steps.SimulateToolCallsInRedPhase)
            .Then(_steps.ServerInjectsConstraintsByPriority)
            .And(_steps.ConstraintMessageContainsAnchors)
            .And(_steps.ConstraintMessageContainsTopKReminders)
            .And(McpServerSteps.PassThroughCallsRemainUnchanged)
            .ExecuteAsync();

        // So that I can trust the constraint enforcement system
        // will provide relevant, prioritized guidance during coding sessions
        // (This is achieved by the successful completion of the above steps)
    }
}
