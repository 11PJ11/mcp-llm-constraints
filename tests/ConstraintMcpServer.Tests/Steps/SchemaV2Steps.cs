using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Tests.Steps;

/// <summary>
/// Business-focused steps for Schema v2.0 migration testing.
/// Encapsulates trigger-based constraint system and composable architecture implementation.
/// </summary>
public class SchemaV2Steps : IDisposable
{
    private string? _configPath;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private JsonDocument? _lastConstraintResponse;
#pragma warning restore CS0649
    private string? _lastTriggerContext;
    private bool _compositeActivated;
    private bool _atomicActivated;

    // Business-focused step: Atomic constraint configuration exists
    public void AtomicConstraintConfigurationExists()
    {
        string configDir = Path.Combine(GetProjectRoot(), "config");
        _configPath = Path.Combine(configDir, "atomic-constraints-v2.yaml");

        // Create v2.0 atomic constraint configuration for testing
        string atomicYaml = """
            version: "0.2.0"
            schema_type: "atomic"
            constraints:
              - id: "testing.write-test-first"
                title: "Write a failing test before implementation"
                type: "atomic"
                priority: 0.92
                triggers:
                  keywords: ["test", "unit test", "failing test", "red phase"]
                  file_patterns: ["*Test.cs", "*Tests.cs", "*Spec.cs"]
                  context_patterns: ["testing", "tdd", "red-green-refactor"]
                  anti_patterns: ["hotfix", "emergency", "production-issue"]
                reminders:
                  - "Start with a failing test (RED) before writing implementation code."
                  - "Ensure your test fails for the right reason before implementing."
              - id: "architecture.single-responsibility"
                title: "Each class should have a single reason to change"
                type: "atomic"
                priority: 0.88
                triggers:
                  keywords: ["class", "responsibility", "single responsibility"]
                  file_patterns: ["*.cs", "*.ts", "*.js"]
                  context_patterns: ["refactoring", "design", "architecture"]
                  anti_patterns: ["quick-fix", "temporary"]
                reminders:
                  - "Ensure each class has only one reason to change."
                  - "Extract classes if multiple responsibilities are detected."
            """;

        Directory.CreateDirectory(configDir);
        File.WriteAllText(_configPath, atomicYaml);
    }

    // Business-focused step: Composite constraint configuration exists
    public void CompositeConstraintConfigurationExists()
    {
        string configDir = Path.Combine(GetProjectRoot(), "config");
        _configPath = Path.Combine(configDir, "composite-constraints-v2.yaml");

        // Create v2.0 composite constraint configuration for testing
        string compositeYaml = """
            version: "0.2.0"
            schema_type: "composite"
            constraints:
              - id: "methodology.outside-in-development"
                title: "Outside-In Development with ATDD, BDD, and TDD"
                type: "composite"
                priority: 0.95
                composition_type: "sequential"
                triggers:
                  keywords: ["outside-in", "atdd", "acceptance test", "behavior driven"]
                  file_patterns: ["*E2E.cs", "*Integration.cs", "*AcceptanceTest.cs"]
                  context_patterns: ["outside-in", "atdd", "bdd", "acceptance-driven"]
                components:
                  - id: "testing.acceptance-test-first"
                    title: "Write failing acceptance test first"
                    type: "atomic"
                    priority: 0.92
                    sequence_order: 1
                    triggers:
                      keywords: ["acceptance", "scenario", "feature"]
                      context_patterns: ["atdd", "bdd", "gherkin"]
                    reminders:
                      - "Start with a failing acceptance test that describes the business scenario."
                      - "Use Given-When-Then structure for clarity."
                  - id: "testing.unit-test-driven"
                    title: "Inner TDD loop for implementation"
                    type: "atomic"
                    priority: 0.90
                    sequence_order: 2
                    triggers:
                      keywords: ["unit test", "tdd", "red-green-refactor"]
                      context_patterns: ["implementation", "inner-loop"]
                    reminders:
                      - "Write failing unit test for smallest behavior."
                      - "Implement minimum code to make test pass."
                      - "Refactor while keeping tests green."
                reminders:
                  - "Outside-In Development: Acceptance test drives inner TDD loops."
                  - "Each acceptance test scenario drives multiple unit test cycles."
            """;

        Directory.CreateDirectory(configDir);
        File.WriteAllText(_configPath, compositeYaml);
    }

