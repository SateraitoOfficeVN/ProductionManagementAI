---
name: database-design
description: Design PostgreSQL schema and migration intent from feature requirements.
---

# database-design

## 1. Purpose and usage scenario

Design the PostgreSQL schema, relationships and migration approach a feature needs. Use once the BD's data needs are known, in parallel with or right before detailed-design, and again whenever a feature changes an existing table already in use.

## 2. Mandatory inputs, optional inputs, and source reference order

**Mandatory:** BD, data requirements.
**Optional:** DD/API drafts, existing schema (when extending it).
**Source reference order:** the existing schema/`database-design.md` for affected tables (if any) → BD → [project context](../../project.md) → [policies](../../policies.md) → [applicable rules](../../rules/database.md) → the [template](../../templates/database-design.md).

## 3. Execution steps and applicable rules

1. Define entities, relationships, types, nullability, keys and constraints — per [database rules](../../rules/database.md).
2. Justify indexes against access patterns (adding one to a live table via `CREATE INDEX CONCURRENTLY`); identify transaction/concurrency needs and the least-privilege role the application uses.
3. Reconcile DB with DD/API and describe migration/data impact, preferring an expand/contract sequence over a single breaking change on a table already in use.

## 4. Required tools/scripts and environmental conditions

A PostgreSQL client and the project's chosen migration tool, once selected (open per [project context](../../project.md)); design-only work needs neither — no live database is required to produce the schema document.

## 5. Output artifacts, templates, ID conventions, and storage locations

Database design `###_DB_{画面名}.md` — `{画面名}` is the Japanese name of the screen it serves, as on its BD; a design that serves no screen uses an English kebab-case slug (RFC 0011) — in its number folder `docs/en/database/###/`, starting from the [template](../../templates/database-design.md). Document ID `###_DB`, using the same number as the BD and DD of the screen it serves (`000_DB` when it serves no screen), per the [documentation rules](../../rules/documentation.md). Table and column names follow the project's naming convention once fixed; until then, use `snake_case` physical names consistently within the document. Each document under `docs/en/` is also rendered to an English PDF under `docs/en/pdf/` and a Japanese PDF under `docs/ja/pdf/`, in the same change, per the [documentation rules](../../rules/documentation.md).

## 6. Checklist and repeatable verification method

Work through the [design-consistency checklist](../../checklists/design-consistency.md); repeat it for every revision that adds, drops or changes a constraint on a table already in use.

## 7. Termination criteria and failure handling

Done when relationships and constraints support the use cases, migration risks are explicit, and the design-consistency checklist passes. If a destructive change is requested without a defined recovery limit, apply the pause conditions in [policies](../../policies.md) rather than assuming the migration is reversible.

## 8. Work item update procedure and handover for the next step

Update `status.md` and `decisions.md` with any open schema decision. Hand the schema to `detailed-design` (persistence mapping), `implementation` (migration authoring) and `ci-cd` (migration/recovery behavior in the pipeline).
