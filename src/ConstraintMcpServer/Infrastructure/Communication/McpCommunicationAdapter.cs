using System.Text;
using System.Text.Json;

namespace ConstraintMcpServer.Infrastructure.Communication;

/// <summary>
/// Handles MCP communication protocol reading and writing over stdio streams.
/// Single responsibility: MCP protocol serialization/deserialization.
/// </summary>
internal sealed class McpCommunicationAdapter : IMcpCommunicationAdapter
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    public async Task<string?> ReadRequestAsync(StreamReader reader)
    {
        return await ReadRequestAsync(reader, DefaultTimeout);
    }

    public async Task<string?> ReadRequestAsync(StreamReader reader, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);

        try
        {
            int? contentLength = await ReadContentLengthHeaderWithTimeout(reader, cts.Token);
            if (contentLength == null)
            {
                return null;
            }

            await ReadBlankLineWithTimeout(reader, cts.Token);

            return await ReadJsonContentWithTimeout(reader, contentLength.Value, cts.Token);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            // Timeout occurred - return null to indicate no request available
            return null;
        }
    }

    public async Task WriteResponseAsync(StreamWriter writer, object response)
    {
        string json = JsonSerializer.Serialize(response);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        await writer.WriteLineAsync($"Content-Length: {bytes.Length}");
        await writer.WriteLineAsync();
        await writer.WriteAsync(json);
        await writer.FlushAsync();
    }

    private static async Task<int?> ReadContentLengthHeaderWithTimeout(StreamReader reader, CancellationToken cancellationToken)
    {
        var readTask = reader.ReadLineAsync();
        var timeoutTask = Task.Delay(Timeout.Infinite, cancellationToken);

        var completedTask = await Task.WhenAny(readTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        string? headerLine = await readTask;

        if (headerLine == null || !headerLine.StartsWith("Content-Length:"))
        {
            return null;
        }

        int.TryParse(headerLine.Substring("Content-Length:".Length).Trim(), out int contentLength);

        return contentLength;
    }

    private static async Task ReadBlankLineWithTimeout(StreamReader reader, CancellationToken cancellationToken)
    {
        var readTask = reader.ReadLineAsync();
        var timeoutTask = Task.Delay(Timeout.Infinite, cancellationToken);

        var completedTask = await Task.WhenAny(readTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        await readTask; // Read blank line
    }

    private static async Task<string> ReadJsonContentWithTimeout(StreamReader reader, int contentLength, CancellationToken cancellationToken)
    {
        char[] buffer = new char[contentLength];
        int totalRead = 0;

        while (totalRead < contentLength)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var readTask = reader.ReadAsync(buffer, totalRead, contentLength - totalRead);
            var timeoutTask = Task.Delay(Timeout.Infinite, cancellationToken);

            var completedTask = await Task.WhenAny(readTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            int read = await readTask;
            if (read == 0)
            {
                break; // No more data available - handle gracefully
            }

            totalRead += read;
        }

        return new string(buffer, 0, totalRead).Trim();
    }
}
