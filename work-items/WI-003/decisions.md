# Production Order List (Screen B) — Decision Log

Decisions for WI-003. Decisions carried over from earlier work items keep their original IDs and are referenced as `WI-00N DEC-0MM`; IDs in this file are WI-003's own.

## Log

| ID | Date | Decision needed | Decision maker | Status | Rationale (summary) |
| --- | --- | --- | --- | --- | --- |
| DEC-001 | 2026-09-22 | Which filters and search the list offers | user | decided | Full set: status, product, due-date range, and free-text on order number |
| DEC-002 | 2026-09-22 | How volume and ordering are handled | user | decided | Server-side paging and sorting, with a user-selectable page size |
| DEC-003 | 2026-09-22 | Which actions a list row offers | user | decided | Open the order in Screen A, plus a "New production order" button; no write path on the list |
| DEC-004 | 2026-09-22 | Whether the list becomes the landing page | user | decided | No — the placeholder home page stays; the list gets its own route and a link from home |
| DEC-005 | 2026-09-22 | Status filter: single-select or multi-select | user | decided | Multi-select, so "all active orders" is one view |
| DEC-006 | 2026-09-22 | Whether the default view includes `Completed` and `Cancelled` orders | user | decided | Show all orders by default; no filter is pre-applied |
| DEC-007 | 2026-09-22 | Whether to seed demo production orders for paging/filtering | user | decided | Seed roughly 60–100 demo orders, as WI-002 DEC-019 seeded 30 products |
| DEC-008 | 2026-09-22 | Whether filters apply as they change or on an explicit Search | Claude (UI, during BD-002) | decided | Explicit Search button; sort, paging and page size still apply immediately |
| DEC-009 | 2026-09-22 | How a list row is activated without losing keyboard access | Claude (UI/accessibility, during BD-002) | decided | The order-number cell is a real link; the whole-row click is a mouse convenience resolving to it |
| DEC-010 | 2026-09-22 | Index for the case-insensitive order-number fragment search | Claude (technical, during DB-003) | decided | `pg_trgm` GIN index on `order_number`; input upper-cased and matched with `LIKE` |
| DEC-011 | 2026-09-22 | Whether seeded due dates are fixed calendar dates or relative to the migration run date | Claude (technical, during DB-003) | decided | Relative to the run date; everything else fixed; insert guarded to an empty table |
| DEC-012 | 2026-09-22 | Whether the demo seed should also apply to the integration-test database | Claude (technical, during implementation) | decided | Yes — the tests run the real migration set; Screen A's numbering test now asserts the sequence continues from the seeded counter |
| DEC-012 | 2026-09-22 | Whether the demo seed should also apply to the integration-test database | Claude (technical, during implementation) | decided | Yes — the tests run the real migration set; Screen A's numbering test now asserts the sequence continues from the seeded counter |

## DEC-001: Which filters and search the list offers

### Context

Screen B has to make an order findable among many. The filter set drives the API contract, the query shape and the indexes DB design has to provide, so it is settled before basic design starts.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Full set: status, product, due-date range, free-text order number | Matches how a planner actually searches; exercises real query and index design in the demo | Largest design and test surface of the three |
| Status filter plus free-text | Smaller scope, faster | Cannot answer "what is due this week for this product" |
| Free-text only | Minimal | Barely more than a lookup by order number |

### Decision and rationale

- **Decision:** the full set — status, product, due-date range and a free-text box matching the order number, combined with AND (REQ-022).
- **Decided by:** user, 2026-09-22 (Screen B scope questions, option "Full set").
- **Rationale:** best demo value, and it is the combination that forces genuine query and index design rather than a single-predicate lookup.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | REQ-022 |
| BD-002, DD-002, DD-002-API | Filter fields, their validation and the query contract |
| DB design | Indexes covering each filter and the default sort |

## DEC-002: How volume and ordering are handled

### Context

The list must stay bounded regardless of how many orders exist, and the demo has to show ordering behavior.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Server-side paging and sorting, user-selectable page size | Bounded queries; realistic; page size adapts to the reviewer's screen | Page-size value must be validated and bounded server-side |
| Server paging with one fixed sort | Simplest paged option | No column sorting; less useful and less to demo |
| Load everything, sort and filter in the browser | Simplest to build | Unbounded query; unrealistic for a manufacturing screen |

