using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// YAML-based implementation of constraint pack reader.
/// Minimal implementation to satisfy failing tests and establish domain foundation.
/// </summary>
public sealed class YamlConstraintPackReader : IConstraintPackReader
{
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// Initializes a new instance of YamlConstraintPackReader.
    /// </summary>
    public YamlConstraintPackReader()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
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
            YamlConstraintPackDto yamlData = _deserializer.Deserialize<YamlConstraintPackDto>(yamlContent);

            if (yamlData == null)
            {
                throw new ValidationException("YAML content cannot be empty or invalid");
            }

            return ConvertToConstraintPack(yamlData);
        }
        catch (ValidationException)
        {
            throw; // Re-throw validation exceptions as-is
        }
        catch (Exception ex)
        {
            throw new ValidationException($"Failed to parse YAML content: {ex.Message}", ex);
        }
    }

    private static ConstraintPack ConvertToConstraintPack(YamlConstraintPackDto yamlData)
    {
        if (string.IsNullOrWhiteSpace(yamlData.Version))
        {
            throw new ValidationException("Constraint pack version is required");
        }

        if (yamlData.Constraints == null || yamlData.Constraints.Count == 0)
        {
            throw new ValidationException("Constraint pack must contain at least one constraint");
        }

        List<Constraint> constraints = new();
        HashSet<string> seenIds = new();

        foreach (YamlConstraintDto constraintDto in yamlData.Constraints)
        {
            // Validate and convert constraint
            if (string.IsNullOrWhiteSpace(constraintDto.Id))
            {
                throw new ValidationException("Constraint ID is required");
            }

            if (!seenIds.Add(constraintDto.Id))
            {
                throw new ValidationException($"Duplicate constraint ID found: {constraintDto.Id}");
            }

            if (string.IsNullOrWhiteSpace(constraintDto.Title))
            {
                throw new ValidationException($"Constraint '{constraintDto.Id}' must have a title");
            }

            // Validate and create priority
            Priority priority = new(constraintDto.Priority);

            // Validate and create phases
            if (constraintDto.Phases == null || constraintDto.Phases.Count == 0)
            {
                throw new ValidationException($"Constraint '{constraintDto.Id}' must have at least one phase");
            }

            List<Phase> phases = new();
            foreach (string phaseValue in constraintDto.Phases)
            {
                phases.Add(new Phase(phaseValue));
            }

            // Validate reminders
            if (constraintDto.Reminders == null || constraintDto.Reminders.Count == 0)
            {
                throw new ValidationException($"Constraint '{constraintDto.Id}' must have at least one reminder (reminders cannot be empty)");
            }

            foreach (string reminder in constraintDto.Reminders)
            {
                if (string.IsNullOrWhiteSpace(reminder))
                {
                    throw new ValidationException($"Constraint '{constraintDto.Id}' has empty or whitespace reminder");
                }
            }

            // Create constraint
            Constraint constraint = new(
                new ConstraintId(constraintDto.Id),
                constraintDto.Title,
                priority,
                phases,
                constraintDto.Reminders);

            constraints.Add(constraint);
        }

        return new ConstraintPack(yamlData.Version, constraints);
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
