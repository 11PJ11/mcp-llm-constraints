using System;

namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Represents a constraint activation with metadata and guidance.
/// Used to coordinate constraint activation across different composition strategies.
/// </summary>
public sealed record ConstraintActivation
{
    /// <summary>
    /// The ID of the constraint to activate.
    /// </summary>
    public string ConstraintId { get; init; }

    /// <summary>
    /// The layer or level at which this constraint operates.
    /// </summary>
    public int LayerLevel { get; init; }

    /// <summary>
    /// Human-readable guidance for the constraint activation.
    /// </summary>
    public string Guidance { get; init; }

    /// <summary>
    /// Timestamp when this constraint was activated.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Creates a new constraint activation.
    /// </summary>
    public ConstraintActivation(string constraintId, int layerLevel, string guidance, DateTime timestamp)
    {
        ConstraintId = constraintId ?? throw new ArgumentNullException(nameof(constraintId));
        LayerLevel = layerLevel;
        Guidance = guidance ?? throw new ArgumentNullException(nameof(guidance));
        Timestamp = timestamp;
    }

    /// <summary>
    /// Represents no constraint activation required.
    /// </summary>
    public static ConstraintActivation None => new("none", -1, "No constraint activation required", DateTime.UtcNow);

    /// <summary>
    /// Represents completion of all constraints.
    /// </summary>
    public static ConstraintActivation Complete => new("complete", -1, "All constraints completed", DateTime.UtcNow);

    /// <summary>
    /// Creates a constraint activation with current timestamp.
    /// </summary>
    public static ConstraintActivation Create(string constraintId, int layerLevel, string guidance)
    {
        return new ConstraintActivation(constraintId, layerLevel, guidance, DateTime.UtcNow);
    }

    /// <summary>
    /// String representation for debugging and logging.
    /// </summary>
    public override string ToString()
    {
        return $"ConstraintActivation({ConstraintId}, Level: {LayerLevel}, '{Guidance}')";
    }
}
