using System.Text.Json;

const string ServerName = "ConstraintMcpServer";
const string ServerVersion = "0.1.0";
const string ProductDescription = "Constraint Enforcement MCP Server - keeps LLM coding agents aligned during code generation with composable software-craft constraints";
const string HelpShortFlag = "-h";
const string HelpLongFlag = "--help";
const int DefaultRequestId = 1;
const int ShutdownDelayMs = 500;
const string ContentLengthHeader = "Content-Length:";
const string JsonRpcVersion = "2.0";
const string UnknownMethod = "unknown";

if (args.Length > 0 && (args[0] == HelpLongFlag || args[0] == HelpShortFlag))
{
    await DisplayHelpMessage();
    Environment.Exit(0);
    return;
}

static async Task DisplayHelpMessage()
{
    await Console.Out.WriteLineAsync($"{ServerName} - Constraint Enforcement MCP Server");
    await Console.Out.WriteLineAsync("Usage: dotnet run [--help]");
    await Console.Out.WriteLineAsync("  --help, -h    Show this help message");
    await Console.Out.WriteLineAsync("");
    await Console.Out.WriteLineAsync("This is a walking skeleton implementation. Full MCP protocol support");
    await Console.Out.WriteLineAsync("will be added through test-driven development.");
}

static string AnalyzeContextAndActivateConstraints(JsonDocument request, int requestId)
{
    const string TddConstraintType = "tdd";
    const string RefactoringConstraintType = "refactoring";
    const string TddGuidance = "Write test first before implementation. Follow TDD RED-GREEN-REFACTOR cycle.";
    const string RefactoringGuidance = "Clean and refactor code while maintaining functionality. Focus on improving maintainability.";
    const string DefaultGuidance = "Apply relevant development constraints.";
    const int ActiveConstraintCount = 1;
    const int NoConstraintCount = 0;

    try
    {
        if (!request.RootElement.TryGetProperty("params", out JsonElement paramsElement))
        {
            return $"{{\"jsonrpc\":\"2.0\",\"id\":{requestId},\"result\":{{\"context_analysis\":{{\"has_activation\":false,\"reason\":\"no_params\"}}}}}}";
        }

        string developerContext = paramsElement.TryGetProperty("context", out JsonElement contextElement)
            ? contextElement.GetString() ?? ""
            : "";

        string targetFilePath = paramsElement.TryGetProperty("filePath", out JsonElement filePathElement)
            ? filePathElement.GetString() ?? ""
            : "";

        bool shouldActivateConstraints = false;
        string detectedConstraintType = "";

        if (developerContext.Contains("test-first") || developerContext.Contains("TDD") ||
            targetFilePath.Contains(".test.") || targetFilePath.Contains(".spec."))
        {
            shouldActivateConstraints = true;
            detectedConstraintType = TddConstraintType;
        }
        else if (developerContext.Contains("refactoring") || developerContext.Contains("improve code quality") ||
                 developerContext.Contains("clean code"))
        {
            shouldActivateConstraints = true;
            detectedConstraintType = RefactoringConstraintType;
        }
        else if (developerContext.Contains("unclear") || string.IsNullOrWhiteSpace(developerContext))
        {
            shouldActivateConstraints = false;
        }

        if (shouldActivateConstraints)
        {
            string constraintGuidance = detectedConstraintType switch
            {
                TddConstraintType => TddGuidance,
                RefactoringConstraintType => RefactoringGuidance,
                _ => DefaultGuidance
            };

            return $"{{\"jsonrpc\":\"2.0\",\"id\":{requestId},\"result\":{{\"context_analysis\":{{\"has_activation\":true,\"constraint_type\":\"{detectedConstraintType}\",\"constraint_count\":{ActiveConstraintCount},\"reason\":\"context_match\"}},\"activated_constraints\":[\"{detectedConstraintType}.primary\"],\"guidance\":\"{constraintGuidance}\"}}}}";
        }
        else
        {
            return $"{{\"jsonrpc\":\"2.0\",\"id\":{requestId},\"result\":{{\"context_analysis\":{{\"has_activation\":false,\"constraint_count\":{NoConstraintCount},\"reason\":\"no_context_match\"}}}}}}";
        }
    }
    catch (Exception)
    {
        return $"{{\"jsonrpc\":\"2.0\",\"id\":{requestId},\"result\":{{\"context_analysis\":{{\"has_activation\":false,\"reason\":\"analysis_error\"}}}}}}";
    }
}

