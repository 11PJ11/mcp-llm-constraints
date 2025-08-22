using System.Text.Json;

namespace ConstraintMcpServer.Infrastructure.Communication;

/// <summary>
/// Extracts client information from MCP initialization requests.
/// Single responsibility: Parse client information from JSON-RPC parameters.
/// </summary>
internal sealed class ClientInfoExtractor : IClientInfoExtractor
{
    public ClientInfo ExtractClientInfo(JsonElement root)
    {
        if (!root.TryGetProperty("params", out JsonElement @params) ||
            !@params.TryGetProperty("clientInfo", out JsonElement clientInfoElement))
        {
            return ClientInfo.Unknown;
        }

        string clientName = clientInfoElement.TryGetProperty("name", out JsonElement nameElement)
            ? nameElement.GetString() ?? "unknown"
            : "unknown";

        string clientVersion = clientInfoElement.TryGetProperty("version", out JsonElement versionElement)
            ? versionElement.GetString() ?? "unknown"
            : "unknown";

        return new ClientInfo(clientName, clientVersion);
    }
}
