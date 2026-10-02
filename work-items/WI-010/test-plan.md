# WI-010 — Test plan

## Test plan identifier

TP-WI-010, revision 1, execution update2026-10-02 under approved implementation plan 2.
TC-366–407 follow the audited repository maximum TC-365. Initial Not run status
belonged to design planning; current executed coverage and limitations follow below.

## References and introduction

[Brief](brief.md), [decisions](decisions.md), [plan](plan.md), [review](review.md),
[requirements](../../docs/en/000_requirements/006/006_REQ_plant-calendar.md),
[BD](../../docs/en/010_basic-design/006/006_BD_稼働カレンダー.md),
[DB](../../docs/en/database/006/006_DB_稼働カレンダー.md),
[main DD](../../docs/en/020_detailed-design/006/006_DD_稼働カレンダー.md),
[API](../../docs/en/020_detailed-design/006/006_DD-API_稼働カレンダー.md),
[FN](../../docs/en/020_detailed-design/006/006_DD-FN_稼働カレンダー.md) and
[SPD](../../docs/en/020_detailed-design/006/006_DD-SPD_稼働カレンダー.md), all version 1
and explicitly approved. This plan verifies SCR-006 lookup and effective rules,
retained transactions and exact current capacity, together with unchanged existing
order/master/dashboard behavior. Earlier static document arithmetic and mockup
rendering are not results of these application cases.

## Test items and features

| Requirement | Tested feature / cases |
| --- | --- |
| REQ-070 | Date/month/activation, applied URL, snapshot reads and stale-result guards: TC-366/375/378–380/394/397/402/405/406 |
| REQ-071 | Weekdays, retained starts/withdrawal/re-add, targets/no-op/conflicts: TC-369/372/376/377/381/391/393/395/398/402 |
| REQ-072 | Exact exception hours/reason/state, removal/history and protected past: TC-367/368/370–372/376/382/392/393/396/398/403 |
| REQ-073 | Whole-winner hours, eligible current references, exact floor and unavailable vs zero: TC-370/371/373–375/387/388/394/404 |
| REQ-074 | All roles, strict inputs, uncertainty, deadlines, telemetry and accessibility: TC-368/377/379–386/389–401/405/406 |
| REQ-075 | Calendar-independent order due dates/starts/dashboard, master/history integrity: TC-397/407 |

Excluded: holiday-provider integration, working-day arithmetic, scheduling, shifts,
BOM, historical capacity, load allocation, live cutover/deployment, external OTLP
export and video recording. Existing features are included as regression coverage,
not new functionality. No CI/package/framework change is assumed.

## Approach and pass/fail criteria

| Level | Approach / ownership |
| --- | --- |
| Backend unit | xUnit in tests/backend/ProductionManagementAI.Application.Tests/PlantCalendar; pure resolver/values and typed service outcomes |
| Frontend unit | Vitest/RTL in src/frontend/tests/unit/plant-calendar; deferred promises, response loss and isolated typed state/formatting |
| Integration | WebApplicationFactory/Testcontainers PostgreSQL 17 in tests/integration/ProductionManagementAI.Integration.Tests/PlantCalendar; real restricted DB login, transactions, locks and HTTP contracts |
| E2E | Playwright/axe in tests/e2e/specs/plant-calendar.spec.ts and plant-calendar.mobile.spec.ts; small complete Japanese UI journeys |
| System / migration rehearsal | Fresh and baseline-upgrade throwaway databases plus isolated Compose project; old data checksum and full regression suites |
| Manual accessibility | Native browser 200% zoom, keyboard/screen-reader spot checks and mobile keyboard; supplements automated checks, does not duplicate data-rule unit cases |

A case passes only when its observable expectation is met by the actual changed
application under the stated setup, with deterministic execution and no hidden
retry/skip. No arbitrary coverage/performance threshold introduced: correctness,
bounded query shape and the approved timeout/size limits are the required gates.
Most cases are unit-level; integration is reserved for storage/HTTP/races, E2E for
complete workflows. Cases may have several parameterized assertions without
repeating equivalent journeys at every level.

## Cases

Each row initially has result Not run. B = backend unit; F = frontend unit;
I = integration; E = E2E; S = system. High priority except where marked Medium.
Test ID describes a scenario group; later evidence links exact test names/results.

