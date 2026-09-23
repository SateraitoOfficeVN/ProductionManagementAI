# backend

.NET 10 backend, layered per `docs/en/architecture/0001/0001_ADR_backend-layered-structure.md`:

- `ProductionManagementAI.Domain` — entities, enums; no external dependencies.
- `ProductionManagementAI.Application` — service interfaces/implementations, DTOs; depends on Domain only.
- `ProductionManagementAI.Infrastructure` — EF Core `AppDbContext`, ASP.NET Core Identity stores, migrations; depends on Domain and Application.
- `ProductionManagementAI.Api` — controllers, `Program.cs`, `appsettings.*.json`; depends on all three.

Build: `dotnet build ProductionManagementAI.slnx`
Test: `dotnet test ProductionManagementAI.slnx`

## Running locally (Development)

Requires a running PostgreSQL 17 with the connection string in `ProductionManagementAI.Api/appsettings.Development.json` (`ConnectionStrings:DefaultConnection`), migrated (`dotnet ef database update --project ProductionManagementAI.Infrastructure --startup-project ProductionManagementAI.Api`).

The `SEED_ADMIN_PASSWORD` environment variable must be set before starting the API in Development — there is no hardcoded fallback, and startup fails immediately (before touching the database) if it's unset:

```
SEED_ADMIN_PASSWORD=<your-local-dev-password> dotnet run --project ProductionManagementAI.Api
```

This seeds the `Admin`/`Operator` roles and one `admin` user on every Development startup (idempotent — skips anything that already exists). Never write the actual password value into any committed file, including `work-items/WI-001/evidence.md`.
