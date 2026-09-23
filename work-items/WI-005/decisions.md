<!-- Based on ai/templates/decisions.md. -->

# Japanese UI and automobile-parts domain — Decision Log

## Log

| ID | Date | Decision needed | Decision maker | Status | Rationale (summary) |
| --- | --- | --- | --- | --- | --- |
| DEC-001 | 2026-09-23 | Japanese only, or switchable languages? | ThanhTN | decided | Japanese only; UI text in one client-side catalog, no i18n library |
| DEC-002 | 2026-09-23 | What happens to the demo data? | ThanhTN | decided | The project becomes production management for automobile parts; demo data in Japanese. The 30 part names are listed below |
| DEC-003 | 2026-09-23 | How do the English design documents quote UI text? | ThanhTN | decided | The real Japanese text followed by an English gloss |
| DEC-004 | 2026-09-23 | The Japanese UI vocabulary | ThanhTN (approved with plan revision 1) | decided | One glossary, applied everywhere |
| DEC-005 | 2026-09-23 | What stays untranslated? | ThanhTN (approved with plan revision 1) | decided | Identifiers and codes: system name, order numbers, product codes, message IDs, routes, API fields |
| DEC-006 | 2026-09-23 | Date, time and number display | ThanhTN (approved with plan revision 1) | decided | `YYYY/MM/DD`, `YYYY/MM/DD HH:mm`, counts with Japanese units, plant timezone unchanged |
| DEC-007 | 2026-09-23 | Where the Japanese strings live | ThanhTN (approved with plan revision 1) | decided | Extend the single catalog (`messages.ts`) with a `labels` section; no new dependency |
| DEC-008 | 2026-09-23 | How the product and note data change reaches existing databases | ThanhTN (approved with plan revision 1) | decided | A new EF migration renames the 30 seeded products (same IDs and codes) and rewrites the seeded notes, guarded to seed rows |
| DEC-009 | 2026-09-23 | Merge PR #17 | ThanhTN | decided | Squash-merged into `master` as `dbc9527` after all three CI jobs passed |

## DEC-001: Japanese only, or switchable languages?

**Status:** decided

### Context

The UI is English, with strings inline in about 17 components plus the message catalog `messages.ts`.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Japanese only | Smallest change; no new dependency; one set of test expectations | English would need a later work item |
| Switchable JA/EN | Both audiences | An i18n library, a switcher, both catalogs maintained, tests per language |

### Decision and rationale

- **Decision:** Japanese only.
- **Decided by:** ThanhTN, 2026-09-23 (answer to the language question).
- **Rationale:** the users read Japanese. Keeping all text in one catalog (DEC-007) leaves room to add a second
  language later without touching components again.

### Impact

| Artifact | Change required |
| --- | --- |
| Frontend | All UI text from the catalog; `<html lang="ja">` |
| Tests | Expected text becomes Japanese |

## DEC-002: What happens to the demo data?

**Status:** decided — part names approved with plan revision 1 (ThanhTN, 2026-09-23).

### Context

Asked whether the English product names should stay, the user answered: "let make the project to be a production
management for automobile parts and update the demo data to this domain in japaneses".

### Decision and rationale

- **Decision:** the project's domain becomes automobile-parts production. The 30 seeded products get Japanese
  automobile-part names; product codes and IDs are unchanged (DEC-005), so every seeded order, figure and test that
  refers to a code keeps working. Seeded order notes ("Demo order 6") become Japanese ("デモ用の製造指示 6").
- **Decided by:** ThanhTN, 2026-09-23.

Names (each keeps its code; the old name is shown for traceability):

| Code | Old name | New name |
| --- | --- | --- |
| P-1001 | Steel bracket | ブレーキキャリパー |
| P-1002 | Aluminium housing | ブレーキディスクローター |
| P-1003 | Control panel assembly | ブレーキパッド |
| P-1004 | Drive shaft | ドライブシャフト |
| P-1005 | Hydraulic valve | 等速ジョイント |
| P-1006 | Gearbox assembly | トランスミッションケース |
| P-1007 | Bearing housing | ハブベアリング |
| P-1008 | Pump impeller | ウォーターポンプ |
| P-1009 | Motor mount plate | エンジンマウント |
| P-1010 | Conveyor roller | ラジエーター |
| P-1011 | Spur gear 40T | タイミングギア |
| P-1012 | Coupling flange | クラッチディスク |
| P-1013 | Pneumatic cylinder | ショックアブソーバー |
| P-1014 | Sensor bracket | コイルスプリング |
| P-1015 | Cable harness A | エンジンワイヤーハーネス |
| P-1016 | Cable harness B | ボディワイヤーハーネス |
| P-1017 | Terminal block unit | ヘッドランプユニット |
| P-1018 | Relay module | リレーボックス |
| P-1019 | Power supply unit | オルタネーター |
| P-1020 | PLC enclosure | ECU ケース |
| P-1021 | Heat sink | インタークーラー |
| P-1022 | Cooling fan assembly | 電動ファン |
| P-1023 | Filter housing | オイルフィルター |
| P-1024 | Valve body | スロットルボディ |
| P-1025 | Piston rod | ピストン |
| P-1026 | Spring assembly | コネクティングロッド |
| P-1027 | Welded frame | サブフレーム |
| P-1028 | Guard panel | ドアパネル |
| P-1029 | Hinge set | ドアヒンジ |
| P-1030 | Fastener kit | ボルト・ナットキット |

### Impact

