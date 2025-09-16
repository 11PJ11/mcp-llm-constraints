# Architectural Design Document - MCP LLM Constraints Phase 1

## System Overview

This document presents the architectural design for Phase 1 infrastructure foundation, transforming the current monolithic Program.cs implementation to a proper Hexagonal Architecture with dependency injection, while preserving all 386 tests and maintaining sub-50ms p95 latency requirements.

## Component Architecture

### Service Boundaries

The system will be restructured into clear hexagonal layers with the following service boundaries:

#### **Domain Core (Inner Hexagon)**
- **ConstraintPack**: Immutable constraint definitions with business rules
- **ConstraintSelector**: Priority-based constraint selection logic
- **Phase**: TDD lifecycle phase management
- **Priority**: Value object for constraint priority calculations
- **SessionContext**: Request context and state management

#### **Application Services (Middle Layer)**
- **IConstraintService**: Core business workflow orchestration
- **ISchedulingService**: Deterministic constraint injection scheduling
- **IContextAnalyzer**: Request context analysis and phase detection
- **IConstraintRepository**: Domain service for constraint retrieval
- **IValidationService**: Configuration and constraint validation

#### **Infrastructure Adapters (Outer Hexagon)**
- **IMcpServer**: MCP protocol communication abstraction
- **IConfigurationReader**: YAML configuration loading
- **IStructuredLogger**: NDJSON logging implementation
- **IPerformanceMetrics**: Performance monitoring and measurement

### Interface Definitions

#### Port Interfaces (Domain → Application)
```csharp
// Domain service ports
public interface IConstraintRepository
{
    Task<ConstraintPack> GetConstraintPackAsync(string version);
    Task<IEnumerable<Constraint>> GetConstraintsByPhaseAsync(Phase phase);
}

public interface IContextAnalyzer
{
    Task<SessionContext> AnalyzeRequestAsync(string jsonRequest);
    Phase DetectPhase(SessionContext context);
}

public interface IConstraintService
{
    Task<ConstraintActivationResult> SelectConstraintsAsync(SessionContext context);
    Task<string> InjectConstraintsAsync(string originalRequest, IEnumerable<Constraint> constraints);
}
```

#### Adapter Interfaces (Application → Infrastructure)
```csharp
// Infrastructure adapter ports
public interface IMcpServer
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task<string> ProcessRequestAsync(string request);
}

public interface IConfigurationReader
{
    Task<ConstraintPackConfiguration> LoadAsync(string configPath);
    Task<ValidationResult> ValidateAsync(ConstraintPackConfiguration config);
}

public interface IStructuredLogger
{
    void LogConstraintSelection(string constraintId, double priority, Phase phase);
    void LogPerformanceMetric(string operation, TimeSpan duration);
    void LogError(string operation, Exception exception);
}
```

### Dependency Relationships

The dependency flow follows hexagonal architecture principles:

```
Infrastructure → Application → Domain
```

**Explicit Dependencies**:
- Infrastructure depends on Application interfaces
- Application depends on Domain entities and services
- Domain has no external dependencies (pure business logic)

## Integration Points

### External System Integration

#### MCP Protocol Communication
- **Boundary**: `IMcpServer` port in Infrastructure layer
- **Implementation**: JSON-RPC over stdio with Content-Length headers
- **Error Handling**: Protocol-level error responses with structured logging
- **Performance**: Sub-50ms response time budget with connection pooling

#### Configuration Management
- **Boundary**: `IConfigurationReader` port in Infrastructure layer
- **Implementation**: YamlDotNet deserialization with FluentValidation
- **Hot Reload**: File system watcher for configuration changes (future)
- **Validation**: Schema validation with business rule enforcement

### Data Flow Patterns

#### Request Processing Flow
```
1. MCP Request → IMcpServer (Infrastructure)
2. Request Analysis → IContextAnalyzer (Application)
3. Constraint Selection → IConstraintService (Application)
4. Domain Logic → ConstraintSelector (Domain)
5. Response Enhancement → IMcpServer (Infrastructure)
6. Structured Logging → IStructuredLogger (Infrastructure)
```

#### Configuration Loading Flow
```
1. Startup → IConfigurationReader (Infrastructure)
2. YAML Parsing → ConstraintPackConverter (Infrastructure)
3. Validation → IValidationService (Application)
4. Domain Mapping → ConstraintPack (Domain)
5. Repository Storage → IConstraintRepository (Application)
```

### Communication Protocols

