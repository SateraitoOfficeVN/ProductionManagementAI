<!-- Based on ai/templates/test-plan.md (IEEE 829-1998). -->

# WI-014 — Order list overflow (BUG-008) — Test Plan

## Test plan identifier

TP-014, work item WI-014, revision 2, 2026-10-06 (plan revisions 1 and 2).

## References

[brief.md](brief.md) (BUG-008 and acceptance criteria), [plan.md](plan.md) revision 1 (step 3),
[002_DD 製造指示一覧](../../docs/en/020_detailed-design/002/002_DD_製造指示一覧.md) test scenarios TC-101–TC-119
(approved, unchanged), WI-003 REQ-020–REQ-024.

## Introduction

BUG-008 lets a long product name in the order-list product filter widen the whole page. The fix is CSS-only,
so this plan adds one E2E regression case (TC-120) for the layout. Plan revision 2 adds TC-121 for the open
list (contained, truncated, full name on hover) and a unit check of the `title` attributes. It relies on the existing Screen B cases for
the unchanged filter behaviour.

## Test items

| Requirement ID | Description |
| --- | --- |
| BUG-008 AC-1 | At 390, 1280 and 1920 px with a 200-character product name, `/production-orders` has no horizontal page scroll and the product filter stays inside its column |
| BUG-008 AC-2 / REQ-022 | The product filter still lists every product with its full `SKU — name` text, and filtering by product still works |
| BUG-008 AC-3 | PC filter layout (status beside product, three-column row below) and SP layout (single column, cards) otherwise unchanged |
| BUG-008 AC-4 | Regression tests cover phone and PC without leaving long-name products in the shared database |
| BUG-008 AC-5 (rev. 2) | The open product list stays inside its control; long names end in an ellipsis; hover shows the full `SKU — name` |

## Features to be tested

- Page width and product-filter containment with a very long product name (TC-120, new).
- Open list containment, truncation, `title` and filtering with a long name (TC-121, new; unit `title` test).
- Existing filtering, paging and SP card layout (TC-115, TC-116, `mobile.spec.ts`), re-run unchanged.

## Features not to be tested

- Backend, API and database: not changed.
- Other screens: not changed. `/production-orders/new`, `/`, `/products`, `/production-lines` and
  `/plant-calendar` were checked by hand when the bug was reproduced and were not affected.
- Product-name length rules: out of scope.

## Approach

| Level | Included? | Rationale |
| --- | --- | --- |
| Unit (Vitest + RTL) | yes, existing suite only | jsdom does no layout, so it cannot measure overflow; the existing unit suite guards filter behaviour and wording (TC-301) |
| Integration | no | No backend change |
| E2E (Playwright) | yes, new TC-120 plus the full existing suite | Overflow is a real-browser layout property |
| Manual measurement | yes | Widths at 390/1280/1920 px against the local demo DB, which holds real long-name products |

## Item pass/fail criteria

Pass when TC-120 fails on the pre-fix image (`46bf430`) and passes after the fix. All other non-environmental
E2E cases and the full unit suite must pass. CI must pass all three jobs.

## Suspension criteria and resumption requirements

Suspend if the local Compose stack is unavailable; resume after `docker compose -f deploy/compose.yaml up -d`.
Plant-calendar specs need CI's activated-calendar fixture. Locally they are recorded as environment failures
and their proof comes from CI.

## Test deliverables

This test plan, the TC-120 case in `tests/e2e/specs/screen-b.spec.ts`, and results in [evidence.md](evidence.md)
and in the CI run linked below.

## Cases

| Test ID | Requirement ID | Precondition / setup | Steps | Expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| TC-120 | BUG-008 AC-1, AC-2, AC-4 | Signed in. `GET /api/products` is intercepted and one mocked product `LONG-BUG-008` with a 200+-character name is appended; no database write | At 390 px, then 1280 px: open `/production-orders` | The option `LONG-BUG-008 — <full name>` exists; `scrollWidth <= innerWidth`; the select's right edge is inside the filter form | high |
| TC-115, TC-116 (existing) | REQ-022, REQ-024 | Seeded demo orders | Filter, page and page-size journeys | Unchanged behaviour | high |
| TC-121 | BUG-008 AC-2, AC-5, REQ-022 | Signed in. P-1001 renamed to a 200+-character name only in the mocked `/api/products` response | At 390 and 1280 px: click the product select; inspect options; choose P-1001; press 検索 | Every option inside the control's edges; long option truncated with its `title` = full label; no page scroll; value and select `title` updated; URL has `productId`; result summary shown | high |
| U-TC-121 (unit) | BUG-008 AC-5 | Mocked products | Render list; choose P-1004 | Select `title` = 「すべての製品」, then `P-1004 — ドライブシャフト`; each option has its full label as `title` | medium |
| `mobile.spec.ts` order-list cards (existing) | BUG-008 AC-3 | Local demo DB with real long-name products | Open the order list at SP width | Cards, not a table, at full width with no page overflow | high |
| M-1 (manual) | BUG-008 AC-1, AC-3 | Local demo DB with real long-name products | Measure page/select/form widths at 390/1280/1920 px; compare screenshots with the 002 mockup layout | No overflow at any width; layout unchanged | medium |

