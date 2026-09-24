# Project context

## Confirmed

- Demo: ProductionManagementAI; small production-management screens for an automobile-parts manufacturer, demonstrating the full AI development lifecycle. The UI is Japanese only, with every UI string in one client-side catalog (`src/frontend/src/features/production-orders/messages.ts`) and no i18n library; the demo data (30 automobile parts, seeded order notes) is Japanese; identifiers such as order numbers, product codes and message IDs are not translated (WI-005 DEC-001, DEC-002, DEC-005).
- Frontend: Vite + React + TypeScript, Tailwind CSS v4 (no component kit), `react-router-dom` (DEC-001, DEC-005; `work-items/WI-001/decisions.md`), and `lucide-react` icons pinned at an exact version and imported only through `src/frontend/src/components/icons.ts` (WI-004 DEC-023).
- Backend: .NET 10, EF Core, conventional layered structure (`Domain`/`Application`/`Infrastructure`/`Api`) — not minimal-APIs, not CQRS/MediatR (DEC-002; see `docs/en/architecture/0001/0001_ADR_backend-layered-structure.md`).
- Database: PostgreSQL 17 (DEC-003).
- Test placement: backend unit tests in `tests/backend/`, integration tests in `tests/integration/`, E2E in `tests/e2e/` (its own npm package). Frontend unit tests stay inside the frontend package at `src/frontend/tests/unit/`, not `tests/frontend/`: the package's `node_modules` can't be reached from outside it without npm workspaces, which the project has chosen not to adopt (WI-001 step 19 note; kept as-is by ThanhTN on 2026-09-23). Put new frontend unit tests there.
- Test frameworks: Vitest + React Testing Library (frontend), xUnit (backend unit + integration, the latter via `WebApplicationFactory` + Testcontainers.PostgreSql) (DEC-004); Playwright for E2E and axe (`vitest-axe`, `@axe-core/playwright`) for automated accessibility checks (WI-002 DEC-025, DEC-026).
- Authentication/RBAC: in scope, built as foundational bootstrap work before Screen A — ASP.NET Core Identity + cookie-based authentication, same-origin via a dev-server/Nginx proxy (DEC-006, DEC-007; see `docs/en/architecture/0002/0002_ADR_auth-rbac-foundation.md`).
- Repository/branch policy: trunk-based, short `feature/<WI-id>-slug` branches, PR back to `master` (DEC-009).
- Source/PR/CI/CD: GitHub and GitHub Actions.
- Packaging/deployment: Docker (local Compose environment implemented; see Current implementation).
- Shared harness: Markdown consumed by Claude and Codex.
- New project artifacts: English by default and the source of truth. Markdown under `docs/en/` is English only; each document is also rendered to an English PDF under `docs/en/pdf/` and a Japanese PDF under `docs/ja/pdf/` in the same change, with `scripts/docs-pdf.py`; existing documents get theirs on their next edit; work-item records stay English-only (WI-001 DEC-013, RFC 0008; see `ai/rules/documentation.md`). English documents quote the Japanese UI text with an English gloss (WI-005 DEC-003).

## Open decisions

- Registry / deployment host beyond local Docker Compose (WI-001 DEC-012).
- Standing merge/deploy permissions — not granted by this scaffold; each merge or deployment still needs task-specific authorization (CI execution itself is settled: see **CI** below).
- How the four demo videos are produced (WI-001 DEC-014).
- Exact role/permission matrix beyond the placeholder `Admin`/`Operator` seed roles — must be confirmed before any screen gates on a specific permission (WI-001 DEC-015).

## Current implementation

Done and merged to `master`:

