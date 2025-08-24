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

### Level 2: Complexity Reduction ✅ COMPLETED (2024-08-24)
*Note: Level 1 (Readability) already applied during Step 4*

#### 8. Extract Method for Response Creation ✅
- **Completed**: Extracted `CreateConstraintResponse()` and `CreateStandardResponse()` methods
- **Result**: `HandleAsync()` method simplified from 52 lines to 15 lines
- **Benefit**: Single responsibility per method, easier to test response formatting separately
- **Files**: `src/ConstraintMcpServer/Presentation/Hosting/ToolCallHandler.cs:62-77`

#### 9. Eliminate Response Duplication ✅
- **Completed**: Extracted `CreateJsonRpcResponse()` method for common response structure
- **Result**: Eliminated 30+ lines of duplicated JSON-RPC response building
- **Benefit**: DRY principle applied, consistent response format guaranteed
- **Files**: `src/ConstraintMcpServer/Presentation/Hosting/ToolCallHandler.cs:86-104`

**Level 2 Refactoring Results:**
- ✅ All 27 tests remain GREEN throughout refactoring
- ✅ Method complexity reduced: `HandleAsync()` now has single decision point
- ✅ Code duplication eliminated: Common response structure extracted
- ✅ Maintainability improved: Easier to modify response format in one place
- ✅ Code formatting verified with `dotnet format`

## Why These Are Deferred

1. **Working Software First**: Current implementation passes all tests (27/27)
2. **YAGNI Principle**: Phase support not yet required by any test  
3. **Incremental Refactoring**: Following our 6-level hierarchy guidelines
4. **Risk Management**: Larger structural changes better done at sprint boundaries
5. **Level 2 Items**: Can be addressed immediately but deferred to maintain momentum

## When to Address

- **Level 2**: Can be addressed immediately in next TDD cycle
- **Level 3-4**: Next sprint or when Step 5 requires phase filtering  
- **Level 5-6**: Before v1.0 release or when extending scheduling strategies

## Notes

- All deferred items are tracked but not blocking progress
- Current code follows Level 1-2 refactoring (readability & complexity)
- Tests provide safety net for future refactoring