# Production Order Create/Edit (Screen A) — Decision Log

WI-002 restarted on 2026-09-18 against the rewritten BD/DD templates. DEC-001–DEC-008 are business answers the user gave during the first (scrapped) design pass on 2026-09-16; they are carried over unchanged rather than re-asked, since the restart was caused by a template change, not a change in requirements. The original questions and answers are recorded in `demos/01-basic-design/WI-002/recording.md` (DEC-001–DEC-006) and `demos/03-database-design/WI-002/recording.md` (DEC-007, DEC-008). IDs are renumbered for this restart; technical decisions from the first pass (order-number generation, CSRF) are not carried over and will be re-decided in database-design/detailed-design.

## Log

| ID | Date | Decision needed | Decision maker | Status | Rationale (summary) |
| --- | --- | --- | --- | --- | --- |
| DEC-001 | 2026-09-16 | Who may create/edit a production order | user | decided | Both `Admin` and `Operator` |
| DEC-002 | 2026-09-16 | Core form fields | user | decided | Standard set: order number (generated), product, quantity, due date, status, notes |
| DEC-003 | 2026-09-16 | Status workflow | user | decided | Draft → InProgress → Completed; Cancelled reachable from Draft or InProgress |
| DEC-004 | 2026-09-16 | Edit behavior once an order is not Draft | user | decided | Lock product and quantity once the order is not Draft |
| DEC-005 | 2026-09-16 | How "product" is represented | user | decided | Reference a minimal Product table, no catalog UI |
| DEC-006 | 2026-09-16 | Order number / quantity / due-date validation rules | user | decided | Standard rules: generated unique order number, quantity positive integer, due date ≥ today, notes ≤ 500 chars |
| DEC-007 | 2026-09-16 | Locked-field change submitted alongside other valid changes | user | decided | Reject the whole request |
| DEC-008 | 2026-09-16 | Status-transition UI control | user | decided | Restricted dropdown — only valid next states selectable |
| DEC-009 | 2026-09-18 | Due-date rule for an existing order already past its due date | user | decided | Check only on create or when the due date value is changed |
| DEC-010 | 2026-09-18 | Concurrent edits of the same order | user | decided | Reject a save based on stale data; user reloads |
| DEC-011 | 2026-09-18 | Timezone that defines "today" | user | decided | One configured plant timezone, applied server-side |
| DEC-012 | 2026-09-18 | Order number format | user | decided | `PO-YYYY-NNNNN`, sequence restarts each year |
| DEC-013 | 2026-09-18 | Per-year order-number generation and overflow | Claude (technical) | decided | Counter table upserted in the create transaction; reject past 99,999/year |
| DEC-014 | 2026-09-18 | Optimistic-concurrency token | Claude (technical) | decided | PostgreSQL `xmin` system column via EF Core |
| DEC-015 | 2026-09-18 | Status storage | Claude (technical) | decided | `varchar(20)` + CHECK constraint |
| DEC-016 | 2026-09-18 | Separate migration-owner and runtime DB roles | user | decided | Split now: owner login runs migrations, restricted login at runtime (local Compose included) |
| DEC-017 | 2026-09-18 | Which plant timezone | user | decided | `Asia/Tokyo` |
| DEC-018 | 2026-09-18 | Cancel with unsaved changes | user | decided | Ask "Discard your changes?" only if the form was edited |
| DEC-019 | 2026-09-18 | Demo product seed data | user | decided | 30 sample products |
| DEC-021 | 2026-09-18 | Client-side "today" for the due-date check | Claude (technical) | decided | Browser-local date for early feedback; server's plant date is authoritative |
| DEC-022 | 2026-09-18 | Product picker for 30 products | Claude (UI) | decided | Native `<select>` ordered by SKU; no searchable combobox yet |
| DEC-023 | 2026-09-18 | API error contract | Claude (technical) | decided | RFC 9457; 400 validation / 404 / 409 stale / 422 rule violation; `code` = message ID |
| DEC-024 | 2026-09-18 | Quantity upper bound | Claude (technical) | decided | 999,999,999 (9 digits, 001_BD width), MSG-E010 |
| DEC-025 | 2026-09-18 | E2E test tool | user | decided | Playwright (`@playwright/test`), in `tests/e2e`, against the Compose stack |
| DEC-026 | 2026-09-18 | Automated accessibility checks | user | decided | `vitest-axe` in component tests + `@axe-core/playwright` in E2E |
| DEC-027 | 2026-09-18 | Git operations authorized for implementation | user | decided | Branch, worktree, local commits, push, open PR; merge not authorized |
| DEC-028 | 2026-09-18 | Where the harness changes go | user | decided | Their own branch/PR, `feature/harness-wi002-feedback` (RFC 0001 + RFC 0002, one commit each) |
| DEC-030 | 2026-09-18 | Merge PR #2 and PR #3 | user | decided | The user squash-merged both into `master` although CI couldn't run (GitHub billing lock) |
| DEC-029 | 2026-09-18 | Index on `production_orders.product_id` | Claude (technical, during implementation) | decided | Keep it: EF Core's FK convention always creates it; 001_DB updated |
| DEC-020 | 2026-09-18 | CSRF protection for order endpoints | Claude (technical security) | decided | No extra anti-forgery token: `SameSite=Lax` cookie + no CORS policy + JSON-only request bodies |

