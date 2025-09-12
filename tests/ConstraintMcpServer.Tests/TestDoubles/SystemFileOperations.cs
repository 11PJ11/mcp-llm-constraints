using System.IO;

namespace ConstraintMcpServer.Tests.TestDoubles;

/// <summary>
/// Handles file system operations for test installations.
/// Business value: Provides consistent file system setup across test scenarios.
/// </summary>
internal static class SystemFileOperations
{
    private const string BinDirectoryName = "bin";
    private const string ConfigDirectoryName = "config";

    public static void CreateInstallationDirectories(string installationPath)
    {
        Directory.CreateDirectory(installationPath);
        Directory.CreateDirectory(Path.Combine(installationPath, BinDirectoryName));
        Directory.CreateDirectory(Path.Combine(installationPath, ConfigDirectoryName));
    }
}
