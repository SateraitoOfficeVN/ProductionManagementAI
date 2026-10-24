# WI-017 — Product type (種別) on the product master — Product Brief

## Status

| Work item | Author | Status | Target release |
| --- | --- | --- | --- |
| WI-017 | Claude Code (from user request 2026-10-25) | draft | unscheduled |

## Overview

Products in the product master (SCR-004, `/products`, WI-006/WI-013) have no classification. This adds a
required product type 「種別」 chosen from a fixed list, shown and filterable on the product list and
editable on register/edit.

## Objective

User request 2026-10-25: "add column Type to table Product". Answers recorded in DEC-001–DEC-003.

## Requirements (in scope)

| ID | Requirement | Acceptance criteria | Priority |
| --- | --- | --- | --- |
| REQ-090 | `products` gains a required type code from a fixed list (proposed: 完成品 / 半製品 / 部品 / 原材料, see Q1) | DB rejects null and unknown codes; migration backfills every existing row with the default (Q2) | must |
| REQ-091 | Register/edit form has a required 「種別」 select | Missing/unknown type is a field error; saved value round-trips; Operator and Admin both may edit (as other fields) | must |
| REQ-092 | Product list shows a 「種別」 column (PC) and the type on SP cards | Label shown for every row | must |
| REQ-093 | Product list can be filtered by type (「すべて」 + each type), kept in the URL and preserved by search, Clear, paging, page size and return-after-save | API `GET /api/product-master` accepts optional allow-listed `type`; invalid value → 400 `VALIDATION` | must |

## Not doing (out of scope)

- Type maintenance screen / separate type master (DEC-001: fixed list).
- Type on order screens, dashboard or order CSV export (DEC-003).
- Editing approved WI-006/WI-013 004 design documents (additive documents only, RFC 0015).

## Open questions

| Question | Impact if unresolved | Owner | Status |
| --- | --- | --- | --- |
| Q1 The fixed values and their codes | Enum, DB check constraint, labels | User | answered — DEC-004/DEC-005 |
| Q2 Backfill default for existing / demo products | Migration and seed data | User | answered — DEC-004/DEC-005 |
