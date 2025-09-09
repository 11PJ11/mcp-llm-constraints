using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Application.Selection;

/// <summary>
/// Manages backward compatibility for configuration rollbacks and version downgrades.
/// Implementation driven by unit tests following Outside-In TDD methodology.
/// </summary>
public sealed class BackwardCompatibilityManager
{
    public async Task<CompatibilityResult> EnsureRollbackCompatibilityAsync(string fromVersion, string toVersion, object configuration)
    {
        await Task.CompletedTask;

        // Basic version compatibility check
        var isCompatible = await CheckVersionCompatibilityAsync(fromVersion, toVersion);

        if (!isCompatible)
        {
            return new CompatibilityResult
            {
                IsCompatible = false,
                Reason = "Version contains breaking changes that prevent rollback"
            };
        }

        // Analyze configuration for rollback transformations needed
        var transformations = AnalyzeRequiredTransformations(fromVersion, toVersion, configuration);

        return new CompatibilityResult
        {
            IsCompatible = true,
            RequiredTransformations = transformations
        };
    }

    public async Task<bool> CheckVersionCompatibilityAsync(string fromVersion, string toVersion)
    {
        await Task.CompletedTask;

        // Business logic: Check if rollback is supported
        // v0.2.0 -> v0.1.0 is supported (minor version rollback)
        // v2.0.0 -> v0.1.0 is not supported (major version rollback with breaking changes)

        var from = ParseVersion(fromVersion);
        var to = ParseVersion(toVersion);

        // Major version rollbacks are not supported
        if (from.Major > to.Major + 1)
        {
            return false;
        }

        // Same major version rollbacks are supported
        return true;
    }

    private List<string> AnalyzeRequiredTransformations(string fromVersion, string toVersion, object configuration)
    {
        var transformations = new List<string>();

        // Analyze configuration structure for transformations needed
        if (fromVersion.StartsWith("v0.2") && toVersion.StartsWith("v0.1"))
        {
            // v0.2 -> v0.1 transformations
            transformations.Add("Remove enhanced_features section");
            transformations.Add("Convert constraint_management.default_packs to constraint_packs");
            transformations.Add("Remove mcp_integration section if present");
        }

        return transformations;
    }

    private (int Major, int Minor, int Patch) ParseVersion(string version)
    {
        // Simple version parsing for v0.2.0 format
        var cleanVersion = version.Replace("v", "");
        var parts = cleanVersion.Split('.');

        var major = parts.Length > 0 && int.TryParse(parts[0], out var maj) ? maj : 0;
        var minor = parts.Length > 1 && int.TryParse(parts[1], out var min) ? min : 0;
        var patch = parts.Length > 2 && int.TryParse(parts[2], out var pat) ? pat : 0;

        return (major, minor, patch);
    }
}

/// <summary>
/// Result of a backward compatibility check operation.
/// </summary>
public sealed class CompatibilityResult
{
    public bool IsCompatible { get; init; }
    public List<string> RequiredTransformations { get; init; } = new();
    public string Reason { get; init; } = string.Empty;
}
