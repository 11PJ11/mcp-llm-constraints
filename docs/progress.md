# PROGRESS.md — Walking Skeleton Plan (Agent‑Ready)

> This is a **step‑by‑step, executable plan**. Each step has: Goal → Tasks → Files → Commands → Acceptance. Follow in order. Keep commits small.

---

## Conventions & Guardrails
- **Runtime:** .NET 8
- **Style:** nullable enabled, warnings as errors
- **Tests:** NUnit (no FluentAssertions); BDD acceptance as Gherkin `.feature` files (SpecFlow optional)
- **Process:** stdio JSON‑RPC MCP server (no CLI help; help is exposed via an MCP method `server.help`)
- **Logging:** Serilog (NDJSON to stdout)
- **Package IDs:** use the exact IDs below
- **Commit format:** `feat|chore|test|docs(scope): message`

Repo layout target:
```
/ src
  / ConstraintMcpServer
    / Application
    / Domain
    / Infrastructure
      / Mcp
      / Config
      / Logging
/ config
  constraints.yaml
  schedule.yaml
/ tests
  ConstraintMcpServer.Tests
.github/workflows
  ci.yml
/docs
  ARCHITECTURE.md
```

---

## Step 0 — Preflight & Bootstrap ✅
**Goal:** Clean repo, solutions created, CI ready to run.

**Status:** ✅ **COMPLETED** (2024-08-20, commit: 447aafc)

**Tasks**
1. ✅ Initialize git and solution.
2. ✅ Add `.editorconfig`, `.gitignore` (dotnet), ~~LICENSE (TBD)~~, ~~`README.md`~~, ~~`ARCHITECTURE.md`~~.
3. ✅ Create solution and projects.

