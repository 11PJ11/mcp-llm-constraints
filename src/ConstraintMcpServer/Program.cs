using ConstraintMcpServer.Infrastructure.Mcp;

try
{
    // For the walking skeleton (Step 1), use simple JSON-RPC handler for server.help
    // This will be replaced with full MCP SDK integration in later steps
    await JsonRpcStdioHandler.ProcessRequestsAsync();
}
catch (Exception ex)
{
    // Log to stderr to avoid interfering with stdout JSON-RPC communication
    await Console.Error.WriteLineAsync($"Server startup error: {ex.Message}");
    Environment.Exit(1);
}
