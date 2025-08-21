namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Factory for creating configured constraint enforcement server instances.
/// Composition root that wires all dependencies following Dependency Inversion Principle.
/// </summary>
internal static class ConstraintServerFactory
{
    /// <summary>
    /// Creates a fully configured constraint enforcement server with all dependencies wired.
    /// </summary>
    public static IMcpServer Create()
    {
        // Create core dependencies
        IServerConfiguration serverConfiguration = new ServerConfiguration();
        IConstraintResponseBuilder responseFactory = new ConstraintResponseBuilder();
        IClientInfoExtractor clientInfoExtractor = new ClientInfoExtractor();
        IMcpCommunicationAdapter protocolHandler = new McpCommunicationAdapter();
        
        // Create request dispatcher with its dependencies
        IConstraintCommandRouter requestDispatcher = new ConstraintCommandRouter(responseFactory, clientInfoExtractor, serverConfiguration);
        
        // Create and return the fully configured server
        return new McpServer(protocolHandler, requestDispatcher);
    }
}