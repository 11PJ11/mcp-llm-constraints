using System.Threading.Tasks;
using ConstraintMcpServer.Tests.Framework;
using ConstraintMcpServer.Tests.Steps;
using NUnit.Framework;
using static ConstraintMcpServer.Tests.Framework.ScenarioBuilder;

namespace ConstraintMcpServer.Tests.E2E;

/// <summary>
/// BDD-style end-to-end tests for pure library-based constraint system.
/// Focuses on business value: Constraint library with ID-based references and composable architecture.
/// No backward compatibility - pure v2.0 library architecture only.
/// </summary>
[TestFixture]
public class LibraryBasedConstraintSystemE2E
{
    private McpServerSteps? _steps;
    private LibraryConstraintSteps? _librarySteps;

    [SetUp]
    public void SetUp()
    {
        _steps = new McpServerSteps();
        _librarySteps = new LibraryConstraintSteps();

        // Establish coordination between step classes
        _librarySteps.SetMcpServerSteps(_steps);
    }

    [TearDown]
    public void TearDown()
    {
        // Deterministic cleanup ordering to prevent resource conflicts
        try
        {
            _steps?.Dispose();
            _librarySteps?.Dispose();
        }
        catch (Exception ex)
        {
            // Log cleanup errors but don't fail the test
            System.Console.WriteLine($"Warning: Test cleanup encountered error: {ex.Message}");
        }
        finally
        {
            _steps = null;
            _librarySteps = null;
        }
    }

    [Test]
    public async Task Constraint_Library_Should_Resolve_Atomic_Constraints_By_ID()
    {
        // Scenario: Atomic constraints are defined in constraint library and resolved by ID
        // Business value: As a constraint pack author, I need to define atomic constraints once
        // in a library and reference them by ID across multiple compositions, so I can
        // maintain consistency and reusability across constraint definitions

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .And(_librarySteps!.ConstraintLibraryWithAtomicConstraintsExists)
            .And(_steps.StartServerWithConfiguration)
            .When(_librarySteps.AtomicConstraintIsReferencedById)
            .Then(_librarySteps.ConstraintLibraryResolvesAtomicConstraint)
            .And(_librarySteps.ResolvedConstraintHasCorrectTriggers)
            .And(_librarySteps.ResolvedConstraintHasCorrectReminders)
            .And(_steps.P95LatencyIsWithinBudget)
            .ExecuteAsync();

        // So that I can build reusable constraint libraries with single source of truth
        // for each constraint definition, enabling better maintainability and consistency
        // (This is achieved by the successful completion of the above steps)
    }

    [Test]
    public async Task Composite_Constraints_Should_Reference_Library_Components_By_ID()
    {
        // Scenario: Composite constraints reference atomic constraints from library by ID only
        // Business value: As a constraint pack author, I need to compose methodologies by
        // referencing existing atomic constraints from the library rather than defining them
        // inline, so I can maintain consistency and enable independent evolution of components

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .And(_librarySteps!.ConstraintLibraryWithAtomicConstraintsExists)
            .And(_librarySteps.CompositeConstraintWithReferencesExists)
            .And(_steps.StartServerWithConfiguration)
            .When(_librarySteps.CompositeConstraintIsActivated)
            .Then(_librarySteps.LibraryResolvesReferencedComponents)
            .And(_librarySteps.ResolvedComponentsHaveCorrectSequence)
            .And(_librarySteps.CompositionLogicCoordinatesResolvedComponents)
            .And(_steps.P95LatencyIsWithinBudget)
            .ExecuteAsync();

        // So that I can build maintainable composite constraints that reference
        // a single source of truth for each atomic constraint definition
        // (This is achieved by the successful completion of the above steps)
    }

    [Test]
    public async Task Constraint_Library_Should_Detect_Circular_References()
    {
        // Scenario: Library prevents circular references in composite constraint definitions
        // Business value: As a constraint pack author, I need the system to detect and prevent
        // circular references when composing constraints, so I can avoid infinite loops
        // and ensure constraint resolution completes successfully

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .And(_librarySteps!.ConstraintLibraryWithCircularReferencesExists)
            .When(_steps.StartServerWithConfiguration)
            .Then(_librarySteps.CircularReferenceIsDetected)
            .And(_librarySteps.ValidationErrorIsReported)
            .And(_librarySteps.ServerFailsToStartWithCircularReferences)
            .ExecuteAsync();

        // So that I can build reliable constraint libraries without the risk of
        // infinite loops during constraint resolution and activation
        // (This is achieved by the successful completion of the above steps)
    }

    [Test]
    public async Task Constraint_Library_Should_Enable_High_Performance_Resolution()
    {
        // Scenario: Library-based constraint resolution maintains <50ms p95 performance
        // Business value: As a developer, I need constraint resolution to be fast enough
        // that it doesn't interrupt my coding flow, even with complex constraint libraries
        // containing many atomic and composite constraints

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .And(_librarySteps!.LargeConstraintLibraryExists)
            .And(_steps.StartServerWithConfiguration)
            .When(_librarySteps.MultipleConstraintsAreResolvedConcurrently)
            .Then(_librarySteps.AllConstraintsResolveSuccessfully)
            .And(_librarySteps.ResolutionTimeIsWithinPerformanceBudget)
            .And(_librarySteps.CachingOptimizesRepeatedResolution)
            .And(_steps.P95LatencyIsWithinBudget)
            .ExecuteAsync();

        // So that I can use rich constraint libraries without experiencing
        // noticeable delays during constraint activation and resolution
        // (This is achieved by the successful completion of the above steps)
    }

    [Test]
    public async Task Constraint_Library_Should_Support_Independent_Component_Evolution()
    {
        // Scenario: Atomic constraints can be updated without affecting composites that reference them
        // Business value: As a constraint pack maintainer, I need to be able to update atomic
        // constraint definitions (triggers, reminders, priorities) without breaking existing
        // composite constraints, so I can evolve constraints independently

        await Given(_steps!.RepositoryBuildsSuccessfully)
            .And(_librarySteps!.ConstraintLibraryWithAtomicConstraintsExists)
            .And(_librarySteps.CompositeConstraintWithReferencesExists)
            .And(_steps.StartServerWithConfiguration)
            .When(_librarySteps.AtomicConstraintDefinitionIsUpdated)
            .Then(_librarySteps.CompositeConstraintStillWorksCorrectly)
            .And(_librarySteps.UpdatedDefinitionIsUsedInComposition)
            .And(_librarySteps.NoBreakingChangesInCompositeLogic)
            .And(_steps.P95LatencyIsWithinBudget)
            .ExecuteAsync();

        // So that I can maintain and improve constraint libraries over time
        // without breaking existing constraint compositions and methodologies
        // (This is achieved by the successful completion of the above steps)
    }
}
