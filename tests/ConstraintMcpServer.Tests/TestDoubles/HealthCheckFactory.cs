using ConstraintMcpServer.Domain.Distribution;

namespace ConstraintMcpServer.Tests.TestDoubles;

/// <summary>
/// Creates standardized health check configurations for test scenarios.
/// Business value: Provides consistent health validation across test installations.
/// </summary>
internal static class HealthCheckFactory
{
    public static List<HealthCheck> CreateStandardHealthChecks()
    {
        return new List<HealthCheck>
        {
            HealthCheck.Pass("Environment", "Runtime environment validated"),
            HealthCheck.Pass("Configuration", "Configuration files intact"),
            HealthCheck.Pass("Connectivity", "MCP protocol connectivity confirmed"),
            HealthCheck.Pass("Functionality", "Constraint system operational")
        };
    }
}
