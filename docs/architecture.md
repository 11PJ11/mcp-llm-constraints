# Constraint Enforcement MCP Server — ARCHITECTURE.md

> **Goal:** Keep LLM coding agents (e.g., Claude Code) *aligned during generation* with composable software-craft constraints (TDD, Hexagonal boundaries, SOLID, YAGNI, etc.), using deterministic reminders injected at MCP tool boundaries. Ship a **walking skeleton** first, then iterate.

---

## 1) Design Principles
- **Deterministic, low-latency**: No online learning in the hot path; sub‑50 ms p95 per tool call on typical projects.
- **Simple first**: YAML constraint packs, fixed injection cadence, priority ordering; evolve later.
- **Bounded contexts**: Catalog, Enforcement, Observation, Learning (offline).
- **Reproducible**: Same inputs → same outputs. Version everything (`constraints.yaml`, `schedule.yaml`).
- **Hexagonal**: Domain at center, adapters around (MCP stdio, YAML config, logging).

---

## 2) System Context (C4‑1)
```mermaid
flowchart LR
  Dev((Developer)) --- IDE[Claude Code / IDE]

  IDE <-->|JSON-RPC (MCP stdio)| MCP[[Constraint Enforcement MCP Server]]

  subgraph Tools[Developer Tools]
    FS[Filesystem]
    GIT[Git]
    TEST[Test Runner]
    BUILD[Build System]
  end

  IDE -->|tool calls| FS
  IDE -->|tool calls| GIT
  IDE -->|tool calls| TEST
  IDE -->|tool calls| BUILD

  MCP -->|reads| CFG[(constraints.yaml\nschedule.yaml)]
  MCP -->|injects anchors & reminders| IDE
  MCP --> LOGS[(structured logs\nNDJSON)]

  CI[GitHub Actions CI/CD] -->|build, test, publish| MCP

  LEARN[Offline Learning (batch)] -->|produces| W[(weights.yaml)]
  W -. optional later .-> MCP

  classDef store fill:#f7f7f7,stroke:#bbb,stroke-width:1px,color:#333
  class CFG,LOGS,W store
```

**Non‑Goals (v1):** hypergraph composition, CSP everywhere, marketplace/runtime plugins, real‑time dashboards, online RLHF.

---

## 3) MCP Server Internals (C4‑2)
```mermaid
flowchart TB
  subgraph MCP[Constraint Enforcement MCP Server (.NET 8)]
    RPC[JSON-RPC / stdio Handler]
    SESS[Session Manager]
    SCHED[Deterministic Scheduler\n(every N, phase overrides)]
    SELECT[Constraint Selector\n(priority & phase aware)]
    INJ[Injector\n(Anchors + Top-K reminders)]
    CONF[Config Loader & Validator\n(YamlDotNet + FluentValidation)]
    LOG[Event Logger\n(Serilog NDJSON)]
  end

  RPC --> SESS --> SCHED --> SELECT --> INJ --> RPC
  CONF --> SELECT
  CONF --> SCHED
  SESS --> LOG
  INJ --> LOG

  subgraph Packs[Constraint Catalog]
    YML[(constraints.yaml)]
    SCH[(schedule.yaml)]
  end

  YML --> CONF
  SCH --> CONF

  classDef store fill:#f7f7f7,stroke:#bbb,stroke-width:1px,color:#333
  class YML,SCH store
```

**Request lifecycle (v1):**
1. `RPC` receives tool call
2. `SESS` updates counters / phase
3. `SCHED` decides inject vs pass‑through (deterministic)
4. `SELECT` picks top‑K constraints by priority & phase
5. `INJ` injects **Anchor** prologue/epilogue + reminders
6. `RPC` forwards; `LOG` emits structured event

---

## 4) Bounded Contexts & Hexagonal Ports
- **Constraint Catalog** (authoring, versioning, composition)
  - Ports: `IConstraintPackReader`, `IConstraintValidator`
- **Enforcement** (scheduling, selection, injection)
  - Ports: `IScheduler`, `IInjector`, `IPhaseTracker`
- **Observation** (logging, metrics, perf)
  - Ports: `IEventLogger`, `IMetrics`
- **Learning (offline)** (aggregation, weight synthesis → `weights.yaml`)
  - Ports: `ILogReader`, `IWeightsExporter`

Adapters: MCP stdio (RPC), YAML loader (YamlDotNet), Serilog sink, Clock.

---

## 5) Data Model (v1 — YAML schema + types)

### 5.1 Types (TypeScript-like)
```ts
type ConstraintId = string;

type Phase = 'kickoff' | 'red' | 'green' | 'refactor' | 'commit';

type InjectionPoint = 'session-start' | 'pre-tool-call' | 'post-tool-call' | 'session-end';

interface Constraint {
  id: ConstraintId;
  title: string;
  priority: number;          // 0..1
  phases: Phase[];           // where it applies
  match?: {                  // optional drift indicators
    keywords?: string[];
    forbiddenRegex?: string[];
  };
  reminders: string[];       // short, high-signal phrases
  examples?: { good: string; bad: string; }[];
}

interface Schedule {
  injectionPoints: InjectionPoint[];
  cadence: { everyNInteractions?: number; phaseOverrides?: Partial<Record<Phase, number>> };
  anchors: { prologue: string[]; epilogue: string[] };
}

interface ConstraintPack { version: string; constraints: Constraint[]; schedule: Schedule; }
```

