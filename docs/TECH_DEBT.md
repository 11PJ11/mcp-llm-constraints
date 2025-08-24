# Technical Debt Register

This document tracks technical debt items identified during development that are deferred for future implementation.

## Refactoring Debt (Identified: 2024-08-24, Step 4 Completion)

### Level 3-4: Responsibilities & Abstractions (Sprint Boundary)

#### 1. Implement IScheduler Interface
- **Current**: `Scheduler` class doesn't implement `IScheduler` interface
- **Target**: Implement `Domain.Enforcement.IScheduler`
- **Impact**: Violates Dependency Inversion Principle, harder to mock for testing
- **Files**: `src/ConstraintMcpServer/Application/Scheduling/Scheduler.cs`

#### 2. Use InteractionContext Domain Object
- **Current**: Using primitive `int` for interaction counting
- **Target**: Use `Domain.Enforcement.InteractionContext` value object
- **Impact**: Primitive obsession, missing rich domain context (session ID, phase, metadata)
- **Files**: `Scheduler.cs`, `ToolCallHandler.cs`

#### 3. Use ScheduleConfiguration
- **Current**: Using primitive `int` for configuration
- **Target**: Use `Domain.Enforcement.ScheduleConfiguration` for rich config
- **Impact**: Missing phase overrides, injection policies, deterministic selection options
- **Files**: `Scheduler.cs`, `ConstraintCommandRouter.cs`

#### 4. Extract SessionManager
- **Current**: `ToolCallHandler` manages its own interaction counter
- **Target**: Separate `SessionManager` for state management
- **Impact**: Mixed responsibilities, difficult to share session state across handlers
- **Files**: Create `Application/Session/SessionManager.cs`

### Level 5: Design Patterns (Release Preparation)

#### 5. Strategy Pattern for Scheduling
- **Current**: Single scheduling algorithm
- **Target**: Strategy pattern for different scheduling strategies
- **Impact**: Extensibility for different injection patterns

#### 6. State Pattern for Phase Transitions
- **Current**: No phase awareness
- **Target**: State pattern for TDD phases (red, green, refactor)
- **Impact**: Better phase-specific constraint injection

### Level 6: SOLID++ Principles (Release Preparation)

#### 7. Full Dependency Inversion
- **Current**: Direct instantiation of concrete classes
- **Target**: Interface-based dependency injection throughout
- **Impact**: Better testability, looser coupling

## Why These Are Deferred

1. **Working Software First**: Current implementation passes all tests (29/29)
2. **YAGNI Principle**: Phase support not yet required by any test
3. **Incremental Refactoring**: Following our 6-level hierarchy guidelines
4. **Risk Management**: Larger structural changes better done at sprint boundaries

## When to Address

- **Level 3-4**: Next sprint or when Step 5 requires phase filtering
- **Level 5-6**: Before v1.0 release or when extending scheduling strategies

## Notes

- All deferred items are tracked but not blocking progress
- Current code follows Level 1-2 refactoring (readability & complexity)
- Tests provide safety net for future refactoring