## Environmental needs

Local Compose stack (`deploy/compose.yaml`) with the frontend rebuilt from the branch, plus CI. TC-120 mocks
its data per request, so it needs no seeding or cleanup. M-1 uses the long-name products left in the local demo
DB by earlier runs.

## Commands and prerequisites

```
cd src/frontend && npm run lint && npm run build && npx vitest run
cd tests/e2e && E2E_ADMIN_PASSWORD=<SEED_ADMIN_PASSWORD> E2E_BASE_URL=http://localhost:3000 npx playwright test specs/screen-b.spec.ts -g BUG-008
cd tests/e2e && E2E_ADMIN_PASSWORD=<SEED_ADMIN_PASSWORD> E2E_BASE_URL=http://localhost:3000 npx playwright test
```

Prerequisites: running Compose stack; seed admin password from `deploy/.env` (never recorded).

## Responsibilities and schedule

Single-agent execution during plan step 3 (author TC-120) and step 4 (run). The user reviews through PR #43.

## Risks and contingencies

| Risk | Contingency |
| --- | --- |
| The mocked product differs from real data | M-1 measured the real long-name products in the local demo DB |
| Zero-minimum tracks change PC proportions | Screenshots compared with the 002 mockup layout (M-1) |

## Results and linked evidence

| Test ID | Result | Evidence link | Date |
| --- | --- | --- | --- |
| TC-120 on pre-fix image | fail (expected) | [evidence.md](evidence.md) | 2026-10-06 |
| TC-120 after fix, local | pass | [evidence.md](evidence.md) | 2026-10-06 |
| Full local E2E | 56 pass, 11 fail (plant-calendar environment only) | [evidence.md](evidence.md) | 2026-10-06 |
| Frontend lint/build/unit | pass (267 unit) | [evidence.md](evidence.md) | 2026-10-06 |
| M-1 | pass (390/390, 1280/1280, 1920/1920 px) | [evidence.md](evidence.md) | 2026-10-06 |
| TC-121 on revision 1 image | fail (expected) | [evidence.md](evidence.md) | 2026-10-06 |
| TC-121 after revision 2, local | pass | [evidence.md](evidence.md) | 2026-10-06 |
| U-TC-121 and frontend lint/build/unit (rev. 2) | pass (269 unit) | [evidence.md](evidence.md) | 2026-10-06 |
| Full local E2E (rev. 2) | 57 pass, 11 fail (plant-calendar environment only) | [evidence.md](evidence.md) | 2026-10-06 |
| CI run 37433461443, final head (TC-120, TC-121 and full suite) | pass: 239 backend unit, 203 integration, 269 frontend, 68 E2E | https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/37433461443 | 2026-10-06 |
| CI run 37427110603, revision 1 head (TC-120 and full suite) | pass: 239 backend unit, 203 integration, 267 frontend, 67 E2E | https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/37427110603 | 2026-10-06 |

## Known gaps

| Gap | Reason | Risk | Follow-up |
| --- | --- | --- | --- |
| 1920 px is not automated | 1280 px already exercises the PC grid; 1920 px only adds empty margin (`max-w-5xl`) | Low | Covered manually by M-1 |
| No automated filtering run with a long-name product selected | CSS-only change; filtering logic is unchanged and covered by TC-115/TC-116 | Low | None planned |
| Tooltip rendering not observed | Headless Chromium does not draw OS tooltips; `title` attributes are asserted | Low | User checks hover in Chrome/Edge |
| Firefox/Safari keep the wide native popup | No `base-select` support (DEC-003) | Medium on those browsers; the page itself does not overflow | Revisit when supported, or a custom component in a new revision |
| Touch devices cannot hover | `title` is mouse-only | Low | Full name stays in the accessible name and on 製品マスタ |

## Approvals

Covered by plan revision 1 and 2 approvals; results final: PR #43 merged as `3e29618` on 2026-10-06.
