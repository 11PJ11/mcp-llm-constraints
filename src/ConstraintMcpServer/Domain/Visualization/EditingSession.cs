using System;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Domain.Visualization;

/// <summary>
/// Represents an active constraint editing session.
/// </summary>
public sealed record EditingSession(
    Guid SessionId,
    ConstraintLibrary Library,
    EditingSessionOptions Options
)
{
    public bool IsActive { get; init; } = true;
}
