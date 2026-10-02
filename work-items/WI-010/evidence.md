# WI-010 — Evidence

## Planning preparation — 2026-10-01

- Read project/policy/common, feature-delivery, planning, documentation/git rules

  and design-consistency checklist, plus WI-009 brief/plan/status/decisions/evidence.

- Main working tree clean before planning; master baseline 42e8932.

- PR #34 state MERGED, merge SHA 42e89326151b1ebb68c8ae30494a621589f5c7dc.

- Read original development-opportunities PDF pages 1/2/3/5 using local pypdf:

  working days/holidays/shutdowns and suggested working-day date arithmetic.

  Initial extraction failed on Windows output encoding and a subsequent sandbox

  package lookup; elevated explicit UTF-8 extraction succeeded. No application failure.

- Created isolated WI010 planning checkout and branch; only initial work-item

  records authored. No design source or companion artifact produced yet.

- Read/write roles, calendar scope and order impact asked; no reply inferred.

- No application verification run or claimed for WI-010. Prior WI-009 runtime

  evidence is dependency evidence, not proof this new calendar is implemented.

## Approved revision 1 step 1 — independent checks

Explicit plan approval recorded. Numbering audit found no existing 006 family;

REQ-070–075 are unique outside this work item's draft and reserved here. Reviewed

existing IPlantClock/date-window and line-hours/timing contracts. Source PDF

working-day date arithmetic remains a proposal requiring DEC-004 confirmation.

PR #34 MERGED at 42e8932; final CI run 36832958896 success on 6f49c31, all three

jobs successful. Reconciled routine WI-009/root current-state records in this

isolated checkout; earlier evidence remains preserved. All 176 existing

files under docs retain their SHA-256 hashes. No application source or design

changed. Step 1 business decision portion remains pending DEC-002–007.

## Initial business answers recorded

Explicit bundled user agreement resolves DEC-002–004. Remaining DEC-005–007

questions sent. Read IPlantClock (DateOnly/plant timezone) and DashboardWindow:

current due-soon uses today.AddDays(7), intentionally unchanged by confirmed

lookup-only boundary. Document family 006 remains reserved; approved 005 sources

read only. No calendar implementation or runtime behavior claimed.

## 006_REQ version 1 review package — 2026-10-01

Recorded explicit approval of the three remaining rule proposals as DEC-005–007;

interpretation stated to the user before authoring. Core rules resolved. RP-01–05

remain explicit source-document review proposals, not invented business answers.

Created only 006_REQ source and its companion English/Japanese PDFs. Japanese

translation remains temporary outside git. No next design Markdown authored.

Actual checks: REQ-070–075 each has success and failure criteria; UC-019–021 unique;

RP-01–05 explicitly marked. All source local links resolve. Both PDFs contain all

REQ/UC/RP IDs and dated version footer: EN 5 pages, JA 4 pages. Representative

first/acceptance/last PDF pages visually inspected: tables, Japanese text and

footers readable, no observed clipping. Existing tracked docs have zero diff;

staged/working whitespace check clean. Source SHA-256: 5df4cd78086586ccb445c2a51e0a601b8547f628bc332613840d1dfbe3b730f9.

Requirements-stage consistency checks pass; full BD/DB/DD design/security gate

belongs to later revision 1 steps. Application tests not run, calendar unimplemented.

Waiting for explicit 006_REQ review/continue instruction before dependent design.

## Approved REQ and 006_BD version 1 review package — 2026-10-01

Explicit user approval accepts 006_REQ version 1 including RP-01–05. Recorded

approval in decisions.md; approved source unchanged. Completed revision 1 step 3

architecture/security assessment, reusing existing ADRs without new boundaries.

Created only the next design source, 006_BD version 1, with four SVG wireframes

(desktop and three mobile panels), EN PDF 15 pages and JA PDF 14 pages.

Temporary Japanese translation and render inspection artifacts stay outside git.

Actual verification: local Markdown links resolve; all four SVGs parse and their

numbered regions match the nine-item BD legend. EN/JA headless Chromium renders

each contain exactly 19 navigation edges, matching the inventory table. Both PDFs

contain REQ-070–075, FN-037–040 and the current dated version footer, with rendered

Mermaid rather than raw diagram source. Representative diagram/table/desktop/

mobile PDF pages visually inspected; final mobile panels readable with no observed

clipping. All 179 protected prior document artifacts retain their SHA-256 hashes.

BD SHA-256: e37d1b8d9a6c621277dd4a34461567295a5b3ad2e09b4201c220ca7bf61feac3.

Tool corrections: initial renderer wrote the EN PDF but its Windows console