## DEC-001: Who may create/edit a production order

**Status:** decided

### Context

WI-001 seeded placeholder `Admin`/`Operator` roles, and WI-001 DEC-015 requires the permission matrix to be confirmed before a screen gates on a permission.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Admin and Operator both | Simple; matches a small shop where planners are operators | No separation of duties |
| Admin only | Tighter control | Operators can't record progress |

### Decision and rationale

- **Decision:** Admin and Operator both may view, create and edit.
- **Decided by:** user, 2026-09-16 (recommended option).
- **Rationale:** keeps the demo screen simple; finer permissions can come later.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md REQ-012, 001_BD 0-1 / actions | Role restriction = Admin, Operator (resolves WI-001 DEC-015 for Screen A only) |

## DEC-002: Core form fields

**Status:** decided

### Context

The screen's fields were not defined in the project description (§14 leaves them to the brief).

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Standard set (order number, product, quantity, due date, status, notes) | Enough to show layout, validation, state machine | — |
| Extended (priority, line/work centre, BOM) | More realistic | Scope too large for the demo |

### Decision and rationale

- **Decision:** standard set.
- **Decided by:** user, 2026-09-16.
- **Rationale:** smallest set that exercises every lifecycle step.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md, 001_BD §3 screen items, DB design | Field list as above plus created/updated timestamps |

## DEC-003: Status workflow

**Status:** decided

### Context

Edit mode needs a defined lifecycle.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Draft → InProgress → Completed, Cancelled from Draft/InProgress | Covers the common case, terminal states clear | — |
| Free status selection | Simple | No business rule to demonstrate |

### Decision and rationale

- **Decision:** Draft → InProgress → Completed; Cancelled reachable from Draft or InProgress; Completed and Cancelled are terminal.
- **Decided by:** user, 2026-09-16.
- **Rationale:** a realistic, testable state machine.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md REQ-017, 001_BD status transition | State machine as above |

## DEC-004: Edit behavior once an order is not Draft

**Status:** decided

### Context

Changing what is being produced after production starts is a business risk.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Lock product and quantity once not Draft | Protects in-flight production; due date/notes stay adjustable | Needs a new order to change product/quantity |
| Everything editable | Simple | Unrealistic |

### Decision and rationale

- **Decision:** product and quantity are read-only once status is not `Draft`; due date and notes stay editable.
- **Decided by:** user, 2026-09-16.
- **Rationale:** protects in-flight production without blocking schedule/notes updates.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md REQ-018, 001_BD §3/§5/§6 | Per-status editability |