    // Business-focused step: Trigger-based configuration exists
    public void TriggerBasedConfigurationExists()
    {
        string configDir = Path.Combine(GetProjectRoot(), "config");
        _configPath = Path.Combine(configDir, "trigger-based-v2.yaml");

        // Create v2.0 configuration focused on trigger system
        string triggerYaml = """
            version: "0.2.0"
            schema_type: "mixed"
            constraints:
              - id: "clean-arch.dependency-inversion"
                title: "Domain must not depend on Infrastructure"
                type: "atomic"
                priority: 0.89
                triggers:
                  keywords: ["dependency", "import", "using", "infrastructure"]
                  file_patterns: ["Domain/*.cs", "Core/*.cs"]
                  context_patterns: ["architecture", "dependency-management"]
                  confidence_threshold: 0.7
                reminders:
                  - "Domain layer should not reference Infrastructure layer."
                  - "Use dependency injection to invert dependencies."
              - id: "performance.async-best-practices"
                title: "Use async/await properly for I/O operations"
                type: "atomic" 
                priority: 0.75
                triggers:
                  keywords: ["async", "await", "task", "database", "http", "file"]
                  file_patterns: ["*Service.cs", "*Repository.cs", "*Controller.cs"]
                  context_patterns: ["io-operation", "database", "web-request"]
                reminders:
                  - "Use async/await for I/O bound operations."
                  - "Don't use async for CPU-bound work unless using Task.Run."
            """;

        Directory.CreateDirectory(configDir);
        File.WriteAllText(_configPath, triggerYaml);
    }

    // Business-focused step: Legacy v1.0 configuration exists
    public void LegacyV1ConfigurationExists()
    {
        // Use the existing constraints.yaml which is v1.0 format
        string configDir = Path.Combine(GetProjectRoot(), "config");
        _configPath = Path.Combine(configDir, "constraints.yaml");

        if (!File.Exists(_configPath))
        {
            throw new InvalidOperationException($"Legacy v1.0 configuration not found at: {_configPath}");
        }

        // Verify it's v1.0 format with phases instead of triggers
        string content = File.ReadAllText(_configPath);
        if (!content.Contains("phases:") || content.Contains("triggers:"))
        {
            throw new InvalidOperationException("Configuration is not in v1.0 format (should have 'phases' not 'triggers')");
        }
    }

    // Business-focused step: Multi-methodology configuration exists  
    public void MultiMethodologyConfigurationExists()
    {
        string configDir = Path.Combine(GetProjectRoot(), "config");
        _configPath = Path.Combine(configDir, "multi-methodology-v2.yaml");

        string multiMethodologyYaml = """
            version: "0.2.0"
            schema_type: "mixed"
            constraints:
              - id: "clean-architecture.layers"
                title: "Clean Architecture layer dependencies"
                type: "composite"
                composition_type: "hierarchical"
                priority: 0.91
                triggers:
                  keywords: ["clean architecture", "layers", "dependency"]
                  context_patterns: ["architecture", "clean-arch", "layering"]
                components:
                  - id: "clean-arch.dependency-rule"
                  - id: "clean-arch.interface-segregation"
              - id: "hexagonal-architecture.ports-adapters"
                title: "Hexagonal Architecture ports and adapters"
                type: "atomic"
                priority: 0.87
                triggers:
                  keywords: ["hexagonal", "ports", "adapters", "boundary"]
                  file_patterns: ["*Port.cs", "*Adapter.cs", "I*Repository.cs"]
                  context_patterns: ["hexagonal", "ports-adapters", "boundaries"]
                reminders:
                  - "Define ports (interfaces) at application boundary."
                  - "Implement adapters for external dependencies."
              - id: "feature-driven.feature-focus"
                title: "Feature-Driven Development practices"
                type: "atomic"
                priority: 0.83
                triggers:
                  keywords: ["feature", "user story", "business value"]
                  context_patterns: ["fdd", "feature-driven", "user-story"]
                reminders:
                  - "Focus on delivering complete features with business value."
                  - "Design around features, not technical layers."
            """;

        Directory.CreateDirectory(configDir);
        File.WriteAllText(_configPath, multiMethodologyYaml);
    }

    // Business-focused step: Trigger context matches atomic constraint
    public void TriggerContextMatchesAtomicConstraint()
    {
        // Simulate context that should match atomic constraint triggers
        _lastTriggerContext = "writing unit test for authentication service";
        
        // This step sets up the context that the trigger matching engine will evaluate
        // In the actual implementation, this would come from the MCP tool call context
    }

    // Business-focused step: Trigger context matches composite constraint
    public void TriggerContextMatchesCompositeConstraint()
    {
        // Simulate context that should match composite constraint triggers
        _lastTriggerContext = "implementing outside-in development with acceptance tests";
        
        // This step sets up the context for composite constraint activation testing
    }

    // Business-focused step: Context contains trigger keywords
    public void ContextContainsTriggerKeywords()
    {
        // Simulate context with keywords that should activate trigger-based constraints
        _lastTriggerContext = "refactoring class with single responsibility principle";
        
        // This context should match the architecture.single-responsibility constraint
    }

