# Production Order List (Screen B) — Test Plan

## Test plan identifier

TP-003, work item WI-003, revision 1, 2026-09-22.

## References

- `brief.md` revision 1 (REQ-020–REQ-027), `decisions.md` DEC-001–DEC-012
- 002_BD version 3, 002_DB, 002_DD version 3, 002_DD-API, 002_DD-FN, 002_DD-SPD
- `plan.md` revision 2, steps 5–12

## Introduction

Covers Screen B (SCR-002) end to end: the list query's validation and normalization, the query itself against real
PostgreSQL with the seeded demo data, the React screen's states and view-state handling, and browser journeys against
the Docker Compose stack. Most coverage is at unit level, less at integration, least at E2E (`ai/rules/testing.md`).

Screen B is read-only (DEC-003), so there is nothing to test about persistence beyond reading: no create, update or
delete path is exercised here. What replaces it is query correctness — filters, ordering, paging bounds — and the fact
that a crafted query cannot widen the result set.

## Test items

| Requirement ID | Description |
| --- | --- |
| REQ-020 | Open the list and see existing orders (incl. the empty and error states) |
| REQ-021 | Row shows the identifying and planning fields, with the overdue marker |
| REQ-022 | Filter by status (multi-select), product, due-date range and order number, combined with AND |
| REQ-023 | Sort by any of the six keys, both directions, over the whole result set |
| REQ-024 | Server-side paging with a user-chosen page size |
| REQ-025 | Open an order in Screen A; start a new one |
| REQ-026 | Only authenticated Admin/Operator |
| REQ-027 | Filters, sort, page and page size kept in the URL |

## Features to be tested

All REQs above, plus the design decisions with observable behavior: the multi-select status filter (DEC-005), the
unfiltered default view (DEC-006), the seeded demo data (DEC-007, DEC-011), explicit Search (DEC-008), row activation
by link (DEC-009), the trigram fragment match and its escaping (DEC-010), index usage (002_DB), and accessibility
(WI-002 DEC-026). Cases TC-101–TC-119 below.

## Features not to be tested

- Screens A and C beyond the one Screen A navigation change WI-003 makes: Screen A's own suite (TP-002) still runs and
  was updated for its new Cancel target.
- Browsers other than Chromium, and real mobile devices: E2E uses Playwright Chromium (desktop + Pixel 7 emulation).
  Known gap, unchanged from TP-002.
- A manual screen-reader pass: axe covers the automated WCAG 2.2 AA rules only. Known gap.
- Query behavior at production volume: the seeded set is 80 rows. The index assertions (TC-119) prove the indexes can
  serve the queries, not that the planner prefers them at that size — it correctly does not. Known gap, recorded in
  002_DB's performance expectations.

## Approach

| Level (unit / integration / system / E2E / smoke) | Included? | Rationale |
| --- | --- | --- |
| Unit — backend (xUnit) | yes | Query validation, normalization and the overdue rule, without a host or a database |
| Unit — frontend (Vitest + RTL + vitest-axe) | yes | Screen states, URL view state, sorting and paging behavior, a11y rules in jsdom |
| Integration (xUnit + WebApplicationFactory + Testcontainers Postgres 17) | yes | Real HTTP pipeline, auth, the real query and its indexes, the seed |
| E2E (Playwright Chromium + @axe-core/playwright) | yes | Browser journeys, real-browser a11y including contrast, against the Compose stack |
| Smoke | yes | Covered by the existing Compose smoke checks; Screen B adds no new service |
| System (other) | no | E2E against the full Compose stack already is the system-level check |

## Item pass/fail criteria

A case passes when every test implementing it passes and the observable behavior matches the brief's acceptance
criteria and the DD, with no unhandled error. A requirement passes when all its cases pass.

## Suspension criteria and resumption requirements

Suspend if Docker is unavailable (integration and E2E cannot run) or a blocking defect stops a journey. Resume once
Docker is running, or once the defect is fixed and re-verified. A suspended case is recorded as not run, never passed.

## Test deliverables

- This file; test code in `tests/backend/.../ProductionOrders/`, `tests/integration/.../ProductionOrders/`,
  `src/frontend/tests/unit/production-orders/`, `tests/e2e/specs/`
- Execution results in `evidence.md`

## Cases

Test names are the actual test methods and titles (U = backend unit, I = integration, F = frontend unit, E = E2E).

