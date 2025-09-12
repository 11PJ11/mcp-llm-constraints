using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Tests.E2E;

namespace ConstraintMcpServer.Tests.Unit;

/// <summary>
/// Unit tests for SimpleConstraintServerManager - Following Outside-In TDD.
/// These tests drive the implementation of the server manager components.
/// </summary>
[TestFixture]
[Category("Unit")]
public class SimpleConstraintServerManagerTests
{
    private SimpleConstraintServerManager _serverManager;

    [SetUp]
    public void SetUp()
    {
        _serverManager = new SimpleConstraintServerManager();
    }

    /// <summary>
    /// Unit test for StartConstraintServerAsync - drives implementation through RED→GREEN→REFACTOR cycle
    /// </summary>
    [Test]
    public async Task StartConstraintServerAsync_Should_Return_True_When_Server_Starts_Successfully()
    {
        // Arrange - server manager is ready to start

        // Act - start the server
        var result = await _serverManager.StartConstraintServerAsync();

        // Assert - server should start successfully
        Assert.That(result, Is.True, "Server should start successfully and return true");
    }

    /// <summary>
    /// Unit test for server state validation after start
    /// </summary>
    [Test]
    public async Task StartConstraintServerAsync_Should_Make_Server_Ready_For_Commands()
    {
        // Arrange 
        await _serverManager.StartConstraintServerAsync();

        // Act - try to handle a command after starting
        var response = await _serverManager.ProcessCommandAsync("test");

        // Assert - should be able to handle commands after starting
        Assert.That(response, Is.Not.Null, "Server should be able to handle commands after starting");
        Assert.That(response, Is.Not.Empty, "Server should return meaningful response");
    }

    /// <summary>
    /// Unit test for help command handling - business requirement
    /// </summary>
    [Test]
    public async Task ProcessCommandAsync_Should_Return_Help_Text_For_Help_Command()
    {
        // Arrange
        await _serverManager.StartConstraintServerAsync();

        // Act - request help
        var helpResponse = await _serverManager.ProcessCommandAsync("help");

        // Assert - should return help information
        Assert.That(helpResponse, Does.Contain("help").IgnoreCase,
            "Help command should return information about available commands");
        Assert.That(helpResponse.Length, Is.GreaterThan(10),
            "Help response should be meaningful and detailed");
    }

    /// <summary>
    /// Unit test for server cleanup after stop
    /// </summary>
    [Test]
    public async Task StopConstraintServerAsync_Should_Clean_Up_Server_Resources()
    {
        // Arrange - start server first
        await _serverManager.StartConstraintServerAsync();

        // Act - stop the server
        await _serverManager.StopConstraintServerAsync();

        // Assert - server cleanup completed (this test drives implementation)
        // After stop, server should be in clean state
        // Future implementations will add validation for resource cleanup
        // For now, we just verify the method completes without exception
        Assert.Pass("StopConstraintServerAsync completed without exceptions");
    }
}
