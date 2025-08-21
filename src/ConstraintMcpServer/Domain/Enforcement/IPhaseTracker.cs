namespace ConstraintMcpServer.Domain.Enforcement;

/// <summary>
/// Domain interface for tracking development phases (TDD cycles, etc.).
/// Part of the Enforcement bounded context.
/// </summary>
internal interface IPhaseTracker
{
    /// <summary>
    /// Gets the current development phase.
    /// </summary>
    string CurrentPhase { get; }

    /// <summary>
    /// Updates the current phase based on interaction analysis.
    /// </summary>
    /// <param name="context">Current interaction context</param>
    void UpdatePhase(InteractionContext context);

    /// <summary>
    /// Determines if a constraint is applicable for the current phase.
    /// </summary>
    /// <param name="constraintPhases">Phases where the constraint applies</param>
    /// <returns>True if the constraint is applicable</returns>
    bool IsConstraintApplicable(IReadOnlyList<string> constraintPhases);

    /// <summary>
    /// Resets phase tracking for a new session or project.
    /// </summary>
    void ResetPhase();

    /// <summary>
    /// Gets phase transition history for diagnostics.
    /// </summary>
    IReadOnlyList<PhaseTransition> GetPhaseHistory();
}