failed to print the Japanese filename; rerunning with PYTHONIOENCODING=utf-8

completed successfully for both PDFs. The initial browser verification observed

an incomplete Mermaid render and used an unsupported SVG innerText call; corrected

to textContent and an explicit 19-edge readiness wait. Final EN/JA checks passed.

These are verification tooling failures, not application defects. Mobile layout

was split into three SVG panels after visual review to improve PDF readability.

BD-stage checklist recorded in review.md. DB/API/FN/SPD and full cross-document

security/design gates remain later. No application test, runtime calendar, schema

execution, deployment, commit/push or PR claimed. Awaiting explicit BD review

before writing 006_DB.

## Approved BD and 006_DB version 1 review package — 2026-10-02

User explicitly approved the presented 006_BD package. Recorded the approval;

continued approved revision 1 step 5 only. Read database-design skill, database

rules/template, approved requirements/BD, existing 005_DB and actual line/master

configuration/migrations/plant clock. Checked PostgreSQL 17 primary documentation

for constraints, numeric precision, partial indexes and transaction isolation.

Created new 006_DB only, with three planned tables, ER diagram and EN/JA PDFs.

Source initially drafted 2026-10-01; resumed and completed 2026-10-02. Japanese

translation and browser/page inspection artifacts remain temporary outside git.

Actual verification: all local source links resolve; EN/JA stable REQ/FN IDs,

named constraints/indexes and ER source match. Each headless Chromium ER render

has five relationships. EN PDF 12 pages, JA 11 pages; both contain REQ-070–075,

rendered diagram (no raw erDiagram) and 006_DB version 1 (2026-10-02) footer.

Representative EN/JA ER, column tables and final Japanese page visually inspected,

with no observed clipping. All 186 protected prior document artifacts retain

SHA-256 hashes. Whitespace check passed. Final DB source SHA-256:

465e06e53985fcddd485f039fbb594c4dc5653edba2492e77f25cc9d54a43793.

Verification tooling corrections: sandbox hash read initially could not access

an approved Japanese BD PDF; elevated read verified all protected artifacts.

A sandbox pypdf import also failed; elevated dependency access succeeded. Initial

PDF ID assertion omitted the leading zero (REQ-70 instead of REQ-070); corrected

the checker, with final IDs passing. These were tooling/access errors and no

application failure or document content defect was inferred. Updated only the

unapproved DB's completion date and re-rendered both PDFs with matching footers.

DB-stage consistency review recorded. Technical schema proposals await user DB

review; exact API/FN budgets and cross-document agreement belong to later steps.

No SQL/migration/application test run, schema execution, code change or runtime

calendar claimed. Awaiting DB review before writing main 006_DD.

## Approved DB and main 006_DD version 1 review package — 2026-10-02

Explicit user approval accepted the presented 006_DB source and both PDFs.

Recorded approval and protected input hashes; continued revision 1 step 6 only.

Read detailed-design and screen-design skills, templates, backend/frontend rules,

approved REQ/BD/DB and prior immutable 005_DD. Created main 006_DD only, with two

numbered editor SVGs, EN/JA nine-state static HTML mockups and EN/JA PDFs.

The skills' referenced design tool/artifact publisher is not available in this

session. Used local HTML artifacts rendered in Chromium as the bounded static

preview fallback; no external publication. Japanese translation and screenshots

remain temporary outside git. Companions DD-API/FN/SPD Markdown not authored:

explicit sequential-review policy and approved plan require their later stops.

The DD family is not claimed complete.

Actual checks: all main-source local links resolve; reserved companion names are

plain text rather than broken links. EN/JA REQ/FN/field IDs agree; all three state

inventories match diagram sources and transition tables. Headless Chromium renders

7/10/10 edges per language. Both SVGs XML-valid; numbered regions match main DD

legend. Mockups each have nine artboards; actual browser checks at 1280px and 320px

found no page horizontal overflow. Both SVGs browser-rendered; representative

validation/confirmation and phone views visually inspected. PDFs contain all

REQ-070–075 and FN-037–040 and current dated footer; no raw stateDiagram-v2.

EN 15 pages, JA 14 pages. Representative read/draft/write diagrams and editor

SVG PDF pages visually inspected; no observed clipping. Some visuals occupy their

own page by renderer pagination. All 189 protected prior artifacts retain hashes.

Whitespace check passed. Main DD SHA-256:

c1869f77f5b960c8effa2fc331f25500032bd9053317d07e787530cf69aa556d.

No application/SQL test, interactive accessibility proof, migration execution,

implemented calendar or deployment claimed. Static spans are illustrative controls,

not a functional form. Main-DD-stage consistency reviewed; exact API/FN/SPD

