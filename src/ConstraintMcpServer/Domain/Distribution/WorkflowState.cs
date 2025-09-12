namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Represents the state of an active workflow that needs continuity during updates.
/// Business value: Enables preservation of user work and workflow progress across system updates.
/// </summary>
public sealed record WorkflowState
{
    /// <summary>
    /// Unique identifier for the workflow.
    /// </summary>
    public string WorkflowId { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable name of the workflow.
    /// </summary>
    public string WorkflowName { get; init; } = string.Empty;

    /// <summary>
    /// Type or category of workflow (e.g., "development", "deployment", "analysis").
    /// </summary>
    public string WorkflowType { get; init; } = string.Empty;

    /// <summary>
    /// Current step or phase of the workflow.
    /// </summary>
    public string CurrentStep { get; init; } = string.Empty;

    /// <summary>
    /// Progress percentage of the workflow completion (0.0 to 100.0).
    /// </summary>
    public double ProgressPercentage { get; init; }

    /// <summary>
    /// Whether the workflow is currently active and executing.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Whether the workflow state has been preserved for continuity.
    /// </summary>
    public bool IsPreserved { get; init; }

    /// <summary>
    /// Whether the workflow has been successfully restored after an update.
    /// </summary>
    public bool IsRestored { get; init; }

    /// <summary>
    /// Timestamp when the workflow was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Timestamp when the workflow was last updated.
    /// </summary>
    public DateTime LastUpdated { get; init; }

    /// <summary>
    /// Timestamp when the workflow state was preserved.
    /// </summary>
    public DateTime? PreservedAt { get; init; }

    /// <summary>
    /// Timestamp when the workflow state was restored.
    /// </summary>
    public DateTime? RestoredAt { get; init; }

    /// <summary>
    /// Creates an active workflow state.
    /// </summary>
    public static WorkflowState CreateActive(string workflowId, string workflowName, string workflowType, string currentStep) =>
        new()
        {
            WorkflowId = workflowId,
            WorkflowName = workflowName,
            WorkflowType = workflowType,
            CurrentStep = currentStep,
            ProgressPercentage = 0.0,
            IsActive = true,
            IsPreserved = false,
            IsRestored = false,
            CreatedAt = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

    /// <summary>
    /// Creates a preserved copy of this workflow state.
    /// </summary>
    public WorkflowState AsPreserved(DateTime preservedAt) =>
        this with
        {
            IsPreserved = true,
            PreservedAt = preservedAt,
            LastUpdated = preservedAt
        };

    /// <summary>
    /// Creates a restored copy of this workflow state.
    /// </summary>
    public WorkflowState AsRestored(DateTime restoredAt) =>
        this with
        {
            IsRestored = true,
            RestoredAt = restoredAt,
            LastUpdated = restoredAt
        };

    /// <summary>
    /// Creates an updated copy with new step and progress information.
    /// </summary>
    public WorkflowState WithProgress(string newStep, double progressPercentage) =>
        this with
        {
            CurrentStep = newStep,
            ProgressPercentage = progressPercentage,
            LastUpdated = DateTime.UtcNow
        };
}
