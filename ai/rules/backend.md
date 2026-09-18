# backend rules

- Target .NET 10 with EF Core (Npgsql) in the layered Domain/Application/Infrastructure/Api structure from [project context](../project.md) and ADR-0001. Introducing another ORM, data-access style or architectural pattern (minimal APIs, CQRS/MediatR, …) needs a new project decision.
- Implement the agreed API contract, validation and error behavior.
- Keep business rules testable; use the agreed transaction and concurrency behavior.
- Read configuration through the selected runtime mechanism; never commit credentials.
- Enable nullable reference types and treat the warnings as real; do not suppress them to silence the compiler.
- Return API errors as RFC 9457 Problem Details; never leak stack traces or internal exception detail in a response.
- Use structured logging, not string-concatenated messages; never log secrets, tokens or full request/response bodies containing personal data.
- Instrument new endpoints and background jobs with OpenTelemetry traces and metrics, not logs alone; missing instrumentation is incomplete work, not an optional extra, for a production backend.
