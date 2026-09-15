# Project context

## Confirmed

- Demo: ProductionManagementAI; small manufacturing screens demonstrating the full AI development lifecycle.
- Frontend: Vite + TypeScript.
- Backend: .NET 10.
- Database: PostgreSQL.
- Source/PR/CI/CD: GitHub and GitHub Actions.
- Packaging/deployment: Docker.
- Shared harness: Markdown consumed by Claude and Codex.
- New project artifacts: English by default, optional Japanese translation.

## Open decisions

UI framework and component library; backend structure and ORM; PostgreSQL version; test frameworks and thresholds; authentication scope; repository/branch policy; registry and deployment host; merge/deploy permissions.

React is a proposal, not a confirmed choice. Do not select unresolved technologies silently when implementation depends on them.

## Current implementation

Documentation scaffold only. No application build/test/run commands, Docker runtime or CI pipeline exist.
Add verified commands here when the application is implemented; distinguish local prerequisites and CI commands.

## Candidate demo

Product catalog, production-order list and production-order create/edit. Proposed screen A is production-order create/edit; business fields and acceptance criteria still need agreement.
