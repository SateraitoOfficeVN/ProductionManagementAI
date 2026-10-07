# deploy

Local Docker Compose environment: PostgreSQL 17 + backend + frontend (Nginx reverse proxy, same-origin per 0002_ADR).

Setting up a new computer from scratch: open [local-setup.html](local-setup.html) in a browser. It is a step-by-step guide in English, Japanese and Vietnamese that also works offline.

Setup: copy `.env.example` to `.env` and fill in real local values (never commit `.env`). `PMAI_APP_DB_PASSWORD` and `SEED_ADMIN_PASSWORD` are required and have no default. `SEED_ADMIN_PASSWORD` must pass ASP.NET Core Identity's default password rules (at least 6 characters with an uppercase letter, a lowercase letter, a digit and a symbol), or the backend stops at startup.

## Database logins (DEC-016)

| Login | Used by | Rights |
| --- | --- | --- |
| `POSTGRES_USER` (owner) | Migrations only (`dotnet ef database update`) | Owns the schema, runs DDL |
| `pmai_app` | The backend at runtime | Only the table grants listed in `docs/en/database/001/001_DB_製造指示登録・編集.md`; no DDL, no DELETE on orders |

`db/init/10-app-login.sh` (copied into the database image by `docker/db.Dockerfile`, so no host file sharing is needed) creates `pmai_app` with `PMAI_APP_DB_PASSWORD` the first time the `db-data` volume is created. The `AddProductionOrders` migration grants its table rights. If the volume already existed before this change, or you change either password, wipe the volume rather than syncing the password by hand: `docker compose -f compose.yaml down -v`.

## Run

Double-click `start-app.cmd` in the repo root (it also activates the local plant calendar if not yet active and opens the browser), or run one command from the repo root, with only Docker installed: `powershell -ExecutionPolicy Bypass -File scripts\start-app.ps1`. It creates `.env` if missing, runs the steps below entirely in Docker (migrations in the one-shot `migrate` container instead of a host `dotnet ef`) and waits until the app answers on `http://localhost:3000`. Add `-ActivateCalendar` to also activate the plant calendar on this local database. The `migrate` service is in the `tools` profile, so `docker compose up` never starts it; run it alone with `docker compose -f compose.yaml run --rm --build migrate`. Manual steps:

1. `docker compose -f compose.yaml up -d --build db`
2. Apply migrations as the owner, from the repo root (replace the password with your `POSTGRES_PASSWORD`):
   `dotnet ef database update --project src/backend/ProductionManagementAI.Infrastructure --startup-project src/backend/ProductionManagementAI.Api --connection "Host=localhost;Port=5433;Database=production_management_ai;Username=postgres;Password=<POSTGRES_PASSWORD>"`
3. `docker compose -f compose.yaml up -d --build`

Validate config without building: `docker compose -f compose.yaml config`

Running the backend outside Docker (`dotnet run`): `appsettings.Development.json` connects as `pmai_app` without a password, so set the `PGPASSWORD` environment variable to your `PMAI_APP_DB_PASSWORD` first (Npgsql reads it).

Registry/deploy host beyond local Compose remain open decisions (see `work-items/WI-001/decisions.md` DEC-012) — not needed to run this locally.


## WI-006 Product master cutover

The WI-006 migrations were rehearsed only against disposable databases. A live or mutable demo cutover needs separate authorization, a verified backup, and a coordinated write pause. Stop old application writers, migrate as the owner in the order `AddProductMasterFields` then `ConvertOrderQuantityToNumeric`, validate, and start the new API/UI together. Do not overlap old and new binaries.

Preflight checks reject duplicate case-folded SKUs, blank/untrimmed product fields, unexpected seed identities, unknown products without reviewed units, and out-of-range order quantities. The reviewed unit map applies to exactly the 30 original seed IDs; an unreviewed product is never assigned a guessed unit. Preserve edited names, product IDs, and all order references.

The unique SKU index is built concurrently outside the migration transaction. If the build fails, inspect `pg_index.indisvalid`, the migration history, and the already applied schema changes before retrying. An invalid index blocks the migration; an owner must repair/drop that invalid index and reconcile partially applied schema with the approved DB recovery procedure. Never assume `IF NOT EXISTS` repairs an invalid index. Both WI-006 `Down` methods refuse automatic rollback; recover from a verified backup or a reviewed forward fix.

After migration, verify 30 reviewed seed units, unchanged historical quantities/IDs/FKs, a valid unique `ux_products_sku_lower`, and `numeric` range/scale checks. The runtime login has product SELECT/INSERT and UPDATE only for mutable fields; SKU updates, product DELETE and DDL remain denied. Product master writes require Admin or Operator.


## WI-009 Production lines migration and recovery

WI-009 is local implementation work. Its migrations run as the owner only on
isolated rehearsal databases; no live/demo cutover is authorized. A later cutover
requires an approved write pause, a verified restorable backup and coordinated
API/UI rollout. Stop all old writers before applying the two stages:
`20261001042810_ExpandProductionLines`, then
`20261001042938_IndexProductionLineAssignments`. Keep the original product IDs,
order quantities, statuses and historical references. Existing orders retain null
line assignments; no line/pair seed or historical timing backfill is introduced.

