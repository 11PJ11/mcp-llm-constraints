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

// Context analysis and constraint activation logic (business logic implementation)
static string AnalyzeContextAndActivateConstraints(JsonDocument request, int id)
{
    try
    {
        // Extract context from MCP tool call parameters
        if (!request.RootElement.TryGetProperty("params", out JsonElement paramsElement))
        {
            return $"{{\"jsonrpc\":\"2.0\",\"id\":{id},\"result\":{{\"context_analysis\":{{\"has_activation\":false,\"reason\":\"no_params\"}}}}}}";
        }

        string context = "";
        string filePath = "";

        if (paramsElement.TryGetProperty("context", out JsonElement contextElement))
        {
            context = contextElement.GetString() ?? "";
        }

        if (paramsElement.TryGetProperty("filePath", out JsonElement filePathElement))
        {
            filePath = filePathElement.GetString() ?? "";
        }

        // Context analysis logic - determine if constraints should be activated
        bool shouldActivate = false;
        string constraintType = "";

        // TDD context detection
        if (context.Contains("test-first") || context.Contains("TDD") || filePath.Contains(".test.") || filePath.Contains(".spec."))
        {
            shouldActivate = true;
            constraintType = "tdd";
        }
        // Refactoring context detection  
        else if (context.Contains("refactoring") || context.Contains("improve code quality") || context.Contains("clean code"))
        {
            shouldActivate = true;
            constraintType = "refactoring";
        }
        // Unclear context - no activation
        else if (context.Contains("unclear") || string.IsNullOrWhiteSpace(context))
        {
            shouldActivate = false;
        }

        // Generate appropriate response based on context analysis
        if (shouldActivate)
        {
            string guidance = constraintType switch
            {
                "tdd" => "Write test first before implementation. Follow TDD RED-GREEN-REFACTOR cycle.",
                "refactoring" => "Clean and refactor code while maintaining functionality. Focus on improving maintainability.",
                _ => "Apply relevant development constraints."
            };

            return $"{{\"jsonrpc\":\"2.0\",\"id\":{id},\"result\":{{\"context_analysis\":{{\"has_activation\":true,\"constraint_type\":\"{constraintType}\",\"constraint_count\":1,\"reason\":\"context_match\"}},\"activated_constraints\":[\"{constraintType}.primary\"],\"guidance\":\"{guidance}\"}}}}";
        }
        else
        {
            return $"{{\"jsonrpc\":\"2.0\",\"id\":{id},\"result\":{{\"context_analysis\":{{\"has_activation\":false,\"constraint_count\":0,\"reason\":\"no_context_match\"}}}}}}";
        }
    }
    catch (Exception)
    {
        // Fallback for context analysis errors
        return $"{{\"jsonrpc\":\"2.0\",\"id\":{id},\"result\":{{\"context_analysis\":{{\"has_activation\":false,\"reason\":\"analysis_error\"}}}}}}";
    }
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

                // Enhanced walking skeleton: Parse request and provide proper MCP protocol responses
                string response;
                string method = "unknown";
                try
                {
                    var request = JsonDocument.Parse(jsonContent);
                    method = request.RootElement.TryGetProperty("method", out var methodProp)
                        ? methodProp.GetString() ?? "unknown"
                        : "unknown";

                    var id = request.RootElement.TryGetProperty("id", out var idProp)
                        ? idProp.GetInt32()
                        : 1;

                    // Basic method routing (walking skeleton with proper MCP compliance)
                    response = method switch
                    {
                        "initialize" => $"{{\"jsonrpc\":\"2.0\",\"id\":{id},\"result\":{{\"capabilities\":{{\"tools\":{{}},\"resources\":{{}},\"notifications\":{{\"constraints\":true}}}},\"serverInfo\":{{\"name\":\"ConstraintMcpServer\",\"version\":\"0.1.0\"}}}}}}",
                        "tools/list" => $"{{\"jsonrpc\":\"2.0\",\"id\":{id},\"result\":{{\"tools\":[]}}}}",
                        "resources/list" => $"{{\"jsonrpc\":\"2.0\",\"id\":{id},\"result\":{{\"resources\":[]}}}}",
                        "tools/call" => AnalyzeContextAndActivateConstraints(request, id),
                        "server.help" => $"{{\"jsonrpc\":\"2.0\",\"id\":{id},\"result\":{{\"product\":\"ConstraintMcpServer\",\"description\":\"Constraint Enforcement MCP Server - keeps LLM coding agents aligned during code generation with composable software-craft constraints\",\"commands\":[\"server.help\",\"initialize\",\"shutdown\",\"tools/list\",\"tools/call\"]}}}}",
                        "notifications/constraints" => $"{{\"jsonrpc\":\"2.0\",\"id\":{id},\"result\":{{}}}}",
                        "shutdown" => $"{{\"jsonrpc\":\"2.0\",\"id\":{id},\"result\":{{}}}}",
                        _ => $"{{\"jsonrpc\":\"2.0\",\"id\":{id},\"result\":{{}}}}"
                    };
                }
                catch (JsonException)
                {
                    // Fallback for malformed JSON (walking skeleton resilience)
                    response = """{"jsonrpc":"2.0","id":1,"result":{"capabilities":{"tools":{},"resources":{},"notifications":{"constraints":true}},"serverInfo":{"name":"ConstraintMcpServer","version":"0.1.0"}}}""";
                }

                await writer.WriteLineAsync($"Content-Length: {response.Length}");
                await writer.WriteLineAsync();
                await writer.WriteLineAsync(response);

                // Handle shutdown gracefully - ensure response is fully sent before exit
                if (method == "shutdown")
                {
                    await writer.FlushAsync();
                    await Task.Delay(100); // Ensure response is fully transmitted
                    break; // Exit the message processing loop
                }
            }
        }
    }
}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync($"Server error: {ex.Message}");
    Environment.Exit(1);
}
