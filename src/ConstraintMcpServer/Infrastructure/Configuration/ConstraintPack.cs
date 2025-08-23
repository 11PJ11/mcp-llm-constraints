using System.Collections.Generic;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Represents a collection of constraints loaded from a YAML configuration file.
/// This is the root configuration object for the constraint enforcement system.
/// </summary>
internal sealed record ConstraintPack
{
    /// <summary>
    /// The version of the constraint pack format.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// The collection of constraints defined in this pack.
    /// </summary>
    public required IReadOnlyList<Constraint> Constraints { get; init; }
}
