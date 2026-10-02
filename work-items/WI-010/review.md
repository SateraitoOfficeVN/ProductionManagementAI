# WI-010 — Design review

2026-10-02. Stage: revision 1 complete; all seven designs approved.
Design-consistency and design-stage security gate passed; runtime/delivery gates pending.
Implementation revision 2 submitted for review, not approved.
006_DD-FN version 1 explicitly approved after its source/PDF handoff.
006_DD-API version 1 explicitly approved after its source/PDF handoff.
Main 006_DD version 1 explicitly approved after its full review package.
006_DB version 1 explicitly approved after its source/PDF handoff.
006_BD version 1 explicitly approved after its review handoff.
Approved plan revision 1; 006_REQ including RP-01–05 explicitly approved.

## Applicable design-consistency checks at BD stage

| Check | Actual result / boundary |
| --- | --- |
| Stable requirements and acceptance criteria | Approved REQ-070–075 covered by BD; protected REQ hash unchanged |
| Navigation, primary actions and exceptions | 19 inventory edges match 19 rendered edges in each EN/JA diagram; save, cancel, conflict, unknown outcome and dirty departure specified |
| Numbered SVG regions | Four XML-valid SVGs; PC 1–9, SP panels 1–5 / 6–7 / 8–9; all match the BD legend |
| Roles and security fields | Admin/Operator read/write; server authorization, safe text, versions, bounded logging and credential exclusion captured in architecture assessment/BD |
| Accessibility | Keyboard/focus/confirmation/month and responsive lookup behavior captured; runtime WCAG verification belongs to implementation |
| Migration and recovery | Additive activation and retained history; no master/order rewrite; exact constraints and recovery limits belong to DB |
| Telemetry | Bounded operation/outcome, no raw reason/identifier/credential labels; exact contracts belong to API/FN |
| Current companion artifacts | EN 15-page and JA 14-page PDFs rendered; IDs/dated footers and representative pages checked; four SVGs browser-rendered |
| Immutable approved artifacts | 179 protected artifacts unchanged by SHA-256 |
| DD/API/DB agreement and tests | Not yet applicable: sequential DB/DD not authored, application unimplemented; full gate remains revision 1 step 11 |

Architecture assessment found no need for a new ADR. No unresolved business
blocker introduced by BD. API paths and icon alias are design proposals; installed
icon export and exact API/DB validation contracts require later verification.
BD review has completed; this record does not approve DD-API or claim full design,
security, delivery or release-readiness gates have passed.


## Applicable design-consistency checks at DB stage

| Check | Actual result / boundary |
| --- | --- |
| Requirement coverage | REQ-070–075 mapped to schema and meaningful later verification scenarios |
| Approved BD consistency | Activation, future-effective rules, protected past, full exception precedence, line-dependent hours, retirement and conservative unit capacity preserved |
| Keys/nullability/types/constraints | All columns defined; three tables; null-safe plant scope uniqueness; explicit payload/null checks; exact numeric scale/range; supported DateOnly bounds |
| ER relationships | Same source in EN/JA; five browser-rendered relationships; optional line FK matches plant-null identity |
| History/concurrency | Retained snapshots/markers and global revision; atomic supersede+append; line-before-calendar lock order; coherent REPEATABLE READ lookup |
| Privileges and sensitive fields | Column-level runtime updates, no payload rewrite/delete/DDL; reasons plain text and excluded from telemetry; roles enforced at API |
| Migration/recovery | Additive three-table owner migration, explicit activation, no old data rewrite; DDL lock limits, unsafe Down and restore token regression described |
| EN/JA companions | 12/11-page PDFs; IDs/footers verified; representative ER/column/final pages visually inspected |
| Prior approved artifacts | All 186 protected document files unchanged by hash |
| API/FN contracts and runtime tests | Deferred until sequential DD documents and later approved implementation; no SQL execution or runtime proof claimed |

Technical decisions submitted for this DB review: global bigint calendar token,
three-table snapshot/marker storage, weekday mask and reason bound 500 code points.
No open business blocker; full design/security gate remains revision 1 step 11.
DB approval subsequently received; main DD authored in the next authorized step.


