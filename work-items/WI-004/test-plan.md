# Production Dashboard (Screen C) — Test Plan

## Test plan identifier

TP-004, work item WI-004, revision 1, 2026-09-22.

## References

- `brief.md` revision 2 (REQ-028–REQ-042, and the REQ-019 extension), `decisions.md` DEC-001–DEC-025
- 003_BD v3, 001_BD v7, 002_BD v4, 003_DB, 003_DD v4 with 003_DD-API v2, 003_DD-FN v4, 003_DD-SPD v2; 001_DD v4 set
- `plan.md` revision 3, steps 7–12

## Introduction

Covers Screen C (SCR-003) end to end: the dashboard snapshot and its calendar rules, the health check and its session
rule, completion stamping in Screen A, the demo seed, the shared navbar and navigation guard on every screen, the icons,
and browser journeys against the Docker Compose stack. Most coverage is at unit level, less at integration, least at
E2E (`ai/rules/testing.md`).

Everything that depends on "today" is tested with a pinned clock: the backend unit tests pin a `FakeTimeProvider`,
and the integration tests pin plant today at Wednesday 2031-06-11 on their own container, with the orders cleared as
the owner (003_DD "Test data isolation"). The seed is tested separately, on a fresh container with the real clock
(TC-218). Boundaries are placed where UTC and `Asia/Tokyo` disagree, so a UTC mistake fails a test.

## Test items

| Requirement ID | Description |
| --- | --- |
| REQ-028 | The dashboard is the landing page; error and empty states |
| REQ-029 | Status count tiles |
| REQ-030 | Overdue and due-soon groups |
| REQ-031 | Workload by due week: overdue, 8 weeks, later |
| REQ-032 | Top 10 products by open quantity |
| REQ-033 | Completion time recorded by Screen A's save |
| REQ-034 | Completed this week and this month |
| REQ-035 | On-time rate over 30 days |
| REQ-036 | Completion trend over 12 weeks |
| REQ-037 | Average lead time over 30 days |
| REQ-038 | Admin/Operator only |
| REQ-039 | Read-only, no drill-down |
| REQ-040 | Navbar on every authenticated screen |
| REQ-041 | Server and database health indicator |
| REQ-042 | Maximize and restore a chart |
| REQ-019 (WI-002, extended by DEC-022) | Confirm before discarding changes, now on any in-app link |

## Features to be tested

All REQs above, plus the decisions with observable behavior: one snapshot (DEC-011, DEC-015), the seed (DEC-013,
DEC-014), SVG charts with table equivalents (DEC-010), the health states and timeouts (DEC-020), the no-renew session
rule (DEC-019, DEC-025), breadcrumbs kept (DEC-021), icons decorative (DEC-023), and accessibility. Cases TC-201–TC-228.

## Features not to be tested

- Browsers other than Chromium, and real mobile devices: Playwright Chromium, desktop plus Pixel 7 emulation. A known
  gap, unchanged from TP-002/TP-003.
- A manual screen-reader pass: axe covers the automated WCAG 2.2 AA rules only. A known gap.
- Query cost at production volume: the demo holds 124 rows. TC-219 proves the partial indexes can serve their queries,
  not that the planner prefers them at that size. A known gap, recorded in 003_DB.
- A daylight-saving plant zone: `Asia/Tokyo` has none. `StartOfDayUtc`'s DST-gap rule is reviewed in code only.
- A real database outage in E2E: the unavailable state is tested by replacing the ping in integration (TC-224) and by
  mocking the response in frontend unit tests (TC-226).
- Browser Back, reload and tab close on an edited form: intentionally unguarded (DEC-022).

## Approach

