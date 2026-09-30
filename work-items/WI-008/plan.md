# WI-008 — Bug-fix implementation plan

| Revision | Date | Purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-09-30 | Correct Product master dialogs and duplicate navbar icon | current — continuation authorized | User instruction to continue WI-008 after the approval correction, 2026-09-30 |

## Revision 1 — Reproduce, fix and verify two UI defects

### Objective and scope

Resolve BUG-001 and BUG-002 against REQ-061–REQ-063 in [brief.md](brief.md),
following [bug-fix](../../ai/workflows/bug-fix.md).

Center both Product master dialogs with explicit auto margins and responsive
viewport bounds, following the existing order-dialog convention. Preserve
native dialog/focus behavior. Export `Package` as a dedicated `NavProductsIcon`
through the existing centralized icon module and use it for `/products` in
both navigation layouts; WI-003 retains `ClipboardList`.

The user now authorizes continuing the existing WI-008 scope after questioning
the premature implementation. This does not establish approval before that work.
The original acknowledgment was incorrectly treated as plan approval by the agent.
Backend, database, permissions, business rules, new dependencies and changes
to previously approved design files are excluded. Recording utilities and
generated media remain outside the project work-item deliverables; a local
rerun is verification evidence for this fix.

### Inputs and assumptions

| Input | Baseline / use |
| --- | --- |
| [Brief](brief.md), [decisions](decisions.md) | Two user-reported defects and explicit work-item/workflow correction |
| [WI-006 brief](../WI-006/brief.md), [plan](../WI-006/plan.md), [status](../WI-006/status.md), [decisions](../WI-006/decisions.md), [evidence](../WI-006/evidence.md) | Completed behavior and actual prior coverage, read only |
| [004_BD](../../docs/en/010_basic-design/004/004_BD_製品マスタ.md), [004_DD-SPD](../../docs/en/020_detailed-design/004/004_DD-SPD_製品マスタ.md) | Existing navbar and accessible dialog contracts, read only |
| Source at master `61ae1ad` | Product dialogs lack `m-auto`; navbar entries duplicate `NavOrdersIcon`; positioning cause must be confirmed at runtime |
| Separate evidence checkout | Existing Playwright recorder/exporter, visible 400 ms pacing and 2x reading pauses; keep existing captures |

### Deliverables and milestones

| # | Step | Depends on | Skill | Deliverable | Verification | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Create isolated `feature/WI-008-product-ui-bugs` branch/worktree; copy only these planning records into it | Plan approval | implementation | Isolated checkout and updated status | Inspect baseline/status; preserve unrelated main/evidence changes | Done: isolated branch at baseline `61ae1ad` |
| 2 | Reproduce both defects on a disposable stack and add meaningful failing checks | 1 | testing | Before screenshots, geometry/icon observations, TC-323–TC-325 | Actual desktop/mobile dialog bounds and duplicated SVG glyphs; record actual failures | Done: 8/8 baseline checks failed; geometry and duplicate SVG captured |
| 3 | Review scoped design consistency, then apply minimal dialog and icon changes | 2 | implementation | Component styling and centralized `NavProductsIcon` | Compare with approved modal/focus/navigation contracts; no old-design edits | Done: scoped gate passed; three application files corrected |
| 4 | Verify the fix and existing interaction behavior | 3 | testing | Regression results and after screenshots | Frontend lint/build, relevant product/navbar component checks, targeted Playwright desktop/mobile checks, axe/focus/Escape/Cancel/zoom checks | Done: lint/build, 4 component and 15 browser checks passed |
| 5 | Run the corrected build visibly and record post-fix evidence | 4 | testing | Separate web/mobile captures and four EN/JA MP4 exports outside work-item records | 400 ms action pacing, 2x pauses; verify capture provenance identifies the corrected application source, dialog screenshots, full video decode and subtitle alignment | Done: headed web/mobile journeys passed; four videos decoded and 16 frames reviewed |
| 6 | Review and prepare local delivery | 4–5 | pr-review | Review, evidence/status updates and local commit | Delivery gate; inspect focused diff and secrets; update project current-state records at local closeout without claiming merge/deployment | Done: local review and delivery gate passed; local commit delivery |

TC-323 covers centering/mobile bounds for both dialogs; TC-324 covers preserved
modal interaction and zoom reachability; TC-325 covers distinct desktop/mobile
navbar glyphs and unchanged labels/routes.

### Roles and resources

Agent: author, implementer and local reviewer. User: plan approver and final
reviewer. Use the existing React/Tailwind/Lucide/Playwright toolchain. Install
only pinned repository dependencies where missing.

### Authorized actions and external operations

| Action | Authorization | Limit |
| --- | --- | --- |
| Create bug-fix planning records | Explicit latest user request | WI-008 planning only |
| Implement/test/local commit | Latest explicit WI-008 continuation instruction | Isolated branch; the two defects and their regression checks |
| Disposable Compose start/migrations/teardown and visible local recordings | Latest explicit WI-008 continuation instruction | Only newly created task-owned stack/database/volumes; retain evidence and avoid credentials in files |
| Push / PR / merge / publish / live or demo migration/deployment | Not authorized | Obtain task-specific authorization later |

### Risks, gates and stop conditions

- Verify centering against viewport geometry after scrolling; avoid relying on
  jsdom or a CSS class assertion to prove layout. Keep mobile margins and check
  constrained-height/200% zoom behavior so centering does not hide actions.
- Preserve focus, native Escape and cancellation semantics. An unrelated
  functional failure needs separate scope review; do not weaken assertions.
- Apply the [design-consistency gate](../../ai/checklists/design-consistency.md)
  to the bounded fix before implementation and the
  [delivery gate](../../ai/checklists/delivery.md) before calling it done.
  Review the [security checklist](../../ai/checklists/security-review.md) for
  credentials/dependencies; no new security boundary is proposed. Release
  readiness is not claimed because deployment is outside scope.
- Keep all approved completed-work-item designs unchanged. If a new design
  amendment becomes required, revise the plan and follow sequential design
  Markdown review with required companion PDFs before implementation.
- Do not tear down unknown stacks or overwrite user changes. Browser closure
  or incomplete captures are failures, not successful evidence.
- New scope, missing access or an external action beyond authorization stops
  the affected step for user input under project policies.

### Approval / sign-off

- Initial plan approval: not granted; the agent incorrectly treated an acknowledgment as approval.
- Current authorization source: the subsequent user instruction on 2026-09-30 (English rendering: "Okay, acceptable for now; continue the work item").
- Authorization applies to continuing the existing revision 1 scope from that instruction onward; it is not retroactive approval of implementation or commit `a628f2e`.
- Closure: existing code and verification retained; corrected authorization records and local handover completed. Push, PR, merge and deployment remain outside authorization.
