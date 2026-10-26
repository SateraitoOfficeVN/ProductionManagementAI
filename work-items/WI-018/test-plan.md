# WI-018 — Fix intermittent E2E failures — Test Plan

TP-018, revision 1, 2026-10-26. Covers REQ-094–096 ([brief](brief.md)).

| TC | REQ | Level | Case | Test | Result |
| --- | --- | --- | --- | --- | --- |
| TC-492 | 094, 095 | Frontend unit | Reopened add dialog starts empty and requests only `q=` | `AddProductDialog.test.tsx` | Pass (failed before the fix) |
| TC-493 | 095 | Frontend unit | An older load answering last never replaces a newer search | `AddProductDialog.test.tsx` | Pass |
| TC-494 | 094 | Frontend unit | Type chosen in the same task as 保存 is submitted | `ProductMasterPage.test.tsx` | Pass |
| TC-489 | 095, 096 | E2E | WI-017 filter/paging/return-after-save, now waiting for the edit page and asserting the PUT type | `product-type.spec.ts` | 20/20 local |
| TC-425/426 | 095, 096 | E2E | Add dialog, now also asserting the reopened search is empty | `production-lines-redesign.spec.ts` | 20/20 local |

Full suite: CI on the PR and on `master` after merge (local full run not performed, DEC-004).
