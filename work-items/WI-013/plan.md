# WI-013 — Execution plan

| Revision | Date | Phase | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-10-06 | Frontend alignment of SCR-004 with approved mockup and behaviour | Superseded; partial code rolled back at user request, nothing committed | User "yes, keep 48px, fix behaviour gaps too, go ahead", 2026-10-06 |
| 2 | 2026-10-06 | Alignment + order-screen control heights + rows per page (+ DEC-010–012 follow-ups) | Complete; user review done ("i review done. continue process", 2026-10-06) | User "approved, decisions record only, go ahead" after revision 2 was presented, 2026-10-06 |
| 3 | 2026-10-06 | Commit, push and PR handoff | In progress | User "approved, go ahead" after revision 3 was presented, 2026-10-06 |

## Revision 2

### Objective and scope

Fix BUG-006/BUG-007 and deliver ENH-001 (see [brief](brief.md)) for SCR-004 `/products`,
`/products/new` and `/products/:id/edit`. Revision 1 code was restored to `master` (`33555f3`)
before this revision was written.

### Steps

| # | Step | Files | Verification |
| --- | --- | --- | --- |
| 1 | **Wording** in the `products` catalog: 「製品を登録」, subtitle 「製品情報を管理します。」, 「製品コード・製品名で検索」, 「操作」, 「製品登録」/「製品編集」, 「必須」/「任意」/「（変更不可）」, retired hint, dialog title/body, 「条件に一致する製品はありません。」/「製品がありません。」, saved-but-hidden notice, 「N件」, 「n / N」, product-specific forbidden text + 「ダッシュボードへ戻る」 | `messages.ts` | Unit tests check rendered text; TC-301 inline-text scan |
| 2 | **List visuals**: mockup colours (blue primary, outlined secondary, red retire), shaded filter panel and table header, 使用中/使用停止 badges, count + pager at the bottom, phone cards (「P-1001　名称」, 「単位：」, badge, stacked full-width 検索/クリア) | `ProductMasterPage.tsx` | PC 1280 / SP 390 screenshots against mockup states 1, 4, 7 |
| 3 | **Control heights = production-order screens**: filled buttons `px-4 py-2` (40 px), outlined buttons and inputs/selects `py-2` + border (42 px), pager buttons as `ListPagination` (`px-3 py-1.5 text-sm`) | `ProductMasterPage.tsx` | E2E compares measured heights on `/products` with `/production-orders` |
| 4 | **Rows per page**: reuse the order list's `PageSizeSelect` (「表示件数」, 10/20/50/100) next to the count; URL `pageSize`, canonicalised to 20 when invalid; changing it goes back to page 1; search, Clear, paging and return-after-edit keep it | `ProductMasterPage.tsx`, `products/api.ts` | Unit + E2E (choose 10 → 10 rows, URL updated, page 1) |
| 5 | **Backend `pageSize`**: optional allow-listed query parameter on `GET /api/product-master` (10/20/50/100, default 20, otherwise 400 on `pageSize`); repository pages with it | `ProductMasterContracts.cs`, `ProductMasterRepository.cs` | Integration tests for each allowed size and for 0/15/101/abc |
| 6 | **Form visuals**: 「必須」 markers, 「任意」 hint, greyed read-only SKU and locked unit with 「（変更不可）」, retired badge + hint, red error summary on validation/conflict, red field border, 保存 (primary) before キャンセル | `ProductMasterPage.tsx` | Screenshots against mockup states 2, 3, 5; axe |
| 7 | **Retire dialog**: 「製品を使用停止にしますか？」, 「{名称}（{SKU}）は新しい製造指示で選択できなくなります。既存の製造指示は残ります。」, red confirm button | `ProductMasterPage.tsx` | E2E dialog name; existing WI-008 centring/focus tests |
| 8 | **Behaviour**: edit route `/products/:id/edit` (old `/products/:id` redirects); list links pass a validated internal return URL; save returns there with 「製品を登録しました。」/「製品を保存しました。」 plus the hidden-by-filter hint; empty vs no-match text; a page past the end moves to the last page; 403 shows the forbidden state | `App.tsx`, `ProductMasterPage.tsx` | Unit + E2E (create/edit now land on the list) |
| 9 | **Tests**: update `ProductMasterPage.test.tsx`, `product-master.spec.ts` and `product-ui.spec.ts` for the new wording, route and flow; add regression cases for steps 3, 4 and 8 | tests | Each new case fails on the old behaviour |
| 10 | **Verify**: frontend lint/build/test; backend build + unit/integration; rebuild local Compose `frontend` and `backend`; full Playwright suite; PC/SP screenshots compared with every mockup state; record evidence and status | — | evidence.md |

### Permitted actions

Edits on local branch `feature/WI-013-product-master-alignment` to the files above and the WI-013
records; rebuilding the local Compose `frontend` and `backend` images and running tests against them
(E2E creates test products, as it does today). No commit, push, PR, merge, deployment, database
migration or edit to an approved design document without further authorization.

### Risks / stop conditions

- The approved 004_DD-API says `pageSize` is always 20; ENH-001 extends that contract additively
  (DEC-007). No design addendum (user, 2026-10-06).
- 40/42 px controls replace the 48 px used on this screen today; still above the WCAG 2.2 AA
  24 px minimum target size.
- Create/edit now return to the list, so the E2E flows that stayed on the form are rewritten.
- Anything needing a migration, a new dependency or another screen stops the work for a new revision.

### Review and approval

Review status: approved. Approval source: user "approved, decisions record only, go ahead", 2026-10-06. Design record: DEC-007 only, no addendum.

## Revision 3 — Commit, push and PR handoff

### Steps

| # | Step | Verification |
| --- | --- | --- |
| 1 | Update WI-013 status/evidence with the final local results | Records current |
| 2 | One commit on `feature/WI-013-product-master-alignment` with the 10 code/test files and `work-items/WI-013/` (Conventional Commit `fix(WI-013): ...`, co-author line) | `git show --stat`; no secrets in diff |
| 3 | Push the branch to `origin` | Remote branch exists |
| 4 | Open a PR to `master` describing BUG-006/BUG-007/ENH-001, DEC-001–012, test results and the local Plant calendar E2E limitation | PR URL |
| 5 | Watch the CI run (backend, frontend, e2e with its calendar fixture) and record the result | Run URL and job results in evidence/status |

### Permitted actions

`git add`/`commit` of the listed files, `git push -u origin feature/WI-013-product-master-alignment`,
`gh pr create` and read-only `gh` checks of the run. Not permitted: merge, branch deletion, deployment,
demo-database changes, approved-design edits. CI failures are diagnosed and reported; fixes need user approval.

### Review and approval

Review status: approved. Approval source: user "approved, go ahead", 2026-10-06.