| ID | Requirement | Level | Preconditions / steps | Observable expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| TC-366 | REQ-070/074 | B | Parse Gregorian dates/months, leap/nonleap February, year 0001/9999, impossible forms and browser-independent strings | Exact supported dates; invalid shapes/ranges rejected without overflow or timezone conversion | High |
| TC-367 | REQ-072/074 | B | Validate string hours at 0/.001/24/24.001, 3/4 fractional digits, signs/exponents/grouping and trim | >0..24, scale<=3 only; no rounding/coercion; exact normalized text | High |
| TC-368 | REQ-072/074 | B/F | Reason with astral Unicode, 500/501 code points, outer/internal whitespace, malformed surrogates; serialize escaped/multibyte 8 KiB body | Correct trim/null/code-point/UTF-8 semantics; invalid text and oversized payload rejected | High |
| TC-369 | REQ-071 | B | Map all seven weekday tokens, [] and duplicates; Monday/Sunday boundaries | Exact 0..127 mask and ordered response; [] valid, duplicates/numeric enums invalid | High |
| TC-370 | REQ-072/073 | B | Resolve plant closure plus working line exception and reverse closure; inspect fallback | Line > plant > weekly whole winner; reopen/close correct; max two ordered fallback sources | High |
| TC-371 | REQ-072/073 | B | Line winner inherits while lower plant winner has explicit hours; plant-only working; valid closed day | Current line baseline, no partial merge; plant hours null/LineDependent; closed hours 0 | High |
| TC-372 | REQ-071/072 | B | Latest same-date head is marker; earlier payload and predecessor weekly start exist | Removed/withdrawn head never resurrects older same-target payload; live predecessor wins | High |
| TC-373 | REQ-073 | B | Run seven FN floor vectors, closed zero, all discrete units and kg/m near quantum; invalid persisted values | Integer exact floor, no upward rounding, canonical strings; invariant violations not clamped | High |
| TC-374 | REQ-073 | B | Combine activation/line/product/pair retirement/missing/unit and generation ABA errors | Exact API unavailable priority; invalid closed pair unavailable; quantity/coefficient null | High |
| TC-375 | REQ-070/073 | B | Missing activation, preactivation date, past inherited day and past capacity query | Unavailable/no invented coverage; past source readable/current-hour disclaimer; past capacity rejected | High |
| TC-376 | REQ-071/072/074 | B | Same normalized payload with matching/mismatching version or target; new/null and marker re-add intent | No-op only after authoritative checks, no revision/time/snapshot advance; creation and re-add append | High |
| TC-377 | REQ-071/074 | B/F | Parse bigint token bounds/invalid forms; return values above JS safe integer and overflow state | Opaque exact string preserved; invalid syntax/overflow rejected atomically, never wrap/reset | High |
| TC-378 | REQ-070 | B | Month on final supported year, 28–31 days, several weekly changes and plant/line markers | Exactly real sorted dates, no AddDays overflow, batched heads/resolution; no adjacent dates | High |
| TC-379 | REQ-070/074 | F | Deep-link valid/malformed/repeated/unknown URL keys; absent month/server date; explicit mismatched month/date | Correct applied context or honest reset error, no browser-clock default or hidden invalid fetch | High |
| TC-380 | REQ-070/074 | F | Resolve obsolete month/day/choice requests after key change; different month/day revisions and explicit refresh | Late result ignored; no relabel/coherent-snapshot claim; bounded one refresh, no loop | High |
| TC-381 | REQ-071/074 | F | New today uses predecessor defaults but today's exact head ID; select retained row or new date with dirty input | Correct target/marker/version; retained payload read-only; unchanged existing Save disabled; guarded reinitialization | High |
| TC-382 | REQ-072/074 | F | Toggle Working/Closed/inherit/explicit and reason; submit invalid/valid input | Contradictory hidden hours cleared; required nullable fields sent; first linked error focused | High |
| TC-383 | REQ-074 | F | Map every error/unavailable code and unknown response code; reason resembling HTML | Central exact Japanese text; safe generic fallback, no arbitrary server HTML/input diagnostics | Medium |
| TC-384 | REQ-074 | F | Capture confirm action/target/invoker; cancel, stale context, discard navigation and pending clicks | No dismissal write, preserved draft and intended route; no wrong-target/duplicate submission | High |
| TC-385 | REQ-074 | B/F | Valid changed/no-op success, refresh failure, known rejection, malformed response, network/timeout/abort | Known success retained with separate read error; NotApplied allows explicit correction; uncertain becomes Unknown without replay | High |
| TC-386 | REQ-074 | F | Unknown with equal observed payload, failed verification, differing observations, explicit accept/discard/leave | Equality never success; failed read cannot enable Accept; draft preserved until explicit discard/new baseline | High |
| TC-387 | REQ-073 | F | Choices empty/no matches/error; changed line/product/date/search; late capacity; stale unit | Clear invalid old result, explicit selection/no silent first product, unavailable is not zero | High |
| TC-388 | REQ-073 | F | Format exact invariant response strings including .001, 8.571, 1440000 and discrete zero | Japanese display grouping/unit retains value, no Number-based transport/arithmetic or mixed totals | High |
| TC-389 | REQ-074 | I | Exercise all eleven endpoints as anonymous, Admin, Operator and another authenticated role with existing cookie posture | Empty 401/403; both authorized roles allowed; no target lookup leak or form/GET mutation bypass | High |
| TC-390 | REQ-074 | I | Duplicate/unknown query/JSON, missing/null/wrong fields, streamed 8 KiB boundary, depth, content type, enum/UUID/version/date/search/paging limits | Exact 400/413/415 and safe Problem Details; strict parser limited to calendar family; old endpoints unchanged | High |
| TC-391 | REQ-071/072/074 | I | Concurrent unrelated target saves with same global token, stale exact ID, forced zero affected/unique failure and no-op | One committed transition; other conflict; atomic rollback/no gap/partial history; token advanced once only for change | High |
| TC-392 | REQ-072/074 | I | Pause line exception during line-before-state lock and concurrent retirement; remove existing retired today/future exception | Real FOR SHARE conflicts with retirement, no reverse lock; create/edit rechecks active; allowed removal preserves history | High |
| TC-393 | REQ-071/072/074 | I | Hold state lock across fake plant midnight before validation; future-only withdraw, initial/current/past rule checks | Fresh date after lock wait rejects now-past action; initial withdrawal/past correction denied without mutation | High |
| TC-394 | REQ-070/073 | I | Interleave calendar and current master/unit change during multi-read response; inspect query count shape | Read-only REPEATABLE READ coherent values; no N+1/month per-day queries, own context; current unit generation respected | High |
| TC-395 | REQ-071/072/074 | I | Multiple rule/history and choice pages, markers/predecessor, page beyond total, mutation before page 2 | Exact totals/order/page sizes; token required for history page 2+; stale409, null plant scope handled safely | High |
| TC-396 | REQ-072/074 | I | Real pmai_app appends/supersedes; attempt protected payload/date/timezone/update, DELETE and DDL with that login | Allowed SELECT/INSERT/column UPDATE only; prohibited commands denied; default privileges do not broaden | High |
| TC-397 | REQ-070/075 | I/S | Fresh/42e8932 upgrade migration, missing activation, owner explicit date/timezone activation, rerun, timezone mismatch, attempted unsafe Down | Three additive tables; old checksums stable; first mask31/rev1 atomic; no silent reset; precoverage unavailable; Down refuses after activation | High |
| TC-398 | REQ-071/072 | I | Direct owner invalid masks/markers/null payload/hours scale/NaN/infinity/date bounds and duplicate null/line current heads | Named checks/FKs/unique indexes reject invalid rows; latest markers occupy logical uniqueness | High |
| TC-399 | REQ-074 | I | Fault before commit with acknowledged rollback, during issued COMMIT, after confirmed commit/response loss; interrupted cleanup | Honest NotApplied/BUSY versus Unknown; no success before acknowledgement or execution-strategy retry; cleanup bounded | High |
| TC-400 | REQ-074 | I | Controlled statement/lock deadline, cancellation and connection reuse in subsequent old-feature operation | Scoped 20s/15s/5s budgets and <=5s cleanup; no calendar settings leak; canceled reads never partial success | High |
| TC-401 | REQ-074 | I | Activity/Meter listeners for success, unavailable200, parser rejection,401/403,busy/unknown; inject distinctive sensitive values | Exactly one final count/duration per request, approved source/meter/span/labels, no raw query/notes/IDs/tokens/cookies/SQL in user/log diagnostics | High |
| TC-402 | REQ-070/071/074 | E | Admin/Operator Japanese weekly flow: new today/future, [] all closed, correction, future withdrawal, marker re-add and history | Persisted effective calendar and retained rows correct; input/errors/navigation honor server context | High |
| TC-403 | REQ-072/074 | E | Plant closure/line reopening, explicit/inherited hours, Cancel/Escape remove, confirmed removal and read-only past | Correct whole precedence/fallback, no canceled mutation, centered captured confirmation, history retained | High |
| TC-404 | REQ-073/074 | E | Query a valid current pair at4h/.125min, closed valid pair, missing/retired/stale-unit configuration | 1,920 個; valid zero distinct from unavailable; current-settings notice and unit preserved | High |
| TC-405 | REQ-070/074 | E | Delay read then change applied selection; drop committed save response, verify equal current state, discard/reload; dirty browser Back | No stale UI/replay/false success; input retained; intended navigation/focus correct; refresh-only after known success | High |
| TC-406 | REQ-074 | E/manual | PC/SP at320/390/640/1280px, scrolled native modal, Tab/Shift+Tab/Escape/focus return, errors, axe, native 200% zoom and mobile keyboard | No unintended horizontal loss, centered viewport dialog/inert background, usable Japanese controls/focus; actual manual limits recorded | High |
| TC-407 | REQ-075 | S | Full existing suites; create/start orders on newly closed days, verify calendar-day due checks/dashboard windows and product/line/history | Existing semantics/data unchanged; full builds/lint/unit/integration/E2E pass or explicit failures recorded | High |

