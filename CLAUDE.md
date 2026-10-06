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
branch deleted locally/remotely; main synchronized; no deployment. Next: the user will
request a new bug-fix WI for the order-list product filter that widens the SP page with
long product names (found by WI-013 CI). See work-items/WI-013.

WI-006 Product master is done and merged via PR #31 (`b806b1c`): `/products` supports Admin/Operator maintenance, immutable SKU, referenced-unit locking and retirement preserving history; orders support exact kg/m decimals and the dashboard uses counts across units with per-unit subtotals. CI passed all three jobs (148 backend unit, 111 integration, 139 frontend and 26 Playwright tests). Only isolated migration rehearsal was performed; no mutable demo/live cutover or deployment is claimed. WI-010 is merged with accepted manual verification limits; Bill of materials remains a later candidate.

## Working in this repo

- Start from [AGENTS.md](AGENTS.md): it points to `ai/project.md`, `ai/policies.md`, `ai/rules/common.md`, then a workflow in `ai/workflows/README.md`, the relevant skill(s) in `ai/skills/README.md`, and the applicable gate in `ai/checklists/README.md`.
- For an existing work item under `work-items/`, read its brief, approved plan, status, decisions and evidence before editing.
- Follow `ai/policies.md` for authorization and pause conditions — plans need review before implementation, but an approved plan doesn't need re-approval for each step within its scope.
- This scaffold does not authorize GitHub push, PR creation, merge, image publication or deployment; task-specific authorization is required for those.