## DEC-005: How "product" is represented

**Status:** decided

### Context

The earlier product-catalog screen was dropped from the roadmap (WI-001 DEC-010).

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Minimal Product table, seeded, no catalog UI | Real foreign key; tiny scope | Products can only change via seed/migration |
| Free-text product name | Simplest | No referential integrity to demonstrate |

### Decision and rationale

- **Decision:** minimal Product reference table (name, SKU), seeded with sample data; no catalog UI.
- **Decided by:** user, 2026-09-16.
- **Rationale:** demonstrates a reference relationship without adding a screen.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_BD data overview, DB design | Product entity + seed data; product lookup API for the dropdown |

## DEC-006: Validation rules

**Status:** decided

### Context

Acceptance criteria need exact rules.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Standard rules (generated unique order number; quantity positive integer; due date ≥ today; notes ≤ 500) | Clear, testable | — |
| User-entered order number | Matches some legacy practices | Uniqueness errors, more UI |

### Decision and rationale

- **Decision:** standard rules as listed.
- **Decided by:** user, 2026-09-16.
- **Rationale:** clear acceptance criteria with success and failure cases.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md REQ-010, REQ-013–REQ-016; 001_BD §5 | Validation table. Order-number format/generation mechanism is a database-design decision. |

## DEC-007: Locked-field change submitted alongside other valid changes

**Status:** decided

### Context

The UI makes locked fields read-only, but the API can still receive a changed product/quantity for a non-Draft order.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Reject the whole request | Explicit, no partial saves | Client must resend without the locked change |
| Ignore locked fields, apply the rest | Forgiving | Silent data loss is hard to notice |

### Decision and rationale

- **Decision:** reject the whole request; nothing is saved.
- **Decided by:** user, 2026-09-16.
- **Rationale:** no silent partial updates.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md REQ-018, 001_BD exception flows | Whole-request rejection |

## DEC-008: Status-transition UI control

**Status:** decided

### Context

How the user picks a new status in edit mode.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Restricted dropdown (current status + valid next states only) | Invalid transitions can't be picked | Server still validates |
| Free dropdown + server rejection | Simpler UI logic | Users hit avoidable errors |

### Decision and rationale

- **Decision:** restricted dropdown; server remains authoritative.
- **Decided by:** user, 2026-09-16.
- **Rationale:** prevents avoidable errors.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_BD §3/§4/§6 | Status select options depend on current status; disabled for terminal statuses |

## DEC-009: Due-date rule for an existing order already past its due date

**Status:** decided

### Context

REQ-014 says due date must be today or later. Applied literally on every save, an order whose due date has passed (e.g. an overdue `InProgress` order) could not be saved at all — including to mark it `Completed` — unless the user also moves the due date forward. This changes business behavior, so it needs the user's answer (`ai/policies.md` pause conditions). 001_BD revision 1 was drafted with the proposal as an open item; revision 2 records the decision.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Check only on create or when the due date value is changed (proposal) | Overdue orders can still be completed/cancelled/annotated; past dates can't be newly entered | Slightly more complex rule |
| Check on every save | Simplest rule | Overdue orders become unsaveable |
| Check only on create | Simplest edit behavior | Allows moving an existing due date into the past |

### Decision and rationale

- **Decision:** check the rule on create, and on edit only when the due date value differs from the saved value.
- **Decided by:** user, 2026-09-18 (recommended option).
- **Rationale:** overdue orders stay saveable (complete, cancel, notes) while no one can newly enter a past date.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md REQ-014, 001_BD §5 validation V-04 | Finalize the check condition once decided |

## DEC-010: Concurrent edits of the same order

**Status:** decided

### Context

Two users can open the same order at once. 001_BD revision 1 left open what happens when the second one saves.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Reject stale save | No silent overwrite | User must reload and redo changes |
| Last write wins | Simplest | Changes can be lost silently |

