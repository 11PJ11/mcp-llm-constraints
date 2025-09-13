using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Installation;

/// <summary>
/// Result of system health validation after installation.
/// Following DDD: Value object representing comprehensive system operational readiness.
/// Immutable record with health status indicators and component diagnostics.
/// </summary>
public sealed record SystemHealthResult
{
    /// <summary>
    /// Whether all system health checks passed successfully.
    /// Business outcome: Primary indicator of system operational readiness.
    /// </summary>
    public required bool IsHealthy { get; init; }

    /// <summary>
    /// Path of installation that was health-checked.
    /// Business context: Installation location for health validation reference.
    /// </summary>
    public required string InstallationPath { get; init; }

    /// <summary>
    /// Overall system health summary message.
    /// Business communication: User-facing health status description.
    /// </summary>
    public string HealthMessage { get; init; } = string.Empty;

    /// <summary>
    /// Individual component health check results.
    /// Business diagnostics: Detailed health status for each system component.
    /// </summary>
    public IReadOnlyList<ComponentHealthCheck> ComponentHealthChecks { get; init; } = Array.Empty<ComponentHealthCheck>();

    /// <summary>
    /// System health issues requiring attention.
    /// Business diagnostics: Problems that may impact system operation.
    /// </summary>
    public IReadOnlyList<HealthIssue> HealthIssues { get; init; } = Array.Empty<HealthIssue>();

    /// <summary>
    /// Timestamp when health validation was performed.
    /// Business audit: Health check execution time for tracking and monitoring.
    /// </summary>
    public DateTimeOffset HealthCheckedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Create healthy system result with all components operational.
    /// Business factory method: System health success with component confirmation.
    /// </summary>
    /// <param name="installationPath">Path of healthy installation.</param>
    /// <param name="componentChecks">List of successful component health checks.</param>
    /// <returns>System health result indicating full operational readiness.</returns>
    public static SystemHealthResult Healthy(
        string installationPath,
        IReadOnlyList<ComponentHealthCheck> componentChecks)
    {
        return new SystemHealthResult
        {
            IsHealthy = true,
            InstallationPath = installationPath,
            HealthMessage = "All system components are healthy and operational",
            ComponentHealthChecks = componentChecks,
            HealthIssues = Array.Empty<HealthIssue>()
        };
    }

    /// <summary>
    /// Create unhealthy system result with identified health issues.
    /// Business factory method: System health failure with diagnostic information.
    /// </summary>
    /// <param name="installationPath">Path of installation with health issues.</param>
    /// <param name="componentChecks">List of component health check results.</param>
    /// <param name="healthIssues">Detailed health issues found.</param>
    /// <returns>System health result indicating operational problems.</returns>
    public static SystemHealthResult Unhealthy(
        string installationPath,
        IReadOnlyList<ComponentHealthCheck> componentChecks,
        IReadOnlyList<HealthIssue> healthIssues)
    {
        return new SystemHealthResult
        {
            IsHealthy = false,
            InstallationPath = installationPath,
            HealthMessage = "System health issues detected",
            ComponentHealthChecks = componentChecks,
            HealthIssues = healthIssues
        };
    }
}

/// <summary>
/// Health check result for individual system component.
/// Following DDD: Value object representing component-specific health status.
/// </summary>
public sealed record ComponentHealthCheck
{
    /// <summary>
    /// Name of system component being checked.
    /// Business identification: Component name for health tracking.
    /// </summary>
    public required string ComponentName { get; init; }

    /// <summary>
    /// Whether component passed health check.
    /// Business outcome: Component operational status indicator.
    /// </summary>
    public required bool IsHealthy { get; init; }

    /// <summary>
    /// Detailed health check message for component.
    /// Business information: Component-specific health description.
    /// </summary>
    public string HealthMessage { get; init; } = string.Empty;

    /// <summary>
    /// Duration of health check in milliseconds.
    /// Business metrics: Performance tracking for health validation.
    /// </summary>
    public long CheckDurationMs { get; init; }
}

/// <summary>
/// Specific health issue found during system validation.
/// Following DDD: Value object representing individual system health problem.
/// </summary>
public sealed record HealthIssue
{
    /// <summary>
    /// Severity level of health issue.
    /// Business classification: Issue priority for system operation.
    /// </summary>
    public required HealthSeverity Severity { get; init; }

    /// <summary>
    /// Component or area where health issue was found.
    /// Business context: System element with operational problem.
    /// </summary>
    public required string Component { get; init; }

    /// <summary>
    /// Detailed description of health issue.
    /// Business information: Specific problem affecting system operation.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Impact of health issue on system operation.
    /// Business assessment: Effect of problem on system functionality.
    /// </summary>
    public string Impact { get; init; } = string.Empty;

    /// <summary>
    /// Suggested action to resolve health issue.
    /// Business guidance: Recommended steps to restore system health.
    /// </summary>
    public string SuggestedAction { get; init; } = string.Empty;
}

/// <summary>
/// Severity levels for system health issues.
/// Following DDD: Business-relevant enumeration for health issue classification.
/// </summary>
public enum HealthSeverity
{
    /// <summary>Minor issue - system operational with reduced capability.</summary>
    Minor = 1,

    /// <summary>Moderate issue - system operational with noticeable impact.</summary>
    Moderate = 2,

    /// <summary>Major issue - system operational but significantly impaired.</summary>
    Major = 3,

    /// <summary>Critical issue - system may not function correctly.</summary>
    Critical = 4
}
