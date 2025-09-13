using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Installation;

/// <summary>
/// Result of installation validation operation.
/// Following DDD: Value object representing validation outcome with business diagnostics.
/// Immutable record with comprehensive validation status and issue reporting.
/// </summary>
public sealed record InstallationValidationResult
{
    /// <summary>
    /// Whether installation validation passed completely.
    /// Business outcome: Primary indicator of installation integrity and completeness.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Path of installation that was validated.
    /// Business context: Installation location for validation reference.
    /// </summary>
    public required string InstallationPath { get; init; }

    /// <summary>
    /// Human-readable validation summary message.
    /// Business communication: User-facing validation outcome description.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Detailed validation issues found during check.
    /// Business diagnostics: Specific problems requiring attention or remediation.
    /// </summary>
    public IReadOnlyList<ValidationIssue> Issues { get; init; } = Array.Empty<ValidationIssue>();

    /// <summary>
    /// Components that passed validation successfully.
    /// Business confirmation: Verified installation elements for completeness tracking.
    /// </summary>
    public IReadOnlyList<string> ValidatedComponents { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Timestamp when validation was performed.
    /// Business audit: Validation execution time for tracking and troubleshooting.
    /// </summary>
    public DateTimeOffset ValidatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Create successful validation result.
    /// Business factory method: Validation success with confirmed components.
    /// </summary>
    /// <param name="installationPath">Path of successfully validated installation.</param>
    /// <param name="validatedComponents">List of components that passed validation.</param>
    /// <returns>Validation result indicating successful validation.</returns>
    public static InstallationValidationResult Success(
        string installationPath,
        IReadOnlyList<string> validatedComponents)
    {
        return new InstallationValidationResult
        {
            IsValid = true,
            InstallationPath = installationPath,
            Message = "Installation validation completed successfully",
            ValidatedComponents = validatedComponents,
            Issues = Array.Empty<ValidationIssue>()
        };
    }

    /// <summary>
    /// Create failed validation result with issue details.
    /// Business factory method: Validation failure with diagnostic information.
    /// </summary>
    /// <param name="installationPath">Path of installation that failed validation.</param>
    /// <param name="issues">Detailed validation issues found.</param>
    /// <returns>Validation result indicating validation failure with diagnostics.</returns>
    public static InstallationValidationResult Failure(
        string installationPath,
        IReadOnlyList<ValidationIssue> issues)
    {
        return new InstallationValidationResult
        {
            IsValid = false,
            InstallationPath = installationPath,
            Message = "Installation validation failed",
            Issues = issues,
            ValidatedComponents = Array.Empty<string>()
        };
    }
}

/// <summary>
/// Specific validation issue found during installation validation.
/// Following DDD: Value object representing individual validation problem with context.
/// </summary>
public sealed record ValidationIssue
{
    /// <summary>
    /// Severity level of validation issue.
    /// Business classification: Issue priority for resolution planning.
    /// </summary>
    public required ValidationSeverity Severity { get; init; }

    /// <summary>
    /// Component or area where issue was found.
    /// Business context: Installation element requiring attention.
    /// </summary>
    public required string Component { get; init; }

    /// <summary>
    /// Detailed description of validation issue.
    /// Business information: Specific problem description for resolution.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Suggested resolution for validation issue.
    /// Business guidance: Recommended action to address the problem.
    /// </summary>
    public string SuggestedResolution { get; init; } = string.Empty;
}

/// <summary>
/// Severity levels for validation issues.
/// Following DDD: Business-relevant enumeration for issue classification.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>Information only - no action required.</summary>
    Info = 1,

    /// <summary>Warning - installation functional but suboptimal.</summary>
    Warning = 2,

    /// <summary>Error - installation may not function correctly.</summary>
    Error = 3,

    /// <summary>Critical - installation will not function.</summary>
    Critical = 4
}