contracts and full design/security gate remain later. Awaiting main DD review

before writing 006_DD-API.

## Approved main DD and 006_DD-API review package — 2026-10-02

User explicitly approved main DD and its presented companions. Recorded source

hash/approval; continued approved revision 1 step 7 only. Read applicable project,

workflow/rules/API template, approved brief/designs and actual Production lines

controller, request reader, Problem Details mapping and shared frontend API client.

Checked RFC 9457 primary source. No existing API or approved design was rewritten.

Created only new 006_DD-API version 1 and EN/JA PDFs; temporary translation outside git.

Actual checks: eleven operation registry entries and one specification section per

operation; local links resolve; EN/JA operation/requirement/function/error IDs and

registry rows agree. English PDF 9 pages, Japanese 8 pages. Both contain all eleven

API-PC IDs, REQ-070–075 and current dated version footer. Representative registry,

shared schema, endpoint and error pages visually inspected, no observed clipping.

All 196 protected prior document artifacts retain SHA-256 hashes. Final API source

SHA-256: a810385a00fe1813b8767fd73d0028c8d80e699901cf48c1cb62709c1ab852f1.

Whitespace check passed.

Tooling corrections: Python warned about an escaped Markdown table pipe in an

ordinary string; the intended literal regex survived and rendered correctly.

URI/span angle placeholders were wrapped in Markdown code to preserve literal

text in HTML/PDF. Initial Japanese PDF placeholder assertion failed because PDF

extraction inserted a line break; visual check showed correct content and corrected

line-wrap normalization passed. No application/document semantic defect inferred.

API-stage consistency checked against approved DB/main DD: global version/target

identity, null inheritance, history markers, snapshot paging, current-unit capacity,

role policies and honest unknown outcome retained. Technical budgets/routes/error

and telemetry proposals await API review. Exact FN/SPD behavior and full gate later.

No API runtime/SQL test, application implementation, schema execution, external

operation, deployment or video claimed. Awaiting DD-API review before DD-FN.

## Approved API and 006_DD-FN review package — 2026-10-02

Explicit user approval accepted API version 1 and both presented PDFs. Recorded

approval/hash in decisions.md; continued approved revision 1 step 8 only. Read

applicable function-design template, backend rules, approved DB/API and actual

plant clock ports and repository patterns. Consulted PostgreSQL primary isolation

and locking documentation. Authored only 006_DD-FN version 1 and EN/JA PDFs;

Japanese translation and review PNGs remain temporary outside git.

Actual checks: all local source links resolve; EN/JA M-01–15, eleven API-PC IDs,

REQ-070–075, function/error catalogs and method indexes agree. SQL block identical.

Seven exact design vectors independently checked using Python Fraction, with floor

inequalities and Int64 bounds; document arithmetic only, not C# implementation or

application test. EN PDF 11 pages; JA PDF 10 pages. Both preserve operation/method/

requirement IDs, literal generic return types and the current dated version footer.

Representative EN pages 1/4/5 and JA pages 4/10 visually inspected; no observed

clipping. All 199 protected prior artifacts unchanged by SHA-256. Whitespace check

passed. FN source SHA-256:

c212eba6b9a8d45e4879bba0144d1ad86b17a8be3cdf31d9d08f0a4596611610.

Before rendering, generic return types were wrapped in Markdown code so angle

brackets remain literal. Initial interface path lookup found no file; rg located

the actual IPlantClock contract in ProductionOrders/Ports.cs, which was then read.

No runtime defect or application behavior claimed from these tooling corrections.

FN-stage consistency checked against approved DB/API: retained revisions, coherent

reads, protected dates, lock order, authoritative no-op checks, exact capacity

flooring, commit uncertainty and bounded telemetry. No application/SQL/concurrency

or telemetry test, migration, code, external publication, commit/push/PR/deployment

or video output. Awaiting DD-FN review before authoring DD-SPD; full gates later.

## Approved FN and 006_DD-SPD review package — 2026-10-02

User explicitly approved FN version 1 and both PDFs. Recorded approval/hash in

decisions.md; continued revision 1 step 9 only. Read project/workflow instructions,

screen-design/detailed-design skills, correct screen-processing template, frontend

rules, approved inputs, existing navigation guard and master visual tokens.

Consulted WAI primary dialog/reflow guidance. No prior design edits.

Authored only 006_DD-SPD version 1, two new numbered SVGs, EN/JA twelve-artboard

static HTML galleries and EN/JA PDFs. Japanese translation, browser verification

script and review screenshots remain temporary outside git. Native design/Artifact

publishing is unavailable; reused the existing local HTML fallback. No API or

