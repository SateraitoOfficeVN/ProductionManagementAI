# backend

xUnit unit tests for the backend's Domain and Application layers, per 0001_ADR (these layers stay testable without an HTTP host or database).

- `ProductionManagementAI.Application.Tests` — covers `ProductionManagementAI.Application`.
- `ProductionManagementAI.Domain.Tests` — not created yet: `ProductionManagementAI.Domain` has no entities as of WI-001 (Screen A, WI-002, introduces the first ones per DEC-008). Add this project when Domain gains real logic to test.

Run: `dotnet test src/backend/ProductionManagementAI.slnx`
