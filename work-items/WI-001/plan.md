<!-- Implementation Plan template, based on common project/implementation-plan conventions (PMI-style plans, Smartsheet/TeamGantt implementation plan templates). Copy into the relevant work item; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# Project Bootstrap (skeleton + auth foundation) — Implementation Plan

Revision 1, 2026-09-16.

## Objective

Stand up the ProductionManagementAI application skeleton (Vite+React+TS+Tailwind frontend; .NET 10+EF Core layered backend; PostgreSQL 17) with a real login/session/RBAC foundation, a local Docker environment, and a build+lint+test CI skeleton — so the first feature (Screen A) can be built on a working base instead of from nothing. Traces to `brief.md` INFRA-001..004.

## Scope

### In scope

- Repo layout: `src/backend` (Domain/Application/Infrastructure/Api), `src/frontend`, `tests/{backend,frontend,integration,e2e}`, `deploy/docker`, `.github/workflows`.
- Auth foundation: ASP.NET Core Identity (EF Core store) + cookie auth, login/me/logout endpoints, `AdminOnly` policy, seeded `Admin`/`Operator` roles (Development only, placeholder values).
- Frontend auth UI: `LoginPage`, `AuthContext`/`useAuth`, `ProtectedRoute`, placeholder authenticated home page.
- Local Docker Compose environment (Postgres 17 + backend + frontend), `.env.example`.
- CI skeleton (`ci.yml`): build + lint + test only.
- `ai/project.md` updated with resolved decisions, verified commands, and the locked Screen A→B→C roadmap.
- 0001_ADR (backend layered structure), 0002_ADR (auth/RBAC foundation incl. brief threat review), 000_DB (Identity schema).

### Out of scope

- Any business screen (Screen A: production-order create/edit; Screen B: production-order list; Screen C: dashboard) — separate work items after WI-001 is done.
- Push, PR, merge, CI execution on GitHub, image publish, deployment — not authorized by `ai/policies.md`.
- Final permission matrix, registry/deploy host, Japanese-translation-sync policy, demo-video production method.

## Inputs and assumptions

| Input (brief / BD / DD / DB / ADR / decisions) | Revision | Assumption made if input is missing or incomplete |
| --- | --- | --- |
| `work-items/WI-001/brief.md` | 1 | None — brief authored alongside this plan |
| `ai/project.md` | as of 2026-09-16 | Confirmed stack facts are correct; Open decisions resolved this session per `decisions.md` |
| `docs/vi/000-mo-ta-harness-va-quy-trinh-phat-trien-ai.md` | v0.1, 2026-09-15 | Treated as a proposal/synthesis, not an approval — decisions re-confirmed directly with the user this session |
| 0001_ADR, 0002_ADR, 000_DB | not yet written | Will be authored as steps 3–4 before dependent code is written |

## Deliverables and milestones

