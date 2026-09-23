# Production Order Create/Edit (Screen A) — Test Plan

## Test plan identifier

TP-002, work item WI-002, revision 1, 2026-09-18.

## References

- `brief.md` revision 1 (REQ-010–REQ-019), `decisions.md` DEC-001–DEC-029
- 001_BD revision 4, 001_DB, 001_DD revision 3, 001_DD-API, 001_DD-FN revision 2, 001_DD-SPD
- `plan.md` revision 2, step 12

## Introduction

Covers Screen A (SCR-001) end to end: domain rules, service orchestration, the HTTP API over real PostgreSQL (running as the restricted runtime login), the React screen, and browser journeys against the Docker Compose stack. Most coverage is at unit level, less at integration, and least at E2E (`ai/rules/testing.md`).

## Test items

| Requirement ID | Description |
| --- | --- |
| REQ-010 | Create a production order |
| REQ-011 | Edit an existing production order (incl. stale-save rejection, DEC-010) |
| REQ-012 | Only authenticated Admin/Operator |
| REQ-013 | Quantity: positive whole number (≤ 999,999,999, DEC-024) |
| REQ-014 | Due date today or later (on create / when changed, DEC-009; plant timezone, DEC-017) |
| REQ-015 | Product must exist |
| REQ-016 | Notes ≤ 500 characters (code points) |
| REQ-017 | Fixed status state machine |
| REQ-018 | Product/quantity locked after Draft (whole-request rejection, DEC-007) |
| REQ-019 | Confirm before discarding changes on Cancel |

## Features to be tested

All REQs above, plus these design decisions with observable behavior: order numbering (DEC-012/013), plant timezone (DEC-017), JSON-only bodies (DEC-020), error contract (DEC-023), least-privilege runtime login (DEC-016), telemetry (001_DD Observability), and accessibility (DEC-026). Cases TC-001–TC-027 below.

## Features not to be tested

- Screens B and C (out of scope).
- Browsers other than Chromium, and real mobile devices: E2E uses Playwright Chromium (desktop + Pixel 7 emulation). Recorded as a known gap.
- A manual screen-reader pass: axe covers the automated WCAG 2.2 AA rules only. Known gap.
- OTLP export to a real collector: the tests verify spans/counters in-process; the exporter is only configured when an endpoint is set.

## Approach

| Level (unit / integration / system / E2E / smoke) | Included? | Rationale |
| --- | --- | --- |
| Unit — backend (xUnit) | yes | Domain invariants and service check order without a host (0001_ADR) |
| Unit — frontend (Vitest + RTL + vitest-axe) | yes | Field behavior, states, error mapping, dialog, a11y rules in jsdom |
| Integration (xUnit + WebApplicationFactory + Testcontainers Postgres 17) | yes | Real HTTP pipeline, auth, migrations, constraints, concurrency, privileges |
| E2E (Playwright Chromium + @axe-core/playwright) | yes | Browser journeys and real-browser a11y (incl. contrast) against the Compose stack |
| Smoke | yes | Compose stack `/health`, frontend 200, unauthenticated 401 via the proxy |
| System (other) | no | E2E against the full Compose stack already is the system-level check |

## Item pass/fail criteria

A case passes when every test implementing it passes, and the observable behavior matches the brief's acceptance criteria and the DD, with no unhandled error. A requirement passes when all its cases pass.

## Suspension criteria and resumption requirements

