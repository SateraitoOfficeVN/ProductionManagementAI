<!-- Decision Log template, based on common project decision-log conventions (ProjectManager, RAID-log decision registers). Copy into the relevant work item; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# Project Bootstrap (skeleton + auth foundation) — Decision Log

## Log

| ID | Date | Decision needed | Decision maker | Status | Rationale (summary) |
| --- | --- | --- | --- | --- | --- |
| DEC-001 | 2026-09-16 | UI framework for the frontend | trannhatthanh31@gmail.com | decided | React matches the earlier proposal and has the deepest Vite+TS ecosystem support |
| DEC-002 | 2026-09-16 | Backend structure and ORM | trannhatthanh31@gmail.com | decided | EF Core with conventional layered structure; explicitly not minimal-APIs/CQRS |
| DEC-003 | 2026-09-16 | PostgreSQL version | trannhatthanh31@gmail.com | decided | 17 — latest stable major as of 2026, best support window for a new project |
| DEC-004 | 2026-09-16 | Test frameworks | trannhatthanh31@gmail.com | decided | Vitest (native Vite integration) + xUnit (standard .NET) |
| DEC-005 | 2026-09-16 | Styling/component library | trannhatthanh31@gmail.com | decided | Tailwind CSS — no component-kit lock-in, lean for CRUD screens |
| DEC-006 | 2026-09-16 | Is auth/RBAC in scope for the MVP? | trannhatthanh31@gmail.com | decided | Full authentication + role-based authorization is in scope |
| DEC-007 | 2026-09-16 | Auth sequencing relative to Screen A | trannhatthanh31@gmail.com | decided | Auth is built as foundational bootstrap work, before Screen A |
| DEC-008 | 2026-09-16 | Which screen is "Screen A" (first vertical slice) | trannhatthanh31@gmail.com | decided | Production-order create/edit |
| DEC-009 | 2026-09-16 | Git branch/PR convention | trannhatthanh31@gmail.com | decided | Trunk-based: short `feature/<WI-id>-slug` branches, PR back to `master` |
| DEC-010 | 2026-09-16 | Screen B and Screen C scope | trannhatthanh31@gmail.com | decided | B = production-order list; C = dashboard (widgets/metrics still open) |
| DEC-011 | 2026-09-16 | Execution mode for WI-001 | trannhatthanh31@gmail.com | decided | User runs scaffold/build/git commands personally; Claude authors docs/content on request |
| DEC-012 | 2026-09-16 | Container registry / deploy host | unresolved | proposed | Not needed to unblock this plan; independent work continues per `ai/policies.md` |
| DEC-013 | 2026-09-16 | Japanese-translation-sync policy | unresolved | proposed | Not needed to unblock this plan |
| DEC-014 | 2026-09-16 | How the four demo videos are produced | unresolved | proposed | Not needed to unblock this plan |
| DEC-015 | 2026-09-16 | Exact role/permission matrix beyond placeholder `Admin`/`Operator` | unresolved | proposed | Placeholder seed roles used for bootstrap; must be confirmed before Screen A gates on any permission |

Add one row per decision as it is raised; keep IDs stable. Expand each decision below.

## DEC-001: UI framework for the frontend

**Status:** decided

### Context

`ai/project.md`'s Open decisions listed "UI framework and component library" as unresolved; React had been floated in `docs/vi/000-...md` but explicitly flagged as "a proposal, not a confirmed choice."

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| React | Matches earlier proposal; largest ecosystem/tooling maturity for Vite+TS | None material for this scope |
| Vue | Pairs well with Vite; SFC structure | Smaller ecosystem for enterprise admin UIs |
| Svelte | Lightest runtime, less boilerplate | Smaller ecosystem, less common for CRUD admin apps |
| Vanilla TS (no framework) | No framework overhead | Much more manual work for CRUD forms/lists |

### Decision and rationale

- **Decision:** React
- **Decided by:** user message (this session), via AskUserQuestion
- **Rationale:** Matches the earlier proposal and offers the deepest ecosystem/tooling support for a Vite+TS project.

### Impact

| Artifact | Change required |
| --- | --- |
| `ai/project.md` | Move "UI framework" from Open decisions to Confirmed as React |
| `work-items/WI-001/plan.md` | Frontend scaffold steps assume React |

## DEC-002: Backend structure and ORM

**Status:** decided

### Context

`ai/project.md` listed "backend structure and ORM" as open; .NET 10 itself was already fixed.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| EF Core, layered | Best docs, built-in migrations, most common .NET choice | None material for this scope |
| Dapper, layered | More control/performance | Manual migrations, more boilerplate |
| EF Core, minimal APIs | Less ceremony | Less familiar to reviewers expecting controllers |
| Other (Clean Architecture/CQRS/MediatR) | More structure for complex domains | Overkill for this MVP's scope |

