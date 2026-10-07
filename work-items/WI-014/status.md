# WI-014 — Status

2026-10-06: done. BUG-008 (order-list product filter widening the page with long product names) fixed under plan
revisions 1 and 2: zero-minimum grid tracks keep the page inside the viewport, and where `appearance: base-select`
is supported (Chrome/Edge) the open product list is as wide as its control, long labels end in an ellipsis and
the full `SKU — name` is the hover title (DEC-003, DEC-004). PR #43
(https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/43) squash-merged as `3e29618` at the user's
request after final head `dd10347` passed CI run 37433461443 (239 backend unit / 203 integration / 269 frontend /
68 E2E). Feature branch deleted locally and remotely; `master` synchronized. No deployment. Documentation
closeout merged via PR #44 as `2f502b1`.

Known limits: Firefox/Safari keep the native (wide) popup until they support `base-select`; hover-tooltip
rendering was not observed in headless Chromium (only the `title` attributes are asserted); touch devices
cannot hover. See [test-plan](test-plan.md) known gaps.

Open follow-up (not started): the user asked that any work item adding new tests have a `test-plan.md`; adopting
that in `ai/workflows/bug-fix.md` needs a separate harness-improvement RFC, which the user has not requested yet.
