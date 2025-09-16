# Architecture Diagrams - MCP LLM Constraints Phase 1

## Visual Architecture Documentation

This document provides comprehensive visual representations of the Phase 1 infrastructure foundation architecture, showing component relationships, data flows, and interaction patterns.

## System Context Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    MCP Client Environment                       │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────┐      │
│  │ Claude Code │    │   VS Code   │    │ Other MCP       │      │
│  │             │    │ Extensions  │    │ Compatible      │      │
│  │             │    │             │    │ Tools           │      │
│  └─────────────┘    └─────────────┘    └─────────────────┘      │
└─────────────┬───────────────┬───────────────────┬────────────────┘
              │               │                   │
              ▼               ▼                   ▼
         JSON-RPC over stdio protocol
              │               │                   │
              ▼               ▼                   ▼
┌─────────────────────────────────────────────────────────────────┐
│              Constraint Enforcement MCP Server                  │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │                 Hexagonal Architecture                  │    │
│  │                                                         │    │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐   │    │
│  │  │ Infrastructure│  │  Application │  │    Domain    │   │    │
│  │  │   Adapters   │  │   Services   │  │    Core      │   │    │
│  │  │              │  │              │  │              │   │    │
│  │  │ • MCP Server │  │ • Constraint │  │ • Constraint │   │    │
│  │  │ • Config     │  │   Service    │  │   Pack       │   │    │
│  │  │ • Logging    │  │ • Context    │  │ • Selector   │   │    │
│  │  │ • Metrics    │  │   Analyzer   │  │ • Priority   │   │    │
│  │  └──────────────┘  └──────────────┘  └──────────────┘   │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │                    Data Sources                         │    │
│  │  ┌────────────────┐       ┌─────────────────────────┐   │    │
│  │  │ constraints.   │       │    Performance         │   │    │
│  │  │ yaml           │       │    Metrics Store       │   │    │
│  │  │                │       │                        │   │    │
│  │  │ • TDD Pack     │       │ • Request Latency      │   │    │
│  │  │ • Refactoring  │       │ • Memory Usage         │   │    │
│  │  │ • Clean Code   │       │ • Constraint Stats     │   │    │
│  │  └────────────────┘       └─────────────────────────┘   │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

## Hexagonal Architecture Detail

