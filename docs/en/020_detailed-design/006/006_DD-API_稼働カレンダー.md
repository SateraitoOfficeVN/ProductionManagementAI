# Plant calendar — API specification design

006_DD-API version 1 supports SCR-006, FN-037–040 and REQ-070–075.
WI-010 approved plan revision 1 step 7. Proposed contracts; not implemented.

## 1. Document control and references

| Field | Value |
| --- | --- |
| Document ID / version | 006_DD-API / 1 |
| System / subsystem | ProductionManagementAI / Master data |
| Work item / author | WI-010 / Agent |
| Created / updated | 2026-10-02 / 2026-10-02 |
| Review state | Submitted for review; not implemented |

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1 | 2026-10-02 | Agent | Initial eleven-operation catalog and exact shared field/error contracts |

Inputs: [main DD](006_DD_稼働カレンダー.md),
[BD](../../010_basic-design/006/006_BD_稼働カレンダー.md),
[DB](../../database/006/006_DB_稼働カレンダー.md),
[requirements](../../000_requirements/006/006_REQ_plant-calendar.md) and
[plan](../../../../work-items/WI-010/plan.md), all prior designs approved.
This document owns HTTP catalogs; later DD-FN owns backend algorithms and
DD-SPD owns client processing. Their names are reservations, not broken links.
Existing Product/Production lines/order API contracts remain unchanged.

## 2. Transport, authorization and exact common bounds

| Concern | Contract |
| --- | --- |
| Base / handler | /api/plant-calendar; PlantCalendarController, all eleven operations |
| Session / roles | Existing same-origin Identity cookie; Admin or Operator on every read/write, existing ProductionOrderEditor policy; established empty-body 401/403, no HTML redirect |
| Representation | UTF-8 application/json success; application/problem+json application errors; Cache-Control: no-store on feature responses |
| Mutation | application/json only, optional UTF-8 charset; JSON object, <=8 KiB including streamed/chunked bodies; depth <=4; no GET/form/query write, batch or bulk endpoint |
| Strict input | Reject unknown/repeated query keys, unknown/duplicate JSON members, wrong types, missing required properties, invalid Unicode/surrogates and unsupported values; allow only explicitly nullable fields |
| UUID | Nonzero canonical hyphenated UUID text; parse case-insensitively, return lowercase; malformed 400, well-formed unknown reference 404 |
| Date / month | Exact YYYY-MM-DD / YYYY-MM, valid Gregorian date in 0001-01-01..9999-12-31; browser timezone independent; final-month arithmetic must not overflow |
| Query span | Weekly from/to inclusive, from<=to, at most 366 dates; does not cap persisted future rules or valid single-date capacity/mutations |
| Pagination | page ASCII 1..10000, default 1; fixed pageSize 20 for rule/history reads, 50 for choices; no client sort identifier |
| Choice search | q optional, Unicode trim, <=100 code points; literal contains, parameterized; ordering code then UUID using existing DB collation |
| Version | Canonical decimal JSON string 1..9223372036854775807; no leading zeros, signs, whitespace or JSON number; server-only aggregate revision, opaque in UI |
| Decimal request | Trim surrounding whitespace, then syntax ^(0\|[1-9][0-9]*)(\.[0-9]{1,3})?$; >0 and <=24; string only, no sign/exponent/grouping/NaN/infinity/rounding |
| Decimal response | Canonical invariant non-exponent strings, remove unnecessary trailing zeros; hours 0..24, minutes/unit current valid bounds from WI-009, quantity floor-rounded per unit; never JS float transport |
| Reason | Required property with nullable string; Unicode trim, blank -> null; <=500 code points after trim, internal content preserved, plain text |
| Budgets | Feature operation deadline 20s; DB command 15s, statement timeout 15s, lock timeout 5s; cancellation propagated; client timeout 25s; scoped feature configuration, not global rewrites |

Date-only future inputs can use the full supported date range. Mutation dates
must additionally be >=activation and >=authoritative plant today; future-only
withdrawal uses >today. No client counter or If-Match header replaces body version.
History/weekly page 2+ requires snapshotVersion equal to the current global revision;
page 1 may omit it. Mismatch is 409; paging never silently combines calendar edits.
Choice pages describe each read snapshot, not stable selection eligibility forever.
Dates before activation are readable as unavailable, never implicitly working.
Timezone mismatch returns 503 and blocks only this feature pending operational repair.

