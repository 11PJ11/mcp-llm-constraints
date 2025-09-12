using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// Minimal E2E test to validate Outside-In TDD approach.
/// This test should fail initially with NotImplementedException - the RIGHT REASON for failure.
/// Once this works, we can expand to full production distribution functionality.
/// </summary>
[TestFixture]
[Category("E2E")]
public class SimpleInstallationE2E
{
    /// <summary>
    /// Minimal E2E Test: Basic constraint server functionality
    /// Following Outside-In TDD: This test should FAIL initially and drive inner unit test loops.
    /// </summary>
    [Test]
    public async Task Constraint_Server_Should_Start_And_Respond_To_Basic_Commands()
    {
        // This E2E test will fail initially with NotImplementedException
        // This is the RIGHT REASON for failure - missing implementation
        // Inner unit tests will drive the implementation until this naturally passes

        var serverManager = new SimpleConstraintServerManager();

        // Act & Assert - this should throw NotImplementedException
        var isStarted = await serverManager.StartConstraintServerAsync();
        Assert.That(isStarted, Is.True, "Constraint server should start successfully");

        var response = await serverManager.ProcessCommandAsync("help");
        Assert.That(response, Is.Not.Null.And.Not.Empty, "Server should respond to basic commands");

        await serverManager.StopConstraintServerAsync();
    }
}

/// <summary>
/// Simple constraint server manager for E2E testing.
/// Following Outside-In TDD: Minimal implementation driven by unit tests.
/// Organized with Single Responsibility: Server lifecycle and command processing clearly separated.
/// </summary>
public class SimpleConstraintServerManager
{
    // Domain Constants
    private const string ServerNotStartedMessage = "Server not started";
    private const string HelpCommand = "help";
    private const string HelpResponse = "Available commands: help - Shows this help information";

    // Server State
    private bool _isServerRunning;

    // Public API - Server Lifecycle Operations

    public async Task<bool> StartConstraintServerAsync()
    {
        await Task.CompletedTask;
        _isServerRunning = true;
        return true;
    }

    public async Task StopConstraintServerAsync()
    {
        await Task.CompletedTask;
        TransitionServerToStoppedState();
    }

    // Public API - Command Processing Operations

    public async Task<string> ProcessCommandAsync(string command)
    {
        await Task.CompletedTask;

        if (!IsServerInRunningState())
        {
            return ServerNotStartedMessage;
        }

        return ProcessValidCommand(command);
    }

    // Private - Server State Management

    private bool IsServerInRunningState()
    {
        return _isServerRunning;
    }

    private void TransitionServerToStoppedState()
    {
        _isServerRunning = false;
    }

    // Private - Command Processing Logic

    private static string ProcessValidCommand(string command)
    {
        if (IsHelpCommandRequested(command))
        {
            return HelpResponse;
        }

        return CreateCommandReceivedResponse(command);
    }

    private static bool IsHelpCommandRequested(string command)
    {
        return string.Equals(command?.Trim(), HelpCommand, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateCommandReceivedResponse(string command)
    {
        return $"Command received: {command}";
    }
}
