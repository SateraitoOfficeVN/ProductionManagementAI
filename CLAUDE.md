# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# Claude entry point

Follow [AGENTS.md](AGENTS.md) as the shared project instruction source.
Use [ai/skills/README.md](ai/skills/README.md) to locate the relevant skill and read its SKILL.md directly.

Keep durable state in work-items, not only in chat. This repository does not yet configure Claude-native skill registration; do not duplicate the shared skills in this adapter.

## Current state

WI-001 (application skeleton and auth foundation: a .NET 10 + EF Core backend in `src/backend/`, a Vite + React + TypeScript + Tailwind CSS v4 frontend in `src/frontend/`, ASP.NET Core Identity cookie auth, PostgreSQL 17, local Docker Compose in `deploy/`), WI-002 (Screen A, production-order create/edit), WI-003 (Screen B, production-order list) and WI-004 (Screen C, the production dashboard at `/`) are done and merged to `master`. Each went through brief, basic design, database design, the full DD set with a mockup, implementation and tests at every level; their records are in `work-items/WI-00N/`. WI-004 also added completion tracking on orders, a health endpoint that never renews the session, a navbar with `lucide-react` icons on every screen, and demo history (124 seeded orders). The app runs as a restricted `pmai_app` database login, and migrations run as the owner (see `deploy/README.md`). GitHub Actions CI runs backend, frontend and e2e (Compose + Playwright) jobs on every PR to `master` and passes; documentation-only changes skip it. The locked demo roadmap (Screens A–C) is complete. WI-005 (done and merged) made the UI Japanese only and the demo domain automobile-parts production, with Japanese demo data; design documents are number-first (`001_BD`, `001_DD`, `001_DB`) in one folder per number, quote the Japanese UI with an English gloss, and each has English and Japanese PDFs (RFCs 0008–0010); a screen's documents are named with the screen's Japanese name, for example `001_BD_製造指示登録・編集.md` (RFC 0011). WI-007 adopted RFC 0012 through PR #29: each design Markdown file is presented with its required companion artifacts for review before the next design file is written. WI-008 merged via PR #33 as `20c8d61`; its initial approval-history correction remains documented. WI-009 is merged via PR #34 as 42e8932; WI-010 Plant calendar merged via PR #36 as d4dd976, final-head CI passed, feature branch/worktree cleaned, with accepted manual accessibility limits. Do not invent or assume commands beyond what `ai/project.md` lists as verified; it also tracks what's still open (deployment host beyond local Compose, merge/deploy permissions, demo-video production, the full role/permission matrix beyond the placeholder `Admin`/`Operator` roles).

WI-009 Production lines merged via PR #34 as 42e8932. WI-010 Plant calendar merged
via PR #36 as d4dd976 after explicit user merge/cleanup authorization. Final reviewed
head c9430e7 passed GitHub run 36975730228: Backend/Frontend/E2E, 239 unit/195 integration/
253 frontend/48 E2E. Local build/lint/native 200% and disposable fixture rehearsals pass.
Approved designs immutable; local/remote feature branch and clean worktree removed,
main synchronized. Prior evidence checkout/videos preserved. Screen-reader speech
and physical mobile keyboard/IME remain Not run with accepted local handoff limits.
No live activation/deployment. WI-010 closeout merged via PR #37 as c227acc;
no next feature approved. See work-items/WI-010/status.md. BOM remains a candidate.

WI-011 calendar button height/spacing fixes merged via PR #38 as de130ab; no deployment.
WI-012 Plant calendar mockup alignment merged via PR #39 as 31b65f9 under explicit
revision5 approval. Final reviewed head e3a1909 passed run37286646826: 239 backend
unit/195 integration/258 frontend/62 E2E. Main synchronized; merged feature branch/
worktree and task-owned review runtime/volumes/browser/temp removed. Existing demo,
other worktrees and prior videos preserved. Approved historical designs unchanged.
Current WI-012 design PNG companions accepted; future designs use wireframe SVG.
No test screenshot or unnecessary generated JSON artifacts in work items. Manual
speech/physical mobile keyboard/IME remain Not run; no live deployment. WI-012 closeout
merged via PR #40 as 33555f3. See WI-012 records.