### Decision and rationale

- **Decision:** server-side paging and sorting, with a page-size dropdown for the user (10, 20, 50, 100; default 20) and a default sort of due date ascending (REQ-023, REQ-024).
- **Decided by:** user, 2026-09-22 ("server side paging + sort but there a dropdown to select the page size for the user").
- **Rationale:** keeps every query bounded while letting a reviewer widen the page during a demo. The allowed page sizes are an explicit list so an arbitrary or unbounded size cannot be requested through the API.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | REQ-023, REQ-024 |
| DD-002-API | `page`, `pageSize`, `sort`, `direction` parameters and their validation |
| BD-002, mockup | Paging controls, page-size dropdown, sortable column headers |

## DEC-003: Which actions a list row offers

### Context

The list could either stay read-only or carry write actions such as an inline status change.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Open in Screen A, plus a "New production order" button | No new write path; Screen A stays the single place where an order changes | A status change takes two steps |
| Also inline status change on the row | Faster for a planner | Duplicates Screen A's state-machine rules in a second place; more design, more tests, more risk |
| View only | Smallest | Leaves Screen A reachable only by URL, which was the gap Screen B exists to close |

### Decision and rationale

- **Decision:** activating a row opens the order in Screen A's edit mode, and a "New production order" button opens Screen A's create mode. The list exposes no save, status-change or delete action (REQ-025).
- **Decided by:** user, 2026-09-22 (Screen B scope questions, option "Open in Screen A + New button").
- **Rationale:** keeps every write path and the status state machine in one screen, so WI-002's rules are not re-implemented or bypassed.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | REQ-025 |
| BD-002 | Screen-transition diagram between Screen B and Screen A |
| Frontend | Routing only; no new mutating API call |

## DEC-004: Whether the list becomes the landing page

### Context

WI-001's placeholder home page carries a temporary "New production order" link, added because no list screen existed (see the comment in `src/frontend/src/App.tsx`).

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Keep the placeholder home; list on its own route | Smallest change to WI-001's shipped behavior; home stays free for Screen C's dashboard | One extra click to reach the list |
| Make the list the landing page | Fewer clicks | Would have to be undone when Screen C (dashboard) arrives |

### Decision and rationale

- **Decision:** the placeholder home page stays and gains a link to the list; the list lives at `/production-orders`.
- **Decided by:** user, 2026-09-22 (Screen B scope questions, option "No, keep placeholder home").
- **Rationale:** avoids changing the landing route twice, since Screen C is the natural owner of the home page.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | Assumptions; REQ-025 route values |
| Frontend | New route `/production-orders`; home page link updated alongside the existing "New production order" link |

## DEC-005: Status filter — single-select or multi-select

### Context

REQ-022 requires a status filter. Single-select ("All", or exactly one status) is the simpler contract; multi-select lets a planner see, for example, `Draft` and `InProgress` together, which is the usual "active orders" view.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Multi-select | One view for "all active orders"; a repeated query parameter is a standard contract | Slightly more UI and validation, and more filter-combination test cases |
| Single-select | Simplest contract and UI | Cannot express "active orders"; a planner filters twice |

### Decision and rationale

- **Decision:** multi-select. The status filter accepts any combination of `Draft`, `InProgress`, `Completed` and `Cancelled`; selecting none means no status restriction (REQ-022).
- **Decided by:** user, 2026-09-22 (Screen B design questions, option "Multi-select").
- **Rationale:** "all active orders" is the view a planner needs most, and it cannot be expressed with a single-select filter. A repeated query parameter is a standard contract and is validated against the allow-list of status names.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | REQ-022 acceptance criteria |
| DD-002-API | Whether `status` is a single value or a repeated parameter |

## DEC-006: Whether the default view includes Completed and Cancelled orders

### Context

