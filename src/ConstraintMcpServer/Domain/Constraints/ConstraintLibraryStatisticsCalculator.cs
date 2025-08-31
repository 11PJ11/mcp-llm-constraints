using System;
using System.Linq;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Calculates statistics for constraint libraries.
/// Extracted from ConstraintLibrary to follow Single Responsibility Principle.
/// </summary>
internal static class ConstraintLibraryStatisticsCalculator
{
    /// <summary>
    /// Calculates statistics for the given constraint storage.
    /// </summary>
    /// <param name="storage">The constraint storage to analyze</param>
    /// <returns>Library statistics</returns>
    public static LibraryStatistics CalculateStatistics(ConstraintLibraryStorage storage)
    {
        ArgumentNullException.ThrowIfNull(storage);

        int totalConstraints = storage.TotalConstraints;
        int atomicCount = storage.AtomicConstraints.Count;
        int compositeCount = storage.CompositeConstraints.Count;

        double averagePriority = totalConstraints > 0
            ? (storage.AtomicConstraints.Sum(c => c.Priority) + storage.CompositeConstraints.Sum(c => c.Priority)) / totalConstraints
            : 0.0;

        int referenceCount = storage.CompositeConstraints.Sum(c => c.ComponentReferences.Count);

        return new LibraryStatistics
        {
            TotalConstraints = totalConstraints,
            AtomicConstraintCount = atomicCount,
            CompositeConstraintCount = compositeCount,
            AveragePriority = averagePriority,
            ReferenceCount = referenceCount
        };
    }
}
