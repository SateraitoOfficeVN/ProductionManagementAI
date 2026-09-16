<!-- Table Definition Document (テーブル定義書) + ER diagram template, based on conventional Japanese SI database-design composition. Copy into the relevant work item or docs/en/database area; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# ProductionManagementAI — Identity Schema — Database Design Document (テーブル定義書)

DB-001 — supports auth/RBAC foundation (`work-items/WI-001/brief.md` INFRA-002), implements ADR-0002 (`docs/en/architecture/0002-auth-rbac-foundation.md`).

Physical naming convention: `snake_case`, per `ai/skills/database-design/SKILL.md` ("until [a convention is] fixed, use snake_case physical names consistently"). ASP.NET Core Identity's default PascalCase table names (`AspNetUsers`, etc.) are remapped to `snake_case` via EF Core Fluent API `ToTable(...)` calls in `AppDbContext`, and `EFCore.NamingConventions` (proposed in `work-items/WI-001/plan.md`, Auth foundation design) snake_cases column names consistently.

## Table list

| Table | Physical name | Purpose |
| --- | --- | --- |
| Users | `users` | Application user accounts (Identity `IdentityUser<Guid>` + app-specific fields) |
| Roles | `roles` | RBAC roles (Identity `IdentityRole<Guid>`); seeded `Admin`, `Operator` |
| UserRoles | `user_roles` | Many-to-many join between users and roles |
| UserClaims | `user_claims` | Identity default — per-user claims (unused by any feature yet, kept for extensibility) |
| RoleClaims | `role_claims` | Identity default — per-role claims (unused by any feature yet, kept for extensibility) |
| UserLogins | `user_logins` | Identity default — external login providers (unused; no external login in scope) |
| UserTokens | `user_tokens` | Identity default — per-user tokens, e.g. password-reset (unused by any feature yet, kept for extensibility) |

## ER diagram and relationships

| Table | Related table | Relationship (1:1 / 1:N / N:N) | FK column |
| --- | --- | --- | --- |
| `users` | `roles` | N:N (via `user_roles`) | `user_roles.user_id` / `user_roles.role_id` |
| `users` | `user_claims` | 1:N | `user_claims.user_id` |
| `users` | `user_logins` | 1:N | `user_logins.user_id` |
| `users` | `user_tokens` | 1:N | `user_tokens.user_id` |
| `roles` | `role_claims` | 1:N | `role_claims.role_id` |

## Table definitions

### `users`

| Item name | Physical name | Data type | Length | NOT NULL | Default | PK/FK | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Id | `id` | uuid | — | yes | `gen_random_uuid()` | PK | Identity `IdentityUser<Guid>` key |
| UserName | `user_name` | varchar | 256 | no | none | — | Identity default |
| NormalizedUserName | `normalized_user_name` | varchar | 256 | no | none | — | Uppercased for case-insensitive lookup; unique-indexed |
| Email | `email` | varchar | 256 | no | none | — | Identity default |
| NormalizedEmail | `normalized_email` | varchar | 256 | no | none | — | Uppercased for case-insensitive lookup |
| EmailConfirmed | `email_confirmed` | boolean | — | yes | `false` | — | Identity default; not used by any feature yet (no email-confirmation flow in scope) |
| PasswordHash | `password_hash` | text | — | no | none | — | Identity-managed hash; never read/written outside Identity APIs |
| SecurityStamp | `security_stamp` | text | — | no | none | — | Identity default; invalidated on credential change |
| ConcurrencyStamp | `concurrency_stamp` | text | — | no | none | — | Identity default; optimistic concurrency |
| PhoneNumber | `phone_number` | varchar | 32 | no | none | — | Identity default; unused |
| PhoneNumberConfirmed | `phone_number_confirmed` | boolean | — | yes | `false` | — | Identity default; unused |
| TwoFactorEnabled | `two_factor_enabled` | boolean | — | yes | `false` | — | Identity default; no 2FA flow in scope |
| LockoutEnd | `lockout_end` | timestamptz | — | no | none | — | Identity default; supports STRIDE lockout mitigation in ADR-0002 |
| LockoutEnabled | `lockout_enabled` | boolean | — | yes | `true` | — | Identity default |
| AccessFailedCount | `access_failed_count` | integer | — | yes | `0` | — | Identity default |
| DisplayName | `display_name` | varchar | 200 | yes | none | — | App-specific addition |
| IsActive | `is_active` | boolean | — | yes | `true` | — | App-specific addition; reserved for future account-disable use case, not yet consumed by any requirement |
| CreatedAtUtc | `created_at_utc` | timestamptz | — | yes | `now()` | — | App-specific addition |

### `roles`

