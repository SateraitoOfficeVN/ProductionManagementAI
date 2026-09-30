# Product master — Evidence

As of 2026-09-30. Evidence for approved design plan revision 3; no WI-006 application code, migration or executable test has run.

## Design-consistency review

| Check | Actual result | Evidence / remaining limit |
| --- | --- | --- |
| Stable requirements and acceptance criteria | Passed | [Brief](brief.md) defines REQ-049–REQ-060 with success and failure paths. |
| BD navigation, actions, exceptions and wireframe legends | Passed by document review | The two WI-006 BD files cover SCR-004 and SCR-001–SCR-003. Their SVGs parse as XML and numbered regions have matching legends. No approved WI-002–WI-004 design file was edited. |
| DD fields, validation, states and diagrams agree with BD | Passed by document review | Main DD and existing-screen addendum cover product fields, retired selection, unit lock, quantity precision and dashboard count/per-unit presentation. New DD wireframes' numbered callouts match the region table; Mermaid is rendered in PDFs. |
| API, DB and FN mappings | Passed by document review | `PRODUCT_UNIT_LOCKED`, `PRODUCT_INACTIVE` and `QUANTITY_UNIT_INVALID` map to specified fields; `numeric` range/scale and product-row lock order agree; dashboard Q2/Q4/Q5 map to unit-safe API fields. FN version 3 separates design and implementation acceptance checks without changing those rules. |
| Decisions, security, accessibility, migration and telemetry | Passed by document review | DEC-001–DEC-011 are recorded; role checks, cookie/PII-safe telemetry, WCAG 2.2 AA, seed preservation, order numeric migration, lock races and recovery limits are specified. Actual runtime behavior is still unverified. |
| Test traceability | Passed at plan level | [Test plan](test-plan.md) revision 2 maps every REQ-049–REQ-060 to TC-303–TC-323, including success, failure, races, PC/SP and regression scenarios. All cases are planned, not executed. |
| English/Japanese PDFs and local links | Passed | Nine WI-006 design Markdown files were scanned; all local links resolve, every English/Japanese PDF has a footer naming the source version on every page, and no raw Mermaid code was extracted. Fifteen WI-006 SVGs parse as XML. The corrected 004_DD-FN version 3 PDFs have 7 EN and 6 JA pages with extractable `lower(sku)`. |
| Protected approved baselines | Passed | Git status for WI-002–WI-004 BD/DB/DD Markdown and PDF paths is empty. |
| Final gate decision | **Passed for design, 2026-09-30** | User approved `004_DD-FN` version 3. The corrected audit found zero issues across 9 design files, 18 PDFs, 15 SVGs, 12 requirement IDs and 21 planned test IDs. Runtime verification remains an implementation acceptance check. |

The corrected automated document check was rerun after the user's approval on 2026-09-30 and found zero missing links, PDFs, revision footers, requirement IDs or planned test IDs. `git diff --check` exited 0 after a trailing blank line in `test-plan.md` was removed; Git emitted line-ending conversion warnings for existing working-tree files. The repository contains unrelated user changes (including `scripts/docs-pdf.py` and WI-007 status), which were preserved. Git status for approved WI-002–WI-004 design Markdown/PDF paths was empty.

## Application verification

| Cases / activity | Result | Reason |
| --- | --- | --- |
| TC-303–TC-323 | Not run | WI-006 implementation has not been authorized or written. |
| Backend/frontend build, unit, integration, E2E and axe | Not run | This is the design phase. |
| Fresh/upgrade migration, PostgreSQL collation and row-lock race | Not run | No WI-006 migration or application implementation exists yet; these are explicit implementation acceptance checks. |
| Deployment/smoke | Not run | No deployment is in approved plan revision 3. |

The design-consistency gate covers specifications and traceability only. TC-303–TC-323, migration rehearsal, collation checks, build, E2E, security review and delivery checks belong to the later implementation phase and retain their not-run state until actually executed.


## Revision 4 implementation evidence — 2026-09-30

Authorization: explicit user approval of revision 4, including the isolated worktree, local migrations/tests, focused commit, feature-branch push and PR to master. No merge or live/demo cutover was performed.

### Executed verification

| Check | Actual result |
| --- | --- |
| `dotnet build src/backend/ProductionManagementAI.slnx` | Passed; 0 warnings, 0 errors |
| Full `dotnet test ...` | Passed: 148 unit + 110 integration tests; no skips |
| Final additional reverse-lock scenario, ProductMasterEndpointTests | Passed: 25/25 (includes one scenario added after the full run) |
| Frontend `npm run build`, `npm run lint`, `npm test` | Passed; 139/139 tests, no lint findings |
| Docker backend/frontend builds | Passed against locked dependencies; isolated `pmaiwi006verify` project only |
| Full Compose Playwright desktop/mobile run | Passed: 25/25; includes axe, retirement focus, stale draft/reload and mobile 200% zoom |
| Additional uncertain-create scenario, product Playwright file | Passed: 3/3, including committed create followed by simulated 503; no write replay |
| Fresh migration | Passed on throwaway PostgreSQL 17 via fixture and isolated Compose database |
| Upgrade rehearsal | Passed from pre-WI-006 schema: 30 products, 124 historical orders, edited seed name, all order ID/product ID/quantity checksums retained; numeric type and valid unique index verified |
| Dependency advisories | npm production lockfile audit: 0 vulnerabilities; NuGet transitive vulnerability report: none reported |
| Approved artifact audit | 46 design/companion files match main-worktree SHA-256; 9 Markdown designs, 18 PDFs, 15 SVGs; zero missing links/PDFs or XML failures |
| Protected prior designs | No diff in WI-002–WI-004 design paths; shared main worktree preserved |