Over time terminal orders (`Completed`, `Cancelled`) dominate the table, so the default view decides whether the screen opens on useful information.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Show all by default (no filter applied) | Simplest to explain, and "no filters" matches the total count shown | The useful rows sink as history grows |
| Default to active statuses only | Opens on what a planner acts on | A hidden default filter has to be made visible and clearable, or it confuses the total count |

### Decision and rationale

- **Decision:** show all orders by default — the screen opens with no filter applied, so the total it reports is the total number of orders (REQ-020).
- **Decided by:** user, 2026-09-22 (Screen B design questions, option "All orders").
- **Rationale:** a hidden default filter makes the displayed total ambiguous and has to be explained; with DEC-005's multi-select filter, a planner reaches the active-orders view in one action. History growth is a real concern but is better answered later by a saved-view feature than by a hidden default.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | REQ-020, REQ-022 acceptance criteria |
| BD-002, DD-002 | Initial filter state and how it is displayed |

## DEC-007: Whether to seed demo production orders

### Context

The database currently has 30 seeded products (WI-002 DEC-019) and no seeded production orders. Paging, sorting and filtering cannot be demonstrated or E2E-tested on an empty table without creating data by hand first.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Seed roughly 60–100 demo orders in a migration | Paging and filtering are demonstrable immediately; E2E tests get stable fixtures | Seed data reaches every environment the migrations run in, including any future non-demo one |
| Seed only in E2E test setup | Keeps the database clean | A manual demo starts on an empty screen |
| No seed | Nothing to maintain | Neither the demo nor a realistic E2E run has data |

### Decision and rationale

- **Decision:** seed roughly 60–100 demo production orders, in the same way the 30 products were seeded — fixed IDs and fixed timestamps in a migration, spread across all four statuses, several products and a range of due dates (past, today and future) so paging, sorting, the overdue marker and every filter have data. The exact count, the environment scope of the seed and its reversibility are settled in DB-003.
- **Decided by:** user, 2026-09-22 (Screen B design questions, option "Seed ~60-100 orders").
- **Rationale:** without data, neither the demo nor the E2E tests can exercise paging or filtering, and creating orders by hand before each demo is not repeatable. Matching WI-002 DEC-019's approach keeps the seed reproducible.

### Impact

| Artifact | Change required |
| --- | --- |
| DB design, migration | A seed migration and its fixed IDs/dates |
| test-plan | Whether E2E relies on seeded data or creates its own |

## DEC-008: Whether filters apply as they change or on an explicit Search

### Context

REQ-022 gives the screen four filters, two of them free-form (a date range and a text fragment). Applying a filter the moment it changes means deciding when a half-typed date or order number counts as a value, and it puts a query — and a history entry (REQ-027) — behind every keystroke.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Explicit **Search** button (plus Enter in the panel) | One query per search; predictable; validation runs at a defined moment; clean URL history | One extra click after setting the filters |
| Apply as values change, debounced | Feels immediate | Queries fire on partial input; a debounce interval becomes a hidden, untestable rule; noisy history and noisy announcements for screen-reader users |

### Decision and rationale

- **Decision:** the filter panel is a form applied by a **Search** button or by pressing Enter inside it; **Clear** resets the filters and re-queries. Sorting, paging and the page-size change still take effect immediately, because each is a single unambiguous value with nothing to finish typing (BD-002 §6 E-11 to E-15).
- **Decided by:** Claude (UI), 2026-09-22, while writing BD-002; open to the user's revision at design review.
- **Rationale:** a defined submit moment makes the filter validation (V-09, V-10), the URL history (REQ-027) and the announced result count all testable, and avoids a debounce constant that no requirement pins down.

### Impact

| Artifact | Change required |
| --- | --- |
| BD-002 | §3 items 12–13, §6 E-11, E-12 |
| DD-002, test-plan | Event spec and the test cases for applying and clearing filters |

## DEC-009: How a list row is activated without losing keyboard access

### Context

