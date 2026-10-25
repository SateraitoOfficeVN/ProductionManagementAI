# WI-017 — Production order notes limit 500 → 1000 characters (備考の上限変更) — Product Brief

## Status

| Work item | Author | Status | Target release |
| --- | --- | --- | --- |
| WI-017 | Claude (for ThongTM) | in progress — local only | unscheduled |

Workflow: feature-delivery (change to an existing rule). Baseline: `master` at `600a5ce`.

## Overview

Planners write packing, routing and supplier instructions into 「備考」 (Notes) on 製造指示登録・編集 (SCR-001).
500 characters is not always enough. This work item raises the limit to 1000 characters everywhere the limit lives:
basic design, detailed design (screen, API, functions), database design, database column, server and client
validation, the message text and the tests.

## Objective

User request, 2026-10-24: a demo in which one instruction to Claude updates every related artifact — a new basic
design, new detailed (spec) designs, a new database design (each new design file named with its update date) and the
code — instead of doing it by hand. Scenario chosen by the user: notes limit 500 → 1000. The user asked for the
design documents as a batch (no review stop between files), PDFs kept, and local work only (no push, PR or merge).

## Requirements

| ID | Requirement | Acceptance criteria | Priority |
| --- | --- | --- | --- |
| REQ-090 | Notes are optional, at most 1000 characters (Unicode code points). Supersedes the 500 limit of REQ-016 (WI-002); every other part of REQ-016 (trim, empty → `null`) is unchanged. | **Success:** empty notes and exactly 1000 characters (an emoji counts as 1) are accepted on create and edit, through the screen and the API, and stored unchanged. **Failure:** 1001 characters are rejected with MSG-E006 「備考は1000文字以内で入力してください。」 (Notes can't exceed 1000 characters.) on the field; nothing is saved. | must |
| REQ-091 | The notes counter on SCR-001 shows `{n}/1000`, turns red above 1000 and is announced politely from 900 characters. | Counter text and colour follow the new limit; the live-region threshold keeps the old 90 % ratio. | must |
| REQ-092 | Existing orders keep their notes; the database column accepts 1000 characters. | Migration widens `production_orders.notes` to `varchar(1000)` without rewriting or losing rows; the app's restricted login keeps working. | must |

## Out of scope

- The plant-calendar exception reason (006, also 500 code points) — a different field.
- Any other field, message or screen. CSV export columns are unchanged; only its documents' reference to the limit
  is updated.
- Push, PR, merge and deployment (user: local only).

## Design documents affected (new dated revisions, 2026-10-24)

The approved files stay unchanged; each affected document gets a full new revision named
`<existing name>_20261024.md` in the same folder (DEC-002).

| Document | Why |
| --- | --- |
| 001_BD | Items 11–12, V-05, MSG-E006 |
| 001_DD, 001_DD-API, 001_DD-FN, 001_DD-SPD | Field spec, counter, message catalog, API field length, DTO length, counter behaviour |
| 001_DB | `notes varchar(1000)`, new migration |
| 002_DD-API-CSV, 002_DD-FN-CSV | Their notes column quotes the limit |
