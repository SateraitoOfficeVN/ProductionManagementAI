# Postgres 17 plus this project's first-boot init scripts (DEC-016). Baked in rather than bind-mounted so it works
# without host file sharing (e.g. Docker Desktop paths outside the shared list, CI runners).
FROM postgres:17
COPY deploy/db/init/ /docker-entrypoint-initdb.d/
