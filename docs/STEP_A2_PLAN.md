# Step A2: Intelligent Trigger Matching - Outside-In Implementation Plan

## Overview
**Goal**: Context-aware constraint activation beyond simple cadence  
**Priority**: HIGH - Foundation for v2.0 composable constraint system  
**Duration**: 4-5 days  

## Current State Analysis

### ✅ Already Implemented (Foundation)
- **Domain Models**: `TriggerConfiguration` and `TriggerContext` with comprehensive functionality
- **Performance Infrastructure**: `SimplifiedTriggerBenchmark.cs` with 10 benchmarks
- **Relevance Scoring**: `CalculateRelevanceScore()` algorithm with weighted matching
- **Basic Pattern Matching**: Keyword, file pattern, and context pattern matching
- **Anti-Pattern Support**: Exclusion logic to prevent inappropriate activation

### 🎯 Missing Implementation (Step A2 Scope)
- **Application Layer**: Trigger matching engine and context analyzer
- **Integration**: MCP pipeline integration for context extraction
- **Configuration**: Confidence scoring and activation strategies
- **Testing**: Comprehensive business scenario validation

## Outside-In Implementation Plan

### Phase 1: Acceptance Tests First (RED-GREEN-REFACTOR)

#### E2E Acceptance Test: Context-Aware Constraint Activation
```csharp
[Test]
public void TriggerMatchingEngine_Should_Activate_Constraints_Based_On_User_Context()
{
    // Business Scenario: Developer types "implement feature test" in development context
    // Expected: System activates TDD constraints with >80% confidence
    
    // This test will FAIL and drive the entire implementation
    var engine = new TriggerMatchingEngine();
    var context = new TriggerContext(
        keywords: new[] { "implement", "feature", "test" },
        filePath: "/src/features/NewFeature.cs",
        contextType: "feature_development"
    );
    
    var activatedConstraints = engine.EvaluateConstraints(context);
    
    // Business validation
    Assert.That(activatedConstraints, Is.Not.Empty);
    Assert.That(activatedConstraints.First().ConstraintId, Is.EqualTo("tdd.test-first"));
    Assert.That(activatedConstraints.First().ConfidenceScore, Is.GreaterThan(0.8));
}
```

### Phase 2: Application Layer (Service-Oriented Architecture)

#### TriggerMatchingEngine (Core Business Logic)
**File**: `src/ConstraintMcpServer/Application/Triggers/TriggerMatchingEngine.cs`

```csharp
/// <summary>
/// Core trigger matching engine for context-aware constraint activation.
/// Implements business logic for evaluating trigger configurations against context.
/// </summary>
public class TriggerMatchingEngine : ITriggerMatchingEngine
{
    // Business behavior: Evaluate constraints against context with confidence scoring
    Task<IReadOnlyList<ConstraintActivation>> EvaluateConstraints(TriggerContext context);
    
    // Business behavior: Get constraints matching specific confidence threshold
    Task<IReadOnlyList<ConstraintActivation>> GetRelevantConstraints(TriggerContext context, double minConfidence = 0.7);
}
```

#### ContextAnalyzer (Context Extraction)
**File**: `src/ConstraintMcpServer/Application/Triggers/ContextAnalyzer.cs`

```csharp
/// <summary>
/// Analyzes user input and development session to extract trigger context.
/// Bridges MCP tool calls to business domain context.
/// </summary>
public class ContextAnalyzer : IContextAnalyzer
{
    // Business behavior: Extract development context from MCP tool calls
    TriggerContext AnalyzeToolCallContext(string methodName, object[] parameters, string sessionId);
    
    // Business behavior: Extract keywords and patterns from user input
    TriggerContext AnalyzeUserInput(string userInput, string sessionId);
    
    // Business behavior: Detect development activity type
    string DetectContextType(IEnumerable<string> keywords, string filePath);
}
```

#### KeywordMatcher (Intelligent Text Processing)
**File**: `src/ConstraintMcpServer/Application/Triggers/KeywordMatcher.cs`

```csharp
/// <summary>
/// Intelligent keyword matching with confidence scoring and fuzzy matching.
/// Handles variations in terminology and development language.
/// </summary>
public class KeywordMatcher : IKeywordMatcher
{
    // Business behavior: Calculate keyword match confidence with fuzzy logic
    double CalculateKeywordConfidence(IEnumerable<string> contextKeywords, IEnumerable<string> targetKeywords);
    
    // Business behavior: Extract keywords from natural language input
    IEnumerable<string> ExtractKeywords(string input);
    
    // Business behavior: Handle synonyms and variations (e.g., "test" -> "testing", "TDD")
    IEnumerable<string> ExpandSynonyms(IEnumerable<string> keywords);
}
```

