# Production Dashboard (Screen C) — Requirements Traceability & Evidence

As of branch `feature/WI-004-production-dashboard`, 2026-09-22 (design phase, plan revision 1).

## Traceability matrix

Design columns are filled as each document is written; code and test columns stay empty until plan revision 2.

| Requirement ID | Requirement | Design artifact | Code | Test case ID | Status |
| --- | --- | --- | --- | --- | --- |
| REQ-028 | Dashboard is the landing page at `/`; error and empty states | BD-003 screen transition, 0-1, §1 items 3–6/6a/25, §6 E-20–E-22, flows; DEC-005, DEC-012 | — | — | designed (BD) |
| REQ-029 | Status count tiles | BD-003 §3 items 7–11, §4 M-11 | — | — | designed (BD) |
| REQ-030 | Overdue and due-soon groups | BD-003 D-01, D-02, §3 items 16–19, §4 M-15, M-19; DEC-007, DEC-008 | — | — | designed (BD) |
| REQ-031 | Workload chart by due week | BD-003 D-03, §3 items 20–21, §4 M-16; DEC-009, DEC-010 | — | — | designed (BD) |
| REQ-032 | Top 10 products by open quantity | BD-003 D-04, §3 item 22 | — | — | designed (BD) |
| REQ-033 | Completion time recorded | BD-003 FN-021, FN-022; BD-001 v6 FN-006 and business rules; DD-001 v4 module 1 step 5, transitions, DB mapping; DD-001-FN v3 UpdateAsync step 7; DB-004 column, constraints, backfill | — | — | designed (BD, DB, DD-001) |
| REQ-034 | Completed this week / month | BD-003 D-05, D-06, §3 items 12–13, §4 M-18 | — | — | designed (BD) |
| REQ-035 | On-time rate, 30 days | BD-003 D-07, §3 item 14, §4 M-13 | — | — | designed (BD) |
| REQ-036 | Completion trend, 12 weeks | BD-003 D-08, §3 items 23–24, §4 M-16; DEC-010 | — | — | designed (BD) |
| REQ-037 | Average lead time, 30 days | BD-003 D-09, §3 item 15, §4 M-14 | — | — | designed (BD) |
| REQ-038 | Admin/Operator only | BD-003 0-1, FN-023, actions, exception flows | — | — | designed (BD) |
| REQ-039 | Read-only, no drill-down | BD-003 §6 closing note, actions; DEC-006 | — | — | designed (BD) |

## Test execution log

| Date | Check | Command | Environment | Result (pass / fail / not run) | Report / log link |
| --- | --- | --- | --- | --- | --- |
| 2026-09-22 | design-consistency checklist (BD-003 scope) | manual review | local | pass for BD-level items; one defect found and fixed (below) | walk below |
| 2026-09-22 | BD review | user review of BD-003 | — | pass — approved ("BD-003 is approved, move on to DB-004") | status.md |
| 2026-09-22 | Seed simulation | Python script over DB-004's seed tables and WI-003's parsed seed, for each run weekday and three days of the month | local | pass after one fix — first draft left week 6 empty on Monday/Tuesday runs; order #44 moved to +42 | DB-004 "What the seed produces" |
| 2026-09-22 | design-consistency checklist (DB-004 and Screen A amendments scope) | manual review | local | pass — walk below | this file |
| 2026-09-22 | DB review | user review of DB-004 and the Screen A amendments | — | not run — awaiting user | — |

## Design-consistency walk — BD-003 scope

| Checklist item | Result |
| --- | --- |
| Requirements have stable IDs and acceptance criteria | pass — REQ-028–REQ-039 each with success and failure criteria; UC-008–UC-011 |
| BD covers navigation, primary actions and exceptions | pass — screen transition from login and to SCR-001/SCR-002; page actions; 401, 403, load failure, empty system and no-recent-completion flows |
| DD agrees with BD | not yet applicable — DD-003 not written |
| API and DB mappings agree | not yet applicable — DB-004 and DD-003-API not written; BD-003 states what each must provide |
| Missing decisions resolved before dependent work | pass — DEC-008 and DEC-009 (user) settled before BD-003; DEC-010–DEC-012 recorded; one technical question (how the snapshot read is made consistent) deferred to DB-004 and listed in BD-003's open questions |
| Test scenarios map to the design | partly — each metric has one definition (D-01–D-09) to derive test viewpoints from in DD-003 |
| Security-relevant fields identified | pass — no input, no PII; completion time is server-written only; role gate server-side |
| Accessibility captured | pass — region headings, SVG `role="img"` with summaries, printed bar values, table equivalents, non-text contrast |
| Migration impact described | not yet applicable — DB-004 |
| Tracing/logging specified | deferred to DD-003-FN, as BD-002 did |

**Defect found and fixed:** the brief's REQ-028 said the application header leads to the list and New-order screens, but the header has no navigation. Satisfying it as written would visibly change SCR-001 and SCR-002, outside plan revision 1's scope. Resolved by DEC-012 (page actions on SCR-003) and the brief's wording corrected. A second gap was found while applying DEC-008: active orders due after the 8-week look-ahead fell into no workload bar; the user decided a "later" bar (DEC-009).

## Design-consistency walk — DB-004 and Screen A amendments scope

| Checklist item | Result |
| --- | --- |
| Requirements have stable IDs and acceptance criteria | pass — unchanged |
| BD covers navigation, primary actions and exceptions | pass — BD-001 v6 adds the completion rule with no new screen behavior |
| DD agrees with BD | pass for DD-001 v4 / DD-001-FN v3 against BD-001 v6: the completion time is set in one place (entity step 5), in the same save, never from the request |
| API and DB mappings agree | pass — DB-004 maps every BD-003 item to a query and a proposed API field; DD-001-API reviewed: no field added, none accepted. `completed_at_utc` is exposed by no endpoint |
| Missing decisions resolved | pass — seed growth asked of the user (DEC-013); DEC-014 and DEC-015 recorded; BD-003's open question on the snapshot mechanism closed by DEC-015 |
| Test scenarios map to the design | pass at this level — DD-001's new viewpoint covers REQ-033; seed properties are stated so DD-003 can derive tests; integration-test isolation from the seed is called out |
| Security-relevant fields identified | pass — no client input reaches any dashboard statement; the time zone is a server configuration value passed as a parameter; no new grant |
| Accessibility captured | not applicable at DB scope |
| Migration impact described | pass — three migrations with type, locking, deploy order, recovery limits and rollback; Screen B's changed demo totals listed |
| Tracing/logging specified | deferred to DD-003-FN, as planned |