    // Business-focused step: Clean Architecture context detected
    public void CleanArchitectureContextDetected()
    {
        _lastTriggerContext = "implementing clean architecture with domain layer";
    }

    // Business-focused step: Hexagonal Architecture context detected  
    public void HexagonalArchitectureContextDetected()
    {
        _lastTriggerContext = "creating ports and adapters for hexagonal architecture";
    }

    // Business-focused step: Feature-Driven context detected
    public void FeatureDrivenContextDetected()
    {
        _lastTriggerContext = "developing user story with business value focus";
    }

    // Business-focused validation: Atomic constraint activates correctly
    public void AtomicConstraintActivatesCorrectly()
    {
        // This will fail initially and drive implementation
        // Should verify that atomic constraint activation logic works
        _atomicActivated = false; // Will be set to true when implementation works
        
        if (!_atomicActivated)
        {
            throw new InvalidOperationException($"Atomic constraint failed to activate for context: {_lastTriggerContext}");
        }
    }

    // Business-focused validation: Composite constraint activates correctly
    public void CompositeConstraintActivatesCorrectly()
    {
        // This will fail initially and drive implementation
        _compositeActivated = false; // Will be set to true when implementation works
        
        if (!_compositeActivated)
        {
            throw new InvalidOperationException($"Composite constraint failed to activate for context: {_lastTriggerContext}");
        }
    }

    // Business-focused validation: Constraint reminder contains atomic content
    public void ConstraintReminderContainsAtomicContent()
    {
        // This will fail initially and drive implementation
        // Should verify reminder content comes from atomic constraint
        throw new InvalidOperationException("Atomic constraint reminder validation not yet implemented");
    }

    // Business-focused validation: Atomic components activate in sequence
    public void AtomicComponentsActivateInSequence()
    {
        // This will fail initially and drive implementation
        // Should verify sequential composition logic works correctly
        throw new InvalidOperationException("Sequential atomic component activation not yet implemented");
    }

    // Business-focused validation: Composition logic coordinates correctly
    public void CompositionLogicCoordinatesCorrectly()
    {
        // This will fail initially and drive implementation
        // Should verify composite constraint coordination works
        throw new InvalidOperationException("Composition coordination logic not yet implemented");
    }

    // Business-focused validation: Trigger matching engine activates constraints
    public void TriggerMatchingEngineActivatesConstraints()
    {
        // This will fail initially and drive implementation
        // Should verify trigger matching replaces phase-based activation
        throw new InvalidOperationException("Trigger matching engine activation not yet implemented");
    }

    // Business-focused validation: No phase-based activation occurs
    public void NoPhaseBasedActivationOccurs()
    {
        // This will fail initially and drive implementation
        // Should verify that old phase-based system is not used
        throw new InvalidOperationException("Phase-based activation exclusion not yet implemented");
    }

    // Business-focused validation: Context relevance score is accurate
    public void ContextRelevanceScoreIsAccurate()
    {
        // This will fail initially and drive implementation
        // Should verify relevance scoring algorithm works correctly
        throw new InvalidOperationException("Context relevance scoring not yet implemented");
    }

    // Business-focused validation: V1 configuration works correctly
    public void V1ConfigurationWorksCorrectly()
    {
        // This will fail initially and drive implementation
        // Should verify backward compatibility with existing configurations
        throw new InvalidOperationException("V1 configuration backward compatibility not yet implemented");
    }

    // Business-focused validation: Phase-based activation still functions
    public void PhaseBasedActivationStillFunctions()
    {
        // This will fail initially and drive implementation
        // Should verify legacy phase system still works for v1.0 configs
        throw new InvalidOperationException("Legacy phase-based activation not yet implemented");
    }

    // Business-focused validation: Clean Architecture constraints activate
    public void CleanArchitectureConstraintsActivate()
    {
        // This will fail initially and drive implementation
        throw new InvalidOperationException("Clean Architecture constraint activation not yet implemented");
    }

    // Business-focused validation: Hexagonal Architecture constraints activate
    public void HexagonalArchitectureConstraintsActivate()
    {
        // This will fail initially and drive implementation
        throw new InvalidOperationException("Hexagonal Architecture constraint activation not yet implemented");
    }

    // Business-focused validation: Feature-Driven constraints activate
    public void FeatureDrivenConstraintsActivate()
    {
        // This will fail initially and drive implementation
        throw new InvalidOperationException("Feature-Driven Development constraint activation not yet implemented");
    }

    // Helper method to get project root
    private string GetProjectRoot()
    {
        string? currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null && !File.Exists(Path.Combine(currentDir, "ConstraintMcpServer.sln")))
        {
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        if (currentDir == null)
        {
            throw new InvalidOperationException("Could not find project root");
        }

        return currentDir;
    }

    public void Dispose()
    {
        _lastConstraintResponse?.Dispose();
    }
}