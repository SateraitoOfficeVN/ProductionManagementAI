# WI-009 — Test plan

## Test plan identifier

TP-WI-009, revision 1, 2026-10-01. Planning only; every runtime case is not run.
IDs TC-326–TC-365 follow the existing repository maximum TC-325.

## References and scope

[Brief](brief.md), [decisions](decisions.md), [plan](plan.md), and the approved
[005_REQ](../../docs/en/000_requirements/005/005_REQ_production-lines.md),
[BD](../../docs/en/010_basic-design/005/005_BD_生産ライン・工程.md),
[DB](../../docs/en/database/005/005_DB_生産ライン・工程.md),
[main DD](../../docs/en/020_detailed-design/005/005_DD_生産ライン・工程.md),
[API version 2](../../docs/en/020_detailed-design/005/005_DD-API_生産ライン・工程.md),
[FN](../../docs/en/020_detailed-design/005/005_DD-FN_生産ライン・工程.md),
[SPD](../../docs/en/020_detailed-design/005/005_DD-SPD_生産ライン・工程.md),
[ORD](../../docs/en/020_detailed-design/005/005_DD-ORD_production-line-assignment.md).
All other versions are 1. REQ-064 list; REQ-065 identity; REQ-066 timings/units;
REQ-067 retirement/history; REQ-068 assignment; REQ-069 security/accessibility.

## Approach and feature boundaries

Twenty unit groups (nine backend, eleven frontend) check rules, exact text and
state handling; fourteen integration groups check PostgreSQL/API semantics;
five E2E groups check composed user behavior; one system group records full
regression. Parameterized subcases may yield more executable tests than these
40 scenario groups; no test count is predicted as a passing result.
No scheduling, capacity reservation, BOM, calendar, historical duration or new
roles/provider, live/demo data migration, deployment or video production.
Product/order/dashboard regression stays in scope because their contracts are
constraints on this feature. No test exists or ran yet for these new IDs.

## Cases

