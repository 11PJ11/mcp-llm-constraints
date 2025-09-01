using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Standard implementation of constraint reminder provider.
/// Contains all standard reminder text for built-in constraint types.
/// </summary>
public sealed class StandardConstraintReminderProvider : IConstraintReminderProvider
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _reminders;

    /// <summary>
    /// Initializes a new instance with standard constraint reminders.
    /// </summary>
    public StandardConstraintReminderProvider()
    {
        _reminders = new Dictionary<string, IReadOnlyList<string>>
        {
            ["tdd"] = new List<string>
            {
                "Start with a failing test (RED) before implementation.",
                "Let the test drive the API design and behavior."
            }.AsReadOnly(),
            
            ["architecture"] = new List<string>
            {
                "Domain layer: pure business logic, no framework dependencies.",
                "Use ports (interfaces) to define infrastructure contracts."
            }.AsReadOnly(),
            
            ["yagni"] = new List<string>
            {
                "Implement only what's needed right now.",
                "Avoid speculative generality and over-engineering."
            }.AsReadOnly()
        };
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetReminders(string constraintType)
    {
        ValidationHelpers.RequireNonEmptyString(constraintType, nameof(constraintType), "Constraint type");
        
        return _reminders.TryGetValue(constraintType.ToLowerInvariant(), out var reminders) 
            ? reminders 
            : Array.Empty<string>();
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAvailableConstraintTypes()
    {
        return _reminders.Keys.ToList().AsReadOnly();
    }
}