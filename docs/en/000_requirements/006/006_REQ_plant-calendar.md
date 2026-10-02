# 006_REQ — Plant calendar

006_REQ version 1. SCR-006 「稼働カレンダー」 (Working calendar).

| Field | Value |
| --- | --- |
| System / group | ProductionManagementAI / Master data |
| Work item | [WI-010](../../../../work-items/WI-010/brief.md) |
| Date / author | 2026-10-01 / Agent |
| State | Submitted for requirements review; no implementation authorization |
| Plan | [Revision 1](../../../../work-items/WI-010/plan.md), explicitly approved |
| Dependency | WI-009 merged by PR #34 as 42e8932; existing plant clock and Product master |

## 1. Objective and scope

Maintain a shared plant working calendar with line-specific date exceptions.
Show working days, holidays, shutdowns and exceptional working days on desktop
and mobile. Explain the effective rule and allow dated capacity lookup using
current line/product configuration. Use the configured plant timezone (currently
Asia/Tokyo) for all dates and for the server's definition of today.

This phase is calendar master and lookup only. Existing due-date validation,
order-start rules, dashboard due-soon windows and historical order data remain
unchanged. The source PDF's working-day date arithmetic is explicitly deferred
by DEC-004. No automatic schedule, allocation, overload control, load chart,
BOM, shift/time-of-day model, holiday-provider integration or live deployment.

## 2. Actors and use cases

| Actor | Use case | Result |
| --- | --- | --- |
| Admin / Operator | UC-019: view the effective calendar | Browse a month, select plant/line scope and see dates, working state and rule source |
| Admin / Operator | UC-020: maintain weekly patterns and date exceptions | Save confirmed date-bounded rules, preserve past dates and reject stale mutations |
| Admin / Operator | UC-021: inspect dated line/product capacity | See effective hours and descriptive quantity in the actual product unit for today/future |

Both roles may read and write; every calendar API operation independently
checks authorization. Other authenticated roles receive no calendar access by
hiding/showing controls. Existing order/dashboard roles are unchanged.

## 3. Requirements and acceptance criteria

| ID | Requirement | Successful path | Failure / exception path |
| --- | --- | --- | --- |
| REQ-070 | View effective monthly calendar | Desktop/mobile show plant-local dates, working/nonworking state, reason where present and winning source; month/scope selection has explicit applied state | Invalid month/scope rejected; failed reads show error, not invented working days; unavailable earlier configuration identified explicitly |
| REQ-071 | Maintain effective weekly patterns | Initial pattern Monday–Friday working, Saturday/Sunday nonworking; new pattern effective today/future; earlier effective versions retained for past lookup | Past effective changes rejected server-side, including crossing plant midnight; concurrent stale save cannot overwrite or partially apply |
| REQ-072 | Maintain manual date exceptions | Plant or line exception can close a day or make it working, optionally replacing that day's hours; removing an editable exception restores documented lower-priority fallback | Past edits/removal, invalid date/hours or conflicting duplicate exception rejected; Cancel/Escape writes nothing; no implicit public-holiday import |
| REQ-073 | Resolve effective hours and dated capacity | Line-date exception outranks plant-date exception, then weekly pattern; working day without explicit hours uses current WI-009 line hours; nonworking day has zero hours; usable current line/product timing gives descriptive unit-bearing capacity for today/future | Retired/nonexistent/incompatible/stale-unit configuration not presented as usable capacity; past capacity unavailable, not fabricated; quantities across products or units never added |
| REQ-074 | Preserve secure, concurrent and accessible behavior | Japanese UI with distinct centralized icon; API/UI Admin/Operator checks; keyboard/month navigation, centered confirmation, focus return and mobile/200% zoom usability | Unauthorized operations denied; invalid data preserves inputs; conflicts require reconciliation; unknown write outcome does not trigger blind replay; no secrets/raw business values in telemetry |
| REQ-075 | Preserve existing order and dashboard date semantics | Existing calendar-day due-date checks, due-soon window and permitted status transitions continue; calendar state only informs this feature's lookup | A nonworking date does not newly reject an order save/start; no historical order mutation, deadline shift, working-day reinterpretation or inferred scheduling |

## 4. Confirmed business rules

[DEC-001–007](../../../../work-items/WI-010/decisions.md) record explicit user
feature/plan choices and approvals of the proposed business rules.

1. Read/write roles: Admin and Operator. Shared plant calendar plus line overrides.
2. Initial weekdays: Monday–Friday working; Saturday/Sunday nonworking. Editors
   may configure the weekly working pattern subject to effective-date protection.
3. A working day uses WI-009's current hours/day for the selected line unless a
   winning date exception explicitly supplies hours. Do not invent a plant-wide
   eight-hour baseline or overwrite the line's existing master value.
