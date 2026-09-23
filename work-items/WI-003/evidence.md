# Production Order List (Screen B) — Requirements Traceability & Evidence

As of `master` at `8eab65f`, the squash merge of PR #9, 2026-09-22.

## Traceability matrix

| Requirement ID | Requirement | Design artifact | Code | Test case ID | Status |
| --- | --- | --- | --- | --- | --- |
| REQ-020 | Open the list and see existing orders | 002_BD business flow, §3, §6 E-10; 002_DB access patterns and seed; 002_DD states; 002_DD-SPD §1, §6 | `ProductionOrderListPage`, `ProductionOrderService.ListAsync`, `SeedDemoProductionOrders` | TC-101, TC-116, TC-117 | verified |
| REQ-021 | Row shows identifying and planning fields | 002_BD §3 items 16–22, §4 M-05–M-08, M-10; 002_DB read projection; 002_DD-FN §6 | `ProductionOrderTable`, `ProductionOrderListMapper.ToListItem`, `ProductionOrderListRow` | TC-102, TC-103, TC-118 | verified |
| REQ-022 | Filter by status, product, due-date range, order number | 002_BD 0-3, §5 V-09–V-12, §6 E-11/E-12; 002_DB queries and trigram index; 002_DD-API request fields | `ProductionOrderListQuery.TryCreate`, `ProductionOrderQueryExtensions.ApplyFilters`, `ProductionOrderFilters`, `listValidation.ts` | TC-104–TC-108 | verified |
| REQ-023 | Sort by any listed column, both directions | 002_BD §3 items 16–21, §5 V-13, §6 E-13; 002_DB sort-key mapping; 002_DD-FN §5 | `ApplySort`, `ProductionOrderTable` header buttons | TC-109, TC-110, TC-118 | verified |
| REQ-024 | Server-side paging with a user-chosen page size | 002_BD §3 items 14/15/23, §4 M-09, §5 V-13; 002_DD-API paging parameters; 002_DD-SPD §5 | `ProductionOrderListQuery` (page/pageSize allow-list), `ListPagination`, `ListSummary` | TC-110–TC-112 | verified |
| REQ-025 | Open an order in Screen A, or start a new one | 002_BD screen transition, §6 E-16/E-17; 002_DD module 3 | `ProductionOrderTable` row and card links, `ProductionOrderListPage` New action, `App.tsx` route | TC-113 | verified |
| REQ-026 | Admin/Operator only | 002_BD 0-1, actions, non-functional security; 002_DD-API common auth | `ProductionOrdersController.List` under policy `ProductionOrderEditor`, `ProtectedRoute`, client role gate | TC-114 | verified |
| REQ-027 | View state kept in the URL | 002_BD 0-3, §6 E-10–E-15, E-19; 002_DD-SPD §2 | `listViewState.ts` (`parseViewState`, `toSearchParams`, `useListViewState`), `api.ts listOrders` | TC-115 | verified |

## Test execution log

