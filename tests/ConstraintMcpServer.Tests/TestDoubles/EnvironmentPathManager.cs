namespace ConstraintMcpServer.Tests.TestDoubles;

/// <summary>
/// Manages environment PATH configuration for test scenarios.
/// Business value: Provides consistent PATH management across test installations.
/// </summary>
internal static class EnvironmentPathManager
{
    private const string PathEnvironmentVariable = "PATH";

    public static void ConfigureEnvironmentPath(string installationPath)
    {
        var currentPath = Environment.GetEnvironmentVariable(PathEnvironmentVariable, EnvironmentVariableTarget.Process);
        var newPath = $"{installationPath}{Path.PathSeparator}{currentPath}";
        Environment.SetEnvironmentVariable(PathEnvironmentVariable, newPath, EnvironmentVariableTarget.Process);
    }
}
