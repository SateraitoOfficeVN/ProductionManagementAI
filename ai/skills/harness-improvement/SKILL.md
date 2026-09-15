---
name: harness-improvement
description: Propose, evaluate and record a change to shared harness guidance — a skill, rule, policy, workflow, checklist or template.
---

# harness-improvement

## 1. Purpose and usage scenario

Propose, evaluate and record a change to shared harness guidance under `ai/` — a skill, rule, policy, workflow, checklist or template. Use when an observed failure, gap or project change makes existing guidance wrong, incomplete or stale; never to make an undocumented ad-hoc change to a shared file.

## 2. Mandatory inputs, optional inputs, and source reference order

**Mandatory:** the observed problem or gap, the affected file(s) under `ai/`.
**Optional:** a project change that makes existing guidance stale (in place of an observed failure).
**Source reference order:** the affected `ai/` file(s) as they exist today → [project context](../../project.md) → [policies](../../policies.md) → [applicable rules](../../rules/common.md) → `ai/evaluations/baseline-cases.md` → the [template](../../templates/improvement.md).

## 3. Execution steps and applicable rules

1. Record the observed problem and its evidence; identify the affected skill, rule, policy, workflow, checklist or template.
2. Propose the smallest useful change and the behavior it is expected to produce, per [common rules](../../rules/common.md) ("keep changes focused").
3. Evaluate the change against relevant cases in `ai/evaluations`, adding a new case when none covers this failure mode.
4. Record the risk and rollback plan, and any remaining limitation; submit for review before shared adoption, per the Harness improvements section of [policies](../../policies.md).

## 4. Required tools/scripts and environmental conditions

None beyond version control, which provides the rollback path. No build or runtime environment is required.

## 5. Output artifacts, templates, ID conventions, and storage locations

`work-items/<ID>/improvement.md`, starting from the [template](../../templates/improvement.md), and, once adopted, the updated `ai/` file(s) themselves. No new ID is assigned to the improvement record; it references the `ai/` file path(s) it changes.

## 6. Checklist and repeatable verification method

The relevant case(s) in `ai/evaluations/baseline-cases.md`; the repeatable method is re-running every case the change touches, not only the one that motivated it — a change to shared guidance can affect scenarios beyond the one observed.

## 7. Termination criteria and failure handling

Done when evaluation cases have a recorded pass/fail result, the change has an explicit rollback path, and no gate was silently relaxed to pass an evaluation. If adoption requires a policy/gate change beyond the smallest useful fix, apply the pause conditions in [policies](../../policies.md) and get explicit review before shared adoption.

## 8. Work item update procedure and handover for the next step

Update `improvement.md`, `status.md` and `decisions.md` with the review outcome and adopted revision. Once adopted, the change is live for every skill that reads the changed `ai/` file — no separate handover step is needed beyond the commit itself.