#### RelevanceScorer (Business Logic for Activation Decisions)
**File**: `src/ConstraintMcpServer/Application/Triggers/RelevanceScorer.cs`

```csharp
/// <summary>
/// Calculates constraint relevance based on multiple factors.
/// Implements business rules for constraint activation prioritization.
/// </summary>
public class RelevanceScorer : IRelevanceScorer
{
    // Business behavior: Calculate overall relevance combining multiple factors
    double CalculateRelevanceScore(TriggerConfiguration config, TriggerContext context);
    
    // Business behavior: Apply anti-pattern penalties
    double ApplyAntiPatternPenalties(double baseScore, TriggerContext context, IEnumerable<string> antiPatterns);
    
    // Business behavior: Boost score based on context type matching
    double CalculateContextTypeBoost(string contextType, IEnumerable<string> patterns);
}
```

### Phase 3: Domain Models (Value Objects & Entities)

#### ConstraintActivation (Value Object)
**File**: `src/ConstraintMcpServer/Domain/Context/ConstraintActivation.cs`

```csharp
/// <summary>
/// Represents an activated constraint with confidence and activation metadata.
/// Value object containing business information about constraint selection.
/// </summary>
public sealed class ConstraintActivation
{
    public string ConstraintId { get; init; }
    public double ConfidenceScore { get; init; }
    public ActivationReason Reason { get; init; }
    public DateTimeOffset ActivatedAt { get; init; }
    public TriggerContext TriggerContext { get; init; }
}
```

#### SessionContext (Aggregate Root)
**File**: `src/ConstraintMcpServer/Domain/Context/SessionContext.cs`

```csharp
/// <summary>
/// Maintains development session state across multiple tool calls.
/// Aggregate root for session-based constraint activation patterns.
/// </summary>
public sealed class SessionContext
{
    // Business behavior: Track constraint activation history
    public IReadOnlyList<ConstraintActivation> ActivationHistory { get; }
    
    // Business behavior: Detect activity patterns within session
    public string DetectedActivityPattern { get; }
    
    // Business behavior: Calculate session-based relevance adjustments
    public double GetSessionRelevanceAdjustment(string constraintId);
}
```

### Phase 4: Infrastructure Integration (MCP Pipeline)

#### MCP Pipeline Integration
**File**: `src/ConstraintMcpServer/Presentation/Hosting/ToolCallHandler.cs` (Enhancement)

```csharp
// Enhanced tool call handler with trigger matching
private async Task<ConstraintInjectionResult> EvaluateConstraintInjection(
    string method, 
    object parameters,
    string sessionId)
{
    // Extract context from MCP tool call
    var triggerContext = await _contextAnalyzer.AnalyzeToolCallContext(method, parameters, sessionId);
    
    // Evaluate constraints against context
    var activations = await _triggerMatchingEngine.EvaluateConstraints(triggerContext);
    
    // Apply business rules for injection decision
    return await _injectionDecisionEngine.DecideInjection(activations, sessionId);
}
```

## Testing Strategy (BDD + Property-Based)

### Unit Tests (Business Behavior Validation)

#### TriggerMatchingEngineTests.cs
```csharp
[TestFixture]
public class TriggerMatchingEngineTests
{
    // Business Scenario Tests
    [Test] public void Should_Activate_TDD_Constraints_For_Testing_Context()
    [Test] public void Should_Activate_Refactoring_Constraints_For_Cleanup_Context()
    [Test] public void Should_Exclude_TDD_During_Hotfix_Context()
    [Test] public void Should_Prioritize_Higher_Confidence_Constraints()
    
    // Edge Cases
    [Test] public void Should_Handle_Empty_Context_Gracefully()
    [Test] public void Should_Apply_Confidence_Threshold_Filtering()
    
    // Performance Requirements  
    [Test] public void Should_Complete_Evaluation_Within_Performance_Budget()
}
```

#### Property-Based Tests (Business Invariants)
```csharp
[Test]
public void ConfidenceScore_Should_Always_Be_Between_Zero_And_One()
{
    // Property: Confidence scores must be valid probabilities
    Prop.ForAll(Generators.TriggerContext(), Generators.TriggerConfiguration(),
        (context, config) =>
        {
            var score = context.CalculateRelevanceScore(config);
            return score >= 0.0 && score <= 1.0;
        }).QuickCheckThrowOnFailure();
}
```

