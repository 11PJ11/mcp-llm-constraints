using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Common;
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
        // Validate the entire pack structure
        yamlData.Validate();

        // Convert each validated constraint DTO to domain object
        List<Constraint> constraints = new();
        foreach (YamlConstraintDto constraintDto in yamlData.Constraints)
        {
            constraints.Add(constraintDto.ToDomainObject());
        }

        return new ConstraintPack(yamlData.Version, constraints);
    }
}

/// <summary>
/// Data transfer object for YAML constraint pack deserialization.
/// Maps directly to YAML structure and provides validation behavior.
/// </summary>
internal sealed class YamlConstraintPackDto
{
    public string Version { get; set; } = string.Empty;
    public List<YamlConstraintDto> Constraints { get; set; } = new();

    /// <summary>
    /// Validates the constraint pack structure and data integrity.
    /// </summary>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Version))
        {
            throw new ValidationException("Constraint pack version is required");
        }

        if (Constraints == null || Constraints.Count == 0)
        {
            throw new ValidationException("Constraint pack must contain at least one constraint");
        }

        HashSet<string> seenIds = new();
        foreach (YamlConstraintDto constraint in Constraints)
        {
            constraint.Validate();

            if (!seenIds.Add(constraint.Id))
            {
                throw new ValidationException($"Duplicate constraint ID found: {constraint.Id}");
            }
        }
    }
}

/// <summary>
/// Data transfer object for YAML constraint deserialization.
/// Maps directly to YAML structure and provides validation and conversion behavior.
/// </summary>
internal sealed class YamlConstraintDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public double Priority { get; set; }
    public List<string> Phases { get; set; } = new();
    public List<string> Reminders { get; set; } = new();

    /// <summary>
    /// Validates the constraint data integrity.
    /// </summary>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ValidationException("Constraint ID is required");
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            throw new ValidationException($"Constraint '{Id}' must have a title");
        }

        if (Phases == null || Phases.Count == 0)
        {
            throw new ValidationException($"Constraint '{Id}' must have at least one phase");
        }

        if (Reminders == null || Reminders.Count == 0)
        {
            throw new ValidationException($"Constraint '{Id}' must have at least one reminder (reminders cannot be empty)");
        }

        foreach (string reminder in Reminders)
        {
            if (string.IsNullOrWhiteSpace(reminder))
            {
                throw new ValidationException($"Constraint '{Id}' has empty or whitespace reminder");
            }
        }
    }

    /// <summary>
    /// Default priority assigned to workflow contexts when loading from YAML configuration.
    /// This represents standard importance for user-defined workflow phases.
    /// </summary>
    private const double DefaultWorkflowContextPriority = 0.8;
    
    /// <summary>
    /// Standard context category for workflow-related contexts loaded from configuration.
    /// </summary>
    private const string WorkflowContextCategory = "workflow";
    
    /// <summary>
    /// Converts this DTO to a domain Constraint object.
    /// </summary>
    /// <returns>Domain Constraint object with validated data.</returns>
    public Constraint ToDomainObject()
    {
        // Validation is assumed to have been called before conversion
        Priority priority = new(Priority);

        List<UserDefinedContext> workflowContexts = new();
        foreach (string phaseValue in Phases)
        {
            // Convert phase strings to UserDefinedContext with standard priority
            workflowContexts.Add(new UserDefinedContext(WorkflowContextCategory, phaseValue, DefaultWorkflowContextPriority));
        }

        return new Constraint(
            new ConstraintId(Id),
            Title,
            priority,
            workflowContexts,
            Reminders);
    }
}
