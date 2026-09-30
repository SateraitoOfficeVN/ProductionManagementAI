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
