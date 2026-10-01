# Production lines — API specification

005_DD-API version 2 supports SCR-005, FN-032–FN-036 and REQ-064–REQ-069.
WI-009 approved plan revision 1, step 7; contracts only, not implemented.

## 1. Document control and references

| Field | Value |
| --- | --- |
| Document ID / version | 005_DD-API / 2 |
| System / subsystem | ProductionManagementAI / Master data |
| Work item / author | WI-009 / Agent |
| Created / updated | 2026-10-01 / 2026-10-01 |
| State | Submitted for review; DEC-010 resolved; not implemented |

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1 | 2026-10-01 | Agent | Initial API contracts, errors, concurrency and telemetry |
| 2 | 2026-10-01 | Agent | Finalize Admin/Operator read authorization after DEC-010 confirmation |

Inputs: approved [005_BD](../../010_basic-design/005/005_BD_生産ライン・工程.md),
[005_DB](../../database/005/005_DB_生産ライン・工程.md) and
[main 005_DD](005_DD_生産ライン・工程.md).
[Requirements](../../000_requirements/005/005_REQ_production-lines.md),
[decisions](../../../../work-items/WI-009/decisions.md) and
[plan](../../../../work-items/WI-009/plan.md) define scope and review stops.
No approved design is edited. FN/SPD and the new order-impact DD are reserved
later documents, not existing links or completed specifications.

## 2. Transport, authorization and common bounds

| Concern | Contract |
| --- | --- |
| Authentication | Existing same-origin Identity cookie; no new CORS/provider/role. Missing session returns established empty-body 401, not HTML redirect |
| Read policy | Admin or Operator for all seven API-PL operations, as confirmed in DEC-010; reuse ProductionOrderEditor policy, matching existing Product master access. Other authenticated roles receive 403; no new role or broader read entitlement |
| Write policy | Admin or Operator, server checked; UI visibility is not authority. Empty-body 403 retains existing convention |
| Representation | UTF-8 application/json; application/problem+json for application failures; no-store for successful reads, writes and application errors |
| Mutation | JSON object only, application/json (optional charset=utf-8); reject other media types with 415; no form/query mutation |
| Identity | Route/query/body UUID strings, nonzero; malformed UUID 400, valid unknown target 404; bind without a guid route constraint so malformed targets do not become accidental 404 |
| Strict fields | New feature requests reject unknown/duplicate JSON member names, wrong scalar types, null required fields and unsupported actions with 400; existing API contracts are not globally tightened |
| Text | Trim both ends; Unicode code-point count after normalization. No extra code alphabet restriction. DB lower(code) comparison governs duplicates in all states |
| Request limits | New mutation body <=256 KiB; changes <=1000 per request; these are technical request limits, not a cap on persisted associations. Oversize 413; invalid action count 400 |
| Query limits | q optional, trim, <=100 code points; page ASCII integer 1–10000, default 1; pageSize fixed 50; unknown/repeated query keys 400 |
| List ordering | Line code ASC then UUID ASC, using DB collation; no user-defined SQL sort identifiers |

### Exact numeric and version representation

New line APIs send hours and minutes as JSON strings. Request decimal syntax is
`^(0|[1-9][0-9]*)(\.[0-9]{1,3})?$`; no whitespace, signs, exponent, grouping,
NaN/infinity or rounding. Hours: 0 < value <=24. Minutes/unit: 0 < value
<=999999999.999. Server parses invariant .NET decimal after lexical validation.
Response is canonical non-exponent decimal text with unnecessary trailing zeros
removed, e.g. "7.5", "0.001". New contracts use no JSON floating-point numbers
for these fields; existing order quantity JSON-number contracts stay unchanged.

Line version is a canonical unsigned-decimal string for parent xmin (0–4294967295),
not an incrementable client counter. Product unitRevision is a canonical string
for bigint 0–9223372036854775807. Reject leading zeros except "0", numeric tokens
and out-of-range values. Both are opaque to UI comparisons except exact equality.
A unit revision is not the Product master edit version. Existing product/order
numeric versions retain their current contract.

## 3. Operation catalog and registry

All proposed handlers belong to ProductionLinesController, except the existing
order controller. Each has Agent / 2026-10-01 as creation/update metadata. API
IDs are local registry entries, not duplicated IDs for existing order routes.