Suspend if Docker is unavailable (integration/E2E can't run) or a blocking defect stops a journey. Resume once Docker is running, or once the defect is fixed and re-verified. A suspended case is recorded as not run, never as passed.

## Test deliverables

- This file; test code under `tests/backend/.../ProductionOrders/`, `tests/integration/.../ProductionOrders/`, `src/frontend/tests/unit/production-orders/`, `tests/e2e/specs/`
- Execution results in `evidence.md`

## Cases

Test names are the actual test methods / titles (U = backend unit, I = integration, F = frontend unit, E = E2E).

| Test ID | Requirement ID | Precondition / setup | Steps | Expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| TC-001 | REQ-010 | Signed in; products seeded | Create a valid order. U `Create_Valid_ReturnsDraftWithIssuedSequenceInPlantYear`; I `Create_Valid_Returns201_DraftWithOrderNumber_AndLocation`; F `creates the order, then shows it in edit mode…`; E journey 1 | 201 + Location; Draft; `PO-YYYY-NNNNN`; edit mode with MSG-I001 | high |
| TC-002 | REQ-010 (DEC-012/013) | Fresh DB | I `Numbers_StartAt00001_AreGapFreeUnderConcurrency_AndOverflowIsRejectedCleanly` | 00001 first; failed create uses no number; 20 concurrent creates → 00003–00022; 100,000th → clean 500, nothing inserted, counter unchanged | high |
| TC-003 | REQ-014 (DEC-017) | FakeTimeProvider | I `PlantClock_YearBoundary_UsesTokyoTime`, `PlantOptions_ValidatesTimeZone` | 2026-12-31T15:30Z → 2027 in Asia/Tokyo; invalid IDs rejected | medium |
| TC-004 | REQ-011 | Existing Draft order | U `Update_ValidTransition_ReturnsNewAllowedStatuses`; F `lets an overdue order save…`; E journey 1 | Changes saved; MSG-I002; new version | high |
| TC-005 | REQ-011 | — | I `Get_UnknownOrNonGuidId_Is404`; U `Update_UnknownOrder_IsNotFound`; F `shows a not-found panel…`; E `unknown order shows the not-found panel` | 404 MSG-E011; panel, no form | medium |
| TC-006 | REQ-011 (DEC-010) | Two sessions load the same version | U `Update_StaleVersion_IsConflict`, `Update_ConcurrentSaveRace_IsConflict`; I `Update_StaleVersion_Is409_AndFirstWriterWins`; F `shows a Reload banner…`; E `two users editing the same order…` | Second save → 409 MSG-E009; first writer's values kept; Reload shows them | high |
| TC-007 | REQ-012 | No session | I `Unauthenticated_Is401_OnEveryEndpoint` (4 endpoints); smoke via proxy | 401 | high |
| TC-008 | REQ-012 | Signed in, no role | I `SignedInWithoutRole_Is403_OnEveryEndpoint`; F `shows the permission panel without calling the API…` | 403; panel MSG-E012, no API call | high |
| TC-009 | REQ-012, REQ-015 (DEC-001, DEC-019) | Operator | I `Operator_ListsThe30SeededProducts_OrderedBySku`; E `the product list shows all 30 seeded products` | 200; 30 products by SKU | medium |
| TC-010 | REQ-013 | — | U `ValidateQuantity_EnforcesRange`; I `Create_QuantityOutOfRange_Is400`, `Create_MalformedJsonValues_…`; F `quantity … → …` | 1/250/999,999,999 ok; 0/−5/2.5/abc → MSG-E003; 1,000,000,000 → MSG-E010 | high |
| TC-011 | REQ-014 (DEC-009) | Plant today = D | U `Create_DueToday_IsAccepted`, `Update_OverdueOrder_CanBeSavedWithoutChangingDueDate`, `Update_ChangingDueDateIntoPast_IsFieldError`; I `Create_UnknownProduct_AndPastDueDate_Are400`, `Create_DueTodayInPlantTimezone_IsAccepted`, `Update_OverdueOrder_SavesWithoutMovingDueDate_…`; F due-date cases; E empty-save test | D accepted; D−1 → MSG-E005 on create/change; unchanged past date saves | high |
| TC-012 | REQ-015 | — | U `Create_UnknownProduct_…`, `Update_ChangingToUnknownProduct_WhileDraft_IsFieldError`; I `Create_UnknownProduct_AndPastDueDate_Are400`; F `maps server field errors inline`, `blocks Save with inline errors…` | Missing → MSG-E001; unknown → MSG-E002 | high |
| TC-013 | REQ-016 | — | U `ValidateNotes_Accepts500CodePoints…`, `NormalizeNotes_EmptyBecomesNull`; I `Notes_500CodePointsIncludingEmoji_…`; F notes counter + validation | 500 (emoji = 1) ok; 501 → MSG-E006; empty → null | medium |
| TC-014 | REQ-017 | Order in each source state | U `Update_AllowsDefinedTransitions`; E journey 1 (Draft→InProgress→Completed) | Allowed transitions saved | high |
| TC-015 | REQ-017 | — | U `Update_RejectsOtherTransitions_WithoutChangingState`, `Update_DisallowedTransition_IsRuleViolation`; I `Update_DisallowedTransition_Is422`, `Update_UnknownStatusName_Is400` | 422 MSG-E007 (unknown name → 400 MSG-E007); status unchanged | high |
| TC-016 | REQ-017 (DEC-008) | Each status | U `AllowedNext_MatchesStateMachine`; F `locks product and quantity…`, `disables the status select for a terminal order`; E journey 1 | Options = current + allowed; disabled when terminal | medium |
| TC-017 | REQ-018 (DEC-007) | InProgress order | U `Update_RejectsLockedFieldChange_AsAWhole_AfterDraft`, `Update_LockCheckedBeforeTransition`, `Update_LockedQuantityChange_IsRuleViolation_EvenWithUnknownProduct`; I `Update_DraftToInProgress_ThenLockedQuantityChange_IsRejectedAsAWhole` | 422 MSG-E008; nothing else in the request saved | high |
| TC-018 | REQ-018 | InProgress order | F `locks product and quantity after Draft, keeps them focusable…`; E journey 1 | Read-only, focusable, lock hint announced | high |
| TC-019 | REQ-019 | Pristine form | F `leaves straight away when nothing changed`; E `Cancel with no changes leaves straight away` | Home, no dialog | medium |
| TC-020 | REQ-019 (DEC-018) | Edited form | F `asks before discarding changes…`, `Discard leaves without saving`; E `Cancel asks before discarding changes; Escape keeps editing…` | Dialog; Keep editing/Escape keeps values and focuses Cancel; Discard → home, nothing saved | high |
| TC-021 | — (DEC-020) | Signed in | I `NonJsonBody_Is415_AndNothingIsCreated` | 415; nothing created | medium |
| TC-022 | — (DEC-023) | — | I `Create_MissingFields_Is400_ProblemDetailsWithMessageIds_AndNoExceptionDetail`; TC-002 overflow step | `application/problem+json`, `type`/`code`/`traceId`; no `exception`/`detail`/driver text | high |
| TC-023 | — (DEC-016) | App as `pmai_app` | I `RuntimeLogin_CannotDeleteOrdersOrRunDdl`; the whole integration suite runs as `pmai_app`; manual psql on Compose | DELETE/DDL/TRUNCATE → 42501; all flows work | high |
| TC-024 | — (Observability) | ActivityListener/MeterListener | I `CreateAndUpdate_EmitSpansAndCounters` | Create/Update spans with id/outcome/status tags; created/updated/status_transitions counters | medium |
| TC-025 | — (DEC-026, WCAG 2.2 AA) | — | F axe in create + locked-edit tests; E axe on create, locked edit, error state, dialog | No violations | high |
| TC-026 | — (001_BD §1 SP) | Pixel 7 emulation | E `SP layout stacks fields and puts Save above Cancel` | Stacked fields; full-width Save above Cancel; no horizontal scroll; no axe violations | medium |
| TC-027 | — (regression) | App as `pmai_app` | I `AuthEndpointsTests` (4) | Login/me/logout still work under the restricted login | high |

## Environmental needs

- Local Windows 11 host, .NET SDK 10.0.303, Node 24.21.0, Docker 29.7.2.
- Integration: Testcontainers starts a throwaway `postgres:17` per test class, migrated as the owner. The app runs as `pmai_app` with a test-only password.
- E2E: the Compose stack from `deploy/` with a freshly wiped `db` volume, migrations applied as the owner, admin credentials from `deploy/.env`. Journeys create their own orders and don't depend on existing data.

## Commands and prerequisites

```
dotnet test src/backend/ProductionManagementAI.slnx
cd src/frontend && npm run lint && npm test && npm run build
cd tests/e2e && npm ci && npx playwright install chromium && E2E_ADMIN_PASSWORD=<SEED_ADMIN_PASSWORD> E2E_BASE_URL=http://localhost:3000 npx playwright test
```

Prerequisites: Docker running. For E2E, the stack must be up per `deploy/README.md`.

## Responsibilities and schedule

Authored and executed by Claude during plan revision 2 (steps 4–13); reviewed by ThanhTN on the PR.

## Risks and contingencies

| Risk | Contingency |
| --- | --- |
| Docker unavailable | Record integration/E2E as not run with the reason |
| CI can't run (GitHub account billing lock, observed on PR #2) | Local results are the evidence; CI re-run once the lock is cleared |
| Time-dependent due-date cases near midnight | Tests compute "today" in Asia/Tokyo (the server's rule) and use dates ≥ 30 days in the future for journeys |

## Results and linked evidence

| Test ID | Result (pass / fail / not run) | Evidence link | Date |
| --- | --- | --- | --- |
| TC-001–TC-027 | pass (all implementing tests pass: 49 U, 38 I, 38 F, 8 E) | `evidence.md` test execution log, final run 2026-09-18T03:12Z | 2026-09-18 |
| CI (GitHub Actions) | not run — account locked for billing | `evidence.md` defects/blockers | 2026-09-18 |

## Known gaps

| Gap | Reason | Risk | Follow-up |
| --- | --- | --- | --- |
| E2E not in CI | Needs the Compose stack in CI (plan revision 2 out of scope) | Browser regressions caught only when run locally | Future CI work item |
| Chromium only | Tooling scope (DEC-025) | Firefox/Safari-specific issues (e.g. native date input) | Add projects later if needed |
| No manual screen-reader pass | Automated axe only | Issues axe can't detect (announcement quality) | Manual pass before release |
| CI not executed | GitHub billing lock | CI-only failures (Linux paths, fresh restore) | Re-run CI when unlocked |

## Approvals

Reviewed with the PR by ThanhTN; not final until then.
