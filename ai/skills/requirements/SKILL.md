---
name: requirements
description: Turn a feature request into a bounded brief and testable acceptance criteria.
---

# requirements

## 1. Purpose and usage scenario

Turn an ambiguous feature request into a bounded brief with stable requirement IDs and testable acceptance criteria. Use at the start of feature-delivery, right after a request arrives and before planning commits to scope.

## 2. Mandatory inputs, optional inputs, and source reference order

**Mandatory:** the user request, business context.
**Optional:** existing requirements or a prior brief revision (when extending or revising scope).
**Source reference order:** this work item's own prior brief.md/decisions.md (if any) → [project context](../../project.md) → [policies](../../policies.md) → [applicable rules](../../rules/documentation.md) → the [template](../../templates/brief.md).

## 3. Execution steps and applicable rules

1. Identify actors, use cases, in/out-of-scope behavior and the objective/success metric this request serves.
2. Assign stable requirement IDs and concrete acceptance criteria — stable IDs are required by [common rules](../../rules/common.md).
3. Record ambiguous business rules as questions; avoid inventing domain facts, per the "separate confirmed facts, proposals, assumptions and open questions" rule in [documentation rules](../../rules/documentation.md).

## 4. Required tools/scripts and environmental conditions

None. Markdown authoring only.

## 5. Output artifacts, templates, ID conventions, and storage locations

Work item brief and requirements under `docs/en/000_requirements/`, and `work-items/<WI-###>/brief.md`, starting from the [template](../../templates/brief.md). Requirements use stable `REQ-###` IDs; use cases use `UC-###` IDs. Once assigned, an ID is never reused or renumbered — a dropped requirement is marked out of scope, not deleted.

## 6. Checklist and repeatable verification method

No dedicated checklist file. Repeatable check: for every `REQ-###`, confirm it has both a successful-path acceptance criterion and at least one relevant failure-path criterion before treating the brief as ready for planning.

## 7. Termination criteria and failure handling

Done when every in-scope requirement has a stable ID and acceptance criteria covering successful behavior and relevant failure paths, and open questions are recorded rather than silently assumed. If a business rule is ambiguous, record it as an open question in decisions.md per the pause conditions in [policies](../../policies.md) — do not invent the missing domain fact.

## 8. Work item update procedure and handover for the next step

Update `status.md` and `decisions.md` with any open question and the brief's completion state. Hand the completed brief to `planning` (to scope the execution plan) and to `basic-design`/`architecture` (to consume the requirement IDs directly).
