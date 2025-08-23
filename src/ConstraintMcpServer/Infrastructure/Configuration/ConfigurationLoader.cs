using System.Threading.Tasks;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Loads constraint pack configurations with validation.
/// Orchestrates reading, parsing, and validation of constraint configurations.
/// </summary>
internal sealed class ConfigurationLoader : IConfigurationLoader
{
    private readonly IConstraintPackReader _reader;
    private readonly IConstraintValidator _validator;

    /// <summary>
    /// Initializes a new instance of the ConfigurationLoader class.
    /// </summary>
    /// <param name="reader">The constraint pack reader to use.</param>
    /// <param name="validator">The constraint pack validator to use.</param>
    public ConfigurationLoader(IConstraintPackReader reader, IConstraintValidator validator)
    {
        _reader = reader;
        _validator = validator;
    }

    /// <summary>
    /// Initializes a new instance of the ConfigurationLoader class with default implementations.
    /// </summary>
    public ConfigurationLoader() : this(new YamlConstraintPackReader(), new ConstraintPackValidator())
    {
    }

    /// <inheritdoc />
    public async Task<ConstraintPack?> LoadAsync(string? configurationSource)
    {
        if (string.IsNullOrEmpty(configurationSource))
        {
            return null;
        }

        ConstraintPack constraintPack = await _reader.LoadAsync(configurationSource);

        ValidationResult validationResult = await _validator.ValidateAsync(constraintPack);
        if (!validationResult.IsValid)
        {
            throw new ValidationException($"Configuration validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        return constraintPack;
    }
}
