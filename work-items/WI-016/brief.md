# WI-016 — Production order CSV export (製造指示一覧 CSV出力) — Product Brief

## Status

| Work item | Author | Status | Target release |
| --- | --- | --- | --- |
| WI-016 | Claude (for ThongTM) | in review | unscheduled |

Workflow: feature-delivery. Baseline: `master` at `a470146`.

## Overview

Planners and line leaders keep asking for production orders in a spreadsheet (to share with suppliers, check
schedules offline, or paste into reports). Today they can only read the list screen 製造指示一覧 (SCR-002) one page
at a time. This work item adds a **CSV出力** (Export CSV) action to SCR-002 that downloads the orders the user is
currently looking at as a CSV file that opens correctly in Japanese Excel.

## Objective

User request, 2026-10-07: "add future export CSV for production order" (read as *feature*). WI-003 listed
"Export (CSV/Excel) and printing" as out of scope for Screen B; this work item delivers the CSV half. It is also the
task recorded for the AI-lifecycle demo video (brief → design → plan → implementation → tests → PR).

## Success metrics

| Goal | Metric | Target |
| --- | --- | --- |
| Export what is on screen | CSV rows equal the list's filtered, sorted result set (all pages) | 100 % match in integration and E2E tests |
| Opens cleanly in Excel (Japanese) | Japanese text, dates and decimals readable without an import wizard | Verified by opening a generated file; encoding/BOM checked by test |
| Safe | No formula injection, no access without sign-in/role | Security-review checklist passed; tests for 401/403 and `=`/`+`/`-`/`@` cells |

## Assumptions

- Same users and roles as SCR-002: signed-in `Admin` and `Operator` (WI-002 DEC-001). No new permission.
- Read-only: export changes no data and writes no audit record (the list itself writes none).
- The export uses the list's existing filters and sort (002_BD 0-3) — the user filters first, then exports.
- No database schema change; the existing list query and indexes (002_DB) are reused.
- UI text is Japanese and goes into the existing catalog `messages.ts`; CSV header names are Japanese.

## Actors and user stories

| Actor | As a… | I want to… | So that… | Use case ID |
| --- | --- | --- | --- | --- |
| Admin / Operator | production planner | download the orders matching my current filters as CSV | I can share and analyse them in Excel | UC-022 |
| Admin / Operator | line leader | see a clear message when there is nothing to export or the export fails | I know whether the file is complete | UC-022 |

## Requirements (in scope)

| ID | Requirement | Acceptance criteria | Priority |
| --- | --- | --- | --- |
| REQ-085 | SCR-002 offers a 「CSV出力」 (Export CSV) action that downloads the orders matching the current filters, in the current sort order, across all pages (not only the visible page) | With a filter that matches N orders over several pages, the file has a header row plus N data rows in the screen's order | must |
| REQ-086 | The CSV has fixed, documented Japanese columns (proposal in plan Q2) with values formatted as on screen: status as its Japanese label, dates `YYYY/MM/DD`, timestamps `YYYY/MM/DD HH:mm` plant time, exact decimal quantities with the unit | Column order and formats match the DD; a kg/m decimal quantity round-trips exactly | must |
| REQ-087 | The file opens correctly in Japanese Excel: RFC 4180 quoting, CRLF line ends, encoding per plan Q3 (recommended UTF-8 with BOM), filename with the plant-local timestamp | Test checks BOM/encoding, quoting of commas, quotes and newlines; manual open in Excel or LibreOffice recorded | must |
| REQ-088 | Export is safe: only authenticated Admin/Operator (401/403 otherwise); the same parameter validation as the list (400 on invalid values); cells starting with `=`, `+`, `-`, `@`, tab or CR are neutralised against spreadsheet formula injection; an upper row limit (plan Q4) returns a clear error instead of an unbounded file | Integration tests for 401, 403, 400, the row limit and injection escaping; security-review checklist | must |
| REQ-089 | The action gives feedback: disabled while a list query or export is running, a busy label during download, an inline message when the result is empty (nothing to export) or the export fails; keyboard and screen-reader accessible on PC and SP | Unit tests, E2E download test on PC and SP, axe passes | must |

## Not doing (out of scope)

- Excel (`.xlsx`) export, printing, PDF — not requested; CSV opens in Excel.
- CSV import / bulk update of orders.
- Export from other screens (製品マスタ, 生産ライン, 稼働カレンダー, dashboard).
- Choosing columns, saving export presets, scheduled or e-mailed exports.
- Order notes (備考) and audit history unless the user chooses them in plan Q2.
- Database schema changes, new roles, deployment.

## Open questions

| Question | Impact if unresolved | Owner | Status |
| --- | --- | --- | --- |
| Q1 Export scope: all matching pages vs. visible page only | REQ-085 behaviour | User | open — see plan revision 1 |
| Q2 Columns (list columns only, or also 備考/作成日時/完了日時) | REQ-086 column set | User | open — see plan revision 1 |
| Q3 Encoding: UTF-8 with BOM vs. Shift_JIS (CP932) | REQ-087 | User | open — see plan revision 1 |
| Q4 Maximum rows per export | REQ-088 limit and message | User | open — see plan revision 1 |
