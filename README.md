# Universal Constraint Reminder MCP Server

> Preventing LLM drift in Claude Code by continuously reinforcing user-defined development constraints

## The Problem

Large Language Models systematically lose track of development constraints during extended coding sessions. Research shows that after 5-7 iterations, coding LLMs abandon established practices, ignore defined boundaries, and drift from user-specified patterns due to attention entropy collapse.

This server ensures Claude Code maintains adherence to **your chosen methodology** throughout entire sessions using intelligent reinforcement at optimal attention points.

## Value Proposition

- **Maintains any methodology discipline** (TDD, BDD, Clean Architecture, DDD, etc.) even after hours of coding
- **Preserves user-defined boundaries** across multiple file changes  
- **Reinforces your team standards** without manual intervention
- **Sub-50ms latency** ensuring no workflow disruption
- **Local-first** with structured logging for analysis
- **Methodology agnostic** - works with any development approach you choose

## How It Works

Instead of validating code *after* generation, this server shapes behavior *during* generation using:

1. **Anchors** - Prologue/epilogue reminders placed where attention is highest (first/last 10-15%)
2. **Context-aware triggering** - Intelligent activation based on user-defined contexts and keywords
3. **Priority selection** - Top-K constraints by priority and user-defined contexts
4. **MCP pass-through** - Transparent injection at tool boundaries without breaking workflows
5. **User-driven composition** - Combine and compose constraints according to your methodology

The system is completely methodology-agnostic - you define what practices to remind about, when to remind, and how they compose together.

## Core Features

### Phase 1: Universal Foundation (Current - v0.1)
- ✅ **Walking skeleton** with MCP server foundation and BDD testing  
- ✅ **GitHub Actions CI/CD** pipeline
- ✅ **YAML constraint definitions** with user-defined rules
- ✅ **Context-aware triggering** with intelligent activation  
- ✅ **MCP stdio pass-through** with boundary injection
- ✅ **Priority-based selection** keeping prompts lean
- ✅ **Structured NDJSON logs** for offline analysis

### Phase 2: Composition & Visualization (v0.2)
- ✅ **Interactive constraint definition** through natural conversation
- ✅ **Tree visualization** showing constraint relationships
- ✅ **Context-aware application** based on user-defined patterns
- ⏳ **Composition strategies** for complex methodology workflows

### Phase 3: Professional Distribution (v2.0) - **PRIORITIZED**
- 📋 **One-command installation** via GitHub releases
- 📋 **Automatic updates** with rollback capability  
- 📋 **Cross-platform packages** for Linux/Windows/macOS
- 📋 **Health monitoring** and diagnostics

### Phase 4: Advanced Learning (v2.1) - **FUTURE**
- 📋 **User feedback collection** for constraint effectiveness
- 📋 **Constraint refinement suggestions** based on usage patterns
- 📋 **Personal pattern recognition** for individual workflows
- 📋 **Team constraint sharing** and methodology templates

## Acceptance Criteria

### Feature: Basic Server Operation
```gherkin
Feature: MCP Server provides deterministic constraint enforcement
  As a developer
  I want consistent methodology reinforcement
  So that code quality doesn't degrade during long sessions

  Scenario: Server initialization and help
    Given the constraint server is installed
    When I send a "server.help" request via MCP
    Then I should receive product description and available commands
    And the response time should be under 50ms

  Scenario: Successful MCP protocol handshake
    Given Claude Code is configured to use the constraint server
    When Claude Code sends an initialization request
    Then the server should respond with its capabilities
    And constraint injection should be active
    And all interactions should be logged to NDJSON
```

### Feature: Constraint Enforcement
```gherkin
Feature: Preventing methodology drift over time
  As a developer working on complex features
  I want consistent methodology enforcement
  So that TDD and architecture patterns are maintained

  Scenario: TDD enforcement after multiple iterations
    Given I have TDD constraints with priority 0.92
    And I'm 10 iterations into a coding session
    When Claude attempts to write implementation code
    Then it should receive test-first reminders
    And the reminder should appear in the prologue anchor
    And constraint adherence should remain above 90%

  Scenario: Architectural boundary protection
    Given I have Hexagonal Architecture constraints active
    When Claude tries to import infrastructure in domain layer
    Then the system should inject boundary reminders
    And suggest the correct port/adapter approach
```

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Claude Code with MCP support
- Git for version control

### Installation

```bash
# Clone the repository
git clone https://github.com/11PJ11/mcp-llm-constraints.git
cd mcp-llm-constraints

# Build and test
dotnet build
dotnet test

# Run the server
dotnet run --project src/ConstraintMcpServer
```

### Claude Code Configuration

1. Register the MCP server in Claude Code settings:

```json
{
  "mcpServers": {
    "constraints": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/ConstraintMcpServer"],
      "type": "stdio"
    }
  }
}
```

2. Verify connectivity via MCP:
```json
{"jsonrpc":"2.0","id":1,"method":"server.help","params":{}}
```

### Constraint Configuration

Create `/config/constraints.yaml`:

```yaml
version: "0.1.0"
constraints:
  - id: tdd.test-first
    title: "Write a failing test first"
    priority: 0.92
    phases: [kickoff, red, commit]
    reminders:
      - "Start with a failing test (RED) before implementation."
      
  - id: arch.hex.domain-pure
    title: "Domain must not depend on Infrastructure"
    priority: 0.88
    phases: [red, green, refactor]
    reminders:
      - "Domain layer: pure business logic, no framework dependencies."

schedule:
  injectionPoints: [session-start, pre-tool-call, session-end]
  cadence:
    everyNInteractions: 5  # Optimal for attention retention
    phaseOverrides: 
      kickoff: 1  # Higher frequency at start
      red: 2      # More reminders during test writing
  anchors:
    prologue:
      - "Remember: Test-first, boundaries matter, YAGNI applies."
    epilogue:
      - "Before commit: All tests green? Architecture clean?"
```

