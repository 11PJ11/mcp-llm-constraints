using System.IO;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Reads constraint packs from YAML configuration files.
/// Orchestrates YAML parsing and domain conversion with clear separation of concerns.
/// </summary>
internal sealed class YamlConstraintPackReader : IConstraintPackReader
{
    private readonly YamlConstraintPackParser _parser;
    private readonly ConstraintPackConverter _converter;

    /// <summary>
    /// Initializes a new instance of the YamlConstraintPackReader class.
    /// </summary>
    public YamlConstraintPackReader()
    {
        _parser = new YamlConstraintPackParser();
        _converter = new ConstraintPackConverter();
    }

    /// <inheritdoc />
    public async Task<ConstraintPack> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Constraint configuration file not found: {filePath}");
        }

        try
        {
            string yamlContent = await File.ReadAllTextAsync(filePath);
            YamlConstraintPackDto yamlData = _parser.Parse(yamlContent);
            return _converter.Convert(yamlData);
        }
        catch (Exception ex) when (!(ex is FileNotFoundException) && !(ex is ValidationException))
        {
            throw new ValidationException($"Failed to load constraint configuration from {filePath}: {ex.Message}", ex);
        }
    }
}
