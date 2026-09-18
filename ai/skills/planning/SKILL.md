---
name: planning
description: Create or revise a reviewable execution plan for a work item.
---

# planning

## 1. Purpose and usage scenario

Turn a brief, bug report or improvement proposal into a bounded, reviewable execution plan before any dependent work starts. Use at the start of any workflow (project-bootstrap, feature-delivery, bug-fix, harness-improvement) once the triggering brief/problem is captured, and again whenever approved scope needs revision.

## 2. Mandatory inputs, optional inputs, and source reference order

**Mandatory:** brief (or the equivalent problem statement for a bug-fix/harness-improvement), acceptance criteria, current repository state.
**Optional:** a prior plan revision (when revising), existing architecture/design decisions if already drafted.
**Source reference order:** this work item's own brief.md/decisions.md/status.md (if it already exists) → [project context](../../project.md) → [policies](../../policies.md) → [applicable rules](../../rules/common.md) → the [template](../../templates/plan.md).

## 3. Execution steps and applicable rules

1. Identify scope, dependencies, assumptions and unresolved decisions — record unresolved decisions explicitly per [common rules](../../rules/common.md).
2. List artifacts, applicable skills, verification — including the relevant checklist from ai/checklists/ — and stop conditions.
3. Specify external actions and permissions separately from local implementation, per the External operations section of [policies](../../policies.md).
4. Record the reviewed plan revision and approval source without inventing approval, per the Plan and authorization section of [policies](../../policies.md).
5. When revising, never delete or overwrite an earlier revision. Close the previous revision in place (fill in its Outcome column and Closure line), append the new revision after it as its own `## Revision {N}` section, and update the revision index table at the top. `plan.md` must read in chronological order, oldest revision first; don't move old revisions into an appendix below the new one.

## 4. Required tools/scripts and environmental conditions

None. Markdown authoring only; no build or runtime environment is required.

## 5. Output artifacts, templates, ID conventions, and storage locations

`work-items/<WI-###>/plan.md` and `status.md`, starting from the [template](../../templates/plan.md). `plan.md` holds every revision of the plan, in full and in chronological order. The `WI-###` work item ID is assigned once and stays stable for the item's lifetime; reuse it in every artifact this work item produces.

## 6. Checklist and repeatable verification method

No dedicated checklist file. Repeatable check: every time the plan is drafted or revised, re-verify plan.md's own "Review status, approval source and approved revision" section is filled in and every step lists a verification method. On a revision, also check that every earlier revision is still present, unchanged except for its Outcome/Closure, in chronological order, and that the index table marks exactly one revision as current. Compare with `git diff`/`git show` on plan.md when it has been committed.

## 7. Termination criteria and failure handling

Done when scope is concrete, every step has a verification method, and approval state is explicit (approved, or explicitly still draft/under review — never left ambiguous). If a missing decision affects scope, apply the pause conditions in [policies](../../policies.md): describe the question, impact, options and recommendation; do not invent approval or proceed past the blocked step.

## 8. Work item update procedure and handover for the next step

Create or update `status.md` (state, e.g. draft/awaiting-plan-review/ready) and `decisions.md` for any open question raised while planning. Hand the approved `plan.md` to whichever skill executes its first step (typically `requirements`, `architecture`, or directly `implementation` for a small bug-fix).