4. Explicit working-day hours: greater than zero, at most 24 hours, at most three
   fractional digits. Closed day has zero effective hours. Zero working hours,
   negative/overprecision/out-of-range hours and contradictory closed-day hours
   are invalid; clearing a working exception's hours restores line-hour fallback.
5. Precedence: line-date exception → plant-date exception → effective weekly
   pattern. A line-specific working exception can reopen a plant nonworking day.
   A line-specific closure can close a plant working day. Removing a date exception
   restores its lower-priority rule; it does not create a hidden replacement.
6. Holidays/shutdowns entered manually. No automatic Japanese holiday feed and no
   implied national-holiday rule. A holiday affects only its explicitly configured
   date(s); a UI may help enter dates without altering resolution semantics.
7. Calendar edits and exception removal for dates before server plant-local today
   are rejected. Weekly changes become effective today or a future date; previous
   effective versions remain available. Client timezone never changes this cutoff.
8. Dated capacity lookup is only for today/future using current valid line/product
   configuration, including current unit generation. Calendar past display is
   not a historical capacity, actual production result or utilization record.
9. Calendar rules do not gate order starts or alter order/dashboard date semantics.

## 5. Review proposals for initialization and lookup presentation

These complete the acceptance boundary for this requirements review. They are
explicit proposals rather than additional user business answers; dependent design
waits for review of this document.

| ID | Proposal | Reason / observable result |
| --- | --- | --- |
| RP-01 | Initialize the first weekly rule at the plant-local activation date; dates before the first recorded rule show configuration unavailable, unless a retained applicable rule exists | Avoid fabricating a calendar the application never stored; before activation, historical default is not a claim the plant operated |
| RP-02 | At most one current exception for each scope/date; replacing/removing today's or future exception preserves prior stored revisions; scheduled weekly patterns may be corrected/withdrawn before their effective date while retaining prior records | Prevent ambiguous resolution; removal restores fallback, without destroying the retained definition of past dates |
| RP-03 | Calendar scope view without a selected line shows working state and explicit exception hours, or line-dependent hours; it never displays an invented universal hour count | Default hours exist on WI-009 line masters, not on the weekly weekday mask |
| RP-04 | For usable line/product pair, descriptive capacity = effective hours × 60 ÷ minutes per product unit; presentation rounds down to whole units for discrete units, or at most three decimals for kg/m | Stay consistent with existing order quantity precision; show product unit and state that current capacity is not reserved or historical |
| RP-05 | Retired lines remain identifiable in retained calendar entries but are not candidates for new line exceptions or usable capacity; removing an editable future retired-line exception is allowed | Preserve references while preventing new operational use; past-date protection still applies |

DB/API/FN designs will specify technical length/date-range/paging limits, exact
numeric transport, concurrency tokens, boundary transactions and retention
representation. They must preserve these reviewed business outcomes; no contract
or implementation is claimed by this document. Generic change-history UI/export
is outside this scope; retained revisions support calendar resolution protection.

## 6. Information and interaction boundaries

SCR-006 「稼働カレンダー」 (Working calendar), proposed route `/plant-calendar`.
Display calendar date, scope, effective working state/hours, source, reason and
appropriate editability. Weekly configuration and date-exception editors are
part of this feature. Capacity lookup includes line/product/date and product
unit; no sum across products and no assumed physical device capture.

Monthly navigation must be keyboard usable; distinguish working/closed/unavailable
states with text as well as color. Explicit loading, empty, validation, forbidden,
read error, conflict, pending and unknown-outcome states preserve honest data.
Past dates readable where configured, with edit controls disabled and API rejection
independent from the UI. Reconcile drafts before reload or leaving dirty forms.

Use existing cookie authentication, same-origin JSON mutations and server-side
validation. Keep telemetry dimensions bounded; no raw dates, search terms, names,
reasons, quantities, user credentials or exception details as metric labels.

## 7. Traceability and verification prerequisites

Trace REQ-070–075 into 006_BD/DB/main DD/API/FN/SPD and the future test plan.
UC-019–021 are reserved for this work item. No existing screen behavior change
requires rewriting approved completed documents. Step 10 impact addendum may be
not applicable if all capacity/calendar interactions remain inside SCR-006.

Planned checks: weekday and leap/month/year boundaries; plant/browser timezone
and midnight; precedence and reopening/removal; explicit hours/precision; initial
unavailable dates; version transitions and protected past dates; stale saves and
concurrent mutation; no invented current/past capacity; unit-change ABA and retired
references; API read/write roles; keyboard/dialog/mobile/zoom; existing order and
dashboard regression. None has run for WI-010; the calendar is not implemented.

New persistence must be additive and initialized with explicit activation policy,
without rewriting old orders, line hours, product units or approved designs.
Migration/rollback and owner permissions are defined later, with isolated rehearsal
only after a separately approved implementation plan. No live deployment.

This source and its English/Japanese PDFs are the current sequential review
package. Requirements review approval authorizes the next step within revision 1;
it does not authorize application implementation or remote delivery.
