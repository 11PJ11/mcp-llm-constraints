namespace ConstraintMcpServer.Presentation.Hosting;

/// <summary>
/// Interface for routing constraint enforcement commands to appropriate handlers.
/// </summary>
internal interface IConstraintCommandRouter
{
    Task<object> DispatchAsync(string requestJson);
}
