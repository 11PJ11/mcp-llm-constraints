using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Methodology-agnostic composition context for all composition strategies.
/// Contains user-defined workflow state, evaluation status, and analysis data.
/// This context works with any methodology (TDD, BDD, Clean Architecture, etc.)
/// </summary>
public sealed record CompositionContext
{
    /// <summary>
    /// Gets the current user-defined workflow state.
    /// </summary>
    public WorkflowState CurrentWorkflowState { get; init; }

    /// <summary>
    /// Gets the current user-defined evaluation status.
    /// </summary>
    public UserDefinedEvaluationStatus EvaluationStatus { get; init; }

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
        CurrentWorkflowState = new WorkflowState("initial", "Initial workflow state");
        EvaluationStatus = new UserDefinedEvaluationStatus("not-evaluated", "Not yet evaluated", false);
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
    /// Creates a context with user-defined workflow state.
    /// </summary>
    public CompositionContext WithWorkflowState(WorkflowState workflowState)
    {
        return this with { CurrentWorkflowState = workflowState };
    }

    /// <summary>
    /// Creates a context with user-defined evaluation status.
    /// </summary>
    public CompositionContext WithEvaluationStatus(UserDefinedEvaluationStatus evaluationStatus)
    {
        return this with { EvaluationStatus = evaluationStatus };
    }
    
    /// <summary>
    /// Creates a context with development context description.
    /// </summary>
    public CompositionContext WithDevelopmentContext(string developmentContext)
    {
        return this with { DevelopmentContext = developmentContext };
    }
    
    /// <summary>
    /// Creates a context with additional metadata.
    /// </summary>
    public CompositionContext WithMetadata(IReadOnlyDictionary<string, object> metadata)
    {
        return this with { Metadata = metadata };
    }
}

/// <summary>
/// User-defined context information for composition strategy decision-making.
/// Contains methodology-agnostic workflow state and evaluation status.
/// Can be configured for any methodology: TDD, BDD, Clean Architecture, Scrum, etc.
/// </summary>
public sealed record CompositionStrategyContext
{
    /// <summary>
    /// Gets the current user-defined workflow state.
    /// </summary>
    public WorkflowState CurrentWorkflowState { get; init; }

    /// <summary>
    /// Gets the current user-defined evaluation status.
    /// </summary>
    public UserDefinedEvaluationStatus EvaluationStatus { get; init; }

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
        CurrentWorkflowState = new WorkflowState("initial", "Initial workflow state");
        EvaluationStatus = new UserDefinedEvaluationStatus("not-evaluated", "Not yet evaluated", false);
        DevelopmentContext = string.Empty;
        Metadata = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Creates a context from the generic CompositionContext.
    /// </summary>
    /// <param name="context">The composition context to convert from.</param>
    /// <returns>A new CompositionStrategyContext with the same data.</returns>
    public static CompositionStrategyContext FromCompositionContext(CompositionContext context)
    {
        return new CompositionStrategyContext
        {
            CurrentWorkflowState = context.CurrentWorkflowState,
            EvaluationStatus = context.EvaluationStatus,
            DevelopmentContext = context.DevelopmentContext,
            Metadata = context.Metadata
        };
    }
}
