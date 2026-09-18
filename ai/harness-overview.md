# The ai/ harness — how it works and why

This describes the harness under `ai/` as it actually exists today: what each part does, how they connect, and the reasoning behind the design. It is a reference for understanding the system, not a work item and not itself a source of authorization — `ai/policies.md` remains the authority on what an agent may do.

## Why this exists

Claude and Codex both work on this repository, often in different sessions with no shared memory. Without a shared, file-based source of instructions, each session would re-derive (or silently disagree about) scope, technology choices, quality bar and authorization boundaries. The harness's job is to make that context durable and agent-neutral: a request plus the repository's own files should be enough for any agent, in any session, to pick up correct, consistent work — without reading prior chat history.

## Folder structure

The current, actual layout of `ai/` (not the v0.1 proposal's layout — compare the 13 skills and four checklists here to that document's 11 skills and no checklists):

```text
ai/
├── README.md                     — index, points to harness-overview.md first
├── harness-overview.md           — this document
├── project.md                    — confirmed vs. open technology decisions
├── policies.md                   — authorization, pause conditions, untrusted-content guardrails
│
├── rules/
│   ├── README.md
│   ├── common.md                 — cross-cutting: scope, stable IDs, focused changes
│   ├── documentation.md          — language, translation, fact/assumption separation
│   ├── frontend.md               — Vite/TypeScript, strict typing, WCAG 2.2 AA
│   ├── backend.md                — .NET 10, RFC 9457, nullable types, OpenTelemetry
│   ├── database.md               — PostgreSQL, parameterized queries, expand/contract
│   ├── testing.md                — pyramid shape, flaky-test quarantine
│   ├── git-review.md             — small PRs, no direct push, review categories
│   └── ci-cd.md                  — SHA-pinned actions, OIDC, non-root images
│
├── workflows/
│   ├── README.md
│   ├── project-bootstrap.md
│   ├── feature-delivery.md
│   ├── bug-fix.md
│   └── harness-improvement.md
│
├── skills/
│   ├── README.md
│   ├── planning/            { README.md, SKILL.md }
│   ├── requirements/        { README.md, SKILL.md }
│   ├── architecture/        { README.md, SKILL.md }
│   ├── basic-design/        { README.md, SKILL.md }
│   ├── database-design/     { README.md, SKILL.md }
│   ├── detailed-design/     { README.md, SKILL.md }
│   ├── screen-design/       { README.md, SKILL.md }
│   ├── implementation/      { README.md, SKILL.md }
│   ├── testing/             { README.md, SKILL.md }
│   ├── pr-review/           { README.md, SKILL.md }
│   ├── security-review/     { README.md, SKILL.md }
│   ├── ci-cd/                { README.md, SKILL.md }
│   └── harness-improvement/ { README.md, SKILL.md }
│
├── templates/
│   ├── README.md
│   ├── brief.md                   — PRD format
│   ├── plan.md                    — implementation-plan format
│   ├── status.md                  — RAG status-report format
│   ├── decisions.md               — decision-log format
│   ├── evidence.md                — RTM + test-execution-log format
│   ├── architecture-decision.md   — MADR format
│   ├── basic-design.md            — 基本設計書 format
│   ├── detailed-design.md         — 詳細設計書 format (main DD)
│   ├── DD/                        — DD companions, always produced with the main DD
│   │   ├── api-design.md                 — API仕様設計 format
│   │   ├── function-design.md            — 機能設計 format
│   │   └── screen-processing-design.md   — 画面処理設計 format
│   ├── example/                   — reference workbooks (BD/, DD/) the BD/DD templates mirror
│   ├── database-design.md         — テーブル定義書 format
│   ├── test-plan.md               — IEEE 829 format
│   ├── review.md                  — Google review-checklist format
│   └── improvement.md             — RFC format
│
├── checklists/
│   ├── README.md
│   ├── design-consistency.md      — gate before dependent implementation
│   ├── security-review.md         — gate before merging a security-relevant change
│   ├── delivery.md                — gate before reporting completion
│   └── release-readiness.md       — gate before an authorized deployment
│
├── evaluations/
│   ├── README.md
│   └── baseline-cases.md          — manual scenarios, including adversarial ones
│
└── improvements/
    ├── README.md                  — where adopted harness-improvement work lands
    ├── 0001-dd-companions-always-produced.md
    └── 0002-plan-revision-history.md
```

