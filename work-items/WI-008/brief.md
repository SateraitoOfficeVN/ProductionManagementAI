# WI-008 — Product dialog positioning and navbar icon defects

| Work item | Workflow | Status | Target release |
| --- | --- | --- | --- |
| WI-008 | [bug-fix](../../ai/workflows/bug-fix.md) | done (local delivery) | Unscheduled |

## Report and objective

During the visible Product master evidence run on 2026-09-30, the user reported
mispositioned popups and a navbar icon duplicated from WI-003. The user then
explicitly requested a new work item following the bug-fix workflow. Restore
correct modal positioning and visually distinct navigation on desktop and mobile.

Recording utilities remain separate from this application bug-fix work item.

## Defects and reproduction

| ID | Actual behavior | Expected behavior | Impact / severity |
| --- | --- | --- | --- |
| BUG-001 | Product master popup is not centered, as observed by the user. Both product dialogs lack the explicit centering used by the existing order dialog. | Retirement and unsaved-change confirmation dialogs are centered in the viewport and fit mobile screens. | Moderate visual/accessibility defect; runtime geometry reproduced and corrected; see evidence. No data loss or production incident reported. |
| BUG-002 | `/products` and `/production-orders` both use `NavOrdersIcon` (`ClipboardList`). | Product master has a product-specific icon distinct from the WI-003 production-order list. | Minor visual/navigation defect. |

Reproduce BUG-001 on an isolated stack: sign in as Admin; open `/products`,
choose an active product's retirement action and inspect the confirmation.
Cancel, then open `/products/new`, enter a draft name and choose Cancel to
inspect the unsaved-change confirmation. Repeat at desktop and mobile viewports,
including after scrolling. Capture dialog bounding rectangles and screenshots.

Reproduce BUG-002: inspect the Product master and production-order list links
in the desktop navbar; repeat with the mobile Menu panel open.

## Requirements and acceptance criteria

| ID | Requirement | Acceptance criteria | Priority |
| --- | --- | --- | --- |
| REQ-061 | Center Product master confirmation dialogs | Both dialogs' bounding-box centers match viewport centers within 2 CSS pixels at 1440 x 900 and 412 x 840, including a scrolled page; each edge remains at least 16 CSS pixels inside the viewport when content fits. | must |
| REQ-062 | Preserve accessible modal interaction | Native modal behavior, initial focus, focus containment, Escape/Cancel without writes, and return focus continue to work; content/actions remain reachable at 200% mobile zoom and constrained height. | must |
| REQ-063 | Distinguish Product master navigation | Desktop and mobile navigation use a dedicated product glyph different from `ClipboardList`; visible Japanese labels, destinations, current-page marker and decorative icon accessibility remain correct. | must |

## Scope boundaries

Limit fixes to Product master dialog styling and its navbar icon, with targeted
regression coverage. Preserve API/database/authentication/business rules and
completed, approved design documents. No new dependency or design document is
planned for restoring these existing UI behaviors. If a design/contract change
becomes necessary, create a new amendment under an explicitly reviewed plan.

## Baseline and references

- Master baseline: `61ae1ade341e21755eab15d297beb716da69f7ff`.
- [WI-006 status](../WI-006/status.md) and [decisions](../WI-006/decisions.md): completed Product master and read-only approved designs.
- [Product screen design](../../docs/en/020_detailed-design/004/004_DD-SPD_製品マスタ.md): native modal and focus behavior.
- [Plan](plan.md), [decisions](decisions.md), [evidence](evidence.md).

Plan revision 1 was approved on 2026-09-30. The Package glyph and dialog corrections
are implemented and verified; see [evidence](evidence.md) and [review](review.md).
Local delivery is complete; master, PR and deployment are unchanged.
