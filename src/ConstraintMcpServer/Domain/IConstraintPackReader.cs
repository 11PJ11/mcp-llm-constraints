using System.Threading.Tasks;

namespace ConstraintMcpServer.Domain;

/// <summary>
/// Interface for loading constraint packs from YAML files.
/// Handles YAML parsing and deserialization into constraint domain objects.
/// </summary>
public interface IConstraintPackReader
{
    /// <summary>
    /// Loads a constraint pack from the specified YAML file.
    /// </summary>
    /// <param name="filePath">Path to the YAML constraint configuration file.</param>
    /// <returns>The loaded and validated constraint pack.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="ValidationException">Thrown when the YAML content is invalid.</exception>
    Task<ConstraintPack> LoadAsync(string filePath);
}
