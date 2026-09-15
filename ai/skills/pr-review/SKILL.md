---
name: pr-review
description: Review a change against requirements, design, correctness and test evidence.
---

# pr-review

## 1. Purpose and usage scenario

Review a change's actual diff against requirements, design and test evidence, and summarize what's safe to merge. Use once implementation and testing produce a diff with recorded check results, before any merge or PR creation/update.

## 2. Mandatory inputs, optional inputs, and source reference order

**Mandatory:** diff, brief, approved plan, check results.
**Optional:** design documents (when the change is design-driven rather than a small fix).
**Source reference order:** the diff itself → work-items/<ID>/evidence.md → brief/plan/design docs it implements → [project context](../../project.md) → [policies](../../policies.md) → [applicable rules](../../rules/git-review.md) → the [template](../../templates/review.md).

## 3. Execution steps and applicable rules

1. Inspect concrete changed behavior and its surrounding context.
2. Prioritize actionable bugs, missing requirements and meaningful regression risks.
3. Give file/line evidence where applicable; avoid speculative findings.
4. For a security-relevant change, apply the [security-review skill](../security-review/SKILL.md); check the change against the [delivery checklist](../../checklists/delivery.md); summarize checks and remaining limits; create or update a remote PR only when authorized.

## 4. Required tools/scripts and environmental conditions

Git (or the repository's VCS) to inspect the diff and repository state, per [git-review rules](../../rules/git-review.md). No build/runtime environment is required beyond what's needed to read the diff and prior check results.

## 5. Output artifacts, templates, ID conventions, and storage locations

Review summary in `work-items/<ID>/review.md`, starting from the [template](../../templates/review.md), plus updated `evidence.md`. No new ID is assigned; findings reference the file:line they concern.

## 6. Checklist and repeatable verification method

Work through the [security-review checklist](../../checklists/security-review.md) (for security-relevant changes) and the [delivery checklist](../../checklists/delivery.md); re-run both after any fix made in response to this review, not only on the first pass.

## 7. Termination criteria and failure handling

Done when findings are actionable, previously-resolved issues are rechecked, and merge rights are not assumed. If a finding can't be resolved within the current authorization (e.g. it needs a design change), apply the pause conditions in [policies](../../policies.md) instead of approving around it.

## 8. Work item update procedure and handover for the next step

Update `review.md`, `evidence.md`, `status.md` and `decisions.md` for any open question. Hand unresolved findings back to `implementation`; hand an approved, checklist-clean change to `ci-cd` for any authorized deployment.
