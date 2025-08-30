using ConstraintMcpServer.Presentation.Hosting;

// Create cancellation token for graceful shutdown
using var cts = new CancellationTokenSource();
bool isShuttingDown = false;

// Handle SIGINT (Ctrl+C) and SIGTERM for graceful shutdown
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true; // Don't terminate immediately
    if (!isShuttingDown)
    {
        isShuttingDown = true;
        cts.Cancel();    // Signal graceful shutdown
        Console.Error.WriteLine("Graceful shutdown initiated...");
    }
};

// Handle application domain shutdown (SIGTERM on Unix)
AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
{
    if (!isShuttingDown)
    {
        isShuttingDown = true;
        try
        {
            cts.Cancel();
            Console.Error.WriteLine("Process exit requested, shutting down...");
        }
        catch (ObjectDisposedException)
        {
            // CTS already disposed, ignore
        }
    }
};

try
{
    // Start the constraint enforcement server to keep LLM coding agents aligned
    // Pure MCP server that communicates via JSON-RPC over stdin/stdout
    // Configuration loading will be handled via MCP protocol messages
    await ConstraintServerHost.ProcessRequestsAsync(cts.Token);

    Console.Error.WriteLine("Server shutdown completed");
}
catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
{
    // Expected shutdown - don't log as error
    Console.Error.WriteLine("Server shutdown gracefully");
}
catch (Exception ex)
{
    // Log all errors to stderr to avoid interfering with stdout JSON-RPC communication
    await Console.Error.WriteLineAsync($"Server error: {ex.Message}");
    Environment.Exit(1);
}
