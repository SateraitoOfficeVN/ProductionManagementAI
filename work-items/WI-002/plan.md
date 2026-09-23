# Production Order Create/Edit (Screen A) — Implementation Plan

Revisions are kept in full and in chronological order (oldest first), so the plan can be back-tracked. The last revision is the current one.

| Revision | Date | Phase | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-09-18 | Design (brief → BD → DB → DD + mockup) | approved; complete; superseded by revision 2 | user message 2026-09-18: "approved the plan, let move on to DB design" |
| 2 | 2026-09-18 | Implementation, tests, push/PR | **current** — approved; complete; PRs squash-merged by the user (DEC-030) | user message 2026-09-18: "ok revision 2 plan seem solid so approved" |

Note: revision 1 was reconstructed on 2026-09-18 from the session record. It was briefly overwritten when revision 2 was drafted, and it had never been committed, so git couldn't restore it. The content is as last approved and edited. The "Outcome" column was added at closure.

## Revision 1 — design phase (complete, superseded by revision 2)

Revision 1, 2026-09-18. This restarts WI-002 after its first design pass was scrapped (templates rewritten in commit `e7e0d36`). It covers design only. Implementation will be added in a later revision, after the designs are reconciled.

### Objective

Produce the reconciled design set for Screen A (brief, 001_BD, 001_DB, and 001_DD with a rendered mockup), tracing every requirement in `brief.md`.

### Scope

#### In scope

- Requirements brief and decision log (`work-items/WI-002/`).
- Basic design 001_BD (`docs/en/010_basic-design/`).
- Database design 001_DB (`docs/en/database/`).
- Detailed design 001_DD with a rendered mockup (`docs/en/020_detailed-design/`), using the DD companion templates only if needed.
- Design-consistency reconciliation across all of the above.

#### Out of scope

- Application code, migrations, tests — plan revision 2.
- Screens B and C.
- Any push, PR, merge, image publication or deployment.

### Inputs and assumptions

