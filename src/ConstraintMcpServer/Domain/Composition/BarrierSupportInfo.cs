namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Represents barrier support information for refactoring levels.
/// Provides additional guidance and support for levels where developers commonly encounter difficulties.
/// </summary>
/// <param name="IsBarrierLevel">Whether this level is identified as a barrier point</param>
/// <param name="AdditionalGuidance">List of additional guidance messages for barrier levels</param>
public sealed record BarrierSupportInfo(
    bool IsBarrierLevel,
    IReadOnlyList<string> AdditionalGuidance)
{
    /// <summary>
    /// Gets the barrier difficulty level based on the guidance provided.
    /// </summary>
    /// <returns>Barrier difficulty level (None, Low, Medium, High)</returns>
    public BarrierDifficulty GetDifficultyLevel()
    {
        if (!IsBarrierLevel)
        {
            return BarrierDifficulty.None;
        }

        return AdditionalGuidance.Count switch
        {
            0 => BarrierDifficulty.None,
            1 or 2 => BarrierDifficulty.Low,
            3 or 4 => BarrierDifficulty.Medium,
            _ => BarrierDifficulty.High
        };
    }

    /// <summary>
    /// Checks if this barrier level requires special attention.
    /// </summary>
    /// <returns>True if barrier requires special attention, false otherwise</returns>
    public bool RequiresSpecialAttention()
    {
        return IsBarrierLevel && AdditionalGuidance.Count > 0;
    }
}

/// <summary>
/// Represents the difficulty level of a refactoring barrier.
/// </summary>
public enum BarrierDifficulty
{
    None,
    Low,
    Medium,
    High
}
