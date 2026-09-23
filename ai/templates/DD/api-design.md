<!-- API Specification Design (API仕様設計) template, a companion to detailed-design.md, based on the reference workbook at ai/templates/example/DD/API/70-00-02.API仕様設計_会員情報.xlsx. Always produced alongside detailed-design.md for every DD, whether its endpoints are large, small, shared or screen-owned (see that template's header for file naming and the one-home-per-content rule); a DD whose screen calls no API still gets this file, with its sections marked "Not applicable — {reason}". Copy into the relevant work item or docs/en/020_detailed-design area, next to the detailed-design.md file(s) that reference it; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval.
The source workbook's "called container" dispatch concept (one physical file serving many operations via a mode flag) and its per-operation-mode required-ness matrix are legacy-ASPX-specific; this template exposes a plain single "Required" column per field, with an optional per-context expansion only when this project's API genuinely reuses one endpoint for more than one calling scenario. Its "VH content" response-conversion column is generalized as "Value mapping". -->

# {API / Module Name} — API Specification Design (API仕様設計)

{ID, e.g. ###_DD-API} — implements/supports {###_DD, …}, requirements {REQ-###, …}.

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

## Overview and operation catalog

| Field | Value |
| --- | --- |
| API / module name | {e.g. CustomerInfoApi} |
| Overview | {one-line summary of what this API covers} |

{Operation catalog: one row per endpoint/method defined below, generalizing the reference workbook's routing table (which mapped an operation code to the container/function serving it).}

| No | Endpoint / method | Handler | Purpose | Notes |
| --- | --- | --- | --- | --- |
| {1} | {METHOD /path, or method name} | {controller/service method that implements it} | {} | {} |

### System-wide API registry entries

{Mirrors the reference workbook's X-4 API-ID appendix (a project-wide registry of every endpoint in the system). List only the entries this document defines; if this project keeps a single project-level API registry elsewhere, put "see {that document}" instead of duplicating it here.}

| ID | Endpoint | Overview | Notes |
| --- | --- | --- | --- |
| {} | {} | {} | {} |

## Endpoint / method design

{Repeat this block once per endpoint or method.}

### {METHOD} {/path}

| Field | Value |
| --- | --- |
| Description | {what this endpoint/method does} |
| Return type | {type, e.g. a response DTO, or "void"} |
| Created by / date | {name} / {YYYY-MM-DD} |
| Last modified by / date | {name} / {YYYY-MM-DD}, {what changed} |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| {1} | {} | {} | {} |

{"None" if it takes no arguments beyond the request body below.}

Processing overview: {prose summary of the algorithm/flow}.

**Processing flow**

{Cite what each step calls in "Calls" — another endpoint in this document, a function-design.md method, or a module in detailed-design.md — per the reference workbook's cross-document step references.}

| Step | Description | Calls |
| --- | --- | --- |
| {1} | {} | {endpoint/method/module name, or "—"} |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| {} | {} | {} |

**Request fields**

{Add one "Required" column per calling context only when this endpoint is genuinely shared by more than one action/screen with different required-field subsets, mirroring the reference workbook's per-operation-mode columns; otherwise a single "Required" column is enough.}

| No | Name | Variable name | Type | Length | Required | Source | Example | Description | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| {1} | {} | {} | {} | {} | {yes \| no} | {request body \| query \| route \| session, …} | {} | {} | {} |

**Response fields**

{"Value mapping" mirrors the reference workbook's "VH content" column — how a raw/stored value is converted to what the field actually returns (e.g. a status code to a label), or "—" when the field is passed through unchanged.}

| No | Name | Variable name | Type | Length | Required | Repeats (array) | Value mapping | Example | Description | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| {1} | {} | {} | {} | {} | {yes \| no} | {yes \| no} | {} | {} | {} | {} |

**Error codes**

| Code | Meaning | HTTP status |
| --- | --- | --- |
| {} | {} | {} |

## Unresolved decisions

{List, or "none" — link each to decisions.md.}