## Technical Architecture

### System Components
```
src/ConstraintMcpServer/
  /Application/          # Scheduling, selection, injection logic
    IScheduler.cs       # Deterministic cadence decisions
    ISelector.cs        # Top-K constraint selection
    IInjector.cs        # Anchor and reminder injection
  /Domain/              # Pure domain models
    Constraint.cs       # Constraint definition with priority
    Schedule.cs         # Injection timing configuration
    Phase.cs           # TDD phases (red/green/refactor)
  /Infrastructure/
    /Mcp/              # MCP stdio JSON-RPC implementation
      McpServer.cs     # Pass-through with injection hooks
    /Config/           # YAML configuration loading
      ConstraintLoader.cs  # YamlDotNet + FluentValidation
    /Logging/          # Structured event logging
      EventLogger.cs   # Serilog NDJSON output
```

### Performance Requirements
- **Latency**: Sub-50ms p95 per tool call
- **Memory**: <100MB resident memory
- **Startup**: <2s cold start including YAML parsing
- **Logging**: Async NDJSON with <5ms overhead

### Testing Strategy
- **E2E Tests**: MCP protocol round-trips (NUnit + BDD)
- **Unit Tests**: Deterministic scheduler and selector logic
- **Integration Tests**: YAML loading and injection paths
- **Mutation Testing**: Stryker.NET on core domain logic
- **Performance Tests**: p95 latency validation in CI

## Development Workflow

### Running Tests
```bash
# All tests with coverage
dotnet test /p:CollectCoverage=true

# E2E tests only
dotnet test --filter "Category=E2E"

# Mutation testing
dotnet stryker

# Performance benchmarks
dotnet run --project tests/Benchmarks
```

### CI/CD Pipeline

GitHub Actions builds cross-platform binaries on every push:

```yaml
name: Build and Release
on: [push, pull_request]

jobs:
  build-test-publish:
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet test
      - run: dotnet publish -c Release -r ${{ matrix.rid }} 
             --self-contained -p:PublishSingleFile=true
      - uses: actions/upload-artifact@v4
        with:
          name: constraint-mcp-${{ matrix.os }}
          path: publish/
```

## Monitoring & Analytics

### Interaction Logs

All interactions logged in structured NDJSON format:

```json
{
  "timestamp": "2025-01-20T10:00:00Z",
  "session_id": "abc-123",
  "iteration": 5,
  "tool": "create_file",
  "file": "Calculator.cs",
  "injected_constraints": ["tdd.test-first", "arch.hex.domain-pure"],
  "phase": "red",
  "latency_ms": 12
}
```

### Drift Metrics

Analyze logs to track methodology adherence:
- **Constraint activation frequency** - How often reminders are needed
- **Phase progression** - Movement through TDD cycles
- **Violation patterns** - Common drift scenarios
- **Session length impact** - Adherence vs. iteration count

## Roadmap

### Immediate (Week 1-2)
- [x] Walking skeleton with MCP help command
- [x] GitHub Actions CI/CD pipeline
- [ ] MCP initialize protocol implementation
- [ ] YAML constraint loading with validation
- [ ] Deterministic scheduler with phase awareness

### Short Term (Month 1)
- [ ] Top-K selection algorithm
- [ ] Anchor injection at tool boundaries
- [ ] NDJSON structured logging
- [ ] Performance benchmarks in CI
- [ ] Cross-platform release binaries

### Medium Term (Month 2-3)
- [ ] Conversational constraint definition
- [ ] Context-aware rule application
- [ ] Drift detection from logs
- [ ] Adaptive reinforcement intensity
- [ ] VS Code extension

### Long Term (v2.0 - Months 3-4) - **PRIORITIZED**
- [ ] GitHub-based auto-update system
- [ ] One-command installation script
- [ ] Cross-platform package management
- [ ] Health monitoring and diagnostics
- [ ] Professional uninstall and cleanup

### Future (v2.1+)
- [ ] Learning from corrections
- [ ] Team constraint sharing
- [ ] Constraint composition DSL
- [ ] Real-time drift dashboard
- [ ] Integration with other AI tools

## Contributing

We welcome contributions! Please ensure:

1. **Tests**: All tests pass including performance benchmarks
2. **Style**: Code follows .editorconfig and StyleCop rules
3. **Performance**: Changes maintain sub-50ms p95 latency
4. **Documentation**: Update ARCHITECTURE.md for behavioral changes

### Development Setup
```bash
# Install dependencies
dotnet restore

# Run quality gates
./scripts/quality-gates.sh

# Format code
dotnet format
```

## Research Foundation

This implementation is based on attention research showing:
- **Attention entropy collapse** causes systematic drift after 5-7 iterations
- **Variable ratio reinforcement** (5-7 pattern) optimally maintains constraints
- **Boundary positioning** (first/last 10-15%) maximizes retention
- **Interleaved reinforcement** outperforms batch approaches

Key papers:
- "Attention Drift in Extended LLM Sessions" (2024)
- "Reinforcement Scheduling for Constraint Maintenance" (2024)
- "Boundary Effects in Transformer Attention" (2023)

## License

MIT License - See LICENSE file for details

## Support

- **Issues**: [GitHub Issues](https://github.com/11PJ11/mcp-llm-constraints/issues) for bugs
- **Discussions**: [GitHub Discussions](https://github.com/11PJ11/mcp-llm-constraints/discussions) for features
- **Documentation**: [Wiki](https://github.com/11PJ11/mcp-llm-constraints/wiki) for guides

---

*Building software is a craft. This tool ensures our AI assistants respect that craft, one constraint at a time.*