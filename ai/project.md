# Project context

## Confirmed

- Demo: ProductionManagementAI; small manufacturing screens demonstrating the full AI development lifecycle.
- Frontend: Vite + React + TypeScript, Tailwind CSS v4 (no component kit), `react-router-dom` (DEC-001, DEC-005; `work-items/WI-001/decisions.md`).
- Backend: .NET 10, EF Core, conventional layered structure (`Domain`/`Application`/`Infrastructure`/`Api`) — not minimal-APIs, not CQRS/MediatR (DEC-002; see `docs/en/architecture/0001-backend-layered-structure.md`).
- Database: PostgreSQL 17 (DEC-003).
- Test frameworks: Vitest + React Testing Library (frontend), xUnit (backend unit + integration, the latter via `WebApplicationFactory` + Testcontainers.PostgreSql) (DEC-004); Playwright for E2E and axe (`vitest-axe`, `@axe-core/playwright`) for automated accessibility checks (WI-002 DEC-025, DEC-026).
- Authentication/RBAC: in scope, built as foundational bootstrap work before Screen A — ASP.NET Core Identity + cookie-based authentication, same-origin via a dev-server/Nginx proxy (DEC-006, DEC-007; see `docs/en/architecture/0002-auth-rbac-foundation.md`).
- Repository/branch policy: trunk-based, short `feature/<WI-id>-slug` branches, PR back to `master` (DEC-009).
- Source/PR/CI/CD: GitHub and GitHub Actions.
- Packaging/deployment: Docker (local Compose environment implemented; see Current implementation).
- Shared harness: Markdown consumed by Claude and Codex.
- New project artifacts: English by default, optional Japanese translation.

## Open decisions

- Registry / deployment host beyond local Docker Compose (WI-001 DEC-012).
- Standing merge/deploy permissions — not granted by this scaffold; each merge or deployment still needs task-specific authorization (CI execution itself is settled: see **CI** below).
- Japanese-translation-sync policy (WI-001 DEC-013).
- How the four demo videos are produced (WI-001 DEC-014).
- Exact role/permission matrix beyond the placeholder `Admin`/`Operator` seed roles — must be confirmed before any screen gates on a specific permission (WI-001 DEC-015).

## Current implementation

Done and merged to `master`:

- WI-001 (`work-items/WI-001/`): application skeleton and auth foundation, via PR #1.
- WI-002 (`work-items/WI-002/`): Screen A, production-order create/edit (brief, BD-001, DB-002, the DD-001 set with mockup, implementation, and unit/integration/E2E tests), via PRs #2 and #3 (WI-002 DEC-030). Since WI-002 the app connects as the restricted `pmai_app` login and migrations run as the owner.

Next planned work: Screen B (production-order list).

Verified commands, run from the repo root unless noted:

**Backend** (`src/backend/ProductionManagementAI.slnx`):
- Build: `dotnet build src/backend/ProductionManagementAI.slnx`
- Test (unit + integration, includes Testcontainers-backed Postgres): `dotnet test src/backend/ProductionManagementAI.slnx`
- Apply migrations (as the database owner; since WI-002 the app's own connection is the restricted `pmai_app` login, which can't run DDL): `dotnet ef database update --project src/backend/ProductionManagementAI.Infrastructure --startup-project src/backend/ProductionManagementAI.Api --connection "Host=localhost;Port=5433;Database=production_management_ai;Username=postgres;Password=<POSTGRES_PASSWORD>"`
- Run locally (Development): requires `SEED_ADMIN_PASSWORD` set (no hardcoded fallback), `PGPASSWORD` set to `PMAI_APP_DB_PASSWORD` (the app connects as `pmai_app`, see `appsettings.Development.json`), and a reachable, migrated Postgres.

**Frontend** (`src/frontend/`):
- Build: `npm run build`
- Lint: `npm run lint`
- Test: `npm test`
- Dev server: `npm run dev` (proxies `/api` to `http://localhost:5033`)

**Docker Compose** (`deploy/`):
- Copy `deploy/.env.example` to `deploy/.env` and fill in real local values first (`PMAI_APP_DB_PASSWORD` is required, no default).
- Full stack (WI-002 order): `docker compose -f deploy/compose.yaml up -d --build db`, then apply migrations as the owner (above), then `docker compose -f deploy/compose.yaml up -d --build`. A `db` volume created before WI-002 has no `pmai_app` login: wipe it (`down -v`) rather than fixing it by hand. Details: `deploy/README.md`.
- Config validation only: `docker compose -f deploy/compose.yaml config`
- Default host ports deviate from the obvious choices to avoid machine-specific conflicts found during WI-001 (see `work-items/WI-001/decisions.md`): Postgres on 5433 (not 5432), frontend on 3000 (not 8080).

**E2E** (`tests/e2e/`, against the running Compose stack): `npm ci`, `npx playwright install chromium`, then `E2E_ADMIN_PASSWORD=<SEED_ADMIN_PASSWORD> E2E_BASE_URL=http://localhost:3000 npx playwright test`. Verified in WI-002.

**CI**: `.github/workflows/ci.yml` has three jobs on push/PR to `master`: backend (build + unit/integration tests), frontend (lint + build + tests), and e2e (Compose stack + Playwright, after the other two; throwaway credentials generated per run; added by RFC 0003). A push or PR that changes only Markdown, `docs/`, `work-items/`, `demos/`, `ai/` or `LICENSE` starts no run at all (RFC 0005); `.github/`, `deploy/`, `src/` and `tests/` always do. Because GitHub evaluates the filter against a PR's whole diff, a PR that also changes code still runs everything. First executed on 2026-09-18 on PR #5, after the repository moved to the `SateraitoOfficeVN` organization (the earlier account billing lock blocked PRs #2–#4): backend, frontend and e2e all passed (https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/35318583226).

## Candidate demo

Roadmap locked (DEC-008, DEC-010): Screen A = production-order create/edit, Screen B = production-order list, Screen C = dashboard (widgets/metrics still open, to be resolved during Screen C's own `requirements` step). The earlier "product catalog" candidate was dropped.
