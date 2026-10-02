# WI-010 — Status

As of2026-10-02: revision3 approved; PR delivery in progress.
Revision2 local handoff approved with screen-reader/physical mobile keyboard/IME
checks explicitly accepted as Not run. All seven approved version1 designs and
companions remain immutable; no merge/deployment/live activation claimed.

Checkout C:/Data/project/ProductionManagementAI-WI010;
feature/WI-010-plant-calendar, initial baseline42e8932. Complete intended feature
package reviewed and ready for commit/rebase/push/PR under approved revision3.

Implemented Japanese month/mobile agenda, weekly definitions, plant/line exceptions,
retained history, exact current capacity, honest Unknown/no replay and eleven
Admin/Operator APIs. Coherent reads/atomic writes, strict parser, restricted schema,
explicit owner activation and safe scoped telemetry. Existing order/dashboard rules
preserved. Existing CI E2E job now explicitly activates only its disposable fixture.

Final local gates: Release build0warnings/errors;239unit/195integration;253frontend;
48full E2E on a fresh CI-style fixture,0failed/skipped/retries. Actual extracted CI
activation shell succeeds. Security/delivery review and209immutable-artifact audit
pass; package/lockfiles unchanged,0generated credential matches. Owned fixture/
images/temp credentials cleaned. Prior main checkout/videos preserved.

Next: commit intended package, synchronize with current master (RFC0013), push and
create PR, then inspect actual CI. Commit/push/PR and in-scope CI fixes authorized
by revision3; merge/live activation/deployment/videos still require separate request.
