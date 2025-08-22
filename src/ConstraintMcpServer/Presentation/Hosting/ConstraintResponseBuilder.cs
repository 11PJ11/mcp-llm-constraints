namespace ConstraintMcpServer.Presentation.Hosting;

/// <summary>
/// Builder for creating constraint enforcement responses.
/// </summary>
internal sealed class ConstraintResponseBuilder : IConstraintResponseBuilder
{
    private const string JsonRpcVersion = "2.0";

    public object CreateSuccessResponse(int id, object result)
    {
        return new
        {
            jsonrpc = JsonRpcVersion,
            id,
            result
        };
    }

    public object CreateErrorResponse(int id, int code, string message)
    {
        return new
        {
            jsonrpc = JsonRpcVersion,
            id,
            error = new
            {
                code,
                message
            }
        };
    }
}
