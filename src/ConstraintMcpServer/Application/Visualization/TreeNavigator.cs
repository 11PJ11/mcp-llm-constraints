using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Common;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Domain.Visualization;

namespace ConstraintMcpServer.Application.Visualization;

/// <summary>
/// Application service for advanced constraint tree navigation and filtering.
/// Provides search, filtering, and hierarchical navigation capabilities.
/// Follows CUPID properties: Composable, Predictable, Idiomatic, Domain-based.
/// </summary>
public sealed class TreeNavigator
{
    /// <summary>
    /// Finds a constraint by its unique identifier.
    /// </summary>
    /// <param name="library">The constraint library to search</param>
    /// <param name="constraintId">The constraint ID to find</param>
    /// <returns>Result containing the constraint if found, or error if not found</returns>
    public async Task<Result<IConstraint, DomainError>> FindConstraintByIdAsync(ConstraintLibrary library, ConstraintId constraintId)
    {
        var libraryValidation = ValidateLibraryNotNull(library);
        if (libraryValidation.IsError)
        {
            return Result<IConstraint, DomainError>.Failure(libraryValidation.Error);
        }

        var constraint = FindConstraintInLibrary(library, constraintId);

        if (constraint == null)
        {
            return Result<IConstraint, DomainError>.Failure(
                new ValidationError("CONSTRAINT_NOT_FOUND", $"Constraint with ID '{constraintId.Value}' not found"));
        }

        return await Task.FromResult(Result<IConstraint, DomainError>.Success(constraint));
    }

    /// <summary>
    /// Filters constraints by priority range.
    /// </summary>
    /// <param name="library">The constraint library to filter</param>
    /// <param name="minPriority">Minimum priority (inclusive)</param>
    /// <param name="maxPriority">Maximum priority (inclusive)</param>
    /// <returns>Result containing matching constraints or validation error</returns>
    public async Task<Result<IEnumerable<IConstraint>, DomainError>> FilterConstraintsByPriorityAsync(
        ConstraintLibrary library, double minPriority, double maxPriority)
    {
        var libraryValidation = ValidateLibraryNotNull(library);
        if (libraryValidation.IsError)
        {
            return Result<IEnumerable<IConstraint>, DomainError>.Failure(libraryValidation.Error);
        }

        var rangeValidation = ValidatePriorityRange(minPriority, maxPriority);
        if (rangeValidation.IsError)
        {
            return Result<IEnumerable<IConstraint>, DomainError>.Failure(rangeValidation.Error);
        }

        var matchingConstraints = FilterConstraintsByPriority(library, minPriority, maxPriority);
        return await Task.FromResult(Result<IEnumerable<IConstraint>, DomainError>.Success(matchingConstraints));
    }

    /// <summary>
    /// Searches constraints by keyword in ID, title, and trigger keywords.
    /// </summary>
    /// <param name="library">The constraint library to search</param>
    /// <param name="keyword">The keyword to search for</param>
    /// <returns>Result containing matching constraints</returns>
    public async Task<Result<IEnumerable<IConstraint>, DomainError>> SearchConstraintsByKeywordAsync(
        ConstraintLibrary library, string keyword)
    {
        var libraryValidation = ValidateLibraryNotNull(library);
        if (libraryValidation.IsError)
        {
            return Result<IEnumerable<IConstraint>, DomainError>.Failure(libraryValidation.Error);
        }

        var matchingConstraints = SearchConstraintsByKeyword(library, keyword);
        return await Task.FromResult(Result<IEnumerable<IConstraint>, DomainError>.Success(matchingConstraints));
    }

    /// <summary>
    /// Gets the hierarchical path to a constraint.
    /// </summary>
    /// <param name="library">The constraint library</param>
    /// <param name="constraintId">The target constraint ID</param>
    /// <returns>Result containing the path as a sequence of constraints</returns>
    public async Task<Result<IEnumerable<IConstraint>, DomainError>> GetConstraintPathAsync(
        ConstraintLibrary library, ConstraintId constraintId)
    {
        var libraryValidation = ValidateLibraryNotNull(library);
        if (libraryValidation.IsError)
        {
            return Result<IEnumerable<IConstraint>, DomainError>.Failure(libraryValidation.Error);
        }

        var constraint = FindConstraintInLibrary(library, constraintId);
        if (constraint == null)
        {
            return Result<IEnumerable<IConstraint>, DomainError>.Failure(
                new ValidationError("CONSTRAINT_NOT_FOUND", $"Constraint with ID '{constraintId.Value}' not found"));
        }

        var path = BuildConstraintPath(library, constraint);
        return await Task.FromResult(Result<IEnumerable<IConstraint>, DomainError>.Success(path));
    }

    /// <summary>
    /// Gets constraints related to the specified constraint.
    /// </summary>
    /// <param name="library">The constraint library</param>
    /// <param name="constraintId">The constraint to find relations for</param>
    /// <returns>Result containing related constraints</returns>
    public async Task<Result<IEnumerable<IConstraint>, DomainError>> GetRelatedConstraintsAsync(
        ConstraintLibrary library, ConstraintId constraintId)
    {
        var libraryValidation = ValidateLibraryNotNull(library);
        if (libraryValidation.IsError)
        {
            return Result<IEnumerable<IConstraint>, DomainError>.Failure(libraryValidation.Error);
        }

        var constraint = FindConstraintInLibrary(library, constraintId);
        if (constraint == null)
        {
            return Result<IEnumerable<IConstraint>, DomainError>.Failure(
                new ValidationError("CONSTRAINT_NOT_FOUND", $"Constraint with ID '{constraintId.Value}' not found"));
        }

        var relatedConstraints = FindRelatedConstraints(library, constraint);
        return await Task.FromResult(Result<IEnumerable<IConstraint>, DomainError>.Success(relatedConstraints));
    }