## Applicable design-consistency checks at main DD stage

| Check | Actual result / boundary |
| --- | --- |
| BD/DB agreement | F-01–16, protected dates, precedence, master eligibility and global token mapped; approved input files unchanged |
| State diagram/table agreement | Three EN/JA sources and rendered diagrams have exact 7/10/10 inventories; same-state/any-state cases identified separately |
| Numbered SVGs | PC 2/10–16; SP 2/10/12/13/14/16; all in main legend; alternative editor modes explicitly distinguished |
| Visual companions | EN/JA static mockups, nine key artboards; 1280px/320px browser render with no page overflow; illustrative spans, no interactive runtime claim |
| Required PDF companions | EN 15 pages, JA 14 pages; IDs and dated footers checked; representative SVG/diagram pages visually inspected |
| Security/accessibility/migration/telemetry | Role per read/write; plain text/code-point validation; no persisted draft secrets; focus/dialog/zoom targets; approved DB recovery and API/FN instrumentation ownership |
| Immutable designs | All 189 protected prior artifacts unchanged by SHA-256 |
| Full DD family and runtime gates | Pending API/FN/SPD sequential reviews; no application tests/schema execution; full gate remains step 11 |

Main DD package is review-ready. Sequential policy overrides simultaneous companion
authoring; all three companion names and single-owner responsibilities are reserved.
Local static artifacts replace unavailable design publishing tools for this preview.
Main DD subsequently approved; DD-API authored in the next authorized step.


## Applicable design-consistency checks at API stage

| Check | Actual result / boundary |
| --- | --- |
| Complete call inventory | Eleven new API-PC operations, matching main DD logical calls; no existing contract changes |
| Fields and DB agreement | Exact shared response fields and endpoint required inputs; opaque bigint version, scope/date IDs, markers and normalized 500-code-point reason |
| Snapshot and quantity semantics | REPEATABLE READ lookup, version-bound history paging, current unit generation, exact decimal floor and unavailable vs zero preserved |
| Failure certainty | Typed NotApplied vs Unknown; abort/network/equal payload never treated as commit proof; no blind replay |
| Security and observability | Admin/Operator every read/write; bounded strict parsing; no-store; parameterized queries; eleven bounded operation labels and single outcome accounting |
| Companion artifacts | EN 9-page and JA 8-page PDFs; IDs/footers/literal placeholders verified; representative pages visually inspected |
| Protected artifacts | All 196 prior document artifacts unchanged by hash |
| Full cross-document/runtime gate | Pending FN/SPD and step 11; no application or SQL test run |

API technical choices await user review. No open business blocker. API budget/route/
error/telemetry choices are reviewable proposals, not implemented behavior.
Wait for DD-API approval before DD-FN.


## Applicable design-consistency checks at FN stage

API approval recorded; continued approved revision 1 step 8 only.

| Check | Actual result / boundary |
| --- | --- |
| Complete method inventory | Fifteen methods: eleven API handlers and four resolution/calculation/transaction helpers; same EN/JA index |
| Approved DB/API agreement | Snapshot isolation, retained markers, exact target/global versions, lock order and role/error semantics preserved |
| Day and capacity resolution | Whole-winner precedence, current master/unit eligibility and closed/unavailable distinctions preserved |
| Exact flooring | Seven design vectors independently verified with Fraction; integer scaling avoids division rounding before floor; no application helper test claimed |
| Write certainty and timing | Authoritative validation after locks, conditional updates, no-op checks, commit phases and bounded cleanup; no blind replay |
| Security and telemetry | Parameterized SQL examples; grants respected; bounded labels, no raw reasons/IDs and one final request outcome |
| Companion artifacts | EN 11-page and JA 10-page PDFs; IDs/footers/literal generic types checked; representative pages visually inspected |
| Protected artifacts | All 199 prior document artifacts unchanged by SHA-256 |
| Full design/runtime gates | DD-SPD and step 11 remain pending; no SQL execution, race test, implementation or deployment |

No open business blocker or architecture change introduced. FN version 1 awaits
user review before DD-SPD. Full design/security gate is not yet claimed complete.


## Applicable design-consistency checks at SPD stage

