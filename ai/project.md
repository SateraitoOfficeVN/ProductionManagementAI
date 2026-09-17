# Project context

## Confirmed

- Demo: ProductionManagementAI; small manufacturing screens demonstrating the full AI development lifecycle.
- Frontend: Vite + React + TypeScript, Tailwind CSS v4 (no component kit), `react-router-dom` (DEC-001, DEC-005; `work-items/WI-001/decisions.md`).
- Backend: .NET 10, EF Core, conventional layered structure (`Domain`/`Application`/`Infrastructure`/`Api`) — not minimal-APIs, not CQRS/MediatR (DEC-002; see `docs/en/architecture/0001-backend-layered-structure.md`).
- Database: PostgreSQL 17 (DEC-003).
- Test frameworks: Vitest + React Testing Library (frontend), xUnit (backend unit + integration, the latter via `WebApplicationFactory` + Testcontainers.PostgreSql) (DEC-004).
- Authentication/RBAC: in scope, built as foundational bootstrap work before Screen A — ASP.NET Core Identity + cookie-based authentication, same-origin via a dev-server/Nginx proxy (DEC-006, DEC-007; see `docs/en/architecture/0002-auth-rbac-foundation.md`).
- Repository/branch policy: trunk-based, short `feature/<WI-id>-slug` branches, PR back to `master` (DEC-009).
- Source/PR/CI/CD: GitHub and GitHub Actions.
- Packaging/deployment: Docker (local Compose environment implemented; see Current implementation).
- Shared harness: Markdown consumed by Claude and Codex.
- New project artifacts: English by default, optional Japanese translation.

## Open decisions

- Registry / deployment host beyond local Docker Compose (WI-001 DEC-012).
- Merge/deploy permissions and actual CI execution — not authorized for this scaffold yet.
- Japanese-translation-sync policy (WI-001 DEC-013).
- How the four demo videos are produced (WI-001 DEC-014).
- Exact role/permission matrix beyond the placeholder `Admin`/`Operator` seed roles — must be confirmed before any screen gates on a specific permission (WI-001 DEC-015).

## Current implementation

WI-001 (`work-items/WI-001/`) stood up the application skeleton and auth foundation on branch `feature/WI-001-bootstrap-skeleton`, merged to `master` via PR #1. Verified commands, run from the repo root unless noted:

**Backend** (`src/backend/ProductionManagementAI.slnx`):
- Build: `dotnet build src/backend/ProductionManagementAI.slnx`
- Test (unit + integration, includes Testcontainers-backed Postgres): `dotnet test src/backend/ProductionManagementAI.slnx`
- Apply migrations: `dotnet ef database update --project src/backend/ProductionManagementAI.Infrastructure --startup-project src/backend/ProductionManagementAI.Api`
- Run locally (Development): requires `SEED_ADMIN_PASSWORD` env var set (no hardcoded fallback) and a reachable Postgres matching `appsettings.Development.json`'s connection string.

**Frontend** (`src/frontend/`):
- Build: `npm run build`
- Lint: `npm run lint`
- Test: `npm test`
- Dev server: `npm run dev` (proxies `/api` to `http://localhost:5033`)

**Docker Compose** (`deploy/`):
- Copy `deploy/.env.example` to `deploy/.env` and fill in real local values first.
- Full stack: `docker compose -f deploy/compose.yaml up -d --build`
- Config validation only: `docker compose -f deploy/compose.yaml config`
- Default host ports deviate from the obvious choices to avoid machine-specific conflicts found during WI-001 (see `work-items/WI-001/decisions.md`): Postgres on 5433 (not 5432), frontend on 3000 (not 8080).

**CI**: `.github/workflows/ci.yml` exists (build+lint+test only, both backend and frontend jobs) but has only been reviewed manually — not executed, since triggering GitHub Actions isn't authorized for this scaffold yet.

## Candidate demo

Roadmap locked (DEC-008, DEC-010): Screen A = production-order create/edit, Screen B = production-order list, Screen C = dashboard (widgets/metrics still open, to be resolved during Screen C's own `requirements` step). The earlier "product catalog" candidate was dropped.
