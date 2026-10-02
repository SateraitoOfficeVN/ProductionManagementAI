# WI-010 — Plant calendar

| Work item | Workflow | State |
| --- | --- | --- |
| WI-010 | feature-delivery | local delivery complete with accepted manual verification limits |

## Objective and sources

The user explicitly selected Plant calendar, Japanese screen name
「稼働カレンダー」 (working calendar), after WI-009 was merged and cleaned up.
Provide maintainable working days, holidays and shutdowns using the existing
plant-local date/clock model. Baseline: master 42e8932, merged Production lines.

The development-opportunities PDF, pages 2, 3 and 5, proposes working-day due-date
checks, due-soon windows and lead-time counting, and identifies the plant clock
as the foundation. These are proposals, not approved business rules. Line-specific
exceptions and dated capacity lookup are additional proposals for confirmation.

## Requirements and acceptance criteria

REQ-070–075 remain stable. DEC-001–007 core business rules are confirmed.
The authoritative review package is [006_REQ version 1](../../docs/en/000_requirements/006/006_REQ_plant-calendar.md), approved by the user including RP-01–05. All seven design documents including
[006_DD-SPD version 1](../../docs/en/020_detailed-design/006/006_DD-SPD_稼働カレンダー.md)
and required companions are explicitly approved. Current review item is
[implementation plan revision 2](plan.md); revision 2 is explicitly approved for local implementation and isolated verification.

| ID | Proposed requirement | Success path | Failure / exception path |
| --- | --- | --- | --- |
| REQ-070 | View plant-local working calendar on desktop/mobile | Browse a month, identify working/nonworking days and their effective source | Invalid range rejected; loading/error/empty states do not fabricate days |
| REQ-071 | Maintain the recurring weekly working pattern | Authorized editor configures working weekdays and an explicit date basis | Invalid or conflicting rules rejected; no silent replacement of concurrent edits |
| REQ-072 | Maintain date exceptions for holidays, shutdowns and extra working days | Effective daily state/hours and reason visible; exception removal restores the documented fallback | Duplicate/conflicting date entries and invalid hours rejected; input preserved on failure |
| REQ-073 | Resolve plant/line hours and dated capacity consistently | If line exceptions are approved, explain precedence and calculate product-unit capacity from effective hours and valid current timing | Retired/stale-unit/incompatible line-product data never presented as usable capacity; mixed units not summed |
| REQ-074 | Preserve authorization, concurrency and accessible Japanese UI | Confirmed API/UI roles, keyboard/dialog/mobile behavior and distinct centralized navigation icon | Unauthorized requests denied; stale saves do not overwrite; unknown outcomes are reconciled without blind replay |
| REQ-075 | Define impact on orders and dashboard date rules | Confirm whether current phase is lookup-only or includes explicitly bounded working-day behavior; document timezone/date boundaries | Existing due dates, historical orders and status rules are not silently rewritten or reinterpreted |

## Proposed initial boundary

Calendar master, weekly pattern, date exceptions and effective-day lookup.
Shared calendar with line overrides and dated capacity confirmed by DEC-003.
DEC-004 confirms lookup only: order validation, starts and dashboard date windows
remain unchanged. Working-day arithmetic from the source proposal is deferred. Automatic scheduling, load charts, BOM,
shift/time-of-day planning and holiday-provider integration are excluded from
the initial proposal. No automatic Japanese public-holiday import assumed.

## Requirements review boundary

Core business DEC-001–007 are resolved. Initial weekdays/hours, exception
precedence/reopening, manual holidays and protected past/effective-date rules
are recorded in decisions.md and 006_REQ. RP-01–05 were accepted with requirements approval. BD, DB, main DD, API, FN and SPD are explicitly approved. Final design-consistency
and design-stage security review passed; runtime security review and automated checks completed; local handoff permitted with manual accessibility limits explicitly accepted. Technical
range/length/transport/transaction limits are settled by the approved designs.
Implementation is locally verified; actual checks and manual limitations are recorded in evidence.md.

## Constraints

Artifacts English; UI and companion Japanese PDFs Japanese. Approved completed
designs stay read only; impacts use new WI-010-owned documents. Sequential design
review stops and new plan-revision approval remain required. Preserve all final
evidence videos. No live migration/deployment authorization.


Revision 1 is explicitly approved on 2026-10-01. Core business decisions are confirmed; 006_REQ, 006_BD, 006_DB and the complete main/API/FN/SPD family are approved; revision 1 is complete and implementation revision 2 is explicitly approved after presentation.


## Confirmed scope — DEC-002–004

Admin and Operator may view/edit calendars. Shared plant calendar supports
line-specific exceptions. Dated capacity lookup uses the confirmed effective
calendar and current valid line/product configuration; it does not schedule
orders. Existing due-date checks, due-soon dashboard windows and start rules
remain unchanged. DEC-005–007 default/effective-history rules are explicitly approved. The
requirements and all sequential design sources/PDFs are approved. TP-WI-010
records executed coverage and unrun manual variations; revision 2 local implementation is verified, with manual limits explicitly accepted for local handoff.

## Current delivery checkpoint — 2026-10-02

Revision 3 explicitly approved and executed: disposable CI calendar activation,
full local gates, commit/rebase/push and PR #36. Implementation-head GitHub run
36975008859 passed all three jobs; final publication-record head checks tracked
in PR #36. Requirements/designs unchanged; manual local handoff limits retained.
Merge, deployment, live activation and videos remain outside this revision.
