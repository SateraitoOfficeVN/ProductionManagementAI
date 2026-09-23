---
name: basic-design
description: Produce basic design and screen-level behavior from accepted requirements.
---

# basic-design

## 1. Purpose and usage scenario

Turn an accepted brief into business flow, screen-level behavior and the primary/exception paths a feature must support. Use once requirements are stable, before detailed-design or database-design commit to implementation-level specifics.

## 2. Mandatory inputs, optional inputs, and source reference order

**Mandatory:** brief, acceptance criteria.
**Optional:** relevant architecture decisions (when the feature crosses a new trust or system boundary).
**Source reference order:** this work item's brief.md and any prior basic-design.md revision → [project context](../../project.md) → [policies](../../policies.md) → [applicable rules](../../rules/documentation.md) → the [template](../../templates/basic-design.md).

## 3. Execution steps and applicable rules

1. Describe business flow, screen entry/exit and primary actions. Draw the screen transition, and any status workflow, as Mermaid diagrams per the [template](../../templates/basic-design.md), never as ASCII art.
2. Define layout/wireframe and visible states without prematurely fixing implementation details. Draw one grey-box SVG wireframe per breakpoint under `docs/en/010_basic-design/###/wireframes/`, following the template's conventions (numbered callouts that match the item legend), per [frontend rules](../../rules/frontend.md) where UI is involved.
3. Map design sections to requirements, flag security/PII-relevant fields and accessibility needs, and identify unresolved behavior — link requirements to BD per [documentation rules](../../rules/documentation.md).

## 4. Required tools/scripts and environmental conditions

None required. Mermaid renders in GitHub and most Markdown viewers, so no tool is needed for the transition diagram. The SVG wireframes can be written by hand or with any drawing tool that exports plain SVG; check that each one renders (open it in a browser) before handing over. No UI framework choice is implied by either.

## 5. Output artifacts, templates, ID conventions, and storage locations

BD document `###_BD_{slug}.md` in its number folder `docs/en/010_basic-design/###/`, starting from the [template](../../templates/basic-design.md). Document ID `###_BD`; screens use stable `SCR-###` IDs and functions use `FN-###` IDs, both referenced (never restated) from detailed-design.md. Each document under `docs/en/` is also rendered to an English PDF under `docs/en/pdf/` and a Japanese PDF under `docs/ja/pdf/`, in the same change, per the [documentation rules](../../rules/documentation.md).

## 6. Checklist and repeatable verification method

Work through the [design-consistency checklist](../../checklists/design-consistency.md); repeat it whenever the BD is revised, not only on first draft.

## 7. Termination criteria and failure handling

Done when primary and exception flows satisfy the brief, screen IDs are stable, and the design-consistency checklist passes. If a requirement can't be satisfied without a missing business decision, apply the pause conditions in [policies](../../policies.md) rather than guessing the business rule.

## 8. Work item update procedure and handover for the next step

Update `status.md` and `decisions.md` with any open question and the BD's completion state. Hand the BD to `database-design` and `detailed-design` (both consume its screen/requirement mapping) and to `screen-design` for any screen needing deeper UI specification.
