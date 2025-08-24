using System;

namespace ConstraintMcpServer.Application.Scheduling;

/// <summary>
/// Deterministic scheduler for constraint injection decisions.
/// Implements the core logic: first interaction always injects, then every Nth interaction thereafter.
/// Designed to satisfy the requirement that same inputs produce same outputs.
/// </summary>
public sealed class Scheduler
{
    private readonly int _injectionCadence;

    /// <summary>
    /// Initializes a new scheduler with the specified injection cadence.
    /// </summary>
    /// <param name="everyNInteractions">Inject constraints every N interactions (after the first). Must be positive.</param>
    /// <exception cref="ArgumentException">Thrown when everyNInteractions is less than or equal to zero.</exception>
    public Scheduler(int everyNInteractions)
    {
        if (everyNInteractions <= 0)
        {
            throw new ArgumentException("Interaction cadence must be positive", nameof(everyNInteractions));
        }

        _injectionCadence = everyNInteractions;
    }

    /// <summary>
    /// Determines whether constraints should be injected for the given interaction.
    /// 
    /// Logic:
    /// - First interaction (1): Always inject (kickoff/session establishment)
    /// - Subsequent interactions: Inject every Nth interaction based on cadence
    /// 
    /// Examples with everyNInteractions=3:
    /// - Interaction 1: inject (first)
    /// - Interaction 2: no inject
    /// - Interaction 3: inject (3rd)
    /// - Interaction 4: no inject  
    /// - Interaction 5: no inject
    /// - Interaction 6: inject (6th)
    /// </summary>
    /// <param name="interactionCount">The current interaction number (1-based).</param>
    /// <returns>True if constraints should be injected, false otherwise.</returns>
    public bool ShouldInject(int interactionCount)
    {
        // First interaction always injects to establish session patterns
        if (interactionCount == 1)
        {
            return true;
        }

        // Subsequent interactions: inject every Nth interaction
        return interactionCount % _injectionCadence == 0;
    }
}
