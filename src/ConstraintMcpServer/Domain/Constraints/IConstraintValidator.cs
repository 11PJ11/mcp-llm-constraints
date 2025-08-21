namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Domain interface for validating constraint definitions and packs.
/// Part of the Constraint Catalog bounded context.
/// </summary>
internal interface IConstraintValidator
{
    /// <summary>
    /// Validates a constraint definition for correctness and completeness.
    /// </summary>
    /// <param name="constraint">Constraint to validate</param>
    /// <returns>Validation result indicating success or specific errors</returns>
    ConstraintValidationResult ValidateConstraint(Constraint constraint);

    /// <summary>
    /// Validates a complete constraint pack for consistency and business rules.
    /// </summary>
    /// <param name="constraintPack">Constraint pack to validate</param>
    /// <returns>Validation result indicating success or specific errors</returns>
    ConstraintValidationResult ValidateConstraintPack(ConstraintPack constraintPack);
}