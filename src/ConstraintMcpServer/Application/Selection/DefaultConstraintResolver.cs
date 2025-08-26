using System;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Application.Selection;

/// <summary>
/// Default constraint resolver implementation for testing and development.
/// Will be replaced with proper constraint library integration.
/// </summary>
internal sealed class DefaultConstraintResolver : IConstraintResolver
{
    public IConstraint ResolveConstraint(ConstraintId constraintId)
    {
        throw new NotImplementedException("Constraint resolution not yet implemented");
    }

    public IResolutionMetrics GetResolutionMetrics()
    {
        throw new NotImplementedException("Resolution metrics not yet implemented");
    }
}
