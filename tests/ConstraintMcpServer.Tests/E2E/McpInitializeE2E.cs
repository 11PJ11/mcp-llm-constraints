using System.Threading.Tasks;
using ConstraintMcpServer.Tests.Framework;
using ConstraintMcpServer.Tests.Steps;
using NUnit.Framework;
using static ConstraintMcpServer.Tests.Framework.ScenarioBuilder;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// BDD-style end-to-end test for MCP server initialization protocol.
/// Focuses on business value of proper MCP handshake and session management.
/// </summary>
[TestFixture]
public class McpInitializeE2E
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
    public async Task Mcp_Initialize_Should_ReturnCapabilities()
    {
        // Scenario: Server acknowledges initialization
        // Business value: As an MCP client (IDE/agent), I need the constraint server to properly
        // handshake via MCP protocol so that I can establish a reliable session for constraint injection

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .When(_steps.SendInitializeRequest)
            .Then(_steps.ReceiveCapabilitiesResponse)
            .And(_steps.VerifyLatencyBudget)
            .And(_steps.VerifyProtocolCompliance)
            .ExecuteAsync();

        // So that an IDE or agent can connect and start using constraint reinforcement
        // (This is achieved by the successful completion of the above steps)
    }

    [Test]
    public async Task Mcp_InitializeShutdown_Should_CompleteCleanly()
    {
        // Scenario: Full MCP session lifecycle
        // Business value: As an MCP client, I need to be able to cleanly start and stop
        // constraint server sessions to ensure proper resource management

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .When(_steps.SendInitializeAndShutdownSequence)
            .Then(_steps.ReceiveShutdownConfirmation)
            .And(_steps.VerifyCleanSessionTermination)
            .ExecuteAsync();

        // So that session management is predictable and safe for CI/CD
        // (This is achieved by the successful completion of the above steps)
    }
}
