# WI-018 — Fix intermittent E2E failures — Evidence

Plan revision 1 (DEC-002), branch `fix/WI-018-flaky-e2e` from `ba7b930`. 2026-10-25.

## Diagnosis (step 2)

Source: `playwright-report` artifact of `master` run 40125879405 (traces of both failures).

### BUG-010 — product-type spec

Trace timeline: edit page GET product (type `PART`) → `toHaveValue('PART')` passes → `selectOption('半製品')` →
保存 clicked 6 ms later → PUT 200, and the **response still has `type: "PART"`**. The type change never reached the
server, so the saved product still matches the `type=PART` filter and the hidden-after-save notice correctly does not
appear. The spec's assertion is right; the 半製品 selection was lost between the select and the save. Whether the
loss is in the page (a state update not applied before submit) or in how the spec drives it is not yet proven.

### BUG-011 — production-lines add dialog TC-425/426

Trace request order: `q=` `q=` (first open) → `q=P-1001` (first search) → **`q=P-1001` then `q=`** on the reopen after
adding P-1001. The last request is the dialog's own `q=` load, issued after the user's search; its result (all
products) replaces the search result, so 「選択できる製品はありません。」 is not shown. `AddProductDialog.tsx` keeps
`query` from the previous session and resets it in the open effect (`setQuery('')`), so a reopen first fetches the
stale query, then `''`; with the user's search in between, the reset can win. This is an **application race** in the
WI-015 dialog, not WI-017 test data (the `q=P-1001` response contains only P-1001).

### Local reproduction

Not reproduced: both specs together, 20 repeats each (40/40 passed), and again with Chrome CPU throttled ×6 via
temporary uncommitted copies (16/16 passed). The races depend on CI runner timing.

## Stop

Both causes involve application code (`AddProductDialog.tsx`; possibly `ProductMasterPage.tsx`). Plan revision 1
forbids application changes (step 3 stop condition), so work stopped for plan revision 2.

## Revision 2 (DEC-003)

- **BUG-011 — application fix.** New `AddProductDialog.test.tsx` reproduced the CI race on the unchanged code (a
  reopened dialog first requested the previous search `P-1001`, then `''`). Fix: the dialog resets its search state
  when it closes instead of when it opens. Both new tests pass; all 64 production-lines unit tests pass.
- **BUG-010 — spec defect, no application defect.** A new unit test proves the form submits a type chosen in the same
  task as 保存. Cause: the spec queried `getByLabel(/種別/)` right after clicking the edit link; until the edit page
  renders, that matches the list page's 種別 filter (value `PART`), so the spec changed the filter and the form saved
  the unchanged type. Fix: wait for the 「製品編集」 heading, assert the selected value, and assert the PUT response type.
- TC-425/426 additionally asserts the reopened dialog's search box is empty.
- Frontend: oxlint clean, build passed, 317 unit tests passed. Local frontend container rebuilt; both specs 20/20
  (`--repeat-each=20`, installed Chrome).
- **Not run:** the full local Playwright suite (stopped by the user, DEC-004); CI on the PR runs the full suite.

## Merge (DEC-004)

PR #54 CI passed (backend, frontend, full E2E); squash-merged as `4a97798`. WI-017 closeout PR #53 merged as `5bc24f5`.
Branches `fix/WI-018-flaky-e2e` and `chore/WI-017-closeout` deleted locally and on origin. `master` run
[40256099518](https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/40256099518) on `4a97798`:
backend, frontend and E2E success.
