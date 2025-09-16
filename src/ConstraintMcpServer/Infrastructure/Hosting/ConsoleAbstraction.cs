namespace ConstraintMcpServer.Infrastructure.Hosting;

/// <summary>
/// Default implementation of console I/O abstraction using System.Console.
/// </summary>
internal sealed class ConsoleAbstraction : IConsoleAbstraction
{
    /// <inheritdoc />
    public void WriteLine(string value) => Console.WriteLine(value);

    /// <inheritdoc />
    public void Write(string value) => Console.Write(value);

    /// <inheritdoc />
    public string? ReadLine() => Console.ReadLine();
}