| Date | Check | Command | Environment | Result (pass / fail / not run) | Report / log link |
| --- | --- | --- | --- | --- | --- |
| 2026-09-22 | design-consistency checklist (002_BD scope) | manual review | local | pass for BD-level items | walk below |
| 2026-09-22 | BD review | user review of 002_BD | — | pass — approved ("the BD look good") | status.md |
| 2026-09-22 | design-consistency checklist (002_DB scope) | manual review | local | pass — every index justified, omissions listed with reasons, migration impact and recovery limits stated | walk below |
| 2026-09-22 | DB review | user review of 002_DB | — | pass — approved ("the DB design is approved") | status.md |
| 2026-09-22 | design-consistency checklist (002_DD set scope) | manual review | local | pass — all four DD files, no duplicated content; one defect found and fixed (message-ID collision) | walk below |
| 2026-09-22 | Mockup published | Artifact publish, private | claude.ai | done — https://claude.ai/artifact/2XrZ9xnEfzQ6pCbUnZovL3 (7 artboards) | 002_DD |
| 2026-09-22 | DD review | user review of the 002_DD set | — | pass — approved ("the DD is approved") | status.md |
| 2026-09-22 | Backend build | `dotnet build src/backend/ProductionManagementAI.slnx` | local, .NET SDK 10 | pass — 0 warnings, 0 errors | — |
| 2026-09-22 | Backend unit | `dotnet test src/backend/ProductionManagementAI.slnx` | local | pass — 116/116 (49 existing + 67 new) | final run, after the accessibility fixes |
| 2026-09-22 | Backend integration | same command | local, Testcontainers `postgres:17`, Docker Desktop; app running as `pmai_app` | pass — 63/63 (38 existing + 25 new) | same run |
| 2026-09-22 | Migrations against the local Compose database | `dotnet ef database update …` as the owner | local Compose (`deploy/`) | pass — both migrations applied; 80 seeded orders across 55 distinct due dates; both new indexes present; `pg_index` reports no INVALID index, so the non-atomic CONCURRENTLY path completed | verification below |
| 2026-09-22 | Frontend lint | `npm run lint` | local, oxlint | pass — no findings | — |
| 2026-09-22 | Frontend build | `npm run build` | local, tsc + Vite | pass | — |
| 2026-09-22 | Frontend unit | `npm test` | local, Vitest + RTL + vitest-axe | pass — 56/56 (38 existing + 18 new) | — |
| 2026-09-22 | E2E | `npx playwright test` | local Compose stack; Playwright Chromium desktop + Pixel 7 | pass — 16/16 (8 existing + 8 new) | `tests/e2e/playwright-report/` |
| 2026-09-22 | security-review checklist | manual review | local | pass — walk below | this file |
| 2026-09-22 | delivery checklist | manual review | local | pass — walk below; re-checked after the push and PR | this file |
| 2026-09-22 | CI, run 1 (commit `435d837`) | GitHub Actions, on PR #9 | ubuntu-latest | pass — Backend 53s, Frontend 14s, E2E 2m35s | https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/35684528505 |
| 2026-09-22 | CI, run 2 (commit `2ca5f7a`, docs only) | GitHub Actions, on PR #9 | ubuntu-latest | **fail** — E2E: TC-109 read the table between the URL change and the new response. A race in the test, not in the product: the same commit's other 15 cases passed, and run 1 passed only by timing. Fixed by synchronizing on `aria-sort`, which comes from the rendered response | https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/35684953784 |
| 2026-09-22 | E2E after the race fix | `npx playwright test` (Screen B spec ×3, then the full suite) | local Compose stack | pass — 7/7 three times, then 16/16 | — |
| 2026-09-22 | CI, run 3 (commit `ea78a11`, the race fix) | GitHub Actions, on PR #9 | ubuntu-latest | pass — Backend 1m0s, Frontend 17s, E2E 2m16s | https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/35685579998 |

Database verification (owner connection, after `dotnet ef database update`):

```
 orders | due_dates |    min     |    max
--------+-----------+------------+------------
     80 |        55 | 2026-09-08 | 2026-11-01

 ix_production_orders_due_date_order_number
 ix_production_orders_order_number_trgm
 ix_production_orders_order_year_order_seq
 ix_production_orders_product_id
 pk_production_orders

 SELECT indexrelid::regclass FROM pg_index WHERE NOT indisvalid;  →  0 rows
```

### design-consistency checklist walk (002_BD, 002_DB and the 002_DD set, 2026-09-22)

| Checklist item | Result |
| --- | --- |
| Requirements have stable IDs and acceptance criteria | pass — REQ-020–REQ-027, each with success and failure criteria |
| BD covers navigation, primary actions and exceptions | pass — screen transition, actions table, 13 success/empty/exception flows |
| DD field/validation/state behavior agrees with BD | pass — 002_DD's items, nine states and transitions trace to 002_BD §3/§5/§6. Divergences found while implementing were folded back into the documents in the same change: string-bound query parameters (002_DD-API, 002_DD-SPD §7), enum values matched by name only (002_DD-API), the sort applied before the projection (002_DD-FN §3/§5), and the two contrast/link corrections (002_DD) |
| API and DB mappings agree, including constraints and errors | pass — 002_DB's mapping table matches 002_DD-API's field catalog and the implemented contract; an integration test asserts the response's exact field set |
| Missing decisions are resolved before dependent implementation | pass — DEC-001–DEC-012 were all decided before the code depending on them |
| Relevant test scenarios map to the design | pass — TP-003 maps TC-101–TC-119 to every REQ and to each decision with observable behavior; every case is implemented and passing |
| Security-relevant fields identified | pass — no PII or secret on this screen; the allow-listed sort/page size, parameter binding, `LIKE` escaping, page-size cap and role gate are implemented and covered (TC-107, TC-108, TC-111, TC-114) |
| Accessibility (WCAG 2.2 AA) captured | pass — captured in 002_BD/002_DD and verified by axe in jsdom and in a real browser; the two violations found there were fixed, not waived |
| Migration impact described | pass — 002_DB specifies both migrations, their non-atomic `CONCURRENTLY` behavior and the INVALID-index recovery; both were applied and verified locally |
| Tracing/logging for a new endpoint specified | pass — 002_DD-FN specifies the span `production_orders.list`, the counter `pmai.production_orders.listed` and the histogram `pmai.production_orders.list_result_size`, all implemented on the existing `ActivitySource`/`Meter` |

