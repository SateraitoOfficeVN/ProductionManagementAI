<!-- Requirements Traceability Matrix + Test Execution Log template, based on common RTM and test-evidence conventions (Katalon, Atlassian, IEEE 829 test logs). Copy into the relevant work item; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# Project Bootstrap (skeleton + auth foundation) — Requirements Traceability & Evidence

As of source revision/commit: branch `feature/WI-001-bootstrap-skeleton`, based on `6cfbb6c`; steps 6–9 not yet committed, 2026-09-16.

## Traceability matrix

| Requirement ID | Requirement | Design artifact | Code / PR | Test case ID | Status |
| --- | --- | --- | --- | --- | --- |
| INFRA-001 | Frontend/backend skeletons build and run locally | ADR-0001 (not yet written) | not yet started | not yet defined | not started |
| INFRA-002 | Login → session → logout flow with user/role model | ADR-0002, DB-001 (not yet written) | not yet started | not yet defined | not started |
| INFRA-003 | CI skeleton (build+lint+test) | `.github/workflows/ci.yml` (not yet written) | not yet started | not applicable — reviewed, not run (push unauthorized) | not started |
| INFRA-004 | `ai/project.md` reflects verified stack/commands | this evidence log | not yet started | not applicable | not started |

## Test execution log

| Date | Check | Command | Environment | Result (pass / fail / not run) | Report / log link |
| --- | --- | --- | --- | --- | --- |
| 2026-09-16 | preflight: .NET SDK version | `dotnet --version` | local (Windows, Git Bash) | pass — `10.0.303` | this row |
| 2026-09-16 | preflight: Node version | `node --version` | local | pass — `v24.21.0` | this row |
| 2026-09-16 | preflight: npm version | `npm --version` | local | pass — `11.19.0` | this row |
| 2026-09-16 | preflight: Docker version | `docker --version` | local | pass — `29.7.2` (build a7dcaa6) | this row |
| 2026-09-16 | preflight: Docker Compose version | `docker compose version` | local | pass — `v5.4.0` | this row |
| 2026-09-16 | preflight: Docker engine OS type (Linux vs Windows containers) | `docker info --format '{{.OSType}}'` | local | pass — `linux` | this row |