#### Synchronous Patterns
- **MCP Request/Response**: Direct synchronous processing with timeout handling
- **Configuration Loading**: Synchronous startup with validation pipeline
- **Constraint Selection**: Synchronous domain logic execution

#### Error Propagation
- **Domain Exceptions**: Business rule violations bubble up unchanged
- **Application Exceptions**: Service-level errors with context preservation
- **Infrastructure Exceptions**: Technical failures with retry logic

## Architectural Patterns and Decisions

### Core Architectural Patterns

#### Hexagonal Architecture (Ports and Adapters)
- **Rationale**: Enables testability, maintainability, and technology independence
- **Implementation**: Clear separation between domain, application, and infrastructure
- **Benefits**: Simplified testing, reduced coupling, improved modularity

#### Dependency Injection with Service Locator Pattern
- **Framework**: Microsoft.Extensions.DependencyInjection
- **Lifetime Management**: Singleton for stateless services, Scoped for request context
- **Performance**: <1ms container resolution requirement with reflection-based installers

#### Configuration as Code
- **Pattern**: Strongly-typed configuration objects with validation
- **Implementation**: YAML → C# POCO → Domain entities
- **Validation**: FluentValidation rules with meaningful error messages

### Component Design Patterns

#### Service Layer Pattern
- **Application Services**: Coordinate domain logic and infrastructure concerns
- **Business Workflows**: Encapsulate complex multi-step business processes
- **Transaction Boundaries**: Define clear service boundaries for operations

#### Repository Pattern
- **Domain Abstraction**: `IConstraintRepository` hides infrastructure details
- **Implementation**: In-memory storage with async interface for future expansion
- **Caching**: Transparent caching layer for performance optimization

#### Factory Pattern
- **Constraint Creation**: `ConstraintFactory` for complex constraint instantiation
- **Configuration Objects**: Factory methods for validated configuration creation
- **Test Doubles**: Factory pattern for test object creation

### Integration Patterns

#### Adapter Pattern for External Systems
- **MCP Protocol**: `McpServerAdapter` implements `IMcpServer` interface
- **Configuration Loading**: `YamlConfigurationAdapter` implements `IConfigurationReader`
- **Logging**: `StructuredLoggerAdapter` implements `IStructuredLogger`

#### Strategy Pattern for Variability
- **Constraint Selection**: Pluggable selection strategies based on context
- **Validation Rules**: Configurable validation strategies per constraint type
- **Performance Monitoring**: Multiple monitoring strategies (console, file, metrics)

## Architecture Decision Records (ADRs)

### ADR-001: Hexagonal Architecture Adoption
- **Context**: Current monolithic Program.cs violates documented architecture vision
- **Decision**: Implement full hexagonal architecture with clear port/adapter boundaries
- **Rationale**: Enables testability, maintainability, and supports 386 test preservation
- **Consequences**: Increased complexity initially, improved maintainability long-term
- **Alternatives Considered**: Layered architecture, service-oriented architecture

### ADR-002: Microsoft.Extensions.DependencyInjection Selection
- **Context**: Need lightweight, performant dependency injection container
- **Decision**: Use Microsoft.Extensions.DependencyInjection with reflection-based installers
- **Rationale**: Native .NET integration, proven performance, comprehensive features
- **Consequences**: <1ms resolution performance, standard .NET patterns
- **Alternatives Considered**: Autofac, Castle Windsor, Simple Injector

### ADR-003: Reflection-Based Service Discovery
- **Context**: Manual service registration creates maintenance overhead
- **Decision**: Implement automatic service discovery via reflection and naming conventions
- **Rationale**: Reduces boilerplate, improves maintainability, prevents registration errors
- **Consequences**: Slight startup cost, improved developer experience
- **Alternatives Considered**: Manual registration, assembly scanning, source generators

### ADR-004: Async-First API Design
- **Context**: MCP protocol and future I/O operations require async support
- **Decision**: Design all service interfaces as async from the beginning
- **Rationale**: Prevents blocking operations, supports future scalability, aligns with .NET best practices
- **Consequences**: Increased complexity, improved performance and scalability
- **Alternatives Considered**: Sync-only interfaces, mixed sync/async APIs

### ADR-005: Immutable Domain Objects
- **Context**: Constraint and configuration objects should prevent accidental mutation
- **Decision**: Design domain entities as immutable value objects with builder patterns
- **Rationale**: Prevents bugs, supports multi-threading, simplifies testing
- **Consequences**: More verbose object creation, improved safety and reliability
- **Alternatives Considered**: Mutable objects with validation, readonly properties

## Quality Attribute Scenarios

### Performance Scenarios

