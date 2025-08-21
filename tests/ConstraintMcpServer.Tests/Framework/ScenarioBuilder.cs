using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Tests.Framework;

/// <summary>
/// Fluent interface for building BDD-style test scenarios with lambda expressions.
/// Enables readable test specifications using given().and().when().then().and() syntax.
/// </summary>
public class ScenarioBuilder
{
    private readonly List<ScenarioStep> _givenSteps = new();
    private readonly List<ScenarioStep> _whenSteps = new();
    private readonly List<ScenarioStep> _thenSteps = new();

    /// <summary>
    /// Start building a scenario with an initial context step.
    /// </summary>
    public static GivenBuilder Given(Action step)
    {
        var builder = new ScenarioBuilder();
        builder._givenSteps.Add(new ScenarioStep(step));
        return new GivenBuilder(builder);
    }

    /// <summary>
    /// Start building a scenario with an async context step.
    /// </summary>
    public static GivenBuilder Given(Func<Task> step)
    {
        var builder = new ScenarioBuilder();
        builder._givenSteps.Add(new ScenarioStep(step));
        return new GivenBuilder(builder);
    }

    /// <summary>
    /// Execute all steps in the scenario in order.
    /// </summary>
    public async Task ExecuteAsync()
    {
        // Execute Given steps
        foreach (ScenarioStep step in _givenSteps)
        {
            await step.ExecuteAsync();
        }

        // Execute When steps
        foreach (ScenarioStep step in _whenSteps)
        {
            await step.ExecuteAsync();
        }

        // Execute Then steps
        foreach (ScenarioStep step in _thenSteps)
        {
            await step.ExecuteAsync();
        }
    }

    /// <summary>
    /// Builder for the Given (context) phase of the scenario.
    /// </summary>
    public class GivenBuilder
    {
        private readonly ScenarioBuilder _scenario;

        internal GivenBuilder(ScenarioBuilder scenario)
        {
            _scenario = scenario;
        }

        /// <summary>
        /// Add another context step.
        /// </summary>
        public GivenBuilder And(Action step)
        {
            _scenario._givenSteps.Add(new ScenarioStep(step));
            return this;
        }

        /// <summary>
        /// Add another async context step.
        /// </summary>
        public GivenBuilder And(Func<Task> step)
        {
            _scenario._givenSteps.Add(new ScenarioStep(step));
            return this;
        }

        /// <summary>
        /// Move to the When (trigger) phase.
        /// </summary>
        public WhenBuilder When(Action step)
        {
            _scenario._whenSteps.Add(new ScenarioStep(step));
            return new WhenBuilder(_scenario);
        }

        /// <summary>
        /// Move to the When (trigger) phase with async step.
        /// </summary>
        public WhenBuilder When(Func<Task> step)
        {
            _scenario._whenSteps.Add(new ScenarioStep(step));
            return new WhenBuilder(_scenario);
        }
    }

    /// <summary>
    /// Builder for the When (trigger) phase of the scenario.
    /// </summary>
    public class WhenBuilder
    {
        private readonly ScenarioBuilder _scenario;

        internal WhenBuilder(ScenarioBuilder scenario)
        {
            _scenario = scenario;
        }

        /// <summary>
        /// Add another trigger step.
        /// </summary>
        public WhenBuilder And(Action step)
        {
            _scenario._whenSteps.Add(new ScenarioStep(step));
            return this;
        }

        /// <summary>
        /// Add another async trigger step.
        /// </summary>
        public WhenBuilder And(Func<Task> step)
        {
            _scenario._whenSteps.Add(new ScenarioStep(step));
            return this;
        }

        /// <summary>
        /// Move to the Then (outcome) phase.
        /// </summary>
        public ThenBuilder Then(Action step)
        {
            _scenario._thenSteps.Add(new ScenarioStep(step));
            return new ThenBuilder(_scenario);
        }

        /// <summary>
        /// Move to the Then (outcome) phase with async step.
        /// </summary>
        public ThenBuilder Then(Func<Task> step)
        {
            _scenario._thenSteps.Add(new ScenarioStep(step));
            return new ThenBuilder(_scenario);
        }
    }

    /// <summary>
    /// Builder for the Then (outcome) phase of the scenario.
    /// </summary>
    public class ThenBuilder
    {
        private readonly ScenarioBuilder _scenario;

        internal ThenBuilder(ScenarioBuilder scenario)
        {
            _scenario = scenario;
        }

        /// <summary>
        /// Add another outcome step.
        /// </summary>
        public ThenBuilder And(Action step)
        {
            _scenario._thenSteps.Add(new ScenarioStep(step));
            return this;
        }

        /// <summary>
        /// Add another async outcome step.
        /// </summary>
        public ThenBuilder And(Func<Task> step)
        {
            _scenario._thenSteps.Add(new ScenarioStep(step));
            return this;
        }

        /// <summary>
        /// Execute the complete scenario.
        /// </summary>
        public async Task ExecuteAsync()
        {
            await _scenario.ExecuteAsync();
        }
    }

    /// <summary>
    /// Represents a single step in the scenario.
    /// </summary>
    private class ScenarioStep
    {
        private readonly Action? _syncAction;
        private readonly Func<Task>? _asyncAction;

        public ScenarioStep(Action action)
        {
            _syncAction = action;
        }

        public ScenarioStep(Func<Task> action)
        {
            _asyncAction = action;
        }

        public async Task ExecuteAsync()
        {
            if (_asyncAction != null)
            {
                await _asyncAction();
            }
            else if (_syncAction != null)
            {
                _syncAction();
            }
        }
    }
}
