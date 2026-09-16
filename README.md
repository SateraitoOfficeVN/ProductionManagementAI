# ProductionManagementAI

AI-assisted development harness and production-management demo.

**Status:** documentation scaffold only. Application, Docker images and CI/CD are not implemented yet.

## Start here

1. Read [how the ai/ harness works](ai/harness-overview.md)
2. Read [project decisions](ai/project.md) and [execution policies](ai/policies.md).
3. Choose a [workflow](ai/workflows/README.md).
4. Create a work item from the [templates](ai/templates/README.md).
5. Ask Claude or Codex to draft a plan; review it before implementation.

Pass the applicable gate in [checklists](ai/checklists/README.md) — design-consistency, security-review, delivery, release-readiness — before calling a stage done.

Claude starts at [CLAUDE.md](CLAUDE.md). Codex and compatible agents start at [AGENTS.md](AGENTS.md). Both read shared Markdown in `ai/`; native skill auto-discovery is not configured.

## Confirmed stack

Vite + TypeScript, .NET 10, PostgreSQL, GitHub Actions, Docker.
UI framework, ORM, test tools, hosting and merge/deploy policy remain open.

## Layout

- [ai](ai/README.md): shared workflows, skills, rules and templates.
- [docs](docs/README.md): system documentation and initial Vietnamese specification.
- [work-items](work-items/README.md): plans, decisions, status and evidence.
- [src](src/README.md), [tests](tests/README.md): future application and checks.
- [deploy](deploy/README.md), [.github](.github/README.md): deployment and GitHub conventions.
- [demos](demos/README.md): four planned walkthroughs.

English is the default for new project artifacts. Japanese versions are optional; the initial overview is Vietnamese.
