using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Tests.Infrastructure;

/// <summary>
/// Fluent builder for BDD-style test scenarios with Given-When-Then lambda pattern.
/// Enables readable, composable test scenarios for Outside-In TDD acceptance tests.
/// Follows the builder pattern with immutable step accumulation.
/// </summary>
public sealed class ScenarioBuilder
{
    private readonly ImmutableList<Func<Task>> _givenSteps;
    private readonly ImmutableList<Func<Task>> _whenSteps;
    private readonly ImmutableList<Func<Task>> _thenSteps;

    public ScenarioBuilder() : this(
        ImmutableList<Func<Task>>.Empty,
        ImmutableList<Func<Task>>.Empty,
        ImmutableList<Func<Task>>.Empty)
    {
    }

    private ScenarioBuilder(
        ImmutableList<Func<Task>> givenSteps,
        ImmutableList<Func<Task>> whenSteps,
        ImmutableList<Func<Task>> thenSteps)
    {
        _givenSteps = givenSteps;
        _whenSteps = whenSteps;
        _thenSteps = thenSteps;
    }

    /// <summary>
    /// Add a Given step to set up test preconditions
    /// </summary>
    public ScenarioBuilder Given(Func<Task> step)
    {
        return new ScenarioBuilder(
            _givenSteps.Add(step),
            _whenSteps,
            _thenSteps);
    }

    /// <summary>
    /// Add an And step to the current phase (works with Given, When, or Then)
    /// </summary>
    public ScenarioBuilder And(Func<Task> step)
    {
        // Determine current phase and add to appropriate list
        if (_thenSteps.Count > 0)
        {
            // Currently in Then phase
            return new ScenarioBuilder(_givenSteps, _whenSteps, _thenSteps.Add(step));
        }
        else if (_whenSteps.Count > 0)
        {
            // Currently in When phase
            return new ScenarioBuilder(_givenSteps, _whenSteps.Add(step), _thenSteps);
        }
        else
        {
            // Currently in Given phase (default)
            return new ScenarioBuilder(_givenSteps.Add(step), _whenSteps, _thenSteps);
        }
    }

    /// <summary>
    /// Add a When step to execute the system under test
    /// </summary>
    public ScenarioBuilder When(Func<Task> step)
    {
        return new ScenarioBuilder(
            _givenSteps,
            _whenSteps.Add(step),
            _thenSteps);
    }

    /// <summary>
    /// Add a Then step to verify expected outcomes
    /// </summary>
    public ScenarioBuilder Then(Func<Task> step)
    {
        return new ScenarioBuilder(
            _givenSteps,
            _whenSteps,
            _thenSteps.Add(step));
    }

    /// <summary>
    /// Execute all accumulated steps in Given-When-Then order.
    /// Returns Task for async test execution with proper error handling.
    /// </summary>
    public async Task ExecuteAsync()
    {
        try
        {
            // Execute Given steps (test setup and preconditions)
            foreach (var givenStep in _givenSteps)
            {
                await givenStep();
            }

            // Execute When steps (system actions and triggers)
            foreach (var whenStep in _whenSteps)
            {
                await whenStep();
            }

            // Execute Then steps (assertions and verifications)
            foreach (var thenStep in _thenSteps)
            {
                await thenStep();
            }
        }
        catch (Exception ex)
        {
            // Enhance exception with scenario context for better debugging
            throw new ScenarioExecutionException(
                $"Scenario execution failed. Given: {_givenSteps.Count}, When: {_whenSteps.Count}, Then: {_thenSteps.Count}",
                ex);
        }
    }
}

/// <summary>
/// Exception thrown when scenario execution fails, providing additional context for debugging
/// </summary>
public sealed class ScenarioExecutionException : Exception
{
    public ScenarioExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
