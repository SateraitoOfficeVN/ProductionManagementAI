# WI-009 — Design consistency and security review

2026-10-01. Approved revision 1 step 11; review of approved design artifacts,
not a code/merge/deployment approval. Baseline 20c8d61, branch
feature/WI-009-production-lines. Runtime implementation review remains pending.

## Revision 1 historical design assessment

## Scope and verification actually performed

Read the complete 005 requirements/BD/DB/DD/API/FN/SPD/ORD family and existing
order/product/UI/auth patterns. git diff --name-only for src/tests/deploy/.github
returned no changes. All eight current document EN/JA PDF pairs, local links,
four DD-family members, main-DD 17 and ORD 6 distinct diagram edges, and all
SVG XML validated. The prior 143 captured source/PDF/visual hashes remain exact.
No application code/test, migration, dependency scan or runtime security test ran.

## Design-consistency checklist

| Checklist item | Result / evidence |
| --- | --- |
| Stable requirements/acceptance | Pass — 005_REQ section 3, REQ-064–069 mapped to TC-326–365 in test-plan.md |
| BD navigation/actions/exceptions | Pass — 005_BD screen/transition tables; three routes retained |
| BD diagram and numbered legend | Pass — existing rendered artifacts/region checks recorded in evidence; all numbers 1–9 mapped |
| DD fields/validation/states match BD | Pass — code immutable; hours/minutes exact; retirement staged versus immediate; history/start distinction consistent |
| DD diagram/transition and regions | Pass — main-DD 8 list + 9 form edges; ORD 6 picker edges; SPD 1–9 and ORD local 1–7 mapped |
| API/DB consistency | Pass — opaque xmin/revision strings, exact decimals, nullable composite FK, seven feature endpoints, parent update and ordered locks |
| Missing decisions | Pass — DEC-001–010 resolved; no new business rule needed |
| Test mapping | Pass for planning — forty scenario groups trace every requirement; all runtime cases not run |
| Auth/PII/secrets identified | Pass for design — Admin/Operator, same-origin cookie; names/search treated as untrusted business text; no credential fields or body logging |
| Accessibility | Pass for design — Japanese catalog, semantic table/cards, field focus, keyboard/zoom/320px/native dialog obligations; conformance unverified |
| Migration/recovery | Pass for design — 005_DB section 10; two stages, owner DDL, isolated fresh/upgrade tests, no unsafe Down or mixed old/new writers |
| Telemetry | Pass for design — API section 8/FN section 8; ActivitySource/Meter, bounded labels, ValidateOrder child span and no double request count |
| EN/JA PDFs current | Pass — 8 pairs; API version 2, others version 1; every page's source footer checked |

## Security checklist — design disposition

| Item | Disposition | Evidence / concrete implementation obligation |
| --- | --- | --- |
| Authentication/authorization on every endpoint | Pass, design — API:34–36/operation catalog; ORD field rules. Seven feature operations and existing orders use ProductionOrderEditor. Test TC-346 / direct UI routes. |
| Validate external input / parameterized access | Pass, design — API:40–44 and numeric/schema contracts; FN:74–142 locks/queries. Strict new-feature JSON/query only, exact decimals/opaque versions; do not globally tighten old order requests. TC-326/328/347/349. |
| Secrets and runtime mechanism | Pass, current artifact diff — no new credentials or production configuration; runtime mechanism remains existing. Never record environment values. |
| Trusted/vulnerability-checked new dependencies | Not applicable — no new/updated dependencies. Do not infer installed packages are vulnerability-free; adding one requires review. |
| Least-privilege roles/permissions | Pass, design — DB:264–279 restricted column grants, no runtime DDL/DELETE/unit_revision update; existing owner migrates only isolated fixtures. TC-357. |
| External content as data | Pass, design — line/product/search/notes text rendered escaped; parameterized SQL; no uploads/webhooks/new integration or executing fetched text. |
| No implementation-detail error leaks | Pass, design — API:253–285 and FN:289–312; fixed codes/paths, RFC9457 generic errors; existing order problem family retained. TC-334/347. |
| No sensitive logs/evidence | Pass, design/current records — API/FN bounded metrics and redaction of raw queries/ids/names/coefficients/version/cookies/body. Telemetry runtime assertions pending TC-334/355. |
| New authentication/payment/PII boundary threat modeled | Not applicable to new boundary — no new provider/payment/PII pipeline. Existing boundary assessed below for new mutations; review again against implementation. |

## Concrete threats and controls

