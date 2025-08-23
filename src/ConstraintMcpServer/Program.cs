using ConstraintMcpServer.Presentation.Hosting;

try
{
    // Start the constraint enforcement server to keep LLM coding agents aligned
    // Pure MCP server that communicates via JSON-RPC over stdin/stdout
    // Configuration loading will be handled via MCP protocol messages
    await ConstraintServerHost.ProcessRequestsAsync();
}
catch (Exception ex)
{
    // Log all errors to stderr to avoid interfering with stdout JSON-RPC communication
    await Console.Error.WriteLineAsync($"Server error: {ex.Message}");
    Environment.Exit(1);
}
