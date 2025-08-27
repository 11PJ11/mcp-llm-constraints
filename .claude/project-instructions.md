# Project-Specific Instructions for MCP LLM Constraints

## Testing Standards

### 1. E2E Tests Format
- **Location**: All E2E tests go under `tests/E2E/` folder
- **Format**: BDD style using Given-When-Then with lambda expressions and ScenarioBuilder
- **Pattern**: 
  ```csharp
  await Given(_steps!.SomeContextStep)
      .And(_steps.AnotherContextStep)
      .When(_steps.TriggerAction)
      .Then(_steps.ExpectedOutcome)
      .And(_steps.AdditionalVerification)
      .ExecuteAsync();
  ```
- **Steps**: Implement business-focused step methods in `tests/Steps/` classes

### 2. Test Assertions - Business Value First
- **❌ Never assert**: `Assert.Throws<NotImplementedException>()` - this provides no business value
- **✅ Always assert**: Real business outcomes and behaviors
- **TDD Approach**: Tests should fail first for the RIGHT REASON (missing implementation), then pass when logic is implemented
- **Example**: 
  ```csharp
  // ❌ Wrong - testing implementation detail
  Assert.Throws<NotImplementedException>(() => matcher.CalculateConfidence(keywords));
  
  // ✅ Right - testing business behavior
  var confidence = matcher.CalculateConfidence(contextKeywords, targetKeywords);
  Assert.That(confidence, Is.GreaterThan(0.8), "should have high confidence for exact keyword matches");
  ```

## Documentation Organization

### 3. Documentation Structure
- **Root folder**: Only `README.md` stays in root
- **All other docs**: Go under `/docs` folder
  - Architecture docs: `/docs/architecture.md`, `/docs/step_a2_plan.md`
  - Progress tracking: `/docs/progress.md`
  - Technical specs: `/docs/performance.md`, `/docs/api.md`

## Code Quality Standards

### 4. Refactoring Requirements
- **After making tests pass**: Always refactor to **at least Level 3**
- **Level 1** (Readability): Comments, dead code, naming, magic strings/numbers
- **Level 2** (Complexity): Method extraction, duplication elimination  
- **Level 3** (Organization): Class responsibilities, coupling reduction, feature envy
- **Required**: Complete Levels 1-3 before marking task as complete
- **Goal**: Maintain clean, maintainable code throughout development

## Implementation Approach

### TDD Cycle with Business Focus
1. **RED**: Write failing test that asserts business behavior
2. **GREEN**: Implement minimal code to make test pass
3. **REFACTOR**: Apply Level 1-3 refactoring systematically
4. **Validate**: Ensure business value is delivered and code quality maintained

### Context-Aware Development
- Follow Outside-In TDD with business scenario focus
- Use realistic test data and scenarios from actual development contexts
- Prioritize business outcomes over technical implementation details
- Maintain clean architecture boundaries and separation of concerns