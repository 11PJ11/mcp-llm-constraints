namespace ConstraintMcpServer.Domain.Progression;

/// <summary>
/// Represents the level of constraint compliance demonstrated by an agent.
/// Used to categorize agent adherence to software-craft constraints during code generation.
/// </summary>
public enum ConstraintComplianceLevel
{
    /// <summary>
    /// Agent consistently violates constraints with minimal adherence
    /// </summary>
    NonCompliant = 0,

    /// <summary>
    /// Agent shows partial compliance with frequent violations
    /// </summary>
    PartiallyCompliant = 1,

    /// <summary>
    /// Agent demonstrates adequate compliance with occasional violations
    /// </summary>
    AdequatelyCompliant = 2,

    /// <summary>
    /// Agent maintains high compliance with rare violations
    /// </summary>
    HighlyCompliant = 3,

    /// <summary>
    /// Agent demonstrates exceptional constraint adherence
    /// </summary>
    ExceptionallyCompliant = 4
}
