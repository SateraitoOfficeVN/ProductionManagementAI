<!-- Screen Processing Design (画面処理設計) template, a companion to detailed-design.md, based on the reference workbook at ai/templates/example/DD/画面処理設計/00-04-01.画面処理設計_会員登録_入力画面.xlsx. Always produced alongside detailed-design.md for every DD (see that template's header for file naming and the one-home-per-content rule): the step-by-step processing (branching, error handling, redirects, per-component breakdown) always lives here, and detailed-design.md's "Processing and state transitions" keeps only the resulting state-transition table and a pointer table to this document's sections. A DD with no screen (e.g. backend-only) documents its request-boundary processing here, or marks the sections "Not applicable — {reason}". Copy into the relevant work item or docs/en/020_detailed-design area, next to the detailed-design.md file it elaborates; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval.
The source workbook gives one processing-definition block per physical file/component (the page itself, plus each significant partial/include it composes); this template keeps that shape as "one block per file/component", generalized to a frontend component, a backend handler, or an API/service call boundary — whatever this project's actual unit of composition is. -->

# {Screen Name} — Screen Processing Design (画面処理設計)

{ID, e.g. DD-###-SPD} — elaborates {DD-###}, implements {BD-###}, requirements {REQ-###, …}.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | {ID} |
| System name | {system name} |
| Subsystem name | {subsystem name, or "not applicable"} |
| Work item | {WI-###} |
| Created by | {name} |
| Created date | {YYYY-MM-DD} |
| Last updated by | {name} |
| Last updated date | {YYYY-MM-DD} |

| Version | Date | Author | Revision content |
| --- | --- | --- | --- |
| 1 | {YYYY-MM-DD} | {name} | Initial creation |

## Overview and process list

| Field | Value |
| --- | --- |
| Screen / file name | {e.g. ProductionOrderForm.tsx} |
| Overview | {one-line summary of what this screen does} |

| No | Process name | Overview | Notes |
| --- | --- | --- | --- |
| {1} | {file/component processing block defined below} | {} | {} |

### Reference documents

| No | Document | Purpose / use | Notes |
| --- | --- | --- | --- |
| {1} | {basic-design.md, detailed-design.md, api-design.md, …} | {why it's referenced} | {} |

## Processing design

{Repeat this block once per file/component that has its own non-trivial processing (the screen's root component, plus any significant sub-component/partial/event handler worth documenting separately). Number steps in execution order; branch/condition steps should say what happens on each outcome.}

### {File / component name}

| Field | Value |
| --- | --- |
| Detail | {one-line function description} |
| Created by / date | {name} / {YYYY-MM-DD} |
| Last modified by / date | {name} / {YYYY-MM-DD}, {what changed} |

Processing overview: {prose summary}.

**Used components / services**

{Generalizes the reference workbook's "使用INCファイル" (used include files).}

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| {} | {} | {} | {} |

{"None" if this block calls nothing external.}

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| {1} | {} | {condition and each outcome, or "—"} | {module/component/endpoint name, or "—"} | {} |

## Unresolved decisions

{List, or "none" — link each to decisions.md.}
