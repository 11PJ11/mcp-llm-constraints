using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Provides reminder text for different constraint types.
/// Separates reminder content from constraint creation logic.
/// </summary>
public interface IConstraintReminderProvider
{
    /// <summary>
    /// Gets the reminder messages for a specific constraint type.
    /// </summary>
    /// <param name="constraintType">The type of constraint (e.g., "tdd", "architecture", "yagni").</param>
    /// <returns>Collection of reminder messages for the constraint type.</returns>
    IReadOnlyList<string> GetReminders(string constraintType);

    /// <summary>
    /// Gets all available constraint types that have reminders.
    /// </summary>
    /// <returns>Collection of constraint type identifiers.</returns>
    IReadOnlyList<string> GetAvailableConstraintTypes();
}