FN approval recorded; continued approved revision 1 step 9 only.

| Check | Actual result / boundary |
| --- | --- |
| Traceability / ownership | Seven processing blocks map SCR-006, FN-037–040 and REQ-070–075 to all eleven API operations; state diagrams remain main-DD-owned |
| API/FN/DB agreement | Exact target/marker and baseline version, independent reads, bounded history, protected dates, line retirement/removal and unit eligibility preserved |
| Error settlement | Fourteen error codes and thirty-four exact Japanese strings agree EN/JA; known success with failed refresh distinct from Unknown; no automatic mutation retry |
| Accessibility design | Focus order, native button/table/agenda semantics, safe dialog focus, dirty Back/Forward, 320px reflow and zoom targets specified; runtime compliance not claimed |
| Numbered visuals | Two XML-valid/browser-rendered SVGs; union of numbered regions 1–16 matches legend; alternate phone modes identified |
| Static mockups | Twelve artboards per language; Chromium at 1280/640/320px: no page overflow, expected agenda breakpoint, confirmation centered within illustration stage |
| Required PDFs | EN 13 pages, JA 12; stable IDs/footers/messages checked; representative control/flow/catalog/final pages inspected, no observed clipping |
| Immutable inputs | All 202 protected prior document artifacts retain SHA-256 hashes |
| Full gates / implementation | Await SPD review before step 10 impact assessment and step 11 full design/security gate; no application tests, migrations or code |

No new business or architecture decision. Static rendered artifacts are the existing
fallback for unavailable design/publishing tools; no external publication. No
existing-screen runtime behavior impact is introduced; final assessment follows review.


## Final design-consistency gate — 2026-10-02, revision 1 step 11

Result: **Pass for the approved design family**. This is not application acceptance,
security approval of unimplemented code, a delivery gate or release-readiness.
All earlier stage entries remain historical evidence, not current pending reviews.
The immutable earlier documents' reserved/later-companion wording describes their
creation stage; current ownership is the complete four-file DD family below.

| Checklist item | Actual evidence / result |
| --- | --- |
| Stable requirements/criteria | Pass: approved 006_REQ REQ-070–075 and RP-01–05 trace through seven designs; TP-WI-010 covers all requirements |
| BD navigation/actions/exceptions | Pass: existing inventory/table/render evidence19 edges; current static count19 includes seven dotted cancel/auth edges |
| BD diagrams/numbered regions | Pass: 19 inventory edges; four valid SVGs; region mapping1–9; earlier browser evidence preserved |
| DD fields/validation/states vs BD | Pass: F-01–16, applied/draft separation, strict exact strings and protected dates preserved across main/API/FN/SPD |
| DD diagrams/region tables | Pass: main7/10/10 edges retain table inventories; main and SPD four SVGs XML-valid, unique region IDs and1–16 legend mapping |
| API/DB agreement | Pass: eleven operations/methods map three tables, global/target IDs and retained markers; nullable inheritance, exact decimal/reason/date limits and safe errors agree |
| Missing decisions | Pass: DEC-001–007 and RP-01–05 resolved; no additional architecture/business question; impact addendum not applicable |
| Test scenario trace | Pass (planning): 42 unique TC-366–407 groups map all requirements and important negative paths; all runtime cases explicitly Not run |
| Security fields | Pass (design): cookie/auth/roles, exact input, free-text reason, safe display/logging, grants and no credentials identified; runtime audit still required |
| Accessibility | Pass (design): keyboard/focus/dialog/agenda, text+color,320px/native 200% targets specified; static gallery evidence not compliance proof |
| Migration/recovery | Pass (design): additive schema, explicit owner activation, bounded DDL, restricted grants and unsafe-Down/client reset limits; no executed migration |
| Telemetry | Pass (design): source/meter, handler/child spans, bounded count/duration/outcome and one final owner including auth/parser; no raw business values |
| Current EN/JA PDFs | Pass: seven source files and14 PDFs; every page carries document/revision marker. EN/JA page counts: REQ5/4,BD15/14,DB12/11,main15/14,API9/8,FN11/10,SPD13/12 |
| Complete DD family / immutability | Pass: main/API/FN/SPD all present; links resolve;209 protected files retain SHA-256; no app/test/migration/config changes |

