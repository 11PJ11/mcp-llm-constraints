namespace ConstraintMcpServer.Domain.Distribution;

/// <summary>
/// Service for managing workflow continuity during system updates.
/// Business value: Ensures zero disruption to user productivity and ongoing workflows.
/// </summary>
public interface IWorkflowContinuityManager
{
    /// <summary>
    /// Gets all currently active workflows that need preservation during updates.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active workflow states</returns>
    Task<IEnumerable<WorkflowState>> GetActiveWorkflowsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Preserves workflow state before system update to ensure seamless continuation.
    /// </summary>
    /// <param name="workflowId">Unique workflow identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if workflow state was successfully preserved</returns>
    Task<bool> PreserveWorkflowStateAsync(string workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores workflow state after system update to continue seamless operation.
    /// </summary>
    /// <param name="workflowId">Unique workflow identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if workflow was successfully restored and continued</returns>
    Task<bool> RestoreWorkflowStateAsync(string workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that ongoing workflows continue seamlessly after system updates.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result indicating workflow continuity success</returns>
    Task<WorkflowContinuityResult> ValidateWorkflowContinuityAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new workflow for tracking continuity during updates.
    /// </summary>
    /// <param name="workflowName">Human-readable workflow name</param>
    /// <param name="workflowType">Type of workflow being tracked</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Unique workflow identifier</returns>
    Task<string> CreateWorkflowAsync(string workflowName, string workflowType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Advances a workflow to the next step to simulate ongoing progress.
    /// </summary>
    /// <param name="workflowId">Unique workflow identifier</param>
    /// <param name="stepDescription">Description of the workflow step</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if workflow step was successfully advanced</returns>
    Task<bool> AdvanceWorkflowStepAsync(string workflowId, string stepDescription, CancellationToken cancellationToken = default);
}