```
                    ╔═══════════════════════════════════════════════════════════════╗
                    ║                     DOMAIN CORE (Inner Hexagon)               ║
                    ║                                                               ║
                    ║  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐    ║
                    ║  │ Constraint  │  │ Constraint  │  │    Session         │    ║
                    ║  │    Pack     │  │  Selector   │  │    Context         │    ║
                    ║  │             │  │             │  │                    │    ║
                    ║  │ • Version   │  │ • Priority  │  │ • Request Data     │    ║
                    ║  │ • Rules     │  │ • Phase     │  │ • Phase Detection  │    ║
                    ║  │ • Metadata  │  │ • Selection │  │ • Tool Context     │    ║
                    ║  └─────────────┘  └─────────────┘  └─────────────────────┘    ║
                    ║                           │                                   ║
                    ║  ┌─────────────┐         │         ┌─────────────────────┐    ║
                    ║  │   Priority  │         │         │       Phase        │    ║
                    ║  │ (Value Obj) │         │         │   (Enumeration)    │    ║
                    ║  │             │         │         │                    │    ║
                    ║  │ • Score     │         │         │ • kickoff          │    ║
                    ║  │ • Weight    │         │         │ • red              │    ║
                    ║  │ • Context   │         │         │ • green            │    ║
                    ║  └─────────────┘         │         └─────────────────────┘    ║
                    ╚═══════════════════════════│═══════════════════════════════════╝
                                               │
              ╔════════════════════════════════│══════════════════════════════════════════╗
              ║              APPLICATION SERVICES (Middle Layer)                         ║
              ║                                │                                          ║
              ║  ┌─────────────────────────────┼─────────────────────────────────────┐   ║
              ║  │             IConstraintService (Orchestration)                    │   ║
              ║  │                              │                                    │   ║
              ║  │  SelectConstraintsAsync() ◄──┼──► InjectConstraintsAsync()        │   ║
              ║  └─────────────────────────────┼─────────────────────────────────────┘   ║
              ║                                │                                          ║
              ║  ┌────────────────┐           │           ┌─────────────────────────┐   ║
              ║  │ IContextAnalyzer│           │           │ IConstraintRepository  │   ║
              ║  │                │           │           │                        │   ║
              ║  │ • AnalyzeAsync │◄──────────┼──────────►│ • GetConstraintsAsync │   ║
              ║  │ • DetectPhase  │           │           │ • ValidateAsync       │   ║
              ║  └────────────────┘           │           └─────────────────────────┘   ║
              ║                                │                                          ║
              ║  ┌─────────────────────────────┼─────────────────────────────────────┐   ║
              ║  │             IValidationService                                     │   ║
              ║  │                              │                                    │   ║
              ║  │  ValidateConfigAsync() ◄─────┼─────► ValidateConstraintAsync()    │   ║
              ║  └─────────────────────────────┼─────────────────────────────────────┘   ║
              ╚════════════════════════════════│══════════════════════════════════════════╝
                                               │
          ╔═══════════════════════════════════│════════════════════════════════════════════════╗
          ║                    INFRASTRUCTURE ADAPTERS (Outer Hexagon)                         ║
          ║                                   │                                                ║
          ║  ┌──────────────────────────────┬─┼─┬──────────────────────────────────────────┐   ║
          ║  │          IMcpServer          │ │ │          IConfigurationReader           │   ║
          ║  │                              │ │ │                                        │   ║
          ║  │ • StartAsync()              │ │ │ • LoadAsync()                          │   ║
          ║  │ • ProcessRequestAsync()     │ │ │ • ValidateAsync()                      │   ║
          ║  │ • StopAsync()               │ │ │ • WatchForChanges()                    │   ║
          ║  └──────────────────────────────┼─┼─┼──────────────────────────────────────────┘   ║
          ║                                 │ │ │                                             ║
          ║  ┌──────────────────────────────┼─┼─┼──────────────────────────────────────────┐   ║
          ║  │       IStructuredLogger      │ │ │       IPerformanceMetrics              │   ║
          ║  │                              │ │ │                                        │   ║
          ║  │ • LogConstraintSelection()   │ │ │ • RecordLatency()                      │   ║
          ║  │ • LogPerformanceMetric()     │ │ │ • RecordMemoryUsage()                  │   ║
          ║  │ • LogError()                 │ │ │ • GenerateReport()                     │   ║
          ║  └──────────────────────────────┼─┼─┼──────────────────────────────────────────┘   ║
          ║                                 │ │ │                                             ║
          ║                                 │ │ │  ┌─────────────────────────────────────┐   ║
          ║                                 │ │ └─►│     File System                    │   ║
          ║                                 │ │    │ • constraints.yaml                │   ║
          ║                                 │ │    │ • performance.log                 │   ║
          ║                                 │ │    │ • application.ndjson              │   ║
          ║                                 │ └────┤                                   │   ║
          ║                                 │      └─────────────────────────────────────┘   ║
          ║                                 │                                                ║
          ║                                 └──────► JSON-RPC stdio                         ║
          ╚════════════════════════════════════════════════════════════════════════════════════╝
```

## Component Interaction Flow

```
  MCP Client                          Constraint MCP Server
      │                                       │
      │ 1. JSON-RPC Request                   │
      ├──────────────────────────────────────►│
      │                                       │
      │                                       ▼
      │                              ┌─────────────────┐
      │                              │  McpServerHost  │◄─── Infrastructure
      │                              │   (Adapter)     │     Layer
      │                              └─────────┬───────┘
      │                                        │
      │                                        ▼
      │                              ┌─────────────────┐
      │                              │ ConstraintService│◄─── Application
      │                              │  (Orchestrator) │     Layer
      │                              └─────────┬───────┘
      │                                        │
      │                              ┌─────────▼───────┐
      │                              │ ContextAnalyzer │
      │                              │   (Service)     │
      │                              └─────────┬───────┘
      │                                        │
      │                                        ▼
      │                              ┌─────────────────┐
      │                              │ConstraintSelector│◄─── Domain
      │                              │    (Domain)     │     Layer
      │                              └─────────┬───────┘
      │                                        │
      │                              ┌─────────▼───────┐
      │                              │ConstraintRepository│
      │                              │   (Service)     │
      │                              └─────────┬───────┘
      │                                        │
      │                              ┌─────────▼───────┐
      │                              │StructuredLogger │◄─── Infrastructure
      │                              │   (Adapter)     │     Layer
      │                              └─────────┬───────┘
      │                                        │
      │                                        ▼
      │ 2. Enhanced JSON-RPC Response           │
      │◄──────────────────────────────────────┤
      │                                       │
```

