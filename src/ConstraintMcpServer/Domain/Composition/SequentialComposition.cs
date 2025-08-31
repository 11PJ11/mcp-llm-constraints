namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Sequential composition strategy for linear workflows like TDD (RED → GREEN → REFACTOR).
/// Enforces strict ordering and phase transitions based on development context.
/// </summary>
public sealed class SequentialComposition
{
    /// <summary>
    /// Gets the next constraint to activate based on current TDD phase and test status.
    /// </summary>
    /// <param name="context">The current composition context</param>
    /// <returns>Result containing next constraint ID and activation reason</returns>
    public SequentialCompositionResult GetNextConstraintId(CompositionStrategyContext context)
    {
        if (context == null)
        {
            return SequentialCompositionResult.Failure("Context cannot be null");
        }

        return context.CurrentPhase switch
        {
            TddPhase.Red when context.TestStatus == TestStatus.NotRun =>
                SequentialCompositionResult.Success(
                    "tdd.write-failing-test",
                    "RED phase: Start by writing a failing test"),

            TddPhase.Red when context.TestStatus == TestStatus.Failing =>
                SequentialCompositionResult.Success(
                    "tdd.write-simplest-code",
                    "GREEN phase: Write simplest code to make test pass"),

            TddPhase.Green when context.TestStatus == TestStatus.Passing =>
                SequentialCompositionResult.Success(
                    "tdd.refactor-code",
                    "REFACTOR phase: Improve code design while keeping tests green"),

            _ => SequentialCompositionResult.Failure(
                $"Invalid TDD phase transition: {context.CurrentPhase} with {context.TestStatus}")
        };
    }
}