## 3. Operation catalog / registry

All created/updated by Agent on 2026-10-02. Success 200; no redirect/204/201.
Operation IDs are new API-PC entries and do not reuse existing API-PL IDs.

| ID | Method / relative path | Controller handler / function | Response |
| --- | --- | --- | --- |
| API-PC-01 | GET /month | Month / FN-037 | MonthResult |
| API-PC-02 | GET /day | Day / FN-037/039 | DayResult |
| API-PC-03 | GET /weekly | Weekly / FN-038 | WeeklyPage |
| API-PC-04 | PUT /weekly/{effectiveFrom} | SaveWeekly / FN-038 | MutationResult |
| API-PC-05 | POST /weekly/{effectiveFrom}/withdraw | WithdrawWeekly / FN-038 | MutationResult |
| API-PC-06 | PUT /exceptions | SaveException / FN-039 | MutationResult |
| API-PC-07 | POST /exceptions/remove | RemoveException / FN-039 | MutationResult |
| API-PC-08 | GET /line-choices | LineChoices / FN-037/040 | LineChoicePage |
| API-PC-09 | GET /product-choices | ProductChoices / FN-040 | ProductChoicePage |
| API-PC-10 | GET /capacity | Capacity / FN-040 | CapacityResult |
| API-PC-11 | GET /exception-history | ExceptionHistory / FN-039 | ExceptionPage |

Each row's full URL is base + relative path. All operations follow: authorize
before target lookup; parse strict input/bounds; call the named use case once;
map typed outcome to the shared response or Problem Details; record bounded
telemetry. This is controller dispatch, not a duplicate of FN processing steps.
Reads use one read-only REPEATABLE READ snapshot; writes follow approved DB locks.
Select only necessary fields, batch month reads, no per-day DB calls.

## 4. Shared response field catalogs

A listed property is always present unless expressly optional; nullable values
are returned as null, not omitted. Object/array types below have no other fields.
Shared schema references in endpoint sections are normative, avoiding repetition.

### CalendarContext and scope

| Property | Type / requiredness | Source / meaning |
| --- | --- | --- |
| plantToday | date string, required | Authoritative operation date basis, e.g. 2026-10-02 |
| timeZone | string, required | Configured IANA timezone, currently Asia/Tokyo |
| activatedOn | date string or null, required | State activated_on; null if not activated |
| version | version string or null, required | State revision; null if unactivated |
| Scope.lineId | UUID or null | Null = plant scope |
| Scope.line | LineChoice or null | Selected retained line data; plant scope null |

LineChoice has id UUID, code string <=50, name string <=200, isActive boolean,
workingHoursPerDay decimal string. ProductChoice has id UUID, sku string <=50,
name string <=200, unit from approved seven values and unitRevision canonical
bigint string >=0. Choices contain no edit token or note/credential fields.

### DaySummary and source

| Property | Type | Mapping / meaning |
| --- | --- | --- |
| date | date string | Requested real calendar date |
| state | Working / Closed / Unavailable | Resolved rule or absent recorded coverage |
| hours | decimal string or null | Closed "0"; selected line/explicit working hours; line-dependent/unavailable null |
| hoursBasis | Explicit / LineCurrent / LineDependent / Closed / Unavailable | No invented plant baseline or historical-hours assertion |
| source | Source object | Winning rule |
| fallback | Source array, 0..2 | Lower applicable rules in priority order; explanation, no partial merge |
| editable | boolean | Server date/activation/scope eligibility observation, not a permanent grant |

Source object: kind LineException/PlantException/Weekly/None; revisionId UUID
or null; effectiveFrom date or null; reason string or null. Exception date uses
DaySummary.date, effectiveFrom null; weekly has effectiveFrom and null reason;
None has all nullable fields null. Source IDs identify immutable snapshots.
Before activation state Unavailable, hours null, source None and fallback [].

### WeeklySnapshot and ExceptionSnapshot