## Data Flow Architecture

```
                    Configuration Loading Flow
                    ═════════════════════════════
    ┌─────────────────┐
    │ constraints.yaml│
    └─────────┬───────┘
              │ 1. File Read
              ▼
    ┌─────────────────────────────┐
    │  YamlConfigurationReader    │ ◄─── Infrastructure
    │        (Adapter)           │
    └─────────┬───────────────────┘
              │ 2. Deserialize
              ▼
    ┌─────────────────────────────┐
    │  ConstraintPackConverter    │
    │     (Infrastructure)       │
    └─────────┬───────────────────┘
              │ 3. Convert to POCO
              ▼
    ┌─────────────────────────────┐
    │   FluentValidation         │
    │     (Infrastructure)       │
    └─────────┬───────────────────┘
              │ 4. Validate
              ▼
    ┌─────────────────────────────┐
    │   ConstraintPack           │ ◄─── Domain
    │     (Domain Entity)        │
    └─────────┬───────────────────┘
              │ 5. Store
              ▼
    ┌─────────────────────────────┐
    │  ConstraintRepository      │ ◄─── Application
    │     (Service)              │
    └─────────────────────────────┘

                    Request Processing Flow
                    ═══════════════════════════
    ┌─────────────────┐
    │ MCP JSON Request│
    └─────────┬───────┘
              │ 1. Receive
              ▼
    ┌─────────────────────────────┐
    │     McpServerHost          │ ◄─── Infrastructure
    │       (Adapter)            │
    └─────────┬───────────────────┘
              │ 2. Parse & Route
              ▼
    ┌─────────────────────────────┐
    │   ConstraintService        │ ◄─── Application
    │    (Orchestrator)          │
    └─────────┬───────────────────┘
              │ 3. Analyze Context
              ▼
    ┌─────────────────────────────┐
    │   ContextAnalyzer          │
    │     (Service)              │
    └─────────┬───────────────────┘
              │ 4. Detect Phase
              ▼
    ┌─────────────────────────────┐
    │  ConstraintSelector        │ ◄─── Domain
    │     (Domain Logic)         │
    └─────────┬───────────────────┘
              │ 5. Select Constraints
              ▼
    ┌─────────────────────────────┐
    │   ConstraintInjector       │ ◄─── Application
    │     (Service)              │
    └─────────┬───────────────────┘
              │ 6. Enhance Request
              ▼
    ┌─────────────────────────────┐
    │  Enhanced JSON Response     │
    │                            │
    └─────────────────────────────┘
```

## Dependency Injection Architecture