| ID | Method / route | Handler / Application responsibility | Return |
| --- | --- | --- | --- |
| API-PL-01 | GET /api/production-lines | List / FN-032 | LinePage, 200 |
| API-PL-02 | GET /api/production-lines/{id} | Get / FN-033/034 | LineDetail, 200 |
| API-PL-03 | POST /api/production-lines | Create / FN-033/034 | LineDetail, 201 |
| API-PL-04 | PUT /api/production-lines/{id} | Update / FN-033/034 | LineDetail, 200 |
| API-PL-05 | POST /api/production-lines/{id}/retire | Retire / FN-035 | LineSummary, 200 |
| API-PL-06 | GET /api/production-lines/product-choices | ProductChoices / FN-034 | ProductChoicePage, 200 |
| API-PL-07 | GET /api/production-lines/eligible | Eligible / FN-036 | EligibleLinePage, 200 |
| API-PO-01–04 | Existing production-order create/get/update/list | Existing controller, FN-036 extension | Existing response plus nullable line |

Literal choice/eligible routes precede the parameter route in API descriptions;
route resolution tests must verify that they are not treated as line IDs. Full
Application algorithms and client processing live in later FN/SPD documents.

### Shared handler boundary

| Step | Responsibility | Calls / owner |
| --- | --- | --- |
| 1 | Authenticate/authorize, validate route/query/media/body and exact scalar syntax | Existing session policy; API validation |
| 2 | Call the scoped use case with cancellation/deadline | FN-032–FN-036 in later 005_DD-FN |
| 3 | Map known validation/conflict/not-found/transient outcome, no SQL detail | This API's error catalog |
| 4 | Return committed result/read projection and bounded operation telemetry | Representation below; FN instrumentation |

## 4. Response field catalog

Fields are required unless explicitly nullable. Do not invent missing rows or
units after integrity errors. UTC timestamps are ISO-8601 strings with Z.

| Representation / field | JSON type / bound | Source / value mapping |
| --- | --- | --- |
| LineSummary.id | UUID string | production_lines.id |
| LineSummary.code / name | string <=50 / <=200 code points | Stored immutable code / current name |
| LineSummary.workingHoursPerDay | decimal string | Exact hours/day, visible UI unit 「時間」 (hours) |
| LineSummary.isActive | boolean | true/false -> 「使用中」 / 「使用停止」 (Active / Retired) |
| LineSummary.updatedAt / version | UTC string / opaque string <=10 | updated_at_utc / parent xmin |
| LinePage.items / total / page / pageSize | array <=50 / integer / integer / 50 | Filtered count and requested page; count must be JS-safe integer |
| LineDetail | LineSummary fields plus pairs | Parent and page read from one snapshot |
| LineDetail.pairs | {items, total, page, pageSize} | All active/retired pairs; fixed 50, sorted product SKU then UUID; fields as PairDetail |
| PairDetail.product | {id, sku, name, unit, unitRevision, isActive} | Current product projection; sku/name <=50/200; unit one of seven; revision opaque <=19 |
| PairDetail.minutesPerUnit | decimal string | Current coefficient; no history recomputation |
| PairDetail.confirmedUnit / confirmedUnitRevision | unit string / opaque string <=19 | Persisted confirmation values |
| PairDetail.isActive / requiresUnitConfirmation | boolean / boolean | Pair state; mismatch of confirmed/current unit OR revision |
| PairDetail.updatedAt | UTC string | Pair update audit time; no independent editor version |
| ProductChoicePage | {items, total, page, pageSize} | Paged ProductChoice array, same current product fields as above |
| EligibleLinePage | {product, items, total, page, pageSize} | Product observation plus eligible summaries; items add minutesPerUnit and unit |

Unit set is exactly `個`, `本`, `枚`, `台`, `セット`, `kg`, `m`. Unit mismatches
remain readable even when an active pair is ineligible. No daily quantity,
capacity total, scheduling estimate, historical duration or credential field.

## 5. Endpoint contracts

### 5.1 API-PL-01 — List

Arguments: query q, state, page. state is active (default), retired or all;
unknown/case-variant value is 400. q searches literal code/name substrings,
case-insensitive under DB comparison; escape LIKE wildcard characters rather
than treating user text as SQL/pattern syntax. No extra sort/pageSize parameter.
200 LinePage; page beyond available results returns empty items with actual total.
Read count and rows in one read-only REPEATABLE READ snapshot. No lock reservation.
Errors: 400, 401/403, 503 or generic 500; no fabricated fallback page.

### 5.2 API-PL-02 — Detail