References: [API](../../docs/en/020_detailed-design/006/006_DD-API_稼働カレンダー.md)
section2 at line29, section6 at line272;
[DB](../../docs/en/database/006/006_DB_稼働カレンダー.md) transactions line243,
grants line297, migration line331;
[FN](../../docs/en/020_detailed-design/006/006_DD-FN_稼働カレンダー.md) resolver line96,
floor line123, write line157, certainty/telemetry line337;
[SPD](../../docs/en/020_detailed-design/006/006_DD-SPD_稼働カレンダー.md)
settlement line216 and catalog/focus line238; [test plan](test-plan.md).

## Security checklist — design-stage assessment

Result: **Pass at design stage; repeat against actual code before delivery/merge**.
No exploitable implementation finding is asserted for an unimplemented feature.
Free-text reason can contain sensitive business/personal text: render it as text,
exclude it from diagnostic/log/metric data and retain only in approved history.

| Checklist item | Status / concrete evidence and later proof |
| --- | --- |
| Every endpoint authenticated/authorized | Design pass: API section2/registry all11 Admin/Operator, existing cookie policy; TC-389 proves runtime and target-leak boundary |
| All external inputs validated | Design pass: API section2 strict body/query/date/UUID/enum/Unicode/size; FN parameterized repositories; TC-366–368/390/398 |
| No credentials/tokens/keys in changes | Pass for current document-only additions by review; real runtime values absent. Version tokens are described as schemas, not recorded actual user credentials. No env/log dump consumed |
| Trusted checked dependencies | Not applicable to this change: no dependencies/manifests added/updated. Locked restore/icon export audit planned in revision 2; no installed icon claim because package absent locally |
| Least privilege | Design pass: DB line297 column grants and owner-only activation, no new login/password/role/workflow privileges; TC-396/397 |
| External content treated as data | Pass for reviewed content: source PDF/WAI/PostgreSQL pages used as references only; no provider/upload/webhook or executable external payload boundary introduced |
| Safe errors | Design pass: API line272 generic Problem Details and stable codes, SPD known code mapping; TC-383/390/399 |
| Sensitive logging/evidence | Design pass: FN line337/ API telemetry no query/reason/ID/token/cookie labels/log data; TC-401 listeners verify actual registrations/accounting/sanitization |
| New sensitive trust boundary threat model | Not applicable to new architecture boundary: existing cookie/Identity ADR reused; calendar integrity/privacy risks assessed in architecture-assessment.md before implementation. No payment/new auth/provider introduced |

## Design-phase delivery boundary

Approved scope/revision 1 identifiable and closed; actual document checks recorded,
not-run runtime cases explained; English records and EN/JA companions present;
no external operation or live data change, no dependency or CI change. Status and
plan preserve the next agent's boundary. Application/code/test agreement, runtime
security review, delivery and release gates are **pending implementation**, with
release-readiness not applicable until an authorized deployment. No code or runtime
verification may start until revision 2 receives explicit approval after presentation.

## Revision 2 code review and security gate — 2026-10-02

Reviewed actual tracked diff and new calendar source/test/operations files against
approved REQ-070–075 and DB/API/FN/SPD. Existing architecture/order/dashboard
semantics and immutable designs preserved. No open actionable code finding after
local fixes; remaining verification limits explicitly listed in test-plan.md.
This is local review, not merge approval, final feature acceptance or release-readiness.

| Requirement | Concrete implementation / executed proof |
| --- | --- |
| REQ-070 | PlantCalendarService.MonthAsync/DayAsync, CalendarRepository.ReadSnapshotAsync; Gregorian values, response identity and obsolete read tests; min/leap/max month and coherent snapshot integration |
| REQ-071 | WriteTransitionAsync retained weekly/current markers/global token, WeeklyPatternEditor exact one-date baseline; no-op/withdraw/re-add/stale and list-refresh draft regression |
| REQ-072 | Whole-winner CalendarResolver, strict reason/hour parsing, DateExceptionEditor independent baseline/history; real save/remove/fallback and retirement-lock tests |
| REQ-073 | Eligibility unit/generation priority plus BigInteger exact floor; seven approved floor vectors, valid closed0, real1920kg and retired-product unavailable E2E |
| REQ-074 | Controller policy, feature parser/client, request telemetry/trace processor/scoped EF logger; all11 role contracts, commit faults, Unknown/no replay, axe/keyboard/native 200% |
| REQ-075 | Existing service bodies unchanged; nonempty upgrade checksums, full old-feature suites and explicit closed-day order creation/start/date rejection integration |

