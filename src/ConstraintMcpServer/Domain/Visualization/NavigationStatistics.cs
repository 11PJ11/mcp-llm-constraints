namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Immutable statistics about constraint library navigation.
/// Provides insights into library size and composition for navigation purposes.
/// Follows CUPID properties: Composable, Predictable, Idiomatic, Domain-based.
/// </summary>
/// <param name="TotalConstraints">Total number of constraints in the library</param>
/// <param name="AtomicConstraintsCount">Number of atomic constraints</param>
/// <param name="CompositeConstraintsCount">Number of composite constraints</param>
public sealed record NavigationStatistics(
    int TotalConstraints,
    int AtomicConstraintsCount,
    int CompositeConstraintsCount
)
{
    /// <summary>
    /// Gets the percentage of atomic constraints in the library.
    /// </summary>
    public double AtomicPercentage => TotalConstraints > 0 ? (double)AtomicConstraintsCount / TotalConstraints * 100.0 : 0.0;

    /// <summary>
    /// Gets the percentage of composite constraints in the library.
    /// </summary>
    public double CompositePercentage => TotalConstraints > 0 ? (double)CompositeConstraintsCount / TotalConstraints * 100.0 : 0.0;

    /// <summary>
    /// Determines if the library has a balanced composition of constraint types.
    /// </summary>
    public bool HasBalancedComposition => TotalConstraints > 0 && 
                                          AtomicPercentage >= 30.0 && 
                                          CompositePercentage >= 30.0;

    /// <summary>
    /// Determines if the library is large enough to benefit from advanced navigation.
    /// </summary>
    public bool BenefitsFromAdvancedNavigation => TotalConstraints >= 10;
}