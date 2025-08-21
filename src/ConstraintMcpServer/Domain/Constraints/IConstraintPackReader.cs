namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Domain interface for reading constraint packs from external sources.
/// Part of the Constraint Catalog bounded context.
/// </summary>
internal interface IConstraintPackReader
{
    /// <summary>
    /// Reads constraint pack definitions from a source.
    /// </summary>
    /// <param name="source">Source identifier or path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Constraint pack containing constraint definitions</returns>
    Task<ConstraintPack> ReadConstraintPackAsync(string source, CancellationToken cancellationToken = default);
}