### Findings resolved and rechecked

- Calendar response shape alone did not establish requested logical identity. Client
  now checks month/date/scope/target/commit context; malformed mutation remains Unknown.
  Two identity tests and affected six E2E journeys pass.
- Weekly list Restart shared edit generation and could silently discard a draft or
  clear Unknown. Independent list generation preserves edit baseline/settlement;
  two regression tests, full253 frontend and affected E2E pass.
- Unactivated migration Down lacked the approved DDL budgets. Added5s lock/15s
  statement limits before its history guard; affected migration test passes.
- Earlier real UI stale success notice/double departure confirmation and reason label
  findings fixed/retested; no global legacy form behavior changed.

### Security checklist — actual code

| Checklist item | Result / evidence |
| --- | --- |
| Every endpoint authenticated/authorized | Pass: class-level ProductionOrderEditor policy on all11 methods; middleware no-store before auth; anonymous/unprivileged rejected before parser/storage; both roles exercised |
| External input validated / no unsafe concatenation | Pass: feature-only exact query keys/types, bounded streaming body8KiB/depth4, duplicate/unknown keys, Gregorian date, UUID, opaque bigint, decimal and Unicode validators. Production SQL parameters interpolated through EF; search wildcards escaped. Activation date/timezone explicitly validated and bound through psql variables. No client route grants authority |
| No credential/token/key in source/diff/evidence | Pass: actual generated passwords scanned across changed tracked/untracked artifacts with0matches; temp credential contents never printed. No package/config credential added |
| Dependencies trusted and checked | No new/updated dependency or CI action; existing locked restores passed, npm audit0reported vulnerabilities. CalendarRange verified from pinned lucide-react export; no new vulnerability scanner claim |
| Least privilege credentials/roles/permissions | Pass: no new runtime login/role/workflow permission; migration explicitly revokes inherited broad grants, enables SELECT/INSERT and only revision/audit/is_current updates. Real restricted-login denials and owner-only activation exercised |
| External content treated as data | Pass: no new provider/upload/webhook or executable external input; reason displayed through React text. Reference/tool output did not authorize actions |
| Error response safe | Pass: stable feature Problem Details/whitelisted fields, generic500, auth empty401/403; unknown property names not echoed. Client uses central Japanese mappings and generic fallback, never arbitrary server detail |
| Sensitive logging/evidence | Pass by code review and representative real trace tests: no reason/IDs/version/query/SQL labels, bounded eleven operations/outcomes and one request owner. CalendarTraceProcessor strips raw URL/DB attributes before export; CalendarLoggerFactory suppresses request-scoped EF diagnostics. No external OTLP exporter test claimed |
| Changed trust boundary threat model | Existing cookie/Identity ADR and role policy reused; calendar integrity/privacy/owner activation assessed in architecture-assessment.md before code. No payment/provider/new auth boundary introduced |

### Delivery checklist — local review with explicit outstanding checks

| Checklist item | Result / boundary |
| --- | --- |
| Approved scope/revision | Pass: plan 2 approval received after presentation; all seven designs approved and immutable |
| Design/code/tests agree | Pass for reviewed implementation and executed representative coverage; scenario variants not run are explicit in test-plan.md |
| Required checks/results/not-run reasons | Automated checks pass; manual accessibility partial: screen-reader and physical mobile keyboard/IME unavailable, not counted as pass |
| Findings / limitations explicit | Resolved findings above, detailed coverage table; no unresolved actionable code finding. Full manual acceptance remains pending |
| External authorization | Pass: only local code/fixtures; no commit/push/PR/merge/publication/live activation/deploy/videos |
| Handoff status/decisions/evidence | Pass: routine records identify baseline, branch, exact executed checks and next boundary |
| English/Japanese document PDFs | Pass: seven approved sources and14PDFs with companions; all209 protected artifacts unchanged. No new design source added during implementation |
| README/project/CLAUDE current state | Updated to local implementation verification and manual pending; no merged/deployed WI-010 claim |
| Secrets / untrusted content | Pass:0generated credential matches; no leaked values in records and no external instructions followed |
| Flaky / skipped checks | Final runs0failures/0skips/retries0; initial failures/fixture fixes and jsdom warning recorded, no quarantine hidden |
| New action/dependency pinned/minimum permission | Not applicable: no added dependency/CI action/workflow permission |