| Test ID | Requirement ID | Precondition / setup | Steps | Expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| TC-101 | REQ-020 | Signed in; 80 seeded orders | I `DefaultView_ReturnsFirstPageInDueDateOrder_WithEveryStatusAndTheTotal`; E `opens from the home page with the default view…` | 20 rows, `total` 80, due date ascending, every status present, no filter pre-applied | high |
| TC-102 | REQ-021 | Rows present | I `Row_CarriesTheDisplayedFieldsOnly`; F `shows each row with its identifying and planning fields` | Order number, SKU + name, quantity, due date, status label, updated; no GUID, no bare enum name, no notes or version in the payload | high |
| TC-103 | REQ-021 | Plant today = D | U `PastDue_ActiveOrder_IsOverdue`, `PastDue_TerminalOrder_IsNotOverdue`, `DueToday_IsNotOverdue`, `DueTomorrow_IsNotOverdue`; I `OverdueFlag_IsSetOnlyForPastDueActiveOrders`; F `marks an overdue row with text, not colour alone` | Overdue only for `Draft`/`InProgress` before D; marker is text | high |
| TC-104 | REQ-022 (DEC-005, DEC-006) | Seeded data | U `Statuses_AreParsedAndDeduplicated`, `UnknownStatus_IsRejected`; I `StatusFilter_IsMultiSelect_AndAbsenceMeansNoRestriction` | 32 Draft, 56 Draft+InProgress, 80 unfiltered; duplicates collapse; unknown → MSG-E018 | high |
| TC-105 | REQ-022 | Seeded data | U date and fragment parsing cases; I `ProductDueRangeAndFragmentFilters_EachReturnExactlyTheMatchingRows` | Each filter alone returns exactly its rows; range inclusive both ends and open-ended; fragment matches case-insensitively mid-string | high |
| TC-106 | REQ-022 | Seeded data | I `Filters_CombineWithAnd_AndTheTotalMatchesTheRowCount` | AND semantics; total equals the filtered row count and is smaller than either filter alone | high |
| TC-107 | REQ-022 | — | U `EveryOffendingParameter_IsReportedTogether` and the per-field rejection cases; I `InvalidQueryParameter_Is400_WithItsMessageIdOnThatField` (11 cases), `UnknownProduct_Is400_AndAllOffendingParametersAreReportedTogether` | 400 with the field's message ID; several bad parameters reported together; no exception text in the body | high |
| TC-108 | REQ-022 (DEC-010) | Order `PO-…-00042` exists | U `WildcardCharactersInTheFragment_AreEscapedSoASearchCannotWidenItself`; I `WildcardsInTheFragment_AreMatchedLiterally_SoASearchCannotWidenItself` | `%`, `_`, `\` are literal; searching `%` returns nothing while `2026` still matches | high |
| TC-109 | REQ-023 | Seeded data | U sort-key parsing and echo cases; I `EverySortKey_OrdersTheWholeResultSet_AndTheDirectionReverses`; F `toggles the direction…`, `marks the active column with aria-sort…`; E `sorting reorders the whole result set and toggles direction` | All six keys sort the full set; direction reverses; status sorts in workflow order; unsupported key → MSG-E019 | high |
| TC-110 | REQ-023, REQ-024 | Orders sharing a due date (25 of 80 do) | I `Paging_IsStable_SoEveryRowAppearsExactlyOnceAcrossPages` | 8 pages of 10 contain each of the 80 rows exactly once | high |
| TC-111 | REQ-024 | Seeded data | U page-size and page-bound cases; I `PageSizeAndPageBounds_BehaveAsDesigned`; F `keeps the paging controls usable on a page past the last one` | 10/20/50/100 accepted, default 20; page past the last returns an empty page with the real total; other sizes → MSG-E019 | high |
| TC-112 | REQ-024 | On page 3 | F `returns to page 1 when a filter, the sort or the page size changes` | Page resets to 1 on every filter, sort or page-size change | medium |
| TC-113 | REQ-025 (DEC-009) | Rows present | E `a row opens Screen A by mouse and by keyboard, and New opens create mode`; F row link assertion | Enter on the focused order-number link and a click on the row body both open the edit screen; New opens create mode | high |
| TC-114 | REQ-026 | No session / no role | I `Unauthenticated_Is401_AndAUserWithoutARole_Is403_WithNoOrderData`; F `shows the forbidden panel without querying`; E `a signed-out visitor is sent to the login screen` | 401 then redirect to `/login`; 403 with no order data in the body; the UI issues no query at all without a role | high |
| TC-115 | REQ-027 | A filtered, sorted, paged view | F `reads filters, sort and paging from the URL…`, `falls back to the default view for unreadable values…`, `writes the applied filters to the URL and omits defaults`; E `filtering, paging and page size keep the view in the URL` | Reload and Back reproduce the view; `sort=bogus&page=-1` renders the default view, not an error | high |
| TC-116 | REQ-020, REQ-022 | Empty table / non-matching filter | F `shows the empty state, and no filter panel…`, `shows the no-match state and keeps the filter values…`; E no-match half of the URL journey | MSG-I003 with New; MSG-I004 with Clear filters and the filter values kept | medium |
| TC-117 | REQ-020 | API returns 500 | F `shows an error banner with Retry, and Retry re-runs the same query` | MSG-E013 banner; Retry re-issues the identical query | medium |
| TC-118 | REQ-021, REQ-023 | Table rendered | F `has no axe violations with rows rendered`, `…in the empty state`, `marks the active column with aria-sort and no other column`; E `expectNoAxeViolations` in every Screen B journey | No axe violations; `aria-sort` on exactly one column; contrast passes in a real browser | high |
| TC-119 | REQ-022, REQ-023 (002_DB) | Seeded data, `ANALYZE` run | I `ListQueries_UseTheirIndex` (2 cases) | The default-sort query uses `ix_production_orders_due_date_order_number`; the fragment query uses `ix_production_orders_order_number_trgm` | medium |

## Results

Recorded in `evidence.md` with the commands, environment and counts. Summary as of 2026-09-22: every case above passes.