| Object / property | Type | Stored mapping / notes |
| --- | --- | --- |
| Both.id | UUID | Immutable revision identity |
| Both.commitRevision | version string | Recorded aggregate revision |
| Both.isCurrent | boolean | Latest snapshot for logical target |
| Both.createdAtUtc | UTC ISO8601 string | Immutable recording instant |
| Weekly.effectiveFrom | date | Immutable logical start |
| Weekly.workingDays | weekday string array or null | Mon/Tue/Wed/Thu/Fri/Sat/Sun in weekday order; withdrawn null; all closed [] |
| Weekly.isWithdrawn | boolean | Retained withdrawal marker |
| Exception.lineId | UUID or null | Plant/line scope |
| Exception.date | date | Immutable logical date |
| Exception.isWorking | boolean or null | Removed null |
| Exception.workingHours | decimal string or null | Explicit hours; working inheritance or closed null |
| Exception.reason | string or null | Normalized plain text; removed null |
| Exception.isRemoved | boolean | Retained removal marker |

Page metadata: page integer, pageSize integer fixed by operation, totalCount
integer >=0, totalPages integer >=0 (empty 0), items array bounded by pageSize.
No truncation of long-standing history: page explicitly. Requested page beyond
last yields items [] with actual totals. Weekly/history pages include context
and snapshotVersion string or null; unactivated has null and zero records.

### MutationResult and target

Properties: context CalendarContext; changed boolean; target WeeklySnapshot or
ExceptionSnapshot matching the called operation. Known success only after commit.
No-op after authoritative version/permission/target validation returns changed=false,
unchanged revision and current target. Changed operation returns appended snapshot
and new revision. No capacity/month cache in this response; UI refresh is separate.
If refresh fails after known success, retain success notice with a read error;
do not reinterpret confirmed commit as unknown.

## 5. Endpoint contracts

### API-PC-01 — GET /month

Request query: month required month string; lineId optional UUID. No other keys.
Return MonthResult {context, scope, month, days}, days in date order for exactly
the requested month's real 28..31 dates. DaySummary per day; no padded adjacent
month cells returned. Missing activation gives unavailable days with 200.
Unknown line 404; retired selected line is readable. Common validation/errors.

### API-PC-02 — GET /day

Query: date required; lineId optional. Return DayResult {context, scope, day,
exception}, day DaySummary; exception current ExceptionSnapshot for exact scope/
date or null if never configured, including a removed marker for safe re-add.
Provides target id + global version for the editor and verification. Past remains
read-only; inherited hours describe current setting, not historical capacity.
Unknown line 404. No separate permission to expose another calendar.

### API-PC-03 — GET /weekly

Query: from/to required dates, span<=366; view optional Current (default) or History;
page optional; snapshotVersion conditional as common rule. Return WeeklyPage
{context, snapshotVersion, page, pageSize, totalCount, totalPages, items,
applicableBeforeFrom}. Current items are latest snapshots with start inside range,
including withdrawal markers; History includes retained revisions for starts in
range. Order effectiveFrom DESC, commitRevision DESC, id ASC. applicableBeforeFrom
is latest nonwithdrawn current WeeklySnapshot strictly before from, else null;
it explains predecessor coverage without pretending the range has no rule.
200 empty/unactivated; no hidden all-future/unbounded history response.

### API-PC-04 — PUT /weekly/{effectiveFrom}

Route required date, no query. Required JSON properties: version string;
targetRevisionId UUID or null; workingDays array of distinct weekday strings,
0..7 entries. All properties required; [] is valid all closed. New logical date
uses null target only if no latest snapshot exists; re-add after withdrawal sends
that marker id; correction sends latest id. Server maps days to weekday mask.
Today replaces only today's start; older existing start not rewritten. Future
correction allowed. Return MutationResult/WeeklySnapshot, including no-op rule.
400 invalid fields, 404 unknown target, 409 stale/date/target mismatch, 503 known
busy rollback; uncertainty as section 7. No client isCurrent/commitRevision fields.

### API-PC-05 — POST /weekly/{effectiveFrom}/withdraw

Route required date, no query. Body exactly {version, targetRevisionId}, both
nonnull strings. Current target must match route, not withdrawn, and start>today.
Initial activation protected. Appends withdrawal marker, returns MutationResult.
Already withdrawn is 409 CALENDAR_TARGET_CHANGED, not a false successful delete.
No bulk withdrawal or physical DELETE.

### API-PC-06 — PUT /exceptions

