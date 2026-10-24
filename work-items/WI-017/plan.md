# WI-017 — Product type — Implementation Plan

| Revision | Date | Phase / purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-10-25 | Design (BD, DB, DD addenda); documents only | Complete — BD, DB, DD, DD-API, DD-SPD v1 approved (DEC-007–DEC-011) with EN/JA PDFs | User "approved, go a head", 2026-10-25 (DEC-006) |
| 2 | 2026-10-25 | Implementation and tests (local only, no commit) | Complete — results in evidence.md; accepted (DEC-013) |
| 3 | 2026-10-25 | Commit, push, PR and CI | in progress | User "approved, continue with commit and PR", 2026-10-25 (DEC-014) | User "approved, continue with implementation", 2026-10-25 (DEC-012) |

## Revision 1 — Design (documents only)

### Objective

Specify REQ-090–REQ-093 ([brief](brief.md)) before any code. Implementation/tests are revision 2, drafted after design approval.

### Proposed direction

- **DB:** `products.product_type varchar(20) NOT NULL` + CHECK on allowed codes; index `(product_type, …)` only if the filtered list query needs it. One EF Core migration, owner-run (deploy/README.md); `pmai_app` grants unchanged (same table).
- **Domain/API:** `ProductType` enum stored as string; create/update contracts gain required `type`; list items/detail return `type`; list gains optional `type` filter (allow-listed, 400 otherwise). Additive to 004_DD-API.
- **UI:** 「種別」 select (必須) on the form; 「種別」 column after 製品名 and a type select in the filter panel, URL `type`; SP card line 「種別：」.
- **Seed:** demo products get realistic types.

### Open decisions

| # | Question | Recommendation |
| --- | --- | --- |
| Q1 | Values (code → label) | `FINISHED` 完成品, `SEMI_FINISHED` 半製品, `PART` 部品, `RAW_MATERIAL` 原材料 |
| Q2 | Backfill for existing rows | `FINISHED` (完成品); demo seed assigns per product |

### Design files, in review order (RFC 0012 stop after each; RFC 0014 dated names)

1. `004_BD-TYPE_製品種別_20261025.md` (+ wireframe SVG)
2. `004_DB-TYPE_製品種別_20261025.md` (ER/column diff)
3. `004_DD-TYPE_製品種別_20261025.md` + `-API`/`-SPD` companions (+ mockup)

Approved 004 documents stay unedited. PDFs (EN/JA) only after each .md is approved.

### Permitted actions

Create WI-017 records and the design files above on a local branch `feature/WI-017-product-type`. No code, migration, commit, push, PR, merge or deployment.

### Risks

- Migration on a populated table: NOT NULL with default backfill in one step; rehearse on a disposable DB in revision 2.
- E2E/fixtures that create products must send `type`.

## Revision 2 — Implementation and tests (local only)

### Objective

Implement REQ-090–REQ-093 exactly as approved 004_BD/DB/DD/DD-API/DD-SPD-TYPE v1, with tests at every level.

### Steps

| # | Step | Files | Verification |
| --- | --- | --- | --- |
| 1 | `test-plan.md` with TC IDs from the DD/API/SPD test viewpoints | `work-items/WI-017/test-plan.md` | Each REQ maps to ≥1 TC |
| 2 | Domain: `ProductType` enum + code table; `Product.Type` | Domain | Build |
| 3 | EF mapping (string converter to codes, `varchar(20)`, `ck_products_product_type`) and migration `AddProductType`: add column with temporary default `FINISHED`, update 17 demo SKUs, drop default, add CHECK, `GRANT UPDATE (product_type) ON products TO pmai_app` (additive to the 20260930021917 grant); `Down` reverses | Infrastructure, Migrations | Migration on a fresh DB and on a DB at current `master` with data (Testcontainers integration test) |
| 4 | Contracts/service/repository: `type` in list query (`TryCreate`), item, create/update requests, `Validate`, list filter | Application, Infrastructure | Unit + integration tests per 004_DD-API-TYPE |
| 5 | Frontend `api.ts` (`productTypes`, types), `messages.ts` (`type`, `typePlaceholder`, `types`, `typeRequired` MSG-E026), `ProductMasterPage.tsx` (filter, column, SP card, form select, validation/focus, URL `type`, hidden-after-save) | Frontend | `ProductMasterPage.test.tsx` cases per SPD-TYPE |
| 6 | Update product-creating tests/fixtures to send `type` | `tests/integration/**` (ProductMaster, ProductionLines, PlantCalendar), `tests/e2e/specs/product-*.spec.ts`, `plant-calendar*.spec.ts` | Existing suites green |
| 7 | New E2E: create with type, filter by 部品 (URL, paging, return-after-save), edit type, SP card, axe | `tests/e2e/specs/product-type.spec.ts` | Playwright |
| 8 | Verify: backend build + `dotnet test`; frontend lint/build/test; rebuild local Compose, apply migration as owner, full Playwright; PC/SP screenshots compared with the 004_DD-TYPE mockup; record evidence, test-plan results and status | — | evidence.md |

### Permitted actions

Edits on local branch `feature/WI-017-product-type` to the files above and WI-017 records; rebuilding the local Compose
stack and applying the new migration to the **local** demo database as owner; running all verified commands
(ai/project.md). Not permitted: commit, push, PR, merge, deployment, editing approved design documents.

### Risks / stop conditions

- The local demo DB gets the migration (data-preserving; `Down` drops the type). Stop if the migration fails on it.
- Any change to the approved API/DB contract, a new dependency, or another screen stops work for a new revision.
- Pre-existing test failures unrelated to WI-017 are reported, not fixed silently.

## Revision 3 — Commit, push and PR

### Steps

| # | Step | Verification |
| --- | --- | --- |
| 1 | Update WI-017 status/evidence for handoff | Records current |
| 2 | Review `git diff`; no secrets, no temp files (scratchpad, local Playwright config, test-results) | Diff reviewed |
| 3 | One commit on `feature/WI-017-product-type`: code, migration, tests, the five `*-TYPE_製品種別_20261025` design docs with wireframes, mockups and EN/JA PDFs, `work-items/WI-017/` (`feat(WI-017): add required product type to the product master`, co-author line) | `git show --stat` |
| 4 | `git push -u origin feature/WI-017-product-type` | Remote branch exists |
| 5 | `gh pr create` to `master`: REQ-090–093, design links, migration/deploy note (migration and API together), test results, local plant-calendar limitation, environment notes | PR URL |
| 6 | Watch CI (backend, frontend, e2e) read-only and record the run in evidence/status | Run URL and job results |

### Permitted actions

`git add`/`commit` of the listed files, push of this branch, `gh pr create`, read-only `gh` checks. Not permitted: merge,
branch deletion, deployment, approved-design edits, demo-DB changes. A CI failure is diagnosed and reported; fixes need approval.
