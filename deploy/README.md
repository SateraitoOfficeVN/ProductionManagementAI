# deploy

Local Docker Compose environment: PostgreSQL 17 + backend + frontend (Nginx reverse proxy, same-origin per ADR-0002).

Setup: copy `.env.example` to `.env` and fill in real local values (never commit `.env`).

Run: `docker compose -f compose.yaml up -d --build`
Validate config without building: `docker compose -f compose.yaml config`

Registry/deploy host beyond local Compose remain open decisions (see `work-items/WI-001/decisions.md` DEC-012) — not needed to run this locally.