#### Scenario P1: MCP Request Processing
- **Source**: MCP client sends tool call request
- **Stimulus**: JSON-RPC request over stdio
- **Environment**: Normal operation with constraint activation
- **Artifact**: Complete request/response cycle
- **Response**: Request processed and enhanced with constraints
- **Response Measure**: p95 latency ≤ 50ms, memory usage ≤ 100MB

#### Scenario P2: Container Resolution Performance
- **Source**: Application service requires dependency injection
- **Stimulus**: Service resolution during request processing
- **Environment**: Normal operation with full container configuration
- **Artifact**: Dependency injection container
- **Response**: Service instance resolved successfully
- **Response Measure**: Resolution time < 1ms per service

### Security Architecture

#### Authentication and Authorization
- **Scope**: MCP server runs in user context, no authentication required
- **Authorization**: File system access controls for configuration files
- **Data Protection**: No sensitive data storage, temporary request data only

#### Input Validation
- **Request Validation**: JSON schema validation for MCP protocol compliance
- **Configuration Validation**: FluentValidation rules for YAML constraint packs
- **Injection Protection**: Sanitized constraint text injection with encoding

#### Error Handling Security
- **Information Disclosure**: Structured error responses without sensitive details
- **Exception Safety**: Controlled exception propagation with logging
- **Resource Protection**: Memory and CPU limits for constraint processing

### Scalability Design

#### Horizontal Scalability
- **Current Scope**: Single-process MCP server design
- **Future Considerations**: Multi-instance support via configuration partitioning
- **Resource Sharing**: Immutable constraint data enables safe sharing

#### Vertical Scalability
- **Memory Management**: Efficient object allocation with pooling strategies
- **CPU Optimization**: Compiled expression trees for constraint evaluation
- **I/O Optimization**: Asynchronous file operations with caching

#### Performance Monitoring
- **Metrics Collection**: Request duration, memory usage, constraint activation rates
- **Alerting**: Performance threshold violations with structured logging
- **Profiling**: Built-in profiling hooks for performance analysis

## Implementation Guidelines

### Development Approach
1. **Test-Driven Development**: All new components developed with TDD approach
2. **Incremental Migration**: Parallel change pattern (EXPAND → VALIDATE → MIGRATE → CONTRACT)
3. **Quality Gates**: Continuous validation of 386 test preservation and performance budgets
4. **Documentation**: Architecture decisions documented with rationale and trade-offs

### Code Organization
```
src/ConstraintMcpServer/
├── Domain/                          # Pure business logic
│   ├── Entities/                    # Core domain entities
│   ├── ValueObjects/                # Immutable value types
│   └── Services/                    # Domain service interfaces
├── Application/                     # Business workflows
│   ├── Services/                    # Application service implementations
│   ├── Workflows/                   # Multi-step business processes
│   └── Interfaces/                  # Application service contracts
└── Infrastructure/                  # External adapters
    ├── Hosting/                     # MCP server hosting
    ├── Configuration/               # YAML configuration loading
    ├── Logging/                     # Structured logging implementation
    └── DependencyInjection/         # Service registration
```

### Service Registration Strategy
```csharp
// Automatic service discovery via reflection
public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddConstraintServices(this IServiceCollection services)
    {
        // Domain services (Singleton - stateless)
        services.AddSingleton<IConstraintSelector, ConstraintSelector>();

        // Application services (Scoped - per request)
        services.AddScoped<IConstraintService, ConstraintService>();
        services.AddScoped<IContextAnalyzer, ContextAnalyzer>();

        // Infrastructure services (Singleton - expensive to create)
        services.AddSingleton<IMcpServer, McpServerAdapter>();
        services.AddSingleton<IConfigurationReader, YamlConfigurationReader>();
        services.AddSingleton<IStructuredLogger, NdjsonStructuredLogger>();

        return services;
    }
}
```

### Error Handling Strategy
- **Domain Exceptions**: Business rule violations with specific error types
- **Application Exceptions**: Service-level errors with operation context
- **Infrastructure Exceptions**: Technical failures with retry and circuit breaker patterns
- **Logging**: Structured error logging with correlation IDs and performance metrics

### Testing Strategy
- **Unit Tests**: Domain logic tested in isolation with comprehensive coverage
- **Integration Tests**: Service layer tested with real dependencies
- **Architecture Tests**: NetArchTest validation of dependency rules
- **Performance Tests**: Automated benchmarks for latency and memory requirements

This architectural design provides a solid foundation for Phase 1 implementation while ensuring all quality attributes are preserved and enhanced for future phases.