interactive application simulation.

Actual checks: local links resolve; EN/JA eleven API IDs, seven processing blocks,

REQ/FN/error IDs, thirty-four exact Japanese messages and step counts

9/8/8/7/7/5/9 agree. Both SVGs XML-valid; each region unique within its sheet,

union 1–16 equals source legend. Browser rendered both galleries at 1280/640/320px:

twelve artboards, no page horizontal overflow, expected 640px agenda breakpoint,

confirmation centered within the illustration stage. Both SVGs browser-rendered.

Phone validation/unknown and desktop confirmation screenshots visually inspected.

English PDF 13 pages, Japanese 12 pages: all API/process/requirement IDs and dated

version footers present. Representative EN pages 1/7/9 and JA pages 10/12 visually

inspected, no observed clipping. All 202 prior protected artifacts unchanged.

SPD SHA-256: 462337bd6a3c9e0de774ec37ed04b6b0f44030ff214b7ebc36487e5444093a78. Whitespace check passed.

Tooling corrections: an initial template lookup used screen-design.md, which does

not exist; the skill identifies screen-processing-design.md, subsequently read.

First PDF assertion incorrectly used REQ-70 instead of REQ-070. After correcting

zero padding, strict Japanese-string extraction comparison encountered compatibility

radicals, ligatures and inserted whitespace. NFKC/whitespace normalization passed;

source messages still compared exactly, and visual inspection confirmed readable

content. These were verification-script/extraction corrections, not skipped checks.

No application/SQL/concurrency test, implemented modal/focus proof, native zoom,

screen-reader audit, migration, code, external publication, commit/push/PR/deploy

or evidence video. SPD-stage consistency checked; full design/security gate and

impact assessment remain after SPD review. Implementation needs a later approved

revision. Awaiting user review of DD-SPD and companions.

## SPD approval and final design-phase reconciliation — 2026-10-02

User explicitly approved 006_DD-SPD source and its presented companions. Recorded

approval/hash; completed approved revision 1 steps10–12. Conditional impact DD is

not applicable: calendar/capacity remains SCR-006 with no order/master/dashboard

business or date-rule change. Existing navigation/router/catalog/DI additions are

specified feature integration, not an old-screen behavior rewrite.

Read planning/testing/security-review skills and full design/security/delivery

checklists; reviewed approved schemas/algorithms/contracts, existing cookie posture,

DI/telemetry and test placement. Prepared routine TP-WI-010 revision 1 with42 unique

TC-366–407 scenario groups, following audited prior maximum365. All runtime results

Not run. Appended concrete implementation revision 2 in plan.md; submitted for user

review, not approved. User continuation while this plan was being prepared does

not approve a not-yet-presented revision.

Actual final artifact checks: seven sources and14 EN/JA PDFs exist; local links

resolve; each PDF page carries document/version marker; final EN/JA page counts

REQ5/4,BD15/14,DB12/11,main15/14,API9/8,FN11/10,SPD13/12. Current static navigation

count19, main state counts7/10/10, eight XML-valid SVGs with unique mapped region

IDs. Prior rendered inventory/table/visual evidence remains valid because all209

protected files retained their hashes. All42 test IDs unique/sequential; requirement

and important error/race paths mapped. Full design-consistency and design-stage

security checklist results recorded in review.md; runtime gates pending.

Audit-script corrections: Japanese REQ footer says 第1版, not English version1;

verified independently with pypdf/fitz and accepted the existing equivalent revision

marker without editing PDF. Navigation count initially counted only solid arrows;

inspection showed seven valid dotted arrows, included in corrected19-edge count.

A diagnostic print lacked UTF-8 environment and failed on the Japanese filename;

subsequent checks explicitly use UTF-8. No checks were skipped/weakened or artifacts

modified to pass them. Guessed infrastructure/telemetry filenames were absent;

rg found the actual DependencyInjection.cs and LineSnapshotTelemetryTests.cs.

CalendarRange export is not verified: frontend node_modules absent in this worktree

and main. Locked package restore/export audit is explicit revision 2 step1.

Git diff inspection found no src/tests/scripts/deploy/.github changes. No application

build/test, SQL/activation/migration rehearsal, package restore, runtime accessibility,

telemetry export or video output run. No commit/push/PR/merge/deploy/publication/live

write. Prior final videos and unrelated main changes preserved. Await revision 2

approval before implementation; evidence of app behavior will be recorded only

when those later tests actually execute.

## Revision 2 implementation checkpoint — 2026-10-02

Explicit approval received after revision 2 presentation. Tool audit: .NET10.0.303,

EF10.0.12, Node24.18.0/npm12.0.2; Docker29.8.1 started locally. Backend restore

