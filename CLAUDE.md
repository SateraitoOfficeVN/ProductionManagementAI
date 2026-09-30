# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# Claude entry point

Follow [AGENTS.md](AGENTS.md) as the shared project instruction source.
Use [ai/skills/README.md](ai/skills/README.md) to locate the relevant skill and read its SKILL.md directly.

Keep durable state in work-items, not only in chat. This repository does not yet configure Claude-native skill registration; do not duplicate the shared skills in this adapter.

## Current state

WI-001 (application skeleton and auth foundation: a .NET 10 + EF Core backend in `src/backend/`, a Vite + React + TypeScript + Tailwind CSS v4 frontend in `src/frontend/`, ASP.NET Core Identity cookie auth, PostgreSQL 17, local Docker Compose in `deploy/`), WI-002 (Screen A, production-order create/edit), WI-003 (Screen B, production-order list) and WI-004 (Screen C, the production dashboard at `/`) are done and merged to `master`. Each went through brief, basic design, database design, the full DD set with a mockup, implementation and tests at every level; their records are in `work-items/WI-00N/`. WI-004 also added completion tracking on orders, a health endpoint that never renews the session, a navbar with `lucide-react` icons on every screen, and demo history (124 seeded orders). The app runs as a restricted `pmai_app` database login, and migrations run as the owner (see `deploy/README.md`). GitHub Actions CI runs backend, frontend and e2e (Compose + Playwright) jobs on every PR to `master` and passes; documentation-only changes skip it. The locked demo roadmap (Screens A–C) is complete. WI-005 (done and merged) made the UI Japanese only and the demo domain automobile-parts production, with Japanese demo data; design documents are number-first (`001_BD`, `001_DD`, `001_DB`) in one folder per number, quote the Japanese UI with an English gloss, and each has English and Japanese PDFs (RFCs 0008–0010); a screen's documents are named with the screen's Japanese name, for example `001_BD_製造指示登録・編集.md` (RFC 0011). WI-007 adopted RFC 0012 through PR #29: each design Markdown file is presented with its required companion artifacts for review before the next design file is written. WI-008 is locally completed; continuation was authorized after the agent corrected its mistaken initial approval attribution. Its branch has not been merged. Do not invent or assume commands beyond what `ai/project.md` lists as verified; it also tracks what's still open (deployment host beyond local Compose, merge/deploy permissions, demo-video production, the full role/permission matrix beyond the placeholder `Admin`/`Operator` roles).

WI-008 (Product master UI bug fix) is locally completed on `feature/WI-008-product-ui-bugs`: both dialogs center within viewport bounds and Product master uses a distinct Package navbar icon. Frontend lint/build, 4 component and 15 targeted Chromium checks passed; see [work-item evidence](work-items/WI-008/evidence.md). The initial implementation preceded plan approval; the user subsequently authorized continuation, and the approval history is corrected in the work-item records. This is local delivery only: no push, PR, merge, remote CI or deployment has occurred.

WI-006 Product master is done and merged via PR #31 (`b806b1c`): `/products` supports Admin/Operator maintenance, immutable SKU, referenced-unit locking and retirement preserving history; orders support exact kg/m decimals and the dashboard uses counts across units with per-unit subtotals. CI passed all three jobs (148 backend unit, 111 integration, 139 frontend and 26 Playwright tests). Only isolated migration rehearsal was performed; no mutable demo/live cutover or deployment is claimed. No next application work item is approved; Plant calendar, Production lines and Bill of materials remain candidates requiring a separate plan.

## Working in this repo

- Start from [AGENTS.md](AGENTS.md): it points to `ai/project.md`, `ai/policies.md`, `ai/rules/common.md`, then a workflow in `ai/workflows/README.md`, the relevant skill(s) in `ai/skills/README.md`, and the applicable gate in `ai/checklists/README.md`.
- For an existing work item under `work-items/`, read its brief, approved plan, status, decisions and evidence before editing.
- Follow `ai/policies.md` for authorization and pause conditions — plans need review before implementation, but an approved plan doesn't need re-approval for each step within its scope.
- This scaffold does not authorize GitHub push, PR creation, merge, image publication or deployment; task-specific authorization is required for those.