WI-013 Product master mockup alignment merged via PR #41 as b32656f (squash) under
approved revisions 2-3 and explicit merge/cleanup request. Final head 77f5ba2 passed
CI run 37412799848; code head 29b6c93 passed run 37412179017 (239 backend unit/203
integration/267 frontend/66 E2E). /products follows the 004_DD mockup; edit route is
/products/:id/edit (old /products/:id redirects); saves return to the list; rows per
page 10/20/50/100 via optional allow-listed pageSize on GET /api/product-master
(DEC-007; approved 004_DD-API unedited); control heights match order screens. Feature
branch deleted locally/remotely; main synchronized; no deployment. WI-013 closeout merged
via PR #42 as 46bf430. See work-items/WI-013.

WI-014 order-list product filter overflow (BUG-008) merged via PR #43 as 3e29618 (squash)
under approved revisions 1-2. Final head dd10347 passed CI run 37433461443 (239 backend
unit/203 integration/269 frontend/68 E2E). Filter grid tracks have a zero minimum; in
Chrome/Edge the open list (appearance: base-select) matches the control width, long names
end in an ellipsis with the full name as title; Firefox/Safari keep the native popup.
Still a native select per 002_DD (unedited). Branch deleted; main synchronized; no
deployment. WI-014 closeout merged via PR #44 as 2f502b1. See work-items/WI-014.

WI-015 production lines redesign (BUG-009) merged via PR #45 as 2470c8e (squash) under
approved revisions 1-3. Additive 005_DD-SPD-REDESIGN v3 (WI-009 005 docs unedited). The
list has rows per page 10/20/50/100; register/edit has a 20-per-page product table and a
製品を追加 dialog (adding confirms the unit). Lists scroll in bounded boxes with sticky
headers. A saved 使用停止 pair cannot be re-added, and the UI says so. Optional allow-listed
page sizes on line list/detail/product-choices APIs, default 50. Final head 7c22966 passed
CI run 37562084126 (251 backend unit/215 integration/282 frontend/73 E2E). Branch deleted;
main synchronized; no deployment. WI-015 closeout merged via PR #46 as a470146. See
work-items/WI-015.

WI-016 order-list CSV export merged via PR #48 as 8649f2d (squash) under approved revisions
1-4. 「CSV出力」 on /production-orders downloads all orders matching the applied filters/sort:
GET /api/production-orders/export, 13 columns, UTF-8 BOM/CRLF/RFC 4180, plant-time timestamps,
formula-like text prefixed with ', 10,000-row limit (422 MSG-E024), X-Total-Count; one
REPEATABLE READ snapshot, streamed. Additive 002_BD-CSV/002_DD-CSV set (approved WI-003 002 docs
unedited). Final head 403befd passed run 37714084761 (286 backend unit/234 integration/307
frontend/76 E2E). *.csv stored -text. TC-469 (real spreadsheet app) Not run. Branch deleted;
main synchronized; no deployment. Next: none approved; candidate RFCs: test-plan.md whenever
a WI adds tests; design PDFs only after the design .md is approved. See work-items/WI-016.

WI-006 Product master is done and merged via PR #31 (`b806b1c`): `/products` supports Admin/Operator maintenance, immutable SKU, referenced-unit locking and retirement preserving history; orders support exact kg/m decimals and the dashboard uses counts across units with per-unit subtotals. CI passed all three jobs (148 backend unit, 111 integration, 139 frontend and 26 Playwright tests). Only isolated migration rehearsal was performed; no mutable demo/live cutover or deployment is claimed. WI-010 is merged with accepted manual verification limits; Bill of materials remains a later candidate.

## Working in this repo

- Start from [AGENTS.md](AGENTS.md): it points to `ai/project.md`, `ai/policies.md`, `ai/rules/common.md`, then a workflow in `ai/workflows/README.md`, the relevant skill(s) in `ai/skills/README.md`, and the applicable gate in `ai/checklists/README.md`.
- For an existing work item under `work-items/`, read its brief, approved plan, status, decisions and evidence before editing.
- Follow `ai/policies.md` for authorization and pause conditions — plans need review before implementation, but an approved plan doesn't need re-approval for each step within its scope.
- This scaffold does not authorize GitHub push, PR creation, merge, image publication or deployment; task-specific authorization is required for those.
