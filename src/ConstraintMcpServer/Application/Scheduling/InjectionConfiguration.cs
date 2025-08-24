namespace ConstraintMcpServer.Application.Scheduling;

/// <summary>
/// Configuration constants for constraint injection scheduling.
/// Centralizes injection timing and cadence rules.
/// </summary>
public static class InjectionConfiguration
{
    /// <summary>
    /// Default cadence for constraint injection after the initial interaction.
    /// Injects constraints every 3rd interaction to maintain optimal attention without overwhelming the model.
    /// </summary>
    public const int DefaultCadence = 3;

    /// <summary>
    /// Maximum number of constraints to include per injection to maintain attention retention.
    /// Research suggests 2-3 constraints provide optimal balance between guidance and cognitive load.
    /// </summary>
    public const int MaxConstraintsPerInjection = 2;
}
