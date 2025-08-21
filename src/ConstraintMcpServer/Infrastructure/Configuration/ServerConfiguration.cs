namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Default server configuration implementation.
/// </summary>
internal sealed class ServerConfiguration : IServerConfiguration
{
    public string ProtocolVersion => "2024-11-05";
    public string ServerVersion => "0.1.0";
    public string ServerName => "Constraint Enforcement MCP Server";
}