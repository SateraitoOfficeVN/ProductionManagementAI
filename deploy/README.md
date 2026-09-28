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
