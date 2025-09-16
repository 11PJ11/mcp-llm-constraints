using ConstraintMcpServer.Infrastructure.Hosting;
using NSubstitute;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Infrastructure.Hosting;

[TestFixture]
public sealed class McpServerHostTests
{
    private IConsoleAbstraction _console = null!;
    private McpServerHost _host = null!;

    [SetUp]
    public void SetUp()
    {
        _console = Substitute.For<IConsoleAbstraction>();
        _host = new McpServerHost(_console);
    }

    [Test]
    public async Task RunAsync_WithHelpFlag_DisplaysHelpMessage()
    {
        // Arrange
        var args = new[] { "--help" };

        // Act
        var exitCode = await _host.RunAsync(args);

        // Assert
        Assert.That(exitCode, Is.EqualTo(0));
        _console.Received().WriteLine(Arg.Is<string>(s => s.Contains("Constraint MCP Server")));
        _console.Received().WriteLine("Usage: dotnet run [--help|--version]");
    }

    [Test]
    public async Task RunAsync_WithShortHelpFlag_DisplaysHelpMessage()
    {
        // Arrange
        var args = new[] { "-h" };

        // Act
        var exitCode = await _host.RunAsync(args);

        // Assert
        Assert.That(exitCode, Is.EqualTo(0));
        _console.Received().WriteLine(Arg.Is<string>(s => s.Contains("Constraint MCP Server")));
    }

    [Test]
    public async Task RunAsync_WithVersionFlag_DisplaysVersionMessage()
    {
        // Arrange
        var args = new[] { "--version" };

        // Act
        var exitCode = await _host.RunAsync(args);

        // Assert
        Assert.That(exitCode, Is.EqualTo(0));
        _console.Received().WriteLine("Constraint MCP Server 0.1.0");
    }

    [Test]
    public async Task RunAsync_WithShortVersionFlag_DisplaysVersionMessage()
    {
        // Arrange
        var args = new[] { "-v" };

        // Act
        var exitCode = await _host.RunAsync(args);

        // Assert
        Assert.That(exitCode, Is.EqualTo(0));
        _console.Received().WriteLine("Constraint MCP Server 0.1.0");
    }

    [Test]
    public async Task RunAsync_WithNoArgs_ReturnsSuccessfully()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var exitCode = await _host.RunAsync(args);

        // Assert
        Assert.That(exitCode, Is.EqualTo(0));
    }
}