passed; frontend npm ci passed134 packages/zero reported vulnerabilities, unchanged

locks. CalendarRange export verified from frontend package after restore.

`dotnet test tests/backend/ProductionManagementAI.Application.Tests --filter FullyQualifiedName~CalendarRulesTests`:

47 passed/0failed/0skipped. Pure exact-value/date/Unicode/mask/precedence/floor and

eligibility cases partially cover TC-366–375; no blanket completion of scenario groups.

`dotnet build src/backend/ProductionManagementAI.Api --no-restore`: passed0warnings/

0errors after service/telemetry DI. Generated additive ExpandPlantCalendar migration,

owner activation SQL and restricted retained repository/eleven APIs; no live execution.

First isolated `dotnet test tests/integration/ProductionManagementAI.Integration.Tests --filter FullyQualifiedName~CalendarApiTests --no-restore`:

12passed/2failed/0skipped. Initial range vector allowed366 inclusive dates and was

corrected to367; accepted bound not weakened. Real line FOR SHARE query required

explicit PostgreSQL xmin projection through EF subquery, fixed without changing

lock/grant behavior. Disposable fresh migration/activation and plant save/noop/stale

checks executed before failure. Temporary diagnostic removed; retest pending.

Frontend/full/race/operational/security/delivery checks pending. All changes uncommitted.

### Calendar integration retest and frontend adapter

After explicit xmin projection, CalendarApiTests passed14/14, then the expanded
suite passed18/18, zero skipped. Disposable PostgreSQL17 fixtures verify all11
routes deny anonymous/unprivileged users before parsing, exact queries/bodies,
media/8KiB limits, no activation on reads, retained exception save/no-op/stale/
removal/fallback, closed capacity0, weekly withdrawal/re-add/predecessor,
stale paging and restricted pmai_app payload/activation/delete grants.

Frontend exact values/types and feature-only25s adapter authored.
`npm test -- tests/unit/plant-calendar`:39 passed,2files,zero skips.
`npm run build` and `npm run lint`: passed. Tests cover date/hour/Unicode bounds,
exact version/quantity display, strict URLs, UTF8 serialized limits, safe write
settlement/malformed success/network loss/no retry and existing auth teardown.
No calendar UI or composed E2E yet. No compiler/dependency upgrades.

Implementation diagnostics corrected: a package-relative writing command initially
used repo-relative paths, so wrote no files; corrected before compiling changed
source. Existing TypeScript target lacks ES2024 isWellFormed/parameter properties;
used compatible surrogate validation/explicit fields. A routine record write used
Windows cp1252 for appended punctuation; repaired these records to UTF8. Approved
source/PDF artifacts were never edited. No test weakened or hidden retry applied.

### Connected frontend checkpoint

Calendar route/CalendarRange navbar and centralized Japanese catalog connected.
Responsive month/day, weekly/date editors/history and independent capacity authored.
Latest `npm test -- tests/unit/plant-calendar`:44passed/4files/zero skipped.
Editor tests verify closed clears hidden hours, all nullable fields, linked validation
focus and Unknown preserves draft/blocks replay/failed observation disables Accept.
Deferred reader tests verify obsolete responses ignored and reader abort on unmount.
Changed-source build/lint pass. Runtime/complete scenario-group coverage pending.

Verified no existing Docker containers and unused ports54410/18110/30110 before
new fixture preparation. Generated throwaway credentials only in OS temp file;
new project pmai-wi010-check-20261002, database wi010_fixture. No app runtime claim
until build/migrate/explicit fixture activation/smoke actually run.

### Isolated composed runtime and transaction certainty

New Compose project pmai-wi010-check-20261002 built/started successfully.
Owner-only fresh EF migrations applied to wi010_fixture; initial missing history
query and existing concurrent-index warnings are fresh/bootstrap diagnostics,
not WI010 failure. Actual scripts/calendar/activate.sql executed as fixture owner
with explicit2026-10-02/Asia-Tokyo, state and initialmask31 inserted atomically.
No existing/demo/live data touched. E2E npm ci8packages/zero reported vulnerabilities.

Latest `npx playwright test specs/plant-calendar` against http://localhost:30110:
4passed/0failed/0skipped/retries0,9.0s. Desktop3/mobile1. Real save/remove/history,
all-closed weekly withdrawal, guarded navigation, committed-response loss with
one persisted snapshot/no replay, independent observation/explicit discard,
centered native modal/Cancel initial focus/Tab cycle/Escape and320px reflow; axe
checks pass on implemented month/editor/dialog/mobile views. Native browser zoom,
physical/mobile keyboard and screen-reader checks not yet run.