REQ-025 says a row is activated "by mouse or keyboard". A whole row made clickable with a JavaScript handler is not keyboard-operable and has no accessible role, which would fail WCAG 2.2 AA and `ai/rules/frontend.md`.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Real link in the order-number cell, plus a mouse click anywhere on the row | Keyboard and screen-reader access come from a real link; the large mouse target is still there | The row handler must ignore clicks that are text selections or that land on another interactive element |
| Whole row as a `<button>`/`role="link"` with a tab stop | One target | A row of cells inside a button breaks table semantics; the accessible name becomes the whole row's text |
| Link only, no row click | Simplest and safest | A narrow mouse target in a wide row |

### Decision and rationale

- **Decision:** the order-number cell of each row is a real link to `/production-orders/{id}`; clicking anywhere else in the row resolves to that same link as a mouse convenience, and does nothing when the click completes a text selection or lands on another interactive element. On SP, the whole card is the link (BD-002 §6 E-16).
- **Decided by:** Claude (UI/accessibility), 2026-09-22, while writing BD-002.
- **Rationale:** keeps table semantics and gives every row a genuine keyboard-reachable activation point, while preserving the wide click target a list screen is expected to have.

### Impact

| Artifact | Change required |
| --- | --- |
| BD-002 | §3 item 22, §6 E-16, non-functional accessibility |
| DD-002, test-plan | Interaction spec plus keyboard and axe coverage for row activation |

## DEC-010: Index for the case-insensitive order-number fragment search

### Context

REQ-022 requires the order-number filter to match a partial, case-insensitive fragment — a *contains* match (`LIKE '%…%'`). No btree index can serve a leading-wildcard pattern, so without a decision the filter would fall back to a sequential scan, contradicting the brief's "index-backed" success metric.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| `pg_trgm` GIN index on `order_number` | Keeps the contains semantics the requirement asks for and makes it index-backed | Needs the `pg_trgm` extension; fragments shorter than three characters still fall back to a scan; a GIN index costs more to update than a btree |
| btree on `upper(order_number)`, prefix matching only (`LIKE 'frag%'`) | Cheapest index; no extension | Changes the requirement — a planner searching "00042" would find nothing |
| No index | Nothing to maintain | Every search scans the table; fails the stated success metric |

### Decision and rationale

