using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Shared validation rules for constraint properties.
/// Extracted to eliminate duplication between AtomicConstraint and CompositeConstraintValidator.
/// Level 2 Refactoring: Eliminates duplicate validation logic across classes.
/// </summary>
internal static class ConstraintValidationRules
{
    /// <summary>
    /// Validates that the title is not null or whitespace.
    /// </summary>
    /// <param name="title">The constraint title to validate</param>
    /// <exception cref="ValidationException">Thrown when title is invalid</exception>
    public static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException("Constraint title cannot be empty or whitespace");
        }
    }

    /// <summary>
    /// Validates that priority is within valid range [0.0, 1.0].
    /// </summary>
    /// <param name="priority">The constraint priority to validate</param>
    /// <exception cref="ValidationException">Thrown when priority is invalid</exception>
    public static void ValidatePriority(double priority)
    {
        if (priority < 0.0 || priority > 1.0)
        {
            throw new ValidationException("Constraint priority must be between 0.0 and 1.0");
        }
    }

    /// <summary>
    /// Validates that reminders collection contains valid non-empty strings.
    /// </summary>
    /// <param name="reminders">The reminders collection to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when reminders is null</exception>
    /// <exception cref="ValidationException">Thrown when reminders are invalid</exception>
    public static void ValidateReminders(IEnumerable<string> reminders)
    {
        ArgumentNullException.ThrowIfNull(reminders);

        var reminderList = reminders.ToList();
        if (reminderList.Count == 0)
        {
            throw new ValidationException("Constraint must have at least one reminder");
        }

        if (reminderList.Any(string.IsNullOrWhiteSpace))
        {
            throw new ValidationException("All reminders must be non-empty and not whitespace");
        }
    }
}
