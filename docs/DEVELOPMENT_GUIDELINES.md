# Development Guidelines - Methodology-Agnostic Constraint System

> **Critical Principle**: This system must remain methodology-agnostic. It provides generic composition strategies and helps users define their own practices without built-in knowledge of specific methodologies.

## Core Architectural Principles

### 1. Methodology Agnosticism - NON-NEGOTIABLE

**✅ ALLOWED**:
- Generic composition strategies (sequential, hierarchical, progressive, layered)
- User-defined workflow states and contexts  
- Configurable constraint definitions
- Generic trigger matching based on user-defined patterns
- Helper functions for definition, updates, visualization, and composition

**❌ FORBIDDEN**:
- Hardcoded methodology names (TDD, BDD, Scrum, Kanban, etc.)
- Built-in knowledge of specific practices (red-green-refactor, acceptance tests, etc.)
- Methodology-specific logic or assumptions
- Hardcoded phase names or workflow states
- Practice-specific validation rules or error messages

### 2. User-Driven Configuration

**✅ REQUIRED**:
- All practices defined by users in configuration
- All workflow states and contexts user-configurable
- All constraint definitions user-created
- All composition relationships user-defined
- All trigger patterns user-specified

**❌ PROHIBITED**:
- Default methodology assumptions
- Built-in constraint definitions
- Hardcoded workflow progressions
- Methodology-specific defaults

## Code Patterns to Avoid

### Anti-Pattern 1: Hardcoded Methodology Enums
```csharp
// ❌ WRONG - Contains built-in methodology knowledge
public enum TddPhase { Red, Green, Refactor }
public enum RefactoringLevel { Level1, Level2, Level3, Level4, Level5, Level6 }

// ✅ CORRECT - User-defined workflow states
public sealed class WorkflowState
{
    public string Name { get; }        // User-defined: "planning", "coding", "reviewing"
    public string? Description { get; } // User-defined description
}
```

### Anti-Pattern 2: Methodology-Specific Logic
```csharp
// ❌ WRONG - Built-in TDD knowledge
public ConstraintActivation GetNextConstraint(Context context)
{
    return context.Phase switch
    {
        "red" => new ConstraintActivation("tdd.write-failing-test"),
        "green" => new ConstraintActivation("tdd.write-minimal-code"),
        "refactor" => new ConstraintActivation("tdd.improve-design")
    };
}

// ✅ CORRECT - Generic user-driven logic  
public ConstraintActivation GetNextConstraint(UserDefinedContext context)
{
    var position = DeterminePosition(context.CurrentState, context.UserDefinedProgression);
    var nextConstraint = context.UserDefinedSequence.ElementAtOrDefault(position);
    return nextConstraint != null ? new ConstraintActivation(nextConstraint.Id) : ConstraintActivation.Complete;
}
```

### Anti-Pattern 3: Practice-Specific Comments and Documentation
```csharp
// ❌ WRONG - References specific methodology
/// <summary>
/// Manages TDD phase transitions from RED to GREEN to REFACTOR.
/// Enforces test-first development discipline.
/// </summary>

// ✅ CORRECT - Generic functionality description
/// <summary>
/// Manages user-defined workflow state transitions in sequential compositions.
/// Enforces user-configured progression rules and constraints.
/// </summary>
```

### Anti-Pattern 4: Hardcoded Constraint Definitions
```csharp
// ❌ WRONG - Built-in practice knowledge
private static readonly Dictionary<int, string> RefactoringLevels = new()
{
    [1] = "Focus on readability improvements",
    [2] = "Reduce complexity and eliminate duplication",
    [3] = "Reorganize class responsibilities"
};

// ✅ CORRECT - User-configurable definitions
public sealed class UserDefinedStage
{
    public string Id { get; }
    public string Title { get; }          // User-defined
    public string Description { get; }    // User-defined
    public IReadOnlyList<string> Guidance { get; } // User-defined
}
```

## Naming Conventions

### Domain Model Naming
- Use generic terms: `Composition`, `Strategy`, `Context`, `State`, `Sequence`
- Avoid methodology terms: `TddPhase`, `RefactoringLevel`, `BddScenario`
- Use user-centric terms: `UserDefinedConstraint`, `WorkflowState`, `UserContext`

### Method and Class Naming
```csharp
// ✅ GOOD - Generic functionality
public class SequentialCompositionStrategy
public UserDefinedContext AnalyzeUserContext(...)
public GenericActivationResult GetNextActivation(...)

// ❌ BAD - Methodology-specific
public class TddCompositionStrategy  
public TddPhase AnalyzeTddContext(...)
public RefactoringActivationResult GetNextRefactoringConstraint(...)
```

## Testing Strategies

