# database rules

- Use PostgreSQL; pin its version when selected.
- Document tables, keys, nullability, constraints, relationships and justified indexes.
- Keep schema and API/DD mappings consistent.
- Describe migration impact and data recovery limits before execution; do not assume a destructive migration is reversible.
- Use parameterized queries or a parameterized query builder/ORM; never build SQL by concatenating input into a statement.
- Prefer backward-compatible, additive migrations on tables already in use: add the new shape, backfill, switch reads/writes, then drop the old shape in a later migration, rather than one breaking change.
- Add an index to an existing production table with `CREATE INDEX CONCURRENTLY` so the write path isn't blocked.
- Grant the application's database role only the privileges its queries need.
