# WI-015 — Production lines screen redesign (SCR-005)

| Work item | Workflow | Status | Baseline |
| --- | --- | --- | --- |
| WI-015 | bug-fix (design + implementation alignment, as WI-012) | Merged via PR #45 as 2470c8e | 2f502b1 |

## Report and objective

On 2026-10-06 the user asked to check 生産ライン・工程 (SCR-005, `/production-lines`) against its mockup
and design. The audit found many gaps (below). The user then asked to "read requirement and re-design for
production line". Objective: produce a new, simpler SCR-005 screen design that traces to
[005_REQ](../../docs/en/000_requirements/005/005_REQ_production-lines.md) REQ-064–REQ-069 and the confirmed
WI-009 business rules. After review, make the running screen match it. The approved WI-009 design files stay
unedited; the new design is additive, as WI-012 did for SCR-006.

## Defect and evidence (BUG-009)

Audit of the running app (local Compose, `2f502b1`, Chromium 1280 and 390 px) against
[005_DD-SPD](../../docs/en/020_detailed-design/005/005_DD-SPD_生産ライン・工程.md) and its
[20-state mockup](../../docs/en/020_detailed-design/005/mockups/005_DD-SPD_SCR-005.ja.html):

- **Wording differs from the SPD §5 catalog:**

  | Item | Design | App |
  | --- | --- | --- |
  | New action | 新規ライン | 生産ラインを登録 |
  | Headings | 生産ライン登録 / 生産ライン編集 | 生産ラインを登録 / 生産ラインを編集 |
  | Products section | 生産可能な製品 | 製造可能な製品 |
  | Timing field | 製造時間／単位 | 生産時間 (分／1単位) |
  | Unit confirmation | 「現在の単位で確認」 button | 「表示単位で生産時間を確認しました」 checkbox |
  | Line retire dialog | このラインを使用停止にしますか？ | 生産ラインを使用停止にしますか？ |
  | Pair retire dialog | この製品を使用停止にしますか？ | 製品との関連を使用停止にしますか？ |
  | Count / pager | 「2 件」 and 「前へ 1 / 1 次へ」 | 検索結果：N件 and Nページ |

- **Behaviour:** DD §6 requires distinct "no lines yet" and "no matches" notices; the app shows one message
  for both. The staged pair-retirement notice 「使用停止予定（保存後に反映）」 is missing.
- **List visuals:** state is plain text instead of a badge; Retire is grey instead of danger red; the table
  header has no grey background; the SP card layout differs.
- **Form visuals:**
  - Missing: 必須 markers, field hints, the grey read-only code, product cards with a state badge and a
    read-only current unit, and the unit-reconfirmation warning box.
  - Different: Save/Cancel order and alignment; the heading shows a visible focus box.
  - Extra: a 「状態: 使用中」 line.
- **Product add:** the app always shows a "製品を追加" panel listing up to 50 products, each with its own
  button, so the edit page grows to about 4,200 px on PC. The approved mockup shows only a 「製品を追加」
  button and never depicts the picker, so the design itself has a gap here.
- **Already matching:** 48 px controls (SPD §2), routes, default state 使用中, 50 rows per page, table on PC
  and cards on SP, a table caption, a centred retire dialog, and Retire only on active lines.

Moderate UX/design-consistency defect; no data loss or security impact found.

## Users and primary tasks (RFC 0013 preference)

Admin/Operator master-data maintainers (REQ-064–069, UC-017):

1. Find a line: search code/name, filter state.
2. Register a line: code, name, hours/day.
3. Set which products the line makes and the minutes per one product unit.
4. Reconfirm a timing after the product's unit changed.
5. Retire a line or a line/product pair, keeping history.

The redesign keeps every confirmed rule:
- immutable case-insensitive code;
- hours greater than 0 and at most 24, with at most 3 decimals;
- minutes greater than 0, with at most 3 decimals, per one displayed unit;
- explicit unit reconfirmation;
- staged pair retirement and immediate line retirement, with no restore;
- conflict and unknown-save handling with no replay;
- dirty-draft guard;
- Admin/Operator only;
- WCAG 2.2 AA.

## Acceptance criteria

| ID | Requirement |
| --- | --- |
| REQ-082 | A new additive design `005_DD-SPD-REDESIGN_生産ライン・工程.md` traces every visible region, field, action and state of SCR-005 (list, register, edit, product add, unit reconfirmation, staged/immediate retirement, empty/no-match/loading/error, conflict/unknown, forbidden, discard, SP) to REQ-064–069 and the approved BD/DD rules. It includes the product-add picker the old mockup omitted, SVG wireframes (PC, SP, dialogs) and EN/JA HTML state mockups, and English/Japanese PDFs. Japanese UI text comes from one catalog table. |
| REQ-083 | The design is simpler for the user: default-visible items each serve a task, the page does not list every candidate product inline, and the wording uses business language. No database or business-rule change. The only API changes are optional allow-listed page sizes: `pageSize` on API-PL-01 for 「表示件数」 10/20/50/100, as on 製品マスタ (DEC-006), and `pairsPageSize` on API-PL-02 and `pageSize` on API-PL-06 for 20-row pages on the form (DEC-008). Lists scroll inside fixed-height boxes (DEC-009). |
| REQ-084 | After design approval, the running SCR-005 on PC and SP matches the new mockups in control order, grouping, labels, conditional visibility and states, verified with screenshots, unit/E2E tests and axe. |

## Out of scope

Edits to approved WI-009 design files; database changes; API changes beyond the optional page sizes of DEC-006 and DEC-008;
SCR-001/002 order-assignment UI; scheduling/capacity; new roles; deployment.
