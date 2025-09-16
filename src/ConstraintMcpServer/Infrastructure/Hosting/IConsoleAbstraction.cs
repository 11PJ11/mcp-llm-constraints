namespace ConstraintMcpServer.Infrastructure.Hosting;

/// <summary>
/// Abstracts console I/O operations for testability and hosting flexibility.
/// </summary>
public interface IConsoleAbstraction
{
    /// <summary>
    /// Writes a line to the output stream.
    /// </summary>
    /// <param name="value">The value to write.</param>
    void WriteLine(string value);

    /// <summary>
    /// Writes text to the output stream without a newline.
    /// </summary>
    /// <param name="value">The value to write.</param>
    void Write(string value);

    /// <summary>
    /// Reads a line from the input stream.
    /// </summary>
    /// <returns>The line read from input.</returns>
    string? ReadLine();
}