## Environment, isolation and commands

Use locked existing packages/frameworks; npm ci in this worktree is planned only
under approved implementation revision. Docker/Testcontainers PostgreSQL 17 fixtures
use owner for DDL/explicit activation and pmai_app for application behavior. Inject
IPlantClock/TimeProvider with controlled dates; coordinate races with barriers rather
than fragile sleeps. No production or mutable existing demo database is a fixture.
Each case resets owned calendar data or uses a fresh fixture; order/master regression
fixtures retained. Simulated uncertain COMMIT uses test-only adapters/fault hooks,
never a production replay or result-forcing endpoint.

E2E uses a separately named Compose project/database/ports with generated temporary
credentials; SEED_ADMIN_PASSWORD and E2E_ADMIN_PASSWORD supplied via environment,
never artifacts or shell output. Owner selects explicit fixture activation date
from the test clock/configured timezone. No auto-seed in startup/read/migration.
Only verified task-owned resources may be removed after results are retained;
existing demo/evidence resources and final videos are preserved.

Commands executed under revision 2; exact results/affected retests in evidence.md:

```powershell
dotnet build src/backend/ProductionManagementAI.slnx
dotnet test src/backend/ProductionManagementAI.slnx
```

From src/frontend/: npm ci, npm run build, npm run lint, npm test.
From tests/e2e/: npm ci, npx playwright install chromium, npx playwright test
with E2E_BASE_URL and E2E_ADMIN_PASSWORD pointing only to the isolated fixture.
Targeted filters are selected after test names exist; record exact final commands,
fixture/commit, counts, failures/skips and report links in evidence.md. Isolated
migration/activation commands are produced and reviewed with their scripts in
revision 2; not a claim of executed DDL. No live migration command is authorized.

