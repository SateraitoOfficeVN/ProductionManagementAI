#!/bin/sh
# Runs once, on a fresh Postgres data volume (docker-entrypoint-initdb.d), before any EF migration.
# Creates the restricted runtime login used by the backend (DEC-016, 001_DB "Application database privileges").
# Table grants come from the AddProductionOrders migration, which runs as the owner (POSTGRES_USER).
# The password comes from PMAI_APP_DB_PASSWORD; there is no default and it is never written to the repository.
set -eu

: "${PMAI_APP_DB_PASSWORD:?PMAI_APP_DB_PASSWORD must be set}"

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB"   --set=app_password="$PMAI_APP_DB_PASSWORD" <<'SQL'
SELECT 'CREATE ROLE pmai_app' WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'pmai_app') \gexec
SELECT format('ALTER ROLE pmai_app LOGIN PASSWORD %L', :'app_password') \gexec
SQL
