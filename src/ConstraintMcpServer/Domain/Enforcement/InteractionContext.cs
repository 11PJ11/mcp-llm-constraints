namespace ConstraintMcpServer.Domain.Enforcement;

/// <summary>
/// Domain value object representing the context of an MCP interaction.
/// Contains session state, timing, and metadata for constraint enforcement decisions.
/// </summary>
internal sealed record InteractionContext
{
    /// <summary>
    /// Unique identifier for the current session.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Sequential interaction number within the session.
    /// </summary>
    public required int InteractionNumber { get; init; }

    /// <summary>
    /// Timestamp when the interaction started.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// MCP method being called (e.g., "tools/call", "resources/read").
    /// </summary>
    public required string Method { get; init; }

    /// <summary>
    /// Optional tool name if this is a tool call.
    /// </summary>
    public string? ToolName { get; init; }

    /// <summary>
    /// Client information from MCP initialization.
    /// </summary>
    public ClientContext? Client { get; init; }

    /// <summary>
    /// Current development phase (e.g., "red", "green", "refactor").
    /// </summary>
    public string? CurrentPhase { get; init; }

    /// <summary>
    /// Number of interactions since last constraint injection.
    /// </summary>
    public int InteractionsSinceLastInjection { get; init; }

    /// <summary>
    /// Optional metadata for context-specific processing.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Domain value object representing client context information.
/// </summary>
internal sealed record ClientContext
{
    /// <summary>
    /// Client name (e.g., "Claude Desktop", "VSCode Extension").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Client version string.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Optional client capabilities or feature flags.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Capabilities { get; init; }
}