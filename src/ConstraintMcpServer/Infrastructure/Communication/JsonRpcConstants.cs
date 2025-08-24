namespace ConstraintMcpServer.Infrastructure.Communication;

/// <summary>
/// Centralized constants for JSON-RPC protocol implementation.
/// Eliminates duplication and provides single source of truth for protocol values.
/// </summary>
internal static class JsonRpcConstants
{
    /// <summary>
    /// JSON-RPC protocol version as specified in the standard.
    /// </summary>
    public const string Version = "2.0";

    /// <summary>
    /// Standard error codes as defined in JSON-RPC 2.0 specification.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// Invalid JSON was received by the server (-32700).
        /// </summary>
        public const int ParseError = -32700;

        /// <summary>
        /// The method does not exist / is not available (-32601).
        /// </summary>
        public const int MethodNotFound = -32601;

        /// <summary>
        /// Invalid method parameter(s) (-32602).
        /// </summary>
        public const int InvalidParams = -32602;

        /// <summary>
        /// Internal JSON-RPC error (-32603).
        /// </summary>
        public const int InternalError = -32603;
    }

    /// <summary>
    /// Default values for request processing.
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// Default request ID when none is provided.
        /// </summary>
        public const int RequestId = 1;
    }
}
