<!-- Function / Module Design (機能設計) template, a companion to detailed-design.md, based on the reference workbook at ai/templates/example/DD/Functions/90-11-02.機能設計_customer_inc.xlsx. Always produced alongside detailed-design.md for every DD (see that template's header for file naming and the one-home-per-content rule). It covers the backend service/Application-layer methods behind the DD's endpoints — whether shared across screens (e.g. a domain service, a shared helper) or used by this screen only; screen-owned domain entities/rules, UI components and controllers stay in detailed-design.md's "Module design". A DD with no backend methods still gets this file, with its sections marked "Not applicable — {reason}". Copy into the relevant work item or docs/en/020_detailed-design area; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval.
The source workbook's "リクエストデータ"/"VH内容" sheets (request parameters this module sends, and response/value-mapping it returns, when it calls an external API) are file-level tables shared by every method below, not per-method — kept that way here. -->

# {Module Name} — Function Design (機能設計)

{ID, e.g. ###_DD-FN} — used by {###_DD, …}, requirements {REQ-###, …}.

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

## Overview and method index

| Field | Value |
| --- | --- |
| Module name | {e.g. CustomerService} |
| Overview | {one-line summary of what this module covers} |

| No | Method name | Overview | Notes |
| --- | --- | --- | --- |
| {1} | {method name defined below} | {} | {} |

### Shared utility references

{Mirrors the reference workbook's インクルードファイル定義 appendix: shared, project-wide utility functions/modules this module relies on (distinct from the per-method "Calls" below, which are its own methods' step-level dependencies). If this project keeps a project-level utilities reference elsewhere, put "see {that document}" instead of duplicating it here.}

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| {} | {} | {} | {} |

## Request data

{Parameters this module sends when it calls another API/service, shared across its methods below. "None" if this module doesn't call out to anything.}

| No | Name | Variable name | Type | Length | Used by (which method(s)) | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| {} | {} | {} | {} | {} | {} | {} |

## Response / value mapping

{This module's response fields and any value-conversion rules, shared across its methods below (mirrors the reference workbook's "VH content"). "None" if not applicable.}

| No | Name | Variable name | Type | Length | Required | Value mapping | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| {} | {} | {} | {} | {} | {yes \| no} | {} | {} |

## Method design

{Repeat this block once per method.}

### {Method name}

| Field | Value |
| --- | --- |
| Description | {what it does, one or two sentences} |
| Return type | {type, or "void"} |
| Created by / date | {name} / {YYYY-MM-DD} |
| Last modified by / date | {name} / {YYYY-MM-DD}, {what changed} |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| {1} | {} | {} | {} |

{"None" if it takes no arguments.}

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| {} | {} | {} |

Processing overview: {prose summary of the algorithm/flow. If this method replaces or supersedes prior logic, note what changed and why.}

**Processing flow**

{Cite what each step calls in "Calls" — an api-design.md endpoint, another method in this document, or a module in detailed-design.md — per the reference workbook's cross-document step references (e.g. "see api-design.md, CustomerInfoApi §GetCustomerInfo").}

| Step | Description | Calls |
| --- | --- | --- |
| {1} | {} | {endpoint/method/module name, or "—"} |

## Unresolved decisions

{List, or "none" — link each to decisions.md.}
