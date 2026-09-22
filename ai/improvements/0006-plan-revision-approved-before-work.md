<!-- Based on ai/templates/improvement.md. -->

# RFC: Every plan revision is shown and approved before its work starts

**Status:** adopted
**Affected:** `ai/policies.md`, `AGENTS.md`, `ai/workflows/feature-delivery.md`, `ai/templates/plan.md`,
`ai/evaluations/baseline-cases.md`

## Summary

Require every plan revision to be shown to the user and explicitly approved before any step it covers starts. This
applies to the first revision and to every later one, such as the implementation revision that follows the design
phase. Approving an earlier artifact or phase ("the DD is approved, move on to the implementation") authorizes
drafting the next revision, not executing it. A revision's recorded approval source must be a user message that
responds to that revision after it was shown.

## Motivation

Observed on 2026-09-22, in WI-003 (Screen B). The user approved the DD-002 set with "the DD is approved, move on to
the implementation". In that same turn the agent drafted plan revision 2 (implementation, tests, PR), appended it to
`plan.md`, and started implementing. The user never saw revision 2 before work under it began. The plan then recorded
the design approval as the revision's approval source and its review status as "approved". Recorded in WI-003
DEC-014.

The harness allowed this. `ai/policies.md` said "a direct request to create or edit a specified artifact authorizes
that bounded work; do not require a second approval", and `AGENTS.md` said "do not add redundant approvals". Nothing
said that a new plan revision is a new thing to approve. "Move on to the implementation" could therefore be read as
approving an implementation plan that did not exist yet when the user wrote it.

A plan revision fixes the scope, steps, permitted actions and risks. If the user can't review it, they can't change
the implementation approach before code is written. The expected outcome is that the user always sees a revision
before any work under it starts.

## Guide-level explanation

When a phase finishes and the user says to move on, the agent drafts the next plan revision, presents it (objective,
scope, steps, permitted actions, risks) and stops. It starts the first step only after the user approves that
revision, and it records that reply as the approval source. Once a revision is approved, its steps still run without
approval per step, exactly as before. The change adds one review point per revision, not one per step.

## Reference-level explanation

- **Current behavior:** the first plan is reviewed, but later revisions can start on the strength of an earlier
  approval. The plan template's approval-source field accepts any message.
- **Proposed behavior:**
  - `ai/policies.md`, "Plan and authorization": every revision must be shown and explicitly approved before its
    steps start. Approving an earlier artifact or phase is not approval of the next revision. Only a reply to the
    shown revision counts as its approval source.
  - `AGENTS.md`: the same rule, stated where every agent starts.
  - `ai/workflows/feature-delivery.md` step 2: draft the revision, show it, wait. Repeat for each later revision.
  - `ai/templates/plan.md`: the approval-source prompt says which message qualifies.
  - `ai/evaluations/baseline-cases.md`: a new case for "design approved with 'move on', implementation revision just
    drafted".
- **Unchanged:** the rest of "Plan and authorization". A direct request to create or edit a named artifact still
  authorizes that bounded work, and steps within an approved revision still need no approval per step.
- **Definition of done:** the rule is in all five files; WI-003's record no longer claims revision 2 was reviewed.

## Drawbacks

Each revision boundary costs the user one extra round-trip. That is the point of the change. It is one message per
revision, and WI-002 and WI-003 each had only two revisions.

## Rationale and alternatives

- **Rely on a memory entry only:** rejected. It would cover one agent's sessions but not the shared harness, which
  Codex and other agents also read (`AGENTS.md`).
- **Treat "move on to X" as approval when the next revision is short or obvious:** rejected. What counts as "obvious"
  is the judgement that failed here, and the user asked to see the revision.
- **Require approval per step:** rejected. The policy deliberately avoids that, and nobody asked for it.

## Prior art / evaluation

| Case (from ai/evaluations) | Before | After | Pass/fail |
| --- | --- | --- | --- |
| New: "design approved with 'move on to the implementation'; implementation revision just drafted" | Revision appended and implementation started in the same turn (WI-003) | Revision shown; agent stops until the user approves it | pass (by inspection of the new rule text; to be observed on the next work item, Screen C) |
| "Approved feature plan, clear next step" | Continue within scope without redundant approval | Unchanged within an approved revision | pass |
| "An approved plan revision is complete and the next phase needs a new revision" | Append the new revision, keeping history | Unchanged, plus the new revision is shown and approved before its work starts | pass |

## Risk and rollback

- **Risk:** low. The change adds a review point and relaxes no gate.
- **Rollback plan:** revert this RFC's commit. The WI-003 record correction (DEC-014) should stay either way, because
  it describes what happened.

## Unresolved questions

None.

## Adoption

- **Reviewer:** ThanhTN (request, 2026-09-22: "when we started the implementation you just started working on it with
  out showing me the newly created plan revisions first and have me approved it first. correct this")
- **Adopted revision:** `bc7d947` on `master`, the squash-merge of PR #13 (2026-09-22)
