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

1. Specify the screen layout/mockup: an ASCII sketch with a region-to-field mapping, plus a rendered mockup covering the screen's key states (create/empty, populated/edit, any locked-or-restricted variant, validation-error, success) — see [screen-design](../screen-design/SKILL.md) for how to produce and link it. Then specify fields, validation, processing sequence and state transitions.
2. Define API request/response/error behavior (RFC 9457 Problem Details for backend errors, no leaked implementation detail), persistence mapping, and what gets traced/logged (OpenTelemetry spans, metrics) for the endpoint — per [backend rules](../../rules/backend.md).
3. Resolve inconsistencies with DB/API before dependent implementation begins.

## 4. Required tools/scripts and environmental conditions

The `design` skill for the rendered mockup (step 1); no live backend runtime is needed to produce the rest of the DD, since API/DB drafts are read, not executed. A DD with no visual screen (e.g. a backend-only endpoint) needs no mockup tool.

## 5. Output artifacts, templates, ID conventions, and storage locations

DD under `docs/en/020_detailed-design/`, starting from the [template](../../templates/detailed-design.md). Document ID `DD-###`, referencing the `BD-###`/`SCR-###`/`REQ-###` IDs it implements rather than restating them.

## 6. Checklist and repeatable verification method

Work through the [design-consistency checklist](../../checklists/design-consistency.md); repeat it whenever the API contract or DB mapping changes after the DD was first written.

## 7. Termination criteria and failure handling

Done when the design is traceable to BD, precise enough for code and tests, and the design-consistency checklist passes. If the DD can't be reconciled with the current DB/API without a new decision, apply the pause conditions in [policies](../../policies.md) instead of implementing an inconsistency.

## 8. Work item update procedure and handover for the next step

Update `status.md` and `decisions.md` with any open question. Hand the DD to `implementation` (as its primary spec) and to `testing` (as the source of test scenarios).