Security review: **Pass for reviewed local code**, subject to recorded proof boundaries.
Delivery: **local package review-ready; final sign-off pending manual accessibility
checks/explicit disposition**. Plan milestone8 and closure stay partial; no user waiver
inferred. Release-readiness not applicable without authorized deployment.

## Final local delivery gate — 2026-10-02

Supersedes the earlier pending manual-disposition conclusion. User explicitly permits
local handoff while retaining screen-reader/physical mobile keyboard/IME Not run.
Reviewed four added streamed-size/paging/overflow integration cases against approved
contracts; all pass. Application code unchanged since earlier full security review.
All checklist items rechecked: scope/approval/requirements and test evidence agree;
manual and remaining variant limits explicit;209immutable artifacts unchanged;
English records/EN-JA design companions present; no external operation, secret,
dependency/CI permission or unsafe fixture change. Earlier full regression passes
remain valid; four new cases passed separately without inventing a full195-case run.

Result: **Pass for local delivery with user-accepted verification limits**. No
screen-reader/mobile-device accessibility certification, user waiver of application
requirements, remote CI, merge/live activation/deployment or video claim. Revision2
local scope complete; commit/push/PR needs separate task-specific instruction.

### Prepared delivery summary

Japanese Plant calendar: month/agenda, effective weekly rules, plant/line exceptions,
retained history, exact current capacity and eleven Admin/Operator APIs. Coherent
snapshot reads, retained atomic/versioned writes, strict bounded parsing, honest
Unknown/no replay, owner activation and restricted grants. Existing orders/dashboard
unchanged. Verified239unit/191full integration/253frontend/48full E2E, final affected
six journeys and four new integration cases. Two manual checks Not run with explicit
local handoff disposition. Approved designs remain immutable. No PR created yet.

## Revision3 pre-publication security and delivery review — 2026-10-02

Reviewed complete intended105-file package, approved designs and new CI step.
Existing endpoint/auth/parser/grant/commit/telemetry review remains valid; no runtime
code changed during revision3. CI uses existing disposable owner/database, fixed
plant timezone, validated explicit SQL date and quoted parameters; generated values
remain masked, no credentials in command text/records. Existing pinned actions and
contents:read permissions unchanged; no new provider/dependency or trust boundary.

Full security checklist: endpoint/inputs/least privilege/safe errors/sensitive logs
retain earlier passing evidence; no secret/dependency/permission change; external
content treated as data; original architecture threat assessment retained. Full
Release239unit/195integration,253frontend and48fresh-fixture E2E pass without skips/
retries.209approved artifacts unchanged. All EN/JA companions included. Main/videos
preserved, task fixtures cleaned. Delivery checklist passes for local publication
preparation within approved revision3; remote PR/CI not yet claimed. No merge/live
activation/deployment/videos authorized. Remaining manual limits explicitly accepted.

## PR delivery review — 2026-10-02

Actual implementation-head CI run 36975008859 passed Backend, Frontend and E2E
with 239 unit / 195 integration / 253 frontend / 48 E2E cases. Owner disposable
calendar activation succeeded before API startup. No unresolved CI finding.
Final publication records contain no application/design/permission/dependency
changes; implementation review remains valid. Approved artifacts stay immutable.
Delivery gate passes for the approved revision 3 PR scope, subject to verifying
the final routine-record head in GitHub before handoff. Actual final head checks
are linked in PR #36. Screen-reader speech/mobile device IME remain Not run with
explicitly accepted local handoff limits. Merge and release remain unauthorized.
