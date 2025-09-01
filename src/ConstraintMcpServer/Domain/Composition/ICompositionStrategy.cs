namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Interface for composition strategies that coordinate constraint activation.
/// Each strategy implements a different approach to orchestrating constraints.
/// </summary>
public interface ICompositionStrategy
{
    /// <summary>
    /// Gets the composition type handled by this strategy.
    /// </summary>
    CompositionType Type { get; }
}
