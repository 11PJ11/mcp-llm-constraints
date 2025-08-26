# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This repository contains a **Constraint Enforcement MCP Server** - a deterministic system that keeps LLM coding agents (like Claude Code) aligned during code generation with composable software-craft constraints (TDD, Hexagonal Architecture, SOLID, YAGNI, etc.). The system injects constraint reminders at MCP tool boundaries to prevent model drift.

**Key Features:**
- **Deterministic scheduling** with sub-50ms p95 latency per tool call
- **YAML constraint packs** with priority-based selection
- **MCP stdio pass-through** with injection at session boundaries
- **Composable constraints** for TDD, architectural patterns, and code quality
- **Structured logging** (NDJSON) for offline analysis

**Status:** Walking skeleton (v0.1) — local-first, deterministic, test-driven.

## Development Commands

### Prerequisites
Ensure .NET 8.0 SDK is installed. If using WSL/Linux, the SDK is installed at `/home/alexd/.dotnet`.

### Building
```bash
# Restore packages
dotnet restore

# Build the solution
dotnet build

# Build for release
dotnet build --configuration Release
```

### Testing
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults

# Run specific test project
dotnet test McpLlmConstraints.Tests
```

### Code Quality & Pre-commit Validation
```bash
# 🔒 RECOMMENDED: Run quality check before committing (prevents CI/CD failures)
./scripts/pre-commit-check.sh           # Linux/macOS/WSL
./scripts/pre-commit-check.ps1          # Windows PowerShell
./scripts/pre-commit-check.ps1 -SkipMutationTesting  # Faster for development

# Manual quality gates (same as CI/CD runs)
./scripts/quality-gates.sh              # Full quality validation
scripts/quality-gates.ps1               # Windows version

# Quick formatting fixes
dotnet format                           # Apply code formatting
dotnet format --verify-no-changes       # Check formatting compliance
```

**⚡ Pre-commit Hook**: Automatically installed - runs quality gates before every `git commit` to prevent CI/CD failures and save development time.

### Development Workflow
```bash
# Clean build outputs
dotnet clean

# Full clean build with quality checks
dotnet clean && dotnet restore && ./scripts/quality-gates.sh

# Run walking skeleton (first e2e check)
dotnet run -- --help

# Start with explicit config (development)
DOTNET_ENVIRONMENT=Development dotnet run -- --config ./config/constraints.yaml
```

## Solution Architecture

The solution follows **Hexagonal Architecture** with bounded contexts:

### Expected Project Structure (per ARCHITECTURE.md)
```
/src/ConstraintMcpServer/
  /Application/          # Scheduling, selection, injection logic
  /Domain/              # Constraint, Schedule, Phase domain types
  /Infrastructure/
    /Mcp/               # MCP stdio JSON-RPC server implementation
    /Config/            # YamlDotNet + FluentValidation for YAML loading
    /Logging/           # Serilog NDJSON structured logging
/config/
  constraints.yaml      # Constraint definitions with priorities and phases
  schedule.yaml        # Injection cadence and anchor configuration
/tests/
  ConstraintMcpServer.Tests/  # NUnit + FluentAssertions + Stryker.NET
```

### Core Components
- **JSON-RPC/stdio Handler** - MCP protocol implementation
- **Deterministic Scheduler** - Decides when to inject (every N interactions, phase overrides)
- **Constraint Selector** - Priority and phase-aware constraint selection
- **Injector** - Injects anchors + top-K reminders into tool boundaries
- **Config Loader** - YAML constraint pack loading with validation

### Performance Requirements
- **Sub-50ms p95 latency** per tool call handler
- **Sub-100MB memory ceiling** for server process
- **Deterministic behavior** - same inputs produce same outputs

### Code Quality Standards
- **Strict Analysis** - Warnings treated as errors, comprehensive static analysis
- **Code Style** - StyleCop analyzers with .editorconfig formatting rules
- **Security** - Security analyzers enabled for vulnerability detection
- **Documentation** - XML documentation required for public APIs
- **Testing** - NUnit + FluentAssertions + Stryker.NET mutation testing

### Configuration Files
- `Directory.Build.props` - Global project properties and analyzer configuration
- `.editorconfig` - Code formatting and style rules
- `stylecop.json` - StyleCop analyzer configuration
- `scripts/quality-gates.*` - Automated quality validation scripts

## Constraint System Design

### YAML Constraint Packs
Constraints are defined in `constraints.yaml` with this structure:
```yaml
version: "0.1.0"
constraints:
  - id: tdd.test-first
    title: "Write a failing test first"
    priority: 0.92                    # 0..1 priority for selection
    phases: [kickoff, red, commit]    # TDD phases where applicable
    reminders:
      - "Start with a failing test (RED) before implementation."
```

### Bounded Contexts & Ports
- **Constraint Catalog** - `IConstraintPackReader`, `IConstraintValidator`
- **Enforcement** - `IScheduler`, `IInjector`, `IPhaseTracker`  
- **Observation** - `IEventLogger`, `IMetrics`
- **Learning (offline)** - `ILogReader`, `IWeightsExporter`

### MCP Request Lifecycle
1. JSON-RPC receives tool call over stdio
2. Session Manager updates counters/phase
3. Deterministic Scheduler decides inject vs pass-through
4. Constraint Selector picks top-K by priority & phase
5. Injector adds anchor prologue/epilogue + reminders
6. Forward request; emit structured NDJSON event

## Development Guidelines

### Testing Strategy (per ARCHITECTURE.md)
- **E2E Test 1:** `dotnet run -- --help` prints usage with exit code 0
- **E2E Test 2:** MCP `initialize` round-trip over stdio
- **Unit Tests:** Constraint selector chooses top-K by priority/phase; scheduler cadence
- **Integration Tests:** Injection path places anchors + reminders; logs events
- **Mutation Testing:** Stryker.NET on core domain rules
- **Performance Tests:** p95 < 50ms handler path enforced in CI

### Current vs Target Dependencies
**Current setup uses:** xUnit + standard analyzers
**Target dependencies:** NUnit + FluentAssertions + YamlDotNet + FluentValidation + Serilog + Stryker.NET

### Code Standards
- **Hexagonal Architecture** - Domain at center, adapters around (MCP stdio, YAML config, logging)
- **Deterministic & Reproducible** - Same inputs → same outputs; version everything
- **Performance-First** - Sub-50ms p95 latency per tool call on typical projects
- **Local-First** - No telemetry exfiltration; structured logs stay on disk

### Roadmap Implementation Order
1. ✅ CLI `--help` e2e (pipeline + packaging proven)  
2. ⏭ MCP `initialize` round-trip over stdio
3. YAML load + deterministic scheduler + anchors
4. Top-K selection + injection + structured logging  
5. Performance budgets enforced in CI
6. Drift indicators via `match.keywords/regex`

### Quality Gates
All code must pass before commit:
1. **Performance:** p95 < 50ms handler path, p99 < 100ms
2. **Memory:** ≤100MB resident for server process
3. **Build:** Zero warnings, all tests passing
4. **Format:** Code properly formatted (dotnet format)
5. **Analysis:** Static analysis and security scans clean
6. **Mutation:** Stryker.NET mutation testing on domain logic