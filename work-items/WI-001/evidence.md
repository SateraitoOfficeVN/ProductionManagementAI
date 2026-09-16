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

Remaining steps from `plan.md`'s Deliverables and milestones table (12 onward) have not been executed yet — step 12 (`AuthController` + auth policies) is next.

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