Earlier runtime failures recorded: stale prior save notice raced subsequent mode
selection; clear after authorized user navigation. Confirmed Cancel previously
reentered the same dirty guard; parent now executes already-confirmed departure.
Reason wrapping label's text changed with textarea contents in Chromium; explicit
htmlFor/id association fixes it without weakening test. Corrected/rebuilt fixture
frontend and reran affected journeys. Two failed extended journeys became4passing.

`dotnet test tests/integration/ProductionManagementAI.Integration.Tests --filter FullyQualifiedName~CalendarTransactionTests --no-restore`:
3passed/0failed/0skipped. Interceptor faults immediately before/after COMMIT both
return Unknown with zero/one actual persisted snapshot respectively, no replay;
state version follows real outcome. Suppressed conditional state update rolls back
inserted snapshot and returns stale/NotApplied. EF timeout/savepoint/transaction
state restored. Additional races/read-snapshot/migration/telemetry gates pending.

Protected artifact hash check:209 unchanged. No commit/push/PR/merge/deploy/video.

## Revision 2 final local verification — 2026-10-02

Environment: isolated feature/WI-010-plant-calendar checkout, baseline 42e8932,
uncommitted implementation; .NET10.0.303/EF10.0.12, Node24.18.0/npm12.0.2,
PostgreSQL17, Docker29.8.1, Chromium. No GitHub CI run or deployment claimed.

| Executed command / check | Actual result |
| --- | --- |
| dotnet test src/backend/ProductionManagementAI.slnx --no-restore | 239 backend unit /190 integration passed;0failed/0skipped before additional BUSY test |
| dotnet test tests/integration/ProductionManagementAI.Integration.Tests --no-restore | Final complete integration run191passed/0failed/0skipped,1m18s |
| dotnet test tests/integration/ProductionManagementAI.Integration.Tests --no-restore --filter FullyQualifiedName~CalendarMigrationTests | 1passed after final Down timeout fix; safe unactivated Down and activated refusal verified |
| dotnet test tests/integration/ProductionManagementAI.Integration.Tests --no-restore --filter FullyQualifiedName~ExplicitActivationRetainedHistoryPrecedenceNoopAndCapacityUseRestrictedLogin | 1passed after adding TC-407 closure/order assertions; real closed today permits due-today creation/start with eligible line, yesterday due still rejected |
| npm test (src/frontend) | Final253passed/21files/0failed/0skipped |
| npm test -- tests/unit/plant-calendar | Final48passed/5files, including two weekly list-refresh draft/Unknown regressions |
| npm run build / npm run lint (src/frontend) | Pass; production build and strict TypeScript check successful |
| npx playwright test --workers=1 --retries=0 (tests/e2e, isolated E2E_BASE_URL) | Full48passed/0failed/0skipped before final feature adapter/list-refresh fixes; existing42 flows retained |
| npx playwright test specs/plant-calendar.spec.ts specs/plant-calendar.mobile.spec.ts --workers=1 --retries=0 | Final affected six journeys pass after rebuilt frontend;5desktop/1mobile,11.4s; no replay/hidden retry |
| Locked restores | Backend restore, frontend npm ci134packages and E2E npm ci8packages passed; npm reported0vulnerabilities; package/lock versions unchanged |
| git diff --check / generated credential scan | Pass; generated fixture passwords have0matches in changed tracked/untracked artifacts; no secret values printed |
| Protected approved artifacts | All209 source/PDF/visual files unchanged by SHA-256 against reconciliation manifest |

Calendar-specific integration coverage totals34 tests:24 API,5 snapshot/lock,
3 transaction fault,1 migration and1 telemetry. Backend pure calendar slice47.
These method counts do not imply every variant in the42 TC scenario groups passed.
See test-plan.md for executed coverage and remaining variations.

### Storage, concurrency and telemetry proof

Real read-only REPEATABLE READ remains coherent across an independent calendar
commit. Controlled clock advances after target query verify authoritative fresh
plant date. Concurrent unrelated saves have exactly one winner/global conflict.
Actual line FOR SHARE conflicts with a non-key retirement update; retired edit is
rejected before no-op while allowed retained removal succeeds. A held state lock
produces503/CALENDAR_BUSY/NotApplied with Retry-After1 after confirmed rollback;
pooled connection shows statement_timeout0, lock_timeout0 and read-only off.
Test-only commit faults before/after acknowledgement preserve honest Unknown and
zero/one persisted result; conditional zero state update rolls back the appended row.