### Test Data and Examples
```csharp
// ✅ CORRECT - Generic test data
var userDefinedConstraint = new UserDefinedConstraint
{
    Id = "user.practice-alpha",
    Contexts = new[] { "user.state-1", "user.state-2" },
    Reminders = new[] { "Remember your chosen practice" }
};

// ❌ WRONG - Methodology-specific test data
var tddConstraint = new Constraint
{
    Id = "tdd.test-first",
    Phases = new[] { "red", "green", "refactor" },
    Reminders = new[] { "Write failing test first" }
};
```

### Test Scenarios
- Use generic workflow examples: "practice-A → practice-B → practice-C"
- Avoid methodology references: Don't test "TDD workflows" or "refactoring progressions"
- Test composition strategies with arbitrary user-defined content
- Validate system works with completely custom methodologies

## Configuration Guidelines

### YAML Schema Design
```yaml
# ✅ CORRECT - User-driven configuration
version: "0.2.0"
user_definitions:
  workflow_states:
    - name: "user-state-alpha"
      description: "User-defined workflow state"
  
  constraints:
    - id: "user.practice-1"
      contexts: ["user-state-alpha", "user-state-beta"]
      reminders: ["User-defined reminder text"]

# ❌ WRONG - Built-in methodology assumptions  
version: "0.2.0"
constraints:
  - id: "tdd.test-first"
    phases: ["red", "green", "refactor"]  # Hardcoded phases
    reminders: ["Write failing test first"]  # Practice-specific
```

### Default Examples and Templates
- Provide generic templates: "methodology-template-1", "workflow-example-a"
- Include separate methodology-specific examples as user-contributed templates
- Never include methodology-specific examples as defaults
- Document how users can recreate common methodologies using generic features

## Code Review Checklist

### Pre-Commit Questions
1. **Does this code contain any hardcoded methodology knowledge?**
2. **Are all workflow states and contexts user-configurable?** 
3. **Would this work with a completely different methodology (e.g., Waterfall, Kanban)?**
4. **Are error messages and documentation methodology-neutral?**
5. **Could a user recreate TDD, Scrum, or any methodology using these generic features?**

### Red Flag Indicators
- References to specific methodologies in code, comments, or tests
- Hardcoded phase names, level numbers, or practice descriptions
- Logic that assumes specific workflow progressions
- Validation rules tied to particular practices
- Default configurations with methodology-specific content

## Migration Guidelines

### When Updating Existing Code
1. **Identify methodology assumptions** in current implementation
2. **Extract user-configurable elements** from hardcoded logic
3. **Replace specific logic with generic patterns** 
4. **Update tests to use generic examples**
5. **Validate works with different methodologies**

### Documentation Updates
- Remove methodology-specific examples from core documentation
- Add generic examples showing flexibility
- Create separate methodology templates as user resources
- Emphasize user-driven configuration in all documentation

## Quality Gates

### Definition of Done - Methodology Agnosticism
- [ ] Zero hardcoded methodology names or concepts in codebase
- [ ] All workflow elements user-configurable
- [ ] System works with at least 3 different methodology examples
- [ ] No methodology assumptions in error messages or logs  
- [ ] Documentation uses only generic examples
- [ ] Tests use generic, user-defined examples

### Enhanced Quality Assurance Process *(Updated December 2025)*

#### **Local-to-CI/CD Validation Parity** 🎯
- ✅ **Pre-commit hooks** now match CI/CD Quality Gates exactly
- ✅ **MSBuild flag consistency**: Both local and CI/CD use `--warnaserror` flag
- ✅ **Warning detection**: Catches ALL warning types including nullable reference warnings
- ✅ **Zero surprise failures**: Local validation predicts CI/CD outcomes with 100% accuracy

#### **Quality Gates Validation Process**
```bash
# Enhanced pre-commit validation (matches CI/CD exactly)
./scripts/quality-gates.sh

# Fast commit option (skips mutation testing for rapid development)
./scripts/fast-commit.sh "commit message"

# Manual quality control
FAST_COMMIT=true ./scripts/quality-gates.sh
```

#### **Validation Coverage**
- **Build validation**: All projects compile with zero warnings using `--warnaserror`
- **Test validation**: 376+ tests across 4 projects (FeedbackTests, Performance, StorageTests, main Tests)
- **Code formatting**: Consistent formatting across all source files
- **Cross-platform**: Ubuntu, Windows, macOS build validation

#### **Developer Experience Improvements**
- **Predictable outcomes**: Local validation exactly matches CI/CD behavior
- **Fast feedback**: Pre-commit hooks catch issues before CI/CD
- **Quality enforcement**: MSBuild warnings treated as errors prevent quality degradation
- **Enhanced error messages**: Clear guidance for fixing common issues

This ensures the system remains a **universal constraint reminder helper** that users can configure for any methodology, with rock-solid development workflow reliability and zero CI/CD surprises.