Arguments: route id, query pairsPage (default 1, 1–10000). 200 LineDetail;
404 if target absent. Include retired lines/pairs. Read parent, pair count/page
and current products in one read-only REPEATABLE READ snapshot to avoid mixed
parent/pair versions. Pair pagination does not mutate or omit history from storage.
Client loading another page compares returned parent version with its baseline;
a changed version requires a guarded reload, not a silent merge. Product unit
changes can occur without changing parent xmin, so every timing intent also
carries its observed unit revision. Errors: 400, 401/403, 404, 503, 500.

### 5.3 API-PL-03 — Create

No route/query arguments. Body CreateLineRequest:

| Field | Type / required | Bound / meaning / example |
| --- | --- | --- |
| code | string / yes | Trim 1–50 code points; "LINE-A"; unique including retired lines |
| name | string / yes | Trim 1–200 code points; Japanese permitted |
| workingHoursPerDay | decimal string / yes | "8", exact approved hour bound |
| products | array / yes, may be empty | <=1000 AddProductInput rows; no duplicate productId |
| products[i].productId | UUID string / yes | Existing active product; server verifies under lock |
| products[i].minutesPerUnit | decimal string / yes | "0.125", exact approved coefficient bound |
| products[i].expectedUnit | unit string / yes | Unit displayed to user |
| products[i].expectedUnitRevision | opaque string / yes | Revision displayed with that unit |
| products[i].confirmUnit | boolean true / yes | Explicit confirmation of coefficient for observed unit |

Server generates line ID/state/timestamps/version; body cannot set them. Save
line and all pairs atomically. Locked product must still match observed unit and
revision; do not silently confirm an unseen newer unit. Zero products is valid.
201 Location: /api/production-lines/{id}; LineDetail (pairs page 1). Read back the
committed projection; failure after commit is not proof of rollback. Errors:
400 structural/product validation, 409 duplicate code/unit race, 401/403,
413/415, 503 known rolled-back transient failure, 500 unexpected result.

### 5.4 API-PL-04 — Update aggregate

Arguments: route id; no query. Body UpdateLineRequest:

| Field | Type / required | Rule |
| --- | --- | --- |
| name | string / yes | Same trim/bound as create |
| workingHoursPerDay | decimal string / yes | Same hour bound |
| version | opaque string / yes | Last read parent version; stale gives 409 |
| productChanges | array / yes, may be empty | <=1000 actions, one per productId; omitted pairs unchanged, never deleted |
| productChanges[i].action | string / yes | Exactly add, setTiming or retire |
| productChanges[i].productId | UUID string / yes | Immutable saved identity or active product to add |

Action-specific fields (all other fields rejected):

| action | Additional fields | Semantics |
| --- | --- | --- |
| add | minutesPerUnit, expectedUnit, expectedUnitRevision, confirmUnit=true | Same rules as AddProductInput; saved retired pair cannot be re-added |
| setTiming | minutesPerUnit, expectedUnit, expectedUnitRevision, confirmUnit boolean | Saved active pair; observed unit/revision must match current product. If saved confirmation is stale, require true; false may edit current-unit timing without replacing confirmation |
| retire | None | Saved active pair becomes inactive; rows/orders retained. No timing edit or restore bundled into this action |

code, id, isActive, timestamps and confirmed fields are not mutable payload
members. Retired-line name/hours corrections retain inactive state. Retired pair
is read-only. New association requires active product; existing timing edits do
not reactivate inactive products. Plain name/hour edits may leave stale pairs
unchanged. Parent/pair operations form one transaction; any failure rolls back
all requested changes. Parent xmin must change even for pair-only mutations.
200 LineDetail (page 1), new parent version. Errors: 400 malformed/immutable/
duplicate actions/invalid product/pair operation; 404 line/pair absent; 409 stale
parent or unit, confirmation required; 401/403, 413/415, 503, 500. Compare parent
version before reporting business conflicts, after globally ordered lock acquisition.

### 5.5 API-PL-05 — Retire line

Arguments: route id; body only {"version":"81234"}. Current active line with
matching parent version becomes inactive. No implicit pair deletion/retirement,
no order rewrite, no restore operation. 200 LineSummary with new version; 404
absent target, 409 stale version, 400 LINE_ALREADY_RETIRED on current-version
already-retired target. Other errors: 401/403, 413/415, 503, 500. Cancel is local
and must not call this endpoint. No automatic replay/idempotency-key promise.

### 5.6 API-PL-06 — Product choices

