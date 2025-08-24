using System.Diagnostics;
using System.Threading.Tasks;
using ConstraintMcpServer.Tests.Framework;
using ConstraintMcpServer.Tests.Steps;
using NUnit.Framework;
using static ConstraintMcpServer.Tests.Framework.ScenarioBuilder;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// BDD-style end-to-end test for performance budget validation.
/// Focuses on business value: sub-50ms p95 latency for production readiness.
/// </summary>
[TestFixture]
public class PerformanceBudgetE2E
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
    [Ignore("Performance test hanging in CI/CD - needs debugging for tools/call communication")]
    public async Task Constraint_Server_Meets_Performance_Budget_Requirements()
    {
        // Scenario: Server meets performance budgets under load
        // Business value: As a developer, I need the constraint server to respond
        // within 50ms (p95) and 100ms (p99) so that it doesn't slow down my coding workflow
        // and can be safely integrated into production environments

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .And(_steps.ServerStartsWithDefaultConfiguration)
            .When(_steps.ProcessMultipleToolCallsUnderLoad)
            .Then(_steps.P95LatencyIsWithinBudget)
            .And(_steps.P99LatencyIsWithinBudget)
            .And(_steps.NoPerformanceRegressionDetected)
            .ExecuteAsync();

        // So that I can trust the constraint enforcement system
        // will not impact my development velocity or production performance
        // (This is achieved by the successful completion of the above steps)
    }
}