**Files Created**
- ✅ `.editorconfig` (minimal C# rules; treat warnings as errors)
- ✅ `.gitignore` from `dotnet new gitignore`
- ✅ `ConstraintMcpServer.sln` - Main solution file
- ✅ `Directory.Build.props` - .NET 8 configuration with nullable enabled
- ✅ `src/ConstraintMcpServer/ConstraintMcpServer.csproj` - Console application
- ✅ `tests/ConstraintMcpServer.Tests/ConstraintMcpServer.Tests.csproj` - NUnit test project
- ✅ `config/` directory for future YAML files

**Commands Executed**
```bash
# Create solution & projects
dotnet new sln -n ConstraintMcpServer
mkdir -p src/ConstraintMcpServer tests/ConstraintMcpServer.Tests config
pushd src/ConstraintMcpServer && dotnet new console -n ConstraintMcpServer && popd
pushd tests/ConstraintMcpServer.Tests && dotnet new nunit -n ConstraintMcpServer.Tests && popd

dotnet sln add src/ConstraintMcpServer/ConstraintMcpServer.csproj
dotnet sln add tests/ConstraintMcpServer.Tests/ConstraintMcpServer.Tests.csproj

dotnet add tests/ConstraintMcpServer.Tests/ConstraintMcpServer.Tests.csproj reference \
  src/ConstraintMcpServer/ConstraintMcpServer.csproj
```

**Acceptance** ✅
- ✅ `dotnet build` succeeds on a clean machine.
- ✅ Zero warnings, zero errors
- ✅ Proper project structure with src/, tests/, config/ directories
- ✅ NUnit test framework configured
- ✅ .NET 8 with nullable reference types enabled

---

## Step 1 — MCP `server.help` (First e2e) ✅
**Goal:** Prove the server exposes a discoverable **help** command **as an MCP method over stdio**, not as a CLI flag. This demonstrates pragmatic usability for agents/IDEs.

**Status:** ✅ **COMPLETED** (2024-08-21, commit: 8512934)
- ✅ MCP SDK integrated with Microsoft.Extensions.Hosting approach
- ✅ Basic MCP server app structure with hexagonal architecture
- ✅ BDD test framework implemented (ScenarioBuilder, McpServerSteps)
- ✅ E2E test `Mcp_ServerHelp_Is_Discoverable` is GREEN locally
- ✅ Serilog logging configuration for structured output
- ✅ CI/CD pipeline implemented with cross-platform builds
- ⏳ Server.help response content verification (delegated to next iteration)

**Tasks**
1. ✅ Add MCP SDK package: Using Microsoft ModelContextProtocol server
2. ✅ Implement basic MCP server structure with stdio transport
3. ✅ Add BDD-style e2e test infrastructure
4. ⏳ Verify server.help response contains proper business value content (future iteration)
5. ✅ Add GitHub Actions (build+test on Linux/Win/macOS)

**Files Created**
- ✅ `src/ConstraintMcpServer/Application/McpApp.cs` — MCP server orchestration
- ✅ `src/ConstraintMcpServer/Infrastructure/Mcp/StdioServer.cs` — stdio transport (stub)
- ✅ `src/ConstraintMcpServer/Infrastructure/Mcp/JsonRpcStdioHandler.cs` — JSON-RPC handling
- ✅ `src/ConstraintMcpServer/Infrastructure/Logging/LoggingConfiguration.cs` — Serilog setup
- ✅ `tests/ConstraintMcpServer.Tests/E2E/HelpE2E.cs` — BDD e2e test
- ✅ `tests/ConstraintMcpServer.Tests/Framework/ScenarioBuilder.cs` — BDD framework
- ✅ `tests/ConstraintMcpServer.Tests/Steps/McpServerSteps.cs` — test steps
- ✅ `.github/workflows/ci.yml` — complete CI/CD pipeline with matrix builds
- ✅ `config/constraints.yaml` — sample constraint configuration
- ✅ `config/README.md` — configuration documentation

**Commands**
```bash
dotnet test  # ✅ PASSES (1/1 tests green)
```

**Acceptance**
- ✅ Test `Mcp_ServerHelp_Is_Discoverable` is **green** locally and in CI
- ✅ CI pipeline implemented with cross-platform matrix builds
- ✅ Self-contained executables generated for Linux/Windows/macOS  
- ✅ Smoke testing validates executables start properly
- ✅ Code quality gates (formatting, static analysis) enforced
- ⏳ Server help response content verification (acceptable for walking skeleton)

---

## Step 2 — MCP stdio pass‑through (Initialize Round‑Trip) ✅
**Goal:** Speak MCP over stdio: accept `initialize`, return capabilities, support `shutdown`.

**Status:** ✅ **COMPLETED** (2024-08-21, commit: TBD)
- ✅ MCP initialize and shutdown methods implemented in JsonRpcStdioHandler
- ✅ Proper MCP capabilities response with constraint notifications advertised
- ✅ BDD E2E tests for full MCP handshake lifecycle implemented
- ✅ Protocol compliance validation (JSON-RPC 2.0, response IDs, error handling)
- ✅ Latency budget verification (< 100ms for E2E test environment)
- ✅ Clean session termination testing

**Tasks**
1. ✅ Add MCP SDK package (ModelContextProtocol already included)
2. ✅ Enhanced `Infrastructure/Mcp/JsonRpcStdioHandler.cs` with initialize/shutdown handlers
3. ✅ JSON-RPC loop handles MCP protocol messages correctly
4. ✅ E2E tests pipe `initialize` and `shutdown` JSON-RPC requests and validate responses

**Files Created/Updated**
- ✅ `src/ConstraintMcpServer/Infrastructure/Mcp/JsonRpcStdioHandler.cs` — enhanced with initialize/shutdown
- ✅ `tests/ConstraintMcpServer.Tests/E2E/McpInitializeE2E.cs` — full MCP handshake E2E tests
- ✅ `tests/ConstraintMcpServer.Tests/Steps/McpServerSteps.cs` — extended with MCP protocol steps

**Commands**
```bash
dotnet test  # ✅ PASSES (3/3 tests green)
```

**Acceptance**
- ✅ E2E `Mcp_Initialize_Should_ReturnCapabilities` passes
- ✅ E2E `Mcp_InitializeShutdown_Should_CompleteCleanly` passes  
- ✅ Response contains proper MCP capabilities: `{ tools: {}, resources: {}, notifications: { constraints: true } }`
- ✅ Protocol compliance validated (JSON-RPC 2.0, matching IDs, no errors)
- ✅ Latency budget verified (< 100ms for E2E environment)
- ✅ Server remains running after shutdown (long-running process model)
- ✅ No stderr exceptions during normal MCP operations

---

## Step 3 — Config Load & Validation (YAML)
**Goal:** Load `constraints.yaml` into domain types with validation errors reported clearly.

**Tasks**
1. Add packages:
   ```bash
   dotnet add src/ConstraintMcpServer package YamlDotNet
   dotnet add src/ConstraintMcpServer package FluentValidation
   ```
2. Create domain types in `Domain/`:
   - `Constraint.cs`, `Schedule.cs`, `ConstraintPack.cs`, enums `Phase`, `InjectionPoint`.
3. Create `Infrastructure/Config/YamlConstraintPackReader.cs` and validators in `Infrastructure/Config/Validators`.
4. Unit tests for valid/invalid YAML samples.

**Files**
- `config/constraints.yaml` — sample pack (from README)
- `src/ConstraintMcpServer/Domain/...` — types
- `src/ConstraintMcpServer/Infrastructure/Config/...` — loader + validators
- `tests/ConstraintMcpServer.Tests/ConfigTests.cs` — loads both good/bad YAML

**Commands**
```bash
dotnet test
```

**Acceptance**
- Invalid YAML fails with actionable messages (duplicate IDs, priority out of range, empty reminders, unknown phases).
- Valid YAML loads into `ConstraintPack`.

---

## Step 4 — Deterministic Scheduler & Session State
**Goal:** Decide inject vs pass‑through based on cadence and phase; maintain per‑session counters.

**Tasks**
1. Add `Application/Scheduling/Scheduler.cs` with:
   - `everyNInteractions` logic
   - `phaseOverrides` for `kickoff`, `red`
2. Add `Application/Session/SessionManager.cs` (tracks interaction count, phase transitions).
3. Unit tests: given interaction index & phase → expected inject decision.

**Files**
- `src/ConstraintMcpServer/Application/Scheduling/Scheduler.cs`
- `src/ConstraintMcpServer/Application/Session/SessionManager.cs`
- `tests/ConstraintMcpServer.Tests/SchedulerTests.cs`

**Commands**
```bash
dotnet test
```

**Acceptance**
- Deterministic decisions for fixed inputs; tests cover boundaries (N=1, N=3, phase overrides).

---

## Step 5 — Selection & Injection
**Goal:** Pick top‑K constraints by priority filtered by phase; inject anchors + reminders.

**Tasks**
1. Add `Application/Selection/ConstraintSelector.cs` (priority sort, phase filter, Top‑K).
2. Add `Application/Injection/Injector.cs` (compose prologue anchors + rotated reminders + epilogue).
3. Wire into MCP pipeline: before forwarding tool call, add anchors/reminders to context payload.
4. Integration test simulating N tool calls → verify injection at the right calls and rotation.

**Files**
- `src/ConstraintMcpServer/Application/Selection/ConstraintSelector.cs`
- `src/ConstraintMcpServer/Application/Injection/Injector.cs`
- `tests/ConstraintMcpServer.Tests/InjectionFlowTests.cs`

**Commands**
```bash
dotnet test
```

**Acceptance**
- On scheduled interactions, output contains anchors and ≤K reminders.
- Non‑scheduled interactions pass through unchanged.

---

## Step 6 — Structured Logging & Perf Budgets
**Goal:** Emit NDJSON events and ensure handler latency budget.

**Tasks**
1. Configure Serilog to write compact JSON to stdout.
2. Emit events: `inject`, `pass`, `error` with `{ phase, selectedConstraintIds, reason }`.
3. Add a perf test (coarse) that measures p95 handler time on 100 synthetic calls.

**Files**
- `src/ConstraintMcpServer/Infrastructure/Logging/SerilogConfig.cs`
- `tests/ConstraintMcpServer.Tests/PerfBudgetTests.cs`

**Commands**
```bash
dotnet test
```

**Acceptance**
- Logs are valid JSON per line; fields present.
- p95 ≤ 50 ms; p99 ≤ 100 ms on CI runner.

---

## Step 7 — Quality Gates
**Goal:** Raise reliability via automation and analysis.

**Tasks**
1. Add Roslyn analyzers and `dotnet format` check in CI.
2. Add Stryker.NET for mutation testing on Domain layer; set threshold (e.g., 70%).
3. Add CODEOWNERS, CONTRIBUTING stub, Conventional Commits check (optional).

**Files**
- `.editorconfig` updated (treat warnings as errors)
- `stryker-config.json`
- `.github/workflows/ci.yml` updated with format/analyzers/mutation (nightly)

**Acceptance**
- CI fails on formatting/analyzer violations; mutation score ≥ threshold.

---

## Step 8 — Optional: Hot‑Reload & Advisory Drift Hints
**Goal:** Improve UX without changing guarantees.

**Tasks**
1. File watcher to reload `constraints.yaml`.
2. Implement advisory `match.keywords/regex` evaluation; log `hint` events when drift suspected.

**Acceptance**
- Editing YAML triggers reload without restart; hints appear in logs (no gating).

---

## Agent Prompts (for each step)
Use these when handing work to a coding agent:
- **Step 1:** *“Implement an MCP stdio server method `server.help` that returns product name, version, a one‑paragraph purpose statement, and high‑level commands. Provide an end‑to‑end test that issues a JSON‑RPC `server.help` request over stdin and verifies a successful acknowledgement without relying on protocol internals.”*
- **Step 2:** *“Implement a stdio JSON‑RPC loop that handles MCP `initialize` and `shutdown`. Echo capabilities `{ notifications.constraints: true }`. Provide an e2e that writes an `initialize` request to stdin and validates that the server acknowledges initialization.”*
- **Step 3:** *“Define Domain types for ConstraintPack and write a YamlDotNet loader with FluentValidation. Provide tests for invalid priorities, duplicate IDs, unknown phases.”*
- **Step 4:** *“Implement a deterministic Scheduler with `everyNInteractions` + `phaseOverrides`. Add unit tests for boundary conditions.”*
- **Step 5:** *“Implement ConstraintSelector (priority/phase filter, Top‑K) and Injector (anchors + rotating reminders). Wire into the MCP pipeline.”*
- **Step 6:** *“Configure Serilog NDJSON logs and add a perf test asserting p95 ≤ 50 ms over 100 calls.”*

---

## Acceptance Test — End‑to‑End Walking Skeleton (BDD)
**Intent:** Demonstrate **business value** that the system is ready for integration and local use. No implementation details.

**Feature:** Walking skeleton proves MCP server is usable by a coding agent and packaged for local run

**Business value:**
> As a developer, I need confidence that the constraint server starts, is discoverable via MCP, acknowledges a client session, and is available as a local package, so that I can integrate it safely and share it with my team.

**Scenario 1: MCP help is discoverable over stdio**
- **Given** the repository builds successfully in CI
- **When** I request help from the server via an MCP command
- **Then** I receive a concise description of the product and main commands
- **And** the process behaves predictably (continues running or exits by design)
- **So that** I can self‑diagnose environment issues in an agent/IDE context

**Scenario 2: Server acknowledges initialization**
- **Given** the server is running locally with standard settings
- **When** a Model Context Protocol client requests to initialize a session
- **Then** the server acknowledges the request and advertises available capabilities
- **And** the server remains available for subsequent requests
- **So that** an IDE or agent can connect and start using constraint reinforcement

**Scenario 3: Deterministic behavior for identical inputs**
- **Given** no configuration changes
- **When** I repeat the same initialization flow
- **Then** the observed outcomes are identical
- **So that** the integration is predictable and safe for CI/CD

**Scenario 4: CI/CD produces a local‑run package**
- **Given** a successful main‑branch build
- **When** the pipeline publishes platform binaries for local use
- **Then** I can download an artifact for my OS and execute the server locally
- **And** requesting MCP help and session initialization succeeds
- **So that** I can integrate and iterate without custom build steps

---

## Step 9 — CI/CD packaging for local run
**Goal:** Produce downloadable, runnable artifacts for Linux/Windows/macOS.

**Tasks**
1. Update GitHub Actions to publish self‑contained binaries:
   ```bash
   dotnet publish src/ConstraintMcpServer/ConstraintMcpServer.csproj \
     -c Release -r linux-x64   --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
   dotnet publish src/ConstraintMcpServer/ConstraintMcpServer.csproj \
     -c Release -r win-x64     --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
   dotnet publish src/ConstraintMcpServer/ConstraintMcpServer.csproj \
     -c Release -r osx-x64     --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
   ```
2. Upload artifacts per OS; name them `constraint-mcp-server-{os}-{arch}.zip` with `/config` samples.
3. (Optional) smoke script in CI to run the binary and request `server.help`.

**Acceptance**
- Artifacts are available for all target OSs on CI.
- Local execution works: `server.help` is discoverable and initialization is acknowledged.

---

## Done Criteria for v1
- ✅ Step 0: Preflight & Bootstrap (COMPLETED 2024-08-20)
- ✅ Step 1: MCP `server.help` e2e (COMPLETED 2024-08-21)
  - ✅ Walking skeleton with MCP server foundation established
  - ✅ CI/CD pipeline with cross-platform builds and artifacts
  - ⏳ Help content verification (acceptable for walking skeleton)
- ✅ Step 2: MCP initialize round‑trip (COMPLETED 2024-08-21)
  - ✅ Full MCP protocol compliance with initialize/shutdown handlers
  - ✅ Proper capabilities response advertising constraint notifications
  - ✅ BDD E2E tests validating complete MCP handshake lifecycle
  - ✅ Protocol compliance and latency budget verification
- ⏭️ Step 3: YAML load + validation (READY TO START)
- ⏭️ Step 4: Deterministic schedule + session state
- ⏭️ Step 5: Selection & injection
- ⏭️ Step 6: Structured logs + perf budgets
- ⏭️ Step 7: Quality gates
- ⏭️ Step 8: (Optional) Hot‑reload & advisory drift hints
- ⏭️ Step 9: CI/CD packaging for local run
- 📄 Docs updated: README (✅), ARCHITECTURE (⏳), PROGRESS (✅ current update)

---

## Next Priority Action (Step 3: YAML Config Load & Validation)

**Step 2 is now COMPLETE** ✅ with full MCP protocol compliance established.

**Next Steps for Step 3:**

1. **Immediate (Next session):** Define domain types for constraint system
   - Create `Domain/Constraint.cs`, `Domain/Schedule.cs`, `Domain/ConstraintPack.cs`
   - Define enums: `Phase`, `InjectionPoint` for TDD phases and injection timing
   - Establish core domain model for constraint definitions and scheduling

2. **Then:** Implement YAML configuration loading with validation
   - Add packages: `YamlDotNet`, `FluentValidation`
   - Create `Infrastructure/Config/YamlConstraintPackReader.cs`
   - Implement validators for constraint priorities, phases, duplicate IDs
   - File: `Infrastructure/Config/Validators/ConstraintPackValidator.cs`

3. **Validate:** Comprehensive testing for valid/invalid YAML scenarios
   - Test constraint pack loading with good/bad YAML samples
   - Validate error messages are actionable (duplicate IDs, priority out of range, etc.)
   - File: `tests/ConstraintMcpServer.Tests/ConfigTests.cs`

**Priority:** Establish constraint configuration foundation to enable deterministic scheduling and injection in Steps 4-5.

---

## Open Questions (track here)
- Default Top‑K (2 vs 3) reminders per injection.
- Minimal phase model vs richer workflow states.
- Exact IDE settings to register local MCP server (document once stable).

