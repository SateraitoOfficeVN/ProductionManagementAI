# ProductionManagementAI

AI-assisted development harness and production-management demo.

**Status:** WI-001's application skeleton and auth foundation are implemented and merged to `master` (backend, frontend, DB migration, local Docker Compose, CI workflow defined but not yet executed). WI-002 (Screen A) is restarting its design from scratch against the updated basic-design/detailed-design templates. See `ai/project.md` for verified commands and what's still open.

## Start here

1. Read [how the ai/ harness works](ai/harness-overview.md)
2. Read [project decisions](ai/project.md) and [execution policies](ai/policies.md).
3. Choose a [workflow](ai/workflows/README.md).
4. Create a work item from the [templates](ai/templates/README.md).
5. Ask Claude or Codex to draft a plan; review it before implementation.

Pass the applicable gate in [checklists](ai/checklists/README.md) — design-consistency, security-review, delivery, release-readiness — before calling a stage done.

Claude starts at [CLAUDE.md](CLAUDE.md). Codex and compatible agents start at [AGENTS.md](AGENTS.md). Both read shared Markdown in `ai/`; native skill auto-discovery is not configured.

## Confirmed stack

Vite + React + TypeScript + Tailwind CSS v4 (no component kit), .NET 10 + EF Core (layered Domain/Application/Infrastructure/Api), PostgreSQL 17, ASP.NET Core Identity (cookie auth), Vitest + React Testing Library / xUnit tests, GitHub Actions, Docker Compose (local).
Still open: UI component kit, registry/deployment host beyond local Compose, exact role/permission matrix. Full detail in `ai/project.md`.

## Layout

- [ai](ai/README.md): shared workflows, skills, rules and templates.
- [docs](docs/README.md): system documentation and initial Vietnamese specification.
- [work-items](work-items/README.md): plans, decisions, status and evidence.
- [src](src/README.md): backend (.NET 10) and frontend (Vite + React + TypeScript) application source — WI-001's skeleton and auth foundation are implemented; verified commands in `ai/project.md`.
- [tests](tests/README.md): automated checks — backend unit/integration tests and frontend unit tests are implemented; see `ai/project.md` for commands.
- [deploy](deploy/README.md), [.github](.github/README.md): deployment and GitHub conventions.
- [demos](demos/README.md): four planned walkthroughs.

English is the default for new project artifacts. Japanese versions are optional;