## Suspension, deliverables and responsibilities

Suspend dependent checks on missing Docker/package access, blocking semantic
failure, unexpected existing-data mutation or inability to establish write outcome.
Resume only with the actual prerequisite/fix; do not silently weaken assertions or
retry a flaky journey. Quarantine with cause and follow-up, then repair before gate.

Agent authors/executes/reviews implementation tests within approved revision 2;
user approves that revision and final feature delivery. No delegation authorized.
Deliverables: this plan; linked test code; actual unit/integration/E2E and manual
results; migration/grant/checksum evidence; design/security/delivery review records.
No new video outputs or testing report Markdown under docs/en/ are required here.

## Risks, contingencies and current results

| Risk | Contingency |
| --- | --- |
| Date/retirement race is flaky | Controlled clock/locks and synchronization barriers; no blind reruns |
| Commit acknowledgement cannot be fault-injected reliably | Separate typed phase unit tests and deterministic test-only transport/transaction fault boundary; record gap rather than infer rollback |
| Native zoom/screen-reader unavailable | Record unrun manual case and exact limitation; automated axe/static reflow is not a substitute |
| New runtime package/dependency required | Stop affected work and revise plan/design for approval; existing lockfile versions remain |
| Existing activation would be overwritten | Inspect and stop; no conflict-ignore/reset; isolated owner activation only |

