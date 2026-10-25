<!-- Based on ai/templates/test-plan.md (IEEE 829-1998). -->

# WI-017 — Notes limit 500 → 1000 — Test Plan

## Test plan identifier

TP-017, work item WI-017, revision 1, 2026-10-24 (plan revision 1).

## References

[brief.md](brief.md) (REQ-090–REQ-092), [plan.md](plan.md) revision 1, dated design revisions of 2026-10-24:
[001_BD](../../docs/en/010_basic-design/001/001_BD_製造指示登録・編集_20261024.md) v10 (V-05),
[001_DD](../../docs/en/020_detailed-design/001/001_DD_製造指示登録・編集_20261024.md) v7 (items 11–12, test viewpoint),
[001_DB](../../docs/en/database/001/001_DB_製造指示登録・編集_20261024.md) No. 7 (`varchar(1000)`). WI-002 TC-013 is the
case these extend; its 500/501 boundaries move to 1000/1001.

## Test items

| Requirement ID | Description |
| --- | --- |
| REQ-090 | Notes ≤ 1000 code points accepted, 1001 rejected with MSG-E006 「備考は1000文字以内で入力してください。」 |
| REQ-091 | Counter `{n}/1000`, red above 1000, polite live region from 900 |
| REQ-092 | Column `production_orders.notes` is `varchar(1000)`; existing rows kept |

## Test cases

| TC | Requirement | Level | Test | Input / boundary | Expected | Status |
| --- | --- | --- | --- | --- | --- | --- |
| TC-470 | REQ-090 | U (backend) | `ProductionOrderTests.ValidateNotes_Accepts1000CodePoints_IncludingEmojiCountedOnce` | 999 × `a` + 😀 (1000 code points, 1001 UTF-16 units); 1001 × `a` | null; `MSG-E006` | Pass (2026-10-25) |
| TC-471 | REQ-090 | U (backend) | `ProductionOrderServiceTests.Create_InvalidFields_AreReportedBeforeProductLookup` | notes 1001 × `x` with other invalid fields | `notes: [MSG-E006]` among the field errors | Pass (2026-10-25) |
| TC-472 | REQ-090, REQ-092 | I | `ProductionOrderEndpointsTests.Notes_1000CodePointsIncludingEmoji_Accepted_1001Rejected_EmptyStoredAsNull` | POST 1000 code points; POST 1001; POST blanks | 201 (the migrated `varchar(1000)` column accepts it); 400 `notes: MSG-E006`; `notes: null` | Pass (2026-10-25) |
| TC-473 | REQ-090 | U (frontend) | `validation.test.ts` "counts notes in code points" | 999 × `a` + 😀; 1001 × `a`; 1000 × `a` padded with spaces | null; `MSG-E006`; null | Pass |
| TC-474 | REQ-090, REQ-091 | U (frontend) | `ProductionOrderPage.test.tsx` "flags notes over 1000 code points" | Order with 1000-character notes, then type one more | `1000/1000` with `aria-live="polite"`; `1001/1000` and 「備考は1000文字以内で入力してください。」 | Pass |
| TC-475 | REQ-092 | I | Every integration test runs on a database migrated through `WidenProductionOrderNotes` (Testcontainers) | Fresh database | Migrations apply (EF Core 10 `MigrateAsync` also rejects a model/migration mismatch, so the hand-written Designer/snapshot is confirmed); app runs as `pmai_app` | Pass (2026-10-25) |
| TC-476 | REQ-092 | Manual | Upgrade rehearsal on a copy of an existing demo volume: `dotnet ef database update` as the owner, then `\d production_orders` | Existing 124 orders | `notes character varying(1000)`; row count and notes unchanged | Partly passed (2026-10-25): on the disposable E2E database the migration ran over the 124 seeded orders; column became `varchar(1000)`, count 124. No long-lived demo volume exists to rehearse on |

Regression baseline: the whole frontend suite (307 tests), the whole backend suite (286 unit, 234 integration) and the E2E suite (76) — all passed 2026-10-24/25.
