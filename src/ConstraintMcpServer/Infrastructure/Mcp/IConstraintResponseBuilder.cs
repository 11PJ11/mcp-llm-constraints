namespace ConstraintMcpServer.Infrastructure.Mcp;

/// <summary>
/// Builder interface for creating constraint enforcement responses.
/// </summary>
internal interface IConstraintResponseBuilder
{
    object CreateSuccessResponse(int id, object result);
    object CreateErrorResponse(int id, int code, string message);
}