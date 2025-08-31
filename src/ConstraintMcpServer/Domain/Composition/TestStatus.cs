namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Represents the current status of test execution.
/// Used by composition strategies to make phase transition decisions.
/// </summary>
public enum TestStatus
{
    /// <summary>
    /// Tests have not been executed yet.
    /// Initial state or when tests are being written.
    /// </summary>
    NotRun,

    /// <summary>
    /// Tests are currently failing.
    /// Indicates RED phase or broken functionality.
    /// </summary>
    Failing,

    /// <summary>
    /// All tests are currently passing.
    /// Indicates GREEN phase and working functionality.
    /// </summary>
    Passing
}
