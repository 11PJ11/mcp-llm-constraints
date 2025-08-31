namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Immutable context information for composition strategy decision-making.
/// Contains development state, test status, and phase information needed for constraint activation.
/// </summary>
public sealed record CompositionStrategyContext
{
    /// <summary>
    /// Gets the current TDD phase for sequential composition decisions.
    /// </summary>
    public TddPhase CurrentPhase { get; init; }

    /// <summary>
    /// Gets the current test execution status.
    /// </summary>
    public TestStatus TestStatus { get; init; }

    /// <summary>
    /// Gets the development context description.
    /// </summary>
    public string DevelopmentContext { get; init; } = string.Empty;

    /// <summary>
    /// Gets additional composition-specific metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } =
        new Dictionary<string, object>();

    /// <summary>
    /// Creates a new CompositionStrategyContext with default values.
    /// </summary>
    public CompositionStrategyContext()
    {
        CurrentPhase = TddPhase.Red;
        TestStatus = TestStatus.NotRun;
        DevelopmentContext = string.Empty;
        Metadata = new Dictionary<string, object>();
    }
}