| Level (unit / integration / system / E2E / smoke) | Included? | Rationale |
| --- | --- | --- |
| Unit — backend (xUnit) | yes | Completion stamping, calendar windows for every weekday and month boundary, mapping and rounding, health outcomes |
| Unit — frontend (Vitest + RTL + vitest-axe) | yes | Page states, formatters, health polling with fake timers, navbar, navigation guard, dialog focus, icons |
| Integration (xUnit + WebApplicationFactory + Testcontainers Postgres 17) | yes | Real pipeline and SQL with a pinned clock, auth, the snapshot transaction, the cookie rule, constraints, seed and indexes |
| E2E (Playwright Chromium + @axe-core/playwright) | yes | Browser journeys and real-browser accessibility (including contrast) against the Compose stack |
| Smoke | yes | Local smoke run of both endpoints against the migrated Compose database (evidence.md) |
| System (other) | no | E2E against the full Compose stack is the system-level check |

## Item pass/fail criteria

A case passes when every test implementing it passes and the observable behavior matches the brief's acceptance
criteria and the DD, with no unhandled error. A requirement passes when all its cases pass.

## Suspension criteria and resumption requirements

Suspend if Docker is unavailable (integration and E2E cannot run) or a blocking defect stops a journey. Resume once
Docker is running, or once the defect is fixed and re-verified. A suspended case is recorded as not run, never passed.

## Test deliverables

- This file. Test code in `tests/backend/.../ProductionOrders/ProductionOrderCompletionTests.cs`,
  `tests/backend/.../Dashboard/`, `tests/integration/.../Dashboard/`, `src/frontend/tests/unit/dashboard/`,
  `tests/e2e/specs/screen-c.spec.ts` and `mobile.spec.ts`
- WI-003 suites updated to the new seed: `ProductionOrderListEndpointTests.cs`, `OrderNumberingTests.cs`,
  `screen-b.spec.ts`, and the E2E `signIn` helper
- Execution results in `evidence.md`

## Cases

Test names are the actual test methods and titles (U = backend unit, I = integration, F = frontend unit, E = E2E).

