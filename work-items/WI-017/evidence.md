# WI-017 — Product type — Evidence

Plan revision 2 (DEC-012), branch `feature/WI-017-product-type`, uncommitted. All runs 2026-10-25, local.

## Environment notes

- No .NET SDK is installed on this machine. Backend build/tests ran in `mcr.microsoft.com/dotnet/sdk:10.0` with the
  repo copied in (the project drive is not shared with Docker) and the Docker socket mounted for Testcontainers
  (`TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal`). The migration was scaffolded the same way (dotnet-ef 10.0.12).
- `npx playwright install chromium` failed (download timeout). E2E ran with installed Google Chrome through a temporary,
  uncommitted config (`channel: 'chrome'`), since deleted. The repo config is unchanged.

## Results

| Check | Result |
| --- | --- |
| Backend build (`-warnaserror`) | Passed, 0 warnings |
| Backend unit (`dotnet test`) | 302 passed (incl. TC-470, TC-471) |
| Backend integration | 247 passed (incl. TC-472–TC-480). First run: 3 failures in existing tests, fixed: two raw-SQL product inserts lacked `product_type`; the WI-010 calendar migration test checksummed `products` across all later migrations, now stops at its own migration |
| Frontend lint / build / test | oxlint clean; build passed; 314 tests passed (incl. TC-481–TC-487) |
| Local migration (Compose `migrate`, owner) | Applied `20261024184015_AddProductType`; demo SKUs 13 FINISHED / 5 SEMI_FINISHED / 12 PART |
| Playwright full suite | 69 passed, 11 failed. All 11 failures are plant-calendar specs (5 + 5 + 1 mobile) timing out on disabled calendar actions: the local demo DB has no activated calendar, the same environment limitation recorded in WI-013 evidence. CI activates it in its fixture. One failure in the new spec (ambiguous `11件` locator) was fixed and all product specs then passed: 32 passed (TC-488–TC-491) |
| Screenshots vs 004_DD-TYPE mockup | PC list filtered by 部品, PC create with type error, SP cards: layout matches (filter after 状態, column after 製品名, 「種別：…」 card line, required select with 「選択してください」 and MSG-E026). Difference: the mockup's error box also lists 「・種別を選択してください。」; the app keeps its existing generic summary line, as for all fields (WI-013 behaviour) |

## Not run

Screen-reader speech and physical mobile keyboard/IME (as earlier WIs). CI not run (no push).

## Local data note

E2E journeys add test products to the local demo DB, as in earlier WIs.

## CI (plan revision 3)

PR [#52](https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/52), head `fec2f93`: run
[40124507111](https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/40124507111) succeeded —
Backend (build, test) 1m45s, Frontend (build, lint, test) 46s, E2E (Compose stack + Playwright) 4m26s, all pass. CI's
E2E includes the plant-calendar specs that need the activated-calendar fixture.

## Merge (plan revision 4)

PR #52 squash-merged as `ba7b930` (guarded by head `fec2f93`); remote and local feature branch deleted; refs pruned;
`master` fast-forwarded. First `master` run
[40125879405](https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/40125879405) on `ba7b930`:
backend and frontend pass; **E2E failed, 78 passed / 2 failed** — `product-type.spec.ts` "filter by type keeps it
through paging and return after save" (hidden-after-save notice not shown within 5 s) and
`production-lines-redesign.spec.ts` TC-425/426 (expected 「選択できる製品はありません。」 not found). The same commit's
tests passed on the PR run, so both are intermittent; diagnosis and fix need approval (plan revision 4 limits).
