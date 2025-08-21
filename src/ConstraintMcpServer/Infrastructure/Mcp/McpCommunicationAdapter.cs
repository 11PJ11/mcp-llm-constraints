using System.Text;
using System.Text.Json;

namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Handles MCP communication protocol reading and writing over stdio streams.
/// Single responsibility: MCP protocol serialization/deserialization.
/// </summary>
internal sealed class McpCommunicationAdapter : IMcpCommunicationAdapter
{
    public async Task<string?> ReadRequestAsync(StreamReader reader)
    {
        int? contentLength = await ReadContentLengthHeader(reader);
        if (contentLength == null)
        {
            return null;
        }

        await reader.ReadLineAsync(); // Read blank line

        return await ReadJsonContent(reader, contentLength.Value);
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

    private static async Task<int?> ReadContentLengthHeader(StreamReader reader)
    {
        string? headerLine = await reader.ReadLineAsync();
        if (headerLine == null)
        {
            return null;
        }

        if (!headerLine.StartsWith("Content-Length:"))
        {
            return null;
        }

        if (!int.TryParse(headerLine.Substring("Content-Length:".Length).Trim(), out int contentLength))
        {
            return null;
        }

        return contentLength;
    }

    private static async Task<string> ReadJsonContent(StreamReader reader, int contentLength)
    {
        char[] buffer = new char[contentLength];
        int totalRead = 0;
        while (totalRead < contentLength)
        {
            int read = await reader.ReadAsync(buffer, totalRead, contentLength - totalRead);
            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        return new string(buffer, 0, totalRead).Trim();
    }
}