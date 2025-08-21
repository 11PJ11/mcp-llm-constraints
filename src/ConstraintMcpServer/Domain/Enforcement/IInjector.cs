namespace ConstraintMcpServer.Domain.Enforcement;

using ConstraintMcpServer.Domain.Constraints;

/// <summary>
/// Domain interface for injecting constraint reminders at tool boundaries.
/// Part of the Enforcement bounded context.
/// </summary>
internal interface IInjector
{
    /// <summary>
    /// Injects constraint reminders into an MCP request.
    /// </summary>
    /// <param name="originalRequest">Original MCP request</param>
    /// <param name="selectedConstraints">Constraints to inject</param>
    /// <param name="context">Current interaction context</param>
    /// <returns>Modified request with injected constraint reminders</returns>
    string InjectConstraints(string originalRequest, IReadOnlyList<Constraint> selectedConstraints, InteractionContext context);

    /// <summary>
    /// Creates anchor prologue for constraint injection.
    /// </summary>
    /// <param name="context">Current interaction context</param>
    /// <returns>Anchor prologue text</returns>
    string CreateAnchorPrologue(InteractionContext context);

    /// <summary>
    /// Creates anchor epilogue for constraint injection.
    /// </summary>
    /// <param name="context">Current interaction context</param>
    /// <returns>Anchor epilogue text</returns>
    string CreateAnchorEpilogue(InteractionContext context);

    /// <summary>
    /// Formats constraint reminders for injection.
    /// </summary>
    /// <param name="constraints">Constraints to format</param>
    /// <param name="context">Current interaction context</param>
    /// <returns>Formatted reminder text</returns>
    string FormatConstraintReminders(IReadOnlyList<Constraint> constraints, InteractionContext context);
}