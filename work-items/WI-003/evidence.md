# Production Order List (Screen B) — Requirements Traceability & Evidence

As of working tree on `master` (WI-003 and Screen B design files untracked), 2026-09-22.

## Traceability matrix

Design phase only: no code and no tests exist yet, so "Code / PR" and "Test case ID" stay empty until plan revision 2. Test viewpoints per requirement are produced in DD-002 and become `TC-###` in the WI-003 test plan.

| Requirement ID | Requirement | Design artifact | Code / PR | Test case ID | Status |
| --- | --- | --- | --- | --- | --- |
| REQ-020 | Open the list and see existing orders | BD-002 system overview, business flow, §3 items 14/22/24/26, §6 E-10, success/exception flows; DB-003 access patterns, demo seed | — | — | designed (BD, DB) |
| REQ-021 | Row shows identifying and planning fields | BD-002 §3 items 16–22, §4 M-05–M-08, M-10; DB-003 read projection, overdue is computed not stored | — | — | designed (BD, DB) |
| REQ-022 | Filter by status, product, due-date range, order number | BD-002 0-3, §3 items 7–13, §5 V-09–V-12, §6 E-11/E-12; DB-003 page/count queries, trigram index (DEC-010) | — | — | designed (BD, DB) |
| REQ-023 | Sort by any listed column, both directions | BD-002 §3 items 16–21, §5 V-13, §6 E-13; DB-003 sort-key mapping and tie-breaker | — | — | designed (BD, DB) |
| REQ-024 | Server-side paging with user-chosen page size | BD-002 §3 items 14/15/23, §4 M-09, §5 V-13, §6 E-14/E-15; DB-003 offset paging and exact count | — | — | designed (BD, DB) |
| REQ-025 | Open an order in SCR-001, or start a new one | BD-002 screen transition, §3 items 5/22, §6 E-16/E-17 | — | — | designed (BD) |
| REQ-026 | Admin/Operator only | BD-002 0-1, actions and business rules, exception flows, non-functional security; DB-003 privileges (unchanged, SELECT only) | — | — | designed (BD, DB) |
| REQ-027 | View state kept in the URL | BD-002 0-3, §6 E-10–E-15, E-19 | — | — | designed (BD) |

Design artifacts per requirement now also include the DD set: DD-002 (screen items, states, modules §1–§8), DD-002-API (query contract and error codes), DD-002-FN (`ListAsync`, repository reads, sort mapping, overdue rule, observability) and DD-002-SPD (P-10–P-15). Test viewpoints TC-101–TC-119 in DD-002 cover every REQ-020–REQ-027 and become the WI-003 test plan in plan revision 2.

## Test execution log

| Date | Check | Command | Environment | Result (pass / fail / not run) | Report / log link |
| --- | --- | --- | --- | --- | --- |
| 2026-09-22 | design-consistency checklist (BD-002 scope) | manual review | local | pass for BD-level items; DD/DB/test items not yet applicable — see the walk below | this file |
| 2026-09-22 | BD review | user review of BD-002 | — | pass — approved by the user ("the BD look good") | status.md |
| 2026-09-22 | design-consistency checklist (DB-003 scope) | manual review | local | pass — every index justified against a named access pattern, indexes deliberately omitted are listed with reasons, migration impact and recovery limits stated for both migrations; BD-002 revised to version 2 where DB-003 sharpened it | this file |
| 2026-09-22 | design-consistency checklist (DD-002 set scope) | manual review | local | pass — all four DD files exist, no content duplicated across them, every REQ has test viewpoints; one defect found and fixed during the walk (message-ID collision with DD-001, see below) | this file |
| 2026-09-22 | Mockup published | Artifact publish, private | claude.ai | done — https://claude.ai/artifact/2XrZ9xnEfzQ6pCbUnZovL3 (7 artboards); source in `docs/en/020_detailed-design/mockups/DD-002-screen-b-mockup.html` | DD-002 |
| 2026-09-22 | Unit / integration / E2E | — | — | not run — no code in scope for plan revision 1 | — |

### design-consistency checklist walk (BD-002 + DB-003 scope, 2026-09-22)