| Test ID | Requirement ID | Precondition / setup | Steps | Expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| TC-201 | REQ-028 | Seeded stack | E `login lands on the dashboard with every widget, healthy status and no axe violations`; F `renders every widget from one snapshot, in plant time…` | Login lands on `/`; every widget, the snapshot time in plant time, Dashboard current in the navbar; tiles sum to the total | high |
| TC-202 | REQ-039 (DEC-006) | Ready state | F `is read-only: no widget is a link, and it asks only for the snapshot and health`; E `widgets are read-only and the navbar reaches every screen…` | No link inside the page body; only `GET /api/dashboard` and the health checks are requested | high |
| TC-203 | REQ-029 | Pinned clock, own rows | U `Missing_statuses_are_zero_and_total_is_their_sum`; I `StatusCounts_AreExact_ZeroFilled_AndSumToTheTotal` | Exact counts; a status with none is 0; total = sum | high |
| TC-204 | REQ-030 | T = 2031-06-11 | I `OverdueAndDueSoon_UseTheBoundariesInD01AndD02`, `OverdueGroup_ReturnsTheFirstTen_InOrder_WithTheFullTotal` | T−1 overdue; T and T+7 due soon; T+8 neither; terminal orders in neither; 10 rows in order with total 12 | high |
| TC-205 | REQ-031 (DEC-009) | Every weekday | U `Week0_is_the_Monday_on_or_before_today_for_every_weekday` (7 cases), `Workload_has_exactly_ten_buckets…`, `The_current_week_bucket_starts_today…`; I `Workload_PutsEveryActiveOrderInExactlyOneOfTenBuckets` | 10 buckets in order; each active order in exactly one; the current week starts today; sum = Draft + In progress | high |
| TC-206 | REQ-032 | 12 products, one tie, one completed-only | I `TopProducts_RanksTenByOpenQuantity_TiesBySku_ActiveOnly` | 10 products, quantity descending, tie by SKU, completed-only product absent | high |
| TC-207 | REQ-033 | InProgress order | U `Completing_an_in_progress_order_records_the_save_time`, `Other_saves_never_record_a_completion_time` (5 cases), `Editing_a_completed_order_later_keeps_its_completion_time`, `A_rejected_transition_records_nothing`; I `CompletingThroughTheApi_RecordsTheSaveTime_AndARequestCannotSetIt` | Set on InProgress → Completed only, equal to `updated_at_utc`; unchanged by later edits; never from a request; not exposed | high |
| TC-208 | REQ-033 (003_DB) | Migrated database | I `CompletionChecks_RejectInconsistentRows` (3 cases); I `Seed_ProducesTheFiguresDb004States` (no negative lead time) | Each inconsistent update is a check violation (23514) | high |
| TC-209 | REQ-034 | Completions at plant midnights | U `Utc_bounds_are_plant_midnights_not_utc_midnights`, `Month_start_on_the_first_of_the_month_is_today`; I `CompletedThisWeekAndMonth_UsePlantMidnights` | Monday 00:00 JST in the week, Sunday 23:59 JST not; 1 June 00:00 JST in the month, 31 May 23:59 not | high |
| TC-210 | REQ-035 | Six orders around the boundaries | U mapper on-time cases; I `OnTimeRate_ComparesThePlantCompletionDate_OverThirtyDays`; F `rounds the on-time rate half up…` | 3 of 4: completion on the due date is on time; 00:30 JST the day after is late (UTC would say on time); T−29 in, T−30 out; cancelled excluded; client 77%/33%/67%/— | high |
| TC-211 | REQ-036 | Four completions | U `Trend_has_twelve_weeks_oldest_first_with_zeros_filled`; I `Trend_HasTwelveWeeks_ZeroFilled_BucketedInPlantTime` | 12 weeks, zero-filled; Monday 00:00 JST counts in its own week; 13 weeks ago excluded | high |
| TC-212 | REQ-037 | Lead times 1.25, 2.0, 2.1 days | U `Lead_time_is_rounded_half_away_from_zero_to_one_decimal` (3 cases); I `LeadTime_IsTheMeanInDays_RoundedToOneDecimal`; F `prints the lead time with one decimal…` | 1.8 days over 3 orders; none → null → "—" | high |
| TC-213 | REQ-038 | No session / no role | I `Unauthenticated_Is401_AndNoRole_Is403` (both endpoints); F `shows the permission panel on 403…`, `never calls the API for a signed-in user without Admin or Operator` | 401; 403 with an empty body; MSG-E021; no request without a role | high |
| TC-214 | REQ-028 (DEC-011, DEC-015) | Mixed rows | I `Snapshot_IsOneReadOnlyTransaction_AndItsFiguresAgree` | `SET TRANSACTION READ ONLY` precedes all seven `production_orders` statements (Npgsql tracing); workload sum = Draft + In progress; overdue bucket = overdue total | high |
| TC-215 | REQ-028 | No orders | U `Nothing_completed_gives_null_lead_time_and_zero_counts`; I `EmptySystem_Is200_WithZerosAndNoAverage`; F `shows zeros, "none" messages and — for an empty system…` | 200 with zeros and `averageDays` null; MSG-I005–MSG-I008 and "—"; no error | medium |
| TC-216 | REQ-028 | Snapshot 500 | F `shows the error banner with Retry…`; E `a failed load shows Retry and no figures; Retry recovers` | MSG-E013 with Retry; no figures; the navbar still works; Retry loads | medium |
| TC-217 | REQ-031, REQ-036 (DEC-010) | Each state | F axe in `renders every widget…` and `toggles each chart table…`; E `expectNoAxeViolations` in every Screen C journey | No axe violations, including contrast; charts `role="img"` named with every value; View as table exposes a real table | high |
| TC-218 | 003_DB (DEC-013, DEC-014) | Fresh container, real clock | I `Seed_ProducesTheFiguresDb004States` | 124 orders 35/25/56/8; every trend week ≥ 2; every workload bucket > 0; 23 of 30 on time; 13.7 days; 15 overdue, 8 due soon; top 10; counter 124 | high |
| TC-219 | 003_DB | Seeded, `ANALYZE`, seqscan off | I `PartialIndexes_CanServeTheDashboardQueries` (2 cases) | The overdue query uses `ix_production_orders_active_due_date`; the completion range uses `ix_production_orders_completed_at_utc` | medium |
| TC-220 | 003_BD SP layout | Pixel 7 | E `SP dashboard: menu navbar, two tiles per row, charts scroll in their card, never the page` | Navbar behind Menu; tiles side by side; chart card scrolls; the page does not | medium |
| TC-221 | 003_DD-API | — | I `QueryString_IsIgnored` | Identical body with a crafted query string | medium |
| TC-222 | REQ-040 (DEC-016) | Each route | F `on %s marks %s as the current page` (4 cases), `opens the SP menu, closes it on Escape…`, `navigates from a navbar link and closes the SP menu`; E `widgets are read-only and the navbar reaches every screen…`, SP journey | Three links everywhere; exactly one `aria-current="page"`, edit route under Production orders; SP menu `aria-expanded`, Escape returns focus | high |
| TC-223 | REQ-019 (DEC-022) | Edited create form | F `leaves at once when nothing was edited`, `asks first; Keep editing stays with the values; Discard goes to the clicked link`, `resets the form when the clicked link is the page itself`, `never intercepts a modified click (new tab)`; E `an edited order asks before a navbar link leaves it…` | Dialog before leaving; Keep editing keeps values; Discard follows the link, or resets on the same page; Ctrl-click not intercepted | high |
| TC-224 | REQ-041 (DEC-020) | App as `pmai_app` | U `A_database_that_answers_is_ok`, `A_database_error_is_unavailable`, `A_ping_that_outlasts_the_timeout_is_unavailable`, `A_caller_that_goes_away_is_not_reported_as_unavailable`; I `Health_ReportsOnlyTheDatabaseVerdict_AndIsNotCached`; I 401/403 in `Unauthenticated_Is401_AndNoRole_Is403` | Two fields only; `no-store`; failure and stall both "unavailable" with 200; no error text or host in the body | high |
| TC-225 | REQ-041 (DEC-019, DEC-025) | Cookie clock advanced 8 days | I `HealthPoll_DoesNotRenewTheCookie_ButTheDashboardDoes` | No `Set-Cookie` on the health request; `Set-Cookie` on the dashboard request | high |
| TC-226 | REQ-041 | Fake timers | F `checks at once, then shows the statuses…`, `reports a database the server cannot reach`, `reports an unreachable server with the database unknown…`, `polls every 30 s, chained so checks never overlap`, `pauses while the tab is hidden…`, `stops polling after unmount`, `leaves out the time until the plant zone is known…` | HS-01–HS-05 as specified; no overlapping checks; pause and resume; the time outside the live region | high |
| TC-227 | REQ-042 (DEC-018) | Ready state | F `toggles each chart table, and maximizes a chart in a dialog that restores focus`; E `a chart maximizes to a dialog, keeps focus inside, and restores focus on Escape` | Modal dialog named by the chart; Restore focused; Tab stays inside; Escape/Restore close and focus returns to Expand; no request | high |
| TC-228 | DEC-023 | Ready state | F `keeps every icon decorative, and names every icon-only control` | Every Lucide `<svg>` `aria-hidden`; every button has an accessible name | medium |

WI-003 regression (not new cases): TC-101, TC-104, TC-109, TC-110, TC-111 and TC-115 were updated for the 124-order
seed. The sort test now reads every page, and the URL journey checks for `Completed`, because the oldest due dates now
belong to historical completed orders. Screen A's and Screen B's suites otherwise run unchanged.

## Results

Recorded in `evidence.md` with the commands, environment and counts. Summary as of 2026-09-22: every case above passes.
