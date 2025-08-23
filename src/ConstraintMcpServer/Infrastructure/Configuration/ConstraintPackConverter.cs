using System.Linq;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Converts YAML data transfer objects to domain model objects.
/// Handles domain validation and business rule enforcement.
/// </summary>
internal sealed class ConstraintPackConverter
{
    /// <summary>
    /// Converts a YAML DTO to a domain constraint pack.
    /// </summary>
    /// <param name="yamlData">The YAML data transfer object.</param>
    /// <returns>Domain constraint pack with validated business rules.</returns>
    /// <exception cref="ValidationException">Thrown when domain validation fails.</exception>
    public ConstraintPack Convert(YamlConstraintPackDto yamlData)
    {
        ValidatePackStructure(yamlData);

        var constraints = new List<Constraint>();
        for (int i = 0; i < yamlData.Constraints.Count; i++)
        {
            YamlConstraintDto yamlConstraint = yamlData.Constraints[i];
            constraints.Add(ConvertConstraint(yamlConstraint, i));
        }

        return new ConstraintPack
        {
            Version = yamlData.Version,
            Constraints = constraints
        };
    }

    private static void ValidatePackStructure(YamlConstraintPackDto yamlData)
    {
        if (yamlData == null)
        {
            throw new ValidationException("Constraint configuration cannot be null");
        }

        if (string.IsNullOrWhiteSpace(yamlData.Version))
        {
            throw new ValidationException("Version is required");
        }

        if (yamlData.Constraints == null || yamlData.Constraints.Count == 0)
        {
            throw new ValidationException("At least one constraint is required");
        }
    }

    private static Constraint ConvertConstraint(YamlConstraintDto yamlConstraint, int index)
    {
        if (yamlConstraint == null)
        {
            throw new ValidationException($"Constraint at index {index} cannot be null");
        }

        ValidateConstraintFields(yamlConstraint, index);

        var constraintId = new ConstraintId(yamlConstraint.Id);
        var priority = new Priority(yamlConstraint.Priority);
        Phase[] phases = yamlConstraint.Phases.Select(PhaseExtensions.ParsePhase).ToArray();

        return new Constraint
        {
            Id = constraintId,
            Title = yamlConstraint.Title,
            Priority = priority,
            Phases = phases,
            Reminders = yamlConstraint.Reminders.ToArray()
        };
    }

    private static void ValidateConstraintFields(YamlConstraintDto yamlConstraint, int index)
    {
        if (string.IsNullOrWhiteSpace(yamlConstraint.Id))
        {
            throw new ValidationException($"Constraint at index {index}: Id is required");
        }

        if (string.IsNullOrWhiteSpace(yamlConstraint.Title))
        {
            throw new ValidationException($"Constraint at index {index}: Title is required");
        }

        if (yamlConstraint.Priority < 0.0 || yamlConstraint.Priority > 1.0)
        {
            throw new ValidationException($"Constraint at index {index}: Priority must be between 0 and 1");
        }

        if (yamlConstraint.Phases == null || yamlConstraint.Phases.Count == 0)
        {
            throw new ValidationException($"Constraint at index {index}: At least one phase is required");
        }

        if (yamlConstraint.Reminders == null || yamlConstraint.Reminders.Count == 0)
        {
            throw new ValidationException($"Constraint at index {index}: At least one reminder is required");
        }
    }
}