| Checklist item | Result |
| --- | --- |
| Requirements have stable IDs and acceptance criteria | pass — REQ-020–REQ-027 in `brief.md`, each with success and failure criteria; BD-002's overview table maps every one to a BD section |
| BD covers navigation, primary actions and exceptions | pass — screen transition (home → SCR-002 → SCR-001, 401 → `/login`), actions table, and 13 success/empty/exception flows |
| DD field/validation/state behavior agrees with BD | pass — DD-002's screen items, nine screen states and view-state transitions trace to BD-002 §3/§5/§6 item by item; the one divergence found during the walk was BD-002's drafted message IDs, corrected rather than carried forward (below) |
| API and DB mappings agree, including constraints and errors | partial — DB-003's API/DD mapping table covers every listed column plus the computed `isOverdue` and `total`; it is confirmed against DD-002-API when that is written in step 5. Two BD-002 statements were sharpened by DB-003 and folded back as BD-002 version 2 (fragment bound and `\` escaping; status sorts in workflow order, not alphabetically) |
| Missing decisions are resolved before dependent implementation | pass — DEC-001–DEC-009 all decided; the remaining open items in BD-002's closing table are DD/DB-level detail (message wording, fragment-match escaping and index, seed detail), each assigned to a later step |
| Relevant test scenarios map to the design | pass at design level — DD-002 lists 19 test viewpoints (TC-101–TC-119) covering every REQ, including index usage (`EXPLAIN`), paging stability across pages, fragment escaping and axe checks; they become executable `TC-###` cases with the test plan in plan revision 2 |
| Security-relevant fields identified | pass — no PII or secret on this screen; the role gate, the allow-listed sort/page-size values, parameter binding, `%`/`_` escaping and the page-size cap are stated in BD-002 non-functional security |
| Accessibility (WCAG 2.2 AA) captured | pass — table semantics with `aria-sort`, keyboard-reachable row activation via the order-number link (DEC-009), live-region result summary, announced loading, text-not-color status and overdue marker, labelled filters with linked errors, named and disabled paging controls |
| Migration impact described | pass — DB-003 specifies two additive migrations: indexes (+`pg_trgm`) created `CONCURRENTLY` outside the migration transaction, with the `INVALID` index recovery step spelled out, and the guarded 80-row seed whose only recovery limit is that reverting it deletes user edits made to seeded orders. No schema change was needed, so the plan's stop condition did not trigger |
| Tracing/logging for a new endpoint specified | pass — DD-002-FN "Observability" specifies the span `production_orders.list` with its attributes (filter shape only, never the fragment text), the counter `pmai.production_orders.listed` with DD-001's outcome names, and the histogram `pmai.production_orders.list_result_size`, all on the existing `ActivitySource`/`Meter` |

## Defects, failures and blockers

| Item | Reason | Blocker | Follow-up |
| --- | --- | --- | --- |
| Message-ID collision (found by the DD-002 design-consistency walk, fixed 2026-09-22) | BD-002 v1 drafted the new messages as MSG-E011–MSG-E016 and MSG-I001–MSG-I002, but DD-001 already defines MSG-E001–MSG-E014 and MSG-I001–MSG-I002, and they ship in `src/frontend/src/features/production-orders/messages.ts`. Had it reached implementation, Screen B would have redefined Screen A's "This production order doesn't exist", "Something went wrong" and both success messages | no | Fixed before any code: Screen B's new IDs are MSG-E015–MSG-E020 and MSG-I003–MSG-I004; MSG-E002 and MSG-E013 are reused rather than duplicated. BD-002 revision 3 records it |

## External references

- PR: not opened — not authorized in plan revision 1
- CI run: not applicable — no code changes yet
- Deployment: not applicable

## Remaining limitations and next action

Basic design, database design, the full DD-002 set and the rendered mockup exist; no code and no test. Of BD-002's open items, the order-number fragment match and its index and the seed detail are now settled in DB-003 (DEC-010, DEC-011); the message wording (MSG-E011–MSG-E016, MSG-I001–MSG-I002) and the exact spans/metrics remain assigned to DD-002 and DD-002-FN. Nothing in DB-003 has been executed against a database — the indexes, extension and seed exist only as design until plan revision 2 writes the migrations. Next action: the DD-002 set with its rendered mockup (plan revision 1, step 5).