| ID | Requirement | Level | Setup | Steps | Expected result | Priority |
| --- | --- | --- | --- | --- | --- | --- |
| TC-326 | REQ-064 | Backend unit | List/choice query input | Validate trim, literal %, _, backslash, repeated/unknown keys and page/state bounds | Canonical safe query or stable validation; no SQL interpolation | High |
| TC-327 | REQ-065 | Backend unit | New/edit line identity | Exercise empty/Unicode limits, trimming and immutable code payload | Code/name bounds enforced; code only set at creation | High |
| TC-328 | REQ-066 | Backend unit | Hours/minutes text | Parameterize 0, negative, 24/24.001, .001, max minutes, extra precision, exponent, leading zeros, NaN/infinity | Exact approved syntax and ranges; no rounding or binary conversion | High |
| TC-329 | REQ-066 | Backend unit | New/fresh/stale saved pairs | Validate add/setTiming/retire, duplicates, unit intent and retired rows | One action/product; correct confirmation requirements; no restore | High |
| TC-330 | REQ-067 | Backend unit | Aggregate with early valid/later invalid action | Validate all mutations before persistence | No partial action plan or commit; field indices retained | High |
| TC-331 | REQ-068 | Backend unit | Origin Draft or non-Draft; omitted/null/UUID | Parameterize presence and status/history rules including Cancelled | Draft optional; start required; outside-Draft lock; real legacy null retained | High |
| TC-332 | REQ-068 | Backend unit | Changed Draft product and retained incompatible UUID | Evaluate resulting product/line and start eligibility decision | Clear allowed; mismatched assignment rejected; no substitution | High |
| TC-333 | REQ-069 | Backend unit | Commit outcome and provider classifications | Simulate known rollback, in-flight commit loss, postcommit read failure and cancelled cleanup | 503 only with rollback proof; unknown never replayed; cleanup bounded | High |
| TC-334 | REQ-069 | Backend unit | Telemetry completion and error mapper | Exercise operation/outcome and safe field paths | One bounded completion; generic errors; no values in labels or leaked detail | High |
| TC-335 | REQ-064 | Frontend unit | List draft filters and router history | Apply/Clear/page/back with late read responses | URL defaults active/page1; only latest results; no fake empty on failure | High |
| TC-336 | REQ-065 | Frontend unit | Create/edit line draft | Validate immutable edit code and Unicode fields | Locked code retained; field error focus and all values preserved | High |
| TC-337 | REQ-066 | Frontend unit | Decimal input and opaque revisions | Enter precision cases and revisions beyond JS safe integer | Exact strings retained; no Number conversion, silent rounding or revision increment | High |
| TC-338 | REQ-066 | Frontend unit | New/fresh/stale/changed-back timing rows | Confirm displayed unit/revision then change coefficient/observation | Explicit intent required; ABA stale; later changes clear intent | High |
| TC-339 | REQ-066 | Frontend unit | Multiple pair pages and >1000 action ledger | Switch pages and map submitted indexed error; change parent version | Off-page intents/errors retained; stale page guarded; action/body caps explicit | High |
| TC-340 | REQ-067 | Frontend unit | Saved pair / unsaved row | Cancel/confirm pair retirement and remove unsaved row | Saved retirement staged until Save; unsaved removal local; Cancel no request | High |
| TC-341 | REQ-069 | Frontend unit | Dirty line form and conflict/unknown response | Attempt Save/navigation/reload during pending, conflict and uncertain outcome | Draft retained; guarded exits; no automatic write replay; known rejection editable | High |
| TC-342 | REQ-068 | Frontend unit | Draft order picker with current historical line | Change q/page/product, select/clear and deliver late old-product response | Selection independent of page; product change clears; no restore by late result | High |
| TC-343 | REQ-068 | Frontend unit | Origin Draft/null or InProgress/Completed/Cancelled | Request start; edit otherwise permitted fields | Required line field focus; non-Draft read-only; legacy null saves; target status does not lock Draft early | High |
| TC-344 | REQ-069 | Frontend unit | Order errors and response transport | Map LINE_* and unknown field errors, save/create success and quantity tokens | Existing messages/number-token quantity/version/routes preserved; lineId carried distinctly | High |
| TC-345 | REQ-068 | Frontend unit | Order table/cards line projection | Render active/retired/real null and invalid missing line property | Non-sortable line; current names; null only when real; URL/counts/units unchanged | High |
| TC-346 | REQ-069 | Integration | Anonymous, Admin, Operator and authenticated other role | Exercise all seven feature endpoints and existing order extension | 401/403 empty body; Admin/Operator granted; denied request makes no DB write | High |
| TC-347 | REQ-069 | Integration | New feature request boundary | Send unknown/duplicate fields, scalar/null mismatch, oversized/non-JSON body and wildcard/injection text | 400/413/415; no leaked exception; safe parameters; no-store; cross-origin form cannot mutate | High |
| TC-348 | REQ-065 | Integration | Concurrent creates and retired code | Submit trimmed/mixed-case same code; stale parent edits | Unique-index conflict deterministic; no retired-code reuse or stale overwrite | High |
| TC-349 | REQ-066 | Integration | Real PostgreSQL exact numeric columns | Attempt API and direct owner precision/range/special numeric writes | Checks reject bad values without typmod rounding; accepted decimals exact | High |
| TC-350 | REQ-066 | Integration | Product no orders with configured pair | Change unit A→B→A, name-only and unchanged-unit update; reconfirm | Trigger advances only actual unit change; ABA remains stale; reconfirm binds locked current revision | High |
| TC-351 | REQ-066 | Integration | Aggregate pair edits under concurrency | Submit pair-only edit with same clock and an invalid later action | Parent xmin advances; stale competing edit rejected; invalid action rolls back all | High |
| TC-352 | REQ-067 | Integration | Assigned line/pair and retired product | Retire then save unchanged history, attempt new selection and read line display | References survive; no new retired selection; no hard deletion; current names shown | High |
| TC-353 | REQ-068 | Integration | Order endpoint presence/status matrix | Create/update with omitted/null/non-null, start Draft and preserve old non-Draft null | Exact API presence/history/start rules; existing order codes/status/quantity guards intact | High |
| TC-354 | REQ-068 | Integration | Two independent DB connections and barriers | Race retirement versus select/start; timing/unit change versus eligibility/reconfirm; competing order status/xmin | Locks serialize eligibility through commit; loser conflicts/rejects; no committed invalid assignment | High |
| TC-355 | REQ-069 | Integration | Held row lock and cancelled write/read | Force timeout/known rollback and observe retry/error/disposal | 5s lock/10s statement/15s use-case budgets; no deadline reset; context/connection safe; real span/metric completion captured once with redacted attributes; no implicit replay | High |
| TC-356 | REQ-064 | Integration | Read with concurrent writer | Interleave count/rows/details/choice observations in one result | REPEATABLE READ snapshot coherent; stable ordering/paging; no N+1 or invented integrity fallback | High |
| TC-357 | REQ-069 | Integration | Runtime pmai_app versus owner | Read/write allowed fields; attempt code/key/delete/DDL/direct revision update | Approved column grants only; runtime cannot migrate/delete/rewrite immutable keys or revision | High |
| TC-358 | REQ-068 | Integration | Fresh and baseline upgrade in disposable PostgreSQL | Apply expand/index stages, inspect catalog/grants/trigger and legacy rows | Valid/ready partial index and FK/checks; old orders unchanged with null; no redundant EF index | High |
| TC-359 | REQ-069 | Integration | Disposable schema with interrupted index / used master data | Inspect failed concurrent index repair and invoke unsafe Down guard | Invalid index not treated as success; bounded owner recovery; destructive rollback rejected after use | High |
| TC-360 | REQ-064, REQ-065, REQ-066 | E2E | Admin/Operator in isolated app data | Create line/pair, find/edit, verify unit labels and success | Japanese lifecycle works with exact units; server state matches visible result | High |
| TC-361 | REQ-067 | E2E | Line/pair dialogs with scrolled desktop/mobile page | Open/cancel/Escape/confirm then Save staged pair | Dialogs viewport-centered, safe/return focus, inert background; Cancel no write, pair save atomic | High |
| TC-362 | REQ-068 | E2E | Draft order with eligible/retired line and legacy non-Draft null | Choose/change product/start/save and read list/cards | Start requires eligible line; historical assignment/null retained and locked correctly | High |
| TC-363 | REQ-069 | E2E | 320px/390px, desktop, 200% zoom, keyboard and axe | Navigate master/order routes, field errors, paging and confirmations | Reachable controls, no page overflow, WCAG automated checks plus manual keyboard/focus/geometry | High |
| TC-364 | REQ-069 | E2E | Intercepted delayed/lost response and dirty forms | Deliver stale read, simulate lost mutation response, try navigation/resave | No stale overwrite, draft loss or automatic replay; explicit verification path visible | High |
| TC-365 | REQ-064, REQ-065, REQ-066, REQ-067, REQ-068, REQ-069 | System regression | Clean isolated environment | Run complete backend/frontend/E2E suites plus Product master/order/dashboard smoke | Existing behavior preserved; no flaky retry hidden; all required cases have actual evidence | High |

