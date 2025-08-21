namespace ConstraintMcpServer.Domain.Enforcement;

/// <summary>
/// Domain interface for deterministic constraint injection scheduling.
/// Part of the Enforcement bounded context.
/// </summary>
internal interface IScheduler
{
    /// <summary>
    /// Determines whether constraints should be injected for the current interaction.
    /// </summary>
    /// <param name="context">Current session and interaction context</param>
    /// <returns>True if constraints should be injected, false for pass-through</returns>
    bool ShouldInjectConstraints(InteractionContext context);

    /// <summary>
    /// Updates the scheduler state after processing an interaction.
    /// </summary>
    /// <param name="context">Interaction context that was processed</param>
    /// <param name="wasInjected">Whether constraints were actually injected</param>
    void UpdateAfterInteraction(InteractionContext context, bool wasInjected);

    /// <summary>
    /// Gets the current schedule configuration for diagnostics.
    /// </summary>
    ScheduleConfiguration GetCurrentConfiguration();
}