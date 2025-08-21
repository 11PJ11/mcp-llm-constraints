namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Domain aggregate root representing a collection of related constraints.
/// Contains metadata and constraint definitions for a specific domain or methodology.
/// </summary>
internal sealed record ConstraintPack
{
    /// <summary>
    /// Version of the constraint pack format.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Optional metadata about the constraint pack (name, description, author).
    /// </summary>
    public PackMetadata? Metadata { get; init; }

    /// <summary>
    /// Collection of constraint definitions in this pack.
    /// </summary>
    public required IReadOnlyList<Constraint> Constraints { get; init; }

    /// <summary>
    /// Optional default configuration for constraint selection and injection.
    /// </summary>
    public PackConfiguration? Configuration { get; init; }
}

/// <summary>
/// Metadata information about a constraint pack.
/// </summary>
internal sealed record PackMetadata
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Author { get; init; }
    public string? Version { get; init; }
}

/// <summary>
/// Configuration settings for constraint pack behavior.
/// </summary>
internal sealed record PackConfiguration
{
    /// <summary>
    /// Default number of top-K constraints to select.
    /// </summary>
    public int? DefaultTopK { get; init; }

    /// <summary>
    /// Default injection cadence (every N interactions).
    /// </summary>
    public int? DefaultCadence { get; init; }

    /// <summary>
    /// Whether to enable drift detection by default.
    /// </summary>
    public bool? EnableDriftDetection { get; init; }
}