- WI-001 (`work-items/WI-001/`): application skeleton and auth foundation, via PR #1.
- WI-002 (`work-items/WI-002/`): Screen A, production-order create/edit (brief, 001_BD, 001_DB, the 001_DD set with mockup, implementation, and unit/integration/E2E tests), via PRs #2 and #3 (WI-002 DEC-030). Since WI-002 the app connects as the restricted `pmai_app` login and migrations run as the owner.
- WI-003 (`work-items/WI-003/`): Screen B, production-order list (brief, 002_BD, 002_DB, the 002_DD set with mockup, implementation, and unit/integration/E2E tests), via PR #9, squash-merged as `8eab65f`. It adds `GET /api/production-orders` (filter, sort, page), the `/production-orders` screen, a `pg_trgm` index for the order-number search, and 80 seeded demo orders. Screen A's Cancel and its panels now return to the list.
- WI-004 (`work-items/WI-004/`): Screen C, production dashboard (brief revision 2, 003_BD, 003_DB, the 003_DD set with mockup, the amendments to Screen A and B's designs, implementation, and unit/integration/E2E tests), via PR #15, squash-merged as `cd3a3b9`. It adds `GET /api/dashboard` (one read-only snapshot, plant-time windows) and `GET /api/system/health` (authenticated; never renews the session), the dashboard at `/` in place of the placeholder home page, `production_orders.completed_at_utc` set by Screen A's `InProgress → Completed` save, two partial indexes, and demo history (the demo database now holds 124 orders). Every screen gets a navbar in the shared header, and an edited Screen A form now asks before any in-app link leaves it.
- WI-005 (`work-items/WI-005/`): Japanese UI and automobile-parts demo data (Japanese UI catalog, ja-JP display formats, data-only migration `LocalizeDemoDataToJapanese`, design documents quoting the Japanese UI, Japanese editions of the mockups), via PR #17, squash-merged as `dbc9527`. The same PR adopted RFCs 0008–0010 (English and Japanese PDFs of every design document, Mermaid and SVG diagrams, number-first document IDs in one folder per number). RFC 0011 (2026-09-24) then named each screen's documents with the screen's Japanese name (`001_BD_製造指示登録・編集.md`).

Next planned work: none yet. Screens A, B and C — the whole locked roadmap (WI-001 DEC-008, DEC-010) — and their Japanese localization (WI-005) are done.

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

**E2E** (`tests/e2e/`, against the running Compose stack): `npm ci`, `npx playwright install chromium`, then `E2E_ADMIN_PASSWORD=<SEED_ADMIN_PASSWORD> E2E_BASE_URL=http://localhost:3000 npx playwright test`. Verified in WI-002, WI-003 and WI-004 (22 cases: Screens A, B and C, and three SP layouts). The `signIn` helper waits for the dashboard heading, not a navbar link, because on SP the navbar sits behind the Menu button.

**Docs PDF** (RFC 0008; needs `pip install markdown`, Google Chrome, and network access for Mermaid): `python scripts/docs-pdf.py docs/en/<path>.md docs/en/pdf/<path>.pdf --source-note "<ID> version <N> (<date>)"`; for the Japanese PDF, `python scripts/docs-pdf.py <temporary translation>.md docs/ja/pdf/<path>.pdf --lang ja --base docs/en/<folder> --source-note "…"`. Verified on 2026-09-23 on 001_BD, 002_BD and 003_BD (six PDFs: Mermaid diagrams, SVG wireframes and Japanese text rendered, page-numbered footers).

**CI**: `.github/workflows/ci.yml` has three jobs on push/PR to `master`: backend (build + unit/integration tests), frontend (lint + build + tests), and e2e (Compose stack + Playwright, after the other two; throwaway credentials generated per run; added by RFC 0003). A push or PR that changes only Markdown, `docs/`, `work-items/`, `demos/`, `ai/` or `LICENSE` starts no run at all (RFC 0005); `.github/`, `deploy/`, `src/` and `tests/` always do. Because GitHub evaluates the filter against a PR's whole diff, a PR that also changes code still runs everything. First executed on 2026-09-18 on PR #5, after the repository moved to the `SateraitoOfficeVN` organization (the earlier account billing lock blocked PRs #2–#4): backend, frontend and e2e all passed (https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/35318583226).

## Candidate demo

Roadmap locked (DEC-008, DEC-010) and complete: Screen A = production-order create/edit (WI-002), Screen B = production-order list (WI-003), Screen C = dashboard (WI-004; its widgets and metrics were settled in WI-004 DEC-001–DEC-009). The earlier "product catalog" candidate was dropped.
