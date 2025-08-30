using System.Threading.Tasks;
using ConstraintMcpServer.Tests.Framework;
using ConstraintMcpServer.Tests.Steps;
using NUnit.Framework;
using static ConstraintMcpServer.Tests.Framework.ScenarioBuilder;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// BDD-style end-to-end test for context-aware constraint activation.
/// Focuses on business value: LLM agents receive relevant constraints based on development context.
/// Tests the complete MCP pipeline from tool call to constraint activation.
/// </summary>
[TestFixture]
public class ContextAwareConstraintActivationE2E
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
    [Timeout(10000)] // 10-second timeout to prevent hanging
    public async Task Developer_Working_On_TDD_Feature_Receives_Relevant_Constraints()
    {
        // Scenario: Developer working on TDD feature development receives relevant constraints
        // Business value: As a developer practicing TDD, I need the constraint system to recognize
        // my development context (test-first approach, feature development) and automatically
        // activate relevant TDD constraints to guide my coding process, ensuring I follow
        // test-driven development practices consistently

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .And(_steps.ValidConstraintConfigurationExists)
            .And(_steps.StartServerWithConfiguration)
            .And(_steps.ServerLoadsConfigurationSuccessfully)
            .When(_steps.SendMcpToolCallWithTddContext)
            .Then(_steps.ServerActivatesTddConstraints)
            .And(_steps.ResponseContainsTddGuidance)
            .And(_steps.ProcessBehavesPredictably)
            .ExecuteAsync();

        // So that I receive timely guidance to write tests first, follow RED-GREEN-REFACTOR cycle,
        // and maintain good TDD practices throughout my development session
        // (This is achieved by the successful completion of the above steps)
    }

    [Test]
    [Timeout(10000)] // 10-second timeout to prevent hanging
    public async Task Developer_Working_On_Refactoring_Receives_Relevant_Constraints()
    {
        // Scenario: Developer working on code refactoring receives relevant constraints
        // Business value: As a developer refactoring code, I need the constraint system to recognize
        // refactoring context and activate clean code constraints to ensure I improve code quality
        // while maintaining functionality

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .And(_steps.ValidConstraintConfigurationExists)
            .And(_steps.StartServerWithConfiguration)
            .And(_steps.ServerLoadsConfigurationSuccessfully)
            .When(_steps.SendMcpToolCallWithRefactoringContext)
            .Then(_steps.ServerActivatesRefactoringConstraints)
            .And(_steps.ResponseContainsCleanCodeGuidance)
            .And(_steps.ProcessBehavesPredictably)
            .ExecuteAsync();

        // So that I receive appropriate guidance for improving code quality, eliminating
        // duplication, and maintaining clean architecture principles
        // (This is achieved by the successful completion of the above steps)
    }

    [Test]
    [Timeout(10000)] // 10-second timeout to prevent hanging
    public async Task Developer_With_Unclear_Context_Receives_No_Irrelevant_Constraints()
    {
        // Scenario: Developer with unclear development context receives no irrelevant constraints
        // Business value: As a developer, I need the constraint system to avoid activating
        // irrelevant constraints when my development context is unclear or doesn't match
        // specific patterns, preventing constraint overload and maintaining focus

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .And(_steps.ValidConstraintConfigurationExists)
            .And(_steps.StartServerWithConfiguration)
            .And(_steps.ServerLoadsConfigurationSuccessfully)
            .When(_steps.SendMcpToolCallWithUnclearContext)
            .Then(_steps.ServerActivatesNoConstraints)
            .And(_steps.ResponseContainsNoConstraintGuidance)
            .And(_steps.ProcessBehavesPredictably)
            .ExecuteAsync();

        // So that I'm not overwhelmed with irrelevant guidance and can focus on my actual task
        // (This is achieved by the successful completion of the above steps)
    }
}
