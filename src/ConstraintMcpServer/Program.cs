using ConstraintMcpServer.Presentation.Hosting;

try
{
    // Start the constraint enforcement server to keep LLM coding agents aligned
    // Provides MCP protocol support for constraint injection and model drift prevention
    await ConstraintServerHost.ProcessRequestsAsync();
}
catch (Exception ex)
{
    // Log to stderr to avoid interfering with stdout JSON-RPC communication
    await Console.Error.WriteLineAsync($"Server startup error: {ex.Message}");
    Environment.Exit(1);
}