```
                    Service Container Architecture
                    ═══════════════════════════════

    ┌─────────────────────────────────────────────────────────────────┐
    │                      IServiceCollection                         │
    │                     (Microsoft.Extensions)                     │
    └─┬───────────────────────┬───────────────────────┬───────────────┘
      │                       │                       │
      ▼                       ▼                       ▼
    ┌───────────────┐    ┌─────────────────┐    ┌─────────────────┐
    │   Domain      │    │   Application   │    │ Infrastructure  │
    │ Services      │    │   Services      │    │   Adapters      │
    │               │    │                 │    │                 │
    │ Singleton     │    │ Scoped          │    │ Singleton       │
    │               │    │                 │    │                 │
    │ • Selector    │    │ • Context       │    │ • MCP Server    │
    │ • Priority    │    │   Analyzer      │    │ • Config Reader │
    │ • Validator   │    │ • Constraint    │    │ • Logger        │
    │               │    │   Service       │    │ • Metrics       │
    └───────────────┘    └─────────────────┘    └─────────────────┘

    Service Lifetime Justification:

    ┌─────────────────────────────────────────────────────────────────┐
    │ Singleton (Expensive to create, Stateless)                     │
    │ • Domain Services: Pure business logic, no state               │
    │ • Infrastructure: Connection pools, loggers, configuration     │
    │                                                                 │
    │ Scoped (Per-request state, Request isolation)                  │
    │ • Application Services: Request context, session state         │
    │ • Business Workflows: Multi-step operations with state         │
    │                                                                 │
    │ Transient (Factory created, Short-lived)                       │
    │ • Value Objects: Created on demand, immutable                  │
    │ • Result Objects: Operation results, data transfer objects     │
    └─────────────────────────────────────────────────────────────────┘

    Resolution Performance Requirements:

    ┌─────────────────────────────────────────────────────────────────┐
    │ Performance Budget: < 1ms per service resolution               │
    │                                                                 │
    │ Optimization Techniques:                                        │
    │ • Compiled Expression Trees                                     │
    │ • Constructor Caching                                           │
    │ • Singleton Instance Reuse                                      │
    │ • Minimal Reflection (startup only)                            │
    │                                                                 │
    │ Monitoring:                                                     │
    │ • Resolution time tracking                                      │
    │ • Memory allocation monitoring                                  │
    │ • Service activation counters                                   │
    └─────────────────────────────────────────────────────────────────┘
```

## Error Handling Architecture

```
                    Exception Flow Architecture
                    ══════════════════════════════

    ┌─────────────────────────────────────────────────────────────────┐
    │                    Error Origination Points                     │
    └─┬───────────────────┬───────────────────────┬───────────────────┘
      │                   │                       │
      ▼                   ▼                       ▼
   Domain Layer      Application Layer      Infrastructure Layer
      │                   │                       │
      │ Business Rule     │ Service              │ Technical
      │ Violations        │ Failures             │ Failures
      │                   │                       │
      ▼                   ▼                       ▼
   ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
   │Domain           │ │Application      │ │Infrastructure   │
   │Exceptions       │ │Exceptions       │ │Exceptions       │
   │                 │ │                 │ │                 │
   │• Constraint     │ │• Service        │ │• MCP Protocol   │
   │  Validation     │ │  Unavailable    │ │  Error          │
   │• Invalid Phase  │ │• Configuration  │ │• File System    │
   │• Priority Range │ │  Error          │ │  Error          │
   └─────────┬───────┘ └─────────┬───────┘ └─────────┬───────┘
             │                   │                   │
             └───────────────────┼───────────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────────┐
                    │      Global Exception       │
                    │         Handler            │
                    │                            │
                    │ • Context Preservation     │
                    │ • Structured Logging       │
                    │ • Error Correlation        │
                    │ • Client Error Response    │
                    └─────────────────────────────┘

    Error Response Flow:

    ┌─────────────────────────────────────────────────────────────────┐
    │ 1. Exception Caught                                             │
    │    ↓                                                            │
    │ 2. Context Captured (Operation, Request ID, Timestamp)         │
    │    ↓                                                            │
    │ 3. Structured Log Entry Created                                 │
    │    ↓                                                            │
    │ 4. Public Error Message Generated (Sanitized)                  │
    │    ↓                                                            │
    │ 5. MCP Error Response Formatted                                 │
    │    ↓                                                            │
    │ 6. Client Receives Safe Error Message                          │
    └─────────────────────────────────────────────────────────────────┘
```

## Performance Monitoring Architecture