| Input (brief / BD / DD / DB / ADR / decisions) | Revision | Assumption made if input is missing or incomplete |
| --- | --- | --- |
| First-pass business answers | `demos/01-basic-design/WI-002/recording.md`, `demos/03-database-design/WI-002/recording.md` | Carried over unchanged as DEC-001–DEC-008 |
| 0001_ADR, 0002_ADR, 000_DB | current `master` | Stack/auth conventions unchanged |
| `ai/templates/basic-design.md`, `detailed-design.md`, `DD/*` | commit `e7e0d36` | — |
| DEC-009–DEC-020 | decided 2026-09-18 | — |
| DEC-016 (split DB logins now) | decided 2026-09-18 | Carried into plan revision 2 (implementation): runtime login, grants script, Compose/`.env.example` change. This touches WI-001's deploy config, so revision 2 needs its own review |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Requirements brief + decision log | none | requirements | `work-items/WI-002/brief.md`, `decisions.md` | Every REQ has success + failure criteria | done 2026-09-18 — REQ-010–REQ-018 (REQ-019 added later via DEC-018) |
| 2 | Basic design | 1 | basic-design, screen-design | `docs/en/010_basic-design/001/001_BD_production-order-create-edit.md` | Every REQ mapped to a BD section; design-consistency checklist | done 2026-09-18 — 001_BD revisions 1–4 |
| 3 | Resolve DEC-009 | 2 | — | `decisions.md`, 001_BD V-04 | User answer recorded | done 2026-09-18 — DEC-009–DEC-012 answered together |
| 4 | Database design | 2 | database-design | `docs/en/database/001/001_DB_production-order-schema.md` (001_DB) | Constraints/indexes justified; migration impact stated | done 2026-09-18 — reviewed by the user; DEC-013–DEC-016, DEC-019 |
| 5 | Detailed design + rendered mockup | 3, 4 | detailed-design, screen-design | `docs/en/020_detailed-design/001/001_DD_production-order-create-edit.md` + mockup | DD agrees with BD and DB; test viewpoints cover every REQ | done 2026-09-18 — 001_DD, 001_DD-API, 001_DD-FN, 001_DD-SPD (FN/SPD added at the user's request) + mockup https://claude.ai/artifact/FEo1RG27UjZ6vxFCUjxoHq; DEC-021–DEC-024 |
| 6 | Reconcile and close design phase | 5 | — | `status.md`, `evidence.md` | design-consistency checklist passes; no open business decision | done 2026-09-18 — checklist passed; DD set approved by the user |

### Roles and responsibilities

| Role | Owner |
| --- | --- |
| Plan author, designer | this agent (Claude) |
| Reviewer, approver, business decisions | ThanhTN |

### Resources and external actions

| Action (push / PR / merge / deploy / publish image / …) | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| Local file edits under `work-items/WI-002/`, `docs/` | yes | user request 2026-09-18 ("let work on screen A as per what we planned") | design documents only |
| Publish rendered mockup as a private Artifact | ask at execution | asked and approved at execution, 2026-09-18 ("Yes, publish privately") | step 5 |
| git commit / push / PR / merge | no | not yet authorized | — |

### Risks and mitigations

| Risk / stop condition | Trigger | Mitigation / response |
| --- | --- | --- |
| Missing business decision | DEC-009 or a new ambiguity found in DB/DD | Pause and ask per `ai/policies.md`; record in `decisions.md` |
| BD and DD drift apart | BD §3–§6 and DD field tables disagree | Update both in the same change (template header rule) |
| A carried-over answer is no longer wanted | User rejects one of DEC-001–DEC-008 | Supersede the decision; revise brief/BD |

### Approval / sign-off

- **Review status:** approved
- **Approval source:** user message 2026-09-18: "approved the plan, let move on to DB design" (steps 1–2 had already been done on the user's direct request of the same day)
- **Approved revision:** revision 1, 2026-09-18
- **Closure:** all steps done; design approved by the user 2026-09-18 ("the DD is reviewed and approved, so let move on"); superseded by revision 2

---

## Revision 2 — implementation (current)

Revision 2, 2026-09-18. Supersedes revision 1 (design phase: brief, 001_BD, 001_DB, 001_DD set and mockup), which is complete: all six steps are done, and the design was approved by the user on 2026-09-18 ("the DD is reviewed and approved, so let move on"). This revision adds implementation, tests, and the authorized push/PR. Revision 1 is kept in full above.

### Objective

Implement SCR-001 exactly as designed in 001_BD (revision 4), 001_DB and the 001_DD set (001_DD, 001_DD-API, 001_DD-FN, 001_DD-SPD), with automated tests covering REQ-010–REQ-019. Deliver it as a pull request to `master`.

### Scope

#### In scope

- Backend: Domain, Application, Infrastructure and Api code for the four endpoints; migration `AddProductionOrders` (3 tables, 30 seeded products, runtime-login grants); Problem Details error handling; OpenTelemetry tracing/metrics.
- DB login split (DEC-016): restricted runtime login `pmai_app` in Docker Compose and in the integration-test fixture; migrations stay on the owner login.
- Frontend: `apiClient`, the production-order feature (page, form, discard dialog, banners), routes, and a "New production order" link on the home page.
- Tests: xUnit unit and integration tests, Vitest + RTL component tests with `vitest-axe`, and Playwright E2E with `@axe-core/playwright` (DEC-025, DEC-026).
- `work-items/WI-002/test-plan.md` with `TC-###` IDs; `evidence.md` with actual results.
- Docs updated in the same change: `ai/project.md` verified commands, `tests/README.md`, `tests/e2e/README.md`, `deploy/README.md`, `.env.example`.
- Push the branch and open a PR to `master` (DEC-027). Opening the PR starts the existing CI workflow (`pull_request` trigger), so this is the project's first real CI run.
- The DD-template harness change (RFC 0001) goes on its own branch and PR (DEC-028).

#### Out of scope

- Merging either PR, deployment, and image publication — not authorized.
- Adding E2E to CI. It needs the Compose stack in CI; E2E runs locally and the results are recorded in evidence.
- Screens B and C; any change to the WI-001 auth endpoints beyond what the runtime login needs.
- The untracked `demos/*/WI-002/` recordings are the user's files and stay untouched.

### Inputs and assumptions

| Input (brief / BD / DD / DB / ADR / decisions) | Revision | Assumption made if input is missing or incomplete |
| --- | --- | --- |
| brief.md | revision 1 (REQ-010–REQ-019) | — |
| 001_BD | revision 4 | — |
| 001_DB | reviewed 2026-09-18 | — |
| 001_DD / -API / -FN / -SPD | 001_DD revision 2, companions revision 1, approved 2026-09-18 | — |
| 0001_ADR, 0002_ADR, 000_DB | current `master` + WI-002 edits | — |
| decisions.md | DEC-001–DEC-028 | — |
| Branch naming | WI-001 DEC-009 (`feature/<WI-id>-slug`) | `ai/skills/implementation/SKILL.md` suggests `work-items/<WI-###>`; the project's explicit decision DEC-009 wins (`ai/rules/common.md`) |
| NuGet/npm package versions | latest stable at implementation time | Pinned exact versions; each new dependency is checked for known vulnerabilities (`dotnet list package --vulnerable`, `npm audit`) before use |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Harness branch: create worktree `../WMS-worktrees/harness-wi002-feedback` on `feature/harness-wi002-feedback` from `master` (renamed by the amendment below); copy the RFC 0001 and RFC 0002 changes (`ai/templates/**`, `ai/skills/detailed-design/SKILL.md`, `ai/skills/planning/SKILL.md`, `ai/evaluations/baseline-cases.md`, `ai/improvements/**`); commit as two commits, one per RFC | none | harness-improvement | commit on the harness branch | `git diff master --stat` shows only `ai/` files | done 2026-09-18 — `feature/harness-wi002-feedback`: `dc228ab` (RFC 0001), `4912857` (RFC 0002, added by the amendment) |
| 2 | Screen A branch: create worktree `../WMS-worktrees/WI-002` on `feature/WI-002-production-order-screen` from `master`; copy the WI-002 design files (work item, docs, `CLAUDE.md`, READMEs, 0002_ADR/000_DB edits); commit the "design phase" | none | implementation | commit on the WI-002 branch | Diff contains only WI-002 design files | done 2026-09-18 — `5c40eaa` design-phase commit |
| 3 | Clean the main worktree: after verifying both commits contain every copied file (`git diff --no-index` per file), revert those modified/new files in the main worktree so `master` is clean again. `demos/` is left alone | 1, 2 | — | clean `master` worktree | `git status` shows only `demos/` untracked | done 2026-09-18 — `git status` on master shows only `demos/` |
| 4 | Domain: `ProductionOrder`, `ProductionOrderStatus`, `Product`, `DomainRuleViolation` + unit tests | 2 | implementation, testing | `src/backend/...Domain/ProductionOrders/`, tests | `dotnet test` (unit) | done 2026-09-18 — `a2c1ea7`; unit tests pass |
| 5 | Application: `ProductionOrderService`, DTOs, `Result`, `IPlantClock`, `IOrderNumberIssuer`, `IProductionOrderRepository`, telemetry + unit tests (`FakeTimeProvider`) | 4 | implementation, testing | `...Application/ProductionOrders/`, tests | `dotnet test` (unit) | done 2026-09-18 — `a2c1ea7`; 49/49 unit tests |
| 6 | Infrastructure: EF configurations (`xmin`, generated column, CHECKs), repository, `PlantClock`, `OrderNumberIssuer`, migration `AddProductionOrders` (seed 30 products; `CREATE ROLE pmai_app NOLOGIN` if missing, plus the least-privilege grants from 001_DB on the new and identity tables — no password in the migration) | 5 | implementation, database-design | `...Infrastructure/ProductionOrders/`, `Migrations/` | Migration SQL reviewed (`dotnet ef migrations script`); applied to a local DB | done 2026-09-18 — `e4a5256`; migration SQL reviewed; DEC-029 (EF keeps the product_id FK index) |
| 7 | DB login split (DEC-016): Postgres init script `deploy/db/init/10-app-login.sh` (sets `LOGIN PASSWORD` for `pmai_app` from the new required env var `PMAI_APP_DB_PASSWORD`, no default); backend connection string uses `pmai_app`; migration command documented with the owner connection; `.env.example`, `deploy/README.md`, `ai/project.md` updated. The existing local `db` volume must be wiped once so the init script runs | 6 | implementation, security-review | `deploy/`, docs | `docker compose config`; stack starts; app works as `pmai_app`; `DELETE` as `pmai_app` is denied | done 2026-09-18 — `e4a5256` + db image change; volume wiped, init script ran, owner migration applied, `DELETE` as `pmai_app` denied. The bind mount was refused by Docker Desktop file sharing, so the script is baked into `deploy/docker/db.Dockerfile` |
| 8 | Api: controllers, `ProductionOrderEditor` policy, `AddProblemDetails` + `InvalidModelStateResponseFactory` (message IDs), JSON enum-as-string, `[Consumes]`, `Plant:TimeZone` options with startup validation, OpenTelemetry (ASP.NET Core, EF/Npgsql, OTLP exporter only if the endpoint is set) | 6 | implementation | `...Api/` | `dotnet build` with no warnings introduced | done 2026-09-18 — `e4a5256`; 0 warnings |
| 9 | Integration tests: all 001_DD I-level viewpoints, including concurrent creates, stale save, 401/403/415, Problem Details shape, runtime-login privileges, telemetry (in-memory exporter). The fixture creates `pmai_app` in the Testcontainers DB and runs the app as it | 7, 8 | testing | `tests/integration/.../ProductionOrders/` | `dotnet test src/backend/ProductionManagementAI.slnx` | done 2026-09-18 — 38/38 integration tests |
| 10 | Frontend: `apiClient`, `AppHeader`, feature folder per 001_DD X-1, routes, home link; component tests incl. `vitest-axe` | 8 | implementation, screen-design, testing | `src/frontend/src/...`, `src/frontend/tests/unit/production-orders/` | `npm run lint`, `npm test`, `npm run build` | done 2026-09-18 — `f5609d7`; lint/tsc/build clean; 38/38 frontend tests |
| 11 | E2E: `tests/e2e` Playwright project (own `package.json`), journeys: create → edit → complete; validation errors; locked fields; discard dialog; stale save (two contexts); axe scan per state | 7, 10 | testing | `tests/e2e/` | `npx playwright test` against the Compose stack | done 2026-09-18 — 8/8 Playwright journeys (desktop + Pixel 7) incl. axe |
| 12 | Test plan: `work-items/WI-002/test-plan.md` with `TC-###` → REQ mapping; update the 001_DD test viewpoints' Test-plan ID column | 9–11 | testing | test-plan.md, 001_DD | Every REQ has ≥ 1 TC with a recorded result | done 2026-09-18 — `test-plan.md` TP-002 (TC-001–TC-027); 001_DD IDs filled |
| 13 | Full verification run from a clean environment; security-review and delivery checklists; `evidence.md`, `status.md` | 12 | security-review, pr-review | evidence, status | Checklists recorded pass/fail with reasons | done 2026-09-18 — final run 03:12Z all green; security-review and delivery checklists pass (`evidence.md`) |
| 14 | Push both branches and open two PRs to `master` (Screen A; harness). Watch the triggered CI runs and record the results. Merge is not done | 13 (Screen A); 1 (harness) | pr-review, ci-cd | 2 PRs | PR links + CI results in evidence | done 2026-09-18 — PR #3 (Screen A) https://github.com/thanhtn95/ProductionManagementAI/pull/3 and PR #2 (harness) opened; CI jobs not started on either (GitHub billing lock); GitGuardian secret scan passed on both; not merged (not authorized) |

### Roles and responsibilities

| Role | Owner |
| --- | --- |
| Plan author, implementer | this agent (Claude) |
| Reviewer, approver, merge decision | ThanhTN |

### Resources and external actions

| Action (push / PR / merge / deploy / publish image / …) | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| Create branches/worktrees, local commits | yes | user answer 2026-09-18 (DEC-027) | `feature/WI-002-production-order-screen`, `feature/harness-wi002-feedback` |
| Revert WI-002/harness files in the main worktree after they're committed on their branches | yes, on approval of this plan | this plan, step 3 | Only files copied in steps 1–2; `demos/` untouched |
| Wipe the local Compose `db` volume | yes, on approval of this plan | this plan, step 7 (standing preference: wipe rather than sync passwords) | Local dev volume only |
| Install new NuGet/npm packages (OpenTelemetry, `Microsoft.Extensions.TimeProvider.Testing`, Playwright, `@axe-core/playwright`, `vitest-axe`) | yes, on approval of this plan | this plan | Pinned versions, vulnerability-checked |
| Push branches to `origin` | yes | DEC-027 | The two branches above |
| Open PRs to `master` (this triggers CI) | yes | DEC-027 | Two PRs |
| Merge, deploy, publish images | no | not authorized | — |

### Risks and mitigations

| Risk / stop condition | Trigger | Mitigation / response |
| --- | --- | --- |
| Design gap found during implementation | Code needs behavior the DD doesn't define, or contradicts it | Pause, ask, record in decisions.md, update the DD in the same change |
| First CI run fails for reasons unrelated to WI-002 | CI red on setup/tooling | Diagnose; fix in scope if it's a WI-002 regression; otherwise record it and ask |
| Runtime login breaks existing auth flows | Identity writes denied | Integration tests run as `pmai_app` catch it; grants adjusted in the migration |
| Existing local DB volume doesn't have `pmai_app` | Init script only runs on a fresh volume | Wipe the volume (step 7); documented in `deploy/README.md` |
| Testcontainers/Docker unavailable | Integration/E2E can't run | Record as blocked, never as passed |
| A new dependency has a known vulnerability | audit output | Choose another version/package; ask if there's no alternative |

### Approval / sign-off

- **Review status:** approved
- **Approval source:** user message 2026-09-18: "ok revision 2 plan seem solid so approved"
- **Approved revision:** revision 2, 2026-09-18
- **Amendment (2026-09-18, same message):** the user also asked for a second harness improvement (RFC 0002, keep every plan revision oldest-first). Both harness RFCs go on one harness branch, renamed from `feature/harness-dd-companions` to `feature/harness-wi002-feedback`, as two separate commits, with one harness PR. Step 1 and the step 14 PR count are unchanged (still two PRs). No other scope change.
- **Merge (2026-09-18):** the user squash-merged PR #2 and PR #3 into `master` from their own account (DEC-030); Claude didn't perform the merge. Deploy and image publication remain unauthorized.
- **Closure:** 2026-09-18 — all 14 steps done; PR #2 and PR #3 squash-merged into `master` by the user (DEC-030). CI still hasn't run (GitHub billing lock).
