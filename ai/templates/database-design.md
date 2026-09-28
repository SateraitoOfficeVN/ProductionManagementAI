<!-- Table Definition Document (テーブル定義書) + ER diagram template, matching ai/templates/example/DatabaseDesign/テーブル定義_SOFMAPG5_EC(マスタ系・ログ系).xlsx: its 改版履歴 (revision history), テーブル一覧 (table list), インデックス一覧 (index list) and トリガー一覧 (trigger list) sheets, and one テーブル定義 (table definition) sheet per table, built from its テーブルテンプレート sheet. This project adds an ER diagram, constraints, database role and privileges, API/DD mapping and migration impact on top of the workbook's sheets.
The example targets SQL Server. Where a SQL Server-specific setting has no PostgreSQL 17 equivalent (clustered vs. non-clustered, PAD_INDEX, FILLFACTOR per index, file groups, SORT_IN_TEMPDB, …), leave it out; do not invent a counterpart. Carry over the ones that do map: index method, uniqueness, column order and sort direction, partial-index predicate, INCLUDE columns, and whether the index is built CONCURRENTLY. The workbook's "Cmd" column (a legacy framework's validation shorthand such as `NL(40)Eq`) maps to the "Validation" column below: it names the application-side rule, or the DD field whose validation covers the column.
Copy into docs/en/database/###/ as `{###_DB}_{画面名}.md` (RFC 0011); replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# {System Name} — {Subject} — Database Design Document (テーブル定義書)