## Item pass/fail and exit criteria

A case passes only when every parameterized expected result is observed and
linked to command output/artifacts in evidence.md. All 40 groups are required;
no fabricated coverage percentage or elapsed-performance threshold. Exact
contract deadlines and request limits come from API/FN. Critical integrity,
permission, migration or unknown-outcome failures block delivery. Existing
regression suites must pass. Review application design/security and delivery
checklists after fixes. Axe alone does not prove WCAG conformance.

## Environment, commands and isolation

Use existing xUnit/WebApplicationFactory/Testcontainers.PostgreSql with
PostgreSQL 17 for integration; owner only migrates fixtures, pmai_app runs app
queries. Independent connections and explicit synchronization barriers drive
race tests, not arbitrary sleeps. Fresh/upgrade/failed-index fixtures use uniquely
named disposable containers/databases; never main/demo services, data or volumes.
Each fixture resets its own test data; tests may not depend on order of execution.
Use existing Vitest/RTL/unit axe, Playwright and E2E axe. New source paths remain
in tests/backend, tests/integration, src/frontend/tests/unit and tests/e2e.
No additional framework/dependency or real secret in test artifacts.

Commands proposed for approved implementation, not executed in revision 1:

```powershell
dotnet build src/backend/ProductionManagementAI.slnx
dotnet test src/backend/ProductionManagementAI.slnx
npm --prefix src/frontend run lint
npm --prefix src/frontend run build
npm --prefix src/frontend test
```

