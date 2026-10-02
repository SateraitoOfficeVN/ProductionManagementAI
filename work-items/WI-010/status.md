# WI-010 — Status

As of 2026-10-02: revision 3 implementation and PR publication complete;
[PR #36](https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/36) open.
Implementation head f2a1d82 passed all three GitHub CI jobs in
[run 36975008859](https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/36975008859).
The final publication-record commit changes routine Markdown only; its actual head
checks and handoff are tracked in PR #36. Merge remains a separate user instruction.

Checkout: C:/Data/project/ProductionManagementAI-WI010;
branch: feature/WI-010-plant-calendar; initial baseline 42e8932.
Reviewed implementation f2a1d82 rebased without conflicts onto master f44283c.
All seven approved version 1 designs and their companions remain immutable.

Implemented Japanese month/mobile agenda, weekly definitions, plant/line exceptions,
retained history, exact current capacity, honest Unknown/no replay and eleven
Admin/Operator APIs. Coherent reads/atomic writes, strict parser, restricted schema,
explicit owner activation and scoped telemetry. Existing order/dashboard rules
preserved. CI explicitly activates only its disposable calendar fixture.

Local gates: Release build with zero warnings/errors; 239 unit / 195 integration /
253 frontend / 48 E2E passed, zero failures/skips/retries. GitHub reports the same
counts; owner fixture activation committed successfully. Security/delivery review
and 209-artifact hash audit pass. Package/lockfiles unchanged; owned fixtures,
images and temporary credentials cleaned. Main checkout/prior videos preserved.

Screen-reader speech and physical mobile keyboard/IME remain Not run because the
required tools/device are unavailable. The user explicitly accepted local handoff
with these limitations; no accessibility certification is claimed.

Next: review PR #36 and its final head checks. Retain the feature worktree/branch.
No merge, live activation, deployment or new video action was performed or authorized.
