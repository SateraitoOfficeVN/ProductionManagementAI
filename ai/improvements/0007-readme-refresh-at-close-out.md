<!-- Based on ai/templates/improvement.md. -->

# RFC: Refresh the root README when a work item closes

**Status:** adopted
**Affected:** `ai/workflows/feature-delivery.md`, `ai/checklists/delivery.md`, `ai/evaluations/baseline-cases.md`

## Summary

Make the root `README.md` part of every work item's close-out. It gets updated in the same follow-up change as
`ai/project.md`, `CLAUDE.md`'s current-state section and the work item's own records. The feature-delivery workflow
gains an explicit close-out step, and the delivery checklist gains a matching item.

## Motivation

Observed on 2026-09-22. WI-003 (Screen B) was closed out in PR #10, which updated `ai/project.md`, `CLAUDE.md` and the
work item's records. The root `README.md` was left as it was, still reading "Next: Screen B, the production-order
list". It also didn't mention the docs-only CI skip (RFC 0005) or the plan-revision approval rule (RFC 0006). The user
had to ask for a separate refresh, which was done in PR #13, and then said the README should be updated after every
work item.

Nothing in the harness named the README as a close-out target. The feature-delivery workflow ended at "record
evidence", and the delivery checklist asked only for status, decisions and evidence. So whether the README got
updated depended on memory. The README is the first thing a new reader sees, so letting it go stale misleads them
most.

## Guide-level explanation

After a work item's PR is merged, the agent prepares one close-out change. That change updates the work item's status,
evidence and plan closure, `ai/project.md`, `CLAUDE.md`'s current state, and the root `README.md`. The README gets the
finished work item in its status list, "Next" pointing at the next planned work, and any CI, workflow or layout change
the work item introduced. The agent doesn't wait to be asked, and describes only what git actually tracks.

## Reference-level explanation

- **Current behavior:** `feature-delivery.md` ends at step 7 (evidence and limitations). No workflow, checklist or
  evaluation names the README. Close-out updates to `ai/project.md` and `CLAUDE.md` happened by precedent, not by
  rule.
- **Proposed behavior:**
  - `ai/workflows/feature-delivery.md`: a new step 8, the close-out after the merge, listing the work item's records,
    `ai/project.md`, `CLAUDE.md` and the root `README.md`.
  - `ai/checklists/delivery.md`: at close-out, the README, `ai/project.md` and `CLAUDE.md` name the finished work item
    and the next planned work, and describe only what git tracks.
  - `ai/evaluations/baseline-cases.md`: a new case for "PR merged, close-out being prepared".
- **Definition of done:** the rule is in all three files, and the next work item's close-out (Screen C) updates the
  README without being asked.

## Drawbacks

Close-out touches one more file. The README's status section is short, so this is a few lines per work item.

## Rationale and alternatives

- **Rely on a memory entry only:** rejected. It covers one agent's sessions, not Codex or anyone else using the
  harness.
- **Generate the README status from `ai/project.md`:** rejected for now. It would need tooling this repository
  doesn't have, for a section that is five lines long.
- **Drop the status section from the README and link to `ai/project.md`:** rejected. The README is meant to say at a
  glance where the project stands.

## Prior art / evaluation

| Case (from ai/evaluations) | Before | After | Pass/fail |
| --- | --- | --- | --- |
| New: "a work item's PR has been merged and its close-out is being prepared" | README left stale (WI-003's close-out, PR #10) | README refreshed in the same close-out change | pass (by inspection of the new rule text; to be observed at Screen C's close-out) |
| "An approved plan revision is complete and the next phase needs a new revision" | Unchanged | Unchanged | pass |

## Risk and rollback

- **Risk:** low. The change is documentation-only and relaxes no gate.
- **Rollback plan:** revert this RFC's commit.

## Unresolved questions

- Whether a bug-fix work item that changes no user-visible status also needs a README update. The checklist item
  asks only that the README names the finished work item and the next planned work. For a small bug fix, that
  may mean no change, which is acceptable.

## Adoption

- **Reviewer:** ThanhTN (request, 2026-09-22: "next time update the README.md after a work item is finished", then
  "add it as improvement")
- **Adopted revision:** `bc7d947` on `master`, the squash-merge of PR #13 (2026-09-22)