Arguments: optional q, page, lineId. lineId is current editing line; valid unknown
line gives 404. Return only active products, excluding every persisted pair for
that line including retired pairs. No lineId in create. SKU/name literal search;
sort SKU then UUID, page size 50. Client also excludes unsaved local product IDs.
200 ProductChoicePage, current unit/revision observation; rows are hints, not
write authorization. No extension or replacement of existing Product master
or GET /api/products contract. Errors: 400, 401/403, 404, 503, 500.

### 5.7 API-PL-07 — Eligible lines

Arguments: required productId; optional q/page. Unknown product 404. Existing
inactive product yields product observation and empty items, not eligible rows.
Require active line, product and pair, plus exact confirmed unit/revision match.
Filter code/name; stable line ordering, size 50. 200 EligibleLinePage from one
read-only snapshot. Selection is not reserved; order save/start rechecks under
FOR SHARE locks through commit. Historical assigned lines are displayed by order
responses, not smuggled into this eligible list. Errors: 400, 401/403, 404, 503, 500.

## 6. Existing-order contract extension

Existing API-PO-01–04 routes, numeric version, decimal JSON quantity and product
fields stay intact. This new document owns additions; no old approved design
is rewritten. Detailed order UI states/compatibility tests belong to step 10.

| Context / field | Addition and omission semantics |
| --- | --- |
| Create body lineId | Optional nullable UUID; missing/null means no assignment in Draft. Non-null requires eligible exact product/pair |
| Update body lineId | Optional nullable UUID; omitted preserves saved assignment, explicit null requests clearing. Outside Draft accept only exact unchanged value/null; other change rejected |
| Detail/list response line | Required nullable object {id, code, name, isActive}; null displays 「未設定」 (Unassigned). Values are current line master, not historical name snapshots |
| Draft -> InProgress | Require non-null currently eligible assignment even if field omitted or old retired assignment kept |
| Unchanged historical edit | Keep exact pair/null despite retirement; preserve existing product/quantity/status locks. No backfill for old non-Draft rows |
| Changed Draft product | Client clears/revalidates line; server rejects incompatible retained non-null pair. No automatic substitution |

Draft-origin assignment changes and the transition to InProgress are checked in
one transaction. Existing order authorization remains unchanged. Product active
and line/pair/unit eligibility checks use DB-approved product -> line -> pair
locks, not only FK enforcement. Errors add LINE_REQUIRED (400, lineId),
LINE_INELIGIBLE (400, lineId) and LINE_LOCKED (400, lineId); existing quantity,
status, product and order-version errors retain their established codes. Existing
clients omitting lineId may save Draft/unmodified history; they cannot start a
newly transitioned Draft without a valid line. Coordinated feature rollout and
write-pause/migration limits remain those of DB; no live deployment performed.

## 7. Error catalog and precedence

Application errors follow [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457.html).
Return type URI `urn:pmai:problem:production-lines:<suffix>`, fixed English title,
matching status, stable code and traceId; optional errors maps JSON paths to code
arrays. No detail/body/SQL/stack/observed product values. UI supplies Japanese
messages from the existing catalog; codes below are new proposals for review.
Malformed body uses errors.body; rows use productChanges[0].minutesPerUnit or
products[0].expectedUnitRevision. Do not echo attacker-controlled property names.

| Condition | HTTP / code | Type suffix / field / response |
| --- | --- | --- |
| Malformed query/body/scalar, overprecision, immutable payload | 400 VALIDATION | validation; stable field paths; retain input |
| New product absent/inactive | 400 LINE_PRODUCT_INVALID | product-invalid; affected productId |
| Duplicate saved association / invalid retired pair operation | 400 LINE_PAIR_INVALID | pair-invalid; affected productId/action |
| Retire already inactive line | 400 LINE_ALREADY_RETIRED | already-retired; no state change |
| Code unique-index race/duplicate | 409 LINE_CODE_CONFLICT | code-conflict; code |
| Parent version stale | 409 LINE_STALE | stale; version; guarded reload |
| Observed product unit/revision differs | 409 LINE_UNIT_STALE | unit-stale; expectedUnitRevision; explicit fresh confirmation |
| Stale saved confirmation and no explicit intent | 409 LINE_UNIT_CONFIRMATION_REQUIRED | unit-confirmation; confirmUnit |
| Line/pair/read product target not found | 404 NOT_FOUND | not-found; generic target message |
| Oversize body / non-JSON mutation | 413 REQUEST_TOO_LARGE / 415 UNSUPPORTED_MEDIA_TYPE | request-too-large / unsupported-media-type |
| Lock timeout/deadlock, rollback confirmed | 503 LINE_BUSY | busy; Retry-After: 1; user may retry known failure, no automatic mutation replay |
| Unexpected/integrity or unverified commit failure | 500 UNEXPECTED | internal; generic message, ambiguous write must be verified |
| Missing session / wrong role | 401 / 403, no application code | Established empty body; no login HTML |