- **Decision:** create the `pg_trgm` extension and a GIN index `ix_production_orders_order_number_trgm` on `order_number` using `gin_trgm_ops`. The application upper-cases the fragment and matches with `LIKE` rather than `ILIKE`, since `order_number` is generated and always upper-case; `%`, `_` and `\` in the input are escaped so a search cannot widen itself (DB-003).
- **Decided by:** Claude (technical), 2026-09-22, while writing DB-003.
- **Rationale:** it is the only option that satisfies REQ-022 as written. `pg_trgm` is a trusted extension in PostgreSQL 17, so the migration's owner login can create it without superuser rights and the runtime login needs no new privilege.

### Impact

| Artifact | Change required |
| --- | --- |
| DB-003, DB-002 | New index and extension; DB-002's "deliberately not added" note for `order_number` now points here |
| DD-002-API | Fragment normalization and `LIKE` escaping |
| Migration | `CREATE EXTENSION IF NOT EXISTS pg_trgm` plus the index, created `CONCURRENTLY` |

## DEC-011: Whether seeded due dates are fixed or relative to the migration run date

### Context

DEC-007 asked for roughly 60–100 seeded orders. Their due dates decide whether the overdue marker (REQ-021) and the due-date range filter (REQ-022) demo meaningfully. DB-002's product seed uses EF Core `HasData`, which requires constant values.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Relative to the run date (`INSERT … SELECT` over a generated series) | The past/today/future mix stays realistic whenever the stack is rebuilt; the overdue marker always has rows to mark | Two runs on different days produce different due dates, so tests must not assert absolute dates; cannot use `HasData` |
| Fixed calendar dates (`HasData`, as the products use) | Byte-identical every run; consistent with DB-002's seed style | Every seeded order becomes overdue within months, and the screen demos badly from then on |

### Decision and rationale

- **Decision:** due dates and the created/updated timestamps are computed from the plant-local date at migration time; ids, order numbers, products, quantities, statuses and the day offsets themselves are fixed constants. The insert is guarded by `WHERE NOT EXISTS (SELECT 1 FROM production_orders)`, so it only ever fills an empty table and is idempotent. Written with `migrationBuilder.Sql(...)`, since `HasData` cannot express it (DB-003).
- **Decided by:** Claude (technical), 2026-09-22, while writing DB-003.
- **Rationale:** the seed exists to make the screen demonstrable; fixed dates would defeat that within a few months. Determinism is preserved where tests need it — counts, statuses, products and offsets from today are all fixed.

### Impact

| Artifact | Change required |
| --- | --- |
| DB-003 | Demo seed section, migration impact and rollback |
| test-plan, E2E | Assertions written against counts, statuses and offsets from today, never absolute dates |

## DEC-012: Whether the demo seed also applies to the integration-test database

### Context

The integration tests migrate a throwaway PostgreSQL container with the same migration set as any other environment,
so `SeedDemoProductionOrders` runs there too. That surfaced immediately: Screen A's `OrderNumberingTests` asserted the
first created order is `PO-YYYY-00001`, and it became `00081` behind the 80 seeded rows.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Let the seed run in tests, and adapt the affected assertion | Tests exercise the real migration set, seeded rows included; the list tests get realistic data for free; the seeded counter row is itself verified | An existing test has to change, and future tests can't assume an empty table |
| Skip the seed in the test environment (an environment flag on the migration) | Every test starts from an empty table | A migration that behaves differently under test is a migration the tests no longer prove; the list tests would each have to build their own fixtures |
| Delete the seeded rows in the test fixture | Empty table, seed still tested | Throws away the realistic data the list tests need, and hides the counter interaction |

### Decision and rationale

- **Decision:** the seed applies everywhere the migrations run, the integration-test database included. Screen A's
  numbering test now reads the seeded counter and asserts the first API-created order is exactly `last_seq + 1`,
  rather than `00001`.
- **Decided by:** Claude (technical), 2026-09-22, during implementation.
- **Rationale:** the changed assertion is stronger than the one it replaces — it verifies precisely what the seed's
  counter row exists to guarantee, that a seeded order number and a user-created one can never collide. A migration
  that behaved differently under test would be a migration the tests no longer prove.

### Impact

| Artifact | Change required |
| --- | --- |
| `OrderNumberingTests` | Asserts continuation from the seeded counter (TC-002 in TP-002 stays valid, with its expectation restated) |
| DB-003, TP-003, evidence | The seed's reach is stated, and future tests needing an empty table must arrange it |

## DEC-012: Whether the demo seed also applies to the integration-test database

### Context

The integration tests migrate a throwaway PostgreSQL container with the same migration set as any other environment,
so `SeedDemoProductionOrders` runs there too. That surfaced immediately: Screen A's `OrderNumberingTests` asserted the
first created order is `PO-YYYY-00001`, and it became `00081` behind the 80 seeded rows.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Let the seed run in tests, and adapt the affected assertion | Tests exercise the real migration set, seeded rows included; the list tests get realistic data for free; the seeded counter row is itself verified | An existing test has to change, and future tests can't assume an empty table |
| Skip the seed in the test environment (an environment flag on the migration) | Every test starts from an empty table | A migration that behaves differently under test is a migration the tests no longer prove; the list tests would each have to build their own fixtures |
| Delete the seeded rows in the test fixture | Empty table, seed still tested | Throws away the realistic data the list tests need, and hides the counter interaction |

### Decision and rationale

- **Decision:** the seed applies everywhere the migrations run, the integration-test database included. Screen A's
  numbering test now reads the seeded counter and asserts the first API-created order is exactly `last_seq + 1`,
  rather than `00001`.
- **Decided by:** Claude (technical), 2026-09-22, during implementation.
- **Rationale:** the changed assertion is stronger than the one it replaces — it verifies precisely what the seed's
  counter row exists to guarantee, that a seeded order number and a user-created one can never collide. A migration
  that behaved differently under test would be a migration the tests no longer prove.

### Impact

| Artifact | Change required |
| --- | --- |
| `OrderNumberingTests` | Asserts continuation from the seeded counter (TC-002 in TP-002 stays valid, with its expectation restated) |
| DB-003, TP-003, evidence | The seed's reach is stated, and future tests needing an empty table must arrange it |
