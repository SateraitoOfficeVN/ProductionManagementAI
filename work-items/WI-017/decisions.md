# WI-017 — Product type — Decision Log

## Log

| ID | Date | Decision needed | Decision maker | Status | Rationale (summary) |
| --- | --- | --- | --- | --- | --- |
| DEC-001 | 2026-10-25 | Kind of value | User (question answer "Fixed list") | decided | Validated code from a fixed list; no type master |
| DEC-002 | 2026-10-25 | Required? | User ("Required, backfill existing") | decided | NOT NULL; migration backfills existing rows |
| DEC-003 | 2026-10-25 | Where it appears | User ("Product list column, Product list filter, Product register/edit") | decided | Product master screens only; not orders/CSV |
| DEC-004 | 2026-10-25 | Q1 type values | User ("approved, go a head" on plan revision 1) | decided | FINISHED 完成品 / SEMI_FINISHED 半製品 / PART 部品 / RAW_MATERIAL 原材料 |
| DEC-005 | 2026-10-25 | Q2 backfill default | User (same message) | decided | Existing rows → FINISHED; demo seed assigns realistic types |
| DEC-006 | 2026-10-25 | Plan revision 1 approval | User ("approved, go a head") | decided | Design phase may start |
| DEC-007 | 2026-10-25 | 004_BD-TYPE v1 review | User ("approved, continue with DB design") | decided | BD approved unchanged; EN/JA PDFs rendered |
| DEC-008 | 2026-10-25 | 004_DB-TYPE v1 review | User ("approved, continue with DD design") | decided | DB approved unchanged; EN/JA PDFs rendered |
| DEC-009 | 2026-10-25 | 004_DD-TYPE v1 review | User ("approved, continue with API design") | decided | DD approved unchanged (incl. no separate FN addendum); EN/JA PDFs rendered |
| DEC-010 | 2026-10-25 | 004_DD-API-TYPE v1 review | User ("approved, continue with SPD design") | decided | API approved unchanged; EN/JA PDFs rendered |
| DEC-011 | 2026-10-25 | 004_DD-SPD-TYPE v1 review | User ("approved, continue with implementation plan") | decided | SPD approved unchanged; EN/JA PDFs rendered; design phase (revision 1) complete |
| DEC-012 | 2026-10-25 | Plan revision 2 approval | User ("approved, continue with implementation") | decided | Implementation/tests local only |
| DEC-013 | 2026-10-25 | Revision 2 result review | User ("approved, continue with commit and PR plan") | decided | Implementation accepted incl. generic error summary (mockup difference kept); revision 3 to be drafted |
| DEC-014 | 2026-10-25 | Plan revision 3 approval | User ("approved, continue with commit and PR") | decided | Commit, push, PR, read-only CI watch |
| DEC-015 | 2026-10-25 | Draft merge/cleanup plan | User ("continue with merge and cleanup plan") | decided | Revision 4 drafted for approval; not executed |
| DEC-016 | 2026-10-25 | Plan revision 4 approval | User ("approved, continue with merge and cleanup") | decided | Merge PR #52, cleanup, closeout PR |
| DEC-017 | 2026-10-25 | Open closeout PR before WI-018 | User ("approved, open the closeout PR now too") | decided | Records state master E2E red, follow-up WI-018 |