Migration rehearsal upgraded nonempty42e8932 schema, checksumming products/orders/
lines/pairs unchanged, and confirmed numeric without typmod, null plant-head uniqueness,
4-digit precision rejection, safe unactivated Down and protected activated Down.
Runtime pmai_app cannot modify payload, activation or delete history. Final review
added the same5s lock/15s statement budgets to Down, then reran migration proof.

Real Activity/Meter listeners observed exactly one operation counter and duration
for each401/403/parser rejection/success with bounded operation/outcome labels.
Feature handler/read/resolve spans and actual database/HTTP trace sanitization
checked; raw query/SQL/business note values removed. Scoped EF logger wrapper
suppresses feature SQL diagnostics without changing old-feature logging. External
OTLP export and all outcome combinations were not exercised.

### Owner tooling rehearsal

Dedicated project pmai-wi010-check-20261002 used new wi010_fixture database and
ports54410/18110/30110, initially verified unused. Actual owner activate.sql succeeded
with explicit2026-10-02 and Asia/Tokyo, atomically inserting singletonrevision 1 and
initialmask31. A second fresh wi010_activation_rehearsal database exercised the
final activate.ps1 wrapper plus SQL. Expected state/date/zone/revision/mask confirmed;
repeat activation rejected without reset, and implicit date 'today' rejected before SQL.
Credentials existed only in OS temp environment file. Operational/recovery instructions
are in deploy/README.md; no mutable existing demo/live database was accessed.

### Frontend and manual observations

Full initial frontend run had10failures (238passed):4 navigation expected-list
updates,5 lexical NoInlineText/internal diagnostic findings,1 existing asynchronous
document-title assertion. Corrected expected calendar entry, statement boundaries,
safe code-only diagnostics and waitFor the existing title effect. Assertions were
not removed/weakened; later full253passed. jsdom canvas diagnostic warnings are
existing test-environment limitations; tests did not fail or skip.

Controlled snapshot fixture initially attempted to rewind FakeTimeProvider using
SetUtcNow; corrected fixture initialization to AdjustTime, then snapshot cases passed.
Final adapter review rejects structurally valid responses for another requested
month/date/scope/target and inconsistent commit versions; two additional tests pass.
Final weekly-list Restart previously remounted the edit baseline without confirmation.
Separated list refresh generation from edit generation; dirty and Unknown drafts
are preserved. Two meaningful regressions and final affected E2E pass.

Native headed Chromium200% zoom used OS Ctrl+0/five Ctrl+Equal keystrokes and Tab.
Observed DPR2, innerWidth672/height442, layout width664, no horizontal overflow.
Dialog center332.25/221.25 matches layout viewport center332/221; Cancel initially
focused and Tab remains contained. Captured only the task-owned browser window,
visually inspected its image: Japanese text/actions/focus readable and modal centered.
The browser/helper closed after observation; no evidence video generated.
Playwright mobile emulation320/390px checks agenda, centered dialog, Tab/Escape,
reflow and axe. Screen-reader speech and physical mobile keyboard/IME are **Not run**:
no corresponding configured tool/device. Automated axe/emulation and native zoom
are not substitutes for these manual checks or full accessibility certification.

### Cleanup and delivery boundary

Verified Compose project/container/volume labels before down -v. Removed only the
three pmai-wi010-check-20261002 containers, its network and its two volumes (including
both disposable databases). Existing master checkout remains clean; four WI-009
final MP4 files remain in its demos/evidence/output. Older product evidence and
approved artifacts were not targets of cleanup. No commit/push/PR/merge/live
activation/deployment/new video action performed. Local source package is ready
for review; full manual accessibility acceptance remains incomplete.

Final cleanup: removed the three explicitly named task-owned Compose images and
only the verified task temporary credential/log/native helper/result/image files.
Protected209-artifact audit repeated after cleanup:0changed. The design-phase hash
manifest remains available for later review. No shared Docker image pruning.

## Resumed revision 2 verification and local close-out — 2026-10-02

User requested continued WI-010 work. After presenting the remaining manual limits,
the user explicitly permitted local handoff with screen-reader and physical mobile
keyboard/IME checks still recorded Not run. Windows Narrator executable exists;
this environment provides no callable desktop/speech verification tool for its
output, and no physical mobile device. No unobserved speech/device result claimed.

Completed concrete remaining API/storage boundary checks in
`tests/integration/ProductionManagementAI.Integration.Tests/PlantCalendar/CalendarBoundaryTests.cs`:

`dotnet test tests/integration/ProductionManagementAI.Integration.Tests --no-restore --filter 'FullyQualifiedName~CalendarStreamBoundaryTests|FullyQualifiedName~CalendarPagingTests|FullyQualifiedName~CalendarOverflowTests'`:
**4 passed /0 failed /0 skipped**,2seconds. Build succeeds before execution.

