---
name: detailed-design
description: Create implementation-ready processing and API specifications from BD.
---

# detailed-design

## 1. Purpose and usage scenario

Turn a BD into an implementation-ready specification: fields, validation, processing sequence, state transitions, API contract and persistence mapping. Use once BD, architecture and database-design (or their drafts) are available, right before implementation starts.

## 2. Mandatory inputs, optional inputs, and source reference order

**Mandatory:** BD, acceptance criteria.
**Optional:** architecture (when a new boundary is involved), DB design or drafts.
**Source reference order:** BD → database-design.md (or its draft) → relevant ADRs → [project context](../../project.md) → [policies](../../policies.md) → [applicable rules](../../rules/backend.md) → the [template](../../templates/detailed-design.md).

## 3. Execution steps and applicable rules

1. Specify the screen layout/mockup: an SVG wireframe (never ASCII art) with a region-to-field mapping, drawn per the [template](../../templates/detailed-design.md), plus a rendered mockup covering the screen's key states (create/empty, populated/edit, any locked-or-restricted variant, validation-error, success) — see [screen-design](../screen-design/SKILL.md) for how to produce and link it. Then specify fields, validation, processing sequence and state transitions, the last as a Mermaid state diagram above the transition table.
2. Define API request/response/error behavior (RFC 9457 Problem Details for backend errors, no leaked implementation detail), persistence mapping, and what gets traced/logged (OpenTelemetry spans, metrics) for the endpoint — per [backend rules](../../rules/backend.md).
3. Produce all four documents of the DD template family every time, each as its own Markdown file: the main DD plus the [api-design](../../templates/DD/api-design.md), [function-design](../../templates/DD/function-design.md) and [screen-processing-design](../../templates/DD/screen-processing-design.md) companions. Endpoint catalogs go in the API companion, backend service/Application-layer methods in the function-design companion, and step-by-step processing flows in the screen-processing companion. The main DD keeps screen items, states, screen-owned modules and the state-transition table, and points to the companions instead of repeating them. Never skip a companion because the main DD "could hold it"; if its subject doesn't exist for this DD, produce it with "Not applicable — {reason}" sections.
4. Resolve inconsistencies with DB/API before dependent implementation begins.

## 4. Required tools/scripts and environmental conditions

The `design` skill for the rendered mockup (step 1); no live backend runtime is needed to produce the rest of the DD, since API/DB drafts are read, not executed. A DD with no visual screen (e.g. a backend-only endpoint) needs no mockup tool.

## 5. Output artifacts, templates, ID conventions, and storage locations

Four files in the number folder `docs/en/020_detailed-design/###/`, always all four, starting from the [main template](../../templates/detailed-design.md) and its companions in [`ai/templates/DD/`](../../templates/DD):

| Document | File name | Document ID |
| --- | --- | --- |
| Main DD | `###_DD_{画面名}.md` | `###_DD` |
| API specification | `###_DD-API_{画面名}.md` | `###_DD-API` |
| Function design | `###_DD-FN_{画面名}.md` | `###_DD-FN` |
| Screen processing design | `###_DD-SPD_{画面名}.md` | `###_DD-SPD` |

`{画面名}` is the screen's Japanese name, the same as on its BD, and all four files use it (RFC 0011).

Each references the `###_BD`/`SCR-###`/`REQ-###` IDs it implements rather than restating them. The rendered mockup's source (if any) sits in `docs/en/020_detailed-design/###/mockups/`. All four files are also rendered to English and Japanese PDFs under `docs/en/pdf/` and `docs/ja/pdf/`, in the same change, per the [documentation rules](../../rules/documentation.md); the mockup itself is not rendered.

## 6. Checklist and repeatable verification method

Work through the [design-consistency checklist](../../checklists/design-consistency.md); repeat it whenever the API contract or DB mapping changes after the DD was first written. Also confirm that all four DD files exist, that the main DD's "Companion design documents" table lists all three companions, and that no content is duplicated across them (each piece has one home; the others point to it).

## 7. Termination criteria and failure handling

Done when all four DD documents exist, the design is traceable to BD, precise enough for code and tests, and the design-consistency checklist passes. A DD with a missing companion is not done. If the DD can't be reconciled with the current DB/API without a new decision, apply the pause conditions in [policies](../../policies.md) instead of implementing an inconsistency.

## 8. Work item update procedure and handover for the next step

Update `status.md` and `decisions.md` with any open question. Hand the DD to `implementation` (as its primary spec) and to `testing` (as the source of test scenarios).