No query. Required body fields: version; targetRevisionId UUID or null; lineId
UUID or null; date; isWorking boolean; workingHours decimal string or null;
reason nullable string. All properties required. Working null hours means line
inheritance; explicit working uses exact positive <=24 hours; closed requires
hours null. isRemoved is not accepted in save. Never-configured target uses null
id; re-add uses latest removal marker id; edit latest snapshot id.
Logical scope/date immutable for edit; cannot use an id from another scope/date.
Line create/edit requires active line; plant has no invented line requirement.
Return MutationResult/ExceptionSnapshot. Unknown line/target 404, retired/stale/
past/identity conflict 409, field errors 400; no automatic overwrite or conversion.

### API-PC-07 — POST /exceptions/remove

No query. Body exactly {version, targetRevisionId, lineId, date}; target id
nonnull; lineId nullable. Exact current nonremoved exception, today/future.
Retired referenced line allowed only for this existing removal. Appends marker;
response MutationResult. Removing absent/already-removed/mismatched target does
not silently succeed: 404 unknown id, 409 target changed. Lower rule visible on
subsequent read; full original snapshots remain in history.

### API-PC-08 — GET /line-choices

Query: q optional; page optional. Return LineChoicePage {context, page, pageSize,
totalCount, totalPages, items}. Active lines only; retained retired scope resolves
through month/day instead of appearing as selectable new use. No line creation
or edit endpoint added. Unactivated calendar still permits master choices, with
context activation/version null; timezone mismatch remains feature unavailable.

### API-PC-09 — GET /product-choices

Query: lineId required UUID; q/page optional. Return ProductChoicePage with same
page shape. Current active line/product/pair and matching unit/unitRevision only.
Unknown line 404; retired line returns 200 empty items. No pair reconfirmation,
coefficient edit or unit conversion. Search SKU/name literal; stable SKU,id order.

### API-PC-10 — GET /capacity

Query: lineId/productId/date required, no other keys. Past date 400 validation;
unknown line/product 404. Return CapacityResult {context, line, product, date,
availability, unavailableReason, day, minutesPerUnit, quantity}. line LineChoice;
product ProductChoice. availability Available/Unavailable; unavailableReason
None/NotActivated/LineRetired/ProductRetired/PairMissing/PairRetired/UnitStale/
CalendarUnavailable; quantity decimal string or null; minutesPerUnit decimal
string or null. day DaySummary; no quantity total/history/order allocation.

Unavailable priority: NotActivated, LineRetired, ProductRetired, PairMissing,
PairRetired, UnitStale, CalendarUnavailable. Available has reason None and valid
minutesPerUnit; quantity=floor(hours*60/minutes) for discrete units, floor at
3 decimals for kg/m. Valid closed pair quantity "0"; invalid pair unavailable
even if closed. For Unavailable quantity null; minutesPerUnit null so invalid
coefficients are not presented as usable. Consistent current snapshot only.

### API-PC-11 — GET /exception-history

Query: date required; lineId optional; page optional; snapshotVersion conditional.
Return ExceptionPage {context, scope, date, snapshotVersion, page, pageSize,
totalCount, totalPages, items, current}. Items immutable ExceptionSnapshot in
commitRevision DESC,id ASC order, including markers; current latest snapshot
or null independently of page. Past/retired history readable, no capacity claim.
Unknown line 404. Used for observation after unknown commit, not success proof.

## 6. Errors, message mapping and outcome certainty

Application error follows [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457.html):
type absolute URN, status actual HTTP integer, title safe generic English,
code stable catalog code, traceId diagnostic string and optional errors field
-> array of catalog codes. No raw input, stack/SQL/detail body. Frontend maps
codes to centralized Japanese strings; it never displays arbitrary server HTML.
Type is `urn:pmai:problem:plant-calendar:<lowercase-hyphenated-code>`.
Write failures additionally carry writeOutcome NotApplied or Unknown; missing/
invalid outcome, generic 5xx, network loss or cancellation is treated as Unknown.
No write success can be inferred from an observed equal payload.

