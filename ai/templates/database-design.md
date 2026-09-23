<!-- Table Definition Document (テーブル定義書) + ER diagram template, based on conventional Japanese SI database-design composition. Copy into the relevant work item or docs/en/database area; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# {System Name} — Database Design Document (テーブル定義書)

{###_DB} — requirements {REQ-###, …}, implements {###_DD}.

## Table list

| Table | Physical name | Purpose |
| --- | --- | --- |
| {logical name} | {physical table name} | {what it stores} |

## ER diagram and relationships

{Link an ERD diagram artifact here if one exists.}

| Table | Related table | Relationship (1:1 / 1:N / N:N) | FK column |
| --- | --- | --- | --- |
| {table} | {related table} | {cardinality} | {column} |

## Table definitions

### `{table_name}`

| Item name | Physical name | Data type | Length | NOT NULL | Default | PK/FK | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| {logical name} | {physical column name} | {type} | {length/precision} | {yes \| no} | {default or "none"} | {PK \| FK \| —} | {notes} |

Repeat this subsection per table.

## Index definitions

| Index name | Table | Column(s) | Type | Rationale (access pattern) |
| --- | --- | --- | --- | --- |
| {name} | {table} | {column(s)} | {unique \| btree \| …} | {the actual query this index serves} |

## Constraints

| Constraint | Type (PK/FK/UNIQUE/CHECK) | Table.column(s) | References | Rule |
| --- | --- | --- | --- | --- |
| {name} | {type} | {table.column(s)} | {table.column, if FK} | {on delete/update behavior or check expression} |

## API / DD mapping

| Table.column | DD field | API field |
| --- | --- | --- |
| {table.column} | {detailed-design.md field name} | {API request/response field name} |

## Migration impact and recovery limits

- **Migration type:** {additive \| destructive \| data-transforming}
- **Data recovery limit:** {what data, if any, cannot be recovered if this migration is reverted}
- **Rollback plan:** {how to reverse this migration, or "not reversible — requires restore from backup taken {when}"}

## Open decisions

| Decision | Options | Recommendation | Status |
| --- | --- | --- | --- |
| {schema question not yet settled} | {options considered} | {recommended option} | {open \| decided — see decisions.md} |
