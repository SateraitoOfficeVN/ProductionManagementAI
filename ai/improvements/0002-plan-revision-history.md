# RFC: Keep every plan revision, oldest first

**Status:** adopted
**Affected:** `ai/templates/plan.md`, `ai/skills/planning/SKILL.md`, `ai/evaluations/baseline-cases.md`

## Summary

`plan.md` keeps every revision of a work item's plan, in full and in chronological order. A new revision is appended after the last one. The previous revision is closed in place (Outcome column, Closure line), and a revision index table at the top marks the current one.

## Motivation

During WI-002 (2026-09-18), when the design-phase plan (revision 1) was finished and the implementation plan (revision 2) was drafted, Claude overwrote `plan.md` with revision 2 only. The template says only "If this supersedes a prior revision, state which and why", and nothing said to keep the old one. Revision 1 had never been committed, so git couldn't restore it; it had to be rebuilt from the session record. The user asked to "keep the design plan in there so we could back-track it". Claude then restored revision 1 as an appendix *below* revision 2, and the user corrected again: "put it in chronological orders do not just move it to below revision 2", followed by "update the harness improvement so this doesn't happened next time". Evidence: `work-items/WI-002/plan.md` (reconstruction note), `work-items/WI-002/status.md`.

## Guide-level explanation

When a plan needs a new revision, an agent:

1. fills in the current revision's Outcome column and Closure line;
2. appends `## Revision {N+1} — {phase}` after a `---` separator;
3. updates the index table at the top.

Nothing earlier is deleted, rewritten or moved. Reading `plan.md` top to bottom shows the plan's history in order.

## Reference-level explanation

- **Current behavior:** the template has one revision's sections at `##` level and a one-line "supersedes" note. Revising overwrites the file.
- **Proposed behavior:**
  - **Template:** the header comment carries the revision rule. The index table and a `## Revision {N} — {phase}` wrapper are added, with sections demoted one level. The deliverables table gains an Outcome column, the approval section gains a Closure line, and the template ends with a pointer for appending the next revision.
  - **Planning skill:** a new execution step 5 (never delete/overwrite; close, append, update the index; chronological order). Section 5 says `plan.md` holds every revision. Section 6 checks on each revision that earlier revisions are intact and in order, with `git diff` when committed.
  - **Evaluations:** a new baseline case.
- **Definition of done:** the template and skill state the rule; the new evaluation case passes against WI-002's `plan.md`.

## Drawbacks

`plan.md` grows with each revision. That is acceptable: plans are short, and the index table gives a quick summary.

## Rationale and alternatives

- **Separate `plan-r1.md`, `plan-r2.md` files:** rejected. The user asked for the revisions in `plan.md`, in order, and other skills already read `plan.md` as the single plan file.
- **Rely on git history only:** rejected. Revisions are often drafted and superseded before any commit (as happened here), and reviewers read the file, not the log.
- **Newest-first order:** rejected by the user in favor of chronological order.

## Prior art / evaluation

| Case (from ai/evaluations) | Before | After | Pass/fail |
| --- | --- | --- | --- |
| New: "An approved plan revision is complete and the next phase needs a new revision" | Revision 1 overwritten, then restored as an appendix below revision 2 (observed in WI-002) | WI-002 `plan.md` now has an index table, then revision 1 (closed, with Outcome and Closure), then revision 2 (current), matching the new template | pass (manual check against the existing file, 2026-09-18; the rule hasn't yet been exercised on a fresh revision) |
| "Approved feature plan, clear next step" | Continue without redundant approval | Unchanged | pass (no approval rule changed) |

## Risk and rollback

- **Risk:** existing single-revision plans (WI-001) don't use the new wrapper. They stay valid and adopt the structure on their next revision; there's no retroactive rewrite.
- **Rollback plan:** revert the files listed under **Affected** to the revision before this change (`e7e0d36`). No gate was relaxed.

## Unresolved questions

- None.

## Adoption

- **Reviewer:** ThanhTN (explicit request, 2026-09-18: "update the harness improvement so this doesn't happened next time")
- **Adopted revision:** the commit after `dc228ab` on `feature/harness-wi002-feedback` (effective for shared use once that branch's PR is merged; see WI-002 `decisions.md` DEC-028)
