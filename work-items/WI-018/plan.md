# WI-018 — Fix intermittent E2E failures — Implementation Plan

| Revision | Date | Phase / purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-10-25 | Diagnose and fix both specs; local verification; commit, push, PR, CI | Stopped at step 3 — causes are in application code (evidence.md) | User "approved, open the closeout PR now too", 2026-10-25 (DEC-002) |
| 2 | 2026-10-25 | Fix the application races, harden the specs, verify, PR | in progress | User "approved, continue with revision 2", 2026-10-25 (DEC-003) |

## Revision 1

### Objective

Make BUG-010 and BUG-011 ([brief](brief.md)) pass reliably without weakening what they test.

### Steps

| # | Step | Verification |
| --- | --- | --- |
| 1 | Branch `fix/WI-018-flaky-e2e` from `master` (`ba7b930`) | Branch exists |
| 2 | Diagnose: read both failure traces from the run's `playwright-report` artifact (network, DOM, timing); reproduce locally by running `product-type.spec.ts` + `production-lines-redesign.spec.ts` together with `--repeat-each=20` | Root cause per bug in evidence.md |
| 3 | Fix BUG-010 according to the cause. Expected: the spec waits for the list to settle after the page-past-end move, or edits a product that stays on its page. If the notice really is lost when the list moves back a page, that is an application defect: stop and present a revision (it touches SCR-004 behaviour) | Spec passes 20/20 |
| 4 | Fix BUG-011 according to the cause. Expected: WI-017 spec data matches the dialog search (shared database); make WI-017 test products impossible to match (distinct SKU/name pattern, retire them at the end) rather than changing the production-lines assertion | Spec passes 20/20 with the WI-017 spec run first |
| 5 | Full local suite (Compose, installed Chrome as in WI-017); frontend/backend untouched unless step 3 stops | Same 11 known local calendar failures only |
| 6 | Records: brief, test-plan (re-run TC-489, TC-425/426), evidence, status | — |
| 7 | Commit `test(WI-018): stabilise product-type and add-dialog E2E specs`, push, PR to `master`, watch CI | PR CI green |

### Permitted actions

Edits to the two specs (and test helpers if needed) and WI-018 records on the branch; local Compose stack and
Playwright runs (they add test data to the local demo DB, as before); commit, push, PR, read-only CI checks.
Not permitted: merge, application/design changes (step 3 stop), retries, deleting tests, deployment.

### Risks / open point

- If the cause is in the application (step 3), scope grows: new revision.
- The WI-017 closeout PR (`chore/WI-017-closeout`, records only, uncommitted) is still pending. Recommendation: open it
  now with its notes stating that `master` E2E is red and WI-018 fixes it, so the WI-017 records are accurate either way.

## Revision 2 — Application fixes

### Steps

| # | Step | Verification |
| --- | --- | --- |
| 1 | **AddProductDialog** (WI-015, SCR-005): reset `draft`/`query`/`page`/`chosen` when the dialog **closes**, and ignore any response whose key is not the current one (already keyed, but also drop the stale-session fetch), so a reopen starts at `''` and a user search can never be overwritten by an older load | New Vitest: reopen after a search, search again, older `''` response resolving last does not replace the result |
| 2 | **ProductMasterPage editor**: confirm whether a select change immediately followed by 保存 can submit the previous value (Vitest with fake timing / React act). If yes, submit from the latest draft (ref or form data) instead of a closure value. If no, the loss is in the spec | Vitest proves the case either way; result in evidence |
| 3 | Specs: after `selectOption`, assert the select value and wait for the PUT response and its `type` before checking the list; in TC-425/426 wait for the dialog's search response. No assertion removed, retries stay 0 | Both specs 20/20 locally |
| 4 | Frontend lint/build/test; full local Playwright (Chrome); design check: behaviour matches 005_DD-SPD-REDESIGN and 004_DD-SPD-TYPE (no design change) | evidence.md |
| 5 | Records (brief, test-plan, evidence, status); commit `fix(WI-018): …`, push, PR, watch CI | PR CI green |

### Permitted actions

Edits to `AddProductDialog.tsx`, `ProductMasterPage.tsx` (only if step 2 proves the defect), their unit tests, the two
E2E specs and WI-018 records; local runs; commit, push, PR, read-only CI. Not permitted: merge, design edits,
retries, deployment.

### Risks

- The dialog change touches WI-015 behaviour; its existing unit and E2E tests guard it.
- If step 2 finds no page defect, BUG-010 is fixed in the spec only, and that is stated in the PR.