### Decision and rationale

- **Decision:** a save based on data that changed since it was loaded is rejected; nothing is saved and the user is told to reload.
- **Decided by:** user, 2026-09-18 (recommended option).
- **Rationale:** no silent data loss. The concurrency-token mechanism is a DD/DB technical choice.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md REQ-011, 001_BD exception flows / V-08 | Stale-save rejection; 001_DD and 001_DB choose the mechanism |

## DEC-011: Timezone that defines "today"

**Status:** decided

### Context

Both the due-date rule (REQ-014, DEC-009) and the order-number year (DEC-012) depend on what "today" means.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| One configured plant timezone, server-side | Same answer for every user; server authoritative | Needs a configuration value |
| Each user's browser timezone | Matches the user's clock | Server must trust client dates; users can disagree |

### Decision and rationale

- **Decision:** a single configured plant timezone; the server computes "today" in it. The client may use the same value for early feedback, but the server decides.
- **Decided by:** user, 2026-09-18 (recommended option).
- **Rationale:** a consistent, server-authoritative rule for a single-site app.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_BD V-04, 001_DD configuration, 001_DB | Plant timezone configuration value; "today" and the order-number year computed in it |

## DEC-012: Order number format

**Status:** decided

### Context

DEC-006 made the order number system-generated and unique but did not fix its format.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| `PO-000001` (never resets) | Simplest generation | No year context |
| `PO-YYYY-NNNNN` (yearly reset) | Year visible at a glance | Needs a per-year sequence |
| Plain number | Simplest | Not recognisable as an order number |

### Decision and rationale

- **Decision:** `PO-YYYY-NNNNN` — "PO-", the 4-digit year the order was created (plant timezone, DEC-011), "-", then a 5-digit zero-padded sequence that restarts at 00001 each year (e.g. `PO-2026-00001`).
- **Decided by:** user, 2026-09-18.
- **Rationale:** user preference. The generation mechanism, and what happens if a year exceeds 99,999 orders, are 001_DB technical decisions.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_BD layout / §3 item 6, 001_DB | Format and per-year sequence generation |

## DEC-013: Per-year order-number generation and overflow

**Status:** decided

### Context

DEC-012 needs a sequence that restarts every year and stays unique when several users create orders at once. These are technical choices that don't change business behavior, so they were decided during database design (001_DB) rather than asked.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Counter table `production_order_number_counters`, upserted in the create transaction | Concurrency-safe via row lock; no gaps on rollback; no runtime DDL | Same-year creates are serialized (negligible at this volume) |
| `MAX(order_seq) + 1` with retry on unique violation | No extra table | Race-prone; needs retry logic |
| One PostgreSQL `SEQUENCE` per year | Fast, no lock contention | Needs DDL at runtime; gaps on rollback |

For overflow past 99,999 orders in one year: reject the create (CHECK constraint), or widen to six digits (changes the DEC-012 format).

### Decision and rationale

- **Decision:** counter table upserted in the create transaction; a CHECK constraint limits the sequence to 1–99999, so the 100,000th order in a year fails with a server error.
- **Decided by:** Claude (technical decision within the approved plan, step 4).
- **Rationale:** simplest concurrency-safe option. The overflow is unreachable at demo volume, and handling it this way keeps the user-chosen format intact.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_DB | Counter table, constraints, issuance SQL |
| 001_DD | Create processing flow calls the counter upsert in the same transaction |

## DEC-014: Optimistic-concurrency token

**Status:** decided

### Context

DEC-010 requires rejecting stale saves.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| PostgreSQL `xmin` system column (Npgsql EF Core row version) | No schema column; changes on every update automatically | PostgreSQL-specific; value wraps (irrelevant for equality checks) |
| Explicit `version integer` column | Portable, readable | Must be incremented on every update |

### Decision and rationale