    /// <summary>
    /// Generates navigation statistics for the constraint library.
    /// </summary>
    /// <param name="library">The constraint library to analyze</param>
    /// <returns>Result containing navigation statistics</returns>
    public async Task<Result<NavigationStatistics, DomainError>> GetNavigationStatisticsAsync(ConstraintLibrary library)
    {
        var libraryValidation = ValidateLibraryNotNull(library);
        if (libraryValidation.IsError)
        {
            return Result<NavigationStatistics, DomainError>.Failure(libraryValidation.Error);
        }

        var statistics = GenerateNavigationStatistics(library);
        return await Task.FromResult(Result<NavigationStatistics, DomainError>.Success(statistics));
    }

    // Level 1 Refactoring: Extracted validation methods for better readability and reuse
    private static Result<bool, DomainError> ValidateLibraryNotNull(ConstraintLibrary? library)
    {
        if (library == null)
        {
            return Result<bool, DomainError>.Failure(new ValidationError("NULL_LIBRARY", "Constraint library cannot be null"));
        }
        return Result<bool, DomainError>.Success(true);
    }

    private static Result<bool, DomainError> ValidatePriorityRange(double minPriority, double maxPriority)
    {
        if (minPriority > maxPriority)
        {
            return Result<bool, DomainError>.Failure(
                new ValidationError("INVALID_PRIORITY_RANGE", $"Invalid priority range: minimum ({minPriority}) cannot be greater than maximum ({maxPriority})"));
        }
        return Result<bool, DomainError>.Success(true);
    }

    // Level 2 Refactoring: Extracted business logic methods to reduce complexity
    private static IEnumerable<IConstraint> FilterConstraintsByPriority(ConstraintLibrary library, double minPriority, double maxPriority)
    {
        return GetAllConstraintsFromLibrary(library)
            .Where(c => c.Priority >= minPriority && c.Priority <= maxPriority)
            .ToList();
    }

    private static IEnumerable<IConstraint> SearchConstraintsByKeyword(ConstraintLibrary library, string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return GetAllConstraintsFromLibrary(library).ToList();
        }

        return GetAllConstraintsFromLibrary(library)
            .Where(c => ConstraintMatchesKeyword(c, keyword))
            .ToList();
    }

    private static NavigationStatistics GenerateNavigationStatistics(ConstraintLibrary library)
    {
        return new NavigationStatistics(
            TotalConstraints: library.AtomicConstraints.Count + library.CompositeConstraints.Count,
            AtomicConstraintsCount: library.AtomicConstraints.Count,
            CompositeConstraintsCount: library.CompositeConstraints.Count
        );
    }

    // Level 3 Refactoring: Single responsibility helper methods
    private static IConstraint? FindConstraintInLibrary(ConstraintLibrary library, ConstraintId constraintId)
    {
        return FindAtomicConstraint(library, constraintId) ?? FindCompositeConstraint(library, constraintId);
    }

    private static IConstraint? FindAtomicConstraint(ConstraintLibrary library, ConstraintId constraintId)
    {
        return library.AtomicConstraints.FirstOrDefault(c => c.Id.Equals(constraintId));
    }

    private static IConstraint? FindCompositeConstraint(ConstraintLibrary library, ConstraintId constraintId)
    {
        return library.CompositeConstraints.FirstOrDefault(c => c.Id.Equals(constraintId));
    }

    private static IEnumerable<IConstraint> GetAllConstraintsFromLibrary(ConstraintLibrary library)
    {
        return library.AtomicConstraints.Cast<IConstraint>().Concat(library.CompositeConstraints.Cast<IConstraint>());
    }

    private static bool ConstraintMatchesKeyword(IConstraint constraint, string keyword)
    {
        return MatchesInId(constraint, keyword) ||
               MatchesInTitle(constraint, keyword) ||
               MatchesInTriggerKeywords(constraint, keyword);
    }

    private static bool MatchesInId(IConstraint constraint, string keyword)
    {
        return constraint.Id.Value.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesInTitle(IConstraint constraint, string keyword)
    {
        return constraint.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesInTriggerKeywords(IConstraint constraint, string keyword)
    {
        return constraint is AtomicConstraint atomic &&
               atomic.Triggers.Keywords.Any(k => k.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<IConstraint> BuildConstraintPath(ConstraintLibrary library, IConstraint constraint)
    {
        // For this implementation, return single constraint in path
        // In future versions, this could trace hierarchy relationships
        return new[] { constraint };
    }

    private static IEnumerable<IConstraint> FindRelatedConstraints(ConstraintLibrary library, IConstraint constraint)
    {
        return constraint switch
        {
            CompositeConstraint composite => FindChildConstraints(library, composite),
            _ => FindParentConstraints(library, constraint)
        };
    }

    private static IEnumerable<IConstraint> FindChildConstraints(ConstraintLibrary library, CompositeConstraint composite)
    {
        return composite.ComponentReferences
            .Select(reference => FindConstraintInLibrary(library, reference.ConstraintId))
            .Where(c => c != null)
            .Cast<IConstraint>()
            .ToList();
    }

    private static IEnumerable<IConstraint> FindParentConstraints(ConstraintLibrary library, IConstraint constraint)
    {
        return library.CompositeConstraints
            .Where(c => c.ComponentReferences.Any(ref_ => ref_.ConstraintId.Equals(constraint.Id)))
            .Cast<IConstraint>()
            .ToList();
    }
}
