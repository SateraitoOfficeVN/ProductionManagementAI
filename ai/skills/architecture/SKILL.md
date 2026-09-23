---
name: architecture
description: Define or revise system architecture for the confirmed stack and feature scope.
---

# architecture

## 1. Purpose and usage scenario

Define or revise the frontend/backend/database boundaries, communication and trust boundaries a feature needs, within the confirmed stack. Use when a feature introduces a new component boundary, a new trust boundary, or a technology decision not yet covered by an existing ADR.

## 2. Mandatory inputs, optional inputs, and source reference order

**Mandatory:** project constraints (confirmed stack), brief.
**Optional:** existing architectural decisions (when revising or extending one).
**Source reference order:** existing ADRs under `docs/en/architecture/` relevant to this feature → [project context](../../project.md) → [policies](../../policies.md) → [applicable rules](../../rules/common.md) → the [template](../../templates/architecture-decision.md).

## 3. Execution steps and applicable rules

1. Describe frontend/backend/database boundaries, communication and trust boundaries (where auth, secrets or external input cross a boundary); for a boundary handling authentication, payment or PII, threat-model it (e.g. STRIDE) before finalizing the decision.
2. Identify decisions required for the feature and assess alternatives proportionally.
3. Record rationale, impact and open decisions; avoid unnecessary layers, per [common rules](../../rules/common.md) ("keep changes focused").

## 4. Required tools/scripts and environmental conditions

None. Markdown authoring only; no build or runtime environment is required.

## 5. Output artifacts, templates, ID conventions, and storage locations

Decision record(s) under `docs/en/architecture/`, one file per decision in its own number folder `NNNN/`, starting from the [template](../../templates/architecture-decision.md). The four-digit sequence follows the MADR convention; the file is named `NNNN_ADR_short-title.md` (e.g. `0001_ADR_...`) and the record is referenced elsewhere as `NNNN_ADR`, per the [documentation rules](../../rules/documentation.md). Each document under `docs/en/` is also rendered to an English PDF under `docs/en/pdf/` and a Japanese PDF under `docs/ja/pdf/`, in the same change, per the [documentation rules](../../rules/documentation.md).

## 6. Checklist and repeatable verification method

When a decision introduces a new trust boundary, work through the [security-review checklist](../../checklists/security-review.md). Repeatable check: re-read the ADR's "Consequences" section against the current brief whenever the feature scope changes, to confirm the decision still holds.

## 7. Termination criteria and failure handling

Done when the architecture supports the requirements and respects confirmed technology choices, and any new trust boundary is covered by the security-review checklist. If a required decision has no acceptable option within the confirmed stack, apply the pause conditions in [policies](../../policies.md) rather than silently picking an unconfirmed technology.

## 8. Work item update procedure and handover for the next step

Update `status.md` and `decisions.md` with the ADR's status and any open decision. Hand the accepted ADR to `basic-design`, `database-design` and `detailed-design` — each must respect the boundaries and decisions it records.
