namespace ConstraintMcpServer.Domain.Progression;

/// <summary>
/// Represents the trend direction in agent constraint compliance over time.
/// Used to identify drift patterns and trigger proactive interventions.
/// </summary>
public enum ComplianceTrend
{
    /// <summary>
    /// Compliance trend cannot be determined due to insufficient data
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Agent compliance is deteriorating over time
    /// </summary>
    Declining = 1,

    /// <summary>
    /// Agent compliance remains relatively stable
    /// </summary>
    Stable = 2,

    /// <summary>
    /// Agent compliance is improving over time
    /// </summary>
    Improving = 3
}
