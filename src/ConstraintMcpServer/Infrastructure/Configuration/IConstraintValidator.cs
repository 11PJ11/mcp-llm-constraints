using System.Threading.Tasks;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Interface for validating constraint pack configurations.
/// Performs business rule validation on loaded constraint data.
/// </summary>
internal interface IConstraintValidator
{
    /// <summary>
    /// Validates a constraint pack for business rule compliance.
    /// </summary>
    /// <param name="constraintPack">The constraint pack to validate.</param>
    /// <returns>Validation result indicating success or failure with error details.</returns>
    Task<ValidationResult> ValidateAsync(ConstraintPack constraintPack);
}
