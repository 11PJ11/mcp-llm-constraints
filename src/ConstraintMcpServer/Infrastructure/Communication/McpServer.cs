using ConstraintMcpServer.Presentation.Hosting;

namespace ConstraintMcpServer.Infrastructure.Communication;

/// <summary>
/// MCP server implementation that processes JSON-RPC requests over stdio.
/// Depends on abstractions only, following Dependency Inversion Principle.
/// </summary>
internal sealed class McpServer : IMcpServer
{
    private readonly IMcpCommunicationAdapter _protocolHandler;
    private readonly IConstraintCommandRouter _requestDispatcher;

    public McpServer(IMcpCommunicationAdapter protocolHandler, IConstraintCommandRouter requestDispatcher)
    {
        _protocolHandler = protocolHandler;
        _requestDispatcher = requestDispatcher;
    }

    public async Task ProcessRequestsAsync(CancellationToken cancellationToken = default)
    {
        using Stream stdin = Console.OpenStandardInput();
        using Stream stdout = Console.OpenStandardOutput();
        using var reader = new StreamReader(stdin);
        using var writer = new StreamWriter(stdout);

        // Use a timeout to prevent infinite hanging if no requests come in
        var timeout = TimeSpan.FromMinutes(30); // 30-minute idle timeout
        var consecutiveNullReads = 0;
        const int maxConsecutiveNullReads = 3; // If we get 3 null reads in a row, assume stdin is closed

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Read with timeout to prevent infinite blocking
                using var readCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                readCts.CancelAfter(timeout);

                string? requestJson = await _protocolHandler.ReadRequestAsync(reader, TimeSpan.FromSeconds(30));

                if (requestJson == null)
                {
                    consecutiveNullReads++;

                    // If we're getting consecutive null reads, stdin might be closed
                    if (consecutiveNullReads >= maxConsecutiveNullReads)
                    {
                        Console.Error.WriteLine("Detected stdin closure after consecutive null reads, terminating...");
                        break;
                    }

                    // Brief pause to prevent CPU spinning on null reads
                    await Task.Delay(100, cancellationToken);
                    continue;
                }

                // Reset counter on successful read
                consecutiveNullReads = 0;

                object response = await _requestDispatcher.DispatchAsync(requestJson);
                await _protocolHandler.WriteResponseAsync(writer, response);

                // Ensure response is immediately sent
                await writer.FlushAsync();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Expected cancellation - exit gracefully
                Console.Error.WriteLine("Server shutdown requested");
                break;
            }
            catch (Exception ex) when (IsStdinClosedException(ex))
            {
                // Stdin was closed - normal termination for MCP servers
                Console.Error.WriteLine("Stdin closed, terminating server");
                break;
            }
            catch (Exception ex)
            {
                // Log unexpected errors but continue processing
                Console.Error.WriteLine($"Error processing request: {ex.Message}");

                // If we're getting too many errors, terminate to prevent resource leaks
                await Task.Delay(1000, cancellationToken);
            }
        }

        Console.Error.WriteLine("Request processing loop terminated");
    }

    private static bool IsStdinClosedException(Exception ex)
    {
        // Check for common exceptions that indicate stdin closure
        return ex is ObjectDisposedException ||
               ex is InvalidOperationException ||
               (ex is IOException && ex.Message.Contains("pipe", StringComparison.OrdinalIgnoreCase));
    }
}
