using System.Diagnostics;
using NUnit.Framework;
using ConstraintMcpServer.Tests.Steps;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// Tests that reproduce the exact E2E timeout/hanging bug in ConfigLoadE2E.
/// These tests demonstrate the specific issue causing 5+ minute hangs.
/// 
/// RED PHASE: These tests should TIMEOUT/HANG when the bug is present
/// GREEN PHASE: These tests should PASS quickly when timeout fixes are implemented
/// </summary>
[TestFixture]
public class E2ETimeoutBugReproductionTests
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
        try
        {
            _steps?.Dispose();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Warning: Test cleanup encountered error: {ex.Message}");
        }
        finally
        {
            _steps = null;
        }
    }

    [Test]
    [Timeout(10000)] // 10 seconds max - should not hang for 5+ minutes
    public async Task ConfigLoadE2E_ShouldTimeout_InsteadOfHangingForMinutes()
    {
        // Arrange - Reproduce exact scenario from Constraint_Server_Loads_Valid_Configuration_Successfully
        // This test should demonstrate the hanging behavior and complete within reasonable time once fixed

        var testStopwatch = Stopwatch.StartNew();

        try
        {
            // Act - Execute the exact same steps that cause the hang
            // These are the steps from ConfigLoadE2E.cs that cause 5+ minute hangs

            _steps!.RepositoryBuildsSuccessfully();
            _steps.ValidConstraintConfigurationExists();
            _steps.StartServerWithConfiguration(); // <- This is where the problem often starts

            // This is the step that often hangs waiting for server response
            _steps.ServerLoadsConfigurationSuccessfully(); // <- Hangs in ValidateProcessState

            // These steps involve MCP communication that can hang indefinitely
            await _steps.ServerAdvertisesConstraintCapabilities(); // <- Hangs in ReadJsonRpcResponse
            _steps.ProcessBehavesPredictably(); // <- May hang in ValidateProcessState

            testStopwatch.Stop();

            // Assert - Should complete within reasonable time (currently hangs for 5+ minutes)
            Assert.That(testStopwatch.ElapsedMilliseconds, Is.LessThan(8000),
                $"E2E test should complete within 8 seconds, not hang for minutes. Actual: {testStopwatch.ElapsedMilliseconds}ms");

            System.Console.WriteLine($"✅ Test completed successfully in {testStopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            testStopwatch.Stop();

            // Even if test fails, it should fail quickly, not hang
            Assert.That(testStopwatch.ElapsedMilliseconds, Is.LessThan(8000),
                $"Even failed E2E test should complete within 8 seconds. Actual: {testStopwatch.ElapsedMilliseconds}ms. Error: {ex.Message}");

            // Re-throw for proper test failure reporting
            throw;
        }
    }

    [Test]
    [Timeout(5000)] // 5 seconds max 
    public void ServerStartup_ShouldTimeout_WhenProcessDoesntStart()
    {
        // Arrange - Test the specific server startup process that can hang
        var startupStopwatch = Stopwatch.StartNew();

        try
        {
            // Act - Try to start server with invalid path (this should fail quickly)
            // This tests the process startup timeout behavior in StartServerWithConfiguration

            _steps!.ValidConstraintConfigurationExists();

            // Attempt server start that should timeout if process doesn't respond
            // In the actual bug, this can hang if the dotnet process doesn't start properly
            _steps.StartServerWithConfiguration();

            startupStopwatch.Stop();

            // Verify server is actually running or failed quickly
            bool serverResponding = true;
            try
            {
                _steps.ServerLoadsConfigurationSuccessfully();
            }
            catch
            {
                serverResponding = false;
            }

            // Assert - Should either start quickly or fail quickly, not hang
            if (serverResponding)
            {
                Assert.That(startupStopwatch.ElapsedMilliseconds, Is.LessThan(3000),
                    $"Server should start within 3 seconds when successful. Actual: {startupStopwatch.ElapsedMilliseconds}ms");
            }
            else
            {
                Assert.That(startupStopwatch.ElapsedMilliseconds, Is.LessThan(2000),
                    $"Server should fail within 2 seconds when unsuccessful. Actual: {startupStopwatch.ElapsedMilliseconds}ms");
            }
        }
        catch (Exception)
        {
            startupStopwatch.Stop();

            // Failures should happen quickly
            Assert.That(startupStopwatch.ElapsedMilliseconds, Is.LessThan(2000),
                $"Server startup failure should be detected quickly. Actual: {startupStopwatch.ElapsedMilliseconds}ms");

            // Expected to fail in this test - we're testing timeout behavior
        }
    }

    [Test]
    [Timeout(5000)] // 5 seconds max
    public async Task McpCommunication_ShouldTimeout_WhenServerDoesntRespond()
    {
        // Arrange - Test MCP communication timeout (this is where the main hang occurs)
        var communicationStopwatch = Stopwatch.StartNew();

        try
        {
            _steps!.ValidConstraintConfigurationExists();
            _steps.StartServerWithConfiguration();

            // Give server time to start
            await Task.Delay(1000);

            // Act - Attempt MCP communication that could hang
            // This is the exact code path that hangs in ServerAdvertisesConstraintCapabilities
            await _steps.ServerAdvertisesConstraintCapabilities();

            communicationStopwatch.Stop();

            // Assert - Communication should complete within reasonable time
            Assert.That(communicationStopwatch.ElapsedMilliseconds, Is.LessThan(4000),
                $"MCP communication should complete within 4 seconds. Actual: {communicationStopwatch.ElapsedMilliseconds}ms");

        }
        catch (Exception ex)
        {
            communicationStopwatch.Stop();

            // Even communication failures should be fast
            Assert.That(communicationStopwatch.ElapsedMilliseconds, Is.LessThan(3000),
                $"MCP communication failure should be detected quickly. Actual: {communicationStopwatch.ElapsedMilliseconds}ms. Error: {ex.Message}");
        }
    }

    [Test]
    [Timeout(3000)] // 3 seconds max
    public void ProcessValidation_ShouldTimeout_WhenValidatingNonResponsiveProcess()
    {
        // Arrange - Test the ValidateProcessState method that can hang
        var validationStopwatch = Stopwatch.StartNew();

        try
        {
            _steps!.ValidConstraintConfigurationExists();
            _steps.StartServerWithConfiguration();

            // Act - This calls ValidateProcessState which can hang waiting for process response
            _steps.ServerLoadsConfigurationSuccessfully();

            validationStopwatch.Stop();

            // Assert - Process validation should be fast
            Assert.That(validationStopwatch.ElapsedMilliseconds, Is.LessThan(2000),
                $"Process validation should complete within 2 seconds. Actual: {validationStopwatch.ElapsedMilliseconds}ms");

        }
        catch (Exception ex)
        {
            validationStopwatch.Stop();

            // Process validation failures should be fast
            Assert.That(validationStopwatch.ElapsedMilliseconds, Is.LessThan(1500),
                $"Process validation should fail quickly if server not ready. Actual: {validationStopwatch.ElapsedMilliseconds}ms. Error: {ex.Message}");
        }
    }

    [Test]
    [Timeout(4000)] // 4 seconds max
    public void MultipleStepSequence_ShouldTimeout_WhenAnyStepHangs()
    {
        // Arrange - Test the complete sequence from ConfigLoadE2E that causes multi-minute hangs
        var sequenceStopwatch = Stopwatch.StartNew();
        var completedSteps = new List<string>();

        try
        {
            // Act - Execute each step with individual tracking to identify where hang occurs

            completedSteps.Add("Starting RepositoryBuildsSuccessfully");
            _steps!.RepositoryBuildsSuccessfully();
            completedSteps.Add("Completed RepositoryBuildsSuccessfully");

            completedSteps.Add("Starting ValidConstraintConfigurationExists");
            _steps.ValidConstraintConfigurationExists();
            completedSteps.Add("Completed ValidConstraintConfigurationExists");

            completedSteps.Add("Starting StartServerWithConfiguration");
            _steps.StartServerWithConfiguration();
            completedSteps.Add("Completed StartServerWithConfiguration");

            completedSteps.Add("Starting ServerLoadsConfigurationSuccessfully");
            _steps.ServerLoadsConfigurationSuccessfully();
            completedSteps.Add("Completed ServerLoadsConfigurationSuccessfully");

            sequenceStopwatch.Stop();

            // Assert - Each step should complete reasonably quickly
            Assert.That(sequenceStopwatch.ElapsedMilliseconds, Is.LessThan(3500),
                $"Complete step sequence should finish within 3.5 seconds. Actual: {sequenceStopwatch.ElapsedMilliseconds}ms. Completed steps: {string.Join(" -> ", completedSteps)}");

        }
        catch (Exception ex)
        {
            sequenceStopwatch.Stop();

            // Log which steps completed to help identify where hang occurs
            System.Console.WriteLine($"Steps completed before failure ({sequenceStopwatch.ElapsedMilliseconds}ms): {string.Join(" -> ", completedSteps)}");

            // Even with failures, should not take more than reasonable time
            Assert.That(sequenceStopwatch.ElapsedMilliseconds, Is.LessThan(3000),
                $"Step sequence should fail quickly if there are issues. Actual: {sequenceStopwatch.ElapsedMilliseconds}ms. Error: {ex.Message}");
        }
    }
}
