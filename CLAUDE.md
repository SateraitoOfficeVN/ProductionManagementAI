# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# Claude entry point

Follow [AGENTS.md](AGENTS.md) as the shared project instruction source.
Use [ai/skills/README.md](ai/skills/README.md) to locate the relevant skill and read its SKILL.md directly.

Keep durable state in work-items, not only in chat. This repository does not yet configure Claude-native skill registration; do not duplicate the shared skills in this adapter.

## Current state

WI-001's application skeleton and auth foundation are implemented and merged to `master`: a .NET 10 + EF Core backend (`src/backend/`, layered Domain/Application/Infrastructure/Api), a Vite + React + TypeScript + Tailwind CSS v4 frontend (`src/frontend/`), ASP.NET Core Identity cookie authentication, a PostgreSQL 17 schema/migration, a local Docker Compose environment (`deploy/`), and a GitHub Actions CI workflow (defined, not yet executed — triggering CI isn't authorized by this scaffold). WI-002 (Screen A — production order create/edit) is restarting from scratch: its prior design work (brief, basic-design, database-design, detailed-design) was scrapped after `ai/templates/basic-design.md` and the detailed-design template family were rewritten to match `ai/templates/example/BD` and `ai/templates/example/DD`, and it restarted on 2026-09-18 with a new brief, decision log, plan revision 1 (design phase only, awaiting review) and `docs/en/010_basic-design/BD-001-production-order-create-edit.md`. Do not invent or assume commands beyond what `ai/project.md` lists as verified; it also tracks what's still open (UI component kit, registry/deployment host beyond local Compose, the exact role/permission matrix beyond the placeholder `Admin`/`Operator` roles).

## Working in this repo

- Start from [AGENTS.md](AGENTS.md): it points to `ai/project.md`, `ai/policies.md`, `ai/rules/common.md`, then a workflow in `ai/workflows/README.md`, the relevant skill(s) in `ai/skills/README.md`, and the applicable gate in `ai/checklists/README.md`.
- For an existing work item under `work-items/`, read its brief, approved plan, status, decisions and evidence before editing.
- Follow `ai/policies.md` for authorization and pause conditions — plans need review before implementation, but an approved plan doesn't need re-approval for each step within its scope.
- This scaffold does not authorize GitHub push, PR creation, merge, image publication or deployment; task-specific authorization is required for those.
