---
status: "accepted"
date: 2026-09-16
decision-makers: trannhatthanh31@gmail.com
consulted:
informed:
---

# Authentication and RBAC foundation: ASP.NET Core Identity + cookie session

## Context and Problem Statement

`work-items/WI-001/decisions.md` DEC-006/DEC-007 committed to full authentication + role-based authorization, built as foundational bootstrap work before Screen A. This ADR records the concrete mechanism and threat-models the resulting login/session trust boundary, per `ai/skills/architecture/SKILL.md` step 1 ("for a boundary handling authentication ... threat-model it before finalizing the decision") and the `ai/checklists/security-review.md` gate.

## Decision Drivers

* Real auth foundation needed now, not a stub (DEC-006).
* Must not conflict with DEC-002 ("not minimal-APIs") — rules out `MapIdentityApi`, which is minimal-API-shaped and defaults to bearer tokens.
* Avoid storing a bearer token in browser-JS-reachable storage (XSS exposure).
* Avoid CORS complexity if a same-origin approach is achievable instead.
* Reuse a well-reviewed, batteries-included auth library rather than hand-rolling password hashing/lockout for a security-relevant boundary.

## Considered Options

* ASP.NET Core Identity (EF Core store) + cookie-based authentication, custom `AuthController`
* JWT bearer tokens stored in `localStorage`, custom `AuthController`
* `MapIdentityApi<TUser>()` (built-in minimal-API Identity endpoints, bearer-token default)

## Decision Outcome

Chosen option: "ASP.NET Core Identity + cookie-based authentication, custom `AuthController`", because it avoids XSS-exposed token storage, avoids CORS entirely via a same-origin dev proxy/reverse-proxy pattern, reuses Identity's reviewed password/lockout handling, and stays consistent with DEC-002's "not minimal-APIs" decision (unlike `MapIdentityApi`).

Concrete shape:

* `AppUser : IdentityUser<Guid>` (+ `DisplayName`, `IsActive`, `CreatedAtUtc`), `AppRole : IdentityRole<Guid>`, using EF Core Identity stores (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, and the rest of the default Identity schema kept for future extensibility).
* `AuthController` with `POST /api/auth/login`, `GET /api/auth/me`, `POST /api/auth/logout`, built on `UserManager`/`SignInManager` directly (not `MapIdentityApi`).
* Global fallback authorization policy = authenticated user required; `[AllowAnonymous]` only on login; an `AdminOnly` policy (`RequireRole("Admin")`) defined and ready for future endpoints.
* Frontend and backend kept same-origin via a Vite dev-server proxy (`/api` → backend) locally, and an Nginx reverse proxy in the container image — so no CORS configuration is needed.
* Seed data (Development only): roles `Admin`, `Operator` (placeholder, not a business decision — `work-items/WI-001/decisions.md` DEC-006 flags this), one seed admin user with password from `SEED_ADMIN_PASSWORD` env var, no hardcoded fallback.

### Consequences

* Good, because no bearer token is ever placed in JS-reachable storage.
* Good, because no CORS configuration is needed, removing a class of misconfiguration risk.
* Good, because Identity supplies reviewed password hashing, lockout and concurrency handling instead of hand-rolled equivalents.
* Neutral, because cookie auth requires the same-origin proxy pattern to be correctly configured in both local dev and the container image — a real but bounded setup cost.
* Bad, because cookie-based auth needs explicit CSRF consideration (see Confirmation) that a stateless bearer-token API would not need in the same way.

### Confirmation

* Integration test: unauthenticated request to a protected endpoint returns 401; authenticated request to `/api/auth/me` returns 200 with the session's user info; seeded admin can log in and log out successfully.
* CSRF: since the cookie is same-origin and state-changing requests go through the frontend's own fetch wrapper, the cookie is issued with `SameSite=Lax` (or stricter) as the baseline mitigation; whether an additional anti-forgery token is needed for the login/order-mutation endpoints was deferred to `detailed-design` for SCR-001 — resolved: no additional token; `SameSite=Lax` + no CORS policy + JSON-only request bodies on mutating endpoints is sufficient for a single-origin deployment (`work-items/WI-002/decisions.md` DEC-020, 2026-09-18; revisit if a CORS policy is added or the app shares a site with other origins).
* Seed admin password: code review confirms startup fails in Development if `SEED_ADMIN_PASSWORD` is unset, and that the password is never written to logs or `evidence.md`.

### STRIDE review of the login/session boundary

| Threat | Mitigation |
| --- | --- |
| Spoofing (impersonating a user) | Identity password hashing (PBKDF2/Argon2-class, library default) plus account lockout after repeated failed attempts; seed admin password is env-var only, never hardcoded |
| Tampering (forging/altering the session) | ASP.NET Core Data Protection signs/encrypts the auth cookie; `DATA_PROTECTION_KEYS_PATH` persists keys across container restarts so cookies aren't silently invalidated in a way that would push toward a weaker workaround |
| Repudiation (denying an action was taken) | Out of scope for this ADR's confirmation — no audit-log requirement was raised in `brief.md`; flagged as a future gap if audit trails become a requirement |
| Information disclosure (leaking account/credential info) | Login failure responses are generic ("invalid credentials"), not field-specific ("wrong password" vs "unknown user"); no stack traces in error responses, per `ai/checklists/security-review.md` |
| Denial of service (via the auth mechanism itself) | Identity's lockout policy could itself be abused to lock out a legitimate user by repeated bad attempts; accepted as a known MVP-scope risk, not mitigated further here |
| Elevation of privilege (bypassing the role check) | Role checks (`AdminOnly` policy, global authenticated-user fallback policy) are enforced server-side via ASP.NET Core `[Authorize]`, not inferred from any client-supplied claim; the frontend's `ProtectedRoute` is a UX convenience only, never the authority |

## Pros and Cons of the Options

### ASP.NET Core Identity + cookie-based authentication

* Good, because it reuses reviewed, maintained password/lockout handling.
* Good, because it avoids XSS-exposed token storage and CORS entirely.
* Neutral, because it needs the same-origin proxy pattern set up correctly in both dev and container environments.
* Bad, because cookie auth needs explicit CSRF consideration that a pure bearer-token API wouldn't.

### JWT bearer tokens in `localStorage`

* Good, because it's stateless and simple to reason about across origins.
* Bad, because a token in `localStorage` is readable by any script on the page — any XSS becomes full account takeover.
* Bad, because it would need CORS configured correctly if frontend/backend aren't proxied to the same origin anyway, undermining the main benefit.

### `MapIdentityApi<TUser>()`

* Good, because it's the least code to write — Identity's built-in minimal-API endpoints.
* Bad, because it's minimal-API-shaped, conflicting with DEC-002's "not minimal-APIs" decision.
* Bad, because its default response shape uses bearer tokens, reintroducing the JWT-in-storage tradeoff above unless heavily customized — at which point little is saved over a custom controller.