### security-review checklist walk (2026-09-22)

| Checklist item | Result |
| --- | --- |
| Every new/changed endpoint enforces the agreed auth rule | pass — `GET /api/production-orders` inherits the controller's `[Authorize(Policy = ProductionOrderEditor)]`; 401 and 403 are asserted by TC-114. The client-side role gate is UX only |
| All external input validated; no query built by concatenation | pass — every query parameter is bound as a string and parsed by `ProductionOrderListQuery.TryCreate`; sort key, direction and page size are allow-listed, so no client string reaches the SQL; filters are bound parameters through EF Core. `%`, `_` and `\` in the order-number fragment are escaped and the match uses `LIKE … ESCAPE` (TC-108) |
| No credential, token or key in code, config, logs, evidence or diff | pass — `deploy/.env` is gitignored and untracked; the local passwords were read from it at run time and never written into a file or this record; the diff was checked for the literal values |
| New/updated dependencies from a trusted source, checked | pass — no new dependency. The change uses `pg_trgm`, an extension shipped with the PostgreSQL image, and existing packages only |
| New credentials, DB roles or permissions request no more access than needed | pass — no new role or grant. `pmai_app` already had `SELECT` on both tables and gains nothing; the trigram operator class needs no grant. The migrations run as the owner, as all migrations do |
| External content treated as data, not instructions | pass — the only external input is the query string, parsed into a typed object before it reaches any query |
| Error responses don't leak stack traces or internal detail | pass — RFC 9457 bodies carry `code`, `errors` (message IDs) and `traceId` only; TC-107 asserts no `detail` and no driver text in the body |
| Sensitive data not written to logs or evidence | pass — the span records the *shape* of the filter (which filters are set, counts) and never the order-number fragment, which is user-supplied text; rejected queries log parameter names and message IDs as structured fields |
| New/changed trust boundary threat-modeled | not applicable — no new trust boundary: the endpoint sits behind the existing cookie session and role policy, reads data those users can already read through Screen A, and writes nothing |

Two points worth stating plainly rather than burying: the endpoint deliberately **rejects** an out-of-range `page` or
`pageSize` instead of clamping it, so a crafted request cannot quietly widen the result set; and the enum allow-lists
match **by name only** — `Enum.TryParse` would also have accepted `status=3`, which a unit test caught before it could
reach the API.

### delivery checklist walk (2026-09-22)

| Checklist item | Result |
| --- | --- |
| Approved scope and plan revision identifiable | pass — plan revision 2, approved for its local scope by "move on to the implementation" and for step 14 by "yes push it and open the PR"; revision 1 is closed above it |
| Design, code and tests agree with requirements | pass — see the traceability matrix; every REQ has passing cases at the levels TP-003 lists |
| Required checks have recorded results | pass — build, lint, unit, integration, E2E and the migration run are recorded above with counts; CI is recorded as not run, with its reason |
| Review findings and remaining limitations explicit | pass — see "Remaining limitations" below and the defect table |
| External operations stayed within authorization | pass — branch, worktree, local commits, the local Compose stack and a local migration; then the push and PR #9, each authorized by the user at execution. No merge, no deployment |
| Status, decisions and evidence support another agent continuing | pass — `status.md` names the branch and worktree; `decisions.md` carries DEC-001–DEC-012; this file carries the commands and results |
| No secret in diff, evidence, status, decisions or PR description | pass — checked; `deploy/.env` is untracked |
| External content treated as data | pass — nothing outside the repository and the local stack's own responses was consumed |
| Flaky or skipped checks quarantined and recorded | pass — one test proved timing-dependent in CI (TC-109) and was fixed at the cause rather than retried or quarantined: it now waits for `aria-sort`, which is rendered from the response, instead of for the URL. Playwright still runs with `retries: 0` |
| New third-party CI action or dependency pinned and least-privilege | not applicable — no CI or dependency change |

## Defects, failures and blockers

Every item below was found by a check in this work item and fixed before it left the branch; none is outstanding.

| Item | Reason | Blocker | Follow-up |
| --- | --- | --- | --- |
| Message-ID collision (found by the 002_DD design-consistency walk) | 002_BD v1 drafted MSG-E011–MSG-E016 and MSG-I001–MSG-I002, but 001_DD already defines MSG-E001–MSG-E014 and MSG-I001–MSG-I002 and ships them in `messages.ts`. Screen B would have redefined Screen A's messages | no | Fixed before any code: Screen B uses MSG-E015–MSG-E020 and MSG-I003–MSG-I004, reusing MSG-E002 and MSG-E013 (002_BD revision 3) |
| Numeric enum aliases accepted (found by a unit test) | `Enum.TryParse` accepts `"3"` for `Cancelled` and `"1"` for a sort key, so a crafted request could slip past a list meant to contain names | no | Fixed: `TryParseName` matches `Enum.GetNames` only; TC-104 and TC-107 cover it |
| EF Core could not translate the ordering (found by an integration test) | Sorting after projecting into a positional record made EF try to read a member out of a constructor call; the endpoint returned 500 | no | Fixed: the sort is applied to the joined entities before the final projection; 002_DD-FN §3/§5 updated |
| Two axe violations on the real screen (found by E2E) | The `Cancelled` badge used `text-gray-400` (≈2.9:1 on white, fails WCAG 1.4.3); the breadcrumb link was underlined only on hover (fails 1.4.1, "link in text block") | no | Fixed in `ProductionOrderTable` and `ProductionOrderListPage`; 002_DD records both. Not waived |
| Seed had no shared due dates (found by an integration test) | Every seeded order had a distinct due date, so the paging tie-breaker was never exercised | no | Fixed: 25 of the 80 orders now share a due date with another (55 distinct). TC-110 asserts it |
| Screen A's numbering test asserted `00001` | The demo seed also runs in the integration-test database, so API-created orders start at `00081` | no | The test now asserts the sequence continues from the seeded counter — a stronger assertion, since that is exactly what the seed's counter row exists to guarantee. Recorded as DEC-012 |
| TC-109 was timing-dependent (found by CI run 2) | The test read the table right after `toHaveURL`, but the URL changes before the query's response renders, so it could compare the previous page's rows. It passed locally and in CI run 1 by luck | no | Fixed at the cause: the test now waits for `aria-sort` on the sorted column, which is rendered from the response. Verified by three consecutive local runs of the spec plus a full-suite run. The same pattern was corrected in the filter journey. Not retried, not quarantined |

## External references

- PR: https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/9 — opened and, on 2026-09-22, squash-merged by the user as `8eab65f`
- CI runs on PR #9: run 1 green, run 2 red on a timing-dependent test of mine, run 3 green after fixing it at the
  cause — https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/35685579998. The commit carrying
  this record re-runs the same suite; its result is the one the PR shows
- Deployment: not applicable — not in scope

## Remaining limitations and next action

- **Merged.** The user squash-merged PR #9 as `8eab65f`; the branch, its worktree and the local Compose stack
  (containers, networks and both volumes) are removed. No environment from this work item is still running.
- **A read-only role cannot be expressed.** The list endpoint reuses the `ProductionOrderEditor` policy, so a viewer
  who may read but not edit is not representable. This waits on WI-001 DEC-015 (the role/permission matrix).
- **The index assertions prove usability, not planner choice.** At 80 rows PostgreSQL correctly prefers a sequential
  scan; TC-119 disables `enable_seqscan` to assert the indexes can serve the queries. Behavior at production volume is
  untested, as 002_DB's performance expectations state.
- **The demo seed reaches every database the migrations run against**, including the integration-test container. That
  is deliberate (DEC-012), but a future test that needs an empty `production_orders` table has to arrange it.
- The local Compose stack and its `db` volume are still running from the worktree; remove them with
  `docker compose -f deploy/compose.yaml down -v` when they are no longer needed.

Next action: the user reviews and merges PR #9 (squash merge, per `ai/rules/git-review.md`). After the merge, the
worktree and the local Compose stack can be removed.
