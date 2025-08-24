using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain;

/// <summary>
/// Domain entity representing a constraint for LLM coding agent enforcement.
/// Contains the constraint metadata and reminders for injection during tool calls.
/// </summary>
public sealed class Constraint
{
    /// <summary>
    /// Gets the unique identifier for this constraint.
    /// </summary>
    public ConstraintId Id { get; }

    /// <summary>
    /// Gets the human-readable title of the constraint.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the priority for constraint selection (0.0 to 1.0, higher is more important).
    /// </summary>
    public Priority Priority { get; }

    /// <summary>
    /// Gets the development phases where this constraint applies.
    /// </summary>
    public IReadOnlyList<Phase> Phases { get; }

    /// <summary>
    /// Gets the reminder messages to inject when this constraint is selected.
    /// </summary>
    public IReadOnlyList<string> Reminders { get; }

    /// <summary>
    /// Initializes a new instance of Constraint.
    /// </summary>
    /// <param name="id">The unique constraint identifier.</param>
    /// <param name="title">The human-readable title.</param>
    /// <param name="priority">The selection priority.</param>
    /// <param name="phases">The applicable development phases.</param>
    /// <param name="reminders">The reminder messages for injection.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ValidationException">Thrown when validation rules are violated.</exception>
    public Constraint(
        ConstraintId id,
        string title,
        Priority priority,
        IEnumerable<Phase> phases,
        IEnumerable<string> reminders)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Priority = priority ?? throw new ArgumentNullException(nameof(priority));

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException("Constraint title cannot be empty or whitespace");
        }

        List<Phase> phaseList = phases?.ToList() ?? throw new ArgumentNullException(nameof(phases));
        if (phaseList.Count == 0)
        {
            throw new ValidationException("Constraint must have at least one phase");
        }
        Phases = phaseList.AsReadOnly();

        List<string> reminderList = reminders?.ToList() ?? throw new ArgumentNullException(nameof(reminders));
        if (reminderList.Count == 0)
        {
            throw new ValidationException("Constraint must have at least one reminder");
        }

        if (reminderList.Any(string.IsNullOrWhiteSpace))
        {
            throw new ValidationException("All reminders must be non-empty and not whitespace");
        }

        Reminders = reminderList.AsReadOnly();
    }

    /// <summary>
    /// Checks if this constraint applies to the specified phase.
    /// </summary>
    /// <param name="phase">The phase to check.</param>
    /// <returns>True if the constraint applies to the phase, false otherwise.</returns>
    public bool AppliesTo(Phase phase)
    {
        return phase != null && Phases.Contains(phase);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Id} (Priority: {Priority})";
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Constraint other && Id.Equals(other.Id);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