### Decision and rationale

- **Decision:** EF Core with a conventional layered structure (Domain/Application/Infrastructure/Api)
- **Decided by:** user message (this session), via AskUserQuestion
- **Rationale:** Best-documented, most conventional choice for .NET; layering keeps Domain/Application unit-testable without hosting.

### Impact

| Artifact | Change required |
| --- | --- |
| `ai/project.md` | Move "backend structure and ORM" from Open decisions to Confirmed |
| ADR-0001 | Documents this structure formally |

## DEC-003: PostgreSQL version

**Status:** decided

### Context

`ai/project.md` fixed PostgreSQL as the database but left the version open.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| 17 | Latest stable, best support window for a new project | None material |
| 16 | More widely deployed already | Slightly behind latest |
| 15 | More conservative/compatible with older tooling | Behind latest by two majors |

### Decision and rationale

- **Decision:** PostgreSQL 17
- **Decided by:** user message (this session), via AskUserQuestion
- **Rationale:** Latest stable major as of 2026; best long-term support window for a project starting now.

### Impact

| Artifact | Change required |
| --- | --- |
| `ai/project.md` | Move "PostgreSQL version" from Open decisions to Confirmed as 17 |
| `deploy/compose.yaml` | Uses `postgres:17` image |

## DEC-004: Test frameworks

**Status:** decided

### Context

`ai/project.md` listed "test frameworks and thresholds" as open.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Vitest + xUnit | Native Vite integration; standard .NET framework | None material |
| Jest + xUnit | Jest is established | Needs extra config for Vite |
| Vitest + NUnit | Vitest native; NUnit also standard | Slightly different assertion style than xUnit |

### Decision and rationale

- **Decision:** Vitest (frontend) + xUnit (backend)
- **Decided by:** user message (this session), via AskUserQuestion
- **Rationale:** Both are first-class, native-tooling choices for the chosen stack.

### Impact

| Artifact | Change required |
| --- | --- |
| `ai/project.md` | Move "test frameworks" from Open decisions to Confirmed (thresholds remain to be set once real coverage exists) |
| `tests/frontend`, `tests/backend` | Scaffolded with these frameworks |

## DEC-005: Styling/component library

**Status:** decided

### Context

Bundled into "UI framework and component library" in `ai/project.md`'s Open decisions.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Tailwind CSS | No component-kit lock-in, minimal bundle, fast for CRUD | More manual composition than a component kit |
| MUI | Full component kit, faster for data-heavy admin screens | Heavier bundle, opinionated look |
| Ant Design | Strong table/form components | Similar tradeoffs to MUI |
| Plain CSS | No dependency | Most manual work |

### Decision and rationale

- **Decision:** Tailwind CSS, no component kit
- **Decided by:** user message (this session), via AskUserQuestion
- **Rationale:** Lean for a demo scope; avoids heavy component-kit lock-in.

### Impact

| Artifact | Change required |
| --- | --- |
| `ai/project.md` | Confirmed alongside DEC-001 |
| `src/frontend` | Tailwind config scaffolded |

## DEC-006: Is auth/RBAC in scope for the MVP?

**Status:** decided

### Context

`ai/project.md` listed "authentication scope" as needing to be determined ("Cần xác định có nằm trong MVP hay không" per `docs/vi/000-...md` §20).

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| No auth in MVP | Keeps first slice minimal, matches harness's stated "prove the workflow with a minimal vertical slice" intent | Auth retrofitted later |
| Simple auth (single demo user) | Some real auth scope without full RBAC | Partial solution, still needs later extension |
| Full auth + RBAC | Real foundation, no retrofit needed later | Significant scope increase before the first feature is proven |

### Decision and rationale

- **Decision:** Full authentication + role-based authorization is in scope
- **Decided by:** user message (this session), via AskUserQuestion — chosen over the recommended "no auth in MVP" option
- **Rationale:** User's explicit choice, despite the scope-increase tradeoff flagged at the time; per `ai/rules/common.md`, explicit user decisions are followed over proposals.

### Impact

| Artifact | Change required |
| --- | --- |
| `ai/project.md` | Move "authentication scope" from Open decisions to Confirmed |
| WI-001 scope | Auth foundation (Identity, login/me/logout, RBAC policy) added to bootstrap plan |

## DEC-007: Auth sequencing relative to Screen A

**Status:** decided

### Context