The local Windows lint installation needed the matching oxlint native binding installed without saving or changing package-lock.json. Local frontend unit/build used Vite 8.3.1 after this repair; the Docker clean install used the locked Vite 8.3.0. Both builds passed. Vitest/axe emits jsdom canvas warnings; assertions pass and browser axe checks also pass.

### Acceptance trace and meaningful coverage

- TC-303–TC-307: paged catalog/search/state, normalized creation, immutable SKU, duplicate/stale conflict, retirement; API, component and browser coverage.
- TC-308–TC-309: unchanged retired product can save; new retired selection rejected; deterministic PostgreSQL lock waits cover retirement-first and order-share-first schedules, plus referenced-unit race.
- TC-310/TC-318: fresh and upgrade migrations preserve history and edited names, refuse unsafe Down, enforce numeric constraints and retain restricted runtime behavior. Migration retries reject invalid SKU indexes; live recovery remains an owner-controlled procedure.
- TC-311/TC-314: every Product master endpoint denies anonymous/no-role calls; Admin/Operator successful journeys, unknown body fields, invalid identifiers/query and non-JSON 415 exercised.
- TC-312/TC-313/TC-317/TC-319/TC-321: Japanese component/axe coverage, original order/list/dashboard browser regressions, product PC/SP layouts, keyboard focus and mobile 200% zoom. Existing tests preserve historic filter and numeric sorting behavior.
- TC-315/TC-316: stale version, referenced-unit lock and row-share race; all five discrete units reject fractional values; kg/m preserve exact decimal tokens and reject exponent/excess precision.
- TC-320: PostgreSQL per-unit deltas (1.235 kg and 999999999 m), descending count ranking, ten workload buckets and count-only completion fields; mapper/regression coverage retains windows and snapshot behavior.
- TC-322: stale draft retained, explicit reload requires discard confirmation; retirement Cancel focus returns; uncertain create after server commit offers catalog lookup and does not replay.
- TC-323: full regression plus telemetry listeners prove Product.Create, unit-validation span tag and bounded metric dimensions; no SKU/name/quantity metric labels.

### Defects found and resolved

Product creation succeeded but its success notice disappeared when React reused the route component; fixed route state/flash handling and verified in Playwright. A duplicate matching the legacy exact-SKU index could become 500; both SKU indexes now map to PRODUCT_SKU_CONFLICT. Missing conflict reload and dialog focus handling were implemented and verified. Binary JSON parsing could round large decimal subtotals; quantity/openQuantity tokens now remain text with digit-based grouping, including 9007199254740.991. The first lock test fixture attempted to write a generated order_number; corrected the fixture to use order_year/order_seq and reran successfully.

### Limits and pending external evidence

Live screen-reader testing, other browser engines and a real environment cutover have not run. Automated axe/keyboard checks do not establish full WCAG conformance. Mobile zoom is CSS 200% zoom under Chromium, not a hardware-device test. Synthetic missing-unit corruption and every uncertain-write permutation were not separately injected; NOT NULL/FK constraints and generic failure/reload paths protect normal execution. No dependency or CI permission change was introduced. GitHub CI is pending until PR creation; no PR/merge/deployment is claimed in this section.


Final staged audit: 122 WI-006 files, no unexpected path, env file or dependency lockfile; private-key/GitHub-token/AWS-key patterns absent from added text. One pre-existing extra blank EOF line in a WI-006 Japanese HTML mockup was removed for diff-check hygiene; design Markdown/PDF contents remain unchanged. The first Git PDF textconv attempt lacked pdftotext in its shell; PDF identity was independently verified by SHA-256, and subsequent source audits disable textconv.


### Authorized delivery

Implementation commit `3251ebf` was pushed to `feature/WI-006-product-master`. [PR #31](https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/31) was opened to `master` with verification and cutover limits. `git diff --cached --no-textconv --check` passed before commit. CI/user review are pending; the PR is not merged. GitHub authentication succeeded outside the restricted network sandbox; the earlier sandbox-only invalid-token report was not a credential failure.


### GitHub CI verification

[Run 36682579899](https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/36682579899), head `8a43a76`, completed successfully on 2026-09-30. Backend build: zero warnings/errors; 148 unit and 111 integration tests passed, no skips. Frontend clean locked install/build/lint/tests passed. Fresh Compose migration/build and all 26 Playwright tests passed, including the additional uncertain-create scenario. This verifies the checked-in source against the dependency lockfile on Linux. The disposable local Compose stack/volumes and ignored credential file were removed; the feature worktree remains clean and retained for PR review. Merge/deploy remain unperformed.
