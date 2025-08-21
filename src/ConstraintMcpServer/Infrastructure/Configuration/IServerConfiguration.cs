namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Interface for server configuration constants.
/// </summary>
internal interface IServerConfiguration
{
    string ProtocolVersion { get; }
    string ServerVersion { get; }
    string ServerName { get; }
}