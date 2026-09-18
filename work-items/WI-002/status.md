# Production Order Create/Edit (Screen A) — Status Report

As of 2026-09-18. Work item state: in-progress.

## Overall status

**RAG:** Amber — at risk
The design phase (plan revision 1) is complete and approved: brief, BD-001, DB-002, the DD-001 set and the mockup. Plan revision 2 is approved; steps 1–13 are done: implemented, and all 133 tests pass locally. Step 14 (push + PR) is in progress. CI is blocked by a GitHub account billing lock.

## Approved plan reference

[plan.md](plan.md) — revision 1 approved and complete (2026-09-18). Revision 2 approved 2026-09-18 (user message "ok revision 2 plan seem solid so approved").

## Accomplishments this period

| Date | Milestone / deliverable | Evidence link |
| --- | --- | --- |
| 2026-09-18 | WI-002 restarted; brief revision 1 (REQ-010–REQ-018) and decision log (DEC-001–DEC-009) | [brief.md](brief.md), [decisions.md](decisions.md) |
| 2026-09-18 | Plan revision 2 steps 1–13: harness PR #2; WI-002 backend, DB login split, frontend, tests (49 unit, 38 integration, 38 frontend, 8 E2E, all pass); test-plan TP-002; checklists | [evidence.md](evidence.md), [test-plan.md](test-plan.md) |
| 2026-09-18 | Plan revision 2 approved; harness RFC 0002 (keep every plan revision, oldest first) applied at the user's request | [plan.md](plan.md), `ai/improvements/0002-plan-revision-history.md` |
| 2026-09-18 | DD set approved by the user; plan revision 1 step 6 closed (design phase complete); DEC-025–DEC-028 answered; plan revision 2 drafted | [plan.md](plan.md), [decisions.md](decisions.md) |
| 2026-09-18 | Added the DD-001-FN and DD-001-SPD companions at the user's request | [DD-001-FN](../../docs/en/020_detailed-design/DD-001-FN-production-order-service.md), [DD-001-SPD](../../docs/en/020_detailed-design/DD-001-SPD-production-order-create-edit.md) |
| 2026-09-18 | DB-002 reviewed by the user; DD-001, DD-001-API and the rendered mockup produced (step 5); DEC-021–DEC-024 decided; BD-001 revision 4 (quantity upper bound) | [DD-001](../../docs/en/020_detailed-design/DD-001-production-order-create-edit.md), [mockup](https://claude.ai/artifact/FEo1RG27UjZ6vxFCUjxoHq) |
| 2026-09-18 | Remaining questions answered: DEC-016 split DB logins now, DEC-017 `Asia/Tokyo`, DEC-018 Cancel confirmation (REQ-019), DEC-019 30 products, DEC-020 CSRF approach; BD-001 revision 3, DB-002, ADR-0002 and DB-001 updated | [decisions.md](decisions.md) |
| 2026-09-18 | Plan revision 1 approved; DB-002 drafted (steps 3–4); DEC-013–DEC-015 decided, DEC-016 proposed | [DB-002](../../docs/en/database/0002-production-order-schema.md), [decisions.md](decisions.md) |
| 2026-09-18 | DEC-009–DEC-012 answered by the user; BD-001 revision 2 and brief updated | [decisions.md](decisions.md) |
| 2026-09-18 | BD-001 drafted against the rewritten BD template | [BD-001](../../docs/en/010_basic-design/BD-001-production-order-create-edit.md), [evidence.md](evidence.md) |

## Planned for next period

User review of the Screen A PR and harness PR #2; merge only on the user's go-ahead.

## Risks and issues

| Issue / blocker | Owner | Since | Impact |
| --- | --- | --- | --- |
| GitHub Actions won't start jobs: account locked for billing | ThanhTN | 2026-09-18 | CI evidence for both PRs |

## Next action

Push the WI-002 branch and open its PR (plan revision 2, step 14). Then: ThanhTN reviews both PRs and clears the GitHub billing lock.