Execution mode: the user runs the actual scaffold/build/git commands themselves (worktree/branch creation, `dotnet`/`npm`/`docker`/`ef` commands); this agent authors documentation and source content on request and reviews reported results. See `status.md` for current step and owner per step.

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method |
| --- | --- | --- | --- | --- | --- |
| 1 | Preflight: confirm local `dotnet`, `node`/`npm`, `docker`/`docker compose` versions | none | implementation | recorded version output | `--version` output for each tool |
| 2 | Draft `work-items/WI-001/{brief,plan,status,decisions}.md` | none | planning | this work item's docs | files present; approval fields filled |
| 3 | 0001_ADR (layered structure), 0002_ADR (auth/RBAC incl. threat review) | 2 | architecture | `docs/en/architecture/0001_ADR_*.md`, `0002_ADR_*.md` | reviewed against `ai/checklists/security-review.md` |
| 4 | 000_DB (Identity schema) | 3 | database-design | `docs/en/database/000/000_DB_identity-schema.md` | reviewed against `ai/checklists/design-consistency.md` |
| 5 | Create worktree + branch `feature/WI-001-bootstrap-skeleton` | 4 | implementation | isolated worktree | `git worktree list`; clean status on correct branch |
| 6 | Backend solution/projects scaffold | 5 | implementation | `src/backend/*` | `dotnet build` |
| 7 | Frontend scaffold (Vite+React+TS+Tailwind+router) | 5 | implementation | `src/frontend/*` | `npm run build` |
| 8 | Frontend lint/strict TS config | 7 | implementation | eslint/tsconfig | `npm run lint` |
| 9 | Vite dev proxy `/api` → backend | 7 | implementation | `vite.config.ts` | config review |
| 10 | EF Core Identity model + `AppDbContext` + config-based connection string | 6 | implementation | `Infrastructure/AppDbContext.cs` | `dotnet build` |
| 11 | Initial EF Core migration | 10 | implementation | `Migrations/InitialIdentitySchema` | `dotnet ef migrations add` succeeds; script generates cleanly |
| 12 | `AuthController` (login/me/logout) + global auth policy + `AdminOnly` policy | 10 | implementation | `Api/Controllers/AuthController.cs` | `dotnet build` |
| 13 | xUnit unit tests (Domain, Application) | 12 | testing | `tests/backend/*` | `dotnet test` (unit projects) |
| 14 | Seed roles/admin user (Development only, env-driven password) | 11 | implementation | seed logic | manual review; confirmed by step 20 |
| 15 | Dockerfiles + `compose.yaml` + `.env.example` | 6,7 | implementation | `deploy/docker/*`, `deploy/compose.yaml` | `docker compose config` |
| 16 | Bring up Postgres, apply migration | 15 | implementation | running DB with schema | `dotnet ef database update`; tables visible via `psql` |
| 17 | Full stack `docker compose up -d` | 16 | implementation | running app | containers healthy; backend `/health` and frontend root return 200 |
| 18 | Frontend auth UI (LoginPage, AuthContext, ProtectedRoute, placeholder home) | 9 | implementation | `src/frontend/src/features/auth/*` | `npm run build`, `npm run lint` |
| 19 | Vitest unit tests (login form, ProtectedRoute redirect) | 18 | testing | `tests/frontend/unit/*` | `npm run test` |
| 20 | Integration tests: migration, seeded roles, login, `/api/auth/me` 401/200 | 14,17 | testing | `tests/integration/*` | `dotnet test tests/integration/...` |
| 21 | (Optional) one Playwright smoke spec | 17 | testing | `tests/e2e/*` | `npx playwright test`, or recorded as deferred with reason |
| 22 | `.github/workflows/ci.yml` + PR template | 8,13,19 | ci-cd | `.github/workflows/ci.yml` | manual YAML review (not run — push unauthorized) |
| 23 | Full local verification pass, results recorded | 6–22 | testing | recorded results | all commands above re-run in sequence; `docker compose down` after |
| 24 | Update `ai/project.md` | 23 | planning | `ai/project.md` diff | diff review against this session's decisions |
| 25 | Update WI-001 `status.md`/`evidence.md`/`decisions.md` | 24 | planning | updated work-item docs | `ai/checklists/delivery.md` pass |
| 26 | Local commits on the feature branch at logical checkpoints | throughout | implementation | commit history | `git log`/`git status` |

## Roles and responsibilities

| Role | Owner |
| --- | --- |
| Plan author | Claude (this session) |
| Implementer (commands/git actions) | trannhatthanh31@gmail.com (user) |
| Implementer (documentation/source content on request) | Claude (this session) |
| Reviewer / approver | trannhatthanh31@gmail.com (user) |

## Resources and external actions

| Action (push / PR / merge / deploy / publish image / …) | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| Local file creation/edits under in-scope paths | yes | this plan's approval | paths listed in Scope → In scope |
| Local `dotnet`/`npm`/`docker`/`docker compose` commands | yes | this plan's approval | run by the user, per Execution mode |
| Create dedicated worktree/branch, local `git commit` | yes | this plan's approval | branch `feature/WI-001-bootstrap-skeleton`; docs commit may land on `master` if requested |
| `git push` | no | not yet authorized | ask at execution if/when needed |
| Open PR | no | not yet authorized | ask at execution if/when needed |
| Merge | no | not yet authorized | ask at execution if/when needed |
| Run GitHub Actions (actual execution) | no | not yet authorized | ask at execution if/when needed |
| Publish/tag container image | no | not yet authorized | ask at execution if/when needed |
| Deploy | no | not yet authorized | ask at execution if/when needed |

## Risks and mitigations

| Risk / stop condition | Trigger | Mitigation / response |
| --- | --- | --- |
| Local `dotnet`/`node`/`docker` missing or mismatched | preflight step 1 fails | report blocker; never fabricate a build/test result |
| `postgres:17` image not pullable | `docker compose up` fails to pull | report blocker; do not silently substitute another version |
| Docker Desktop in Windows-container mode | `docker info` shows Windows containers | report blocker before going deep into compose up |
| Identity/Npgsql/naming-convention packages not yet available for `net10.0` | `dotnet add package` fails | report exact error; do not silently downgrade target framework |
| Tailwind major-version config drift (v3 vs v4) | installed version's config shape differs from assumed | verify installed version's actual setup at implementation time |
| Playwright unavailable in this environment | install/browser fetch fails | record E2E smoke as deferred with reason; unit/integration coverage still required |
| Seed admin password hardcoded or logged | code/log review | fail startup if unset in Development; never write it to `evidence.md` or logs |
| Role names mistaken for a settled business decision | any code/doc assumes specific permission semantics | flagged explicitly in `decisions.md`; must be confirmed before Screen A |
| Missing decision changes business behavior, or an action exceeds this plan's authorization | encountered mid-step | stop, describe question/impact/options/recommendation, record in `decisions.md`; independent work continues — silence is not approval |

## Approval / sign-off

- **Review status:** approved
- **Approval source:** user message in this session, via plan-mode `ExitPlanMode` approval, 2026-09-16
- **Approved revision:** Revision 1, 2026-09-16
