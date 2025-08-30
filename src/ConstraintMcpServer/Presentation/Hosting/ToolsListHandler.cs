using System.Text.Json;

namespace ConstraintMcpServer.Presentation.Hosting;

/// <summary>
/// Handles MCP tools/list requests to provide available tool information.
/// Returns the list of tools supported by this constraint enforcement server.
/// </summary>
internal sealed class ToolsListHandler : IMcpCommandHandler
{
    /// <summary>
    /// Handles a tools/list MCP request.
    /// </summary>
    /// <param name="requestId">The JSON-RPC request ID.</param>
    /// <param name="request">The JSON-RPC request element.</param>
    /// <returns>JSON-RPC response with list of available tools.</returns>
    public async Task<object> HandleAsync(int requestId, JsonElement request)
    {
        // Satisfy async requirement
        await Task.CompletedTask;

        return new
        {
            jsonrpc = "2.0",
            id = requestId,
            result = new
            {
                tools = new[]
                {
                    new
                    {
                        name = "constraint-enforcement",
                        description = "Enforce software development constraints during LLM code generation",
                        inputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                context = new
                                {
                                    type = "string",
                                    description = "Development context for constraint selection"
                                },
                                filePath = new
                                {
                                    type = "string",
                                    description = "File being worked on"
                                },
                                sessionId = new
                                {
                                    type = "string",
                                    description = "Development session identifier"
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}