Authentication/authorization runs first. Shape/bounds next; lock products in UUID
order before parent/pairs. After locks, validate target/version before unit/pair
business conflicts. Duplicate code at commit still maps named index 23505 to 409.
No partial-success/multi-status response. Read and write integrity failures are
not converted into invented empty data. Existing order errors remain their own
problem type family. Generic middleware fallback may use the existing internal
type/code; the feature boundary must not expose its exception detail.

## 8. Resource deadlines and telemetry

Proposed implementation settings, not currently configured behavior:

| Limit / policy | Contract |
| --- | --- |
| Feature use-case deadline | 15 seconds, linked to request cancellation |
| DB statement / lock waits | statement_timeout 10s; lock_timeout 5s, transaction-local for this feature; FN defines exact setup and disposal |
| Read snapshots | Read-only REPEATABLE READ per result, bounded deadline; do not retain across browser pages |
| Client deadline | 20s feature adapter; cancelled/timeout mutation has unknown outcome until verified. Abort does not prove rollback |
| Retry | Explicit read retry; write retry only after known failure/verification. No global EF retry strategy that replays ambiguous mutations |

PostgreSQL distinguishes statement and lock timeouts; use transaction-local
configuration, not a blanket change to existing endpoints. See
[connection-default documentation](https://www.postgresql.org/docs/17/runtime-config-client.html).
Deadlock/timeout 503 requires proven aborted/rolled-back transaction; if commit
may have succeeded, do not label it a safely retryable write. FN will detail this.

ActivitySource/Meter name: ProductionManagementAI.ProductionLines. Spans:
ProductionLine.List, .Get, .Create, .Update, .Retire, .ProductChoices, .Eligible;
FN-036 contributes bounded validation spans to existing order traces. Register
source/meter in existing OpenTelemetry pipeline, export only under its existing
configuration. Nothing is implemented by this document.

Metrics: pmai.production_lines.requests counter and
pmai.production_lines.duration seconds histogram. Labels only operation
(list/get/create/update/retire/product_choices/eligible/order_validation) and
outcome (success/validation/not_found/forbidden/conflict/busy/error/cancelled/unknown).
No raw paths, UUIDs, names, code/search, coefficients, cookies, versions or request
bodies in labels. Authorization failures are captured once at feature boundary;
known use-case outcomes once after classification, preventing double counting.
Structured failure logs carry operation, classified outcome and traceId only.
SQL parameter values are not enabled. FN defines exact instrumentation sites.

## 9. Persistence mapping and verification viewpoints

| Contract / requirement | DB mapping / later verification |
| --- | --- |
| Identity and hours, REQ-064/065/066 | production_lines, code lower index, exact numeric checks; query bounds, duplicates and decimal string binding |
| Timing actions and unit observation, REQ-066 | Composite pair PK, product unit_revision trigger; ABA, stale intent and pair-only parent xmin |
| Retirement/history, REQ-067 | RESTRICT FKs and no runtime DELETE; omission not deletion, Cancel no request, retired pair no re-add |
| Order assignment, REQ-068 | Nullable (line_id, product_id) FK; eligible start/lock serialization, legacy null and omitted-field compatibility |
| Access/telemetry, REQ-069 | Confirmed Admin/Operator read/write roles (DEC-010); per-column grants, no-store, body limits and bounded labels |

Later tests must include missing/wrong-typed tokens, "1.2340", "1e1", "0.001",
maximum boundaries, multiple changes where one fails, unit change/back-to-original,
retirement/start races, new and paged pair reads, route precedence, malicious
unknown fields and lost create/update responses. Test IDs are assigned at step 11.
No endpoint, SQL, timeout, migration or application test has been executed.

Unresolved business decisions: none; DEC-010 confirms Admin/Operator reads. Technical limits,
transport and action/error names are review proposals within approved scope.
FN/SPD/order-impact and final family/security consistency remain future work.
Submit this finalized version 2 API file and its current EN/JA PDFs; wait for explicit
review before authoring DD-FN. No implementation approval inferred.
