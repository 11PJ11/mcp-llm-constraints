using ConstraintMcpServer.Infrastructure.Communication;

namespace ConstraintMcpServer.Presentation.Hosting;

/// <summary>
/// Builder for creating constraint enforcement responses.
/// </summary>
internal sealed class ConstraintResponseBuilder : IConstraintResponseBuilder
{

    public object CreateSuccessResponse(int id, object result)
    {
        return new
        {
            jsonrpc = JsonRpcConstants.Version,
            id,
            result
        };
    }

    public object CreateErrorResponse(int id, int code, string message)
    {
        return new
        {
            jsonrpc = JsonRpcConstants.Version,
            id,
            error = new
            {
                code,
                message
            }
        };
    }
}
