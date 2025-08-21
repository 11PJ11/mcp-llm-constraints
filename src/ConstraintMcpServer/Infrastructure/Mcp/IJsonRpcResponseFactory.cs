namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Factory interface for creating JSON-RPC responses.
/// </summary>
internal interface IJsonRpcResponseFactory
{
    object CreateSuccessResponse(int id, object result);
    object CreateErrorResponse(int id, int code, string message);
}