{###_DB} — requirements {REQ-###, …}, basic design {###_BD}, implements {###_DD}.

{Physical naming convention in one paragraph: table/column case, key type and default, timestamp type and suffix, constraint name prefixes. Point to the DB document that fixed it (e.g. 000_DB) rather than restating it when one exists.}

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | {###_DB} |
| System name | {system name} |
| Work item | {WI-###} |
| Database | {database name}, schema {schema name} |
| DBMS | {PostgreSQL 17} |
| Character set | {UTF-8} |
| Created by | {name} |
| Created date | {YYYY-MM-DD} |
| Last updated by | {name} |
| Last updated date | {YYYY-MM-DD} |

One row per change. "Target" names the table (or "All" / "Index list" / "Trigger list") the change touched, like the workbook's 対象シート column.

| No | Target | Work item | Change | Date | Author |
| --- | --- | --- | --- | --- | --- |
| 1 | {table physical name} | {WI-###} | Initial creation | {YYYY-MM-DD} | {name} |

## Table list (テーブル一覧)

Add a row whenever a table is created, and keep it when the table is later dropped. Change its status instead. Link each logical name to its section under "Table definitions".

| No | Schema | Physical name | Logical name | Overview | Indexes beyond PK | Triggers | Created | Schema changed | Dropped | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | {schema} | `{table_name}` | [{logical name}](#{anchor}) | {what one row represents and what it stores} | {yes \| no} | {yes \| no} | {YYYY-MM-DD or "—" if not yet created} | {YYYY-MM-DD or "—"} | {YYYY-MM-DD or "—"} | {in use \| planned \| unused \| dropped} |

## ER diagram and relationships

{Mermaid `erDiagram` of the tables above and the tables they reference. Show PK/FK/UK markers on the key columns; list every column only if the diagram stays readable.}

```mermaid
erDiagram
    {parent_table} ||--o{ {child_table} : "{relationship}"
```

| Table | Related table | Relationship (1:1 / 1:N / N:N) | FK column |
| --- | --- | --- | --- |
| {table} | {related table} | {cardinality, and whether the child side is optional} | {table.column} |

## Table definitions (テーブル定義)

### {Logical name} (`{table_name}`)

| Field | Value |
| --- | --- |
| Logical name | {logical name, as in the UI/BD where it appears} |
| Physical name | `{table_name}` |
| Schema | {schema} |
| Overview | {what one row represents, who writes it (screen, API, batch, seed), and the source document for its columns, if any} |
| Estimated volume | {rows now / growth rate, or "small, bounded — {why}"} |
| Retention / deletion | {physical delete \| soft delete via {column} \| kept forever \| purged after {period}} |

Every column is listed in physical order. Put the project's standard audit columns ({e.g. `created_at_utc`, `updated_at_utc`}) at the end, as the workbook's template does.

| No | Item name | Physical name | Data type | Length | NOT NULL | Default | PK | UK | FK | Validation | Used | Description | Code values | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | {logical name} | `{column_name}` | {PostgreSQL type} | {length/precision, or "—"} | {yes \| no} | {default expression, or "none"} | {○ or blank} | {○ or blank} | {`table.column` or blank} | {DD field / app rule enforcing it, or "—"} | {○ if the application reads or writes it; blank if reserved or unused} | {what the value means} | {each allowed value and its meaning, e.g. `Draft`: 下書き (draft); or "—"} | {derivation, source system, open points} |

- **Usage patterns (パターン):** {When one table stores several kinds of record (e.g. per `type` value), a table whose columns are the patterns and whose rows are the columns, stating what each column holds for each pattern, or "—" if unused. Otherwise "Not applicable — one kind of record."}
- **CSV import/export (DL対象 / UL更新):** {For a table with a CSV download or upload, a table listing each column, whether it is exported, whether an upload updates it, and notes. Otherwise "Not applicable — no CSV import/export."}
- **Seed data:** {rows inserted by a migration or seeder, and whether they are illustrative or real master data, or "None."}

Repeat this subsection per table.

## Index definitions (インデックス一覧)

One row per index, including the primary key's. List a composite index's columns in key order, each with its sort direction.

| No | Table | Index name | Method | Unique | Column(s) and sort order | Partial predicate / INCLUDE | Built concurrently | Created | Dropped | Rationale (access pattern) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | `{table_name}` | `{pk_/ix_/ux_ name}` | {btree \| gin \| gist \| brin \| hash} | {yes \| no} | {`column` ASC, `column` DESC} | {`WHERE …` / `INCLUDE (…)`, or "—"} | {yes \| no — {why: new table, not yet live}} | {YYYY-MM-DD or "—"} | {YYYY-MM-DD or "—"} | {the actual query or constraint this index serves} |

## Trigger definitions (トリガー一覧)

{"None — no triggers." when the design has none, which is the default. Business logic belongs in the application layer.}

| No | Table | Trigger name | Timing / event | Function | Overview | Enabled | Created | Dropped | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | `{table_name}` | `{trigger_name}` | {BEFORE \| AFTER} {INSERT \| UPDATE \| DELETE} {FOR EACH ROW \| STATEMENT} | `{function_name}()` | {what it does and why it can't live in the application} | {enabled \| disabled} | {YYYY-MM-DD or "—"} | {YYYY-MM-DD or "—"} | {in use \| unused \| dropped} |

## Constraints

| Constraint | Type (PK/FK/UNIQUE/CHECK/EXCLUDE) | Table.column(s) | References | Rule |
| --- | --- | --- | --- | --- |
| {name} | {type} | {table.column(s)} | {table.column, if FK} | {on delete/update behavior, or the check expression} |

## Database role and privileges

| Role | Used by | Privileges on these tables | Reason |
| --- | --- | --- | --- |
| {application login} | {the API at runtime} | {SELECT, INSERT, UPDATE on `{table}`; USAGE on sequences, …} | {the queries that need each privilege; nothing broader} |
| {owner / migration login} | {migrations only} | {owner — DDL} | {} |

## API / DD mapping

| Table.column | DD field | API field |
| --- | --- | --- |
| {table.column} | {detailed-design.md field name} | {API request/response field name} |

## Migration impact and recovery limits

| Migration | Type | Tables touched | Live table? | Sequence |
| --- | --- | --- | --- | --- |
| {migration name} | {additive \| destructive \| data-transforming} | {tables} | {yes \| no} | {single step \| expand → backfill → switch → contract (which step this is)} |

- **Data recovery limit:** {what data, if any, cannot be recovered if this migration is reverted}
- **Rollback plan:** {how to reverse this migration, or "not reversible; requires restore from backup taken {when}"}

## Open decisions

| Decision | Options | Recommendation | Status |
| --- | --- | --- | --- |
| {schema question not yet settled} | {options considered} | {recommended option} | {open \| decided — see decisions.md} |
