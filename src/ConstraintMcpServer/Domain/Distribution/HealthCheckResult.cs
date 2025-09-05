namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Result of system health validation operation.
/// Business value: Provides diagnostic information for troubleshooting and system validation.
/// </summary>
public sealed record HealthCheckResult
{
    /// <summary>
    /// Whether all health checks passed successfully.
    /// </summary>
    public bool IsHealthy { get; init; }

    /// <summary>
    /// Time taken for health check in seconds.
    /// </summary>
    public double CheckTimeSeconds { get; init; }

    /// <summary>
    /// List of individual check results.
    /// </summary>
    public IReadOnlyList<HealthCheck> Checks { get; init; } = Array.Empty<HealthCheck>();

    /// <summary>
    /// Diagnostic report with detailed information.
    /// </summary>
    public string? DiagnosticReport { get; init; }

    /// <summary>
    /// Creates successful health check result.
    /// </summary>
    public static HealthCheckResult Healthy(double checkTimeSeconds, IReadOnlyList<HealthCheck> checks, string? report = null) =>
        new()
        {
            IsHealthy = true,
            CheckTimeSeconds = checkTimeSeconds,
            Checks = checks,
            DiagnosticReport = report
        };

    /// <summary>
    /// Creates failed health check result.
    /// </summary>
    public static HealthCheckResult Unhealthy(double checkTimeSeconds, IReadOnlyList<HealthCheck> checks, string? report = null) =>
        new()
        {
            IsHealthy = false,
            CheckTimeSeconds = checkTimeSeconds,
            Checks = checks,
            DiagnosticReport = report
        };
}

/// <summary>
/// Individual health check result.
/// </summary>
public sealed record HealthCheck
{
    /// <summary>
    /// Name of the health check.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Whether this check passed.
    /// </summary>
    public bool Passed { get; init; }

    /// <summary>
    /// Message describing the check result.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Creates a passed health check.
    /// </summary>
    public static HealthCheck Pass(string name, string message = "") =>
        new() { Name = name, Passed = true, Message = message };

    /// <summary>
    /// Creates a failed health check.
    /// </summary>
    public static HealthCheck Fail(string name, string message = "") =>
        new() { Name = name, Passed = false, Message = message };
}
