<!-- Product Requirements Document (PRD) template, based on common PRD conventions (Atlassian/Confluence, Aha, monday.com). Copy into the relevant work item; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# Project Bootstrap (skeleton + auth foundation) — Product Brief

## Status

| Work item | Author | Status | Target release |
| --- | --- | --- | --- |
| WI-001 | Claude (this session), reviewed by trannhatthanh31@gmail.com | approved | unscheduled — precedes the first feature (Screen A) |

## Overview

`ai/` fully describes a Claude/Codex development process, but the repository has no `src/`, no `tests/`, no CI and no Docker yet, and every technology choice in `ai/project.md`'s "Open decisions" was unresolved. This work item stands up the first real application skeleton — a Vite+React+TypeScript frontend and a .NET 10 (EF Core, layered) backend against PostgreSQL 17 — with a working authentication + role-based-authorization foundation, so the first feature (Screen A) can be built on top of it instead of starting from nothing.

## Objective

Prove the harness end-to-end on real infrastructure work before committing it to a business feature, and give every subsequent work item (Screen A/B/C) a working, tested, locally-runnable base to build on. This is the "Giai đoạn 1 — Harness nền tảng" stage described in `docs/vi/000-mo-ta-harness-va-quy-trinh-phat-trien-ai.md` §18.

## Success metrics

| Goal | Metric | Target |
| --- | --- | --- |
| Skeleton builds and runs locally | `dotnet build`, `npm run build`, `docker compose up` | All succeed with recorded output |
| Auth foundation works end-to-end | Login → session → protected route/endpoint → logout | Verified by integration test + manual check |
| Harness decisions captured, not just chat | `ai/project.md` and `work-items/WI-001/*` reflect this session's decisions | Diff review against this session |

## Assumptions

- Local toolchain (.NET 10 SDK, Node/npm, Docker) is available in the environment the commands are run in; this is verified at step 1 (preflight) rather than assumed.
- The exact role/permission matrix beyond a placeholder `Admin`/`Operator` seed is not yet a business decision — it will be confirmed before Screen A relies on any specific permission.
- Registry/deploy host, Japanese-translation-sync policy, and how the four demo videos are produced remain open and are not blocked by this work item.

## Actors and user stories

Not applicable in the usual end-user sense — this work item is technical infrastructure with no business-facing screen. The "actor" is whoever builds the next feature (developer or AI agent) on top of this skeleton.

| Actor | As a… | I want to… | So that… | Use case ID |
| --- | --- | --- | --- | --- |
| Feature implementer (human or AI, next work item) | person picking up Screen A | start from a working, authenticated, tested app skeleton | I don't have to solve auth/build/CI/DB wiring while also building the first business screen | N/A — infrastructure, not a business use case |

## Requirements (in scope)

Framed as infrastructure requirements rather than business `REQ-###`s, since no business behavior is being delivered yet:

| ID | Requirement | Acceptance criteria | Priority |
| --- | --- | --- | --- |
| INFRA-001 | Frontend and backend skeletons build and run locally per the confirmed stack (Vite+React+TS+Tailwind; .NET 10+EF Core, layered) | `npm run build` and `dotnet build` succeed; app boots via `docker compose up` | must |
| INFRA-002 | A real login → session → logout flow exists, backed by a user/role data model | Integration test: seeded admin can log in, `/api/auth/me` reflects session state, protected route/endpoint rejects unauthenticated access | must |
| INFRA-003 | CI skeleton (build+lint+test) exists for both frontend and backend | `.github/workflows/ci.yml` present and reviewed; cannot be verified green since push isn't authorized in this work item | must |
| INFRA-004 | `ai/project.md` reflects the real, verified stack and commands | Diff shows Open decisions resolved into Confirmed, with actual verified commands recorded | must |

## Not doing (out of scope)

- Any business screen (production-order create/edit = Screen A, production-order list = Screen B, dashboard = Screen C) — those are separate work items after this one is done.
- Deciding the real permission matrix beyond the placeholder `Admin`/`Operator` seed roles.
- Any push, PR, merge, CI execution, image publish or deployment — not authorized by `ai/policies.md`'s "External operations" section for this scaffold.
- Choosing the container registry/deploy host, the Japanese-translation-sync policy, or how the four demo videos get produced.

## Open questions

| Question | Impact if unresolved | Owner | Status |
| --- | --- | --- | --- |
| What is the real role/permission matrix beyond placeholder `Admin`/`Operator`? | Screen A can't assume any specific permission gating until this is confirmed | Business owner (user) | open |
| Container registry / deploy host? | Blocks CD setup and any real deployment | Business owner (user) | open |
| Japanese-translation-sync policy? | Blocks any bilingual doc maintenance process | Business owner (user) | open |
| How are the four demo videos produced (tool, Claude/Codex split)? | Blocks recording the harness's own proof-of-process artifacts | Business owner (user) | open |