- **Decision:** map `xmin` as the concurrency token; expose it to the client as `version`.
- **Decided by:** Claude (technical decision within the approved plan, step 4).
- **Rationale:** it's the standard Npgsql approach and needs no column or increment logic. The project is committed to PostgreSQL (WI-001 DEC-003).

### Impact

| Artifact | Change required |
| --- | --- |
| 001_DB, 001_DD API contract | `version` field in responses and update requests; mismatch → V-08 |

## DEC-015: Status storage

**Status:** decided

### Context

REQ-017 has four status values.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| `varchar(20)` + CHECK | Readable; easy to add a value with an additive migration | Slightly larger than a code |
| PostgreSQL `ENUM` type | Type-safe | Changing values needs `ALTER TYPE`; extra EF mapping |
| `smallint` code | Compact | Unreadable in SQL and backups |

### Decision and rationale

- **Decision:** `varchar(20)` with a CHECK listing the four values, mapped by EF Core as a string enum conversion.
- **Decided by:** Claude (technical decision within the approved plan, step 4).
- **Rationale:** readable and easy to evolve.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_DB | `ck_production_orders_status` |

## DEC-016: Separate migration-owner and runtime DB roles

**Status:** decided

### Context

`ai/rules/database.md` asks for least privilege. Today the app and the migrations both connect as the Compose `POSTGRES_USER`, which owns the database. 001_DB lists the minimum runtime privileges Screen A needs.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Split now (owner role runs migrations, restricted role at runtime) | Meets the rule immediately | Changes WI-001's Compose/deployment setup, which is outside this plan's design scope |
| Keep the single owner role for the local demo; split before any shared/non-local deployment | No change to WI-001's setup now | Rule not yet met locally |

### Decision and rationale

- **Decision:** split now. The existing owner login (`POSTGRES_USER`) is used only to run migrations. The backend connects at runtime with a new restricted login that has only the privileges listed in 001_DB (plus what ASP.NET Core Identity and the startup seeder need). This applies to the local Compose environment too.
- **Decided by:** user, 2026-09-18 (chose "Split now" over the recommended "split before non-local deployment").
- **Rationale:** meet `ai/rules/database.md` least privilege from the first business table on, rather than retrofitting it later.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_DB | Runtime login and grant list |
| Plan revision 2 (implementation) | Must include: creating the runtime login and grants (SQL script run as the owner), a second connection string in `deploy/compose.yaml`/`.env.example`, and updating the `dotnet ef database update` command in `ai/project.md` to use the owner login. Changes WI-001's deploy config, so the revision needs review |
| 000_DB | Identity-table grants for the runtime login |

## DEC-017: Which plant timezone

**Status:** decided

### Context

DEC-011 made one configured plant timezone the source of "today" (REQ-014) and of the order-number year (DEC-012), but did not name it.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| `Asia/Ho_Chi_Minh` (UTC+7) | — | — |
| `Asia/Tokyo` (UTC+9) | — | — |
| `UTC` | No conversion | "Today" can differ from the plant's local date near midnight |

### Decision and rationale

- **Decision:** `Asia/Tokyo` (IANA ID; no daylight saving time). It is held as a configuration value so it can change without code changes.
- **Decided by:** user, 2026-09-18.
- **Rationale:** the plant's local timezone.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_BD V-04, 001_DB, 001_DD configuration | "Today" and the order year are computed in `Asia/Tokyo` |

## DEC-018: Cancel with unsaved changes

**Status:** decided

### Context

001_BD revision 2 had Cancel leave immediately. That was a Claude assumption, not a recorded decision.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Confirm only if the form was edited | Prevents accidental loss of work | One more dialog to build and test |
| Always leave without asking | Simplest | Unsaved changes are lost silently |

### Decision and rationale

