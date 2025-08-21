namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Factory for creating configured MCP server instances.
/// Composition root that wires all dependencies following Dependency Inversion Principle.
/// </summary>
internal static class McpServerFactory
{
    /// <summary>
    /// Creates a fully configured MCP server with all dependencies wired.
    /// </summary>
    public static IMcpServer Create()
    {
        // Create core dependencies
        IServerConfiguration serverConfiguration = new ServerConfiguration();
        IConstraintResponseBuilder responseFactory = new ConstraintResponseBuilder();
        IClientInfoExtractor clientInfoExtractor = new ClientInfoExtractor();
        IJsonRpcProtocolHandler protocolHandler = new JsonRpcProtocolHandler();
        
        // Create request dispatcher with its dependencies
        IRequestDispatcher requestDispatcher = new McpRequestDispatcher(responseFactory, clientInfoExtractor, serverConfiguration);
        
        // Create and return the fully configured server
        return new McpServer(protocolHandler, requestDispatcher);
    }
}