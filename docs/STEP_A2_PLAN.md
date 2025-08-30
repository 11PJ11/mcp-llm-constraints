# Step A2: Intelligent Trigger Matching - Outside-In Implementation Plan

## Overview
**Goal**: Context-aware constraint activation beyond simple cadence  
**Priority**: HIGH - Foundation for v2.0 composable constraint system  
**Duration**: 4-5 days (IN PROGRESS - Day 2 Complete)  
**Status**: 🟡 Context Analysis components implemented and tested

## Current State Analysis

### ✅ Already Implemented (Foundation)
- **Domain Models**: `TriggerConfiguration` and `TriggerContext` with comprehensive functionality
- **Performance Infrastructure**: `SimplifiedTriggerBenchmark.cs` with 10 benchmarks
- **Relevance Scoring**: `CalculateRelevanceScore()` algorithm with weighted matching
- **Basic Pattern Matching**: Keyword, file pattern, and context pattern matching
- **Anti-Pattern Support**: Exclusion logic to prevent inappropriate activation

### ✅ COMPLETED Implementation (Step A2 Progress)
- **✅ Application Layer**: ContextAnalyzer implemented with comprehensive keyword extraction
- **✅ Domain Layer**: SessionContext aggregate root with pattern recognition and analytics
- **✅ Testing**: 17 comprehensive tests added (7 ContextAnalyzer + 10 SessionContext) - all passing
- **✅ Quality Gates**: All 209 tests passing with full formatting compliance

### 🟡 IN PROGRESS Implementation (Remaining Scope)
- **🟡 Integration**: MCP pipeline integration for trigger matching (next milestone)
- **Pending**: KeywordMatcher with fuzzy matching and synonyms
- **Pending**: RelevanceScorer with configurable business rules
- **Pending**: End-to-end integration tests and performance validation

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

### Day 1: Foundation & E2E Test ✅ COMPLETED
- [x] ✅ Write failing E2E acceptance test (RED)
- [x] ✅ Create ITriggerMatchingEngine interface
- [x] ✅ Implement minimal TriggerMatchingEngine (GREEN)
- [x] ✅ Refactor domain model integration (REFACTOR)

### Day 2: Context Analysis ✅ COMPLETED 
- [x] ✅ Write failing context analysis tests (RED)
- [x] ✅ Implement IContextAnalyzer interface and ContextAnalyzer (GREEN)
- [x] ✅ Implement SessionContext aggregate root with pattern recognition (GREEN)
- [x] ✅ Added comprehensive test coverage (7 ContextAnalyzer + 10 SessionContext tests)
- [x] ✅ All quality gates passing with code formatting compliance
- [x] ✅ Context type detection working (feature_development, testing, refactoring)
- [x] ✅ Session-based pattern tracking (test-driven, feature-focused, mixed-development)
- [x] ✅ Session analytics and relevance adjustments implemented

### Day 3: MCP Integration ✅ COMPLETED
- [x] ✅ **Enhance ToolCallHandler with trigger matching integration** - Successfully implemented with context analysis constructor
- [x] ✅ **Write failing MCP pipeline integration tests (RED)** - Created ToolCallHandlerMcpContextExtractionTests and ToolCallHandlerTriggerMatchingTests
- [x] ✅ **Implement context extraction from MCP tool calls (GREEN)** - Integrated ContextAnalyzer and TriggerMatchingEngine with MCP pipeline
- [x] ✅ **Refactor constraint activation decision logic (REFACTOR)** - Applied Level 3 refactoring with extracted parameter extraction method

**🎉 MILESTONE ACHIEVED**: E2E test `MCP_Pipeline_Should_Extract_Context_And_Activate_Relevant_Constraints` passes naturally through outside-in TDD/BDD implementation!

### Day 4: Keyword Matching & Scoring
- [ ] Write failing keyword matching tests (RED)
- [ ] Implement KeywordMatcher with fuzzy matching and synonyms (GREEN)
- [ ] Implement RelevanceScorer with configurable business rules (GREEN)
- [ ] Refactor scoring algorithms (REFACTOR)

### Day 5: Integration & Validation
- [ ] Integration tests with complete MCP pipeline
- [ ] Performance validation (<50ms p95 with new components)
- [ ] End-to-end scenario validation
- [ ] Property-based test coverage for edge cases

### Day 6: Polish & Documentation
- [ ] Performance optimization based on benchmarks
- [ ] API documentation updates
- [ ] ✅ Update progress.md (COMPLETED)
- [ ] Final integration validation

## Success Criteria (Business Validation)

### Functional Requirements
- [x] ✅ **Context detection works across different development activities** - Implemented with feature_development, testing, refactoring classification
- [x] ✅ **Session-based pattern recognition functional** - Detects test-driven, feature-focused, mixed-development patterns
- [x] ✅ **Context analysis extracts keywords from user input** - Comprehensive keyword extraction from natural language and tool calls
- [x] ✅ **MCP integration enables context-aware activation** - Successfully integrated with ToolCallHandler context analysis
- [ ] Constraints activate based on user context with >80% relevance accuracy
- [ ] Anti-patterns prevent inappropriate reminder activation in 100% of test cases
- [ ] Confidence scoring enables intelligent constraint prioritization

### Performance Requirements  
- [ ] Constraint evaluation completes within 50ms p95 latency
- [ ] Memory usage stays under 100MB for typical constraint libraries
- [ ] Performance regression prevention integrated into CI/CD

### Quality Requirements
- [x] ✅ **Comprehensive unit test coverage implemented** - 17 new tests added (7 ContextAnalyzer + 10 SessionContext)
- [x] ✅ **Business scenario focus in tests** - All tests use business language and domain concepts
- [x] ✅ **Quality gates passing** - All 209 tests passing with full code formatting compliance
- [x] ✅ **Integration tests prove end-to-end workflows** - E2E test validates complete MCP pipeline with context-aware constraint activation
- [ ] Property-based tests validate business invariants  
- [ ] Mutation testing validates test effectiveness

### Architecture Requirements
- [x] ✅ **Clean separation maintained** - Domain models (SessionContext) separated from Application services (ContextAnalyzer)
- [x] ✅ **Dependency injection implemented** - IContextAnalyzer interface enables testing and flexibility
- [x] ✅ **Hexagonal architecture preserved** - Clear port/adapter boundaries maintained
- [x] ✅ **MCP integration preserves protocol compliance** - ToolCallHandler enhanced with backward-compatible context analysis support

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