| Threat / failure scenario | Design control / later verification |
| --- | --- |
| Authenticated non-editor forges mutation or calls hidden route | Server policy before validation; UI direct-route gate; TC-346 |
| Cross-site browser submits form to change a line | Existing HttpOnly/SameSite=Lax cookie posture and JSON-only mutations, no new CORS/form handling; TC-347 verifies rejection. No claim of a new anti-forgery token mechanism. |
| Forged key/confirmation/code/version or SQL wildcard payload | Strict feature field allowlist, server-owned confirmed revision, immutable columns and bound parameters; TC-327/329/347/357 |
| Retire/unit edit races order start or confirmation | Product→line→pair locks held to commit, observed revision and original order xmin; TC-350/354 |
| Client retries after response lost but commit succeeded | Explicit unknown state/read reconciliation, no automatic write replay or compensating delete; TC-333/341/344/364 |
| Query/body resource exhaustion | 100-code-point q, bounded paging/body/actions and 5/10/15-second budgets; TC-326/340/347/355. No invented global rate limit. |
| Business names/search/credentials leak into trace or error | Safe generic errors and redacted attributes/bounded metrics; TC-334/347; implementation diff recheck required |

## Findings and gate limits

No unresolved blocking design finding identified in this comparison. Earlier
snapshot descriptions such as "later DD" inside approved documents describe
that document's original review phase; current work-item records and approved
new companions resolve ownership. Completed sources remain immutable.
No name uniqueness beyond the approved code unique constraint is introduced.
Design gate permits proposing implementation; it does not prove implementation
security. Re-run the full security checklist against code/test/migration diff
and actual results before reporting application delivery or authorized merge.
Release-readiness is not applicable: no deployment authorized.

## Requirement-to-design traceability

| Requirement | Primary design owners | Test groups |
| --- | --- | --- |
| REQ-064 | BD list; main-DD/SPD URL and reads; API/FN snapshot/paging | TC-326,335,356,360,365 |
| REQ-065 | BD identity; DB normalized index/xmin; API/FN writes; SPD form | TC-327,336,348,360,365 |
| REQ-066 | DB exact numbers/unit trigger; API/FN confirmation; SPD editor | TC-328,329,337–339,340,349–351,360,365 |
| REQ-067 | DB RESTRICT/history; API/FN retirement; SPD dialogs; ORD history | TC-330,340,352,361,365 |
| REQ-068 | DB nullable composite FK; API/FN order integration; ORD | TC-331,332,342,343,345,353,354,358,362,365 |
| REQ-069 | API roles/errors/bounds; FN telemetry/deadlines; SPD/ORD accessible UI | TC-333,334,341,344,346,347,355,357,359,363–365 |

## Immutable source hashes

| Document | SHA-256 |
| --- | --- |
| 005_BD | `9ff444e228293ae5e9a0fd5bc73204c8e9440c0639c54a18c04fe3ab5b4a4c28` |
| 005_DB | `856e05827f2dae352a7403f65beecbb575aec86bb932f6006b7975c0b48b1a93` |
| 005_DD | `af4dd7ab27574068c40d8f300e6e0f6ed4a534eb5757f20c4881f2f79f639d4f` |
| 005_DD-API | `8b27005ed4f31500839512ce6c9c4b35dc69ac68bd14a877411d872ad0f51b30` |
| 005_DD-FN | `ab397310e9e19b067b69a45f4ce5f8952da9b3c91a1be2ab315e84a21c62e809` |
| 005_DD-ORD | `0b06cb38263959401fc8c83490bb3b3970bae0795d7f3214fb9e0688891592fa` |
| 005_DD-SPD | `e5c1121bdcf28616d4b0d7a85ca10cf6a7efd3aedcab71d09f6eddcdc9dc5845` |
| 005_REQ | `f4a714325c2049a237061a9e86c43eab63cc306a39585279b2575c7aa0bb4f3d` |


## Revision 2 implementation review — 2026-10-01

Agent review of the local branch, not user runtime acceptance or merge approval.
All eight revision 2 steps have implementations and executed evidence; source
and companion designs remain immutable. Review includes new files as well as
tracked diffs. Final source verification: 192 unit / 157 integration / 196
frontend / 42 E2E pass; model consistency and protected artifact hashes pass.

### Resolved findings

| Finding | Resolution and actual verification |
| --- | --- |
| EF-wrapped lock timeout mapped to 500 | ProductionLineRepository.cs:127 recognizes only actual inner PostgreSQL SQLSTATE evidence; real timeout returns 503 only after rollback |
| Native dialog Tab left modal | LineDialog.tsx:26 contains forward/reverse Tab and restores safe focus; browser journeys and native 200% zoom check pass |
| Cached unit observation or coefficient change retained old confirmation | ProductionLineFormPage.tsx:82 refreshes observations and clears intent; cross-page/ABA/coefficient tests pass |
| Missing order line metadata fabricated unassigned history | production-orders/api.ts:43 rejects missing/malformed projection; null/retired/quantity/list regression pass |
| Unknown retirement dialog prevented read verification | Cancel stays available while confirm stays blocked; committed-response-loss E2E confirms one write and read reconciliation |
| Failed rollback could restart explicit cleanup | ProductionLineRepository.cs:201/209 records attempt and discards failed context; real HTTP cancellation and timeout checks plus final full backend pass |