For E2E, run `npx playwright test` from tests/e2e against an isolated Compose
project with dynamically selected non-conflicting ports and isolated data.
E2E_BASE_URL and E2E_ADMIN_PASSWORD are supplied in the process environment;
never write credential values to records. Generate the exact Compose override
and owner migration command from the isolated fixture topology during revision 2;
record redacted commands after execution. Existing lockfiles/package restore
only if needed; no dependency upgrade. No generic migration command may point to
live/demo DB. Migration tests inspect pg_index.indisvalid/indisready, pg_constraint
validation state, grant/trigger definitions and unchanged legacy rows.

## Suspension, schedule and responsibilities

The agent implements cases during revision 2 after explicit approval; user is
business owner and plan approver. Required tool/service absence is not run/blocked
with its cause; resume after environment is available. A contract mismatch or new
business choice pauses dependent work for a new approved plan/design addendum.
Quarantine flakiness immediately with cause; fix before delivery rather than
retrying until green or silently skipping. Never weaken a gate to pass.

## Deliverables, results and known gaps

Deliver this plan, implementation test sources, redacted execution logs,
requirement-to-result evidence, and review records. All TC-326–TC-365 are currently
**not run**: application/test/migration implementation is outside approved
revision 1 and awaits revision 2. Static document/mockup checks previously run
are listed in evidence.md; they are not passes for these runtime cases.
Known gap: runtime behavior, SQL races/grants, fresh/upgrade recovery,
telemetry, actual keyboard/focus/zoom and security rejection are unverified.
Revision 2 must supply those results before application delivery. Live release
smoke is not applicable without separately authorized deployment.
Approvals: the approved ORD authorizes drafting this plan. Executing it requires
explicit approval of implementation plan revision 2; final acceptance uses actual
results, not approval of this document alone.


## Revision 2 execution results — 2026-10-01

The planning-only statements above describe revision 1 at authoring time.
This executed disposition supersedes those not-run statuses. Evidence combines
appropriate unit/integration/E2E levels where the same business behavior does
not need duplicate tests. Detailed failures, fixes, commands and isolation are
recorded in [evidence.md](evidence.md). No live release verification is claimed.

