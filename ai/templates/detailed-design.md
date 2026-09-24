<!-- Detailed Design Document (詳細設計書) template — the main per-screen/module DD, matching ai/templates/example/DD/00-04-01.詳細設計_会員登録_入力画面.xlsx (module/container/rule design, include-file/component organization, validation, task index). It is one of a 4-file family mirroring the example set's own layout:
- detailed-design.md (this file) — the main screen/module DD, matching the example's top-level 00-04-01.詳細設計_....xlsx.
- DD/api-design.md — per-endpoint request/response field catalogs, matching example/DD/API/70-00-02.API仕様設計_....xlsx. Always produced: every endpoint this DD's screen calls is catalogued there, including small screen-owned ones; this file's "APIs used" only lists the endpoints and links to their sections.
- DD/function-design.md — per-method design of the backend service/Application-layer methods behind this DD's endpoints (shared across screens or not), matching example/DD/Functions/90-11-02.機能設計_....xlsx. Always produced: this file's "Module design" keeps the screen-owned domain entities/rules, UI components and controllers, and points to the companion for the service methods.
- DD/screen-processing-design.md — step-by-step processing flow (branching, redirects, per-component breakdown), matching example/DD/画面処理設計/00-04-01.画面処理設計_....xlsx. Always produced: the step-by-step flows live there, one block per component; this file's "Processing and state transitions" keeps only the state-transition table and a flow → section pointer table.
All four documents are always produced for every DD, as separate Markdown files side by side in docs/en/020_detailed-design/: `{###_DD}_{画面名}.md` (this file), `{###_DD}-API_{画面名}.md`, `{###_DD}-FN_{画面名}.md` and `{###_DD}-SPD_{画面名}.md` (`{画面名}` is the screen's Japanese name, the same as on its BD; RFC 0011), with document IDs `{###_DD}`, `{###_DD}-API`, `{###_DD}-FN` and `{###_DD}-SPD`. Each piece of content has exactly one home among the four; the others point to it rather than repeating it. When a companion's subject genuinely doesn't exist for a DD (e.g. a backend-only DD with no screen), the companion is still produced, with each of its sections marked "Not applicable — {reason}" rather than the file being skipped.
List all three companion documents in "Companion design documents" below. This file's own "Module design" keeps the reference workbook's Rule-type/Rule-references/Condition-references/Check-parameters sub-block (from its Container/Rule sheets) for a module that is itself a business rule or validator owned by this screen. Where a legacy ASPX-specific concept (e.g. an XML rule engine, .aspx include paths) has no direct equivalent in this project's .NET 10 + EF Core / React stack, fill that field with "not applicable — {reason}" rather than deleting it.
Copy into the relevant work item or docs/en/020_detailed-design area; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# {Screen / Module Name} — Detailed Design Document (詳細設計書)

{###_DD} — implements {###_BD}, requirements {REQ-###, …}.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | {###_DD} |
| Category | {e.g., UI, API, Function} |
| System name | {system name} |
| Subsystem name | {subsystem name, or "not applicable"} |
| Work item | {WI-###} |
| Implements | {###_BD} |
| Created by | {name} |
| Created date | {YYYY-MM-DD} |
| Last updated by | {name} |
| Last updated date | {YYYY-MM-DD} |

| Version | Date | Author | Revision content |
| --- | --- | --- | --- |
| 1 | {YYYY-MM-DD} | {name} | Initial creation |

## Overview and reference documents (概要・目次)

| Field | Value |
| --- | --- |
| File / component name | {e.g., ProductionOrderForm.tsx, ProductionOrdersController} |
| Overview | {one-line summary of what this file/component covers} |

### Module / method / processing index

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| {1} | {module, method, endpoint, or processing-flow name defined below} | {} | {} |

### Reference documents

{Documents this DD reads from or depends on — the reference-tracking convention from ai/templates/example/DD (kept both ways to avoid missed updates when either side changes).}

| No | Document | Purpose / use | Notes |
| --- | --- | --- | --- |
| {1} | {basic-design.md, database-design.md, an ADR, …} | {why it's referenced} | {} |

### Referenced by

| No | Document | Purpose / use | Notes |
| --- | --- | --- | --- |
| {} | {} | {} | {} |

{"None yet" if no other document references this DD.}

### Component / file organization

{Mirrors the reference workbook's X-1–X-5 include-file definition rules, one sub-part each. Leave a sub-part as "not applicable" (as the source itself does for X-3/X-5 on a simple screen) rather than deleting it.}

**X-1. Path structure** — where this DD's code lives, generalizing the source's root/common/function/API/html folder convention to this project's layered backend (`src/backend/{Domain,Application,Infrastructure,Api}`) and frontend (`src/frontend/src/...`) structure:

| No | Path / namespace | Purpose | Notes |
| --- | --- | --- | --- |
| {1} | {e.g. src/frontend/src/features/production-orders/} | {} | {} |

**X-2. Shared/common components used** — cross-feature utilities, hooks, or base components this screen depends on:

| No | Name | Purpose | Notes |
| --- | --- | --- | --- |
| {} | {} | {} | {} |

{"Not applicable — no shared components beyond the framework defaults" if none.}

**X-3. Feature-level components used** — components/services specific to this feature that this screen composes:

| No | Name | Purpose | Notes |
| --- | --- | --- | --- |
| {} | {} | {} | {} |

{"Not applicable — this screen has no sub-components of its own" if none.}

**X-4. External APIs used** — see "APIs used" below; not repeated here.

**X-5. Responsive composition** — how PC/desktop and SP/mobile layouts (from basic-design.md's per-breakpoint layout) are composed at implementation level: one shared component with responsive styling, or separate components per breakpoint.

{One or two sentences, or "not applicable — single breakpoint only".}

### Companion design documents

{All three companion documents are always produced (see header); list each one with what it covers, generalizing the reference workbooks' task index. Detailed review/task status stays in this work item's status.md — this table is only "what exists and what it covers," not a tracking sheet. A companion whose subject doesn't exist for this DD still gets a row, with "Not applicable — {reason}" in Covers.}

| No | Document | Type | Covers |
| --- | --- | --- | --- |
| 1 | {###_DD}-API_{画面名}.md | api-design | {endpoints} |
| 2 | {###_DD}-FN_{画面名}.md | function-design | {service methods} |
| 3 | {###_DD}-SPD_{画面名}.md | screen-processing-design | {per-component processing blocks} |

### Task / design index

{Mirrors the reference workbook's タスク一覧記載ルール (task list) sheet: every design/implementation task this screen requires, cross-referencing which document defines it and its review state. Repeat as needed; a large screen may have many rows.}

| No | Category | File-level task | Function/process-level task | Item-level task | Confirmed | Issue | Reviewer | Reworked | Date | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| {1} | {basic-design \| detailed-design \| api-design \| function-design \| screen-processing-design} | {which document} | {which section/module} | {which field/step, or "—"} | {yes \| no} | {open issue, or "—"} | {name} | {yes \| no} | {YYYY-MM-DD} | {} |

## Module design

{Repeat this block once per screen-owned domain entity/rule, UI component or controller. Backend service/Application-layer methods are designed in the function-design companion (always produced) — list them here only as a one-line pointer block, and reference them from "Dependencies" below.}

### {Module / method name}

| Field | Value |
| --- | --- |
| Description | {what it does, one or two sentences} |
| Return type | {type, or "void"} |
| Created by / date | {name} / {YYYY-MM-DD} |
| Last modified by / date | {name} / {YYYY-MM-DD}, {what changed} |

Preconditions: {what must be true before it runs, or "none"}.

{The four fields below mirror the reference workbooks' Container/Rule sheets, for a module that is itself a business rule or validator rather than a plain handler. Omit this sub-block entirely — write "not applicable — plain handler, no rule/validator fields" — when the module isn't one.}

Rule type: {e.g. format check, cross-field consistency, business invariant}.

**Rule references** — other rules/validators this one composes or delegates to:

| No | Rule / validator | Location | Notes |
| --- | --- | --- | --- |
| {} | {} | {} | {} |

**Condition / data references** — lookups or data sources this rule's decision depends on (e.g. a config value, a DB query, another entity's state):

| No | Key | Source | Notes |
| --- | --- | --- | --- |
| {} | {} | {} | {} |

**Check parameters** — what is being validated:

| No | Name | Type | Target field |
| --- | --- | --- | --- |
| {} | {} | {} | {} |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| {1} | {} | {} | {} |

{"None" if it takes no arguments.}

**Dependencies**

{Other modules/components/services this one calls or includes — generalizes the reference workbook's "使用INCファイル" (used include files).}

| No | Module / component | Overview | Notes |
| --- | --- | --- | --- |
| {} | {} | {} | {} |

{"None" if self-contained.}

Processing overview: {prose summary of the algorithm/flow}.

**Processing flow**

{Cite the module/component/endpoint a step calls in "Calls", per the reference workbooks' cross-document step references (e.g. "70-00-02.API仕様設計_会員情報, sheet 1" in the source becomes an entry in "APIs used" below, or another module's name).}

| Step | Description | Calls |
| --- | --- | --- |
| {1} | {} | {module/component/endpoint name, or "—"} |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| {} | {} | {} |

{"None — no return value / side-effect only" if applicable.}

## Screen layout and mockup

{Visual layout for this screen, at implementation fidelity: an SVG wireframe (authoritative for the field/region mapping, since it's plain XML and version-controlled), plus a rendered mockup covering the screen's key states — at minimum create/empty, populated/edit, a locked-or-restricted variant if any field is conditionally read-only, a validation-error state, and success. Produce the rendered mockup with the `design` skill (Claude Design canvas, published as an Artifact) per `ai/skills/screen-design/SKILL.md`, matching the frontend's existing visual vocabulary (styles/tokens already in `src/frontend`) rather than inventing a new look; a static mockup is the default, a clickable prototype only if asked. If this refines a BD-level wireframe, note what changed and why. "None" is not acceptable once fields are defined — a reader must be able to see where each field/control sits and, for anything the wireframe can't convey (color, exact styling), what it actually looks like.}

{The wireframe is an SVG, not ASCII art (RFC 0009), saved at `docs/en/020_detailed-design/{###}/wireframes/{###_DD}_{SCR-###}-pc.svg` and embedded as an image. It follows the BD wireframe conventions in `ai/templates/basic-design.md` (canvas widths, greyscale palette, numbered callout badges using the BD's item numbers, realistic sample values, a note for what isn't drawn), at DD fidelity: the representative populated state with its real labels, hint texts, locked or read-only styling, and error-text slots. Draw dialogs and whole-screen states the main wireframe can't show (empty, no-match) as their own SVGs, `…-dialog.svg` or `…-states.svg`, each embedded under a one-line caption. Add an SP wireframe (`…-sp.svg`) only where the DD changes the BD's SP layout.}

![{SCR-###} PC layout with numbered items](wireframes/{###_DD}_{SCR-###}-pc.svg)

{Mockup artifact: link to the published rendered mockup (default), or "none — {reason}" only for a DD with no visual screen (e.g. a backend-only endpoint).}

| Region | Contains (field/control) | Notes |
| --- | --- | --- |
| {region name from the wireframe above} | {field/control, cross-referenced to Screen item definition} | {responsive behavior, conditional visibility, or "none"} |

## Screen item definition

| Field | Type | Required | Validation rule | Source (BD ref) |
| --- | --- | --- | --- | --- |
| {field name} | {data type} | {yes \| no} | {format, range, cross-field rule} | {basic-design.md section} |

## Loading / empty / error / success states

| State | Trigger | UI behavior | Data shown |
| --- | --- | --- | --- |
| {loading \| empty \| error \| success} | {what puts the screen in this state} | {what the user sees/can do} | {placeholder, real data, or error message} |

## Processing and state transitions

{The resulting state transitions for each action from basic-design.md's "Actions and business rules." Step-by-step processing (phases, branching, dependencies) always lives in the screen-processing-design companion; here, keep only the state-transition table below and a "Processing flows" table mapping each flow to its section in that companion.}

### State transitions

{A Mermaid `stateDiagram-v2` (`direction LR`) of the transitions in the table below, then the table itself, which stays the authoritative list with each transition's side effect. The diagram shows the transitions between distinct states; transitions from "any" state and events that keep the state may stay table-only, with a one-line note under the diagram saying so. Example:}

```mermaid
stateDiagram-v2
    direction LR
    [*] --> {state}: {event}
    {state} --> {state}: {event}
    {state} --> [*]
```

| From state | Event | To state | Side effect |
| --- | --- | --- | --- |
| {state} | {event} | {state} | {e.g. record created, notification sent} |

### Processing flows

| Flow | Where (screen-processing-design / function-design section) | Events |
| --- | --- | --- |
| {flow name} | {{###_DD}-SPD §… / {###_DD}-FN §…} | {BD event IDs} |

## APIs used

{Endpoints this screen calls. Full request/response field catalogs always live in the api-design companion — list each endpoint here and link its section there; don't repeat the field tables.}

| Endpoint | Method | Purpose | Design doc |
| --- | --- | --- | --- |
| {/path} | {GET/POST/PUT/DELETE} | {} | {{###_DD}-API §…} |

## Database and transaction mapping

{Data-access conventions, mirroring the reference workbooks' Condition-file rules: one query per table (don't compose a multi-table query where two single-table ones stay clear), select only the columns actually used rather than the full row, and don't rewrite an existing working query without cause — note any deliberate exception below the table.}

| Operation | Table(s) | Transaction boundary | Concurrency handling |
| --- | --- | --- | --- |
| {create/update/delete/read} | {table(s) from database-design.md} | {what is atomic} | {locking/optimistic-concurrency approach, or "not applicable"} |

## Exception handling

| Failure | Retry policy | Timeout | User-facing error | Logging |
| --- | --- | --- | --- | --- |
| {what can fail} | {retry rule or "none"} | {timeout value or "not applicable"} | {message shown} | {what gets logged} |

## Test viewpoints and unresolved decisions

| Scenario | Precondition | Expected result | Test-plan ID |
| --- | --- | --- | --- |
| {scenario name} | {starting state} | {expected outcome} | {TC-### once test-plan.md is written} |

Unresolved decisions: {list, or "none" — link each to decisions.md}.