The first stage adds line/pair tables, generated product unit revision, nullable
order line, restricted column grants and unvalidated legacy-table constraints.
The second builds the partial `(line_id, product_id)` order index **concurrently**
outside the migration transaction, then validates the constraints. It uses
5-second lock and 10-second statement budgets. Inspect migration history,
`pg_index.indisvalid/indisready`, `pg_get_indexdef` and constraint validation state
before declaring success. An existing invalid, unready or differently defined
`ix_orders_line_product` aborts the stage; `IF NOT EXISTS` does not repair it.

After an interrupted build, keep writers paused and have the owner review the
actual catalog and partial migration history. On an authorized isolated rehearsal,
drop only the reviewed invalid index concurrently, rerun the second stage, then
confirm a valid/ready index with the approved definition and validated FK/checks.
Do not rerun the expand stage blindly or remove either master table. Both `Down`
methods refuse destructive rollback; recovery requires a reviewed forward fix
or separately authorized restore from the verified backup.

Runtime `pmai_app` has SELECT/INSERT on the new tables, UPDATE only for line
name/hours/state/audit and pair timing/confirmation/state/audit. It cannot change
line codes or pair keys, delete masters, run DDL or directly set product revisions.
The invoker trigger increments revision only on actual product unit changes,
including A→B→A. Verify these grants and trigger behavior as part of cutover.

Feature writes have a linked 15-second use-case deadline, transaction-local
5-second lock / 10-second statement budgets and fresh bounded 2-second cleanup.
`LINE_BUSY` (503, Retry-After 1) is safe for explicit retry only after precommit
provider failure and confirmed rollback. Commit acknowledgement loss or failed
postcommit refresh remains unknown: verify through reads and never replay the
mutation automatically. Order assignment keeps the existing order transaction;
no additional transaction or scheduling reservation is created.

The WI-009 test topology is `pmai-wi009-isolated`, database `pmai_wi009_test`,
host ports 5499/8099/3099. Its random credentials live in a temporary environment
file outside git. Never use this topology's cleanup command against an existing
application project; inspect the exact Compose project labels before removing
only its disposable containers/volumes.


## WI-010 Plant calendar activation and recovery

WI-010 rehearsal uses disposable fixtures only. Live/demo migration or activation
requires separate authorization, a verified restorable backup, a write pause and
coordinated API/UI rollout. Record the actual target database/owner, migration
history, configured Plant:TimeZone and chosen explicit plant-local activation date.
Stop old writers before owner migration20261002030411_ExpandPlantCalendar.
The additive migration creates three new tables/indexes and column grants only;
it does not seed/activate the calendar, rewrite old tables or backfill order data.
DDL uses transaction-local5s lock/15s statement budgets. Inspect the catalog and
migration history after interruption before any retry; never guess partial state.

Unactivated reads return unavailable coverage/nullable context version; writes are
rejected. There is no startup/API/read activation. After verifying schema/grants and
the plant date/timezone, run scripts/calendar/activate.ps1 as the owner using
explicit PGHOST, PGPORT, PGDATABASE and PGUSER plus credentials in PGPASSWORD or a
protected .pgpass; never commit/print credentials. Example from the repository root:

```powershell
./scripts/calendar/activate.ps1 -ActivationDate '2026-10-02' -TimeZone 'Asia/Tokyo'
```

Those values are an example, not an automatic production date choice. Use the
exact timezone configured in every app instance; changing it later causes calendar
503 TIMEZONE_MISMATCH. The wrapper validates explicit dates and target variables;
activate.sql validates date/timezone/owner inside a bounded transaction, then locks
the singleton table and inserts state revision1 plus initial Mon–Fri mask31 as one
commit. An existing activation always refuses reset. After any uncertainty inspect
state and initial weekly record through a separate owner connection; do not blindly
replay. Verify activated_on/time_zone_id/revision and matching initial current head.

Runtime pmai_app can SELECT the three tables, UPDATE only state revision/audit,
INSERT revision snapshots and UPDATE only is_current. Activation/date/timezone
changes, payload UPDATE, DELETE and DDL are denied. Old snapshots/markers remain;
weekly/date mutations validate exact latest ID and global opaque version. Existing
order starts, due dates and dashboard windows remain calendar-independent.

Calendar operations use a20s request deadline,15s command/statement,5s lock and
at most5s cleanup budget; client deadline25s. Known rollback BUSY permits explicit
retry after Retry-After1. Lost commit/response confirmation is Unknown: preserve
input, inspect current/history and explicitly discard/accept a fresh baseline;
observed equality never proves the earlier client committed. No automated replay.

Down refuses after activation. Prefer an approved forward repair. A coordinated
backup restore must restore state and all revisions together, maintain old product/
line/order integrity and reset all client drafts/snapshot tokens; never reset only
the aggregate version, delete markers or combine versions from different backups.
Rehearse restore and validate constraints/grants before resuming writers. No live
cutover, deployment or externally exported telemetry is claimed by local tests.

The WI010 test project is pmai-wi010-check-20261002, database wi010_fixture,
ports54410/18110/30110, generated credentials outside git. Verify exact Docker
Compose labels before removing only these task-owned containers/volumes. Preserve
all existing demo/evidence stacks and final videos.

CI E2E explicitly activates its newly migrated disposable calendar as the owner
before API startup, using activate.sql with the current date in its fixed plant
timezone. This fixture step is separate from production deployment; startup and
migrations remain unactivated, and live activation needs separate authorization.
