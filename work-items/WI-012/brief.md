# WI-012 — Plant calendar capacity design/mockup alignment

| Work item | Workflow | Status | Baseline |
| --- | --- | --- | --- |
| WI-012 | bug-fix | PR delivery in progress | de130ab505470f5d608f84351ebc6794851a0dcb |

## Report and objective

The user highlighted the two Previous/Next pairs in the capacity panel after
WI-011 and asked what they do because they are absent from the visual design.
The user then requested mockups consistent with the design and an implemented
screen consistent with those mockups. Initial scope is SCR-006 reference capacity
on desktop/mobile, the panel shown in the report. Wider calendar redesign needs
explicit scope review.

## Defect and evidence

BUG-005: approved BD and SPD visual artifacts do not depict the actual capacity
selection/search/paging controls or their relationships. Runtime CapacityPanel
places two independent Previous/Next pairs in a row-flow two-column grid without
clear grouping. WI-011 restored heights/gaps but did not resolve visual-design
alignment. Moderate UX/design consistency defect; no reported data loss.

Existing SPD section4.4 specifies paged current line/product choices; DD-FN
M-08/M-09 and frontend api.ts use pages of50. BD F-14 defines line/product/date
selection and explicit request. Bounded API paging remains required; exposing
raw paging machinery as loose default-visible buttons is not required.

## Acceptance criteria

| ID | Requirement / acceptance |
| --- | --- |
| REQ-079 | New capacity visual package traces each visible field/action to existing BD/SPD intent; no unexplained Previous/Next pairs. Japanese UI; English source captions and Japanese companion edition. |
| REQ-080 | Clear line/product/date selection and explicit capacity request/result; all matching eligible options remain reachable beyond50 items without backend/schema changes. Selection/search/recovery behavior is explicitly illustrated. |
| REQ-081 | Eventual web/mobile implementation matches the approved new mockup in control order, grouping, labels, conditional visibility and states; visual review includes screenshots and meaningful browser checks at desktop/mobile/320px and native200% where feasible. WI-011 48px minimum/gaps and content-safe wrapping retained. |

Preserve all completed approved designs and artifacts. Create an additive design
and new mockups rather than editing WI-010/WI-011 approved history. Preserve
existing video evidence. No database/API/business-rule redesign or new dependencies.

## Restart scope — 2026-10-02

The user reported multiple differences across the entire 稼働カレンダー screen
and requested WI-012 be replanned from the beginning, removing newly added files.
The initial capacity-only assumption is superseded. BUG-005 now covers full SCR-006
visual/design alignment; no new WI is created. Revision2 scopes the full-screen
audit and replacement design review; application correction follows revision3.

REQ-079 now requires every SCR-006 region/action/state to trace to approved intent,
including all editor/history/confirmation modes. REQ-080 preserves all approved
selection, calendar and editor behavior while simplifying only justified visual gaps.
REQ-081 applies approved mockup parity to the entire desktop/mobile screen, not only
capacity. Existing business constraints and completed approved artifacts are retained.
