---
status: "accepted"
date: 2026-09-16
decision-makers: trannhatthanh31@gmail.com
consulted:
informed:
---

# Backend layered structure (Domain/Application/Infrastructure/Api) with EF Core

## Context and Problem Statement

The .NET 10 backend needs an internal project structure before any code is written. `work-items/WI-001/decisions.md` DEC-002 already fixed "EF Core, conventional layered structure" over minimal-APIs/CQRS/Dapper; this ADR records the concrete project boundaries that decision implies and the confirmation method, per `ai/skills/architecture/SKILL.md`.

## Decision Drivers

* Domain/Application logic (including auth logic) must be unit-testable with xUnit without booting the ASP.NET host.
* Persistence technology (EF Core, Npgsql) should be isolated so it doesn't leak into business logic.
* Explicit user decision: layered, not minimal-APIs, not CQRS/MediatR (DEC-002).
* Avoid unnecessary layers beyond what the decision requires, per `ai/rules/common.md` ("keep changes focused").

## Considered Options

* EF Core, 4-project layered structure (Domain/Application/Infrastructure/Api)
* Dapper, layered structure
* EF Core, minimal APIs (no separate Api project, no MVC controllers)
* Clean Architecture with CQRS/MediatR

## Decision Outcome

Chosen option: "EF Core, 4-project layered structure", because it directly implements DEC-002 and keeps Domain/Application testable in isolation while using the most conventional, best-documented .NET data-access story.

Concrete structure:

* `ProductionManagementAI.Domain` — entities, enums, no external dependencies.
* `ProductionManagementAI.Application` — service interfaces/implementations, DTOs; depends only on Domain; this is where auth/business logic that needs xUnit coverage lives.
* `ProductionManagementAI.Infrastructure` — EF Core `AppDbContext`, ASP.NET Core Identity stores, `Migrations/`; depends on Domain and Application (implements Application's interfaces).
* `ProductionManagementAI.Api` — Controllers, `Program.cs`, `appsettings.*.json`; wires DI, depends on all three.

### Consequences

* Good, because Application-layer logic (e.g. order status-transition rules, auth flows) can be unit-tested without an HTTP host.
* Good, because swapping or upgrading the persistence technology later only touches Infrastructure.
* Good, because it's the most common .NET layering pattern, minimizing onboarding friction for any reviewer or future agent.
* Bad, because 4 projects is more ceremony than a single-project/minimal-API approach for what is currently a small demo app.

### Confirmation

`dotnet build` succeeds across all four projects; `dotnet test` runs Domain/Application unit tests without requiring a running web host or database; Infrastructure and Api projects contain no business-rule logic that isn't covered by an Application-layer unit test.

## Pros and Cons of the Options

### EF Core, 4-project layered structure

* Good, because Domain/Application stay dependency-free/testable.
* Good, because EF Core has first-class Npgsql support and built-in migrations.
* Neutral, because it requires explicit project-reference wiring (`dotnet add reference`).
* Bad, because more boilerplate than a single project for a small app.

### Dapper, layered structure

* Good, because more control over generated SQL, often faster.
* Bad, because migrations are manual/hand-rolled; more boilerplate for CRUD than EF Core.
* Bad, because ASP.NET Core Identity's default stores assume EF Core; adopting Dapper would mean hand-building the auth data layer too, materially increasing WI-001's scope.

### EF Core, minimal APIs

* Good, because less ceremony than MVC controllers.
* Bad, because it conflicts with DEC-002's explicit "not minimal-APIs" choice.
* Bad, because `MapIdentityApi`, the minimal-API helper for Identity, defaults to bearer tokens, conflicting with the cookie-auth decision in 0002_ADR.

### Clean Architecture with CQRS/MediatR

* Good, because it scales well to complex domains with many use cases.
* Bad, because it conflicts with DEC-002's explicit "not CQRS/MediatR" choice.
* Bad, because it's disproportionate ceremony for a demo app with one screen so far.