| Scenario groups | Executed coverage / result | Remaining boundary |
| --- | --- | --- |
| TC-366–375/378 | Pass for executed pure47-case slice and real minimum/leap/maximum month API vectors | Not every permutation duplicated at each layer |
| TC-376/377/391 | Pass: no-op/target/global conflict/markers, opaque bounds and conditional-zero atomic rollback | Max persisted revision overflow now passes separate real DB test; forced unique-race fault not independently injected |
| TC-379/380 | Pass: strict URL values, deferred obsolete read cancellation, response identity rejection and scoped month refresh | Every malformed deep link / all month-day interleavings not exercised in E2E |
| TC-381/382 | Pass: exact baseline/marker ownership reviewed; linked field focus/hidden hours and weekly list-refresh draft preservation tested | Every retained row selection variant not covered by a separate unit test |
| TC-383/387/388 | Pass: centralized safe code mapping reviewed, exact formatting/unit tests, real current capacity and invalid closed pair journey | Every choice search/empty/late-capacity combination not independently exercised |
| TC-384–386 | Pass: dirty confirmation/Cancel/Escape, known save with failed refresh, committed response loss/Unknown/no replay, failed observation blocks accept; weekly Unknown survives list refresh | Dirty browser Back and every settlement combination not separately exercised |
| TC-389/390 | Pass: all11 operations for anonymous/other-role/Admin/Operator; strict query/JSON/media/size vectors | Streamed exactly8192/8193 now pass without Content-Length; every depth/header permutation not separately injected |
| TC-392–394/400 | Pass: real line retirement lock, retained removal, controlled fresh clock, coherent calendar snapshot, real5s BUSY rollback/pool reset | Concurrent master unit change, full20s cancellation and cleanup timeout fault not separately injected |
| TC-395/396 | Pass: retained ordering/current marker/predecessor/stale page validation and restricted grants | 23 retained history rows and52 choices now verify bounded non-overlapping pages; every forbidden column/DDL combination not independently exercised |
| TC-397/398 | Pass: fresh/nonempty baseline upgrade, old checksums, actual explicit SQL/wrapper activation, rerun rejection, numeric precision/null-scope uniqueness and protected Down | Every owner-invalid mask/NaN/infinity/FK/timezone combination not separately tested |
| TC-399/401 | Pass: before/after COMMIT fault certainty/no replay; real single count/duration for401/403/parser/success and sanitized spans | All telemetry outcomes/exporter and interrupted cleanup not separately tested |
| TC-402–405 | Pass for six actual desktop/mobile calendar journeys; behavior variants also checked at unit/integration layers | Every business/UI permutation is not an E2E journey |
| TC-406 | Partial: axe,320/390px mobile emulation, semantic desktop calendar/dialog keyboard and visually inspected native 200% zoom pass | Screen-reader speech and physical mobile keyboard/IME Not run; configured tool/device unavailable |
| TC-407 | Pass: full regressions and real closed-today due-today order creation/start with eligible line, past due still rejected | No live/demo data or deployment verification authorized |

Final executed suites:239 backend unit,191 integration,253 frontend,48 full E2E;
final affected migration and closed-order integration each1pass, final affected E2E6pass.
Zero final failures/skips/retries. The42 scenario groups describe broader variants;
these counts are not a blanket claim that every listed variant was run. Representative
coverage above and source review support local implementation review. The user explicitly permits local handoff with unrun manual checks;
local delivery sign-off complete, full manual accessibility remains unverified. No waiver,
GitHub CI, live activation or release-readiness inferred. See evidence.md/review.md.

Execution update2026-10-02: four additional streamed-body/paging/overflow integration
cases pass separately. Prior full integration191pass retained; no full195-case run
claimed. Screen-reader/physical mobile keyboard/IME checks remain Not run with
explicit user permission for local handoff. See latest evidence/decision/review.

## Revision3 final execution

Supersedes the earlier partial-count boundary: actual full Release suite now passes
239unit/195integration (includes the four new cases), frontend253 and fresh-fixture
E2E48,0skips/retries. Existing CI owner-activation shell successfully rehearsed against
a new database before API startup. Manual Not run disposition unchanged. Remote CI
results will be recorded separately; no GitHub pass inferred from local checks.