| Code | HTTP | Meaning / write certainty |
| --- | --- | --- |
| VALIDATION | 400 | Strict field/date/precision error; NotApplied |
| REQUEST_TOO_LARGE | 413 | Body >8 KiB; NotApplied |
| UNSUPPORTED_MEDIA_TYPE | 415 | JSON required; NotApplied |
| NOT_FOUND | 404 | Unknown required identity; NotApplied |
| CALENDAR_STALE | 409 | Aggregate/snapshotVersion mismatch; NotApplied |
| CALENDAR_TARGET_CHANGED | 409 | Latest id, scope/date or removed/withdrawn target mismatch; NotApplied |
| CALENDAR_PAST_DATE | 409 | Mutation date now past; NotApplied |
| CALENDAR_LINE_RETIRED | 409 | New/edit line exception unavailable; NotApplied |
| CALENDAR_PROTECTED_RULE | 409 | Initial withdrawal or invalid old-start correction; NotApplied |
| CALENDAR_NOT_ACTIVATED | 409 | Write attempted before explicit activation; NotApplied |
| CALENDAR_BUSY | 503 | Confirmed rollback lock/deadline/deadlock failure; NotApplied, Retry-After: 1 |
| CALENDAR_TIMEZONE_MISMATCH | 503 | Stored/configured date basis differs; NotApplied |
| CALENDAR_WRITE_UNKNOWN | 503 | Commit confirmation lost; Unknown, no Retry-After |
| UNEXPECTED | 500 | Safe generic unexpected failure; Unknown for writes |

Auth 401/403 retain established empty bodies; middleware/proxy failures may not
follow feature Problem Details. Client handles these separately. Read transient
errors allow explicit retry. NotApplied allows user correction/manual retry;
never automatic mutation retry. An abort is not proof the transaction rolled back.
After Unknown, use day/weekly/history reads, keep original input and observed
revision, block replay. Verification shows current observation; explicit draft
discard/accept current state starts a new baseline, never claims old save success.
No idempotency ledger, request completion endpoint or exactly-once promise.
DD-FN must distinguish known rollback from uncertain commit reliably.

Suggested Japanese catalog phrases (new, not claimed implemented):
「入力内容を確認してください。」 (Check the input.),
「カレンダーが更新されています。再読込してください。」 (Calendar changed; reload.),
「保存結果を確認できません。再送せず現在の状態を確認してください。」
(Save result is unknown; inspect current state without resending.).
Exact full catalog/focus sequence belongs to SPD; codes defined here remain stable.

## 7. Instrumentation, security and consistency

Telemetry registration: ActivitySource/Meter ProductionManagementAI.PlantCalendar.
Span names `plant-calendar.<handler>` and child resolve/read/write from DD-FN;
ASP.NET Core request span route templates only, no raw query/body. Metric
plant_calendar_operations_total and plant_calendar_duration_ms with bounded
operation = eleven registry handler names; outcome = success/validation/conflict/
not_found/unavailable/busy/unknown/unauthorized/forbidden/unexpected/cancelled.
Count one final outcome per request including parser/auth errors; avoid controller/
use-case double count. Capacity domain Unavailable is HTTP 200 with unavailable
outcome. No raw dates/IDs/search/notes/coefficients/tokens/cookies in labels or logs.
DD-FN specifies internal spans and measurements; implementation must register both
source and meter. No external exporter configuration claimed.

Reuse existing cookie/JSON/same-origin posture; no CORS relaxation or new identity
provider. Parameterized SQL and strict bounded parsing; notes render text only.
Authorization precedes target-specific responses. Owner activation/DDL never
available through API. Snapshot writes honor column grants and line-before-state
locks. Calendar reads do not change existing session health or order rules.

| Requirement | Contract / later verification |
| --- | --- |
| REQ-070 | Month/day exact dates, unavailable coverage, scope and bounded snapshots |
| REQ-071 | Weekday transport, start identity, paged history, future withdrawal |
| REQ-072 | Strict exception fields, null inheritance, markers and removal confirmation target |
| REQ-073 | Eligible choices and unit-aware current capacity; unavailable vs valid zero |
| REQ-074 | Roles, limits, errors, versions, unknown certainty and bounded telemetry |
| REQ-075 | No changes to orders, starts, due dates or dashboard date windows |

Technical choices submitted for API review: eleven routes, schema/enums,
8 KiB/depth/page/span/time budgets, error outcomes and telemetry names. No open
business blocker. SQL/use-case proof and API runtime tests not run. Full DB/FN/SPD
consistency gate remains later. Submit this source and EN/JA PDFs and wait for
approval before writing DD-FN. Approved designs unchanged.
