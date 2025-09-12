namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Represents the result of workflow continuity validation during system updates.
/// Business value: Provides evidence that user workflows continue seamlessly across updates.
/// </summary>
public sealed record WorkflowContinuityResult
{
    /// <summary>
    /// Whether workflow continuity validation was successful.
    /// </summary>
    public bool IsSuccessful { get; init; }

    /// <summary>
    /// Total number of workflows that were being tracked.
    /// </summary>
    public int TotalWorkflowsTracked { get; init; }

    /// <summary>
    /// Number of workflows that maintained continuity successfully.
    /// </summary>
    public int WorkflowsContinuedSuccessfully { get; init; }

    /// <summary>
    /// Number of workflows that experienced disruption.
    /// </summary>
    public int WorkflowsDisrupted { get; init; }

    /// <summary>
    /// Collection of workflow IDs that continued successfully.
    /// </summary>
    public IReadOnlyList<string> SuccessfulWorkflows { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Collection of workflow IDs that were disrupted.
    /// </summary>
    public IReadOnlyList<string> DisruptedWorkflows { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Detailed validation message describing the continuity result.
    /// </summary>
    public string ValidationMessage { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp when the continuity validation was performed.
    /// </summary>
    public DateTime ValidatedAt { get; init; }

    /// <summary>
    /// Time taken to complete the continuity validation.
    /// </summary>
    public TimeSpan ValidationDuration { get; init; }

    /// <summary>
    /// Creates a successful workflow continuity result.
    /// </summary>
    public static WorkflowContinuityResult Success(int totalWorkflows, IReadOnlyList<string> successfulWorkflows, TimeSpan duration) =>
        new()
        {
            IsSuccessful = true,
            TotalWorkflowsTracked = totalWorkflows,
            WorkflowsContinuedSuccessfully = successfulWorkflows.Count,
            WorkflowsDisrupted = 0,
            SuccessfulWorkflows = successfulWorkflows,
            DisruptedWorkflows = Array.Empty<string>(),
            ValidationMessage = $"All {totalWorkflows} workflows continued seamlessly after system update",
            ValidatedAt = DateTime.UtcNow,
            ValidationDuration = duration
        };

    /// <summary>
    /// Creates a partially successful workflow continuity result.
    /// </summary>
    public static WorkflowContinuityResult Partial(int totalWorkflows, IReadOnlyList<string> successfulWorkflows, IReadOnlyList<string> disruptedWorkflows, TimeSpan duration) =>
        new()
        {
            IsSuccessful = disruptedWorkflows.Count == 0,
            TotalWorkflowsTracked = totalWorkflows,
            WorkflowsContinuedSuccessfully = successfulWorkflows.Count,
            WorkflowsDisrupted = disruptedWorkflows.Count,
            SuccessfulWorkflows = successfulWorkflows,
            DisruptedWorkflows = disruptedWorkflows,
            ValidationMessage = $"{successfulWorkflows.Count} of {totalWorkflows} workflows continued successfully, {disruptedWorkflows.Count} experienced disruption",
            ValidatedAt = DateTime.UtcNow,
            ValidationDuration = duration
        };

    /// <summary>
    /// Creates a failed workflow continuity result.
    /// </summary>
    public static WorkflowContinuityResult Failure(int totalWorkflows, IReadOnlyList<string> disruptedWorkflows, string reason, TimeSpan duration) =>
        new()
        {
            IsSuccessful = false,
            TotalWorkflowsTracked = totalWorkflows,
            WorkflowsContinuedSuccessfully = 0,
            WorkflowsDisrupted = disruptedWorkflows.Count,
            SuccessfulWorkflows = Array.Empty<string>(),
            DisruptedWorkflows = disruptedWorkflows,
            ValidationMessage = $"Workflow continuity validation failed: {reason}",
            ValidatedAt = DateTime.UtcNow,
            ValidationDuration = duration
        };
}