- TC-390: custom HttpContent has no Content-Length and writes127-byte chunks.
  Valid JSON padded to exactly8192 UTF-8 bytes reaches the unactivated-state check
  (409/CALENDAR_NOT_ACTIVATED);8193 bytes returns413/REQUEST_TOO_LARGE. Both
  NotApplied; no state created. This exercises the streamed reader, not only the
  Content-Length fast rejection.
- TC-395:23 actual exception commits including retained removal marker. Page1 has20
  rows and page2 has3, no overlapping IDs, descending exact revisions24..2; page3
  empty with correct total. A later unrelated weekly commit makes old page2 token
  fail409/CALENDAR_STALE. Real52 matching line choices produce ordered50/2 pages.
- TC-377/391: owner fixture at long.MaxValue; changed save returns500/UNEXPECTED/
  Unknown, revision unchanged, no appended exception or altered initial weekly head.
  No reset/wrap/replay. Isolated fixture setup only, no live or demo writes.

Testcontainers owns and disposes these fresh PostgreSQL fixtures. No composed app
restarted, no package/application implementation changes or old-suite reruns needed
for this test-only addition. Previous full regression results remain239 unit,
191 integration,253 frontend and48 E2E plus final affected six journeys. Four new
integration cases passed separately; do not call this a full195-case run.
Calendar integration inventory now38; no blanket claim every TC variant was run.

Reviewed new tests against approved API/FN/DB and security/delivery boundaries;
no unresolved code finding. Final diff whitespace check passes. Protected artifacts
rechecked:209unchanged. No credential/new dependency/CI permission introduced.
Local delivery approved for handoff with documented manual limits; no external
operation performed. WI-010 worktree retained with all changes uncommitted.

## PR-readiness assessment — 2026-10-02

Read actual .github/workflows/ci.yml and tests/e2e/playwright.config.ts. Existing E2E
job: generated/masked credentials, fresh Compose database, owner migrations, stack
startup and Playwright; no calendar activation step. Playwright workers1/retries0.
Current calendar journeys require the explicitly activated state used in earlier
local Compose proof. This is a concrete CI setup gap, not an observed GitHub failure;
no WI-010 PR/CI run exists. Main master nowf44283c (RFC0013 only). WI-010 unchanged
and uncommitted on its isolated branch. No plan3 execution or workflow edit performed.

## Revision3 CI fixture rehearsal and precommit gates — 2026-10-02

Explicit revision3 approval recorded before execution. Added one existing E2E-job
step: source generated masked environment, compute plant-local ISO date, run existing
activate.sql through container psql as owner after migrations and before API startup.
No new action/job/permission/package/secret; no runtime or live autoactivation.

Fresh project pmai-wi010-ci-20261002/database wi010_ci_rehearsal, initially unused
ports54710/18410/30410. Generated passwords only in OS temp environment. Owner
Release migrations succeeded on fresh PostgreSQL17. Executed the exact activation
shell extracted from ci.yml through Git Bash, changing only environment-source path
and Compose project name for isolation; same owner psql/arguments/SQL. Command exited0;
calendar state/date Asia/Tokyo and initial mask31/commit1 observed (subsequent E2E
writes advanced state). Full stack build/start succeeded, health/frontend ready.

| Actual final command | Result |
| --- | --- |
| dotnet build src/backend/ProductionManagementAI.slnx --no-restore --configuration Release | Pass,0warnings/0errors |
| dotnet test src/backend/ProductionManagementAI.slnx --no-build --configuration Release |239unit/195integration passed,0failed/0skipped |
| npm run lint / npm run build / npm test in src/frontend | All pass;253tests/21files,0skipped |
| npx playwright test in tests/e2e against owned fresh fixture |48passed,1.2min,workers1/retries0,0failed/0skipped |
| Changed artifact audit / git diff --check |105intended files reviewed;0generated credential matches, package/lockfiles unchanged; whitespace pass |
| Approved design/source/PDF/visual protection |209unchanged by SHA-256 |

No YAML parser installed: reviewed YAML indentation/step placement and executed
extracted shell; no parser pass claimed. Actual GitHub workflow acceptance/CI remains
pending PR. Existing fresh history-table/concurrent-index migration diagnostics and
jsdom canvas warnings are recorded baseline/tool limitations; commands passed.

Verified exact Compose/container/volume labels, then removed only the owned three
containers/network/two volumes and explicitly named three project images. Deleted
only the two verified temporary environment/shell files. No shared image pruning,
old checkout/video/approved design or live/demo writes. Manual screen-reader/device
checks remain Not run with explicit user acceptance for local handoff.
