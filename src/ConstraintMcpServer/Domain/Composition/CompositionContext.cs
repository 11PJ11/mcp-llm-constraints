namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Comprehensive composition context for all composition strategies.
/// Contains development state, test status, phase information, and code analysis data.
/// </summary>
public sealed record CompositionContext
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
    /// Gets code analysis information for architectural decisions.
    /// </summary>
    public CodeAnalysisInfo? CodeAnalysis { get; init; }

    /// <summary>
    /// Gets the current file being worked on.
    /// </summary>
    public FileInfo? CurrentFile { get; init; }

    /// <summary>
    /// Gets additional composition-specific metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } =
        new Dictionary<string, object>();

    /// <summary>
    /// Creates a new CompositionContext with default values.
    /// </summary>
    public CompositionContext()
    {
        CurrentPhase = TddPhase.Red;
        TestStatus = TestStatus.NotRun;
        DevelopmentContext = string.Empty;
        Metadata = new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a context with code analysis information.
    /// </summary>
    public CompositionContext WithCodeAnalysis(CodeAnalysisInfo codeAnalysis)
    {
        return this with { CodeAnalysis = codeAnalysis };
    }

    /// <summary>
    /// Creates a context with current file information.
    /// </summary>
    public CompositionContext WithCurrentFile(FileInfo currentFile)
    {
        return this with { CurrentFile = currentFile };
    }

    /// <summary>
    /// Creates a context with TDD phase.
    /// </summary>
    public CompositionContext WithPhase(TddPhase phase)
    {
        return this with { CurrentPhase = phase };
    }

    /// <summary>
    /// Creates a context with test status.
    /// </summary>
    public CompositionContext WithTestStatus(TestStatus status)
    {
        return this with { TestStatus = status };
    }
}

/// <summary>
/// Legacy context type for backward compatibility.
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