```
                    Performance Instrumentation
                    ═══════════════════════════════

    ┌─────────────────────────────────────────────────────────────────┐
    │                  Performance Collection Points                  │
    └─┬───────────────────┬───────────────────────┬───────────────────┘
      │                   │                       │
      ▼                   ▼                       ▼
   Request Level      Component Level         System Level
      │                   │                       │
      ▼                   ▼                       ▼
   ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
   │• Total Latency  │ │• Service        │ │• Memory Usage   │
   │• Constraint     │ │  Resolution     │ │• CPU Usage      │
   │  Selection Time │ │• Configuration  │ │• GC Pressure    │
   │• Injection Time │ │  Load Time      │ │• Thread Pool    │
   │• Response Size  │ │• Validation     │ │• Handle Count   │
   └─────────┬───────┘ └─────────┬───────┘ └─────────┬───────┘
             │                   │                   │
             └───────────────────┼───────────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────────┐
                    │     Performance Metrics     │
                    │         Aggregator          │
                    │                            │
                    │ • Real-time Monitoring     │
                    │ • Threshold Alerting       │
                    │ • Trend Analysis           │
                    │ • Performance Reports      │
                    └─────────────────────────────┘

    Performance Budget Enforcement:

    ┌─────────────────────────────────────────────────────────────────┐
    │ Budget Item                 │ Target        │ Alert Threshold   │
    │─────────────────────────────│───────────────│──────────────────│
    │ Request Latency (p95)       │ ≤ 50ms        │ > 45ms           │
    │ Request Latency (p99)       │ ≤ 100ms       │ > 90ms           │
    │ Memory Usage (RSS)          │ ≤ 100MB       │ > 85MB           │
    │ Service Resolution          │ < 1ms         │ > 0.8ms          │
    │ Configuration Load          │ < 10ms        │ > 8ms            │
    │ Constraint Selection        │ < 5ms         │ > 4ms            │
    │─────────────────────────────│───────────────│──────────────────│
    │ Continuous Monitoring       │ < 1% CPU      │ > 0.8% CPU       │
    │ Logging Overhead           │ < 2% Total    │ > 1.5% Total     │
    └─────────────────────────────────────────────────────────────────┘
```

## Security Architecture Diagram

```
                    Security Boundary Architecture
                    ═════════════════════════════════

    External Environment                Internal Environment
    ┌─────────────────────┐            ┌─────────────────────────────┐
    │                     │            │                             │
    │ ┌─────────────────┐ │            │    Constraint MCP Server    │
    │ │   MCP Client    │ │            │                             │
    │ │                 │ │            │ ┌─────────────────────────┐ │
    │ │ • Claude Code   │ │            │ │    Input Validation     │ │
    │ │ • VS Code       │ │            │ │                         │ │
    │ │ • Other Tools   │ │            │ │ • JSON Schema Check     │ │
    │ └─────────────────┘ │            │ │ • Protocol Validation   │ │
    │                     │            │ │ • Business Rule Check   │ │
    └─────────────────────┘            │ │ • Input Sanitization    │ │
                │                      │ └─────────────────────────┘ │
                │ JSON-RPC             │               │             │
                │ over stdio           │               ▼             │
                ▼                      │ ┌─────────────────────────┐ │
    ═══════════════════════════════════│ │   Security Boundary    │ │
           Trust Boundary              │ │                         │ │
    ═══════════════════════════════════│ │ • User Context Only     │ │
                                      │ │ • No Network Access     │ │
                                      │ │ • File System Limited   │ │
                                      │ │ • No Privileged Ops     │ │
                                      │ └─────────────────────────┘ │
                                      │               │             │
                                      │               ▼             │
                                      │ ┌─────────────────────────┐ │
                                      │ │   Error Sanitization    │ │
                                      │ │                         │ │
                                      │ │ • Safe Error Messages   │ │
                                      │ │ • No Internal Details   │ │
                                      │ │ • Correlation IDs Only  │ │
                                      │ │ • Structured Logging    │ │
                                      │ └─────────────────────────┘ │
                                      └─────────────────────────────┘

    Security Controls by Layer:

    ┌─────────────────────────────────────────────────────────────────┐
    │ Infrastructure Layer                                            │
    │ • Input validation and sanitization                            │
    │ • Resource limits and quotas                                   │
    │ • Secure error handling                                        │
    │ • Audit logging                                                │
    │                                                                │
    │ Application Layer                                              │
    │ • Business rule enforcement                                    │
    │ • Authorization checks                                         │
    │ • Data validation                                              │
    │ • Session management                                           │
    │                                                                │
    │ Domain Layer                                                   │
    │ • Invariant enforcement                                        │
    │ • Business logic integrity                                     │
    │ • Immutable objects                                            │
    │ • Pure functions                                               │
    └─────────────────────────────────────────────────────────────────┘
```

This comprehensive set of architecture diagrams provides clear visual guidance for implementing the Phase 1 infrastructure foundation while maintaining all quality attributes and performance requirements.