| Artifact | Change required |
| --- | --- |
| `ProductSeed.cs`, new migration | New names; seeded notes rewritten (DEC-008) |
| 001_DB | Product seed table |
| `ai/project.md`, `README.md`, `CLAUDE.md` | The demo is described as automobile-parts production management |
| Tests that assert product names | New names |

## DEC-003: How do the English design documents quote UI text?

**Status:** decided

- **Decision:** quote the implemented Japanese text, followed by an English gloss in parentheses, e.g.
  「製品を選択してください。」 (Select a product.). The Japanese PDFs quote the Japanese text alone.
- **Decided by:** ThanhTN, 2026-09-23.
- **Impact:** `ai/rules/documentation.md` and RFC 0008 currently say quoted UI text stays English because the UI is
  English. That rule changes with this work item.

## DEC-004: The Japanese UI vocabulary

**Status:** decided — approved with plan revision 1 (ThanhTN, 2026-09-23).

One glossary, used by the catalog, the design documents and the Japanese PDFs:

| English | Japanese |
| --- | --- |
| Production order / Production orders | 製造指示 / 製造指示一覧 |
| New production order | 新規製造指示 |
| Dashboard | ダッシュボード |
| Order number | 指示番号 |
| Product | 製品 |
| Quantity | 数量 |
| Due date / Due from / Due to | 納期 / 納期（開始） / 納期（終了） |
| Notes | 備考 |
| Status | ステータス |
| Draft / In progress / Completed / Cancelled | 下書き / 進行中 / 完了 / 取消 |
| Save / Saving… / Cancel | 保存 / 保存中… / キャンセル |
| Discard your changes? / Discard / Keep editing | 変更を破棄しますか？ / 破棄 / 編集を続ける |
| Search / Clear / Clear filters / Filters | 検索 / クリア / 絞り込みをクリア / 絞り込み |
| Previous / Next / Page n of m / Rows | 前へ / 次へ / {n} / {m} ページ / 表示件数 |
| Overdue / Due in the next 7 days | 納期遅れ / 7 日以内に納期 |
| Needs attention / Top products by open quantity | 要注意 / 未完了数量の多い製品 |
| Open workload by due week / Completed per week | 納期週別の未完了作業量 / 週別の完了件数 |
| View as table / Expand / Restore / Retry / Reload | 表で表示 / 拡大 / 元に戻す / 再試行 / 再読み込み |
| Server / Database / OK / Unreachable / Unavailable / Unknown / Checking… | サーバー / データベース / 正常 / 接続不可 / 利用不可 / 不明 / 確認中… |
| Sign in / Sign out / Menu | ログイン / ログアウト / メニュー |
| Home (breadcrumb) | ホーム |

Message texts follow this vocabulary; the Japanese wording already drafted in the Japanese PDFs is the starting point.

## DEC-005: What stays untranslated?

**Status:** decided — approved with plan revision 1 (ThanhTN, 2026-09-23).

Identifiers and codes stay as they are, in the UI and in the documents: the system name `ProductionManagementAI`, order
numbers `PO-YYYY-NNNNN`, product codes `P-1001`…, message IDs, routes, API field names, enum values in the API, and
document IDs. Page titles become `{Japanese screen name} — ProductionManagementAI`.

## DEC-006: Date, time and number display

**Status:** decided — approved with plan revision 1 (ThanhTN, 2026-09-23).

| Value | Today | Proposed |
| --- | --- | --- |
| Due date (list, dashboard) | `2026-09-30` | `2026/09/30` |
| Timestamps (created/updated, snapshot, health) | browser locale, e.g. `Sep 18, 2026, 10:02 AM` | `2026/09/18 10:02` (plant timezone, 24-hour) |
| Counts and quantities | `24 orders`, `3,097 units` | `24件`, `3,097個` |
| Lead time | `13.7 days` | `13.7日` |
| Chart week labels | `09-28`, `This week`, `Later` | `9/28`, `今週`, `それ以降` |

The date input keeps the browser's native `<input type="date">`, which the browser localizes itself.

Clarification recorded during step 5 (2026-09-23): only the format changes; which timezone a value is shown in stays as
each screen's design already says. Screen A's created/updated times (001_BD M-04) and Screen B's updated time (002_BD
M-10) remain in the browser's timezone; the dashboard's snapshot and health times remain in the plant timezone. The
"plant timezone" in the table above applies to the dashboard only.

## DEC-007: Where the Japanese strings live

**Status:** decided — approved with plan revision 1 (ThanhTN, 2026-09-23).

`messages.ts` is already the single message catalog. It gains a `labels` section for every other UI string, so each
component reads text from one module and no string is inline. No i18n library (DEC-001).

## DEC-008: How the data change reaches existing databases

**Status:** decided — approved with plan revision 1 (ThanhTN, 2026-09-23).

A new migration `LocalizeDemoDataToJapanese` updates the 30 product names by their fixed IDs (EF `UpdateData`, from the
changed `ProductSeed`), and rewrites the notes of the seeded orders only (matched by the fixed seed-ID prefixes of
WI-003 and WI-004), so orders users created are never touched. It changes no schema and needs no new privilege. The
Compose volume does not need wiping.

## DEC-009: Merge PR #17

**Status:** decided

- **Decision:** PR #17 squash-merged into `master` as `dbc9527`, 2026-09-23; branch `feature/WI-005-japanese-ui` deleted.
- **Decided by:** ThanhTN, 2026-09-23: "merge the PR when CI passes".
- **Rationale:** backend, frontend and E2E CI jobs all passed on the PR (https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/35828068056); local verification complete (evidence.md).