Given DEC-006, it was unclear whether auth should be built as foundational work now (in WI-001) or as a follow-up work item after Screen A ships without auth gating — the latter matches the harness's stated "minimal vertical slice first" intent more closely.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Bootstrap builds auth first | Screen A is gated by real auth/roles from day one; no retrofit | More upfront cost before any feature is proven |
| Screen A first, auth as follow-up work item | Matches harness's original minimal-slice intent; faster first feature | Screen A needs retrofitting with auth later |

### Decision and rationale

- **Decision:** Bootstrap (WI-001) builds auth first, before Screen A
- **Decided by:** user message (this session), via AskUserQuestion
- **Rationale:** User's explicit choice to avoid retrofitting auth onto Screen A later.

### Impact

| Artifact | Change required |
| --- | --- |
| `work-items/WI-001/plan.md` | Auth foundation is part of WI-001's deliverables, not a separate later work item |

## DEC-008: Which screen is "Screen A"

**Status:** decided

### Context

`docs/vi/000-...md` §14.1 proposed production-order create/edit as Screen A but flagged it as not yet locked.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Production-order create/edit | Compact but complete slice: UI, API, DB, validation, state transitions | More business rules to define than a pure list/read screen |
| Production-order list | Simpler, read-only | Proves less of the full stack (no mutation/validation path) |
| Product catalog | Simplest CRUD entity | Less illustrative of production-order workflow logic |

### Decision and rationale

- **Decision:** Production-order create/edit
- **Decided by:** user message (this session), via AskUserQuestion
- **Rationale:** Matches the original proposal; best single slice to prove the full workflow.

### Impact

| Artifact | Change required |
| --- | --- |
| `ai/project.md` | "Candidate demo" section locks Screen A |
| Next work item | Screen A's `requirements`/`basic-design` work starts from this decision |

## DEC-009: Git branch/PR convention

**Status:** decided

### Context

`ai/project.md` listed "repository/branch policy" as open.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Trunk-based: `feature/<WI-id>-slug` → PR → `master` | Simple, matches repo's existing single `master` branch | Less structure for large multi-stage releases |
| GitFlow-style (develop + release branches) | More release structure | Overkill ceremony for a small demo repo |
| Defer | No commitment yet | Blocks PR-based workflow steps |

### Decision and rationale

- **Decision:** Trunk-based, `feature/<WI-id>-slug` → PR → `master`
- **Decided by:** user message (this session), via AskUserQuestion
- **Rationale:** Matches the repo's existing single-branch state; low ceremony for a small demo repo.

### Impact

| Artifact | Change required |
| --- | --- |
| `ai/project.md` | Move "repository/branch policy" from Open decisions to Confirmed |
| `work-items/WI-001/plan.md` | Branch named `feature/WI-001-bootstrap-skeleton` |

## DEC-010: Screen B and Screen C scope

**Status:** decided

### Context

With Screen A locked (DEC-008), the user chose to also lock the next two screens in the demo roadmap in the same session.

### Options considered

Not applicable — the user specified both directly rather than choosing among presented options.

### Decision and rationale

- **Decision:** Screen B = production-order list; Screen C = dashboard (exact widgets/metrics still open, to be resolved during Screen C's own `requirements` step)
- **Decided by:** user message (this session)
- **Rationale:** User's explicit sequencing preference; dashboard content intentionally left open rather than invented here.

### Impact

| Artifact | Change required |
| --- | --- |
| `ai/project.md` | "Candidate demo" section updated: catalog dropped, A→B→C roadmap locked |
| Future work items | Screen B and Screen C work items reference this locked order |

## DEC-011: Execution mode for WI-001

**Status:** decided

### Context

After the plan was drafted, the user clarified they want to personally run the implementation commands rather than having the agent execute the plan's steps autonomously.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Agent executes, pausing after each step | Agent does the work, user retains checkpoint control | Still agent-driven |
| User runs commands/git actions themselves | User retains full hands-on control of the build | Agent's role narrows to docs/content authorship |
| Not ready to start at all | Maximum caution | Stalls progress unnecessarily |

### Decision and rationale

- **Decision:** User runs the actual scaffold/build/git commands themselves; Claude authors documentation and source content on request and reviews reported results
- **Decided by:** user message (this session), via AskUserQuestion
- **Rationale:** User's explicit preference for hands-on control of implementation commands.

### Impact

| Artifact | Change required |
| --- | --- |
| `work-items/WI-001/plan.md` | "Execution mode" section added; Deliverables table's step ownership reflects this split |
| `work-items/WI-001/status.md` | Next-action owner reflects command execution belongs to the user |
