using System.Text.Json;

// Simple walking skeleton MCP server - minimal implementation for E2E test compatibility
// Full implementation will be added when driven by failing acceptance tests per TDD principles

// Check if help is requested
if (args.Length > 0 && (args[0] == "--help" || args[0] == "-h"))
{
    await Console.Out.WriteLineAsync("ConstraintMcpServer - Constraint Enforcement MCP Server");
    await Console.Out.WriteLineAsync("Usage: dotnet run [--help]");
    await Console.Out.WriteLineAsync("  --help, -h    Show this help message");
    await Console.Out.WriteLineAsync("");
    await Console.Out.WriteLineAsync("This is a walking skeleton implementation. Full MCP protocol support");
    await Console.Out.WriteLineAsync("will be added through test-driven development.");
    Environment.Exit(0);
    return;
}

// Basic MCP protocol handler over stdin/stdout
// This minimal implementation supports the E2E tests while maintaining TDD discipline
try
{
    using var reader = new StreamReader(Console.OpenStandardInput());
    using var writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };

    // Simple message loop for basic MCP protocol support
    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
        if (line.StartsWith("Content-Length:"))
        {
            // Parse Content-Length header
            if (int.TryParse(line.Substring("Content-Length:".Length).Trim(), out int contentLength))
            {
                // Read empty line
                await reader.ReadLineAsync();

                // Read JSON content
                var buffer = new char[contentLength];
                await reader.ReadAsync(buffer, 0, contentLength);
                var jsonContent = new string(buffer);

                // Basic JSON-RPC response (walking skeleton)
                var response = """{"jsonrpc":"2.0","id":1,"result":{"capabilities":{"tools":{}},"serverInfo":{"name":"ConstraintMcpServer","version":"0.1.0"}}}""";

                await writer.WriteLineAsync($"Content-Length: {response.Length}");
                await writer.WriteLineAsync();
                await writer.WriteLineAsync(response);
            }
        }
    }
}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync($"Server error: {ex.Message}");
    Environment.Exit(1);
}
