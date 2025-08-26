namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Defines the different types of composition strategies for composite constraints.
/// Each type determines how atomic constraint components are coordinated and activated.
/// </summary>
public enum CompositionType
{
    /// <summary>
    /// Unknown or undefined composition type.
    /// </summary>
    Unknown,

    /// <summary>
    /// Sequential composition - components activate in a specific order.
    /// Example: Outside-In Development (ATDD → BDD → TDD).
    /// </summary>
    Sequential,

    /// <summary>
    /// Parallel composition - all components are active simultaneously.
    /// Example: Multiple code quality constraints applied together.
    /// </summary>
    Parallel,

    /// <summary>
    /// Hierarchical composition - components activate based on hierarchy levels.
    /// Example: Clean Architecture layers (Domain → Application → Infrastructure).
    /// </summary>
    Hierarchical,

    /// <summary>
    /// Progressive composition - components unlock progressively based on completion.
    /// Example: Refactoring levels (1: Readability → 2: Complexity → 3: Responsibilities).
    /// </summary>
    Progressive,

    /// <summary>
    /// Layered composition - components form layers with dependencies.
    /// Example: Testing pyramid (Unit → Integration → E2E).
    /// </summary>
    Layered
}