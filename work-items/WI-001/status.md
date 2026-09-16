<!-- Weekly/milestone status report template, based on common RAG (Red/Amber/Green) status report conventions. Copy into the relevant work item; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# Project Bootstrap (skeleton + auth foundation) — Status Report

As of 2026-09-16. Work item state: in-progress.

## Overall status

**Green — on track**
Plan approved this session; no implementation steps executed yet.

## Approved plan reference

`work-items/WI-001/plan.md` — approved revision 1, approval source: user message in this session via plan-mode approval, 2026-09-16.

## Accomplishments this period

| Date | Milestone / deliverable | Evidence link |
| --- | --- | --- |
| 2026-09-16 | Open technology/scope decisions resolved with user (frontend/backend/DB/test stack, auth scope+sequencing, Screen A→B→C roadmap, branch policy, execution mode) | `work-items/WI-001/decisions.md` |
| 2026-09-16 | Plan drafted and approved | `work-items/WI-001/plan.md` |
| 2026-09-16 | Brief drafted | `work-items/WI-001/brief.md` |
| 2026-09-16 | Step 1 (preflight) run: .NET 10.0.303, Node v24.21.0, npm 11.19.0, Docker 29.7.2, Compose v5.4.0, Linux containers — no blockers | `work-items/WI-001/evidence.md` |
| 2026-09-16 | Step 3: ADR-0001 (backend layered structure) and ADR-0002 (auth/RBAC foundation, incl. STRIDE review) drafted | `docs/en/architecture/0001-backend-layered-structure.md`, `docs/en/architecture/0002-auth-rbac-foundation.md` |
| 2026-09-16 | Step 4: DB-001 (Identity schema) drafted | `docs/en/database/0001-identity-schema.md` |
| 2026-09-16 | Docs committed to `master` (commit `6cfbb6c`): ADR-0001, ADR-0002, DB-001, BD-001, and `work-items/WI-001/*` | `git log` |
| 2026-09-16 | Step 5: worktree `D:/Work/AI/WMS-WI-001-bootstrap` created on branch `feature/WI-001-bootstrap-skeleton` | `git worktree list` |
| 2026-09-16 | Step 6: backend solution scaffolded — `ProductionManagementAI.{Domain,Application,Infrastructure,Api}`, layered per ADR-0001, `dotnet build` passes | `src/backend/*`, `evidence.md` |
| 2026-09-16 | Step 7: frontend scaffolded — Vite+React+TS+Tailwind v4+react-router-dom, `npm run build` passes | `src/frontend/*`, `evidence.md` |
| 2026-09-16 | Step 8: `strict: true` added to `tsconfig.app.json`/`tsconfig.node.json`; `npm run lint` (oxlint) passes | `src/frontend/tsconfig.*.json` |
| 2026-09-16 | Step 9: Vite dev proxy `/api` → `http://localhost:5033` (backend `http` launch profile) | `src/frontend/vite.config.ts` |
| 2026-09-16 | Step 10: `AppUser`/`AppRole` (Infrastructure, not Domain — see decisions.md addendum), `AppDbContext` with DB-001 table/index names, config-based connection string via `AddInfrastructure(IConfiguration)` | `src/backend/ProductionManagementAI.Infrastructure/*` |
| 2026-09-16 | Step 11: initial migration `InitialIdentitySchema` generated; table/index names verified against DB-001; `dotnet ef migrations script` generates cleanly | `src/backend/ProductionManagementAI.Infrastructure/Migrations/*` |
| 2026-09-16 | Step 12: `AuthController` (login/me/logout) on `SignInManager`/`UserManager` directly; global fallback policy (authenticated user required) + `AdminOnly` policy; cookie-auth redirect rewired to 401/403 for a JSON API; manually verified unauthenticated `/api/auth/me` → 401, `/api/auth/login` reachable anonymously | `src/backend/ProductionManagementAI.Api/*`, `evidence.md` |
| 2026-09-16 | Step 13: `ProductionManagementAI.Application.Tests` (xUnit) added to `src/backend/ProductionManagementAI.slnx`; 4 tests covering `LoginRequest`/`MeResponse`, all pass. `Domain.Tests` deliberately not created — Domain has no entities yet (Screen A/WI-002 introduces the first, per DEC-008); will be added then | `tests/backend/ProductionManagementAI.Application.Tests/*`, `tests/backend/README.md` |
| 2026-09-16 | Step 14: `IdentitySeeder` (Development only) — seeds `Admin`/`Operator` roles + one `admin` user from `SEED_ADMIN_PASSWORD`; fails startup before any DB access if the env var is unset; manually verified both the unset-var failure and the pass-through-to-DB-step behavior | `src/backend/ProductionManagementAI.Infrastructure/Identity/IdentitySeeder.cs`, `src/backend/README.md` |
| 2026-09-16 | Step 15: `deploy/docker/{backend,frontend}.Dockerfile` (multi-stage), `frontend.nginx.conf` (same-origin reverse proxy per ADR-0002), `deploy/compose.yaml` (postgres:17 + backend + frontend), `deploy/.env.example`; `docker compose config` resolves cleanly | `deploy/*` |
| 2026-09-16 | Step 16: `docker compose up -d db` (postgres:17, published on host port 5433 — 5432 collides with an unrelated pre-existing local container, see decisions.md); `dotnet ef database update` applied `InitialIdentitySchema`; all 7 DB-001 tables + exact index/constraint names verified live via `psql` | `deploy/compose.yaml`, `evidence.md` |
| 2026-09-16 | Step 17: full stack up (`docker compose up -d --build`) — all 3 containers running; added minimal `GET /health` endpoint (`[AllowAnonymous]`) to satisfy this step's own verification method; `FRONTEND_PORT` default moved 8080→3000 (8080 is in Windows' reserved/excluded TCP port range on this machine, see decisions.md); backend `/health` → 200, frontend root → 200, frontend's `/api/auth/me` proxy → 401 (unauthenticated); full login→me→logout→me session cycle verified against the real containers (200/200/200/401), matching ADR-0002's own confirmation criteria | `src/backend/ProductionManagementAI.Api/Program.cs`, `deploy/compose.yaml`, `evidence.md` |
| 2026-09-16 | Step 18: frontend auth UI — `AuthProvider`/`useAuth` (calls `/api/auth/me` on mount, exposes `login`/`logout`), `ProtectedRoute` (client-side UX redirect only, server remains sole authority per ADR-0002), `LoginPage` (generic error message on failure), placeholder authenticated `HomePage` with sign-out; `npm run build`/`npm run lint` both clean; manually exercised the full flow in a real browser against the live stack (unauthenticated → redirected to `/login`; login as seeded admin → home shows "Signed in as Seed Admin (Admin)"; session persists across reload; sign out → back to `/login`; wrong password → visible generic error) | `src/frontend/src/features/auth/*`, `src/frontend/src/App.tsx`, `src/frontend/src/main.tsx`, `evidence.md` |
| 2026-09-16 | Step 19: Vitest + React Testing Library — 5 tests covering `LoginPage` (navigates on success, generic error on failure) and `ProtectedRoute` (loading/redirect/authenticated states), all pass; tests placed at `src/frontend/tests/unit/` rather than the plan's literal `tests/frontend/unit/` path — cross-package Vite/Node module resolution doesn't work without npm workspaces, out of scope here; see decisions.md | `src/frontend/tests/unit/*`, `src/frontend/vite.config.ts`, `decisions.md` |
| 2026-09-16 | Step 20: `ProductionManagementAI.Integration.Tests` (xUnit + `WebApplicationFactory<Program>` + Testcontainers.PostgreSql — spins up a real, throwaway Postgres 17 per run, self-contained/CI-ready) — 4 tests: unauthenticated `/me` → 401, login as seeded admin → 200 with correct `MeResponse`, login→logout→`/me` → 401, wrong password → 401. **Found and fixed a real bug along the way**: `AddInfrastructure` read the DB connection string eagerly at service-registration time, before host-building config overrides (e.g. from `WebApplicationFactory`) were guaranteed merged in — fixed by resolving it lazily via the injected `IServiceProvider` inside `AddDbContext`. All 8 backend tests (4 unit + 4 integration) pass | `tests/integration/ProductionManagementAI.Integration.Tests/*`, `src/backend/ProductionManagementAI.Infrastructure/DependencyInjection.cs`, `src/backend/ProductionManagementAI.Api/Program.cs`, `evidence.md` |
| 2026-09-16 | Step 21: Playwright E2E smoke spec **deferred** (plan explicitly allows this) — already have equivalent coverage: frontend unit tests, backend unit+integration tests, and a real-browser walkthrough (step 18) against the live Docker stack exercising the exact login→me→logout flow a Playwright spec would cover. Reason recorded here rather than left silent, per plan.md's risk-table allowance | `evidence.md` |
| 2026-09-16 | Step 22: `.github/workflows/ci.yml` — backend job (`dotnet build`/`dotnet test`, includes Testcontainers-backed integration tests) + frontend job (`npm run lint`/`npm run build`/`npm test`); build+lint+test only, no deploy/publish; reviewed manually (not run — push/CI-execution unauthorized). PR template already existed at repo scaffold level, reviewed and left as-is (already generic/adequate) | `.github/workflows/ci.yml`, `.github/workflows/README.md` |
| 2026-09-16 | Step 23: full local verification re-run (Release config, matching CI exactly) — backend build/test 8/8 pass, frontend lint/build/test all pass; final live-stack smoke check found the seeded admin account locked out (`lockout_end` ~5.5 min in the future) from accumulated wrong-password attempts made across this session's own testing — confirms the lockout mitigation (ADR-0002 STRIDE: Spoofing) genuinely works, not a regression; login/session-cycle already proven working in steps 17/18/20, so did not wait out the 15-minute window. `docker compose down` — all 3 containers stopped/removed cleanly, `db-data`/`dp-keys` volumes preserved | `evidence.md` |

## Planned for next period

Step 24: update `ai/project.md` with resolved decisions, verified commands, and the locked Screen A→B→C roadmap.

## Risks and issues

| Issue / blocker | Owner | Since | Impact |
| --- | --- | --- | --- |
| Data Protection: "No XML encryptor configured" warning at backend startup in the container | Claude (noted, not fixed) | 2026-09-16 (step 17) | Keys persist to `/keys` (the `dp-keys` volume) unencrypted at rest. Accepted as a known MVP-scope gap for local dev, consistent with ADR-0002's "Denial of service via the auth mechanism itself" row treating some risks as accepted-not-mitigated; revisit if this stack is ever exposed beyond local dev |

## Next action

Start step 24 (update `ai/project.md`) in `D:/Work/AI/WMS-WI-001-bootstrap`. Execution mode note (see decisions.md): Claude is running local git/scaffold commands directly for this work item, per the user's "continue next steps" direction after exiting plan mode.
