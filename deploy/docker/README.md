# docker

- `backend.Dockerfile` — multi-stage .NET 10 build (SDK build stage, ASP.NET runtime stage), listens on `:8080`.
- `frontend.Dockerfile` — multi-stage Vite build (Node build stage, Nginx runtime stage), listens on `:80`.
- `migrate.Dockerfile` — builds an EF Core migrations bundle (SDK stage) and runs it on the ASP.NET runtime as the database owner; the `migrate` compose service (profile `tools`) uses it so migrations need no host .NET SDK.
- `frontend.nginx.conf` — serves the built frontend and reverse-proxies `/api/` to the `backend` service, so the browser only ever talks to one origin (0002_ADR: no CORS needed).

All Dockerfiles expect the repo root as build context (see `../compose.yaml`).
