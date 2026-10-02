# WI-011 — Plant calendar button height and spacing defects

| Work item | Workflow | Status | Target release |
| --- | --- | --- | --- |
| WI-011 | bug-fix | Complete — local delivery | Unscheduled |

## Report and objective

On 2026-10-02, after reviewing Plant calendar evidence, the user reported uneven
button heights and buttons touching the content immediately above/below them.
Restore consistent, readable action layout on desktop and mobile. Existing user
instructions require a new bug work item and plan approval before application fixes.

## Defects, impact and reproduction

| ID | Actual / reported behavior | Expected behavior | Severity |
| --- | --- | --- | --- |
| BUG-003 | Ordinary action buttons have inconsistent heights; capacity paging buttons visibly stretch beyond adjacent search/lookup actions. | Single-line action buttons remain consistently 48 CSS px high at normal text size, without stretching with surrounding fields. | Moderate usability/layout defect; no data loss reported. |
| BUG-004 | Capacity search buttons touch preceding inputs; history/restart and recovery actions can touch surrounding rows/text. | Separate action groups and text/control boundaries have intentional spacing; wrapping preserves separation. | Moderate usability/layout defect. |

Reproduce on /plant-calendar: inspect month/scope Apply and search controls, scroll
to reference capacity and compare line/product search, paging and lookup actions.
Open weekly rules and dated exceptions, then inspect history/restart, action rows,
and existing error/Unknown recovery states. Repeat at desktop, mobile and 320 CSS
px reflow. Evidence videos are in the main checkout's ignored
`demos/evidence/output/plant-calendar/`; preserve them.

Baseline and corrected browser geometry, before/after screenshots, production
Playwright checks and native 200% zoom have now been executed; see evidence.md.

## Approved requirements and acceptance criteria

| ID | Requirement | Acceptance criteria |
| --- | --- | --- |
| REQ-076 | Consistent ordinary action heights | At normal text size, single-line calendar command buttons measure 48 CSS px within 1 px, enabled or disabled. Grid/flex rows do not stretch them. Multi-line labels and calendar date cells may grow to fit content, with a minimum 48 px target. |
| REQ-077 | Separate control groups and adjacent content | A minimum 8 CSS px visible gap separates adjacent action groups and standalone text/input-to-action boundaries, including wrapped rows, history and recovery actions. Labels stay associated with their fields; no clipped/overlapping controls. |
| REQ-078 | Preserve interaction and reflow | Desktop/mobile actions, keyboard focus, native centered confirmations, dirty/Unknown protection and capacity results remain correct. At 320 CSS px and native 200% zoom, controls/text remain readable and reachable without new horizontal overflow. |

## Scope and constraints

Calendar-only presentation correction: PlantCalendarPage, CapacityPanel,
WeeklyPatternEditor and DateExceptionEditor. Inspect shared button styling, but
prefer calendar-local alignment/spacing changes to avoid unrelated-screen impact.
Existing Japanese text, API/data/business rules and approved designs remain unchanged.
No redesign, dependency, new feature, deployment or replacement evidence videos.
If approved designs need amendment, stop and propose a new additive design under
a reviewed revision. Never revise completed approved WI-010 or other WI designs.

Baseline: c227acc76ee8463e0408858cfbbc90e327195518.
References: WI-010 approved 006_DD-SPD section 5 (48 px actions, 320 px/200% reflow),
[plan](plan.md), [decisions](decisions.md), [evidence](evidence.md).
