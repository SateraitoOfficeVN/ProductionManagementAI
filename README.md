# ProductionManagementAI

An AI-assisted development harness, and the production-management demo app built with it: production orders for an automobile-parts manufacturer, with a Japanese UI. Claude and Codex work from the same Markdown process in [`ai/`](ai/README.md), and every piece of work is traceable from requirement to design, code, tests and evidence in [`work-items/`](work-items/README.md).

**Status:**
- **WI-001 (bootstrap):** application skeleton and auth foundation, merged.
- **WI-002 (Screen A, production-order create/edit):** designed, implemented, tested and merged.
- **WI-003 (Screen B, production-order list):** designed, implemented, tested and merged. It adds filtering, sorting and paging over production orders, plus the first seeded demo orders.
- **WI-004 (Screen C, production dashboard):** designed, implemented, tested and merged. The dashboard is the landing page at `/`: status and delivery figures, overdue and due-soon orders, top products, and workload and completion-trend charts that can be maximized, with a server/database health indicator. It also adds completion tracking on orders, a navbar with icons on every screen, and demo history (124 seeded orders).
- **WI-005 (Japanese UI and automobile-parts domain):** done and merged. Every screen is Japanese and the demo is production management for automobile parts, with 30 Japanese part names. The design documents quote the Japanese UI with an English gloss, and every design document is also published as an English and a Japanese PDF under `docs/en/pdf/` and `docs/ja/pdf/`.
- **WI-007 (design Markdown review):** merged through PR #29. The agent presents each ADR, BD, DB and DD Markdown file with its required companion artifacts, then waits for review before writing the next design file ([RFC 0012](ai/improvements/0012-sequential-document-review.md)).
- **WI-006 (Product master):** done and merged via [PR #31](https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/31). Admin and Operator can maintain products at `/products`; SKU is immutable, referenced units are locked and retired products remain on historical orders. Orders support exact kg/m decimals; dashboard cross-unit metrics use order counts with per-unit quantities. Live/demo migration and deployment have not been performed.
- **CI:** runs the backend, frontend and end-to-end jobs on every PR to `master` and passes. Changes that touch only documentation, work items, demos or `ai/` skip CI (RFC 0005).
- **WI-008:** Product master dialog centering and distinct Package navigation glyph merged via [PR #33](https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/33) as `20c8d61`; Backend, Frontend and E2E CI passed. Branch/worktree cleanup complete; no deployment.
- **WI-009:** Production lines and order assignment merged via [PR #34](https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/34) as `42e8932`; all three final-head CI jobs passed. Local 192 unit, 157 integration, 196 frontend and 42 E2E pass; merged feature branch/worktree cleaned up. No live/demo deployment.
- **WI-010:** Plant calendar merged via [PR #36](https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/36) as `d4dd976`: weekly patterns, plant/line exceptions/history, current dated capacity and eleven Admin/Operator APIs. Final-head [CI run 36975730228](https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/36975730228) passed 239 unit, 195 integration, 253 frontend and 48 E2E tests. Feature branch/worktree cleaned; approved designs unchanged. Screen-reader speech/physical mobile keyboard/IME remain Not run with accepted local handoff limits. See [status](work-items/WI-010/status.md). No live activation/deployment.
- **WI-011:** Calendar button height/spacing fixes delivered in [PR #38](https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/38) under approved revision 2. CI on reviewed head d383d41 passed 239 unit/195 integration/253 frontend/59 E2E; existing designs unchanged. New web/mobile English/Japanese evidence verified locally and kept outside git. See [status](work-items/WI-011/status.md).
- **Next:** WI-011 PR review and separate merge authorization; no next feature approved. BOM remains a candidate.

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

Still open: registry/deployment host beyond local Compose, merge/deploy permissions, how the demo videos are produced, and the full role/permission matrix. Full detail is in [`ai/project.md`](ai/project.md).

## Run it locally

Setting up a new computer? Open the [local setup guide](deploy/local-setup.html) in a browser (download it or open it from your clone; GitHub shows its source). It covers every step from installing the tools to signing in, in English, Japanese and Vietnamese, and works offline. The short version, once the tools are installed:

1. Copy `deploy/.env.example` to `deploy/.env` and fill it in. `PMAI_APP_DB_PASSWORD` and `SEED_ADMIN_PASSWORD` are required and have no default. The admin password must pass ASP.NET Core Identity's default rules: at least 6 characters, with an uppercase letter, a lowercase letter, a digit and a symbol.
2. `docker compose -f deploy/compose.yaml up -d --build db`
3. Apply migrations as the database owner (the exact command is in [`deploy/README.md`](deploy/README.md)).
4. `docker compose -f deploy/compose.yaml up -d --build`, then open http://localhost:3000 and sign in as `admin` with your `SEED_ADMIN_PASSWORD`. You land on the dashboard; the navbar leads to the order list, a new order and Product master. Existing databases need the coordinated WI-006 cutover described in deploy/README.md before running the new application.

## Layout

- [ai](ai/README.md): shared workflows, skills, rules, templates, checklists and harness-improvement records.
- [docs](docs/README.md): requirements, basic/detailed/database designs, ADRs and test documentation in English (`docs/en/`), each with English and Japanese PDFs (`docs/en/pdf/`, `docs/ja/pdf/`), plus the initial Vietnamese specification.
- [work-items](work-items/README.md): each work item's brief, plan (every revision), decisions, status and evidence.
- [src](src/README.md): backend (.NET 10) and frontend (Vite + React + TypeScript) application source.
- [tests](tests/README.md): backend unit (`tests/backend/`) and integration (`tests/integration/`) tests, and Playwright E2E journeys (`tests/e2e/`).
- Frontend unit tests are the one exception: they live inside the frontend package at `src/frontend/tests/unit/` (Vitest + React Testing Library), not in `tests/frontend/`. `src/frontend` is a standalone npm package, and Vite resolves package imports by walking up from the test file to a `node_modules` folder, which a file under `tests/` never reaches. Moving them would first need an npm-workspaces setup at the repository root; that is deliberately not done ([WI-001 step 19 note](work-items/WI-001/decisions.md), [tests/frontend/README.md](tests/frontend/README.md)).
- [deploy](deploy/README.md): local Docker Compose environment, database logins, and the [local setup guide](deploy/local-setup.html).
- `.github/`: the [CI workflow](.github/workflows/ci.yml) (backend, frontend and e2e jobs on every push/PR to `master`, except documentation-only changes) and the [pull request template](.github/pull_request_template.md).
- [demos](demos/README.md): the four Screen A lifecycle walkthroughs (basic design, database design, detailed design, implementation), each with a transcript and a presentation deck (PDF). The video files are kept outside git.

English is the default for new project artifacts and the source of truth. The Markdown under `docs/en/` is English only; each document is also published as an English PDF under `docs/en/pdf/` and a Japanese PDF under `docs/ja/pdf/`, regenerated with `scripts/docs-pdf.py` whenever the document changes. Documents that predate this rule get their PDFs the next time they are edited ([RFC 0008](ai/improvements/0008-english-and-japanese-pdfs.md)).
