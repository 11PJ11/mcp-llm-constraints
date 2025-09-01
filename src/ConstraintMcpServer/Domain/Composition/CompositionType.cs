namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Defines the composition strategies available for orchestrating multiple constraints.
/// Each type represents a different approach to coordinating constraint activation.
/// </summary>
public enum CompositionType
{
    /// <summary>
    /// Sequential composition: Constraints activate in linear order.
    /// Used for workflows like TDD (RED → GREEN → REFACTOR).
    /// </summary>
    Sequential,

    /// <summary>
    /// Hierarchical composition: Multi-level constraint coordination.
    /// Used for Outside-In development (Acceptance → BDD → TDD).
    /// </summary>
    Hierarchical,

    /// <summary>
    /// Progressive composition: Systematic level advancement.
    /// Used for refactoring cycles (Level 1 → 2 → 3 → 4 → 5 → 6).
    /// </summary>
    Progressive,

    /// <summary>
    /// Layered composition: Clean Architecture layer dependency enforcement.
    /// Used for architectural compliance (Domain → Application → Infrastructure → Presentation).
    /// </summary>
    Layered
}
