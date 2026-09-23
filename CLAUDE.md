# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# Claude entry point

Follow [AGENTS.md](AGENTS.md) as the shared project instruction source.
Use [ai/skills/README.md](ai/skills/README.md) to locate the relevant skill and read its SKILL.md directly.

Keep durable state in work-items, not only in chat. This repository does not yet configure Claude-native skill registration; do not duplicate the shared skills in this adapter.

## Current state

WI-001 (application skeleton and auth foundation: a .NET 10 + EF Core backend in `src/backend/`, a Vite + React + TypeScript + Tailwind CSS v4 frontend in `src/frontend/`, ASP.NET Core Identity cookie auth, PostgreSQL 17, local Docker Compose in `deploy/`), WI-002 (Screen A, production-order create/edit), WI-003 (Screen B, production-order list) and WI-004 (Screen C, the production dashboard at `/`) are done and merged to `master`. Each went through brief, basic design, database design, the full DD set with a mockup, implementation and tests at every level; their records are in `work-items/WI-00N/`. WI-004 also added completion tracking on orders, a health endpoint that never renews the session, a navbar with `lucide-react` icons on every screen, and demo history (124 seeded orders). The app runs as a restricted `pmai_app` database login, and migrations run as the owner (see `deploy/README.md`). GitHub Actions CI runs backend, frontend and e2e (Compose + Playwright) jobs on every PR to `master` and passes; documentation-only changes skip it. The locked demo roadmap (Screens A–C) is complete. WI-005 (in progress) makes the UI Japanese only and the demo domain automobile-parts production, with Japanese demo data; its records are in `work-items/WI-005/`. Do not invent or assume commands beyond what `ai/project.md` lists as verified; it also tracks what's still open (deployment host beyond local Compose, merge/deploy permissions, demo-video production, the full role/permission matrix beyond the placeholder `Admin`/`Operator` roles).

## Working in this repo

- Start from [AGENTS.md](AGENTS.md): it points to `ai/project.md`, `ai/policies.md`, `ai/rules/common.md`, then a workflow in `ai/workflows/README.md`, the relevant skill(s) in `ai/skills/README.md`, and the applicable gate in `ai/checklists/README.md`.
- For an existing work item under `work-items/`, read its brief, approved plan, status, decisions and evidence before editing.
- Follow `ai/policies.md` for authorization and pause conditions — plans need review before implementation, but an approved plan doesn't need re-approval for each step within its scope.
- This scaffold does not authorize GitHub push, PR creation, merge, image publication or deployment; task-specific authorization is required for those.
