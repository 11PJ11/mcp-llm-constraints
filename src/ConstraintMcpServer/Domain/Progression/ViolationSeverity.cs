namespace ConstraintMcpServer.Domain.Progression;

/// <summary>
/// Represents the severity level of constraint violations detected in agent behavior.
/// Used to prioritize interventions and measure constraint adherence impact.
/// </summary>
public enum ViolationSeverity
{
    /// <summary>
    /// Minor deviation from constraint guidelines with minimal impact
    /// </summary>
    Minor = 0,

    /// <summary>
    /// Moderate violation affecting code quality or methodology adherence
    /// </summary>
    Moderate = 1,

    /// <summary>
    /// Major violation significantly impacting software craft principles
    /// </summary>
    Major = 2,

    /// <summary>
    /// Critical violation fundamentally undermining constraint objectives
    /// </summary>
    Critical = 3
}