### Security-review checklist

| Checklist item | Result / evidence |
| --- | --- |
| Agreed authentication/authorization at each endpoint | Pass: controller policy at ProductionLinesController.cs:11; seven-route 401/403/no-store matrix; real Admin/Operator operations; UI role tests |
| Validate external input; no executable unvalidated interpolation | Pass: strict feature-owned JSON/query/media/size parser, lexical rules and parameterized EF/FromSqlInterpolated/escaped LIKE; strict boundary and numeric tests |
| No real credential/token/key in change or evidence | Pass: random test-stack credentials only in temporary env; source/diff scan returned existing placeholder and an unrelated textual regex match, no real secrets; local traces disabled |
| Dependency provenance/vulnerability review | Not applicable to a new dependency: manifests, lockfiles and CI actions unchanged; existing locked restores previously reported zero npm vulnerabilities; no new scanner claim |
| Least-privilege credentials/roles/workflow permissions | Pass: existing pmai_app and roles only, restricted grants exercised; no new workflow permissions or runtime DDL |
| External content treated as data | Pass: no new external-content ingestion or executable command construction from application inputs; tool output used only as evidence |
| Safe generic errors | Pass: LineProblems allowlisted types/codes/paths, no SQL/exception body; strict response tests and commit ambiguity checks |
| Sensitive diagnostics suppressed | Pass: bounded operation/outcome labels and traceId logs, no request/exception objects, no EF sensitive logging; actual meter/span capture excludes marker search values |
| Threat assessment before new trust-boundary implementation | Pass: revision 1 threat assessment above; existing cookie/same-origin/JSON posture retained; no payment/PII/provider boundary added |

### Design-consistency and delivery checklist

| Gate item | Result |
| --- | --- |
| Stable requirements / approved scope / plan source | Pass: REQ-064–069, DEC-001–010 and explicitly approved revision 2 |
| BD/DD/API/DB and code agree | Pass: string decimals/tokens, unit generations, durable retirements, scoped snapshots/lock ordering, nullable presence-aware order assignment and original status/xmin semantics reviewed |
| Design diagrams/legends/accessibility/migration/instrumentation | Pass: original design gate retained; runtime implementation/test comparison completed; 143+8 hashes unchanged |
| Required verification and case results | Pass: TC-326–365 disposition and real full-suite results; earlier failures and corrections retained |
| Findings / limits explicit | Pass: resolved findings above; axe is not exhaustive WCAG proof; OTLP external export and live release not run |
| Authorized external operations only | Pass: no commit/push/PR/merge/deployment; disposable fixtures only; main/demo resources preserved |
| Current records / handover | Pass: local delivery status, evidence, test dispositions, plan outcomes and owner runbook updated |
| Current English/Japanese PDF companions | Pass: approved 005 family unchanged and current companions preserved by hashes; no design document modified in implementation |
| Root README / ai/project / CLAUDE state | Updated to identify verified local WI-009 working-tree delivery, pending commit/PR authorization; no merged or released feature claim |
| Secrets / untrusted content / skipped or flaky checks | Pass: no real secrets; no skipped tests or automatic retries; telemetry listener quarantine isolated process-wide capture and final reruns passed |
| New third-party CI action/dependency pinned | Not applicable: none introduced |

Security and design-consistency gates pass for local delivery. Delivery gate
passes on the recorded final evidence and housekeeping completion. Release-
readiness is not applicable without deployment authorization. No unresolved
blocking code finding remains; no approval of a future external action inferred.


## Revision 3 pre-publication review

Authorized staged scope reviewed: 123 files; baseline unchanged; protected hashes
143+8 match; no generated evidence videos or temporary files; staged whitespace
check passes. Prior implementation security/design/delivery gates remain valid,
with no new runtime change in this stage. Commit/push/PR/CI now explicitly
authorized by revision 3; merge and deployment still excluded.


## Published review handoff

[PR #34](https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/34) publishes implementation commit 810ea06. Remote Backend/Frontend
checks passed; E2E and final record-only head checks still pending at this snapshot.
No new implementation finding. Final check results will be recorded in the PR
body/checks on the exact SHA before handoff. Merge/deployment not authorized.
