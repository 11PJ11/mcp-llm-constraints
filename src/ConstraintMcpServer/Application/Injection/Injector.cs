using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConstraintMcpServer.Domain;

namespace ConstraintMcpServer.Application.Injection;

/// <summary>
/// Formats constraint messages with anchors and reminders for injection.
/// Creates the final constraint text that gets injected into MCP responses.
/// </summary>
public sealed class Injector
{
    /// <summary>
    /// Formats a complete constraint message with anchors and reminders.
    /// </summary>
    /// <param name="constraints">Selected constraints to include in the message.</param>
    /// <param name="interactionNumber">Current interaction number for context.</param>
    /// <returns>Formatted constraint message with anchors and reminders.</returns>
    public string FormatConstraintMessage(IReadOnlyList<Constraint> constraints, int interactionNumber)
    {
        var message = new StringBuilder();

        // Add processing context with constraint marker for test compatibility
        message.AppendLine($"Tool call {interactionNumber} processed. CONSTRAINT:");
        message.AppendLine();

        // Add anchor prologue
        message.AppendLine("Remember: Test-first, boundaries matter, YAGNI applies.");
        message.AppendLine();

        // Add constraint reminders
        if (constraints.Any())
        {
            foreach (Constraint constraint in constraints)
            {
                foreach (string reminder in constraint.Reminders)
                {
                    message.AppendLine($"• {reminder}");
                }
            }
            message.AppendLine();
        }

        // Add anchor epilogue
        message.AppendLine("Before commit: All tests green? Architecture clean?");

        return message.ToString().Trim();
    }
}