| Case group | Disposition | Actual supporting verification |
| --- | --- | --- |
| TC-326 | Pass | LineValidationTests and strict endpoint queries; literal SQL LIKE escaping, fixed pages and snapshot list verification. |
| TC-327 | Pass | LineValueRulesTests Unicode/trim bounds; strict immutable payload boundary and real code uniqueness; client Unicode focus test. |
| TC-328 | Pass | Parameterized LineValueRulesTests and lineRules.test.ts; direct hours/minutes numeric constraints preserve exact values. |
| TC-329 | Pass | LineValidationTests actions/duplicates/booleans; aggregate endpoint retirement/re-add and ABA confirmation cases. |
| TC-330 | Pass | AtomicChangesPreserveOmittedPairsAndRetiredPairsCannotBeReadded observes unchanged aggregate after a later invalid intent. |
| TC-331 | Pass | Domain non-Draft matrix (InProgress/Completed/Cancelled), order service start rejection and HTTP omitted/null/presence/history cases. |
| TC-332 | Pass | DraftProductChangeRejectsRetainedIncompatibleLineAndWrongTypedLineIsInvalid; resulting-product server eligibility plus picker product clearing. |
| TC-333 | Pass | LineWriteOutcomeTests precommit/commit/rollback matrix, cancelled fresh cleanup, postcommit failure and unchanged shared deadline token. |
| TC-334 | Pass | EachHttpOutcomeRecordsOneCounterAndDurationWithBoundedTagsAndRedactedSpans; safe problem responses and reviewed bounded span events. |
| TC-335 | Pass | LinePages URL/default/late-read/failure tests and browser Back/Forward applied-filter history journey. |
| TC-336 | Pass | Immutable edit field, retained error values and oversized Unicode field focus observed by LinePages tests. |
| TC-337 | Pass | Exact decimals and bigint revision strings in lineRules/LinePages; exact request body and invalid response decoder tests. |
| TC-338 | Pass | Coefficient change/back clears confirmation; revisiting a page refreshes unit/revision and clears intent; server ABA/reconfirmation tested. |
| TC-339 | Pass | Off-page intent/indexed error focus and parent-version conflict tests; aggregate 1000/1001 and exact 256-KiB UTF-8 boundaries. |
| TC-340 | Pass | Unsaved removal sends no mutation; saved pair Cancel/Escape preserve state and confirmed retirement persists only on Save in Playwright. |
| TC-341 | Pass | Pending repeated submit/navigation suppressed; validation retains editability; conflict/lost responses retain draft and block replay; explicit guarded reload. |
| TC-342 | Pass | OrderLinePicker tests historic selection independent from filters, obsolete product response suppression, pending/forbidden behavior; order product clearing. |
| TC-343 | Pass | Existing order form terminal/non-Draft locks and legacy null HTTP saves; Draft start field focus and selection-before-start in Playwright. |
| TC-344 | Pass | Order error mapping/guarded reload regression, malformed assignment response rejection and quantityCodec number-token serializer test. |
| TC-345 | Pass | OrderLineProjection tests distinguish actual null/retired/malformed metadata; desktop non-sortable column, current history and existing mobile cards verified. |
| TC-346 | Pass | AllSevenOperationsDenyAnonymousAndUnprivilegedUsersWithNoStore (seven routes); Admin/Operator real maintenance and reads; denied UI roles. |
| TC-347 | Pass | StrictMutationBody parameterized cases plus query/route, duplicate/unknown/scalar, media and 256-KiB limits; safe error-field mapping. |
| TC-348 | Pass | Real aggregate endpoint tests and synchronized equivalent-code race: exactly one create winner, one conflict; stale expected version rejected. |
| TC-349 | Pass | Owner numeric constraint tests reject precision/range/special values in both numeric columns; exact accepted timing ceiling and API strings. |
| TC-350 | Pass | Runtime trigger ABA/name-only/unchanged-unit test; stale eligibility and explicit fresh reconfirmation; fresh observations after a real lock wait. |
| TC-351 | Pass | Pinned-clock pair-only update advances parent xmin; original timestamp unchanged; competing stale edit rejected; atomic invalid later action rollback. |
| TC-352 | Pass | HistoricalRetiredPairMayBeKeptButDraftStartRechecksAndLegacyNullSurvives plus pair retirement/re-add tests and current line history display. |
| TC-353 | Pass | OrderLineEndpointTests presence, clearing, start, product-change, outside-Draft/history and list projection plus original order regressions. |
| TC-354 | Pass | Independent-connection barriers cover duplicate code, retire-before-start, start-before-retire, unit/timing observation wait, pair retirement and order status/xmin winner. |
| TC-355 | Pass | Actual 5s lock timeout and 10s statement timeout; actual HTTP cancellation releases wait; fresh cleanup, single 15s deadline/shared postcommit budget, bounded telemetry. |
| TC-356 | Pass | List, detail/pair, choice and eligible observation snapshots interleave independent commits; 51-row stable pages/no overlap/beyond-total; constant list query count. |
| TC-357 | Pass | Restricted pmai_app real aggregate writes and generated revisions; denied immutable code/pair key, direct revision, DELETE and DDL attempts. |
| TC-358 | Pass | Fresh migration fixtures and baseline upgrade: unchanged legacy checksum/null lines, valid/ready partial index and validated constraints/grants/trigger. |
| TC-359 | Pass | Actual failed concurrent index rejected, explicit isolated owner repair rehearsed, current definition checked and unsafe Down rejected after master use. |
| TC-360 | Pass | Japanese line create/search/edit lifecycle with exact 0.125 timing and persisted unit confirmation in composed app. |
| TC-361 | Pass | Scrolled desktop/mobile modal geometry, safe focus, Tab containment, Escape/restoration and staged pair retirement verified. |
| TC-362 | Pass | Draft required/eligible start, locked assignment, retired historic detail/list, legacy null server path and existing mobile order regressions. |
| TC-363 | Pass | Axe plus keyboard/geometry at 320/390/640/1280; 320px timing editor/error focus/discard; CSS zoom and separately inspected native browser 200% zoom. |
| TC-364 | Pass | Late list/picker reads, dirty browser Back, committed create response loss and committed retirement response loss: no replay, readable verification and retained drafts. |
| TC-365 | Pass | Final full frozen-source build/test runs: 192 backend unit, 157 integration, 196 frontend and 42 E2E; zero skips/retries; existing product/order/dashboard journeys included. |

All required groups have supporting executed results. Automated axe and the
recorded visual/keyboard checks do not establish exhaustive WCAG conformance.
OTel was captured in-process; external OTLP collector export was not configured
or claimed. Real lost responses and deterministic port fault injection verify
unknown outcomes; no live network outage or deployment rehearsal is claimed.
