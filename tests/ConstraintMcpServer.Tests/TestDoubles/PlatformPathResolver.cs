using ConstraintMcpServer.Domain.Distribution;

namespace ConstraintMcpServer.Tests.TestDoubles;

/// <summary>
/// Resolves platform-specific installation paths for test scenarios.
/// Business value: Provides consistent cross-platform path handling in tests.
/// </summary>
internal static class PlatformPathResolver
{
    public static string GetDefaultInstallationPath(PlatformType platform)
    {
        return platform switch
        {
            PlatformType.Windows => @"C:\Program Files\ConstraintMcpServer",
            PlatformType.Linux => "/usr/local/bin/constraint-mcp-server",
            PlatformType.MacOS => "/usr/local/bin/constraint-mcp-server",
            _ => throw new PlatformNotSupportedException($"Platform {platform} not supported")
        };
    }
}