### 5.2 Example `constraints.yaml`
```yaml
version: "0.1.0"
constraints:
  - id: tdd.test-first
    title: "Write a failing test first"
    priority: 0.92
    phases: [kickoff, red, commit]
    match:
      keywords: ["test", "assert", "describe(", "it("]
    reminders:
      - "Start with a failing test (RED) before any implementation."
      - "Stay in TDD: write test → see it fail → implement."

  - id: arch.hex.domain-no-infra
    title: "Domain must not depend on Infrastructure"
    priority: 0.88
    phases: [red, green, refactor, commit]
    match:
      forbiddenRegex: ["from\\s+['\"](infrastructure|infra)"]
    reminders:
      - "Respect hexagonal boundaries: domain depends on ports, not adapters."

schedule:
  injectionPoints: [session-start, pre-tool-call, session-end]
  cadence:
    everyNInteractions: 3
    phaseOverrides: { kickoff: 1, red: 1 }
  anchors:
    prologue:
      - "Active method: Outside-In → ATDD → BDD → TDD. Honor priorities."
    epilogue:
      - "If GREEN, consider refactor; if committing, ensure constraints satisfied."
```

**Notes:** `match` is optional in v1 (we can inject on cadence alone). Keep reminders short for high attention weight.

---

## 6) Enforcement Logic
- **Selection:** Sort by `priority`; filter by current `phase`.
- **Top‑K:** cap to small K (e.g., 2–3) to avoid prompt dilution.
- **Anchors:** short prologue+epilogue always win attention vs mid‑context.
- **Cadence:** every N interactions, with phase overrides (`kickoff`, `red` = every interaction).
- **Tie‑breaks:** if contradictions arise, show only the highest priority + one‑line rationale (e.g., *“YAGNI > Documentation on trivial utility”*).

---

## 7) Performance Budgets
- **Request handler p95:** ≤ 50 ms; p99 ≤ 100 ms (on dev laptop).
- **Memory ceiling:** ≤ 100 MB resident for server process (idle + typical load).
- **I/O:** YAML read only on startup (hot‑reload optional later).

---

## 8) Observability
- **Structured events (NDJSON):**
```json
{"ts":"2025-08-20T20:13:00Z","evt":"inject","phase":"red","top":["tdd.test-first","arch.hex.domain-no-infra"],"reason":"cadence:everyN=3"}
```
- **Counters:** injections/session, per‑constraint usage, handler latency.
- **Perf tests:** tracked in CI; fail the build if p95 budgets regress.

---

## 9) Testing Strategy
- **E2E 1:** `--help` prints usage; exit code 0 (proves pipeline + packaging).
- **E2E 2:** MCP `initialize` round‑trip on stdio (no injection yet).
- **Unit:** selector chooses top‑K by priority/phase; scheduler cadence decision.
- **Integration:** injection path places anchors + reminders; logs event.
- **Mutation testing:** Stryker.NET on core domain rules.

---

## 10) CI/CD (GitHub Actions)
- Build/test matrix: `ubuntu-latest`, `windows-latest`, `macos-latest`.
- Cache NuGet; publish single‑file binaries (`linux-x64`, `osx-x64`, `win-x64`).
- Artifacts: zipped binaries with README + sample configs.

---

## 11) Security & Privacy (Local‑first)
- v1 is local‑only. No telemetry exfiltration. Logs stay on disk.
- Redaction hook (later): mask secrets in observed tool responses before logging.

---

## 12) Roadmap (incremental)
1. **Walking skeleton**: `--help`, pass‑through RPC, YAML load, deterministic schedule, anchors+reminders, logs.
2. Phase tracking + top‑K selection; perf budget tests; hot‑reload `constraints.yaml` (optional).
3. Simple drift indicators via `match.keywords/regex` (advisory only).
4. Offline learning pipeline to synthesize `weights.yaml` (no hot‑path changes).
5. Plugin packs (shareable constraints), semantic examples, richer conflict resolution.

---

## 13) Repository Layout (proposed)
```
/ src
  / ConstraintMcpServer
    / Application          # scheduling, selection, injection
    / Domain               # Constraint, Schedule, Phase, types
    / Infrastructure
      / Mcp                # stdio JSON-RPC server (ModelContextProtocol)
      / Config             # YamlDotNet + FluentValidation
      / Logging            # Serilog NDJSON
/ config
  constraints.yaml
  schedule.yaml
/ tests
  ConstraintMcpServer.Tests  # NUnit + FluentAssertions; e2e + unit
.github/workflows
  ci.yml
/docs
  ARCHITECTURE.md (this)
```

---

## 14) Open Questions (deliberately deferred)
- How strict should `match` be for v1 (pure cadence vs heuristic drift)?
- Best default K for reminders (2 vs 3) to avoid prompt dilution?
- When to introduce hot‑reload and pack version pinning?
- What is the minimal *phase* model that keeps guidance sharp but simple?

---

**Summary:** This architecture ships a deterministic, low‑friction MCP server that reinforces development constraints *during* LLM code generation via short, well‑placed anchors and minimal reminders—fast to build, easy to test, and safe to evolve.

