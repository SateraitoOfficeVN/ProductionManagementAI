# WI-011 — Decisions and open points

## Confirmed

- Follow bug-fix workflow in a new work item, as the user's standing bug instruction.
- Plan review is mandatory before implementation; bug report is not plan approval.
- User report concerns uneven action heights and insufficient vertical separation.
- Persist records in English; user conversation Vietnamese; application Japanese.
- Completed approved design documents are immutable.

## Planning assumptions / proposals

- Initial scope is Plant calendar, following its just-recorded evidence.
- Prefer calendar-local parent alignment/spacing over a shared button-class change.
- Proposed metrics: 48 px ordinary single-line actions; minimum 8 px separate group
  and text/control gap. Allow natural growth for multi-line labels and date cells.
- No business decision or design amendment is needed for restoring existing intent;
  if inspection disproves this, stop dependent implementation for review.

No approval inferred. No live production incident/data-loss report.

## Revision 1 approval — 2026-10-02

The user explicitly replied "approved" after plan revision 1 was presented.
Its five local steps and disposable reproduction fixtures are authorized.
Commit/push/PR/merge, live deployment and new videos remain excluded.

## Correction and verification outcome — 2026-10-02

Confirmed candidate cause with actual geometry: default stretch in parent rows
created 72/120 px actions; search input/button gap was 0 px. Use calendar-local
items-end/items-start and explicit gap groups. Shared LineDialog button/input
classes unchanged, preserving other screens. Search actions moved outside input
labels without changing their Japanese field names or action handlers; error
announcement remains role=alert on its text, with actions in a separate group.

48 px ordinary actions and 8 px search gaps now measured at 1440/540/320 widths.
Natural growth for long labels/date cells preserved. No design amendment required.
Local review complete; no external-delivery authority inferred.

## Separate evidence authorization — 2026-10-02

Direct user request for new evidence videos after local fix review. Record four
English/Japanese web/mobile outputs from local WI-011 production build; preserve
prior videos and approved designs. No new WI; no commit/push/PR/merge authority.
See evidence.md for actual capture/export results and proof boundaries.

## Next-phase proposal — 2026-10-02

Continuation request prompts revision2 for commit/push/PR/CI delivery. Approval
is pending; historical revision1 authority unchanged. Merge/cleanup/deployment
will require separate authorization. Preserve ignored videos and approved designs.


## Revision 2 approval — 2026-10-02

Explicit user "approved" after revision 2 was presented authorizes commit/push/PR
and exact-head CI handoff. Merge, branch/worktree cleanup and deployment excluded.