### Integration Tests (End-to-End Scenarios)

#### TriggerMatchingIntegrationTests.cs
```csharp
[TestFixture]
public class TriggerMatchingIntegrationTests
{
    [Test]
    public void Complete_TDD_Workflow_Should_Activate_Appropriate_Constraints()
    {
        // Given: Developer starting new feature development
        // When: Context indicates "new feature", "test first", "TDD"  
        // Then: System activates TDD methodology constraints with high confidence
    }
    
    [Test] 
    public void Outside_In_Development_Should_Coordinate_Constraint_Hierarchies()
    {
        // Given: Complex methodology composition (Outside-In = Acceptance + BDD + TDD)
        // When: Context indicates "acceptance test", "behavior", "feature"
        // Then: System activates hierarchical constraint composition correctly
    }
}
```

### Performance Tests (Sub-50ms Requirement)

#### TriggerMatchingPerformanceTests.cs
```csharp
[Test]
public void Constraint_Evaluation_Should_Meet_Latency_Requirements()
{
    // Performance contract: <50ms p95 latency
    var stopwatch = Stopwatch.StartNew();
    
    var activations = await _engine.EvaluateConstraints(complexContext);
    
    stopwatch.Stop();
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(50));
}
```

## Implementation Schedule (Outside-In TDD)

### Day 1: Foundation & E2E Test
- [ ] Write failing E2E acceptance test (RED)
- [ ] Create ITriggerMatchingEngine interface
- [ ] Implement minimal TriggerMatchingEngine (GREEN)
- [ ] Refactor domain model integration (REFACTOR)

### Day 2: Context Analysis
- [ ] Write failing context analysis tests (RED)
- [ ] Implement ContextAnalyzer (GREEN) 
- [ ] Refactor MCP integration (REFACTOR)

### Day 3: Keyword Matching & Scoring
- [ ] Write failing keyword matching tests (RED)
- [ ] Implement KeywordMatcher and RelevanceScorer (GREEN)
- [ ] Refactor scoring algorithms (REFACTOR)

### Day 4: Integration & Validation
- [ ] Integration tests with MCP pipeline
- [ ] Performance validation (<50ms p95)
- [ ] End-to-end scenario validation

### Day 5: Polish & Documentation
- [ ] Property-based test coverage
- [ ] Performance optimization
- [ ] API documentation
- [ ] Update progress.md

## Success Criteria (Business Validation)

### Functional Requirements
- [ ] Constraints activate based on user context with >80% relevance accuracy
- [ ] Anti-patterns prevent inappropriate reminder activation in 100% of test cases
- [ ] Context detection works across different file types (C#, JS, Python test coverage)
- [ ] Confidence scoring enables intelligent constraint prioritization

### Performance Requirements  
- [ ] Constraint evaluation completes within 50ms p95 latency
- [ ] Memory usage stays under 100MB for typical constraint libraries
- [ ] Performance regression prevention integrated into CI/CD

### Quality Requirements
- [ ] 90%+ unit test coverage with business scenario focus
- [ ] Property-based tests validate business invariants
- [ ] Integration tests prove end-to-end methodology workflows
- [ ] Mutation testing validates test effectiveness

### Architecture Requirements
- [ ] Clean separation: Domain → Application → Presentation → Infrastructure
- [ ] Dependency injection enables testing and flexibility
- [ ] MCP integration preserves protocol compliance
- [ ] Hexagonal architecture maintained with clear port/adapter boundaries

## Risk Mitigation

### Technical Risks
- **Risk**: Context extraction complexity impacts performance
- **Mitigation**: Performance tests enforced at each TDD cycle

- **Risk**: Keyword matching false positives reduce effectiveness
- **Mitigation**: Property-based tests validate matching algorithms

- **Risk**: MCP integration breaks existing functionality  
- **Mitigation**: Comprehensive integration tests with existing pipeline

### Business Risks
- **Risk**: Over-engineering reduces development velocity
- **Mitigation**: Outside-In approach ensures business value first

- **Risk**: Confidence scoring complexity confuses users
- **Mitigation**: Simple threshold-based approach with clear business rules

This outside-in plan ensures that every implementation decision is driven by failing business scenarios and validated through comprehensive testing aligned with the project's established patterns.