# Product master — Code Review

Reviewing feature/WI-006-product-master against e03246a, 2026-09-30. Self-review; user design approval is recorded separately.

## Scope and correctness

Reviewed Product master API/service/repository, row locks, migration preflight/recovery, quantity lexing/codec, dashboard SQL/mapping and Japanese UI against approved revision 4 and the nine WI-006 designs. Design, functionality, naming, comments, style and test coverage are suitable for PR review. No unresolved blocking code finding remains. Large decimal subtotals use textual digits; counts drive cross-unit charts/ranking. No approved prior design was edited.

## Resolved findings

| Location | Finding and resolution | Verification |
| --- | --- | --- |
| Infrastructure/Products/ProductMasterRepository.cs | Exact-case duplicates may hit ix_products_sku before the lower index; handle either constraint as SKU conflict | Sequential/concurrent duplicate integration tests |
| frontend/features/products/ProductMasterPage.tsx | Missing create announcement, explicit stale reload and focus restoration; fixed state isolation, recovery confirmation and native dialog focus | Product Playwright journeys |
| frontend/lib/apiClient.ts and lib/format.ts | Large decimal totals may round via JSON.parse/Number; preserve quantity tokens before parsing and group decimal digits | Exact large-subtotal unit test and browser regression |
| AddProductMasterFields migration | Invalid concurrently built index must block retry rather than be silently reused | Preflight check, fresh/upgrade valid-index assertion, recovery instructions |

## Security-review checklist

| Item | Result / evidence |
| --- | --- |
| Endpoint authentication/authorization | Pass: controller policy requires Admin or Operator; all five route operations deny anonymous/no-role users |
| Input validation and safe queries | Pass: bounded fields/query, strict request members, exact quantity lexer; EF/Npgsql parameters and escaped ILIKE; no external input becomes executable SQL |
| Secrets in diff/logs | Pass: isolated credentials remain ignored and absent from staged artifacts; no parameter-value logging enabled |
| Dependencies | Pass: no manifest/lockfile change; npm production audit and NuGet transitive check report none |
| Least privilege | Pass: INSERT and column-scoped mutable-field UPDATE only; no SKU UPDATE, DELETE or DDL grant |
| External content | Pass: guidance/tool output treated as data; no embedded authorization accepted |
| Safe errors | Pass: generic Problem Details, safe stable conflict codes; framework exception detail removed |
| Sensitive logs/telemetry | Pass: bounded operation/outcome dimensions; listeners prove metric keys; no SKU/name/drawing/quantity values in new instrumentation |
| New auth/payment/PII trust boundary | Not applicable: existing cookie/RBAC boundary reused; no new authentication flow or PII storage |

## Delivery gate

Scope and approval are identifiable; actual test commands and limits are in evidence.md. Required EN/JA PDFs and links match approved artifacts. The branch excludes unrelated shared-tree edits, old design changes, dependency changes and CI permission changes. Local implementation/review gate passed; feature commit/push/PR are authorized. CI, user PR review, merge and release remain separate stages. README/ai/project/CLAUDE post-merge closeout remains due after an authorized merge under feature-delivery step 8.

## Limitations

Manual live screen-reader and cross-browser checks have not run. CSS 200% mobile zoom and automated axe passed; full WCAG conformance is not claimed. Real cutover requires backup, write pause, owner migration and coordinated binaries; neither live migration nor deployment was performed. See evidence.md for focused tests added after full-suite counts.