| Item name | Physical name | Data type | Length | NOT NULL | Default | PK/FK | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Id | `id` | uuid | — | yes | `gen_random_uuid()` | PK | Identity `IdentityRole<Guid>` key |
| Name | `name` | varchar | 256 | no | none | — | `Admin` / `Operator`, seeded |
| NormalizedName | `normalized_name` | varchar | 256 | no | none | — | Uppercased; unique-indexed |
| ConcurrencyStamp | `concurrency_stamp` | text | — | no | none | — | Identity default |

### `user_roles`

| Item name | Physical name | Data type | Length | NOT NULL | Default | PK/FK | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| UserId | `user_id` | uuid | — | yes | none | PK, FK → `users.id` | Composite PK with `role_id` |
| RoleId | `role_id` | uuid | — | yes | none | PK, FK → `roles.id` | Composite PK with `user_id` |

### `user_claims`, `role_claims`, `user_logins`, `user_tokens`

Kept at ASP.NET Core Identity's standard EF Core column mapping (near-zero cost to retain, per the Auth foundation design section of `work-items/WI-001/plan.md`: "avoids a breaking schema change if external login/claims are needed later"). Not itemized column-by-column here since no column deviates from Identity's default shape; generated automatically by `IdentityDbContext<AppUser, AppRole, Guid, ...>` and reviewed at migration-generation time (`work-items/WI-001/plan.md` step 11).

## Index definitions

| Index name | Table | Column(s) | Type | Rationale (access pattern) |
| --- | --- | --- | --- | --- |
| `ix_users_normalized_user_name` | `users` | `normalized_user_name` | unique btree | Login looks up by normalized username; Identity requires uniqueness |
| `ix_users_normalized_email` | `users` | `normalized_email` | btree | Supports future email-based lookup; not unique (Identity's default `RequireUniqueEmail` is off unless explicitly enabled — left off since brief.md doesn't require email-based login) |
| `ix_roles_normalized_name` | `roles` | `normalized_name` | unique btree | Role lookup/seed-idempotency by name |
| `pk_user_roles` | `user_roles` | `user_id, role_id` | unique btree (PK) | Primary lookup path: "what roles does this user have" |

## Constraints

| Constraint | Type (PK/FK/UNIQUE/CHECK) | Table.column(s) | References | Rule |
| --- | --- | --- | --- | --- |
| `pk_users` | PK | `users.id` | — | — |
| `pk_roles` | PK | `roles.id` | — | — |
| `pk_user_roles` | PK | `user_roles.(user_id, role_id)` | — | — |
| `fk_user_roles_user_id` | FK | `user_roles.user_id` | `users.id` | `ON DELETE CASCADE` — removing a user removes their role assignments |
| `fk_user_roles_role_id` | FK | `user_roles.role_id` | `roles.id` | `ON DELETE CASCADE` — removing a role removes its assignments (role deletion itself is not exposed by any feature yet) |
| `uq_users_normalized_user_name` | UNIQUE | `users.normalized_user_name` | — | Enforced by `ix_users_normalized_user_name` |
| `uq_roles_normalized_name` | UNIQUE | `roles.normalized_name` | — | Enforced by `ix_roles_normalized_name` |

## API / DD mapping

| Table.column | DD field | API field |
| --- | --- | --- |
| `users.id` | not yet written (WI-001 has no DD; auth endpoints are specified directly in ADR-0002) | `GET /api/auth/me` → `id` |
| `users.user_name` | — | `GET /api/auth/me` → `userName`; `POST /api/auth/login` request → `userName` |
| `users.display_name` | — | `GET /api/auth/me` → `displayName` |
| `roles.name` (via `user_roles`) | — | `GET /api/auth/me` → `roles: string[]` |

## Migration impact and recovery limits

- **Migration type:** additive — this is the first migration in the project; no existing data to preserve or transform.
- **Data recovery limit:** not applicable — no pre-existing rows in any of these tables.
- **Rollback plan:** `dotnet ef database update <previous-migration-or-0>` drops these tables cleanly since nothing else references them yet; safe to roll back at this stage.

## Open decisions

| Decision | Options | Recommendation | Status |
| --- | --- | --- | --- |
| Whether an additional CSRF token (beyond `SameSite` cookie attribute) is needed for state-changing endpoints | `SameSite=Lax` only vs. `SameSite=Lax` + anti-forgery token | Deferred to Screen A's detailed-design (`work-items/WI-002/brief.md` scope), since it affects the order-mutation endpoints more concretely than the login endpoint itself | open — see ADR-0002 Confirmation section |
| Whether `email_confirmed`/`two_factor_enabled`/`phone_number*` columns are ever exercised by a future requirement | keep unused vs. trim from the model | Keep (near-zero cost, avoids a later breaking migration) — already decided, see `users` table Notes | decided — see `work-items/WI-001/decisions.md` (auth foundation design) |
