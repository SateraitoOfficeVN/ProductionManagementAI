---
name: screen-design
description: Define a screen layout or detailed UI behavior consistent with BD and DD.
---

# screen-design

## 1. Purpose and usage scenario

Specify a single screen's layout/navigation (BD-level) or its fields, interactions and states (DD-level). Use whenever a screen needs more detail than the BD's screen list gives, either during basic-design (layout/navigation) or detailed-design (fields/interactions).

## 2. Mandatory inputs, optional inputs, and source reference order

**Mandatory:** screen ID, the requirements it implements, BD.
**Optional:** requested design fidelity (layout-only vs. full interaction spec), existing visual artifacts.
**Source reference order:** the target screen's entry in basic-design.md (and detailed-design.md, if it already exists) → [project context](../../project.md) → [policies](../../policies.md) → [applicable rules](../../rules/frontend.md) → the [template](../../templates/detailed-design.md).

## 3. Execution steps and applicable rules

1. For BD-level work, describe layout and navigation; for DD-level work, specify fields and interactions.
2. Specify validation, loading/empty/error/success states, and WCAG 2.2 AA behavior (keyboard operability, focus order, contrast) — per [frontend rules](../../rules/frontend.md).
3. Map detailed interactions to the agreed API; do not select an unapproved UI framework, per [frontend rules](../../rules/frontend.md) ("UI framework is not selected yet").

## 4. Required tools/scripts and environmental conditions

None required. An optional mockup tool may be used; it does not imply a UI framework choice.

## 5. Output artifacts, templates, ID conventions, and storage locations

Screen sections written directly into the relevant BD (`docs/en/010_basic-design/`) or DD (`docs/en/020_detailed-design/`), plus linked visual artifacts when needed. Uses the screen's existing `SCR-###` ID from basic-design.md; do not assign a new ID for the same screen.

## 6. Checklist and repeatable verification method

Work through the [design-consistency checklist](../../checklists/design-consistency.md); repeat it whenever the screen's spec changes, including a fidelity upgrade from layout-only to full interaction spec.

## 7. Termination criteria and failure handling

Done when field behavior, interaction states, accessibility and API mapping agree, and the design-consistency checklist passes. If the interaction depends on an API contract that isn't settled yet, apply the pause conditions in [policies](../../policies.md) instead of guessing the contract.

## 8. Work item update procedure and handover for the next step

Update `status.md` and `decisions.md` with any open question. Hand the finished screen section back to `basic-design` (if it was layout-only) or to `detailed-design`/`implementation` (once fields/interactions/API mapping are complete).
