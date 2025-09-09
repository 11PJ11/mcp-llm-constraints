using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Application.Selection;

/// <summary>
/// Validates configuration format and rules for constraint management system.
/// Implementation driven by unit tests following Outside-In TDD methodology.
/// </summary>
public sealed class ConfigurationValidator
{
    public async Task<ValidationResult> ValidateAsync(object configuration)
    {
        await Task.CompletedTask;

        var errors = new List<string>();
        var configDict = ConvertToConfigDictionary(configuration);

        // Validate version is present
        if (!configDict.ContainsKey("version") || configDict["version"] == null)
        {
            errors.Add("Version is required");
        }

        // Validate constraint management section
        if (configDict.ContainsKey("constraint_management"))
        {
            var constraintMgmt = ConvertToConfigDictionary(configDict["constraint_management"]);

            // Validate injection cadence
            if (constraintMgmt.ContainsKey("injection_cadence"))
            {
                if (constraintMgmt["injection_cadence"] is int cadence && cadence < 0)
                {
                    errors.Add("Injection cadence must be positive");
                }
            }
        }

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            ErrorMessages = errors
        };
    }

    private static Dictionary<string, object?> ConvertToConfigDictionary(object? config)
    {
        if (config == null)
        {
            return new Dictionary<string, object?>();
        }

        var result = new Dictionary<string, object?>();
        var properties = config.GetType().GetProperties();

        foreach (var prop in properties)
        {
            result[prop.Name] = prop.GetValue(config);
        }

        return result;
    }
}

/// <summary>
/// Result of a configuration validation operation.
/// </summary>
public sealed class ValidationResult
{
    public bool IsValid { get; init; }
    public List<string> ErrorMessages { get; init; } = new();
}