static string RouteMethodCall(string requestMethod, JsonDocument parsedRequest, int requestId)
{
    return requestMethod switch
    {
        "initialize" => CreateInitializeResponse(requestId),
        "tools/list" => CreateToolsListResponse(requestId),
        "resources/list" => CreateResourcesListResponse(requestId),
        "tools/call" => AnalyzeContextAndActivateConstraints(parsedRequest, requestId),
        "server.help" => CreateServerHelpResponse(requestId),
        "notifications/constraints" => CreateNotificationResponse(requestId),
        "shutdown" => CreateShutdownResponse(requestId),
        _ => CreateDefaultResponse(requestId)
    };
}

static string CreateJsonRpcResponse(int requestId, string resultContent)
{
    return $"{{\"jsonrpc\":\"{JsonRpcVersion}\",\"id\":{requestId},\"result\":{resultContent}}}";
}

static string CreateInitializeResponse(int requestId)
{
    var capabilities = $"{{\"capabilities\":{{\"tools\":{{}},\"resources\":{{}},\"notifications\":{{\"constraints\":true}}}},\"serverInfo\":{{\"name\":\"{ServerName}\",\"version\":\"{ServerVersion}\"}}}}";
    return CreateJsonRpcResponse(requestId, capabilities);
}

static string CreateToolsListResponse(int requestId)
{
    return CreateJsonRpcResponse(requestId, "{\"tools\":[]}");
}

static string CreateResourcesListResponse(int requestId)
{
    return CreateJsonRpcResponse(requestId, "{\"resources\":[]}");
}

static string CreateServerHelpResponse(int requestId)
{
    var helpResult = $"{{\"product\":\"{ServerName}\",\"description\":\"{ProductDescription}\",\"commands\":[\"server.help\",\"initialize\",\"shutdown\",\"tools/list\",\"tools/call\"]}}";
    return CreateJsonRpcResponse(requestId, helpResult);
}

static string CreateNotificationResponse(int requestId)
{
    return CreateJsonRpcResponse(requestId, "{}");
}

static string CreateShutdownResponse(int requestId)
{
    return CreateJsonRpcResponse(requestId, "{}");
}

static string CreateDefaultResponse(int requestId)
{
    return CreateJsonRpcResponse(requestId, "{}");
}

try
{
    using var inputReader = new StreamReader(Console.OpenStandardInput());
    using var outputWriter = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };

    string? currentLine;
    while ((currentLine = await inputReader.ReadLineAsync()) != null)
    {
        if (currentLine.StartsWith(ContentLengthHeader))
        {
            if (int.TryParse(currentLine.Substring(ContentLengthHeader.Length).Trim(), out int messageContentLength))
            {
                await inputReader.ReadLineAsync();

                var messageBuffer = new char[messageContentLength];
                await inputReader.ReadAsync(messageBuffer, 0, messageContentLength);
                var requestJsonContent = new string(messageBuffer);

                string mcpResponse;
                string requestMethod = UnknownMethod;
                try
                {
                    var parsedRequest = JsonDocument.Parse(requestJsonContent);
                    requestMethod = parsedRequest.RootElement.TryGetProperty("method", out var methodProperty)
                        ? methodProperty.GetString() ?? UnknownMethod
                        : UnknownMethod;

                    var requestId = parsedRequest.RootElement.TryGetProperty("id", out var idProperty)
                        ? idProperty.GetInt32()
                        : DefaultRequestId;

                    mcpResponse = RouteMethodCall(requestMethod, parsedRequest, requestId);
                }
                catch (JsonException)
                {
                    mcpResponse = CreateInitializeResponse(DefaultRequestId);
                }

                await outputWriter.WriteLineAsync($"{ContentLengthHeader} {mcpResponse.Length}");
                await outputWriter.WriteLineAsync();
                await outputWriter.WriteLineAsync(mcpResponse);

                if (requestMethod == "shutdown")
                {
                    await outputWriter.FlushAsync();
                    await Task.Delay(ShutdownDelayMs);
                    break;
                }
            }
        }
    }
}
catch (Exception serverException)
{
    await Console.Error.WriteLineAsync($"Server error: {serverException.Message}");
    Environment.Exit(1);
}
