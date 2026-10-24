# WI-017 — Product type — Status Report

As of 2026-10-25. Work item state: in-review.

## Overall status

**RAG:** Green — design approved (DEC-007–DEC-011); implementation and tests complete locally (revision 2, DEC-012); nothing committed.

## Approved plan reference

[plan.md](plan.md) — revision 2, approval source DEC-012.

## Accomplishments this period

| Date | Milestone / deliverable | Evidence link |
| --- | --- | --- |
| 2026-10-25 | 004_BD/DB/DD/DD-API/DD-SPD-TYPE v1 approved with EN/JA PDFs | docs/en/**/004_*-TYPE_製品種別_20261025.md |
| 2026-10-25 | Backend, migration, frontend, tests; local migration applied | [evidence.md](evidence.md) |
| 2026-10-25 | Test plan TC-470–TC-491 | [test-plan.md](test-plan.md) |

## Risks and issues

| Issue | Owner | Since | Impact |
| --- | --- | --- | --- |
| Plant-calendar E2E fails locally (no activated calendar), pre-existing | — | WI-013 | Covered by CI fixture |
| Mockup error box lists the type message; app shows generic summary | User | 2026-10-25 | Cosmetic; decide keep or change |

## Next action

Revision 3 (DEC-014): commit, push, PR, CI watch.
