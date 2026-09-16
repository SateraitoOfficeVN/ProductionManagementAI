# backend

.NET 10 backend, layered per `docs/en/architecture/0001-backend-layered-structure.md`:

- `ProductionManagementAI.Domain` — entities, enums; no external dependencies.
- `ProductionManagementAI.Application` — service interfaces/implementations, DTOs; depends on Domain only.
- `ProductionManagementAI.Infrastructure` — EF Core `AppDbContext`, ASP.NET Core Identity stores, migrations; depends on Domain and Application.
- `ProductionManagementAI.Api` — controllers, `Program.cs`, `appsettings.*.json`; depends on all three.

Build: `dotnet build ProductionManagementAI.slnx`
