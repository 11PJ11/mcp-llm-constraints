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

### Step 5 Additional Technical Debt Items (2024-08-24)

#### 8. Configuration-Based Constraint Loading ✅ RESOLVED
- **Previous**: Hard-coded constraints in `ToolCallHandler` for walking skeleton
- **Resolution**: Moved to `ConstraintFactory` in Domain layer (Step 5.1 refactoring)
- **Status**: Using proper domain factory pattern, eliminates 42 lines of duplication
- **Files**: `src/ConstraintMcpServer/Domain/ConstraintFactory.cs`

#### 9. Proper Phase Management
- **Current**: Hard-coded "red" phase for walking skeleton
- **Target**: Dynamic phase detection and tracking via `IPhaseTracker`
- **Impact**: Missing phase transitions, not context-aware
- **Files**: `ToolCallHandler.WalkingSkeletonPhase`

#### 10. Interface Extraction for Selection and Injection
- **Current**: Direct instantiation of `ConstraintSelector` and `Injector`
- **Target**: Extract `IConstraintSelector` and `IInjector` interfaces
- **Impact**: Violates Dependency Inversion, harder to test and extend
- **Files**: `ConstraintSelector.cs`, `Injector.cs`

#### 11. Rich Domain Context Usage
- **Current**: Using primitive `int` for interaction number
- **Target**: Use `InteractionContext` value object with session state
- **Impact**: Missing rich context (session ID, timestamps, metadata)
- **Files**: `Injector.FormatConstraintMessage()`

## Level 1-3 Refactoring: COMPREHENSIVE COMPLETION ✅ (2024-08-24)

### Level 1: Readability Improvements ✅ COMPLETED
*Previously completed during Step 4 & 5, enhanced in Step 5.1*

#### Constants Centralization ✅
- **Completed**: Created `JsonRpcConstants` and `InjectionConfiguration` classes
- **Result**: Eliminated constant duplication across 3 files (ConstraintCommandRouter, ConstraintResponseBuilder, ConstraintServerHost)
- **Benefit**: Single source of truth for protocol values and business configuration
- **Files**: `JsonRpcConstants.cs`, `InjectionConfiguration.cs`

#### Dead Code Cleanup ✅
- **Completed**: Removed TODO comments and unused imports
- **Result**: Cleaner codebase with no legacy comments
- **Files**: `ConstraintCommandRouter.cs`

### Level 2: Complexity Reduction ✅ COMPLETED

#### Constraint Factory Extraction ✅
- **Completed**: Extracted `ConstraintFactory` pattern eliminating major duplication
- **Result**: 42 lines of constraint creation duplication removed from ToolCallHandler
- **Benefit**: DRY principle applied, moved to proper Domain layer
- **Files**: `src/ConstraintMcpServer/Domain/ConstraintFactory.cs`

#### YAML Reader Simplification ✅
- **Completed**: Simplified `ConvertToConstraintPack()` using DTO behavior
- **Result**: Method reduced from 57 lines to 13 lines (78% complexity reduction)
- **Benefit**: Enhanced maintainability through validation method extraction
- **Files**: `YamlConstraintPackReader.cs:61-74`

#### Response Creation Extraction ✅ (Previously completed)
- **Completed**: Extracted `CreateConstraintResponse()` and `CreateStandardResponse()` methods
- **Result**: `HandleAsync()` method simplified from 52 lines to 15 lines
- **Files**: `ToolCallHandler.cs:75-96`

#### Response Duplication Elimination ✅ (Previously completed)
- **Completed**: Extracted `CreateJsonRpcResponse()` method for common response structure
- **Result**: Eliminated 30+ lines of duplicated JSON-RPC response building
- **Files**: `ToolCallHandler.cs:105-123`

### Level 3: Responsibility Organization ✅ COMPLETED

#### Feature Envy Resolution ✅
- **Completed**: Moved constraint creation from Presentation to Domain layer
- **Result**: Fixed anti-pattern where ToolCallHandler was creating domain objects
- **Benefit**: Proper separation of concerns, better architectural compliance
- **Files**: Moved factory methods to `ConstraintFactory.cs`

#### Data Class Enhancement ✅
- **Completed**: Enhanced `YamlConstraintDto` and `YamlConstraintPackDto` with behavior
- **Result**: Added `Validate()` and `ToDomainObject()` methods to eliminate anemic data pattern
- **Benefit**: Rich domain objects with proper responsibilities
- **Files**: `YamlConstraintPackReader.cs:142-245`

### Comprehensive Refactoring Results ✅

**Code Quality Achievements:**
- ✅ **~60 lines of duplication eliminated** (factory pattern + response creation)
- ✅ **78% complexity reduction** in YAML reader core method
- ✅ **Constants centralized** across 7 scattered constants
- ✅ **Anti-patterns fixed**: Feature Envy, Anemic Domain Model
- ✅ **Architecture enhanced**: Proper Domain/Application/Infrastructure boundaries

**Testing & Quality Assurance:**
- ✅ **All 38 tests remain GREEN** throughout entire refactoring process
- ✅ **Cross-platform CI/CD success** on Ubuntu, Windows, macOS
- ✅ **Quality gates passed** including formatting, analysis, performance
- ✅ **Zero regression** introduced during improvements

**Development Productivity:**
- ✅ **Maintainability improved** through centralized constants and factories
- ✅ **Extensibility enhanced** with proper separation of concerns
- ✅ **Future refactoring enabled** with solid foundation established

## Remaining Technical Debt Priority

### High Priority (Level 3-4: Next Sprint)
1. **Interface Extraction for Selection and Injection** - Violates Dependency Inversion
2. **Proper Phase Management** - Missing dynamic phase detection
3. **Rich Domain Context Usage** - Primitive obsession with `int` interaction numbers

### Medium Priority (Level 5-6: Release Preparation)  
4. **Strategy Pattern for Scheduling** - Extensibility for different injection patterns
5. **State Pattern for Phase Transitions** - Better phase-specific constraint injection
6. **Full Dependency Inversion** - Interface-based dependency injection throughout

### Low Priority (Already Resolved)
- ✅ **Configuration-Based Constraint Loading** - RESOLVED via ConstraintFactory
- ✅ **Level 1-2 Complexity & Readability** - COMPLETED comprehensively

## When to Address

- **Level 4**: Next sprint or when Step 6 requires better abstractions
- **Level 5-6**: Before v1.0 release or when extending scheduling strategies  
- **Interface Extraction**: Can be addressed in next TDD cycle for better testability

## Notes

- All deferred items are tracked but not blocking progress
- Current code follows Level 1-2 refactoring (readability & complexity)
- Tests provide safety net for future refactoring