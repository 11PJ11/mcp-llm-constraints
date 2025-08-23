using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Parses YAML content into intermediate data transfer objects.
/// Focused solely on YAML deserialization without business logic.
/// </summary>
internal sealed class YamlConstraintPackParser
{
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// Initializes a new instance of the YamlConstraintPackParser class.
    /// </summary>
    public YamlConstraintPackParser()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// Parses YAML content into a data transfer object.
    /// </summary>
    /// <param name="yamlContent">The YAML content to parse.</param>
    /// <returns>Parsed YAML data transfer object.</returns>
    /// <exception cref="ValidationException">Thrown when YAML parsing fails.</exception>
    public YamlConstraintPackDto Parse(string yamlContent)
    {
        try
        {
            YamlConstraintPackDto yamlData = _deserializer.Deserialize<YamlConstraintPackDto>(yamlContent);
            return yamlData ?? throw new ValidationException("YAML content cannot be empty");
        }
        catch (Exception ex) when (!(ex is ValidationException))
        {
            throw new ValidationException($"Failed to parse YAML content: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Data transfer object for YAML constraint pack deserialization.
/// Maps directly to YAML structure without business logic.
/// </summary>
internal sealed class YamlConstraintPackDto
{
    public string Version { get; set; } = string.Empty;
    public List<YamlConstraintDto> Constraints { get; set; } = new();
}

/// <summary>
/// Data transfer object for YAML constraint deserialization.
/// Maps directly to YAML structure without business logic.
/// </summary>
internal sealed class YamlConstraintDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public double Priority { get; set; }
    public List<string> Phases { get; set; } = new();
    public List<string> Reminders { get; set; } = new();
}