`ai/` sits inside the wider repository alongside its consumers and adapters — `AGENTS.md` and `CLAUDE.md` at the repo root, `.claude/` as a thin per-agent adapter (a `.codex/` counterpart hasn't been needed or created yet), `work-items/<WI-###>/` as the durable state each skill reads and writes, and `docs/en/...` as where design skills place BD/DD/DB/ADR/test output. The root `README.md`'s own "Layout" section is the map of that wider structure; this document only expands on `ai/` itself.

## The routing chain — how a request actually flows

Every task enters through the same chain, regardless of which agent is running it:

1. **Entry point.** `AGENTS.md` is the shared entry point both agents read. `CLAUDE.md` is Claude's thin adapter — it points at `AGENTS.md` rather than repeating it, per the explicit rule that shared skill/process content is never duplicated into an adapter. `.claude/` exists as the place for genuinely agent-specific configuration; today it holds only a README pointer, since no agent-specific behavior has been needed yet. A `.codex/` counterpart would be added the same way if Codex ever needs one.
2. **Always read first.** `ai/project.md` (confirmed vs. open technology decisions), `ai/policies.md` (authorization and pause conditions) and `ai/rules/common.md` (cross-cutting rules).
3. **Pick a workflow.** `ai/workflows/README.md` routes to `project-bootstrap`, `feature-delivery`, `bug-fix`, or `harness-improvement` depending on what triggered the work.
4. **Pick skill(s).** The workflow's steps name the kind of work needed; `ai/skills/README.md` routes to the matching `SKILL.md`, which is read directly (there is no native skill auto-discovery configured, by design — see "Why Markdown, and why no auto-discovery" below).
5. **Start from a template.** Each skill's output starts from the matching file in `ai/templates/`.
6. **Pass the relevant checklist.** At specific points, a skill's own Verification/Checklist step names the `ai/checklists/` gate it must pass.
7. **Record durable state.** Every skill ends the same way: update `work-items/<ID>/status.md`, `decisions.md` and `evidence.md`, so the next step — possibly run by a different agent, in a different session — can continue from what's in the repository, not from what's in the chat.

`ai/evaluations/` sits outside this per-task chain: it's how the harness checks *itself*, run when a change to shared guidance is proposed (see "How the harness evolves" below).

## What's inside `ai/`, and why each part exists

| Folder/file | What it is | Why it exists |
|---|---|---|
| `project.md` | Confirmed stack (Vite + React + TS + Tailwind, .NET 10 + EF Core in a layered structure, PostgreSQL 17, xUnit/Vitest/Playwright, cookie auth, GitHub Actions, Docker) and verified commands vs. open decisions (deployment host, merge/deploy permissions and CI execution, translation sync, demo-video production, the full role/permission matrix) | So no agent invents or assumes a technology choice the project hasn't actually made |
| `policies.md` | Plan/authorization rules, pause conditions, evidence requirements, harness-change governance, external-operation gating, and untrusted-content/tool-use guardrails | The harness's safety boundary: what an agent may do without asking, and exactly when it must stop and ask instead |
| `rules/` (8 files: common, documentation, frontend, backend, database, testing, git-review, ci-cd) | Technology- and task-specific standards | So every skill that touches, say, the backend applies the same current standard (RFC 9457 errors, nullable reference types, OpenTelemetry instrumentation, ...) instead of restating or reinventing it per skill |
| `workflows/` (4 files) | Ordered sequences of skills for a given trigger, each with an explicit exit condition | Gives a starting sequence for the four ways work actually begins here — first-time setup, a feature, a defect, or a harness change — without hard-coding every possible path |
| `skills/` (13 skills) | The actual "how to do X" procedures | Each is a bounded, checkable unit of work with the same 8-part shape (purpose/inputs/steps/tooling/outputs/checklist/termination/handover — see below), so it's re-runnable by any agent and its output is predictable |
| `templates/` (12, plus the 3 `DD/` companions and the `example/` reference workbooks) | The starting document for each skill's output | Each is modeled on a real, named industry format rather than an invented one — MADR for ADRs, IEEE 829 for test plans, a PRD structure for the brief, an RFC format for improvement proposals, a Requirements Traceability Matrix for evidence, Japanese SI conventions (基本設計書/詳細設計書/テーブル定義書) for BD/DD/DB, Google's review-checklist categories for `review.md` — so the output is actually useful, not just a formality |
| `checklists/` (4: design-consistency, security-review, delivery, release-readiness) | Point-in-time pass/fail gates | Turns "did this follow the rules" from something held in memory into an explicit, repeatable check at the moment it matters: before dependent implementation, before merge, before reporting done, before an authorized deploy |
| `evaluations/` | Manual scenarios (17 today, including adversarial ones — prompt injection, scope creep, secret leakage) with expected behavior | Lets a change to shared guidance be checked against concrete cases instead of trusted on faith |
| `improvements/` | Where a harness-improvement work item's artifacts land once adopted — two RFCs so far (0001: always produce all four DD documents; 0002: keep every plan revision, oldest first) | Keeps the history of *why* the harness changed, not just its current state |

**Why `policies.md`, `rules/`, and `checklists/` are three separate things** rather than one file: they answer different questions. `policies.md` answers "is this agent allowed to do this, or must it stop and ask?" `rules/` answers "what does correct look like for this kind of change?" `checklists/` answers "right now, before I move to the next step, did I actually meet that bar?" Collapsing them would either bury authorization rules inside technical detail, or turn every rule into an unenforced suggestion with no checkpoint.

## Skill-by-skill reference

What each of the 13 skills actually does, in the order they normally compose (a workflow picks the subset a given task needs — not every task runs all 13):

| Skill | What it actually does | Primary output |
|---|---|---|
| `requirements` | Turns an ambiguous request into actors, use cases, in/out-of-scope behavior, and stable `REQ-###` IDs each with a success-path and failure-path acceptance criterion. Records ambiguous business rules as open questions instead of inventing them. | `work-items/<ID>/brief.md`, `docs/en/000_requirements/` |
| `planning` | Turns a brief (or a bug/improvement problem statement) into a bounded plan: scope, dependencies, assumptions, a step list with a verification method per step, external actions separated from local work, and an explicit approval record. A revision never replaces an earlier one: the previous revision is closed in place and the new one appended, so `plan.md` reads oldest-first (RFC 0002). | `work-items/<ID>/plan.md`, `status.md` |
| `architecture` | Describes frontend/backend/database boundaries and where auth, secrets or external input cross one; threat-models (STRIDE) a boundary touching auth/payment/PII; records one decision per ADR with alternatives assessed proportionally. | ADR(s) under `docs/en/architecture/` |
| `basic-design` | Describes business flow, screen list, primary actions and success/exception paths for a feature; flags which fields are security/PII-relevant and where accessibility matters, without fixing implementation detail yet. | BD under `docs/en/010_basic-design/` |
| `database-design` | Defines entities, relationships, types, keys, constraints and justified indexes; decides the migration approach (expand/contract for a live table, `CREATE INDEX CONCURRENTLY` for a live index) and the least-privilege role the app uses. | DB design under `docs/en/database/` |
| `detailed-design` | Turns a BD into fields/validation/state transitions, the API contract (RFC 9457 error shape), persistence mapping, and what gets traced/logged for each endpoint, plus a rendered mockup for screens. Always produces all four DD documents, each piece of content in exactly one of them (RFC 0001). | `DD-###` main DD plus `DD-###-API`, `DD-###-FN`, `DD-###-SPD` under `docs/en/020_detailed-design/` |
| `screen-design` | Specifies one screen in more depth than the BD's screen list gives — layout/navigation at BD level, or fields/interactions/WCAG 2.2 AA states at DD level. | Sections embedded in BD/DD |
| `implementation` | Works on a dedicated branch in its own git worktree from the first change onward; writes the actual vertical slice — code, migration, config, tests and the instrumentation the DD specified — for a bounded piece of the plan; quarantines (doesn't silently retry or delete) any check that proves flaky; routes a security-relevant change through `security-review` before calling it done; once the delivery checklist passes, pushes the branch and opens a PR when authorized (otherwise leaves it ready, pending authorization). | A branch per work item (the skill suggests `work-items/<WI-###>`; this project's DEC-009 uses `feature/<WI-###>-slug`), code under `src/`, tests under `tests/`, `evidence.md` |
| `testing` | Maps requirements and risk to test cases and the right test level (mostly unit, less integration, least E2E); runs what's available and records real results, including not-run and blocked checks. | Tests, `docs/en/testing/`, `evidence.md` |
| `pr-review` | Reviews the actual diff for behavior, missing requirements and regression risk with file/line evidence; runs the Google-style review checklist (design/functionality/complexity/tests/naming/comments/style/docs) embedded in `templates/review.md`. | `review.md` |
| `security-review` | A dedicated pass distinct from general review: auth/authz on every changed endpoint, input validation, no secrets in the diff, vetted dependencies, untrusted content never built into a query/command, and threat-modeling follow-through for a sensitive boundary. | `review.md` or `evidence.md` |
| `ci-cd` | Designs/implements the GitHub Actions + Docker pipeline: CI kept separate from image publishing and deployment, third-party actions pinned to a commit SHA, least-privilege default permissions, OIDC preferred over long-lived secrets, non-root minimal images scanned before publishing. | `.github/workflows/`, `evidence.md` |
| `harness-improvement` | Turns an observed harness gap into the smallest useful change to a skill/rule/policy/workflow/checklist/template, evaluated against `ai/evaluations` cases (adding one if none covers the failure mode) before shared adoption. | `work-items/<ID>/improvement.md`, then the changed `ai/` file |

## Rule-by-rule reference

What each file under `ai/rules/` actually constrains:

| Rule file | Covers |
|---|---|
| `common.md` | Preserve approved scope; use stable IDs and link artifacts instead of duplicating them; record unresolved decisions explicitly; keep changes focused. |
| `documentation.md` | English by default; stable IDs survive translation; separate confirmed facts from proposals/assumptions/open questions; update docs in the same change as the code they describe, not a follow-up. |
| `frontend.md` | Vite + TypeScript (the file still says the UI framework is open, although `project.md` now records React + Tailwind — see "Known gaps" below); strict TypeScript with no `any`/non-null-assertion escapes; WCAG 2.2 AA; config via `import.meta.env`, never a secret in client code, since anything shipped to the browser is public. |
| `backend.md` | .NET 10 (the file still says ORM/layers are open, although `project.md` now records EF Core in a layered structure — see "Known gaps" below); nullable reference types enforced; API errors as RFC 9457 Problem Details with no leaked exception detail; structured logging with no secrets/PII; OpenTelemetry traces and metrics on new endpoints and jobs, not logs alone. |
| `database.md` | PostgreSQL; parameterized queries only, never string-built SQL; expand/contract for a breaking change on a live table; `CREATE INDEX CONCURRENTLY` on a live table; least-privilege application DB role; migration impact and recovery limits described before executing. |
| `testing.md` | Trace tests to acceptance criteria and real failure cases; test-pyramid shape; isolated resettable data; quarantine (don't silently retry/delete) a flaky check; a test not run is not a pass; no silent choice of thresholds/frameworks. |
| `git-review.md` | Implementation work happens on a dedicated branch in its own git worktree, isolated from the main working directory, removed once merged or abandoned; inspect the diff before editing or reviewing; small focused PRs, no bundling unrelated changes; never push directly to a protected branch; check consistency across requirements/design/code/migration/tests; review never itself authorizes a merge. |
| `ci-cd.md` | GitHub Actions + Docker when authorized; SHA-pin third-party actions; default workflow permissions to read-only; OIDC over long-lived cloud secrets; non-root minimal, scanned container images; registry/target/trigger/migration/rollback defined before CD is enabled. |

## Workflow-by-workflow reference

| Workflow | Sequence |
|---|---|
| `project-bootstrap` | Confirm open technology decisions and demo scope → draft a bootstrap plan (skeleton, local env, CI checks) → build it in bounded, approved steps → verify real commands and update `project.md` → record remaining gaps before handing off to feature-delivery. |
| `feature-delivery` | Read brief/acceptance criteria → plan and get it reviewed → check architecture → produce BD/DB/API/DD via the linked skills → reconcile design artifacts before implementing them → implement plus unit/integration checks, then system/E2E → review the diff and prepare the PR within authorization → record evidence; deploy/smoke-test only if included and authorized. |
| `bug-fix` | Capture failing vs. expected behavior with a reproducible case → assess severity (a live incident mitigates first, root-causes after) and make a proportional plan → find the cause and add a regression check → fix and update affected design/contracts → run targeted checks and record evidence → for a live incident, record a blameless postmortem (what failed, not who). |
| `harness-improvement` | Record an observed failure or project change and the affected guidance → propose the smallest useful change → evaluate it with relevant `ai/evaluations` cases → record limitations and submit for review before shared adoption → keep a rollback path through version control; never silently relax a gate to pass an evaluation. |

## Checklist-by-checklist reference

| Checklist | Gates |
|---|---|
| `design-consistency` (used before dependent implementation) | Requirement IDs/acceptance criteria exist; BD covers navigation/actions/exceptions; DD agrees with BD; API/DB mappings agree; missing decisions are resolved first; test scenarios map to the design; security/PII fields, WCAG 2.2 AA needs, migration impact, and what's traced/logged are all specified before implementation depends on them. |
| `security-review` (used before merging a security-relevant change) | Auth/authz on every changed endpoint; input validated, never concatenated into a query/command; no secret in code/logs/evidence/diff; dependencies vetted; least-privilege credentials/roles; untrusted content never built into an executable query/command; no leaky error responses; no sensitive data in logs; a new sensitive trust boundary was threat-modeled before implementation. |
| `delivery` (used before reporting completion) | Scope/plan identifiable; design/code/tests agree; checks have recorded results (not-run has a reason); findings and limitations explicit; external operations stayed in authorization; status/decisions/evidence let another agent continue; no secret in any output; untrusted content handled correctly; flaky checks quarantined with a reason; third-party CI actions/dependencies pinned and least-privilege scoped. |
| `release-readiness` (used before an authorized deployment) | Target-environment config confirmed; migration matches the expand/contract plan; rollback path confirmed executable, not just described; deployment authorization recorded; smoke-test criteria defined before the deploy; monitoring in place; deployed image/version traceable to its commit/PR; incident steps understood in advance if the release goes wrong. |

## Template-by-template reference

Every template below is a real, named document format — not an invented one — chosen to match the artifact's actual purpose. The BD and DD templates mirror the reference workbooks in `templates/example/`:

| Template | Modeled on | Captures |
|---|---|---|
| `architecture-decision.md` | MADR (Markdown Architectural Decision Records) | Context/problem, decision drivers, considered options, decision outcome and consequences, pros/cons per option |
| `brief.md` | PRD conventions (Atlassian/Aha/monday) | Status, overview, objective, success metrics, assumptions, actors/user stories, requirements with `REQ-###` + acceptance criteria, explicit "not doing," open questions |
| `plan.md` | PMI-style implementation plan | A revision index table, then every revision in full, oldest first. Each has objective, scope, inputs/assumptions, a deliverables/milestones table with dependency/skill/verification/outcome columns, roles, authorized external actions, risks, and approval/sign-off with a closure line |
| `status.md` | RAG (Red/Amber/Green) status report | Overall status and why, accomplishments this period, planned next, risks/issues with owner, single next action |
| `decisions.md` | Decision-log convention | An index table of every decision plus a per-decision record: context, options with pros/cons, chosen answer, rationale, downstream artifact impact |
| `evidence.md` | Requirements Traceability Matrix + test-execution log | Requirement→design→code→test mapping, a checks table (command/environment/result/report), defects/blockers, external (PR/CI/deploy) references |
| `improvement.md` | Rust-style engineering RFC | Summary, motivation, guide-level and reference-level explanation, drawbacks, rationale/alternatives, prior art/evaluation, unresolved questions, adoption record |
| `basic-design.md` | Japanese 基本設計書 (basic design document) convention | System overview, architecture, function list, business flow, screen list, actions/business rules, success/exception flows, non-functional requirements, open questions |
| `detailed-design.md` | Japanese 詳細設計書 (detailed design document) convention | Module design for screen-owned modules, component organization, screen layout and rendered mockup, screen item definition, states, state transitions, pointers to the companions, DB/transaction mapping, exception handling, test viewpoints |
| `DD/api-design.md` | Japanese API仕様設計 (API specification) convention | Operation catalog, per-endpoint processing, request/response field catalogs with value mapping, error codes |
| `DD/function-design.md` | Japanese 機能設計 (function design) convention | Backend service methods behind the DD's endpoints: method index, shared utilities, request data, response/value mapping, per-method processing |
| `DD/screen-processing-design.md` | Japanese 画面処理設計 (screen processing design) convention | Step-by-step processing, one block per component, with branches, calls and resulting state |
| `database-design.md` | Japanese テーブル定義書 (table definition document) + ER diagram convention | Table list, ERD/relationships, per-table column definitions, index definitions, constraints, API/DD field mapping, migration impact/recovery limits |
| `test-plan.md` | IEEE 829-1998 test plan standard | Test items, approach/test levels, pass/fail criteria, environment, a case table, results linked to evidence, known gaps |
| `review.md` | Google's "what to look for in a code review" categories | Change summary, a review checklist (design/functionality/complexity/tests/naming/comments/style/docs), a findings table, design/test consistency, verification performed |

## The skill shape

Every `SKILL.md` follows the same 8-part structure:

1. Purpose and usage scenario
2. Mandatory inputs, optional inputs, and source reference order
3. Execution steps and applicable rules
4. Required tools/scripts and environmental conditions
5. Output artifacts, templates, ID conventions, and storage locations
6. Checklist and repeatable verification method
7. Termination criteria and failure handling
8. Work item update procedure and handover for the next step

This shape is deliberate: (1)-(2) bound what the skill needs before it starts; (3) ties every step to the rule that governs it, instead of leaving "current standard" implicit; (4) is honest about what this skill can and can't do without a missing toolchain; (5) makes output location and ID conventions (`REQ-###`, `BD-###`, `DD-###`, `DB-###`, `SCR-###`, `TC-###`, `DEC-###`, `WI-###`, ADRs as `NNNN-title.md`) explicit rather than left to guesswork; (6) and (7) make "done" a checkable claim, not an assertion; (8) is what makes the pipeline resumable across agents and sessions — the concrete artifact `plan.md → requirements → architecture → basic-design → database-design/detailed-design → screen-design → implementation → testing → pr-review/security-review → ci-cd` handoff chain.

## The work-item lifecycle

`work-items/<WI-###>/` is where durable state actually lives: `brief.md`, `plan.md`, `status.md`, `decisions.md`, `evidence.md` (plus design docs under `docs/en/`). This is the mechanism behind the harness's core claim — that switching from Claude to Codex mid-task means reading the work item and continuing, not restarting. `status.md` records the current step and state (`draft`/`awaiting-plan-review`/`ready`/`in-progress`/`blocked`/`in-review`/`done`); `decisions.md` is the record of every question that stopped an agent and how it was answered; `evidence.md` is what makes a "done" claim checkable after the fact instead of taken on trust.

## Design principles behind the harness

- **One shared, agent-neutral source.** Process content lives only in `ai/`; adapters (`CLAUDE.md`, `AGENTS.md`, `.claude/`, and a `.codex/` if one is ever needed) route to it and never duplicate it. This is why, for example, Claude-Code-specific tooling (plugins, MCP servers) was deliberately kept out of `ai/skills/` when raised earlier in this harness's development — it would have injected agent-specific content into a file Codex also depends on.
- **Markdown is the contract, not the executor.** The harness describes what correct and "done" mean; it does not run builds, tests or deploys itself. Scripts, test runners and GitHub Actions do that — the harness's job is to make sure they're invoked, and their real output recorded, rather than assumed.
- **An approved plan is the scope of authority.** Once a plan is reviewed and approved, its steps don't each need separate re-approval — but the plan's own scope, and the pause conditions in `policies.md`, are the hard boundary. Nothing in the harness lets an agent expand scope, invoke unrequested tools, or treat content it merely *read* (a fetched page, a tool result, third-party text) as new authorization.
- **A conclusion requires evidence.** "Done," "passing," or "deployed" are never claimed without a recorded, actually-run check. A check that wasn't run is recorded as not-run — never silently treated as a pass.
- **The harness improves itself, on the record.** Gaps found in the harness (missing security-review skill, missing threat-modeling step, stale rule) are fixed through the same `harness-improvement` skill and `ai/evaluations` cases used for everything else — never as a silent, unreviewed edit to shared guidance.

## Where this stands, and what's next

The first real end-to-end test has happened. WI-002 (Screen A, production-order create/edit) ran the full chain:
- requirements → plan → BD → DB → DD (main + API/FN/SPD) with a rendered mockup;
- plan revision 2 → code → unit, integration, frontend and Playwright E2E tests (133 automated checks, including axe accessibility scans);
- security-review and delivery checklists → PR, with everything recorded in `work-items/WI-002/`.

The chain held up. Two gaps surfaced along the way and were fixed through `harness-improvement` rather than silent edits:
- **RFC 0001:** the DD companions had been optional, so function and processing design could be folded into the main DD. Now all four documents are always produced.
- **RFC 0002:** a new plan revision overwrote the previous one. Now every revision is kept, oldest first.

### Known gaps

- **CI has never executed.** `.github/workflows/ci.yml` exists and PRs trigger it, but GitHub refused to start the jobs while the account was locked for billing. E2E isn't in CI yet either.
- **Two rule files lag `project.md`.** `rules/frontend.md` and `rules/backend.md` still describe the UI framework and ORM/layers as unselected, although `project.md` records React and EF Core. Fixing that is a harness change, so it goes through `harness-improvement` with review.
- **The implementation skill's branch-name example differs from the project's convention.** It suggests `work-items/<WI-###>`, while the project uses `feature/<WI-###>-slug` (WI-001 DEC-009). The explicit project decision wins.
- **Open project decisions remain** (deployment host, merge/deploy permissions, translation sync, demo videos, the full role matrix). They are listed in `ai/project.md`.

The next test is Screen B (production-order list), the first screen built on top of an existing one. It's the first time the harness has to evolve a live schema and reuse shared modules (`GET /api/products`, `DD-001-FN`) rather than create them.