Step 1 (preflight) complete — no blockers. Step 2 (this work item's own docs) complete.

| 2026-09-16 | Step 3: ADR-0001/ADR-0002 review | manual review against `ai/checklists/security-review.md` | local (doc review) | pass — every checklist line addressed: auth boundary enforced server-side (STRIDE table's Elevation-of-privilege row), no credentials hardcoded (seed password via env var, startup fails if unset), generic error responses (Information-disclosure row), CSRF approach flagged and deferred explicitly to DD rather than left silently unaddressed | `docs/en/architecture/0001-backend-layered-structure.md`, `docs/en/architecture/0002-auth-rbac-foundation.md` |

| 2026-09-16 | Step 4: DB-001 review | manual review against `ai/checklists/design-consistency.md` | local (doc review) | pass — entities/relationships/constraints support INFRA-002; migration impact stated (additive, first migration, no recovery-limit concern); one open decision (CSRF strategy) explicitly deferred to Screen A's detailed-design rather than left silent | `docs/en/database/0001-identity-schema.md` |

| 2026-09-16 | Step 5: docs commit to `master` | `git commit` (9 files) | local | pass — commit `6cfbb6c` | `git log` |
| 2026-09-16 | Step 5: worktree + branch creation | `git worktree add ../WMS-WI-001-bootstrap -b feature/WI-001-bootstrap-skeleton` | local | pass — worktree at `D:/Work/AI/WMS-WI-001-bootstrap`, branch `feature/WI-001-bootstrap-skeleton` | `git worktree list` |
| 2026-09-16 | Step 6: backend solution build | `dotnet build ProductionManagementAI.slnx` | local, `src/backend` | pass — 4 projects (Domain/Application/Infrastructure/Api), 0 warnings, 0 errors | this row |
| 2026-09-16 | Step 7: frontend build | `npm run build` | local, `src/frontend` | pass — Tailwind CSS output present (`index-*.css`, 6.61 kB), Vite build succeeded | this row |
| 2026-09-16 | Step 8: frontend lint | `npm run lint` (oxlint) | local, `src/frontend` | pass — no findings | this row |
| 2026-09-16 | Step 8: strict TS build re-check after `strict: true` | `npm run build` | local, `src/frontend` | pass — no new type errors | this row |
| 2026-09-16 | Step 9: Vite proxy config review | manual review of `vite.config.ts` | local (doc review) | pass — `/api` → `http://localhost:5033`, matches backend `http` launch profile in `Properties/launchSettings.json` | `src/frontend/vite.config.ts` |
| 2026-09-16 | Step 10: backend build after EF Core Identity model + `AppDbContext` | `dotnet build ProductionManagementAI.slnx` | local, `src/backend` | pass — 0 warnings, 0 errors | this row |
| 2026-09-16 | Step 11: migration generation | `dotnet ef migrations add InitialIdentitySchema --project ProductionManagementAI.Infrastructure --startup-project ProductionManagementAI.Api` | local, `src/backend` | pass — first attempt used Identity's default index names (`RoleNameIndex`/`EmailIndex`/`UserNameIndex`); fixed via explicit `HasDatabaseName` calls in `AppDbContext`, migration regenerated (`dotnet ef migrations remove --force`, then re-add) | `src/backend/ProductionManagementAI.Infrastructure/Migrations/20260916040510_InitialIdentitySchema.cs` |
| 2026-09-16 | Step 11: migration script review against DB-001 | `grep` for table/index/default-value lines in the migration | local (doc review) | pass — table names (`users`,`roles`,`user_roles`,`user_claims`,`role_claims`,`user_logins`,`user_tokens`) and index names (`ix_users_normalized_user_name`, `ix_users_normalized_email`, `ix_roles_normalized_name`, plus Identity defaults for claims/logins) match DB-001 exactly; `id` columns have `gen_random_uuid()` default, `created_at_utc` has `now()` default | this row |
| 2026-09-16 | Step 11: migration script generates cleanly | `dotnet ef migrations script --project ProductionManagementAI.Infrastructure --startup-project ProductionManagementAI.Api` | local, `src/backend` | pass — 100-line SQL script generated with no errors | this row |
| 2026-09-16 | Step 12: backend build after `AuthController` + auth wiring | `dotnet build ProductionManagementAI.slnx` | local, `src/backend` | pass — 0 warnings, 0 errors; required adding `<FrameworkReference Include="Microsoft.AspNetCore.App" />` to Infrastructure.csproj (plain classlib SDK doesn't pull in `AddIdentity`/`ConfigureApplicationCookie`/`AddDataProtection` without it) | this row |
| 2026-09-16 | Step 12: pending-model-changes check | `dotnet ef migrations has-pending-model-changes --project ProductionManagementAI.Infrastructure --startup-project ProductionManagementAI.Api` | local, `src/backend` | pass — "No changes have been made to the model since the last migration" (AddIdentity/cookie config don't alter the EF model) | this row |
| 2026-09-16 | Step 12: unauthenticated protected endpoint | `dotnet run` (API only, no DB) then `curl http://localhost:5033/api/auth/me` | local | pass — `401` (confirms `OnRedirectToLogin` rewire works; without it Identity's cookie middleware would return a 302 redirect, breaking a JSON API) | this row |
| 2026-09-16 | Step 12: anonymous login endpoint reachable | `curl -X POST http://localhost:5033/api/auth/login -d '{"userName":"admin","password":"wrong"}'` | local | pass — reached the controller action (failed downstream at Npgsql connection, expected since Postgres isn't running until step 15/16); confirms `[AllowAnonymous]` bypasses the fallback policy correctly | this row |
| 2026-09-16 | Step 13: `dotnet test src/backend/ProductionManagementAI.slnx` | `dotnet test` | local, `src/backend` | pass — 4/4 tests passed (`LoginRequestTests`, `MeResponseTests`) | this row |
| 2026-09-16 | Step 13: full solution build with test project added | `dotnet build src/backend/ProductionManagementAI.slnx` | local, `src/backend` | pass — 0 warnings, 0 errors | this row |
| 2026-09-16 | Step 13: scope note | — | — | `Domain.Tests` not created — `ProductionManagementAI.Domain` has zero entities as of this work item; an empty test project would assert nothing. Will be added in WI-002 (Screen A) when Domain gains its first entity | `tests/backend/README.md` |
| 2026-09-16 | Step 14: backend build after `IdentitySeeder` | `dotnet build ProductionManagementAI.slnx` | local, `src/backend` | pass — 0 warnings, 0 errors | this row |
| 2026-09-16 | Step 14: startup fails when `SEED_ADMIN_PASSWORD` unset | `ASPNETCORE_ENVIRONMENT=Development dotnet run --no-build` (no `SEED_ADMIN_PASSWORD` set) | local | pass — `InvalidOperationException: SEED_ADMIN_PASSWORD must be set...`, thrown before any DB call (the check is ordered first in `IdentitySeeder.SeedAsync`, ahead of role/user creation, so a DB error can never mask it); no password value in output since none was ever set | this row |
| 2026-09-16 | Step 14: seeding proceeds when env var is set | `ASPNETCORE_ENVIRONMENT=Development SEED_ADMIN_PASSWORD='<test value>' dotnet run --no-build` | local | pass — guard passed, failed only at the (expected, not-yet-running) `RoleManager.RoleExistsAsync` DB call; test password value did not appear in any output or in this log | this row |
| 2026-09-16 | Step 15: compose config validation | `docker compose -f deploy/compose.yaml config` | local (with a throwaway, gitignored `deploy/.env` copied from `.env.example`, deleted immediately after) | pass — resolved cleanly: `db`/`backend`/`frontend` services, `db-data`/`dp-keys` volumes, port mappings, `depends_on: db (condition: service_healthy)` all as authored | this row |
| 2026-09-16 | Step 15: repo hygiene | manual review | local | pass — added `.dockerignore` (repo root) excluding `bin/`,`obj/`,`node_modules/`,`dist/`,`.git/` from the build context; confirmed `deploy/.env` is git-ignored (`.gitignore`'s existing `.env`/`.env.*`/`!.env.example` rules) | this row |
| 2026-09-16 | Step 16: port conflict found and avoided | `Get-NetTCPConnection -LocalPort 5432` → `docker ps -a` | local | found host port 5432 already used by a pre-existing, unrelated container named `PG` (`postgres:latest`), not touched; `deploy/compose.yaml`'s `db` service and `appsettings.Development.json` both moved to port 5433 (see decisions.md) | this row |
| 2026-09-16 | Step 16: bring up db | `docker compose up -d db` (from `deploy/`, using a local, gitignored `deploy/.env`) | local | pass — `deploy-db-1` reached `healthy` within ~8s, published on `0.0.0.0:5433->5432/tcp` | this row |
| 2026-09-16 | Step 16: apply migration | `dotnet ef database update --project ProductionManagementAI.Infrastructure --startup-project ProductionManagementAI.Api` | local, `src/backend` | pass — applied `20260916040510_InitialIdentitySchema` | this row |
| 2026-09-16 | Step 16: verify schema via psql | `docker exec deploy-db-1 psql -U postgres -d production_management_ai -c "\dt"` | local | pass — all 7 DB-001 tables present (`users`,`roles`,`user_roles`,`user_claims`,`role_claims`,`user_logins`,`user_tokens`) plus `__EFMigrationsHistory` | this row |
| 2026-09-16 | Step 16: verify index/constraint names via psql | `SELECT tablename, indexname FROM pg_indexes WHERE tablename IN ('users','roles')` | local | pass — `pk_users`, `pk_roles`, `ix_users_normalized_user_name`, `ix_users_normalized_email`, `ix_roles_normalized_name` all present, matching DB-001 exactly | this row |

| 2026-09-16 | Step 17: port conflict found and avoided | `docker compose up -d --build` (first attempt) | local | `frontend` container stuck in `Created` (never `Running`): `ports are not available: exposing port TCP 0.0.0.0:8080` — `netsh interface ipv4 show excludedportrange protocol=tcp` confirmed 8080 is in Windows' reserved/excluded range on this machine; moved `FRONTEND_PORT` default to 3000 (not excluded), recreated the container (see decisions.md) | this row |
| 2026-09-16 | Step 17: full stack up | `docker compose up -d --build` (retry, from `deploy/`) | local | pass — all 3 containers (`db`, `backend`, `frontend`) `Up`; `db` healthy | `docker compose ps` |
| 2026-09-16 | Step 17: backend /health | `curl http://localhost:8081/health` | local | pass — `200` (added minimal `GET /health` `[AllowAnonymous]` endpoint to `Program.cs` to satisfy this step's verification method — not previously required by any earlier step) | this row |
| 2026-09-16 | Step 17: frontend root | `curl http://localhost:3000/` | local | pass — `200` | this row |
| 2026-09-16 | Step 17: same-origin proxy end-to-end | `curl http://localhost:3000/api/auth/me` (unauthenticated) | local | pass — `401`, proxied correctly through `frontend`'s nginx to `backend` (confirms the ADR-0002 same-origin design works through the real containers, not just in isolation) | this row |
| 2026-09-16 | Step 17: full session lifecycle (ADR-0002 confirmation criteria) | `curl` login → `/me` → logout → `/me`, with cookie jar, through `http://localhost:3000` | local | pass — `200` (login, returns correct `MeResponse` shape) → `200` (`/me`, authenticated) → `200` (logout) → `401` (`/me`, post-logout) | this row |
| 2026-09-16 | Step 17: seeding confirmed live | `docker compose logs backend` | local | pass — log shows `INSERT INTO user_roles` / `UPDATE users` during startup, confirming `IdentitySeeder` ran successfully against the real container | this row |

Remaining steps from `plan.md`'s Deliverables and milestones table (18 onward) have not been executed yet — step 18 (frontend auth UI) is next. `db`/`backend`/`frontend` containers left running (plan step 23 tears everything down after the full verification pass).

## Defects, failures and blockers

| Item | Reason | Blocker | Follow-up |
| --- | --- | --- | --- |
| None currently | — | — | — |

## External references

- PR: not opened — not authorized
- CI run: not applicable — no code exists yet
- Deployment: not applicable — not authorized

## Remaining limitations and next action

Nothing has been implemented yet; this work item is at plan-approved, step-1-not-started state. Next action: run preflight (plan.md step 1) and report results so this log and `status.md` can be updated.
