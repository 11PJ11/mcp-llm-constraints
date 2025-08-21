using System.Threading.Tasks;
using ConstraintMcpServer.Tests.Framework;
using ConstraintMcpServer.Tests.Steps;
using NUnit.Framework;
using static ConstraintMcpServer.Tests.Framework.ScenarioBuilder;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// BDD-style end-to-end test for MCP server help discoverability.
/// Focuses on business value without exposing implementation details.
/// </summary>
[TestFixture]
public class HelpE2E
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
        _steps?.Dispose();
    }

    [Test]
    public async Task Mcp_ServerHelp_Is_Discoverable()
    {
        // Scenario: MCP help is discoverable over stdio
        // Business value: As a developer, I need confidence that the constraint server starts,
        // is discoverable via MCP, and provides useful information about its purpose and capabilities

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .When(_steps.RequestHelpFromServer)
            .Then(_steps.ReceiveConciseProductDescription)
            .And(_steps.ReceiveMainCommands)
            .And(_steps.ProcessBehavesPredictably)
            .ExecuteAsync();

        // So that I can self‑diagnose environment issues in an agent/IDE context
        // (This is achieved by the successful completion of the above steps)
    }
}