- **Decision:** when Cancel is pressed and any field differs from the loaded (edit) or initial (create) values, show a confirmation dialog "Discard your changes?" with **Discard** and **Keep editing**. If nothing changed, leave immediately. This covers the Cancel button only; browser back/close and header links are not covered.
- **Decided by:** user, 2026-09-18 (recommended option).
- **Rationale:** avoids losing work by accident.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md REQ-019, 001_BD §3/§6 | New requirement; confirmation dialog items and event |

## DEC-019: Demo product seed data

**Status:** decided

### Context

001_DB revision 1 seeded 5 placeholder products.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| 5 samples | Minimal | Doesn't show how the product dropdown copes with a realistic list |
| ~15 samples | More realistic | — |
| 30 samples (user's choice) | Shows the screen under a realistic list size | Longer seed list |

### Decision and rationale

- **Decision:** seed 30 sample products (listed in 001_DB).
- **Decided by:** user, 2026-09-18 ("let do 30 sample so we could get a sense of the screen work load").
- **Rationale:** gives a realistic sense of the product dropdown's load and usability in the demo.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_DB seed data | 30 rows |
| 001_DD | Product dropdown usability with 30 entries (e.g. ordering, and whether it needs to be searchable) |

## DEC-020: CSRF protection for order endpoints

**Status:** decided

### Context

0002_ADR left open whether cookie-authenticated state-changing endpoints need an anti-forgery token on top of `SameSite`. The first WI-002 pass concluded "no", but that record was reset and had to be reaffirmed. The current code sets the auth cookie `HttpOnly` and `SameSite=Lax` (`DependencyInjection.cs`) and registers no CORS policy.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| No extra token: `SameSite=Lax` + no CORS + JSON-only bodies | No extra client/server plumbing; blocks the classic attacks (a cross-site form POST doesn't carry the cookie under `Lax`; a cross-origin `fetch` with `application/json` needs a preflight, which fails without CORS) | Doesn't protect against an attacker on a *same-site* origin (e.g. a compromised sibling subdomain) |
| `SameSite=Lax` + ASP.NET Core anti-forgery token | Defence in depth | Token endpoint, header plumbing in the fetch wrapper, extra tests |

### Decision and rationale

- **Decision:** no additional anti-forgery token. Mutating endpoints (`POST`/`PUT`) accept only `Content-Type: application/json` (anything else → 415). No CORS policy is registered, and state-changing GET endpoints are not allowed.
- **Decided by:** Claude (technical security decision; 0002_ADR delegated it to detailed design). The user can revisit it.
- **Rationale:** sufficient for a single-origin app on one host. Revisit if the app is ever served next to other same-site origins, or if a CORS policy is added.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_BD non-functional requirements, open questions | Marked decided |
| 001_DD API contract | JSON-only bodies, 415 on other content types |
| 0002_ADR, 000_DB | Replace the "reaffirm or revisit" note with a reference to this decision |

## DEC-021: Client-side "today" for the due-date check

**Status:** decided

### Context

"Today" is the plant date in `Asia/Tokyo` (DEC-017), which the server holds as configuration. The client-side check (V-04) is only there for early feedback.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Browser-local date (advisory); the server decides | No extra endpoint, no duplicated config | A user outside Japan near midnight can pass the client check and then get MSG-E005 from the server (still shown inline) |
| Hard-code `Asia/Tokyo` in the frontend | Exact for everyone | Duplicates a config value that can drift |
| Extra endpoint returning the plant date | Exact, single source | One more endpoint and request for an edge case |

### Decision and rationale

- **Decision:** the browser-local date, used for advisory checks only.
- **Decided by:** Claude (technical, during detailed design). The user can revisit it.
- **Rationale:** the server is authoritative anyway, and the mismatch only affects users outside the plant timezone, near midnight.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_DD item (10), `validation.ts` | As described |

## DEC-022: Product picker for 30 products

**Status:** decided

### Context

DEC-019 seeds 30 products "to get a sense of the screen work load". No UI component kit is selected (`ai/project.md`).

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Native `<select>` ordered by SKU, labels `{sku} — {name}` | Accessible and keyboard-friendly for free; fine at 30 items | Type-ahead matches only the start of the label (the SKU), not the name |
| Custom searchable combobox | Search by name | Hand-built ARIA combobox is significant work and accessibility risk without a component kit |

### Decision and rationale

- **Decision:** native `<select>`. Revisit if the product list grows well beyond 30 or users ask for name search.
- **Decided by:** Claude (UI, during detailed design). The user can revisit it after seeing the mockup.
- **Rationale:** smallest accessible solution for the agreed data size.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_DD item (8) | Native select |

## DEC-023: API error contract

**Status:** decided

### Context

`ai/rules/backend.md` requires RFC 9457 Problem Details. 001_BD defines message IDs.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| 400 validation, 404, 409 stale, 422 business-rule violation; `code` = message ID; `errors` map of field → message IDs | Each failure has one distinct status; the client maps IDs to text in one place | Custom model-state response factory needed |
| Everything as 400 with messages | Simpler | The client can't tell stale saves or rule violations apart |

### Decision and rationale

- **Decision:** first option (details in 001_DD-API).
- **Decided by:** Claude (technical, during detailed design).
- **Rationale:** distinct UI behavior per failure (inline, banner, banner + Reload).

### Impact

| Artifact | Change required |
| --- | --- |
| 001_DD, 001_DD-API | Error tables |

## DEC-024: Quantity upper bound

**Status:** decided

### Context

001_BD gives quantity a 9-digit width but no explicit maximum. The column is `integer`.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| 999,999,999 (matches the 9-digit width) | Consistent with the BD; safely inside `int32` | — |
| `int32` max (2,147,483,647) | No extra rule | Doesn't fit the 9-digit field |

### Decision and rationale

- **Decision:** 1–999,999,999, with the new message MSG-E010 "Quantity can't exceed 999,999,999."
- **Decided by:** Claude (technical, during detailed design).
- **Rationale:** makes the BD width explicit as a rule.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_BD V-02, 001_DD | Upper bound + MSG-E010 |

## DEC-025: E2E test tool

**Status:** decided

### Context

No E2E tool was selected; WI-001 deferred a Playwright smoke spec. `ai/rules/testing.md` forbids choosing a test framework silently. 001_DD lists E-level scenarios.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Playwright | Multi-context (stale-save test), axe integration, fast | New toolchain in `tests/e2e` |
| Cypress | Popular | Single-tab model makes the two-user stale-save journey awkward |
| No E2E | Nothing new | 001_DD E-level scenarios uncovered |

### Decision and rationale

- **Decision:** Playwright, in its own `tests/e2e` package, run locally against the Compose stack (not in CI for now).
- **Decided by:** user, 2026-09-18 (recommended option).
- **Rationale:** covers the journeys, including two concurrent users.

### Impact

| Artifact | Change required |
| --- | --- |
| plan.md revision 2 step 11, `tests/e2e/README.md`, `ai/project.md` | New commands and tool |

## DEC-026: Automated accessibility checks

**Status:** decided

### Context

001_DD requires an automated axe check (tool deferred to plan revision 2).

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| `vitest-axe` + `@axe-core/playwright` | Catches regressions at component and page level | Two dev dependencies |
| Manual only | No dependencies | Not repeatable |

### Decision and rationale

- **Decision:** both axe integrations.
- **Decided by:** user, 2026-09-18 (recommended option).
- **Rationale:** repeatable WCAG 2.2 AA checks.

### Impact

| Artifact | Change required |
| --- | --- |
| plan.md revision 2 steps 10–11 | Dev dependencies and checks |

## DEC-027: Git operations authorized for implementation

**Status:** decided

### Context

`ai/policies.md`: the scaffold doesn't authorize push/PR/merge; task-specific authorization is needed.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Local branch + commits only | Most conservative | User pushes and opens the PR |
| Local + push + open PR | End-to-end delivery; runs CI for the first time | Opening a PR triggers GitHub Actions |
| No commits | — | Slow |

### Decision and rationale

- **Decision:** create branches/worktrees, commit, push, and open PRs to `master`. Merge, deploy and image publication are **not** authorized. Opening a PR triggers the existing CI workflow; this is accepted as part of the PR authorization.
- **Decided by:** user, 2026-09-18.
- **Rationale:** deliver a reviewable PR with CI evidence.

### Impact

| Artifact | Change required |
| --- | --- |
| plan.md revision 2 Resources table | As above |

## DEC-028: Where the DD-template harness change goes

**Status:** decided

### Context

RFC 0001 (always produce all four DD documents) changed shared `ai/` files. It isn't part of Screen A, and `ai/rules/git-review.md` asks for focused PRs.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Separate branch/PR | Focused PRs | Two PRs to review |
| Same branch, separate commit | One PR | Mixes topics |

### Decision and rationale

- **Decision:** own branch and its own PR. Amended 2026-09-18: when the user asked for a second harness improvement (RFC 0002), the branch was renamed `feature/harness-wi002-feedback` to hold both RFCs as separate commits in one harness PR.
- **Decided by:** user, 2026-09-18 (recommended option).
- **Rationale:** focused review.

### Impact

| Artifact | Change required |
| --- | --- |
| plan.md revision 2 steps 1, 14 | Separate branch and PR |

## DEC-029: Index on `production_orders.product_id`

**Status:** decided

### Context

001_DB (approved) deliberately omitted an index on `production_orders.product_id`. During implementation (plan revision 2, step 6), EF Core's foreign-key index convention generated one anyway. Removing it in entity configuration, and then in a model-finalizing convention, didn't work: the convention re-creates a foreign-key index whenever one is removed. Disabling the convention globally would drop the existing Identity-table foreign-key indexes in the next migration.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Keep the index, update 001_DB | No framework fighting; supports the RESTRICT check and Screen B's likely product filter | Small write cost on insert/update |
| Hand-edit the migration to drop the `CreateIndex` | Matches 001_DB | The model snapshot still contains the index, so every future migration diverges |
| Disable the FK-index convention globally | Full control | Drops the existing Identity FK indexes |

### Decision and rationale

- **Decision:** keep `ix_production_orders_product_id`, declared explicitly, and update 001_DB's index table.
- **Decided by:** Claude (technical; it doesn't change business behavior). The user can revisit it.
- **Rationale:** negligible cost at this data size, and it avoids a model/migration drift.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_DB index definitions | Row added; "deliberately not added" note struck through |

## DEC-030: Merge PR #2 and PR #3

**Status:** decided

### Context

Plan revision 2 didn't authorize merging (DEC-027). Both PRs were reviewed by the user. CI couldn't run on either: GitHub refused to start the jobs because the account is locked for billing. All checks had passed locally (`evidence.md`), and GitGuardian passed on both PRs. `master` has no branch protection.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Merge now | Local evidence complete | No CI run on Linux runners before merge |
| Wait for CI | CI evidence first | Blocked on an account issue outside the repo |

### Decision and rationale

- **Decision:** merge both PRs into `master` with "Squash and merge". The user did this from their own GitHub account: PR #2 at 02:47Z (`cadc67c`) and PR #3 at 03:18Z (`1eccf9c`, head `16ed4f3`). The user then asked Claude to "merge both PRs"; Claude's `gh pr merge` calls found both already merged and changed nothing.
- **Decided by:** user, 2026-09-18 ("merge both PRs"; "i used squad and merge").
- **Rationale:** user review complete. Squash merge is the user's convention for this project (one commit per PR on `master`).

### Impact

| Artifact | Change required |
| --- | --- |
| plan.md revision 2, status.md, evidence.md | Merge outcome recorded via a follow-up PR: this record was written after PR #3 was squash-merged, so it wasn't in it. CI still to run once the lock is cleared |
