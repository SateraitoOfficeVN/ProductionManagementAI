# ProductionManagementAI

An AI-assisted development harness, and the production-management demo app built with it. Claude and Codex work from the same Markdown process in [`ai/`](ai/README.md), and every piece of work is traceable from requirement to design, code, tests and evidence in [`work-items/`](work-items/README.md).

**Status:**
- **WI-001 (bootstrap):** application skeleton and auth foundation, merged.
- **WI-002 (Screen A, production-order create/edit):** designed, implemented, tested and merged.
- **WI-003 (Screen B, production-order list):** designed, implemented, tested and merged. It adds filtering, sorting and paging over production orders, plus the first seeded demo orders.
- **WI-004 (Screen C, production dashboard):** designed, implemented, tested and merged. The dashboard is the landing page at `/`: status and delivery figures, overdue and due-soon orders, top products, and workload and completion-trend charts that can be maximized, with a server/database health indicator. It also adds completion tracking on orders, a navbar with icons on every screen, and demo history (124 seeded orders).
- **CI:** runs the backend, frontend and end-to-end jobs on every PR to `master` and passes. Changes that touch only documentation, work items, demos or `ai/` skip CI (RFC 0005).
- **Next:** Screens A, B and C — the whole locked demo roadmap — are done. No further work item is planned yet.

[`ai/project.md`](ai/project.md) has the verified commands and what's still open.

## Start here

1. Read [how the ai/ harness works](ai/harness-overview.md).
2. Read [project decisions](ai/project.md) and [execution policies](ai/policies.md).
3. Choose a [workflow](ai/workflows/README.md).
4. Create a work item from the [templates](ai/templates/README.md).
5. Ask Claude or Codex to draft a plan. The agent shows you each plan revision and waits for your approval before starting its steps. That includes a later revision, such as the implementation plan after the design is approved (RFC 0006).

Pass the applicable gate in [checklists](ai/checklists/README.md) before calling a stage done: design-consistency, security-review, delivery or release-readiness.

Claude starts at [CLAUDE.md](CLAUDE.md). Codex and compatible agents start at [AGENTS.md](AGENTS.md). Both read the shared Markdown in `ai/`; native skill auto-discovery is not configured.

## Confirmed stack

- **Frontend:** Vite + React + TypeScript, Tailwind CSS v4 (no component kit), `react-router-dom`, `lucide-react` icons.
- **Backend:** .NET 10 + EF Core, layered Domain/Application/Infrastructure/Api; ASP.NET Core Identity with cookie auth; RFC 9457 errors; OpenTelemetry.
- **Database:** PostgreSQL 17. Migrations run as the owner; the app runs as a restricted login.
- **Tests:** xUnit (unit + Testcontainers integration), Vitest + React Testing Library, Playwright E2E, axe accessibility checks.
- **Delivery:** GitHub Actions CI, Docker Compose for local environments.

Still open: registry/deployment host beyond local Compose, merge/deploy permissions, the Japanese-translation sync policy, how the demo videos are produced, and the full role/permission matrix. Full detail is in [`ai/project.md`](ai/project.md).

## Run it locally

1. Copy `deploy/.env.example` to `deploy/.env` and fill it in. `PMAI_APP_DB_PASSWORD` is required and has no default.
2. `docker compose -f deploy/compose.yaml up -d --build db`
3. Apply migrations as the database owner (the exact command is in [`deploy/README.md`](deploy/README.md)).
4. `docker compose -f deploy/compose.yaml up -d --build`, then open http://localhost:3000 and sign in as `admin` with your `SEED_ADMIN_PASSWORD`. You land on the dashboard; the navbar leads to the order list and to a new order.

## Layout

- [ai](ai/README.md): shared workflows, skills, rules, templates, checklists and harness-improvement records.
- [docs](docs/README.md): requirements, basic/detailed/database designs, ADRs and test documentation (English), plus the initial Vietnamese specification.
- [work-items](work-items/README.md): each work item's brief, plan (every revision), decisions, status and evidence.
- [src](src/README.md): backend (.NET 10) and frontend (Vite + React + TypeScript) application source.
- [tests](tests/README.md): backend unit and integration tests, and Playwright E2E journeys. Frontend unit tests live in `src/frontend/tests/`.
- [deploy](deploy/README.md): local Docker Compose environment and database logins.
- `.github/`: the [CI workflow](.github/workflows/ci.yml) (backend, frontend and e2e jobs on every push/PR to `master`, except documentation-only changes) and the [pull request template](.github/pull_request_template.md).
- [demos](demos/README.md): the four Screen A lifecycle walkthroughs (basic design, database design, detailed design, implementation), each with a transcript and a presentation deck (PDF). The video files are kept outside git.

English is the default for new project artifacts. Japanese versions are optional; how translations stay in sync with their English source is still an open decision (WI-001 DEC-013).
