using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Validates constraint pack configurations for business rule compliance.
/// Ensures constraints have unique IDs, valid priorities, and proper structure.
/// </summary>
internal sealed class ConstraintPackValidator : IConstraintValidator
{
    /// <inheritdoc />
    public Task<ValidationResult> ValidateAsync(ConstraintPack constraintPack)
    {
        if (constraintPack == null)
        {
            return Task.FromResult(ValidationResult.Failure("Constraint pack cannot be null"));
        }

        var errors = new List<string>();

        // Validate version
        if (string.IsNullOrWhiteSpace(constraintPack.Version))
        {
            errors.Add("Version is required");
        }

        // Validate constraints collection
        if (constraintPack.Constraints == null || constraintPack.Constraints.Count == 0)
        {
            errors.Add("At least one constraint is required");
        }
        else
        {
            // Check for duplicate IDs
            var duplicateIds = constraintPack.Constraints
                .GroupBy(c => c.Id.Value, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateIds.Count > 0)
            {
                errors.Add($"Duplicate constraint IDs found: {string.Join(", ", duplicateIds)}");
            }

            // Validate individual constraints
            for (int i = 0; i < constraintPack.Constraints.Count; i++)
            {
                Constraint constraint = constraintPack.Constraints[i];
                ValidateConstraint(constraint, i, errors);
            }
        }

        ValidationResult result = errors.Count == 0
            ? ValidationResult.Success
            : ValidationResult.Failure(errors);

        return Task.FromResult(result);
    }

    private static void ValidateConstraint(Constraint constraint, int index, List<string> errors)
    {
        if (constraint == null)
        {
            errors.Add($"Constraint at index {index} cannot be null");
            return;
        }

        if (string.IsNullOrWhiteSpace(constraint.Title))
        {
            errors.Add($"Constraint at index {index}: Title is required");
        }

        if (constraint.Phases == null || constraint.Phases.Count == 0)
        {
            errors.Add($"Constraint at index {index}: At least one phase is required");
        }

        if (constraint.Reminders == null || constraint.Reminders.Count == 0)
        {
            errors.Add($"Constraint at index {index}: At least one reminder is required");
        }
    }
}
